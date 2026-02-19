using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Enums;

namespace ServiceAxis.Domain.Entities.Platform;

/// <summary>
/// Metadata definition of a field (column) belonging to a <see cref="SysTable"/>.
/// Drives database-independent dynamic data storage in <see cref="Records.RecordValue"/>.
/// </summary>
public class SysField : BaseEntity
{
    /// <summary>Owning table.</summary>
    public Guid TableId { get; set; }
    public SysTable Table { get; set; } = null!;

    /// <summary>Internal API/storage name (e.g. "priority", "assigned_to").</summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>Human-readable label shown on forms.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>The data type — controls validation, storage, and UI widget.</summary>
    public FieldDataType DataType { get; set; } = FieldDataType.Text;

    /// <summary>Whether the field is mandatory when creating a record.</summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>Default value serialised as a string (e.g. "true", "1", "high").</summary>
    public string? DefaultValue { get; set; }

    /// <summary>Whether the field appears in list views and is searchable via API.</summary>
    public bool IsSearchable { get; set; } = false;

    /// <summary>Whether the field is visible in the list/table view.</summary>
    public bool IsListVisible { get; set; } = true;

    /// <summary>Whether this field can be sorted in list views.</summary>
    public bool IsSortable { get; set; } = false;

    /// <summary>
    /// For <see cref="FieldDataType.Lookup"/> fields — the referenced table name.
    /// </summary>
    public string? LookupTableName { get; set; }

    /// <summary>
    /// For <see cref="FieldDataType.Choice"/> fields — JSON array of
    /// choice options: [{"value":"high","label":"High"}, ...].
    /// </summary>
    public string? ChoiceOptions { get; set; }

    /// <summary>
    /// For <see cref="FieldDataType.AutoNumber"/> fields — format pattern.
    /// e.g. "{PREFIX}{NUMBER:0000}" → "INC0001"
    /// </summary>
    public string? AutoNumberFormat { get; set; }

    /// <summary>Maximum length for text fields (null = unlimited).</summary>
    public int? MaxLength { get; set; }

    /// <summary>Minimum value for number fields.</summary>
    public decimal? MinValue { get; set; }

    /// <summary>Maximum value for number fields.</summary>
    public decimal? MaxValue { get; set; }

    /// <summary>Display order on forms (lower = rendered first).</summary>
    public int DisplayOrder { get; set; } = 100;

    /// <summary>Help text shown as tooltip/hint next to the field.</summary>
    public string? HelpText { get; set; }

    /// <summary>Whether this is a system field that cannot be deleted (e.g. "number", "state").</summary>
    public bool IsSystemField { get; set; } = false;

    /// <summary>Whether the field value is read-only in UI forms.</summary>
    public bool IsReadOnly { get; set; } = false;

    /// <summary>Tenant scope (null = global).</summary>
    public Guid? TenantId { get; set; }

    // Navigation
    public ICollection<SysChoice> Choices { get; set; } = new List<SysChoice>();
}
