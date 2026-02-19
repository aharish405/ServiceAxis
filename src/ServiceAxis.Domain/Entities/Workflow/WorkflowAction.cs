using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Enums;

namespace ServiceAxis.Domain.Entities.Workflow;

/// <summary>
/// Records a single action taken on a <see cref="WorkflowInstance"/> step.
/// This creates the full audit trail for every workflow execution.
/// </summary>
public class WorkflowAction : BaseEntity
{
    /// <summary>Gets or sets the workflow instance this action belongs to.</summary>
    public Guid InstanceId { get; set; }

    /// <summary>Gets or sets the step that was actioned.</summary>
    public Guid StepId { get; set; }

    /// <summary>Gets or sets the event that was triggered (maps to a <see cref="WorkflowTransition.TriggerEvent"/>).</summary>
    public string TriggerEvent { get; set; } = string.Empty;

    /// <summary>Gets or sets the resulting status of the step after the action.</summary>
    public StepStatus ResultStatus { get; set; }

    /// <summary>Gets or sets the identity of the actor who performed this action.</summary>
    public string? ActorId { get; set; }

    /// <summary>Gets or sets an optional comment entered by the actor.</summary>
    public string? Comment { get; set; }

    /// <summary>Gets or sets when this action was performed (UTC).</summary>
    public DateTime ActionedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets optional JSON data associated with the action.</summary>
    public string? ActionData { get; set; }

    // Navigation
    public WorkflowInstance? Instance { get; set; }
    public WorkflowStep? Step { get; set; }
}
