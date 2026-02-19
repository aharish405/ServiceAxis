using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServiceAxis.Application.Contracts.Infrastructure;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Entities.Platform;
using ServiceAxis.Domain.Entities.Workflow;
using ServiceAxis.Domain.Enums;
using ServiceAxis.Infrastructure.Persistence;

namespace ServiceAxis.Infrastructure.Services;

public class WorkflowEngine : IWorkflowEngine
{
    private readonly ServiceAxisDbContext _db;
    private readonly IActivityService _activity;
    private readonly ILogger<WorkflowEngine> _logger;

    public WorkflowEngine(ServiceAxisDbContext db, IActivityService activity, ILogger<WorkflowEngine> logger)
    {
        _db = db;
        _activity = activity;
        _logger = logger;
    }

    public async Task ProcessTriggersAsync(Guid tableId, Guid recordId, WorkflowTriggerEvent eventType, string? changedField = null, CancellationToken ct = default)
    {
        // Fetch active triggers with definition details
        var triggers = await _db.WorkflowTriggers
            .Include(t => t.Table) // Include table to get the name
            .Include(t => t.WorkflowDefinition)
            .Where(t => t.TableId == tableId && 
                        t.EventType == eventType && 
                        t.Status == WorkflowTriggerStatus.Active)
            .OrderBy(t => t.Priority)
            .ToListAsync(ct);

        foreach (var trigger in triggers)
        {
            // Field-specific trigger check
            if (eventType == WorkflowTriggerEvent.FieldChanged && 
                !string.IsNullOrEmpty(trigger.WatchFieldName) && 
                !trigger.WatchFieldName.Equals(changedField, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var workflowName = trigger.WorkflowDefinition?.Name ?? "Unknown Workflow";

            _logger.LogInformation("Triggering workflow {WorkflowName} ({WorkflowId}) for record {RecordId}", 
                workflowName, trigger.WorkflowDefinitionId, recordId);

            // 1. Create Workflow Instance
            var instance = new WorkflowInstance
            {
                DefinitionId = trigger.WorkflowDefinitionId,
                TriggerEntityType = trigger.Table?.Name ?? "PlatformRecord",
                TriggerEntityId = recordId.ToString(),
                Status = WorkflowStatus.Active,
                StartedAt = DateTime.UtcNow,
                TenantId = trigger.TenantId,
                ReferenceNumber = $"WF-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}"
            };

            _db.WorkflowInstances.Add(instance);

            // 2. Log Activity Stream entry
            await _activity.LogActivityAsync(
                tableId, 
                recordId, 
                ActivityType.WorkflowEvent, 
                $"Workflow '{workflowName}' started (Event: {eventType})", 
                isSystem: true, 
                ct: ct);
        }
    }
}
