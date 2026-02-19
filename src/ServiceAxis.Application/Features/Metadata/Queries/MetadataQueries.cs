using MediatR;
using ServiceAxis.Application.Contracts.Infrastructure;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Entities.Platform;
using ServiceAxis.Shared.Exceptions;

namespace ServiceAxis.Application.Features.Metadata.Queries;

// ─── Get Table with Fields ────────────────────────────────────────────────────

public record GetTableSchemaQuery(string TableName) : IRequest<TableSchemaDto>;

public record TableSchemaDto(
    Guid Id,
    string Name,
    string DisplayName,
    string SchemaName,
    string? AutoNumberPrefix,
    bool AuditEnabled,
    bool AllowAttachments,
    IReadOnlyList<FieldSchemaDto> Fields);

public record FieldSchemaDto(
    Guid Id,
    string FieldName,
    string DisplayName,
    string DataType,
    bool IsRequired,
    bool IsSearchable,
    bool IsReadOnly,
    string? DefaultValue,
    string? ChoiceOptions,
    string? LookupTableName,
    int DisplayOrder,
    string? HelpText);

public class GetTableSchemaHandler : IRequestHandler<GetTableSchemaQuery, TableSchemaDto>
{
    private readonly IMetadataCache _cache;

    public GetTableSchemaHandler(IMetadataCache cache) => _cache = cache;

    public async Task<TableSchemaDto> Handle(GetTableSchemaQuery q, CancellationToken ct)
    {
        var table = await _cache.GetTableAsync(q.TableName, ct)
            ?? throw new NotFoundException($"Table '{q.TableName}' not found.");

        return new TableSchemaDto(
            table.Id,
            table.Name,
            table.DisplayName,
            table.SchemaName,
            table.AutoNumberPrefix,
            table.AuditEnabled,
            table.AllowAttachments,
            table.Fields
                .Where(f => f.IsActive)
                .OrderBy(f => f.DisplayOrder)
                .Select(f => new FieldSchemaDto(
                    f.Id,
                    f.FieldName,
                    f.DisplayName,
                    f.DataType.ToString(),
                    f.IsRequired,
                    f.IsSearchable,
                    f.IsReadOnly,
                    f.DefaultValue,
                    f.ChoiceOptions,
                    f.LookupTableName,
                    f.DisplayOrder,
                    f.HelpText))
                .ToList());
    }
}

// ─── List All Tables ──────────────────────────────────────────────────────────

public record ListSysTablesQuery(int Page = 1, int PageSize = 20) : IRequest<PagedResult<TableSummaryDto>>;

public record TableSummaryDto(Guid Id, string Name, string DisplayName, string SchemaName, int FieldCount, bool IsSystemTable);

public class ListSysTablesHandler : IRequestHandler<ListSysTablesQuery, PagedResult<TableSummaryDto>>
{
    private readonly ISysTableRepository _tables;

    public ListSysTablesHandler(ISysTableRepository tables) => _tables = tables;

    public async Task<PagedResult<TableSummaryDto>> Handle(ListSysTablesQuery q, CancellationToken ct)
    {
        var paged = await _tables.GetPagedAsync(q.Page, q.PageSize, ct);
        return new PagedResult<TableSummaryDto>
        {
            Items      = paged.Items.Select(t => new TableSummaryDto(
                t.Id, t.Name, t.DisplayName, t.SchemaName, t.Fields.Count, t.IsSystemTable)).ToList(),
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize   = paged.PageSize
        };
    }
}

// ─── Get Form Schema ──────────────────────────────────────────────────────────

public record GetFormSchemaQuery(string TableName, string Context = "default") : IRequest<FormSchemaDto>;

public class GetFormSchemaHandler : IRequestHandler<GetFormSchemaQuery, FormSchemaDto>
{
    private readonly IFormEngineService _formEngine;

    public GetFormSchemaHandler(IFormEngineService formEngine) => _formEngine = formEngine;

    public async Task<FormSchemaDto> Handle(GetFormSchemaQuery request, CancellationToken ct)
    {
        return await _formEngine.GetFormSchemaAsync(request.TableName, request.Context, ct);
    }
}
