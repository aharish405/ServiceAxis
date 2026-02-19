using ServiceAxis.Domain.Entities.Platform;
using ServiceAxis.Domain.Enums;

namespace ServiceAxis.Application.Contracts.Infrastructure;

/// <summary>
/// Application-level service for dynamic form schema retrieval.
/// Returns the JSON form schema consumed by frontend auto-rendering.
/// </summary>
public interface IFormEngineService
{
    /// <summary>
    /// Returns the form schema for a table and optional context (create/edit/view).
    /// </summary>
    Task<FormSchemaDto> GetFormSchemaAsync(
        string tableName,
        string context = "default",
        CancellationToken ct = default);
}

/// <summary>Form schema DTO — consumed by frontend to auto-render forms.</summary>
public record FormSchemaDto(
    string TableName,
    string FormContext,
    IReadOnlyList<FormSectionDto> Sections);

public record FormSectionDto(
    string Title,
    int DisplayOrder,
    bool IsCollapsed,
    int Columns,
    IReadOnlyList<FormFieldDto> Fields);

public record FormFieldDto(
    string FieldName,
    string DisplayName,
    string DataType,
    bool IsRequired,
    bool IsReadOnly,
    bool IsHidden,
    string? DefaultValue,
    string? HelpText,
    string? ChoiceOptions,
    string? LookupTableName,
    int ColSpan,
    int DisplayOrder);
