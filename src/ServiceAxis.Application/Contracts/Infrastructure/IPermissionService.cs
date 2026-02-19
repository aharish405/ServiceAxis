using ServiceAxis.Domain.Enums;

namespace ServiceAxis.Application.Contracts.Infrastructure;

public interface IPermissionService
{
    /// <summary>Checks if the current user has the specified platform-level permission key.</summary>
    Task<bool> HasPermissionAsync(string permissionKey, CancellationToken ct = default);

    /// <summary>Checks if the current user can perform an action on a specific table.</summary>
    Task<bool> CanAccessTableAsync(string tableName, PermissionType action, CancellationToken ct = default);

    /// <summary>Checks if the current user can read or write to a specific field in a table.</summary>
    Task<bool> CanAccessFieldAsync(string tableName, string fieldName, bool isWriteRequest, CancellationToken ct = default);

    /// <summary>Filters a list of field names based on the user's read/write permissions.</summary>
    Task<IEnumerable<string>> GetAuthorizedFieldsAsync(string tableName, bool isWriteRequest, CancellationToken ct = default);

    /// <summary>Returns a list of all functional permission keys assigned to the current user.</summary>
    Task<IEnumerable<string>> GetUserPermissionsAsync(CancellationToken ct = default);
}
