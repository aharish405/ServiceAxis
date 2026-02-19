namespace ServiceAxis.Domain.Enums;

public enum SlaMetricType
{
    FirstResponse = 0,
    Resolution = 1
}

public enum SlaEscalationTrigger
{
    BeforeBreach = 0,
    OnBreach = 1,
    AfterBreach = 2
}

public enum SlaEscalationAction
{
    NotifyUser = 0,
    NotifyGroup = 1,
    ReassignGroup = 2,
    TriggerWorkflowEvent = 3
}

public enum SlaStatus
{
    Pending = 0,
    Active = 1,
    Paused = 2,
    Completed = 3, 
    Cancelled = 4,
    Breached = 5
}

public enum SlaTimerEventType
{
    Started = 0,
    Paused = 1,
    Resumed = 2,
    Completed = 3,
    Breached = 4,
    Cancelled = 5,
    Escalated = 6
}
