namespace ServiceAxis.Domain.Enums;

/// <summary>
/// Supported data types for dynamic fields (<see cref="Entities.Platform.SysField"/>).
/// Each type governs validation, UI rendering, and storage serialisation.
/// </summary>
public enum FieldDataType
{
    /// <summary>Single-line or multi-line plain text.</summary>
    Text = 1,

    /// <summary>Integer or decimal number.</summary>
    Number = 2,

    /// <summary>Date only (no time).</summary>
    Date = 3,

    /// <summary>Date + time (UTC).</summary>
    DateTime = 4,

    /// <summary>Boolean (true/false).</summary>
    Boolean = 5,

    /// <summary>Foreign-key lookup to another SysTable.</summary>
    Lookup = 6,

    /// <summary>Single value from a predefined choice list.</summary>
    Choice = 7,

    /// <summary>Multiple values from a predefined choice list.</summary>
    MultiChoice = 8,

    /// <summary>Arbitrary JSON object.</summary>
    Json = 9,

    /// <summary>URL or file attachment reference.</summary>
    Attachment = 10,

    /// <summary>Email address (validated).</summary>
    Email = 11,

    /// <summary>Phone number.</summary>
    Phone = 12,

    /// <summary>Auto-incrementing number with optional prefix (e.g. INC0001).</summary>
    AutoNumber = 13,

    /// <summary>Multi-line or rich text content.</summary>
    LongText = 14
}
