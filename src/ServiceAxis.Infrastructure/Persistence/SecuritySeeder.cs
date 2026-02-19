using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServiceAxis.Domain.Entities;
using ServiceAxis.Domain.Entities.Forms;
using ServiceAxis.Domain.Entities.Workflow;
using ServiceAxis.Domain.Entities.Security;
using ServiceAxis.Domain.Enums;
using ServiceAxis.Infrastructure.Persistence;

namespace ServiceAxis.Infrastructure.Persistence;

public static class SecuritySeeder
{
    public static async Task SeedAsync(ServiceAxisDbContext db, RoleManager<IdentityRole> roleManager)
    {
        // 1. Ensure Default Tenant exists
        var tenant = await EnsureTenantAsync(db);

        // 2. Ensure Role-Permission mappings for standard roles
        await SeedRolePermissionsAsync(db, roleManager);

        // 3. Table Permissions (Incident)
        await SeedTablePermissionsAsync(db, roleManager, tenant.Id);
    }

    private static async Task<Tenant> EnsureTenantAsync(ServiceAxisDbContext db)
    {
        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Code == "DEFAULT");
        if (tenant == null)
        {
            tenant = new Tenant
            {
                Code = "DEFAULT",
                Name = "Default Organization",
                IsActive = true
            };
            db.Tenants.Add(tenant);
            await db.SaveChangesAsync();
        }
        return tenant;
    }

    private static async Task SeedRolePermissionsAsync(ServiceAxisDbContext db, RoleManager<IdentityRole> roleManager)
    {
        // Define some core functional permissions
        var permissions = new[]
        {
            new { Key = "platform.activity.internal_notes", Name = "View Internal Activity Notes", Module = "Collaboration" },
            new { Key = "platform.admin.metadata", Name = "Manage Metadata", Module = "System" },
            new { Key = "platform.admin.security", Name = "Manage Security", Module = "System" }
        };

        foreach (var p in permissions)
        {
            if (!await db.Permissions.AnyAsync(x => x.Key == p.Key))
            {
                db.Permissions.Add(new Permission { Key = p.Key, Name = p.Name, Module = p.Module });
            }
        }
        await db.SaveChangesAsync();

        // Assign 'platform.activity.internal_notes' to Agent and Admin
        var agentRole = await roleManager.FindByNameAsync("Agent");
        var adminRole = await roleManager.FindByNameAsync("Admin");
        var internalNotesPerm = await db.Permissions.FirstAsync(p => p.Key == "platform.activity.internal_notes");

        if (agentRole != null && !await db.RolePermissions.AnyAsync(rp => rp.RoleId == agentRole.Id && rp.PermissionId == internalNotesPerm.Id))
        {
            db.RolePermissions.Add(new RolePermission { RoleId = agentRole.Id, PermissionId = internalNotesPerm.Id });
        }

        if (adminRole != null && !await db.RolePermissions.AnyAsync(rp => rp.RoleId == adminRole.Id && rp.PermissionId == internalNotesPerm.Id))
        {
            db.RolePermissions.Add(new RolePermission { RoleId = adminRole.Id, PermissionId = internalNotesPerm.Id });
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedTablePermissionsAsync(ServiceAxisDbContext db, RoleManager<IdentityRole> roleManager, Guid tenantId)
    {
        var agentRole = await roleManager.FindByNameAsync("Agent");
        if (agentRole == null) return;

        // Give Agent 'Write' access to Incident table by default
        if (!await db.TablePermissions.AnyAsync(tp => tp.RoleId == agentRole.Id && tp.TableName == "incident"))
        {
            db.TablePermissions.Add(new TablePermission
            {
                RoleId = agentRole.Id,
                TableName = "incident",
                PermissionType = PermissionType.Write,
                TenantId = tenantId
            });
        }

        // Give Admin 'Admin' level access
        var adminRole = await roleManager.FindByNameAsync("Admin");
        if (adminRole != null && !await db.TablePermissions.AnyAsync(tp => tp.RoleId == adminRole.Id && tp.TableName == "incident"))
        {
            db.TablePermissions.Add(new TablePermission
            {
                RoleId = adminRole.Id,
                TableName = "incident",
                PermissionType = PermissionType.Admin,
                TenantId = tenantId
            });
        }

        await db.SaveChangesAsync();
    }
}
