using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Enums;

namespace ServiceAxis.Domain.Entities;

/// <summary>
/// Immutable audit record capturing every significant state change across the platform.
/// Designed for compliance and traceability requirements.
/// </summary>
public class AuditLog : BaseEntity
{
    /// <summary>Gets or sets the module / bounded context that raised this event.</summary>
    public string Module { get; set; } = string.Empty;

    /// <summary>Gets or sets the type of action performed (e.g. Create, Update, Delete, Login).</summary>
    public AuditAction Action { get; set; }

    /// <summary>Gets or sets the entity type that was affected.</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Gets or sets the primary key of the affected entity.</summary>
    public string? EntityId { get; set; }

    /// <summary>Gets or sets a JSON snapshot of the old values (before the change).</summary>
    public string? OldValues { get; set; }

    /// <summary>Gets or sets a JSON snapshot of the new values (after the change).</summary>
    public string? NewValues { get; set; }

    /// <summary>Gets or sets the correlation / request ID for distributed tracing.</summary>
    public string? CorrelationId { get; set; }

    /// <summary>Gets or sets the IP address of the requesting client.</summary>
    public string? IpAddress { get; set; }

    /// <summary>Gets or sets the User-Agent string of the requesting client.</summary>
    public string? UserAgent { get; set; }

    /// <summary>Gets or sets the identity of the acting user.</summary>
    public string? UserId { get; set; }

    /// <summary>Gets or sets the identifier of the tenant context.</summary>
    public Guid? TenantId { get; set; }

    // Navigation
    public ApplicationUser? User { get; set; }
}
