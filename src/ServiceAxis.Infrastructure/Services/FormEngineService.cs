using Microsoft.EntityFrameworkCore;
using ServiceAxis.Application.Contracts.Infrastructure;
using ServiceAxis.Domain.Entities.Forms;
using ServiceAxis.Infrastructure.Persistence;
using ServiceAxis.Shared.Exceptions;

namespace ServiceAxis.Infrastructure.Services;

public class FormEngineService : IFormEngineService
{
    private readonly ServiceAxisDbContext _db;

    public FormEngineService(ServiceAxisDbContext db) => _db = db;

    public async Task<FormSchemaDto> GetFormSchemaAsync(string tableName, string context, CancellationToken ct)
    {
        var form = await _db.FormDefinitions
            .Include(f => f.Table)
            .Include(f => f.Sections)
                .ThenInclude(s => s.FieldMappings)
                    .ThenInclude(m => m.Field)
            .Where(f => f.Table.Name == tableName && f.FormContext == context && f.IsActive)
            .OrderByDescending(f => f.IsDefault)
            .FirstOrDefaultAsync(ct);

        if (form is null)
        {
            // Fallback: try default context
            if (context != "default")
                return await GetFormSchemaAsync(tableName, "default", ct);
            
            throw new NotFoundException($"No form definition found for table '{tableName}' in context '{context}'.");
        }

        return new FormSchemaDto(
            form.Table.Name,
            form.FormContext,
            form.Sections
                .OrderBy(s => s.DisplayOrder)
                .Select(s => new FormSectionDto(
                    s.Title,
                    s.DisplayOrder,
                    s.IsCollapsed,
                    s.Columns,
                    s.FieldMappings
                        .OrderBy(m => m.DisplayOrder)
                        .Select(m => new FormFieldDto(
                            m.Field.FieldName,
                            m.LabelOverride ?? m.Field.DisplayName,
                            m.Field.DataType.ToString(),
                            m.IsRequiredOverride ?? m.Field.IsRequired,
                            m.IsReadOnlyOverride ?? m.Field.IsReadOnly,
                            m.IsHidden,
                            m.Field.DefaultValue,
                            m.Field.HelpText,
                            m.Field.ChoiceOptions,
                            m.Field.LookupTableName,
                            m.ColSpan,
                            m.DisplayOrder
                        ))
                        .ToList()
                ))
                .ToList()
        );
    }
}
