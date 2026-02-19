using ServiceAxis.Domain.Common;

namespace ServiceAxis.Domain.Entities.Workflow;

/// <summary>
/// Represents an edge (arc) between two <see cref="WorkflowStep"/> nodes.
/// Transitions define how the workflow state-machine moves from one step to another.
/// </summary>
public class WorkflowTransition : BaseEntity
{
    /// <summary>Gets or sets the parent workflow definition this transition belongs to.</summary>
    public Guid DefinitionId { get; set; }

    /// <summary>Gets or sets the source step identifier.</summary>
    public Guid FromStepId { get; set; }

    /// <summary>Gets or sets the destination step identifier.</summary>
    public Guid ToStepId { get; set; }

    /// <summary>Gets or sets the event name / trigger that fires this transition (e.g. "Approved", "Rejected").</summary>
    public string TriggerEvent { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional C#-expression condition string evaluated at runtime.</summary>
    public string? Condition { get; set; }

    /// <summary>Gets or sets an ordinal used to resolve priority when multiple transitions are eligible.</summary>
    public int Priority { get; set; }

    // Navigation
    public WorkflowDefinition? Definition { get; set; }
    public WorkflowStep? FromStep { get; set; }
    public WorkflowStep? ToStep { get; set; }
}
