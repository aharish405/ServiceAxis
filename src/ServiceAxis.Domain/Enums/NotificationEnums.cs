namespace ServiceAxis.Domain.Enums;

/// <summary>Delivery channels for the Notification Engine.</summary>
public enum NotificationChannelType
{
    Email = 1,
    WebPush = 2,
    Sms = 3,
    Teams = 4,
    Slack = 5,
    InApp = 6
}

/// <summary>Current delivery status of a notification attempt.</summary>
public enum NotificationStatus
{
    Pending = 1,
    Sent = 2,
    Failed = 3,
    Cancelled = 4
}

/// <summary>Events that can trigger a notification.</summary>
public enum NotificationTriggerEvent
{
    RecordCreated = 1,
    RecordUpdated = 2,
    RecordAssigned = 3,
    WorkflowStepReached = 4,
    SlaBreached = 5,
    SlaWarning = 6,
    CustomEvent = 99
}
