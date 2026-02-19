using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceAxis.Domain.Entities;

namespace ServiceAxis.Infrastructure.Persistence.Configurations;

internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants", "platform");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Code).IsRequired().HasMaxLength(50);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Plan).IsRequired().HasMaxLength(50);

        builder.HasIndex(t => t.Code).IsUnique();

        builder.HasMany(t => t.Users)
               .WithOne(u => u.Tenant)
               .HasForeignKey(u => u.TenantId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("ApplicationUsers", "platform");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.IdentityUserId).IsRequired().HasMaxLength(450);
        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.Property(u => u.Department).HasMaxLength(200);
        builder.Property(u => u.JobTitle).HasMaxLength(200);

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.IdentityUserId).IsUnique();

        // Ignore computed property
        builder.Ignore(u => u.DisplayName);
    }
}

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs", "platform");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Module).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EntityType).IsRequired().HasMaxLength(200);
        builder.Property(a => a.EntityId).HasMaxLength(450);
        builder.Property(a => a.CorrelationId).HasMaxLength(100);
        builder.Property(a => a.IpAddress).HasMaxLength(50);
        builder.Property(a => a.UserId).HasMaxLength(450);
        builder.HasOne(a => a.User)
               .WithMany()
               .HasForeignKey(a => a.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        // Audit logs are append-only; disable updates
        builder.Property(a => a.UpdatedAt).IsRequired(false);
    }
}
