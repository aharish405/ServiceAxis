using MediatR;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Entities.Sla;
using ServiceAxis.Domain.Enums;
using ServiceAxis.Shared.Exceptions;

namespace ServiceAxis.Application.Features.Sla.Queries;

// ─── List SLA Definitions ─────────────────────────────────────────────────────

public record ListSlaDefinitionsQuery(string? TableName = null, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<SlaDefinitionDto>>;

public record SlaDefinitionDto(
    Guid Id,
    string Name,
    string? Description,
    string TableName,
    string Type,
    string ScheduleType,
    int BusinessStartHour,
    int BusinessEndHour,
    bool IsActive,
    DateTime CreatedAt,
    IReadOnlyList<SlaPolicyDto> Policies);

public record SlaPolicyDto(
    Guid Id,
    string Priority,
    int ResponseTimeMinutes,
    int ResolutionTimeMinutes,
    int WarningThresholdPercent,
    bool NotifyOnBreach,
    bool EscalateOnBreach);

public class ListSlaDefinitionsHandler : IRequestHandler<ListSlaDefinitionsQuery, PagedResult<SlaDefinitionDto>>
{
    private readonly IUnitOfWork _uow;

    public ListSlaDefinitionsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<SlaDefinitionDto>> Handle(ListSlaDefinitionsQuery q, CancellationToken ct)
    {
        var paged = await _uow.Repository<SlaDefinition>()
            .GetPagedAsync(q.Page, q.PageSize,
                d => d.IsActive && (q.TableName == null || d.TableName == q.TableName), ct);

        var dtos = new List<SlaDefinitionDto>();
        foreach (var def in paged.Items)
        {
            var policies = (await _uow.Repository<SlaPolicy>()
                .FindAsync(p => p.SlaDefinitionId == def.Id && p.IsActive, ct))
                .Select(p => new SlaPolicyDto(p.Id, p.Priority.ToString(),
                    p.ResponseTimeMinutes, p.ResolutionTimeMinutes,
                    p.WarningThresholdPercent, p.NotifyOnBreach, p.EscalateOnBreach))
                .ToList();

            dtos.Add(new SlaDefinitionDto(def.Id, def.Name, def.Description, def.TableName,
                def.Type.ToString(), def.ScheduleType.ToString(),
                def.BusinessStartHour, def.BusinessEndHour, def.IsActive, def.CreatedAt, policies));
        }

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

        var policies = (await _uow.Repository<SlaPolicy>()
            .FindAsync(p => p.SlaDefinitionId == def.Id && p.IsActive, ct))
            .Select(p => new SlaPolicyDto(p.Id, p.Priority.ToString(),
                p.ResponseTimeMinutes, p.ResolutionTimeMinutes,
                p.WarningThresholdPercent, p.NotifyOnBreach, p.EscalateOnBreach))
            .ToList();

        return new SlaDefinitionDto(def.Id, def.Name, def.Description, def.TableName,
            def.Type.ToString(), def.ScheduleType.ToString(),
            def.BusinessStartHour, def.BusinessEndHour, def.IsActive, def.CreatedAt, policies);
    }
}
