namespace ServiceAxis.Domain.Enums;

/// <summary>Auto-assignment strategy for a queue.</summary>
public enum AssignmentStrategy
{
    Manual = 1,
    RoundRobin = 2,
    LeastLoaded = 3,
    SkillBased = 4
}

/// <summary>Membership type within an assignment group.</summary>
public enum GroupMemberRole
{
    Member = 1,
    Manager = 2,
    Escalation = 3
}
