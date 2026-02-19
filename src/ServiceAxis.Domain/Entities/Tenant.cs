using ServiceAxis.Domain.Common;

namespace ServiceAxis.Domain.Entities;

/// <summary>
/// Represents a tenant in a multi-tenant deployment of ServiceAxis.
/// This entity supports future isolation of data per organisation.
/// </summary>
public class Tenant : AggregateRoot
{
    /// <summary>Gets or sets the unique, human-readable tenant code (e.g. "ACME").</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Gets or sets the full display name of the tenant.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional description / tagline.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the primary contact email for this tenant.</summary>
    public string? ContactEmail { get; set; }

    /// <summary>Gets or sets the subscription plan key (e.g. "ENTERPRISE", "SMB").</summary>
    public string Plan { get; set; } = "ENTERPRISE";

    /// <summary>Gets or sets the UTC date/time from which this tenant is licensed.</summary>
    public DateTime LicensedFrom { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the optional UTC expiry date for the tenant.</summary>
    public DateTime? LicensedUntil { get; set; }

    // Navigation
    public ICollection<ApplicationUser> Users { get; set; } = [];
}
