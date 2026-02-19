using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Enums;
using ServiceAxis.Domain.Entities.Platform;
using ServiceAxis.Domain.Entities.Records;

namespace ServiceAxis.Domain.Entities.Activity;

public class Activity : BaseEntity
{
    public Guid RecordId { get; set; }
    public Guid TableId { get; set; }
    public ActivityType Type { get; set; }
    public string? Message { get; set; }
    public bool IsSystemGenerated { get; set; }

    // Navigation
    public SysTable Table { get; set; } = null!;
    public PlatformRecord Record { get; set; } = null!;
    public ICollection<FieldChange> FieldChanges { get; set; } = new List<FieldChange>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}

public class FieldChange : BaseEntity
{
    public Guid ActivityId { get; set; }
    public Guid? FieldId { get; set; }
    public string? FieldName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    // Navigation
    public Activity Activity { get; set; } = null!;
}

public class Comment : BaseEntity
{
    public Guid ActivityId { get; set; }
    public Guid RecordId { get; set; }
    public string CommentText { get; set; } = string.Empty;
    public bool IsInternal { get; set; }

    // Navigation
    public Activity Activity { get; set; } = null!;
}

public class Attachment : BaseEntity
{
    public Guid RecordId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public string? UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public PlatformRecord Record { get; set; } = null!;
}
