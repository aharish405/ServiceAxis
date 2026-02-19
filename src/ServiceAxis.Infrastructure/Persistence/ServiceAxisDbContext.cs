using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ServiceAxis.Domain.Entities;

using ServiceAxis.Domain.Entities.Assignment;
using ServiceAxis.Domain.Entities.Forms;
using ServiceAxis.Domain.Entities.Notifications;
using ServiceAxis.Domain.Entities.Platform;
using ServiceAxis.Domain.Entities.Records;
using ServiceAxis.Domain.Entities.Security;
using ServiceAxis.Domain.Entities.Sla;
using ServiceAxis.Domain.Entities.Workflow;

namespace ServiceAxis.Infrastructure.Persistence;

/// <summary>
/// Main application DbContext combining ASP.NET Identity tables with
/// platform domain entities. Uses schema separation to keep concerns clean:
///   - [identity] — ASP.NET Identity tables
///   - [platform] — Core platform entities (Tenant, Users, AuditLogs)
///   - [workflow]  — Workflow engine entities
/// </summary>
public class ServiceAxisDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
{
    public ServiceAxisDbContext(DbContextOptions<ServiceAxisDbContext> options)
        : base(options) { }

    // ── Platform entities ─────────────────────────────────────────────────
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // ─── Platform Metadata ───
    public DbSet<SysTable> SysTables => Set<SysTable>();
    public DbSet<SysField> SysFields => Set<SysField>();
    public DbSet<SysChoice> SysChoices => Set<SysChoice>();

    // ─── Universal Record Engine ───
    public DbSet<PlatformRecord> PlatformRecords => Set<PlatformRecord>();
    public DbSet<RecordValue> RecordValues => Set<RecordValue>();

    // ─── Forms Engine ───
    public DbSet<FormDefinition> FormDefinitions => Set<FormDefinition>();
    public DbSet<FormSection> FormSections => Set<FormSection>();
    public DbSet<FormFieldMapping> FormFieldMappings => Set<FormFieldMapping>();

    // ─── Workflow & Triggers ───
    public DbSet<WorkflowTrigger> WorkflowTriggers => Set<WorkflowTrigger>();

    // ─── Security (RBAC) ───
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<TablePermission> TablePermissions => Set<TablePermission>();
    public DbSet<FieldPermission> FieldPermissions => Set<FieldPermission>();

    // ─── Notifications ───
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
    public DbSet<NotificationChannel> NotificationChannels => Set<NotificationChannel>();
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

    // ─── Assignment ───
    public DbSet<AssignmentGroup> AssignmentGroups => Set<AssignmentGroup>();
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
    public DbSet<Queue> Queues => Set<Queue>();

    // ─── SLA ───
    public DbSet<SlaDefinition> SlaDefinitions => Set<SlaDefinition>();
    public DbSet<SlaPolicy> SlaPolicies => Set<SlaPolicy>();
    public DbSet<SlaInstance> SlaInstances => Set<SlaInstance>();

    // ── Workflow entities ─────────────────────────────────────────────────
    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();
    public DbSet<WorkflowStep> WorkflowSteps => Set<WorkflowStep>();
    public DbSet<WorkflowTransition> WorkflowTransitions => Set<WorkflowTransition>();
    public DbSet<WorkflowAction> WorkflowActions => Set<WorkflowAction>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Move Identity tables to [identity] schema
        builder.Entity<IdentityUser>().ToTable("Users", "identity");
        builder.Entity<IdentityRole>().ToTable("Roles", "identity");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles", "identity");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims", "identity");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins", "identity");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims", "identity");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens", "identity");

        // Apply all IEntityTypeConfiguration<T> classes from this assembly
        builder.ApplyConfigurationsFromAssembly(typeof(ServiceAxisDbContext).Assembly);

        // Fix multiple cascade paths (SQL Server error 1785)
        // We set DeleteBehavior.Restrict for core platform entities to prevent
        // unintentional cascading deletes and circumvent cycles.

        // SysTable relationships - require manual cleanup of dependencies
        builder.Entity<SysField>()
            .HasOne(f => f.Table)
            .WithMany(t => t.Fields)
            .HasForeignKey(f => f.TableId)
            .OnDelete(DeleteBehavior.Restrict);

        // Choice configuration - Cascading delete is safe here (Part of field definition)
        builder.Entity<SysChoice>()
            .HasOne(c => c.Field)
            .WithMany(f => f.Choices)
            .HasForeignKey(c => c.FieldId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PlatformRecord>()
            .HasOne<SysTable>()
            .WithMany()
            .HasForeignKey(r => r.TableId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<FormDefinition>()
            .HasOne(fd => fd.Table) // Navigation prop exists
            .WithMany()
            .HasForeignKey(fd => fd.TableId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<WorkflowTrigger>()
            .HasOne<SysTable>()
            .WithMany()
            .HasForeignKey(wt => wt.TableId)
            .OnDelete(DeleteBehavior.Restrict);



        // RecordValue dependencies
        builder.Entity<RecordValue>()
            .HasOne<SysField>()
            .WithMany()
            .HasForeignKey(v => v.FieldId)
            .OnDelete(DeleteBehavior.Restrict);

        // Fix multiple cascade paths (SQL Server error 1785) & Enforce strict delete safety
        foreach (var relationship in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }
}
