using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Enums;

namespace ServiceAxis.Domain.Entities.Workflow;

/// <summary>
/// A running execution of a <see cref="WorkflowDefinition"/>.
/// Tracks the current state and history of one workflow run.
/// </summary>
public class WorkflowInstance : AggregateRoot
{
    /// <summary>Gets or sets the workflow definition this instance is based on.</summary>
    public Guid DefinitionId { get; set; }

    /// <summary>Gets or sets a reference number displayed to end users.</summary>
    public string ReferenceNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the entity type that triggered this workflow (e.g. "Incident").</summary>
    public string? TriggerEntityType { get; set; }

    /// <summary>Gets or sets the primary key of the entity that triggered this workflow.</summary>
    public string? TriggerEntityId { get; set; }

    /// <summary>Gets or sets the current overall state of the instance.</summary>
    public WorkflowStatus Status { get; set; } = WorkflowStatus.Draft;

    /// <summary>Gets or sets the step the instance is currently at.</summary>
    public Guid? CurrentStepId { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the workflow was started.</summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the workflow completed or was cancelled.</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Gets or sets optional JSON payload passed into the workflow context.</summary>
    public string? InputPayload { get; set; }

    /// <summary>Gets or sets optional JSON output collected from the workflow.</summary>
    public string? OutputPayload { get; set; }

    /// <summary>Gets or sets the tenant this instance belongs to.</summary>
    public Guid? TenantId { get; set; }

    // Navigation
    public WorkflowDefinition? Definition { get; set; }
    public WorkflowStep? CurrentStep { get; set; }
    public ICollection<WorkflowAction> Actions { get; set; } = [];
}
