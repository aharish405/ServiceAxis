using MediatR;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Entities.Sla;
using ServiceAxis.Domain.Entities.Platform;
using ServiceAxis.Shared.Exceptions;

namespace ServiceAxis.Application.Features.Sla.Queries;

// ─── List SLA Definitions ─────────────────────────────────────────────────────

public record ListSlaDefinitionsQuery(string? TableName = null, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<SlaDefinitionDto>>;

public record SlaDefinitionDto(
    Guid Id,
    string Name,
    string? Description,
    Guid TableId,
    bool IsActive,
    DateTime CreatedAt);

public class ListSlaDefinitionsHandler : IRequestHandler<ListSlaDefinitionsQuery, PagedResult<SlaDefinitionDto>>
{
    private readonly IUnitOfWork _uow;

    public ListSlaDefinitionsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<SlaDefinitionDto>> Handle(ListSlaDefinitionsQuery q, CancellationToken ct)
    {
        Guid? tableId = null;
        if (!string.IsNullOrEmpty(q.TableName))
        {
            var tables = await _uow.Repository<SysTable>().FindAsync(t => t.Name == q.TableName, ct);
            var table = tables.FirstOrDefault();
            if (table != null) tableId = table.Id;
            else return new PagedResult<SlaDefinitionDto> { Items = [], TotalCount = 0, PageNumber = q.Page, PageSize = q.PageSize };
        }

        var paged = await _uow.Repository<SlaDefinition>()
            .GetPagedAsync(q.Page, q.PageSize,
                d => d.IsActive && (!tableId.HasValue || d.TableId == tableId.Value), ct);

        var dtos = paged.Items.Select(def => new SlaDefinitionDto(
            def.Id, def.Name, def.Description, def.TableId, def.IsActive, def.CreatedAt))
            .ToList();

        return new PagedResult<SlaDefinitionDto>
        {
            Items      = dtos,
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize   = paged.PageSize
        };
    }
}

// ─── Get SLA Definition by Id ─────────────────────────────────────────────────

public record GetSlaDefinitionQuery(Guid Id) : IRequest<SlaDefinitionDto?>;

public class GetSlaDefinitionHandler : IRequestHandler<GetSlaDefinitionQuery, SlaDefinitionDto?>
{
    private readonly IUnitOfWork _uow;

    public GetSlaDefinitionHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<SlaDefinitionDto?> Handle(GetSlaDefinitionQuery q, CancellationToken ct)
    {
        var def = await _uow.Repository<SlaDefinition>().GetByIdAsync(q.Id, ct);
        if (def is null) return null;

        return new SlaDefinitionDto(
            def.Id, def.Name, def.Description, def.TableId, def.IsActive, def.CreatedAt);
    }
}
