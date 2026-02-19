using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Enums;

namespace ServiceAxis.Domain.Entities.Workflow;

/// <summary>
/// Defines the blueprint of a workflow — the "template" that instances are created from.
/// A single definition may produce many instances (e.g. an "Incident Approval" workflow).
/// </summary>
public class WorkflowDefinition : AggregateRoot
{
    /// <summary>Gets or sets the machine-readable unique code for this workflow.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Gets or sets the human-readable name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional description of what this workflow handles.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the category / module this workflow belongs to (e.g. "ITSM", "HR").</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Gets or sets the version number (allows versioned workflow upgrades).</summary>
    public int Version { get; set; } = 1;

    /// <summary>Gets or sets a value indicating whether this definition is the currently published/live version.</summary>
    public bool IsPublished { get; set; }

    /// <summary>Gets or sets optional JSON-encoded metadata for this workflow.</summary>
    public string? MetaData { get; set; }

    /// <summary>Gets or sets the tenant scope (null = platform-wide).</summary>
    public Guid? TenantId { get; set; }

    // Navigation
    public ICollection<WorkflowStep> Steps { get; set; } = [];
    public ICollection<WorkflowTransition> Transitions { get; set; } = [];
    public ICollection<WorkflowInstance> Instances { get; set; } = [];
}
