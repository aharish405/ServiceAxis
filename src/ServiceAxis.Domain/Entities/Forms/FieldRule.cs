using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Entities.Platform;
using ServiceAxis.Domain.Enums;

namespace ServiceAxis.Domain.Entities.Forms;

/// <summary>
/// Allows admins to configure Auto-Fill and Field Actions:
/// e.g. "IF Country=India, SET Currency=INR".
/// </summary>
public class FieldRule : BaseEntity
{
    public Guid TableId { get; set; }
    public SysTable Table { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// When does this rule trigger? (e.g. When the Department field changes)
    /// Null = executes on load or generic form eval.
    /// </summary>
    public Guid? TriggerFieldId { get; set; }
    public SysField? TriggerField { get; set; }

    /// <summary>
    /// Rules engine parsable JSON condition or expression
    /// {"field": "status", "operator": "equals", "value": "approved"}
    /// </summary>
    public string ConditionJson { get; set; } = "{}";

    public Guid TargetFieldId { get; set; }
    public SysField TargetField { get; set; } = null!;

    public FieldRuleActionType ActionType { get; set; } = FieldRuleActionType.SetValue;

    /// <summary>
    /// For SetValue: "INR"
    /// For Calculate: "quantity * price"
    /// </summary>
    public string ValueExpression { get; set; } = string.Empty;

    public int ExecutionOrder { get; set; } = 200; // Usually runs after UI Policies
    public int Version { get; set; } = 1;
}
