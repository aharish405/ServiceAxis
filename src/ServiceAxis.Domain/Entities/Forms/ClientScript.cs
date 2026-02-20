using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Entities.Platform;
using ServiceAxis.Domain.Enums;

namespace ServiceAxis.Domain.Entities.Forms;

/// <summary>
/// Allows admins to write lightweight JS logic mapped to form lifecycles.
/// Equivalent to ServiceNow Client Scripts.
/// Executed natively in the browser mapping through a safe shim API.
/// </summary>
public class ClientScript : BaseEntity
{
    public Guid TableId { get; set; }
    public SysTable Table { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ClientScriptEventType EventType { get; set; } = ClientScriptEventType.OnLoad;

    /// <summary>
    /// If EventType is OnChange, the script executes when this particular field triggers it.
    /// </summary>
    public Guid? TriggerFieldId { get; set; }
    public SysField? TriggerField { get; set; }

    /// <summary>
    /// The Javascript code snippet payload mapped inside the executor.
    /// e.g. "form.setValue('field_name', 'value');"
    /// </summary>
    public string ScriptCode { get; set; } = "function invoke(form) { /* your code here */ }";

    public int ExecutionOrder { get; set; } = 300; // Final sweep post rules
    public int Version { get; set; } = 1;
}
