namespace ServiceAxis.Domain.Enums;

/// <summary>Type of SLA metric being measured.</summary>
public enum SlaType
{
    ResponseTime = 1,
    ResolutionTime = 2,
    UpdateTime = 3
}

/// <summary>Current state of a live SLA instance.</summary>
public enum SlaStatus
{
    Active = 1,
    Paused = 2,
    Met = 3,
    Breached = 4,
    Warning = 5,
    Cancelled = 6
}

/// <summary>Business impact priority, used for SLA tier selection.</summary>
public enum SlaPriority
{
    Critical = 1,
    High = 2,
    Medium = 3,
    Low = 4
}

/// <summary>Work-schedule type used for SLA calculation windows.</summary>
public enum SlaScheduleType
{
    /// <summary>24 hours × 7 days.</summary>
    AlwaysOn = 1,
    /// <summary>Business hours only (e.g. 09:00–18:00 Mon–Fri).</summary>
    BusinessHours = 2,
    /// <summary>Custom schedule defined per policy.</summary>
    Custom = 3
}
