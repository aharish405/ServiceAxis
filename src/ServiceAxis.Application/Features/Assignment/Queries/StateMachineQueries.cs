using MediatR;
using ServiceAxis.Application.Contracts.Infrastructure;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Shared.Exceptions;

namespace ServiceAxis.Application.Features.Assignment.Queries;

// ─── Get available state transitions for a record ─────────────────────────────

public record GetAvailableTransitionsQuery(Guid RecordId, IEnumerable<string> CallerRoles)
    : IRequest<IReadOnlyList<AvailableTransitionDto>>;

public class GetAvailableTransitionsHandler
    : IRequestHandler<GetAvailableTransitionsQuery, IReadOnlyList<AvailableTransitionDto>>
{
    private readonly IStateMachineService _sm;
    public GetAvailableTransitionsHandler(IStateMachineService sm) => _sm = sm;

    public Task<IReadOnlyList<AvailableTransitionDto>> Handle(
        GetAvailableTransitionsQuery req,
        CancellationToken ct)
        => _sm.GetAvailableTransitionsAsync(req.RecordId, req.CallerRoles, ct);
}

// ─── Get all states defined for a table ───────────────────────────────────────

public record GetTableStatesQuery(string TableName)
    : IRequest<IReadOnlyList<StateDefinitionDto>>;

public class GetTableStatesHandler : IRequestHandler<GetTableStatesQuery, IReadOnlyList<StateDefinitionDto>>
{
    private readonly IStateMachineService _sm;
    private readonly IMetadataCache       _cache;

    public GetTableStatesHandler(IStateMachineService sm, IMetadataCache cache)
    {
        _sm    = sm;
        _cache = cache;
    }

    public async Task<IReadOnlyList<StateDefinitionDto>> Handle(GetTableStatesQuery req, CancellationToken ct)
    {
        var table = await _cache.GetTableAsync(req.TableName, ct)
            ?? throw new NotFoundException("Table", req.TableName);

        return await _sm.GetStatesForTableAsync(table.Id, ct);
    }
}
