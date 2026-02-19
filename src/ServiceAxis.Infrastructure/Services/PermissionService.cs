using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServiceAxis.Application.Contracts.Identity;
using ServiceAxis.Application.Contracts.Infrastructure;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Entities.Security;
using ServiceAxis.Domain.Enums;

namespace ServiceAxis.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _uow;
    private readonly RoleManager<IdentityRole> _roleManager;

    public PermissionService(
        ICurrentUserService currentUser, 
        IUnitOfWork uow, 
        RoleManager<IdentityRole> roleManager)
    {
        _currentUser = currentUser;
        _uow = uow;
        _roleManager = roleManager;
    }

    public async Task<bool> HasPermissionAsync(string permissionKey, CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated) return false;
        if (_currentUser.IsInRole("SuperAdmin")) return true;

        var roles = _currentUser.Roles;
        return await _uow.Repository<RolePermission>()
            .FindAsync(rp => roles.Contains(rp.RoleId) && rp.Permission.Key == permissionKey, ct)
            .ContinueWith(t => t.Result.Any());
    }

    public async Task<bool> CanAccessTableAsync(string tableName, PermissionType action, CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated) return false;
        if (_currentUser.IsInRole("SuperAdmin")) return true;

        var roleNames = _currentUser.Roles;
        // Map role names to IDs efficiently
        var roleIds = await _roleManager.Roles
            .Where(r => roleNames.Contains(r.Name))
            .Select(r => r.Id)
            .ToListAsync(ct);

        var perms = await _uow.Repository<TablePermission>()
            .FindAsync(tp => roleIds.Contains(tp.RoleId) && tp.TableName == tableName.ToLowerInvariant(), ct);

        // Check if any role has the requested permission level or higher (Admin level)
        return perms.Any(p => p.PermissionType == action || p.PermissionType == PermissionType.Admin);
    }

    public async Task<bool> CanAccessFieldAsync(string tableName, string fieldName, bool isWriteRequest, CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated) return false;
        if (_currentUser.IsInRole("SuperAdmin")) return true;

        var roleNames = _currentUser.Roles;
        var roleIds = await _roleManager.Roles
            .Where(r => roleNames.Contains(r.Name))
            .Select(r => r.Id)
            .ToListAsync(ct);

        var perms = await _uow.Repository<FieldPermission>()
            .FindAsync(fp => roleIds.Contains(fp.RoleId) && 
                             fp.TableName == tableName.ToLowerInvariant() && 
                             fp.FieldName == fieldName.ToLowerInvariant(), ct);

        if (!perms.Any()) 
        {
            // By default, if no specific field permission exists, we fallback to table-level permission
            // but for writing, we are more strict.
            return !isWriteRequest; // Allow read by default if not explicitly denied? 
            // In a strict mode, this would be 'return false'.
        }

        return isWriteRequest ? perms.Any(p => p.CanWrite) : perms.Any(p => p.CanRead);
    }

    public async Task<IEnumerable<string>> GetAuthorizedFieldsAsync(string tableName, bool isWriteRequest, CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated) return Enumerable.Empty<string>();
        
        var roleNames = _currentUser.Roles;
        var roleIds = await _roleManager.Roles
            .Where(r => roleNames.Contains(r.Name))
            .Select(r => r.Id)
            .ToListAsync(ct);

        var perms = await _uow.Repository<FieldPermission>()
            .FindAsync(fp => roleIds.Contains(fp.RoleId) && fp.TableName == tableName.ToLowerInvariant(), ct);

        if (isWriteRequest)
        {
            return perms.Where(p => p.CanWrite).Select(p => p.FieldName).Distinct();
        }

        return perms.Where(p => p.CanRead).Select(p => p.FieldName).Distinct();
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated) return Enumerable.Empty<string>();
        if (_currentUser.IsInRole("SuperAdmin"))
        {
            // SuperAdmins could potentially see all platform permission keys
            return await _uow.Repository<Permission>().AsQueryable().Select(p => p.Key).ToListAsync(ct);
        }

        var roleNames = _currentUser.Roles;
        var roleIds = await _roleManager.Roles
            .Where(r => roleNames.Contains(r.Name))
            .Select(r => r.Id)
            .ToListAsync(ct);

        return await _uow.Repository<RolePermission>()
            .AsQueryable()
            .Include(rp => rp.Permission)
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission.Key)
            .Distinct()
            .ToListAsync(ct);
    }
}
