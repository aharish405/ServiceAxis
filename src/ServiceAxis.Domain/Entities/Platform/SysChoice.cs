using ServiceAxis.Domain.Common;

namespace ServiceAxis.Domain.Entities.Platform;

/// <summary>
/// Defines a selectable option for a Choice field (e.g. Priority: High, Medium, Low).
/// </summary>
public class SysChoice : BaseEntity
{
    public Guid FieldId { get; set; }
    public SysField Field { get; set; } = null!;

    /// <summary>Stored database value (e.g. "high", "1", "closed_complete").</summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>User-facing label (e.g. "High", "Critical", "Closed Complete").</summary>
    public string DisplayText { get; set; } = string.Empty;


    /// <summary>Sequence order in the dropdown.</summary>
    public int Order { get; set; } = 100;

    public Guid? TenantId { get; set; }
}
