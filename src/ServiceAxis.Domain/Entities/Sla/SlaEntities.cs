using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Enums;

namespace ServiceAxis.Domain.Entities.Sla;

/// <summary>
/// Named SLA target definition (e.g. "Gold SLA", "Platinum Critical Response").
/// Defines the expected response and resolution times per priority.
/// </summary>
public class SlaDefinition : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Which table this SLA applies to (e.g. "incident").</summary>
    public string TableName { get; set; } = string.Empty;

    public SlaType Type { get; set; } = SlaType.ResolutionTime;

    public SlaScheduleType ScheduleType { get; set; } = SlaScheduleType.AlwaysOn;

    /// <summary>Business hours start (24h, e.g. 9 = 09:00). Only used for BusinessHours schedule.</summary>
    public int BusinessStartHour { get; set; } = 9;

    /// <summary>Business hours end (24h). Only used for BusinessHours schedule.</summary>
    public int BusinessEndHour { get; set; } = 18;

    /// <summary>Working days as JSON array: ["Mon","Tue","Wed","Thu","Fri"].</summary>
    public string WorkingDaysJson { get; set; } = "[\"Mon\",\"Tue\",\"Wed\",\"Thu\",\"Fri\"]";

    public bool IsSystemDefinition { get; set; } = false;

    public Guid? TenantId { get; set; }

    // Navigation
    public ICollection<SlaPolicy> Policies { get; set; } = new List<SlaPolicy>();
}

/// <summary>
/// Concrete SLA target for a specific priority tier within a <see cref="SlaDefinition"/>.
/// One SlaDefinition has multiple SlaPolicy rows (one per priority level).
/// </summary>
public class SlaPolicy : BaseEntity
{
    public Guid SlaDefinitionId { get; set; }
    public SlaDefinition SlaDefinition { get; set; } = null!;

    public SlaPriority Priority { get; set; }

    /// <summary>Target response time in minutes.</summary>
    public int ResponseTimeMinutes { get; set; }

    /// <summary>Target resolution time in minutes.</summary>
    public int ResolutionTimeMinutes { get; set; }

    /// <summary>Warn at this percentage of time elapsed (e.g. 75 = warn at 75%).</summary>
    public int WarningThresholdPercent { get; set; } = 75;

    /// <summary>Whether breaching this policy sends notifications.</summary>
    public bool NotifyOnBreach { get; set; } = true;

    /// <summary>Whether breaching auto-escalates to the escalation group.</summary>
    public bool EscalateOnBreach { get; set; } = false;
}

/// <summary>
/// A live SLA timer instance attached to a specific <see cref="Records.PlatformRecord"/>.
/// The Hangfire SLA engine evaluates all Active instances every minute.
/// </summary>
public class SlaInstance : BaseEntity
{
    public Guid SlaDefinitionId { get; set; }
    public SlaDefinition SlaDefinition { get; set; } = null!;

    public Guid SlaPolicyId { get; set; }
    public SlaPolicy SlaPolicy { get; set; } = null!;

    /// <summary>The record this SLA is measuring.</summary>
    public Guid RecordId { get; set; }

    public string TableName { get; set; } = string.Empty;

    public SlaStatus Status { get; set; } = SlaStatus.Active;

    /// <summary>When the SLA clock started.</summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Calculated response deadline (StartedAt + ResponseTimeMinutes).</summary>
    public DateTime ResponseDueAt { get; set; }

    /// <summary>Calculated resolution deadline.</summary>
    public DateTime ResolutionDueAt { get; set; }

    /// <summary>When response was actually made.</summary>
    public DateTime? RespondedAt { get; set; }

    /// <summary>When the record was actually resolved.</summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>Total paused time in minutes (excluded from SLA calculation).</summary>
    public int PausedMinutes { get; set; } = 0;

    /// <summary>When the SLA was last paused (e.g. status = "Waiting for Customer").</summary>
    public DateTime? PausedAt { get; set; }

    /// <summary>Whether the response SLA warning has been fired.</summary>
    public bool ResponseWarningFired { get; set; } = false;

    /// <summary>Whether the response SLA breach notification has been sent.</summary>
    public bool ResponseBreachFired { get; set; } = false;

    /// <summary>Whether the resolution SLA warning has been fired.</summary>
    public bool ResolutionWarningFired { get; set; } = false;

    /// <summary>Whether the resolution SLA breach notification has been sent.</summary>
    public bool ResolutionBreachFired { get; set; } = false;

    public Guid? TenantId { get; set; }
}
