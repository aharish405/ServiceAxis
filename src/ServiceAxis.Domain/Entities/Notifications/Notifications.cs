using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Enums;

namespace ServiceAxis.Domain.Entities.Notifications;

/// <summary>
/// Reusable notification template with variable substitution support.
/// Variables are referenced as {{variableName}} in subject and body.
/// </summary>
public class NotificationTemplate : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Template subject (for email/push). Supports {{vars}}.</summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>Template body HTML/Markdown. Supports {{vars}}.</summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>The event that triggers this template.</summary>
    public NotificationTriggerEvent TriggerEvent { get; set; }

    /// <summary>Which table/module this template belongs to (e.g. "incident").</summary>
    public string? TableName { get; set; }

    public bool IsSystemTemplate { get; set; } = false;

    public Guid? TenantId { get; set; }
}

/// <summary>
/// Delivery channel configuration for a notification.
/// One template can have multiple channel bindings (email + in-app).
/// </summary>
public class NotificationChannel : BaseEntity
{
    public Guid TemplateId { get; set; }
    public NotificationTemplate Template { get; set; } = null!;

    public NotificationChannelType ChannelType { get; set; }

    /// <summary>Whether this channel delivery is enabled.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Recipient resolution strategy:
    /// "assigned_to" | "requester" | "group_members" | "role:Admin" | "email:fixed@domain.com"
    /// </summary>
    public string RecipientStrategy { get; set; } = "assigned_to";

    /// <summary>Channel-specific settings JSON (e.g. from/reply-to for email).</summary>
    public string? Settings { get; set; }
}

/// <summary>
/// Immutable delivery log for every notification dispatch attempt.
/// </summary>
public class NotificationLog : BaseEntity
{
    public Guid TemplateId { get; set; }

    public NotificationChannelType Channel { get; set; }

    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

    /// <summary>Comma-separated list of recipients.</summary>
    public string Recipients { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    /// <summary>Rendered body sent to the provider.</summary>
    public string? Body { get; set; }

    /// <summary>Error message if delivery failed.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Number of delivery attempts made.</summary>
    public int AttemptCount { get; set; } = 0;

    public DateTime? SentAt { get; set; }

    /// <summary>Related record that triggered this notification.</summary>
    public Guid? RecordId { get; set; }

    public Guid? TenantId { get; set; }
}
