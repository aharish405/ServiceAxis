using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceAxis.Domain.Entities.Workflow;

namespace ServiceAxis.Infrastructure.Persistence.Configurations;

internal sealed class WorkflowDefinitionConfiguration : IEntityTypeConfiguration<WorkflowDefinition>
{
    public void Configure(EntityTypeBuilder<WorkflowDefinition> builder)
    {
        builder.ToTable("WorkflowDefinitions", "workflow");

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Code).IsRequired().HasMaxLength(50);
        builder.Property(w => w.Name).IsRequired().HasMaxLength(200);
        builder.Property(w => w.Category).IsRequired().HasMaxLength(100);

        builder.HasIndex(d => new { d.Code, d.Version }).IsUnique();

        builder.HasMany(d => d.Steps)
               .WithOne(s => s.Definition)
               .HasForeignKey(s => s.DefinitionId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Transitions)
               .WithOne(t => t.Definition)
               .HasForeignKey(t => t.DefinitionId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Instances)
               .WithOne(i => i.Definition)
               .HasForeignKey(i => i.DefinitionId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class WorkflowStepConfiguration : IEntityTypeConfiguration<WorkflowStep>
{
    public void Configure(EntityTypeBuilder<WorkflowStep> builder)
    {
        builder.ToTable("WorkflowSteps", "workflow");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Code).IsRequired().HasMaxLength(50);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.StepType).IsRequired().HasMaxLength(50);
        builder.Property(s => s.RequiredRole).HasMaxLength(100);

        builder.HasMany(s => s.OutgoingTransitions)
               .WithOne(t => t.FromStep)
               .HasForeignKey(t => t.FromStepId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(s => s.IncomingTransitions)
               .WithOne(t => t.ToStep)
               .HasForeignKey(t => t.ToStepId)
               .OnDelete(DeleteBehavior.NoAction);
    }
}

internal sealed class WorkflowTransitionConfiguration : IEntityTypeConfiguration<WorkflowTransition>
{
    public void Configure(EntityTypeBuilder<WorkflowTransition> builder)
    {
        builder.ToTable("WorkflowTransitions", "workflow");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.TriggerEvent).IsRequired().HasMaxLength(100);
    }
}

internal sealed class WorkflowInstanceConfiguration : IEntityTypeConfiguration<WorkflowInstance>
{
    public void Configure(EntityTypeBuilder<WorkflowInstance> builder)
    {
        builder.ToTable("WorkflowInstances", "workflow");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.ReferenceNumber).IsRequired().HasMaxLength(50);
        builder.Property(i => i.TriggerEntityType).HasMaxLength(200);
        builder.Property(i => i.TriggerEntityId).HasMaxLength(450);

        builder.HasIndex(i => i.ReferenceNumber).IsUnique();

        builder.HasMany(i => i.Actions)
               .WithOne(a => a.Instance)
               .HasForeignKey(a => a.InstanceId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class WorkflowActionConfiguration : IEntityTypeConfiguration<WorkflowAction>
{
    public void Configure(EntityTypeBuilder<WorkflowAction> builder)
    {
        builder.ToTable("WorkflowActions", "workflow");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.TriggerEvent).IsRequired().HasMaxLength(100);
        builder.Property(a => a.ActorId).HasMaxLength(450);
    }
}
