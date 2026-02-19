using Microsoft.EntityFrameworkCore;
using ServiceAxis.Domain.Entities.Sla;
using ServiceAxis.Domain.Enums;

namespace ServiceAxis.Infrastructure.Persistence;

public class SlaSeeder
{
    private readonly ServiceAxisDbContext _db;

    public SlaSeeder(ServiceAxisDbContext db)
    {
        _db = db;
    }

    public async Task SeedAsync(CancellationToken ct)
    {
        // 1. Force Clean Slate for SLA Configuration (Foundation MVP Reset)
        await _db.Database.ExecuteSqlRawAsync("DELETE FROM platform.SlaTimerEvents", ct);
        await _db.Database.ExecuteSqlRawAsync("DELETE FROM platform.SlaInstances", ct);
        await _db.Database.ExecuteSqlRawAsync("DELETE FROM platform.SlaEscalationRules", ct);
        await _db.Database.ExecuteSqlRawAsync("DELETE FROM platform.SlaPolicies", ct);
        await _db.Database.ExecuteSqlRawAsync("DELETE FROM platform.SlaTargets", ct);
        await _db.Database.ExecuteSqlRawAsync("DELETE FROM platform.SlaDefinitions", ct);
        await _db.Database.ExecuteSqlRawAsync("DELETE FROM platform.BusinessCalendars", ct);

        // 2. Identify Table
        var incidentTable = await _db.SysTables.FirstOrDefaultAsync(t => t.Name == "incident", ct);
        if (incidentTable == null) return;

        // 3. Create Business Calendar
        var calendar = new BusinessCalendar
        {
            Name = "Standard Business Hours",
            WorkingDays = "Monday,Tuesday,Wednesday,Thursday,Friday",
            StartHour = 9,
            EndHour = 17,
            TimeZone = "UTC",
            IsActive = true
        };
        _db.BusinessCalendars.Add(calendar);

        // 4. Create Definition
        var definition = new SlaDefinition
        {
            Name = "Standard Incident SLA",
            Description = "Standard SLA for all high priority incidents.",
            TableId = incidentTable.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "Seed"
        };

        // 5. Create Targets
        definition.Targets.Add(new SlaTarget
        {
            MetricType = SlaMetricType.FirstResponse,
            TargetDurationMinutes = 30,
            BusinessHoursOnly = false
        });

        definition.Targets.Add(new SlaTarget
        {
            MetricType = SlaMetricType.Resolution,
            TargetDurationMinutes = 8 * 60,
            BusinessHoursOnly = true,
            BusinessCalendar = calendar // Use navigation property
        });

        // 6. Create Policy
        definition.Policies.Add(new SlaPolicy
        {
            TableId = incidentTable.Id,
            PriorityValue = "1"
        });

        // 7. Create Escalation Rules
        definition.EscalationRules.Add(new SlaEscalationRule
        {
            TriggerType = SlaEscalationTrigger.BeforeBreach,
            OffsetMinutes = -15,
            ActionType = SlaEscalationAction.NotifyGroup
        });

        definition.EscalationRules.Add(new SlaEscalationRule
        {
            TriggerType = SlaEscalationTrigger.OnBreach,
            ActionType = SlaEscalationAction.NotifyGroup
        });

        _db.SlaDefinitions.Add(definition);

        // 8. Link Rules to Service Desk (post-save relationship handling)
        var serviceDesk = await _db.AssignmentGroups.FirstOrDefaultAsync(g => g.Name == "Service Desk", ct);
        if (serviceDesk != null)
        {
            foreach (var rule in definition.EscalationRules)
            {
                rule.TargetGroupId = serviceDesk.Id;
            }
        }

        await _db.SaveChangesAsync(ct);
    }
}
