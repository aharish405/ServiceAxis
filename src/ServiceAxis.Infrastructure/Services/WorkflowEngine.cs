using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServiceAxis.Application.Contracts.Infrastructure;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Entities.Platform;
using ServiceAxis.Domain.Entities.Workflow;
using ServiceAxis.Domain.Enums;
using ServiceAxis.Infrastructure.Persistence;
using ServiceAxis.Infrastructure.Services.Workflow.Handlers;

namespace ServiceAxis.Infrastructure.Services;

public class WorkflowEngine : IWorkflowEngine
{
    private readonly ServiceAxisDbContext _db;
    private readonly IActivityService _activity;
    private readonly ILogger<WorkflowEngine> _logger;
    private readonly IWorkflowStepHandler[] _handlers;

    public WorkflowEngine(
        ServiceAxisDbContext db, 
        IActivityService activity, 
        ILogger<WorkflowEngine> logger,
        IEnumerable<IWorkflowStepHandler> handlers)
    {
        _db = db;
        _activity = activity;
        _logger = logger;
        _handlers = handlers.ToArray();
    }

    public async Task ProcessTriggersAsync(Guid tableId, Guid recordId, WorkflowTriggerEvent eventType, string? changedField = null, CancellationToken ct = default)
    {
        var triggers = await _db.WorkflowTriggers
            .Include(t => t.Table)
            .Include(t => t.WorkflowDefinition)
                .ThenInclude(d => d.Steps)
            .Include(t => t.WorkflowDefinition)
                .ThenInclude(d => d.Transitions)
            .Where(t => t.TableId == tableId && 
                        t.EventType == eventType && 
                        t.Status == WorkflowTriggerStatus.Active)
            .OrderBy(t => t.Priority)
            .ToListAsync(ct);

        foreach (var trigger in triggers)
        {
            if (eventType == WorkflowTriggerEvent.FieldChanged && 
                !string.IsNullOrEmpty(trigger.WatchFieldName) && 
                !trigger.WatchFieldName.Equals(changedField, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var initialStep = trigger.WorkflowDefinition.Steps.FirstOrDefault(s => s.IsInitial);
            if (initialStep == null) continue;

            var instance = new WorkflowInstance
            {
                DefinitionId = trigger.WorkflowDefinitionId,
                TriggerEntityType = trigger.Table?.Name ?? "PlatformRecord",
                TriggerEntityId = recordId.ToString(),
                Status = WorkflowStatus.Active,
                StartedAt = DateTime.UtcNow,
                TenantId = trigger.TenantId,
                ReferenceNumber = $"WF-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}",
                CurrentStepId = initialStep.Id
            };

            _db.WorkflowInstances.Add(instance);
            
            await _activity.LogActivityAsync(tableId, recordId, ActivityType.WorkflowEvent, 
                $"Workflow '{trigger.WorkflowDefinition.Name}' started", isSystem: true, ct: ct);

            // Start Execution Loop
            await ExecuteCurrentStepAsync(instance, ct);
        }
    }

    public async Task ExecuteCurrentStepAsync(WorkflowInstance instance, CancellationToken ct)
    {
        bool moved = true;
        while (moved && instance.Status == WorkflowStatus.Active && instance.CurrentStepId.HasValue)
        {
            moved = false;
            var step = await _db.WorkflowSteps
                .Include(s => s.OutgoingTransitions)
                .FirstOrDefaultAsync(s => s.Id == instance.CurrentStepId, ct);

            if (step == null) break;

            var handler = _handlers.FirstOrDefault(h => h.StepType.Equals(step.StepType, StringComparison.OrdinalIgnoreCase));
            if (handler == null)
            {
                _logger.LogWarning("No handler found for step type {StepType}", step.StepType);
                break;
            }

            var result = await handler.ExecuteAsync(instance, step, ct);

            if (result.IsCompleted)
            {
                // Find next transition
                var transition = step.OutgoingTransitions
                    .FirstOrDefault(t => string.IsNullOrEmpty(t.TriggerEvent) || t.TriggerEvent == result.TriggerEvent);

                if (transition != null)
                {
                    instance.CurrentStepId = transition.ToStepId;
                    moved = true;
                    
                    _db.WorkflowActions.Add(new WorkflowAction {
                        InstanceId = instance.Id,
                        StepId = step.Id,
                        TriggerEvent = result.TriggerEvent ?? "Auto",
                        ActionedAt = DateTime.UtcNow,
                        ResultStatus = StepStatus.Completed
                    });
                }
                else if (step.IsTerminal)
                {
                    instance.Status = WorkflowStatus.Completed;
                    instance.CompletedAt = DateTime.UtcNow;
                    moved = false;
                }
            }
            else
            {
                // Waiting for human or external event
                moved = false;
            }
        }
    }
}
