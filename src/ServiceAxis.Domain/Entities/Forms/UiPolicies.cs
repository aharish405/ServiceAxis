using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Entities.Platform;
using ServiceAxis.Domain.Enums;

namespace ServiceAxis.Domain.Entities.Forms;

/// <summary>
/// Defines a UI Policy that alters form behavior without code.
/// e.g. "If Priority = 1, show VP Approval Checkbox".
/// </summary>
public class UiPolicy : BaseEntity
{
    public Guid TableId { get; set; }
    public SysTable Table { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public int ExecutionOrder { get; set; } = 100;

    /// <summary>
    /// When does this policy apply? "All", "Create", "Edit", "View"
    /// </summary>
    public FormContextType FormContext { get; set; } = FormContextType.All;

    /// <summary>
    /// Versioning control for the policy.
    /// </summary>
    public int Version { get; set; } = 1;

    // Navigation properties for conditions and actions
    public ICollection<UiPolicyCondition> Conditions { get; set; } = new List<UiPolicyCondition>();
    public ICollection<UiPolicyAction> Actions { get; set; } = new List<UiPolicyAction>();
}

public class UiPolicyCondition : BaseEntity
{
    public Guid UiPolicyId { get; set; }
    public UiPolicy UiPolicy { get; set; } = null!;

    public Guid FieldId { get; set; }
    public SysField Field { get; set; } = null!;

    public UiPolicyOperator Operator { get; set; } = UiPolicyOperator.Equals;
    
    /// <summary>
    /// For IsEmpty / IsNotEmpty, this can be null.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Logical grouping (e.g., AND/OR with the next condition).
    /// Repurposing the Automation LogicalGroup.
    /// </summary>
    public LogicalGroup LogicalGroup { get; set; } = LogicalGroup.And;
}

public class UiPolicyAction : BaseEntity
{
    public Guid UiPolicyId { get; set; }
    public UiPolicy UiPolicy { get; set; } = null!;

    public Guid TargetFieldId { get; set; }
    public SysField TargetField { get; set; } = null!;

    public UiPolicyActionType ActionType { get; set; }
}
