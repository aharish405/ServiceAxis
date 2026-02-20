using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Enums;

namespace ServiceAxis.Domain.Entities.Workflow;

/// <summary>
/// A manual work item created by a <see cref="WorkflowInstance"/> that requires 
/// human intervention (e.g. Approval, Task Assignment).
/// </summary>
public class WorkflowTask : BaseEntity
{
    public Guid InstanceId { get; set; }
    public WorkflowInstance Instance { get; set; } = null!;

    public Guid StepId { get; set; }
    public WorkflowStep Step { get; set; } = null!;

    /// <summary>The human-readable label or title of the task.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Detailed instructions for the assignee.</summary>
    public string? Description { get; set; }

    /// <summary>The ID of the user assigned to this task.</summary>
    public string? AssignedToUserId { get; set; }

    /// <summary>The ID of the group assigned to this task.</summary>
    public string? AssignedToGroupId { get; set; }

    /// <summary>Current status of the task.</summary>
    public TaskStatus Status { get; set; } = TaskStatus.New;

    /// <summary>When the task was completed.</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Who completed the task.</summary>
    public string? CompletedByUserId { get; set; }

    /// <summary>Resolution/Close notes from the assignee.</summary>
    public string? ResolutionNotes { get; set; }

    public Guid? TenantId { get; set; }
}

public enum TaskStatus
{
    New,
    InProgress,
    Completed,
    Cancelled,
    Rejected
}
