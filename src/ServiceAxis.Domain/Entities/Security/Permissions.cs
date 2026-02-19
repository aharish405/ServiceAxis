using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Enums;

namespace ServiceAxis.Domain.Entities.Security;

/// <summary>
/// A named capability (e.g. "incident.read", "incident.create", "workflow.approve").
/// Permissions form the leaves of the RBAC tree and are assigned to roles
/// via <see cref="RolePermission"/>.
/// </summary>
public class Permission : BaseEntity
{
    /// <summary>Dot-notation key (e.g. "incident.create", "platform.configure").</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Human-readable name.</summary>
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>Module grouping (e.g. "ITSM", "Platform", "Workflow").</summary>
    public string Module { get; set; } = string.Empty;

    public PermissionType Type { get; set; }

    // Navigation
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

/// <summary>
/// Assigns a <see cref="Permission"/> to an ASP.NET Identity Role.
/// </summary>
public class RolePermission : BaseEntity
{
    /// <summary>ASP.NET Identity Role Id.</summary>
    public string RoleId { get; set; } = string.Empty;

    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;

    public PermissionScope Scope { get; set; } = PermissionScope.Global;
}

/// <summary>
/// Table-level data access permission for a role.
/// Controls which roles can Read/Write/Create/Delete records in a given table.
/// </summary>
public class TablePermission : BaseEntity
{
    public string RoleId { get; set; } = string.Empty;

    /// <summary>The table name (SysTable.Name) this permission applies to.</summary>
    public string TableName { get; set; } = string.Empty;

    public PermissionType PermissionType { get; set; }

    public PermissionScope Scope { get; set; } = PermissionScope.Global;

    public Guid? TenantId { get; set; }
}

/// <summary>
/// Field-level data access permission for a role.
/// Controls read/write visibility of individual SysFields per role.
/// </summary>
public class FieldPermission : BaseEntity
{
    public string RoleId { get; set; } = string.Empty;

    /// <summary>The table name.</summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>The field name (SysField.FieldName).</summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>Whether this role can read the field value.</summary>
    public bool CanRead { get; set; } = true;

    /// <summary>Whether this role can write to / update the field value.</summary>
    public bool CanWrite { get; set; } = false;

    public Guid? TenantId { get; set; }
}
