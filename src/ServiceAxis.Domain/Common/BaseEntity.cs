namespace ServiceAxis.Domain.Common;

/// <summary>
/// Base entity for all domain entities.
/// Provides standard audit and soft-delete fields.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the creation timestamp (UTC).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the last update timestamp (UTC).</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Gets or sets the identifier of the user who created this record.</summary>
    public string? CreatedBy { get; set; }

    /// <summary>Gets or sets the identifier of the user who last updated this record.</summary>
    public string? UpdatedBy { get; set; }

    /// <summary>Gets or sets a value indicating whether this record is active (soft-delete support).</summary>
    public bool IsActive { get; set; } = true;
}
