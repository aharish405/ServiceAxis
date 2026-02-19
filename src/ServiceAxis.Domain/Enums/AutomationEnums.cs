namespace ServiceAxis.Domain.Enums;

public enum AutomationEventType
{
    RecordCreated = 0,
    RecordUpdated = 1,
    StateChanged = 2,
    AssignmentChanged = 3,
    SlaBreached = 4,
    CommentAdded = 5
}

public enum AutomationExecutionMode
{
    Synchronous = 0,
    Background = 1
}

public enum AutomationActionType
{
    UpdateField = 0,
    AssignUser = 1,
    AssignGroup = 2,
    ChangeState = 3,
    SendNotification = 4,
    StartWorkflow = 5,
    AddComment = 6
}

public enum AutomationExecutionStatus
{
    Success = 0,
    Failed = 1,
    Skipped = 2
}

public enum AutomationOperator
{
    Equals = 0,
    NotEquals = 1,
    GreaterThan = 2,
    LessThan = 3,
    Contains = 4,
    StartsWith = 5,
    ChangedTo = 6,
    ChangedFrom = 7
}

public enum LogicalGroup
{
    And = 0,
    Or = 1
}
