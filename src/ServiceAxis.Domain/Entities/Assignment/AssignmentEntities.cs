using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Enums;

namespace ServiceAxis.Domain.Entities.Assignment;

/// <summary>
/// A named group of agents/users who handle work (e.g. "Level 1 Support", "Network Ops").
/// Records are assigned to groups before being assigned to individuals.
/// </summary>
public class AssignmentGroup : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Email address for the group (used for shared-mailbox routing).</summary>
    public string? Email { get; set; }

    /// <summary>Auto-assignment strategy for this group.</summary>
    public AssignmentStrategy DefaultStrategy { get; set; } = AssignmentStrategy.Manual;

    /// <summary>Maximum concurrent open items per member (0 = unlimited).</summary>
    public int MaxConcurrentPerMember { get; set; } = 0;

    /// <summary>Escalation group if SLA is breached.</summary>
    public Guid? EscalationGroupId { get; set; }

    public Guid? TenantId { get; set; }

    // Navigation
    public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
    public ICollection<Queue> Queues { get; set; } = new List<Queue>();
}

/// <summary>
/// Membership record linking a user to an <see cref="AssignmentGroup"/>.
/// </summary>
public class GroupMember : BaseEntity
{
    public Guid GroupId { get; set; }
    public AssignmentGroup Group { get; set; } = null!;

    /// <summary>ASP.NET Identity User Id.</summary>
    public string UserId { get; set; } = string.Empty;

    public GroupMemberRole Role { get; set; } = GroupMemberRole.Member;

    /// <summary>Whether this member is currently available for auto-assignment.</summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>Current active item count (maintained by the assignment engine).</summary>
    public int ActiveItemCount { get; set; } = 0;

    /// <summary>Sequence number for round-robin assignment.</summary>
    public int RoundRobinSequence { get; set; } = 0;
}

/// <summary>
/// A priority-ordered work queue within an <see cref="AssignmentGroup"/>.
/// Enables separate handling for VIP, critical, or specialised items.
/// </summary>
public class Queue : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Guid GroupId { get; set; }
    public AssignmentGroup Group { get; set; } = null!;

    /// <summary>Records matching this table will be placed in this queue.</summary>
    public string? TableName { get; set; }

    /// <summary>JSONPath condition for auto-routing records to this queue.</summary>
    public string? RoutingCondition { get; set; }

    /// <summary>Queue priority for the router (lower = higher priority).</summary>
    public int Priority { get; set; } = 10;

    public AssignmentStrategy Strategy { get; set; } = AssignmentStrategy.RoundRobin;

    public Guid? TenantId { get; set; }
}
