using ServiceAxis.Domain.Common;

namespace ServiceAxis.Domain.Entities.Forms;

/// <summary>
/// A collapsible / tabbed section inside a <see cref="FormDefinition"/>.
/// Groups related fields visually (e.g. "Basic Information", "Assignment", "Resolution").
/// </summary>
public class FormSection : BaseEntity
{
    public Guid FormDefinitionId { get; set; }
    public FormDefinition FormDefinition { get; set; } = null!;

    /// <summary>Section label shown as a heading or tab.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Render order (lower = shown first).</summary>
    public int DisplayOrder { get; set; } = 10;

    /// <summary>Whether the section is collapsed by default.</summary>
    public bool IsCollapsed { get; set; } = false;

    /// <summary>Optional bootstrap columns count (1-12).</summary>
    public int Columns { get; set; } = 1;

    /// <summary>Optional condition JSON for conditional visibility.</summary>
    public string? VisibilityCondition { get; set; }

    // Navigation
    public ICollection<FormFieldMapping> FieldMappings { get; set; } = new List<FormFieldMapping>();
}
