using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Entities.Platform;

namespace ServiceAxis.Domain.Entities.Records;

/// <summary>
/// Stores the value of a single field for a <see cref="PlatformRecord"/>.
/// All field values are serialised to string using a type-aware strategy.
/// The <see cref="SysField"/> metadata defines how to validate and deserialise them.
/// 
/// This is the Entity-Attribute-Value (EAV) pattern used by platforms like
/// ServiceNow and Salesforce to achieve schema-less extensibility.
/// </summary>
public class RecordValue : BaseEntity
{
    public Guid RecordId { get; set; }
    public PlatformRecord Record { get; set; } = null!;

    public Guid FieldId { get; set; }
    public SysField Field { get; set; } = null!;

    /// <summary>
    /// The serialised field value as a string.
    /// Storage convention by FieldDataType:
    /// - Text        → raw string
    /// - Number      → invariant decimal string ("1234.56")
    /// - Date        → ISO 8601 date ("2026-02-19")
    /// - DateTime    → ISO 8601 UTC ("2026-02-19T11:30:00Z")
    /// - Boolean     → "true" | "false"
    /// - Lookup      → referenced record GUID string
    /// - Choice      → choice value string (e.g. "high")
    /// - MultiChoice → JSON array string (e.g. ["value1","value2"])
    /// - Json        → raw JSON string
    /// - AutoNumber  → formatted number string (e.g. "INC0001")
    /// </summary>
    public string? Value { get; set; }
}
