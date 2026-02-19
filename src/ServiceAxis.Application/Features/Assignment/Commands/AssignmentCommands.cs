using MediatR;
using ServiceAxis.Application.Contracts.Infrastructure;

namespace ServiceAxis.Application.Features.Assignment.Commands;

// ─── Request to assign record to a specific user ───────────────────────────

public record AssignToUserCommand(string TableName, Guid RecordId, string UserId)
    : IRequest<bool>;

public class AssignToUserHandler : IRequestHandler<AssignToUserCommand, bool>
{
    private readonly IAssignmentService _assign;
    public AssignToUserHandler(IAssignmentService assign) => _assign = assign;

    public async Task<bool> Handle(AssignToUserCommand req, CancellationToken ct)
    {
        await _assign.AssignAsync(req.RecordId, req.UserId, null, ct);
        return true;
    }
}

// ─── Request to assign record to a specific group ──────────────────────────

public record AssignToGroupCommand(string TableName, Guid RecordId, Guid GroupId)
    : IRequest<bool>;

public class AssignToGroupHandler : IRequestHandler<AssignToGroupCommand, bool>
{
    private readonly IAssignmentService _assign;
    public AssignToGroupHandler(IAssignmentService assign) => _assign = assign;

    public async Task<bool> Handle(AssignToGroupCommand req, CancellationToken ct)
    {
        await _assign.AssignAsync(req.RecordId, null, req.GroupId, ct);
        return true;
    }
}
