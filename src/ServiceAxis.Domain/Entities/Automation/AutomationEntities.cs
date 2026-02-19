using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Entities.Platform;
using ServiceAxis.Domain.Enums;

namespace ServiceAxis.Domain.Entities.Automation;

public class AutomationRule : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid TableId { get; set; }
    public AutomationExecutionMode ExecutionMode { get; set; }
    public bool StopProcessingOnMatch { get; set; }
    public Guid? TenantId { get; set; }

    // Navigation
    public SysTable Table { get; set; } = null!;
    public ICollection<AutomationTrigger> Triggers { get; set; } = new List<AutomationTrigger>();
    public ICollection<AutomationCondition> Conditions { get; set; } = new List<AutomationCondition>();
    public ICollection<AutomationAction> Actions { get; set; } = new List<AutomationAction>();
}

public class AutomationTrigger : BaseEntity
{
    public Guid RuleId { get; set; }
    public AutomationEventType EventType { get; set; }

    // Navigation
    public AutomationRule Rule { get; set; } = null!;
}

public class AutomationCondition : BaseEntity
{
    public Guid RuleId { get; set; }
    public Guid FieldId { get; set; }
    public AutomationOperator Operator { get; set; }
    public string? Value { get; set; }
    public LogicalGroup LogicalGroup { get; set; }

    // Navigation
    public AutomationRule Rule { get; set; } = null!;
    public SysField Field { get; set; } = null!;
}

public class AutomationAction : BaseEntity
{
    public Guid RuleId { get; set; }
    public AutomationActionType ActionType { get; set; }
    public string ConfigurationJson { get; set; } = "{}";

    // Navigation
    public AutomationRule Rule { get; set; } = null!;
}

public class AutomationExecutionLog : BaseEntity
{
    public Guid RuleId { get; set; }
    public Guid RecordId { get; set; }
    public AutomationExecutionStatus ExecutionStatus { get; set; }
    public string? ExecutionMessage { get; set; }
    public long DurationMs { get; set; }
    public Guid? TenantId { get; set; }

    // Navigation
    public AutomationRule Rule { get; set; } = null!;
}
