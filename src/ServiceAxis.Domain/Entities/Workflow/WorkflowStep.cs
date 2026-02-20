using ServiceAxis.Domain.Common;

namespace ServiceAxis.Domain.Entities.Workflow;

/// <summary>
/// A single node (state) in a <see cref="WorkflowDefinition"/>.
/// Steps can be automated, require human approval, send notifications, etc.
/// </summary>
public class WorkflowStep : BaseEntity
{
    /// <summary>Gets or sets the parent workflow definition.</summary>
    public Guid DefinitionId { get; set; }

    /// <summary>Gets or sets the machine-readable step code (unique within a definition).</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name for this step.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the step type (e.g. "Approval", "Notification", "Automated", "Manual").</summary>
    public string StepType { get; set; } = "Manual";

    /// <summary>Gets or sets the ordinal execution order within the workflow.</summary>
    public int Order { get; set; }

    /// <summary>Gets or sets a value indicating whether this is the initial entry step.</summary>
    public bool IsInitial { get; set; }

    /// <summary>Gets or sets a value indicating whether this is a terminal (end) step.</summary>
    public bool IsTerminal { get; set; }

    /// <summary>Gets or sets the role required to action this step (for approval steps).</summary>
    public string? RequiredRole { get; set; }

    /// <summary>Gets or sets optional JSON configuration specific to this step type.</summary>
    public string? Configuration { get; set; }

    /// <summary>Visual coordinate X for designers.</summary>
    public double? X { get; set; }
    
    /// <summary>Visual coordinate Y for designers.</summary>
    public double? Y { get; set; }

    // Navigation
    public WorkflowDefinition? Definition { get; set; }
    public ICollection<WorkflowTransition> OutgoingTransitions { get; set; } = [];
    public ICollection<WorkflowTransition> IncomingTransitions { get; set; } = [];
}
