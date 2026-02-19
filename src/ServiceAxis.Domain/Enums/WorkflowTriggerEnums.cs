namespace ServiceAxis.Domain.Enums;

/// <summary>Domain events that can trigger workflow execution.</summary>
public enum WorkflowTriggerEvent
{
    RecordCreated = 1,
    RecordUpdated = 2,
    RecordDeleted = 3,
    FieldChanged = 4,
    StateChanged = 5,
    ManualTrigger = 6,
    ScheduledTrigger = 7,
    SlaBreached = 8
}

/// <summary>Current execution status of a workflow trigger.</summary>
public enum WorkflowTriggerStatus
{
    Active = 1,
    Inactive = 2,
    Draft = 3
}
