using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Entities.Platform;

namespace ServiceAxis.Domain.Entities.Forms;

/// <summary>
/// Defines the layout and structure of a form for a specific <see cref="SysTable"/>.
/// Multiple form definitions can exist per table (e.g. "Create Form", "Edit Form", "View Form").
/// </summary>
public class FormDefinition : BaseEntity
{
    /// <summary>The table this form belongs to.</summary>
    public Guid TableId { get; set; }
    public SysTable Table { get; set; } = null!;

    /// <summary>Internal name (e.g. "incident_default_form").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Display name shown in admin UI.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Form usage context: "create", "edit", "view", "mobile", "portal".
    /// Controls which form is loaded in each context.
    /// </summary>
    public string FormContext { get; set; } = "default";

    /// <summary>Whether this is the default form for its context + table.</summary>
    public bool IsDefault { get; set; } = true;

    /// <summary>Optional condition (JSON rule) for when to display this form.</summary>
    public string? DisplayCondition { get; set; }

    /// <summary>Tenant scope.</summary>
    public Guid? TenantId { get; set; }

    // Navigation
    public ICollection<FormSection> Sections { get; set; } = new List<FormSection>();
}
