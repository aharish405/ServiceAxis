using ServiceAxis.Domain.Common;

namespace ServiceAxis.Domain.Entities.Records;

/// <summary>
/// Detailed field-level history for platform records.
/// Used to render a human-readable "Activity Stream" or "Audit Trail" in the UI.
/// </summary>
public class RecordAudit : BaseEntity
{
    public Guid RecordId { get; set; }
    public Guid? FieldId { get; set; }
    
    /// <summary>The name of the field (or 'system' for non-EAV updates like state/priority).</summary>
    public string FieldName { get; set; } = string.Empty;
    
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    
    public string? Action { get; set; } // Create, Update, Delete, Transition
    public Guid? UserId { get; set; }

    // Navigation
    public PlatformRecord Record { get; set; } = null!;
}
