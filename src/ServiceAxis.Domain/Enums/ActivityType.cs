namespace ServiceAxis.Domain.Enums;

public enum ActivityType
{
    RecordCreated,
    FieldChanged,
    CommentAdded,
    WorkNoteAdded,
    AttachmentAdded,
    WorkflowEvent,
    StatusChanged,
    AssignmentChanged,
    StateTransitioned,
    SlaStarted,
    SlaPaused,
    SlaResumed,
    SlaCompleted,
    SlaBreached,
    SlaEscalated,
    AutomationExecuted,
    AutomationFailed
}
