using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Entities.Platform;
using ServiceAxis.Domain.Entities.Records;

namespace ServiceAxis.Domain.Entities.Assignment;

/// <summary>
/// Defines a single state within a metadata table's lifecycle.
/// Each table owns its own set of states — lifecycle is fully metadata-driven.
/// </summary>
public class RecordStateDefinition : BaseEntity
{
    /// <summary>The table this state belongs to.</summary>
    public Guid TableId { get; set; }
    public SysTable Table { get; set; } = null!;

    /// <summary>Machine-readable state key (e.g. "in_progress").</summary>
    public string StateName { get; set; } = string.Empty;

    /// <summary>Human-readable label shown in the UI.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Records start here when created.</summary>
    public bool IsInitialState { get; set; } = false;

    /// <summary>Terminal state — no further transitions allowed.</summary>
    public bool IsFinalState { get; set; } = false;

    /// <summary>UI display order (lower = first).</summary>
    public int Order { get; set; } = 10;

    /// <summary>Optional colour hint for UI badges (e.g. "green", "#3B82F6").</summary>
    public string? Color { get; set; }

    public Guid? TenantId { get; set; }

    // Navigation
    public ICollection<StateTransition> TransitionsFrom { get; set; } = new List<StateTransition>();
    public ICollection<StateTransition> TransitionsTo   { get; set; } = new List<StateTransition>();
}

/// <summary>
/// An allowed edge between two states in a table's lifecycle graph.
/// Controls exactly which state-to-state moves are permitted, and by whom.
/// </summary>
public class StateTransition : BaseEntity
{
    public Guid TableId { get; set; }
    public SysTable Table { get; set; } = null!;

    public Guid FromStateId { get; set; }
    public RecordStateDefinition FromState { get; set; } = null!;

    public Guid ToStateId { get; set; }
    public RecordStateDefinition ToState { get; set; } = null!;

    /// <summary>If true, an approval workflow must complete before the state changes.</summary>
    public bool RequiresApproval { get; set; } = false;

    /// <summary>Optional ASP.NET Identity role name that must be held to execute this transition.</summary>
    public string? AllowedRole { get; set; }

    /// <summary>Human-readable label for the transition button (e.g. "Start", "Resolve").</summary>
    public string? Label { get; set; }

    public Guid? TenantId { get; set; }
}

/// <summary>
/// Immutable audit log of every assignment event for a record.
/// One row is written whenever a record's ownership changes.
/// </summary>
public class RecordAssignment : BaseEntity
{
    public Guid RecordId { get; set; }
    public PlatformRecord Record { get; set; } = null!;

    public Guid TableId { get; set; }

    /// <summary>ASP.NET Identity user ID the record was assigned to (nullable).</summary>
    public string? AssignedUserId { get; set; }

    /// <summary>Assignment group the record was routed to (nullable).</summary>
    public Guid? AssignedGroupId { get; set; }

    /// <summary>UTC timestamp of this assignment action.</summary>
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Identity user who performed the assignment.</summary>
    public string? AssignedBy { get; set; }
}
