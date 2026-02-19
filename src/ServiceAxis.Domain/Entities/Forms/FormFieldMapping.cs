using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Entities.Platform;

namespace ServiceAxis.Domain.Entities.Forms;

/// <summary>
/// Binds a <see cref="SysField"/> into a <see cref="FormSection"/> with
/// section-specific display overrides (read-only, hidden, mandatory override, etc.).
/// </summary>
public class FormFieldMapping : BaseEntity
{
    public Guid FormSectionId { get; set; }
    public FormSection FormSection { get; set; } = null!;

    public Guid FieldId { get; set; }
    public SysField Field { get; set; } = null!;

    /// <summary>Override display order within the section.</summary>
    public int DisplayOrder { get; set; } = 10;

    /// <summary>Override the field's global read-only flag for this form.</summary>
    public bool? IsReadOnlyOverride { get; set; }

    /// <summary>Override the mandatory flag for this form context.</summary>
    public bool? IsRequiredOverride { get; set; }

    /// <summary>Hide this field in this form (visible in list but not form).</summary>
    public bool IsHidden { get; set; } = false;

    /// <summary>Label override shown on this specific form.</summary>
    public string? LabelOverride { get; set; }

    /// <summary>
    /// Column span in a multi-column section (1 = half width, 2 = full width for 2-col section).
    /// </summary>
    public int ColSpan { get; set; } = 1;

    /// <summary>Optional condition JSON for conditional visibility (field-level).</summary>
    public string? VisibilityCondition { get; set; }
}
