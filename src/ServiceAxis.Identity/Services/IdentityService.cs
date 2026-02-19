using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServiceAxis.Application.Contracts.Identity;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Entities;
using ServiceAxis.Shared.Exceptions;

namespace ServiceAxis.Identity.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public IdentityService(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<TenantDto?> GetTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _uow.Repository<Tenant>().GetByIdAsync(tenantId, ct);
        if (tenant == null) return null;

        return new TenantDto(tenant.Id, tenant.Code, tenant.Name, tenant.ContactEmail, tenant.Plan, tenant.IsActive);
    }

    public async Task<List<TenantDto>> GetAllTenantsAsync(CancellationToken ct = default)
    {
        var tenants = await _uow.Repository<Tenant>().GetAllAsync(ct);
        return tenants.Select(t => new TenantDto(t.Id, t.Code, t.Name, t.ContactEmail, t.Plan, t.IsActive)).ToList();
    }

    public async Task<Guid> CreateTenantAsync(CreateTenantRequest request, CancellationToken ct = default)
    {
        if (await _uow.Repository<Tenant>().ExistsAsync(t => t.Code == request.Code, ct))
            throw new ConflictException($"Tenant with code '{request.Code}' already exists.");

        var tenant = new Tenant
        {
            Code = request.Code,
            Name = request.Name,
            ContactEmail = request.ContactEmail,
            Plan = request.Plan,
            IsActive = true
        };

        await _uow.Repository<Tenant>().AddAsync(tenant, ct);
        await _uow.SaveChangesAsync(ct);

        return tenant.Id;
    }

    public async Task UpdateTenantAsync(Guid tenantId, UpdateTenantRequest request, CancellationToken ct = default)
    {
        var tenant = await _uow.Repository<Tenant>().GetByIdAsync(tenantId, ct)
            ?? throw new NotFoundException("Tenant", tenantId);

        tenant.Name = request.Name;
        tenant.ContactEmail = request.ContactEmail;
        tenant.Plan = request.Plan;
        tenant.IsActive = request.IsActive;

        _uow.Repository<Tenant>().Update(tenant);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<UserDto>> GetUsersAsync(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var query = _uow.Repository<ApplicationUser>().AsQueryable();

        // If not superadmin, only show users for current tenant
        if (!_currentUser.Roles.Contains("SuperAdmin"))
        {
            var tenantId = _currentUser.TenantId;
            query = query.Where(u => u.TenantId == tenantId);
        }

        var total = await query.CountAsync(ct);
        var users = await query
            .OrderBy(u => u.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var dtos = new List<UserDto>();
        foreach (var u in users)
        {
            var idUser = await _userManager.FindByIdAsync(u.IdentityUserId);
            var roles = idUser != null ? await _userManager.GetRolesAsync(idUser) : Enumerable.Empty<string>();
            dtos.Add(new UserDto(u.IdentityUserId, u.Email, u.FirstName, u.LastName, roles, u.TenantId, u.IsActive));
        }

        return new PagedResult<UserDto>
        {
            Items = dtos,
            TotalCount = total,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    public async Task<UserDto?> GetUserAsync(string userId, CancellationToken ct = default)
    {
        var appUser = (await _uow.Repository<ApplicationUser>().FindAsync(u => u.IdentityUserId == userId)).FirstOrDefault();
        if (appUser == null) return null;

        var idUser = await _userManager.FindByIdAsync(userId);
        var roles = idUser != null ? await _userManager.GetRolesAsync(idUser) : Enumerable.Empty<string>();

        return new UserDto(appUser.IdentityUserId, appUser.Email, appUser.FirstName, appUser.LastName, roles, appUser.TenantId, appUser.IsActive);
    }

    public async Task UpdateUserRolesAsync(string userId, IEnumerable<string> roles, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        var currentRoles = await _userManager.GetRolesAsync(user);
        
        // Remove old roles, add new ones
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        
        foreach (var role in roles)
        {
            if (await _roleManager.RoleExistsAsync(role))
            {
                await _userManager.AddToRoleAsync(user, role);
            }
        }
    }

    public async Task UpdateUserProfileAsync(string userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var appUser = (await _uow.Repository<ApplicationUser>().FindAsync(u => u.IdentityUserId == userId)).FirstOrDefault()
            ?? throw new NotFoundException("User Profile", userId);

        appUser.FirstName = request.FirstName;
        appUser.LastName = request.LastName;
        appUser.PhoneNumber = request.PhoneNumber;

        _uow.Repository<ApplicationUser>().Update(appUser);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task DeactivateUserAsync(string userId, CancellationToken ct = default)
    {
        var appUser = (await _uow.Repository<ApplicationUser>().FindAsync(u => u.IdentityUserId == userId)).FirstOrDefault()
            ?? throw new NotFoundException("User Profile", userId);

        appUser.IsActive = false;
        _uow.Repository<ApplicationUser>().Update(appUser);
        await _uow.SaveChangesAsync(ct);
    }
}
