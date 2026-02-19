using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServiceAxis.Application.Common.Models;
using ServiceAxis.Application.Contracts.Infrastructure;
using ServiceAxis.Domain.Entities.Automation;
using ServiceAxis.Domain.Enums;
using ServiceAxis.Infrastructure.Persistence;
using Hangfire;

namespace ServiceAxis.Infrastructure.Services.Automation;

public class AutomationEngine : IAutomationEngine
{
    private readonly ServiceAxisDbContext    _db;
    private readonly IConditionEvaluator   _evaluator;
    private readonly IAutomationActionExecutor _executor;
    private readonly IMetadataCache         _metadata;
    private readonly IActivityService       _activity;
    private readonly ILogger<AutomationEngine> _logger;
    private readonly IBackgroundJobClient   _backgroundJobs;

    private static readonly AsyncLocal<int> _executionDepth = new();
    private const int MaxDepth = 5;

    public AutomationEngine(
        ServiceAxisDbContext db,
        IConditionEvaluator evaluator,
        IAutomationActionExecutor executor,
        IMetadataCache metadata,
        IActivityService activity,
        ILogger<AutomationEngine> logger,
        IBackgroundJobClient backgroundJobs)
    {
        _db             = db;
        _evaluator      = evaluator;
        _executor       = executor;
        _metadata       = metadata;
        _activity       = activity;
        _logger         = logger;
        _backgroundJobs = backgroundJobs;
    }

    public async Task ProcessEventAsync<T>(T @event) where T : PlatformEvent
    {
        if (_executionDepth.Value >= MaxDepth)
        {
            _logger.LogWarning("Automation loop detected and halted. Record: {RecordId}, Depth: {Depth}", @event.RecordId, _executionDepth.Value);
            return;
        }

        _executionDepth.Value++;

        try
        {
            var table = await _metadata.GetTableAsync(@event.TableName);
            if (table == null) return;

            var eventType = MapEventToType(@event);

            // Load active rules for this table and event type
            var rules = await _db.AutomationRules
                .Include(r => r.Triggers)
                .Include(r => r.Conditions)
                .Include(r => r.Actions)
                .Where(r => r.IsActive && 
                           r.TableId == table.Id && 
                           r.Triggers.Any(t => t.EventType == eventType))
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();

            _logger.LogInformation("AutomationEngine: Found {Count} active rules for {Table} on {Event}", rules.Count, @event.TableName, eventType);

            foreach (var rule in rules)
            {
                if (rule.ExecutionMode == AutomationExecutionMode.Synchronous)
                {
                    await ExecuteRuleAsync(rule, @event);
                }
                else
                {
                    // Queue for Background processing
                    _backgroundJobs.Enqueue<IAutomationEngine>(e => e.ExecuteRuleByIdAsync(rule.Id, @event, CancellationToken.None));
                }

                if (rule.StopProcessingOnMatch) break;
            }
        }
        finally
        {
            _executionDepth.Value--;
        }
    }

    public async Task ExecuteRuleByIdAsync(Guid ruleId, PlatformEvent @event, CancellationToken ct)
    {
        var rule = await _db.AutomationRules
            .Include(r => r.Triggers)
            .Include(r => r.Conditions)
            .Include(r => r.Actions)
            .FirstOrDefaultAsync(r => r.Id == ruleId);

        if (rule == null || !rule.IsActive) return;

        await ExecuteRuleAsync(rule, @event);
    }

    private async Task ExecuteRuleAsync(AutomationRule rule, PlatformEvent @event)
    {
        var sw = Stopwatch.StartNew();
        var status = AutomationExecutionStatus.Skipped;
        var message = "";

        try
        {
            // 1. Evaluate Conditions
            var isMatch = await _evaluator.EvaluateAsync(rule, @event);
            if (!isMatch) return;

            // 2. Execute Actions
            _logger.LogInformation("Executing actions for rule '{RuleName}' on record {RecordId}", rule.Name, @event.RecordId);
            await _executor.ExecuteAsync(rule, @event);

            status = AutomationExecutionStatus.Success;
            message = "Rule executed successfully.";

            // 3. Log Activity
            await _activity.LogActivityAsync(
                rule.TableId,
                @event.RecordId,
                ActivityType.AutomationExecuted,
                $"Automation Rule '{rule.Name}' executed successfully.",
                isSystem: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing automation rule {RuleId}", rule.Id);
            status = AutomationExecutionStatus.Failed;
            message = ex.Message;

            await _activity.LogActivityAsync(
                rule.TableId,
                @event.RecordId,
                ActivityType.AutomationFailed,
                $"Automation Rule '{rule.Name}' failed: {ex.Message}",
                isSystem: true);
        }
        finally
        {
            sw.Stop();
            // 4. Log Execution
            _db.AutomationExecutionLogs.Add(new AutomationExecutionLog
            {
                RuleId = rule.Id,
                RecordId = @event.RecordId,
                ExecutionStatus = status,
                ExecutionMessage = message,
                DurationMs = sw.ElapsedMilliseconds,
                TenantId = @event.TenantId
            });
            await _db.SaveChangesAsync();
        }
    }

    private AutomationEventType MapEventToType(PlatformEvent @event) => @event switch
    {
        RecordCreatedEvent     => AutomationEventType.RecordCreated,
        RecordUpdatedEvent     => AutomationEventType.RecordUpdated,
        StateChangedEvent      => AutomationEventType.StateChanged,
        AssignmentChangedEvent => AutomationEventType.AssignmentChanged,
        SlaBreachedEvent       => AutomationEventType.SlaBreached,
        CommentAddedEvent      => AutomationEventType.CommentAdded,
        _                      => throw new ArgumentException("Unknown event type", nameof(@event))
    };
}
