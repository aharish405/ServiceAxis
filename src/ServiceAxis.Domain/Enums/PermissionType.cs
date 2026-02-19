namespace ServiceAxis.Domain.Enums;

/// <summary>Permission types for RBAC + data-level access control.</summary>
public enum PermissionType
{
    /// <summary>Can read records.</summary>
    Read = 1,
    /// <summary>Can create new records.</summary>
    Create = 2,
    /// <summary>Can update existing records.</summary>
    Write = 3,
    /// <summary>Can delete records.</summary>
    Delete = 4,
    /// <summary>Can approve workflow steps.</summary>
    Approve = 5,
    /// <summary>Can configure the table/field schema.</summary>
    Configure = 6,
    /// <summary>Full unrestricted access.</summary>
    Admin = 99
}

/// <summary>Scope of a permission grant.</summary>
public enum PermissionScope
{
    /// <summary>All records in a table.</summary>
    Global = 1,
    /// <summary>Only records owned by the user's department.</summary>
    Department = 2,
    /// <summary>Only records directly assigned to the user.</summary>
    Own = 3
}
