using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Entities.Platform;
using ServiceAxis.Domain.Entities.Workflow;
using ServiceAxis.Domain.Enums;

namespace ServiceAxis.Domain.Entities.Platform;

/// <summary>
/// Binds a domain event on a <see cref="SysTable"/> to a <see cref="WorkflowDefinition"/>.
/// When the trigger condition is met, the Workflow Engine automatically starts
/// or advances a workflow instance.
///
/// Example: When an Incident record is Created → start "Incident Approval Workflow".
/// </summary>
public class WorkflowTrigger : BaseEntity
{
    /// <summary>The table that raises the trigger event.</summary>
    public Guid TableId { get; set; }
    public SysTable Table { get; set; } = null!;

    /// <summary>The event type that fires the trigger.</summary>
    public WorkflowTriggerEvent EventType { get; set; }

    /// <summary>The workflow definition to instantiate.</summary>
    public Guid WorkflowDefinitionId { get; set; }
    public WorkflowDefinition WorkflowDefinition { get; set; } = null!;

    /// <summary>
    /// Optional JSONPath condition that must evaluate to true for the trigger to fire.
    /// Example: "$.priority == 'critical'" — only triggers for critical records.
    /// </summary>
    public string? Condition { get; set; }

    /// <summary>
    /// Optional field name — for <see cref="WorkflowTriggerEvent.FieldChanged"/>, 
    /// specifies which field change fires the trigger.
    /// </summary>
    public string? WatchFieldName { get; set; }

    /// <summary>Execution priority when multiple triggers match (lower = first).</summary>
    public int Priority { get; set; } = 10;

    public WorkflowTriggerStatus Status { get; set; } = WorkflowTriggerStatus.Active;

    public Guid? TenantId { get; set; }
}
