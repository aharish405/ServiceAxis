using ServiceAxis.Domain.Enums;

namespace ServiceAxis.Application.Common.Models;

public abstract class PlatformEvent
{
    public Guid RecordId { get; set; }
    public Guid TableId { get; set; }
    public string TableName { get; set; } = string.Empty;
    public Guid? TenantId { get; set; }
    public DateTime EventTime { get; set; } = DateTime.UtcNow;
}

public class RecordCreatedEvent : PlatformEvent { }

public class RecordUpdatedEvent : PlatformEvent
{
    public IDictionary<string, (string? OldValue, string? NewValue)> ChangedFields { get; set; } = new Dictionary<string, (string?, string?)>();
}

public class StateChangedEvent : PlatformEvent
{
    public string OldState { get; set; } = string.Empty;
    public string NewState { get; set; } = string.Empty;
}

public class AssignmentChangedEvent : PlatformEvent
{
    public string? OldUserId { get; set; }
    public string? NewUserId { get; set; }
    public Guid? OldGroupId { get; set; }
    public Guid? NewGroupId { get; set; }
}

public class SlaBreachedEvent : PlatformEvent
{
    public SlaMetricType MetricType { get; set; }
    public DateTime TargetTime { get; set; }
}

public class CommentAddedEvent : PlatformEvent
{
    public string CommentText { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
}
