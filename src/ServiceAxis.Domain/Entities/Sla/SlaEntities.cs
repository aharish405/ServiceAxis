using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Enums;

namespace ServiceAxis.Domain.Entities.Sla;

// ─── Definitions ──────────────────────────────────────────────────────────────

/// <summary>
/// Defines an SLA (e.g. "Standard Incident SLA").
/// Linked to a specific table.
/// </summary>
public class SlaDefinition : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    /// <summary>Table this SLA applies to (e.g. "incident").</summary>
    public Guid TableId { get; set; }
    
    public Guid? TenantId { get; set; }

    // Navigation
    public ICollection<SlaPolicy> Policies { get; set; } = new List<SlaPolicy>();
    public ICollection<SlaTarget> Targets { get; set; } = new List<SlaTarget>();
    public ICollection<SlaEscalationRule> EscalationRules { get; set; } = new List<SlaEscalationRule>();
}

/// <summary>
/// Defines the specific targets (duration) for the SLA.
/// E.g. "First Response = 30m", "Resolution = 8h".
/// </summary>
public class SlaTarget : BaseEntity
{
    public Guid SlaDefinitionId { get; set; }
    public SlaDefinition? SlaDefinition { get; set; }

    public SlaMetricType MetricType { get; set; }
    
    /// <summary>Target duration in minutes.</summary>
    public int TargetDurationMinutes { get; set; }

    /// <summary>If true, calculates using BusinessCalendar. If false, 24x7.</summary>
    public bool BusinessHoursOnly { get; set; }

    public Guid? BusinessCalendarId { get; set; }
    public BusinessCalendar? BusinessCalendar { get; set; }
}

/// <summary>
/// Condition to apply this SLA.
/// E.g. If Priority = 'High' (Value=1), use this SLA.
/// </summary>
public class SlaPolicy : BaseEntity
{
    public Guid SlaDefinitionId { get; set; }
    public SlaDefinition? SlaDefinition { get; set; }
    
    public Guid TableId { get; set; }
    
    /// <summary>Field to check (e.g. Priority field ID).</summary>
    public Guid? PriorityFieldId { get; set; }
    
    /// <summary>Value to match (e.g. "1" for High).</summary>
    public string? PriorityValue { get; set; }
}

public class BusinessCalendar : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string TimeZone { get; set; } = "UTC";
    
    /// <summary>Comma-separated days: "Mon,Tue,Wed,Thu,Fri"</summary>
    public string WorkingDays { get; set; } = "Mon,Tue,Wed,Thu,Fri";
    
    /// <summary>Start hour (0-23).</summary>
    public int StartHour { get; set; } = 9;
    
    /// <summary>End hour (0-23).</summary>
    public int EndHour { get; set; } = 17;
    
    public Guid? TenantId { get; set; }
}

public class SlaEscalationRule : BaseEntity
{
    public Guid SlaDefinitionId { get; set; }
    public SlaDefinition? SlaDefinition { get; set; }

    public SlaEscalationTrigger TriggerType { get; set; }
    
    /// <summary>Minutes offset. e.g. -15 for "15 min before breach".</summary>
    public int OffsetMinutes { get; set; }
    
    public SlaEscalationAction ActionType { get; set; }
    
    // For Notify/Reassign
    public string? TargetUserId { get; set; }
    public Guid? TargetGroupId { get; set; }
}

// ─── Runtime Instances ────────────────────────────────────────────────────────

/// <summary>
/// A running SLA timer for a specific record and metric (e.g. Incident #123 - Resolution).
/// </summary>
public class SlaInstance : BaseEntity
{
    public Guid RecordId { get; set; }
    public Guid TableId { get; set; }
    
    public Guid SlaDefinitionId { get; set; }
    // public SlaDefinition SlaDefinition { get; set; } // Optional nav
    
    public SlaMetricType MetricType { get; set; }
    
    public SlaStatus Status { get; set; } = SlaStatus.Pending;
    
    public DateTime StartTime { get; set; }
    public DateTime TargetTime { get; set; }
    
    /// <summary>Time when the Breach actually occurred (TargetTime passed).</summary>
    public DateTime? BreachTime { get; set; }
    
    public DateTime? CompletedTime { get; set; }
    
    public bool IsBreached { get; set; }
    public bool IsPaused { get; set; }
    
    public Guid? TenantId { get; set; }

    public ICollection<SlaTimerEvent> TimerEvents { get; set; } = new List<SlaTimerEvent>();
}

public class SlaTimerEvent : BaseEntity
{
    public Guid SlaInstanceId { get; set; }
    
    public SlaTimerEventType EventType { get; set; }
    public DateTime EventTime { get; set; }
    public bool TriggeredBySystem { get; set; }
}
