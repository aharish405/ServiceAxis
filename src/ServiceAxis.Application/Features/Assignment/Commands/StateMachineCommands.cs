using MediatR;
using ServiceAxis.Application.Contracts.Infrastructure;

namespace ServiceAxis.Application.Features.Assignment.Commands;

// ─── Execute a state transition ────────────────────────────────────────────────

public record ChangeStateCommand(Guid RecordId, Guid TargetStateId, IEnumerable<string> CallerRoles)
    : IRequest<StateChangeResult>;

public class ChangeStateHandler : IRequestHandler<ChangeStateCommand, StateChangeResult>
{
    private readonly IStateMachineService _sm;
    public ChangeStateHandler(IStateMachineService sm) => _sm = sm;

    public Task<StateChangeResult> Handle(ChangeStateCommand req, CancellationToken ct)
        => _sm.ChangeStateAsync(req.RecordId, req.TargetStateId, req.CallerRoles, ct);
}
