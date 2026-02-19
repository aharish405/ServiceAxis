using ServiceAxis.Domain.Common;

namespace ServiceAxis.Application.Contracts.Identity;

public interface IIdentityService
{
    // Tenant Management
    Task<TenantDto?> GetTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<List<TenantDto>> GetAllTenantsAsync(CancellationToken ct = default);
    Task<Guid> CreateTenantAsync(CreateTenantRequest request, CancellationToken ct = default);
    Task UpdateTenantAsync(Guid tenantId, UpdateTenantRequest request, CancellationToken ct = default);

    // User Management
    Task<PagedResult<UserDto>> GetUsersAsync(int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<UserDto?> GetUserAsync(string userId, CancellationToken ct = default);
    Task UpdateUserRolesAsync(string userId, IEnumerable<string> roles, CancellationToken ct = default);
    Task UpdateUserProfileAsync(string userId, UpdateProfileRequest request, CancellationToken ct = default);
    Task DeactivateUserAsync(string userId, CancellationToken ct = default);
}

public record TenantDto(Guid Id, string Code, string Name, string? ContactEmail, string Plan, bool IsActive);
public record CreateTenantRequest(string Code, string Name, string? ContactEmail, string Plan = "ENTERPRISE");
public record UpdateTenantRequest(string Name, string? ContactEmail, string Plan, bool IsActive);

public record UserDto(string Id, string Email, string FirstName, string LastName, IEnumerable<string> Roles, Guid? TenantId, bool IsActive);
public record UpdateProfileRequest(string FirstName, string LastName, string? PhoneNumber);
