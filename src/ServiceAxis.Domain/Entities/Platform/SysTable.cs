using ServiceAxis.Domain.Common;

namespace ServiceAxis.Domain.Entities.Platform;

/// <summary>
/// Metadata definition of a dynamic table in the platform.
/// This is the core of the ServiceAxis configuration engine — administrators
/// create application tables (e.g. Incident, Asset, ChangeRequest) here
/// without writing any code.
/// </summary>
public class SysTable : BaseEntity
{
    /// <summary>Internal unique name used in API routes and code (e.g. "incident").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Human-readable display name (e.g. "Incident").</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Database schema name (e.g. "itsm", "asset", "hr").</summary>
    public string SchemaName { get; set; } = "platform";

    /// <summary>Optional description for admin UI.</summary>
    public string? Description { get; set; }

    /// <summary>Icon name (e.g. Lucide / FontAwesome icon key).</summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Parent table enabling inheritance (like ServiceNow's Task table).
    /// Child tables inherit all parent fields automatically.
    /// </summary>
    public Guid? ParentTableId { get; set; }
    public SysTable? ParentTable { get; set; }

    /// <summary>Whether this table supports attachments.</summary>
    public bool AllowAttachments { get; set; } = false;

    /// <summary>Whether audit logging is enabled for this table.</summary>
    public bool AuditEnabled { get; set; } = true;

    /// <summary>Auto-number prefix (e.g. "INC", "CHG", "REQ"). Null = no auto-number.</summary>
    public string? AutoNumberPrefix { get; set; }

    /// <summary>Current auto-number counter value.</summary>
    public int AutoNumberSeed { get; set; } = 1000;

    /// <summary>Whether this is a system-managed table (cannot be deleted by admin).</summary>
    public bool IsSystemTable { get; set; } = false;

    /// <summary>Tenant that owns this table definition (null = global/shared).</summary>
    public Guid? TenantId { get; set; }

    // Navigation
    public ICollection<SysField> Fields { get; set; } = new List<SysField>();
    public ICollection<SysTable> ChildTables { get; set; } = new List<SysTable>();
}
