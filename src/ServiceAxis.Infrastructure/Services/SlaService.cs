using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServiceAxis.Application.Contracts.Infrastructure;
using ServiceAxis.Domain.Entities.Sla;
using ServiceAxis.Domain.Enums;
using ServiceAxis.Infrastructure.Persistence;
using ServiceAxis.Shared.Exceptions;

namespace ServiceAxis.Infrastructure.Services;

public class SlaService : ISlaService
{
    private readonly ServiceAxisDbContext _db;
    private readonly IActivityService     _activity;
    private readonly ILogger<SlaService>  _logger;

    public SlaService(
        ServiceAxisDbContext db,
        IActivityService activity,
        ILogger<SlaService> logger)
    {
        _db       = db;
        _activity = activity;
        _logger   = logger;
    }

    // ─── Start SLA ────────────────────────────────────────────────────────────

    public async Task StartSlaAsync(
        Guid recordId,
        string tableName,
        int priority,
        Guid? tenantId,
        CancellationToken ct = default)
    {
        // 1. Find applicable Policy (and thus Definition)
        
        var table = await _db.SysTables.FirstOrDefaultAsync(t => t.Name == tableName, ct);
        if (table == null) 
        {
            _logger.LogWarning("StartSlaAsync: Table '{TableName}' not found.", tableName);
            return;
        }
        
        // We look for active Definitions for this table
        var policies = await _db.SlaPolicies
            .Include(p => p.SlaDefinition)
            .ThenInclude(d => d.Targets)
            .ThenInclude(t => t.BusinessCalendar)
            .Where(p => 
                p.SlaDefinition != null &&
                p.SlaDefinition.IsActive && 
                p.SlaDefinition.TableId == table.Id)
            .ToListAsync(ct);
        
        _logger.LogInformation("StartSlaAsync: Found {Count} active policies for table '{TableName}'.", policies.Count, tableName);

        // Match priority (simple string match for now, assuming integer string)
        var priorityVal = priority.ToString();
        var policy = policies.FirstOrDefault(p => p.PriorityValue == priorityVal);

        if (policy == null || policy.SlaDefinition == null) 
        {
            _logger.LogWarning("StartSlaAsync: No matching policy found for priority '{Priority}' on table '{TableName}'.", priorityVal, tableName);
            return; // No SLA applies
        }

        _logger.LogInformation("StartSlaAsync: Starting SLA '{SlaName}' for record {RecordId}.", policy.SlaDefinition.Name, recordId);

        var definition = policy.SlaDefinition;
        var now = DateTime.UtcNow;

        // 2. Create Instances for each Target
        foreach (var target in definition.Targets)
        {
            var deadline = CalculateDeadline(now, target);

            var instance = new SlaInstance
            {
                RecordId = recordId,
                TableId = table.Id,
                SlaDefinitionId = definition.Id,
                MetricType = target.MetricType,
                Status = SlaStatus.Active,
                StartTime = now,
                TargetTime = deadline,
                TenantId = tenantId,
                IsActive = true,
                CreatedAt = now,
                CreatedBy = "System"
            };

            instance.TimerEvents.Add(new SlaTimerEvent
            {
                EventType = SlaTimerEventType.Started,
                EventTime = now,
                TriggeredBySystem = true
            });

            _db.SlaInstances.Add(instance);

            // 3. Log Activity
            await _activity.LogActivityAsync(
                definition.TableId,
                recordId,
                ActivityType.SlaStarted,
                $"SLA '{target.MetricType}' started. Target: {deadline:g} ({(target.BusinessHoursOnly ? "Business Hours" : "24x7")})",
                isSystem: true,
                ct: ct);
        }

        await _db.SaveChangesAsync(ct);
    }

    // ─── Pause SLA ────────────────────────────────────────────────────────────

    public async Task PauseSlaAsync(Guid recordId, CancellationToken ct = default)
    {
        var activeInstances = await _db.SlaInstances
            .Where(i => i.RecordId == recordId && i.Status == SlaStatus.Active && !i.IsPaused)
            .ToListAsync(ct);

        if (!activeInstances.Any()) return;

        var now = DateTime.UtcNow;

        foreach (var instance in activeInstances)
        {
            instance.IsPaused = true;
            instance.Status   = SlaStatus.Paused;
            
            instance.TimerEvents.Add(new SlaTimerEvent
            {
                EventType = SlaTimerEventType.Paused,
                EventTime = now,
                TriggeredBySystem = true
            });
        }
        
        // Log generic activity once
        await _activity.LogActivityAsync(
            activeInstances.First().TableId,
            recordId,
            ActivityType.SlaPaused,
            "SLA timers paused due to state change.",
            isSystem: true,
            ct: ct);

        await _db.SaveChangesAsync(ct);
    }

    // ─── Resume SLA ───────────────────────────────────────────────────────────

    public async Task ResumeSlaAsync(Guid recordId, CancellationToken ct = default)
    {
        var pausedInstances = await _db.SlaInstances
            .Where(i => i.RecordId == recordId && i.Status == SlaStatus.Paused && i.IsPaused)
            .ToListAsync(ct);

        if (!pausedInstances.Any()) return;

        var now = DateTime.UtcNow;

        foreach (var instance in pausedInstances)
        {
            // Calculate pause duration
            // Find the last 'Paused' event
            var lastPause = await _db.SlaTimerEvents
                .Where(e => e.SlaInstanceId == instance.Id && e.EventType == SlaTimerEventType.Paused)
                .OrderByDescending(e => e.EventTime)
                .FirstOrDefaultAsync(ct);

            if (lastPause != null)
            {
                var duration = now - lastPause.EventTime;
                // Shift TargetTime
                instance.TargetTime = instance.TargetTime.Add(duration);
            }

            instance.IsPaused = false;
            instance.Status   = SlaStatus.Active;
            
            instance.TimerEvents.Add(new SlaTimerEvent
            {
                EventType = SlaTimerEventType.Resumed,
                EventTime = now,
                TriggeredBySystem = true
            });
        }

        await _activity.LogActivityAsync(
            pausedInstances.First().TableId,
            recordId,
            ActivityType.SlaResumed,
            "SLA timers resumed.",
            isSystem: true,
            ct: ct);

        await _db.SaveChangesAsync(ct);
    }

    // ─── Complete Metrics ─────────────────────────────────────────────────────

    public Task MarkResponseCompletedAsync(Guid recordId, CancellationToken ct = default)
        => CompleteSlaMetricAsync(recordId, SlaMetricType.FirstResponse, ct);

    public Task CompleteSlaAsync(Guid recordId, CancellationToken ct = default)
        => CompleteSlaMetricAsync(recordId, SlaMetricType.Resolution, ct);

    private async Task CompleteSlaMetricAsync(Guid recordId, SlaMetricType metric, CancellationToken ct)
    {
        var instance = await _db.SlaInstances
            .Where(i => i.RecordId == recordId && i.MetricType == metric && i.Status != SlaStatus.Completed && i.Status != SlaStatus.Cancelled)
            .FirstOrDefaultAsync(ct);

        if (instance == null) return;

        var now = DateTime.UtcNow;
        instance.Status = SlaStatus.Completed;
        instance.CompletedTime = now;
        instance.IsPaused = false; 

        // Check if Breached (if strictly completed after target)
        // Wait, EvaluateAllActiveAsync might have already marked it breached.
        // If not, verify now.
        if (!instance.IsBreached && now > instance.TargetTime)
        {
            instance.IsBreached = true;
            // Late completion == Breach
             instance.TimerEvents.Add(new SlaTimerEvent { EventType = SlaTimerEventType.Breached, EventTime = now, TriggeredBySystem = true });
        }

        instance.TimerEvents.Add(new SlaTimerEvent { EventType = SlaTimerEventType.Completed, EventTime = now, TriggeredBySystem = true });

        await _activity.LogActivityAsync(
            instance.TableId,
            recordId,
            ActivityType.SlaCompleted,
            $"SLA '{metric}' completed. {(instance.IsBreached ? "BREACHED" : "Met")}.",
            isSystem: true,
            ct: ct);

        await _db.SaveChangesAsync(ct);
    }

    // ─── Evaluate SLAs ────────────────────────────────────────────────────────

    public async Task EvaluateAllActiveAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        
        // Find active un-breached instances
        var instances = await _db.SlaInstances
            .Include(i => i.TimerEvents)
            .Where(i => 
                i.Status == SlaStatus.Active && 
                !i.IsPaused)
            .ToListAsync(ct);

        _logger.LogInformation("EvaluateAllActiveAsync: Found {Count} active instances.", instances.Count);

        if (!instances.Any()) return;

        foreach (var instance in instances)
        {
            // 1. Check for Breach
            if (!instance.IsBreached && instance.TargetTime < now)
            {
                instance.IsBreached = true;

                instance.TimerEvents.Add(new SlaTimerEvent
                {
                    EventType = SlaTimerEventType.Breached,
                    EventTime = now,
                    TriggeredBySystem = true
                });

                await _activity.LogActivityAsync(
                    instance.TableId,
                    instance.RecordId,
                    ActivityType.SlaBreached,
                    $"SLA '{instance.MetricType}' has BREACHED. Target was {instance.TargetTime:g}",
                    isSystem: true,
                    ct: ct);

                // Execute Escalations for Breach
                await ExecuteEscalationsAsync(instance, SlaEscalationTrigger.OnBreach, ct);
            }
            else if (!instance.IsBreached)
            {
                // 2. Check for Warning (BeforeBreach escalations)
                await CheckWarningEscalationsAsync(instance, now, ct);
            }
        }

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict during SLA evaluation.");
        }
    }

    private async Task CheckWarningEscalationsAsync(SlaInstance instance, DateTime now, CancellationToken ct)
    {
        var rules = await _db.SlaEscalationRules
            .Where(r => r.SlaDefinitionId == instance.SlaDefinitionId && r.TriggerType == SlaEscalationTrigger.BeforeBreach)
            .ToListAsync(ct);

        _logger.LogInformation("CheckWarningEscalationsAsync: Found {Count} rules for instance {InstanceId}.", rules.Count, instance.Id);

        foreach (var rule in rules)
        {
            var triggerTime = instance.TargetTime.AddMinutes(rule.OffsetMinutes);
            _logger.LogInformation("  - Rule {RuleId} trigger time: {TriggerTime}, Now: {Now}", rule.Id, triggerTime, now);

            if (now >= triggerTime)
            {
                // Check if already fired (idempotency)
                // We could use TimerEvents to track this.
                var alreadyFired = instance.TimerEvents.Any(e => 
                    e.EventType == SlaTimerEventType.Started && // Reuse or add new event type? 
                    e.TriggeredBySystem &&
                    e.CreatedAt > instance.StartTime && // Within this instance lifecycle
                    // We need a way to distinguish specific rule firing.
                    // For MVP foundation, we check if we've logged a "Warning" activity recently for this instance.
                    false); // simplified for now: we'll check SlaTimerEvents for a pseudo "Warning" event

                // Let's check for an event with specific offset logic
                bool fired = instance.TimerEvents.Any(e => e.EventType == SlaTimerEventType.Escalated && e.CreatedBy == $"Escalation_{rule.Id}");
                
                if (!fired)
                {
                    await _activity.LogActivityAsync(
                        instance.TableId,
                        instance.RecordId,
                        ActivityType.SlaEscalated,
                        $"SLA WARNING: {instance.MetricType} is within {(instance.TargetTime - now).TotalMinutes:F0} minutes of breach.",
                        isSystem: true,
                        ct: ct);

                    await ExecuteEscalationActionAsync(instance, rule, ct);

                    // Track firing
                    instance.TimerEvents.Add(new SlaTimerEvent {
                        EventType = SlaTimerEventType.Escalated, 
                        EventTime = now,
                        CreatedBy = $"Escalation_{rule.Id}",
                        TriggeredBySystem = true
                    });
                }
            }
        }
    }

    private async Task ExecuteEscalationsAsync(SlaInstance instance, SlaEscalationTrigger trigger, CancellationToken ct)
    {
        var rules = await _db.SlaEscalationRules
            .Where(r => r.SlaDefinitionId == instance.SlaDefinitionId && r.TriggerType == trigger)
            .ToListAsync(ct);

        foreach (var rule in rules)
        {
            await ExecuteEscalationActionAsync(instance, rule, ct);
        }
    }

    private async Task ExecuteEscalationActionAsync(SlaInstance instance, SlaEscalationRule rule, CancellationToken ct)
    {
        _logger.LogInformation("Executing SLA Escalation Action {Action} for Instance {InstanceId}", rule.ActionType, instance.Id);

        if (rule.ActionType == SlaEscalationAction.NotifyUser || rule.ActionType == SlaEscalationAction.NotifyGroup)
        {
            // Integrates with Notification Engine (Log entry only for MVP)
            await _activity.LogActivityAsync(
                instance.TableId,
                instance.RecordId,
                ActivityType.SlaEscalated,
                $"ESCALATION ACTION: Notification sent to {(rule.ActionType == SlaEscalationAction.NotifyGroup ? "Group " + rule.TargetGroupId : "User " + rule.TargetUserId)}",
                isSystem: true,
                ct: ct);
        }
        // Potential future: ReassignGroup, workflow trigger, etc.
    }

    // ─── Calculations ─────────────────────────────────────────────────────────

    private DateTime CalculateDeadline(DateTime start, SlaTarget target)
    {
        if (!target.BusinessHoursOnly || target.BusinessCalendar == null)
        {
            return start.AddMinutes(target.TargetDurationMinutes); // 24x7
        }

        var calendar = target.BusinessCalendar;
        var minutes = target.TargetDurationMinutes;

        // --- Robust Business Calendar Logic ---
        // 1. Parse working hours/days
        var workingDays = calendar.WorkingDays.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(d => Enum.Parse<DayOfWeek>(d, true))
            .ToHashSet();

        var current = start;
        var remainingMinutes = minutes;

        // Iterate until all minutes are consumed
        while (remainingMinutes > 0)
        {
            // Is it a working day?
            if (workingDays.Contains(current.DayOfWeek))
            {
                var workStart = current.Date.AddHours(calendar.StartHour);
                var workEnd = current.Date.AddHours(calendar.EndHour);

                if (current < workStart) current = workStart;
                
                if (current < workEnd)
                {
                    var minsAvailableToday = (int)(workEnd - current).TotalMinutes;
                    var minsToConsume = Math.Min(remainingMinutes, minsAvailableToday);
                    
                    current = current.AddMinutes(minsToConsume);
                    remainingMinutes -= minsToConsume;
                    
                    if (remainingMinutes == 0) return current;
                }
            }

            // Move to start of next day
            current = current.Date.AddDays(1).AddHours(calendar.StartHour);
        }

        return current;
    }

    // ─── Query ───────────────────────────────────────────────────────────────

    public async Task<List<SlaStatusDto>> GetRecordSlaStatusAsync(Guid recordId, CancellationToken ct = default)
    {
        var instances = await _db.SlaInstances
            .AsNoTracking()
            .Include(i => i.TimerEvents)
            .Where(i => i.RecordId == recordId)
            // .OrderBy(i => i.StartTime) // Optional
            .ToListAsync(ct);

        var result = new List<SlaStatusDto>();
        var now = DateTime.UtcNow;

        foreach (var i in instances)
        {
            int remaining = 0;
            if (i.Status == SlaStatus.Active || i.Status == SlaStatus.Paused)
            {
                if (i.IsPaused)
                {
                    var lastPause = i.TimerEvents
                        .Where(e => e.EventType == SlaTimerEventType.Paused)
                        .OrderByDescending(e => e.EventTime)
                        .FirstOrDefault();
                    
                    if (lastPause != null)
                        remaining = (int)(i.TargetTime - lastPause.EventTime).TotalMinutes;
                }
                else
                {
                    remaining = (int)(i.TargetTime - now).TotalMinutes;
                }
            }
            else if (i.Status == SlaStatus.Completed && i.CompletedTime.HasValue)
            {
                 // For completed, maybe show how much margin was left?
                 // Or just 0. Let's show actual margin at completion.
                 remaining = (int)(i.TargetTime - i.CompletedTime.Value).TotalMinutes;
            }

            result.Add(new SlaStatusDto(
                i.Id,
                i.MetricType.ToString(),
                i.TargetTime,
                remaining,
                i.IsBreached,
                i.IsPaused,
                i.Status.ToString()));
        }

        return result;
    }

}
