using ServiceAxis.Domain.Common;

namespace ServiceAxis.Domain.Entities;

/// <summary>
/// Represents a platform user. Identity-specific data lives in ApplicationUser (Identity layer);
/// this entity carries the domain-facing profile for a user.
/// </summary>
public class ApplicationUser : BaseEntity
{
    /// <summary>Gets or sets the ASP.NET Identity UserId (foreign key correlation).</summary>
    public string IdentityUserId { get; set; } = string.Empty;

    /// <summary>Gets or sets the user's first name.</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Gets or sets the user's last name.</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Gets or sets the user's primary email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional phone number.</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>Gets or sets the display name used in the UI.</summary>
    public string DisplayName => $"{FirstName} {LastName}".Trim();

    /// <summary>Gets or sets the user's department / organisational unit.</summary>
    public string? Department { get; set; }

    /// <summary>Gets or sets the user's job title.</summary>
    public string? JobTitle { get; set; }

    /// <summary>Gets or sets the identifier of the tenant this user belongs to.</summary>
    public Guid? TenantId { get; set; }

    // Navigation
    public Tenant? Tenant { get; set; }
    public ICollection<AuditLog> AuditLogs { get; set; } = [];
}
