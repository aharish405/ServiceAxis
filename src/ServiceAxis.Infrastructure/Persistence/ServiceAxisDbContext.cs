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
using ServiceAxis.Domain.Entities.Activity;
using ServiceAxis.Domain.Entities.Automation;
using ServiceAxis.Domain.Entities.Workflow;
using ServiceAxis.Application.Contracts.Identity;

namespace ServiceAxis.Infrastructure.Persistence;

public class ServiceAxisDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
{
    private readonly ICurrentUserService _currentUser;

    public ServiceAxisDbContext(DbContextOptions<ServiceAxisDbContext> options, ICurrentUserService currentUser)
        : base(options) 
    {
        _currentUser = currentUser;
    }

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
    public DbSet<AssignmentGroup>        AssignmentGroups        => Set<AssignmentGroup>();
    public DbSet<GroupMember>            GroupMembers            => Set<GroupMember>();
    public DbSet<Queue>                  Queues                  => Set<Queue>();
    public DbSet<RecordStateDefinition> RecordStateDefinitions  => Set<RecordStateDefinition>();
    public DbSet<StateTransition>       StateTransitions        => Set<StateTransition>();
    public DbSet<RecordAssignment>      RecordAssignments       => Set<RecordAssignment>();

    // ─── SLA ───
    public DbSet<SlaDefinition>      SlaDefinitions     => Set<SlaDefinition>();
    public DbSet<SlaPolicy>          SlaPolicies        => Set<SlaPolicy>();
    public DbSet<SlaInstance>        SlaInstances       => Set<SlaInstance>();
    public DbSet<SlaTarget>          SlaTargets         => Set<SlaTarget>();
    public DbSet<SlaEscalationRule>  SlaEscalationRules => Set<SlaEscalationRule>();
    public DbSet<BusinessCalendar>   BusinessCalendars  => Set<BusinessCalendar>();
    public DbSet<SlaTimerEvent>      SlaTimerEvents     => Set<SlaTimerEvent>();

    // ── Workflow entities ─────────────────────────────────────────────────
    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();
    public DbSet<WorkflowStep> WorkflowSteps => Set<WorkflowStep>();
    public DbSet<WorkflowTransition> WorkflowTransitions => Set<WorkflowTransition>();
    public DbSet<WorkflowAction> WorkflowActions => Set<WorkflowAction>();

    // ─── Activity & Collaboration ───
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<FieldChange> FieldChanges => Set<FieldChange>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Attachment> Attachments => Set<Attachment>();

    // ─── Automation Engine ───
    public DbSet<AutomationRule> AutomationRules => Set<AutomationRule>();
    public DbSet<AutomationTrigger> AutomationTriggers => Set<AutomationTrigger>();
    public DbSet<AutomationCondition> AutomationConditions => Set<AutomationCondition>();
    public DbSet<AutomationAction> AutomationActions => Set<AutomationAction>();
    public DbSet<AutomationExecutionLog> AutomationExecutionLogs => Set<AutomationExecutionLog>();

    // ─── Dynamic UI Rules Engine ───
    public DbSet<ServiceAxis.Domain.Entities.Forms.UiPolicy> UiPolicies => Set<ServiceAxis.Domain.Entities.Forms.UiPolicy>();
    public DbSet<ServiceAxis.Domain.Entities.Forms.UiPolicyCondition> UiPolicyConditions => Set<ServiceAxis.Domain.Entities.Forms.UiPolicyCondition>();
    public DbSet<ServiceAxis.Domain.Entities.Forms.UiPolicyAction> UiPolicyActions => Set<ServiceAxis.Domain.Entities.Forms.UiPolicyAction>();
    public DbSet<ServiceAxis.Domain.Entities.Forms.FieldRule> FieldRules => Set<ServiceAxis.Domain.Entities.Forms.FieldRule>();
    public DbSet<ServiceAxis.Domain.Entities.Forms.ClientScript> ClientScripts => Set<ServiceAxis.Domain.Entities.Forms.ClientScript>();


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
            .HasOne(r => r.Table)
            .WithMany()
            .HasForeignKey(r => r.TableId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PlatformRecord>()
            .HasIndex(r => new { r.TableId, r.CreatedAt });
        
        builder.Entity<PlatformRecord>()
            .HasIndex(r => r.RecordNumber).IsUnique();

        builder.Entity<FormDefinition>()
            .HasOne(fd => fd.Table) // Navigation prop exists
            .WithMany()
            .HasForeignKey(fd => fd.TableId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<WorkflowTrigger>()
            .HasOne(wt => wt.Table)
            .WithMany()
            .HasForeignKey(wt => wt.TableId)
            .OnDelete(DeleteBehavior.Restrict);


        // --- Multi-tenancy & Soft Delete Filters ---
        builder.Entity<PlatformRecord>().HasQueryFilter(r => !r.IsDeleted && (r.TenantId == _currentUser.TenantId || _currentUser.TenantId == null));
        builder.Entity<WorkflowInstance>().HasQueryFilter(w => w.TenantId == _currentUser.TenantId || _currentUser.TenantId == null);
        builder.Entity<TablePermission>().HasQueryFilter(tp => tp.TenantId == _currentUser.TenantId || _currentUser.TenantId == null);
        builder.Entity<FieldPermission>().HasQueryFilter(fp => fp.TenantId == _currentUser.TenantId || _currentUser.TenantId == null);
        builder.Entity<Activity>().HasQueryFilter(a => a.Record.TenantId == _currentUser.TenantId || _currentUser.TenantId == null);
        builder.Entity<Attachment>().HasQueryFilter(a => a.Record.TenantId == _currentUser.TenantId || _currentUser.TenantId == null);
        builder.Entity<AssignmentGroup>().HasQueryFilter(ag => ag.TenantId == _currentUser.TenantId || _currentUser.TenantId == null);
        builder.Entity<Queue>().HasQueryFilter(q => q.TenantId == _currentUser.TenantId || _currentUser.TenantId == null);
        builder.Entity<NotificationTemplate>().HasQueryFilter(nt => nt.TenantId == _currentUser.TenantId || _currentUser.TenantId == null);
        builder.Entity<NotificationLog>().HasQueryFilter(nl => nl.TenantId == _currentUser.TenantId || _currentUser.TenantId == null);




        // RecordValue dependencies
        builder.Entity<RecordValue>()
            .HasOne(v => v.Record)
            .WithMany(r => r.Values)
            .HasForeignKey(v => v.RecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RecordValue>()
            .HasOne(v => v.Field)
            .WithMany()
            .HasForeignKey(v => v.FieldId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<RecordValue>()
            .HasIndex(v => new { v.RecordId, v.FieldId });

        builder.Entity<RecordAudit>()
            .HasOne(ra => ra.Record)
            .WithMany(r => r.Audits)
            .HasForeignKey(ra => ra.RecordId)
            .OnDelete(DeleteBehavior.Cascade);

        // ─── Activity System Configuration ───
        builder.Entity<Activity>().ToTable("Activities", "platform");
        builder.Entity<FieldChange>().ToTable("FieldChanges", "platform");
        builder.Entity<Comment>().ToTable("Comments", "platform");
        builder.Entity<Attachment>().ToTable("Attachments", "platform");

        builder.Entity<Activity>()
            .HasOne(a => a.Table)
            .WithMany()
            .HasForeignKey(a => a.TableId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Activity>()
            .HasIndex(a => new { a.RecordId, a.CreatedAt });

        // ─── State Machine Configuration ───
        builder.Entity<RecordStateDefinition>().ToTable("RecordStateDefinitions", "platform");
        builder.Entity<StateTransition>().ToTable("StateTransitions", "platform");
        builder.Entity<RecordAssignment>().ToTable("RecordAssignments", "platform");

        builder.Entity<RecordStateDefinition>()
            .HasOne(s => s.Table)
            .WithMany()
            .HasForeignKey(s => s.TableId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StateTransition>()
            .HasOne(t => t.Table)
            .WithMany()
            .HasForeignKey(t => t.TableId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StateTransition>()
            .HasOne(t => t.FromState)
            .WithMany(s => s.TransitionsFrom)
            .HasForeignKey(t => t.FromStateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StateTransition>()
            .HasOne(t => t.ToState)
            .WithMany(s => s.TransitionsTo)
            .HasForeignKey(t => t.ToStateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<RecordAssignment>()
            .HasOne(ra => ra.Record)
            .WithMany()
            .HasForeignKey(ra => ra.RecordId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for performance (requirement §11)
        builder.Entity<PlatformRecord>()
            .HasIndex(r => r.CurrentStateId);
        builder.Entity<RecordAssignment>()
            .HasIndex(ra => ra.RecordId);
        builder.Entity<RecordAssignment>()
            .HasIndex(ra => ra.AssignedGroupId);
        builder.Entity<RecordAssignment>()
            .HasIndex(ra => ra.AssignedUserId);

        // CurrentState — optional FK (nullable)
        builder.Entity<PlatformRecord>()
            .HasOne(r => r.CurrentState)
            .WithMany()
            .HasForeignKey(r => r.CurrentStateId)
            .OnDelete(DeleteBehavior.SetNull);

        // Tenant filters
        builder.Entity<RecordStateDefinition>().HasQueryFilter(
            s => s.TenantId == _currentUser.TenantId || _currentUser.TenantId == null);
        builder.Entity<StateTransition>().HasQueryFilter(
            t => t.TenantId == _currentUser.TenantId || _currentUser.TenantId == null);

        builder.Entity<FieldChange>()
            .HasOne(f => f.Activity)
            .WithMany(a => a.FieldChanges)
            .HasForeignKey(f => f.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Comment>()
            .HasOne(c => c.Activity)
            .WithMany(a => a.Comments)
            .HasForeignKey(c => c.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Attachment>()
            .HasOne(a => a.Record)
            .WithMany()
            .HasForeignKey(a => a.RecordId)
            .OnDelete(DeleteBehavior.Cascade);

        // ─── SLA Engine Configuration ───
        builder.Entity<SlaDefinition>().ToTable("SlaDefinitions", "platform");
        builder.Entity<SlaTarget>().ToTable("SlaTargets", "platform");
        builder.Entity<SlaPolicy>().ToTable("SlaPolicies", "platform");
        builder.Entity<SlaInstance>().ToTable("SlaInstances", "platform");
        builder.Entity<SlaTimerEvent>().ToTable("SlaTimerEvents", "platform");
        builder.Entity<SlaEscalationRule>().ToTable("SlaEscalationRules", "platform");
        builder.Entity<BusinessCalendar>().ToTable("BusinessCalendars", "platform");

        builder.Entity<SlaInstance>()
            .HasIndex(s => s.RecordId);
        
        builder.Entity<SlaInstance>()
            .HasIndex(s => new { s.IsBreached, s.Status }); // For monitoring job

        builder.Entity<SlaInstance>()
            .HasIndex(s => s.TargetTime); // For identifying nearing breaches

        builder.Entity<SlaInstance>()
            .HasIndex(s => s.IsPaused);

        // Explicit relationships
        builder.Entity<SlaTarget>()
            .HasOne(t => t.SlaDefinition)
            .WithMany(d => d.Targets)
            .HasForeignKey(t => t.SlaDefinitionId)
            .OnDelete(DeleteBehavior.Cascade); // Def deletion removes targets

        builder.Entity<SlaPolicy>()
            .HasOne(p => p.SlaDefinition)
            .WithMany(d => d.Policies)
            .HasForeignKey(p => p.SlaDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SlaEscalationRule>()
            .HasOne(r => r.SlaDefinition)
            .WithMany(d => d.EscalationRules)
            .HasForeignKey(r => r.SlaDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SlaTimerEvent>()
            .HasOne<SlaInstance>()
            .WithMany(i => i.TimerEvents)
            .HasForeignKey(e => e.SlaInstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        // ─── Automation Engine Configuration ───
        builder.Entity<AutomationRule>().ToTable("AutomationRules", "platform");
        builder.Entity<AutomationTrigger>().ToTable("AutomationTriggers", "platform");
        builder.Entity<AutomationCondition>().ToTable("AutomationConditions", "platform");
        builder.Entity<AutomationAction>().ToTable("AutomationActions", "platform");
        builder.Entity<AutomationExecutionLog>().ToTable("AutomationExecutionLogs", "platform");

        builder.Entity<AutomationTrigger>()
            .HasOne(t => t.Rule)
            .WithMany(r => r.Triggers)
            .HasForeignKey(t => t.RuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<AutomationCondition>()
            .HasOne(c => c.Rule)
            .WithMany(r => r.Conditions)
            .HasForeignKey(c => c.RuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<AutomationAction>()
            .HasOne(a => a.Rule)
            .WithMany(r => r.Actions)
            .HasForeignKey(a => a.RuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<AutomationExecutionLog>()
            .HasOne(l => l.Rule)
            .WithMany()
            .HasForeignKey(l => l.RuleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Automation Tenant filters
        builder.Entity<AutomationRule>().HasQueryFilter(x => x.TenantId == _currentUser.TenantId || _currentUser.TenantId == null);

        // Fix multiple cascade paths (SQL Server error 1785)
        foreach (var relationship in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            if (relationship.DeclaringEntityType.ClrType == typeof(RecordValue) && relationship.PrincipalEntityType.ClrType == typeof(PlatformRecord))
                continue;

            // Automation/UI Rule Ownership paths
            if (relationship.DeclaringEntityType.ClrType == typeof(AutomationTrigger) && relationship.PrincipalEntityType.ClrType == typeof(AutomationRule))
                continue;
            if (relationship.DeclaringEntityType.ClrType == typeof(AutomationCondition) && relationship.PrincipalEntityType.ClrType == typeof(AutomationRule))
                continue;
            if (relationship.DeclaringEntityType.ClrType == typeof(AutomationAction) && relationship.PrincipalEntityType.ClrType == typeof(AutomationRule))
                continue;

            if (relationship.DeclaringEntityType.ClrType == typeof(ServiceAxis.Domain.Entities.Forms.UiPolicyCondition) && relationship.PrincipalEntityType.ClrType == typeof(ServiceAxis.Domain.Entities.Forms.UiPolicy))
                continue;
            if (relationship.DeclaringEntityType.ClrType == typeof(ServiceAxis.Domain.Entities.Forms.UiPolicyAction) && relationship.PrincipalEntityType.ClrType == typeof(ServiceAxis.Domain.Entities.Forms.UiPolicy))
                continue;

            if (relationship.DeclaringEntityType.ClrType == typeof(SysChoice) && relationship.PrincipalEntityType.ClrType == typeof(SysField))
                continue;

            if (relationship.DeclaringEntityType.ClrType == typeof(FieldChange) && relationship.PrincipalEntityType.ClrType == typeof(Activity))
                continue;

            if (relationship.DeclaringEntityType.ClrType == typeof(Comment) && relationship.PrincipalEntityType.ClrType == typeof(Activity))
                continue;

            if (relationship.DeclaringEntityType.ClrType == typeof(Activity) && (relationship.PrincipalEntityType.ClrType == typeof(PlatformRecord) || relationship.PrincipalEntityType.ClrType == typeof(SysTable)))
                continue;

            if (relationship.DeclaringEntityType.ClrType == typeof(Attachment) && relationship.PrincipalEntityType.ClrType == typeof(PlatformRecord))
                continue;

            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }
}
