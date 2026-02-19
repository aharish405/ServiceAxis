using MediatR;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Entities.Assignment;
using ServiceAxis.Domain.Enums;
using ServiceAxis.Shared.Exceptions;

namespace ServiceAxis.Application.Features.Assignment.Commands;

// ─── Create Assignment Group ──────────────────────────────────────────────────

public record CreateAssignmentGroupCommand(
    string Name,
    string? Description,
    string? Email,
    string DefaultStrategy,
    int MaxConcurrentPerMember,
    Guid? TenantId) : IRequest<Guid>;

public class CreateAssignmentGroupHandler : IRequestHandler<CreateAssignmentGroupCommand, Guid>
{
    private readonly IUnitOfWork _uow;

    public CreateAssignmentGroupHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Guid> Handle(CreateAssignmentGroupCommand cmd, CancellationToken ct)
    {
        if (!Enum.TryParse<AssignmentStrategy>(cmd.DefaultStrategy, true, out var strategy))
            throw new BusinessException($"Unknown assignment strategy '{cmd.DefaultStrategy}'.");

        var group = new AssignmentGroup
        {
            Name                    = cmd.Name,
            Description             = cmd.Description,
            Email                   = cmd.Email,
            DefaultStrategy         = strategy,
            MaxConcurrentPerMember  = cmd.MaxConcurrentPerMember,
            TenantId                = cmd.TenantId
        };

        await _uow.Repository<AssignmentGroup>().AddAsync(group, ct);
        await _uow.SaveChangesAsync(ct);
        return group.Id;
    }
}

// ─── Add Group Member ─────────────────────────────────────────────────────────

public record AddGroupMemberCommand(
    Guid GroupId,
    string UserId,
    string Role) : IRequest<Guid>;

public class AddGroupMemberHandler : IRequestHandler<AddGroupMemberCommand, Guid>
{
    private readonly IUnitOfWork _uow;

    public AddGroupMemberHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Guid> Handle(AddGroupMemberCommand cmd, CancellationToken ct)
    {
        var group = await _uow.Repository<AssignmentGroup>().GetByIdAsync(cmd.GroupId, ct)
            ?? throw new NotFoundException("AssignmentGroup", cmd.GroupId);

        // Prevent duplicate membership
        var existing = await _uow.Repository<GroupMember>()
            .ExistsAsync(m => m.GroupId == cmd.GroupId && m.UserId == cmd.UserId && m.IsActive, ct);
        if (existing)
            throw new ConflictException($"User '{cmd.UserId}' is already a member of this group.");

        if (!Enum.TryParse<GroupMemberRole>(cmd.Role, true, out var role))
            throw new BusinessException($"Unknown member role '{cmd.Role}'.");

        var member = new GroupMember
        {
            GroupId = cmd.GroupId,
            UserId  = cmd.UserId,
            Role    = role
        };

        await _uow.Repository<GroupMember>().AddAsync(member, ct);
        await _uow.SaveChangesAsync(ct);
        return member.Id;
    }
}

// ─── Remove Group Member ──────────────────────────────────────────────────────

public record RemoveGroupMemberCommand(Guid MemberId) : IRequest;

public class RemoveGroupMemberHandler : IRequestHandler<RemoveGroupMemberCommand>
{
    private readonly IUnitOfWork _uow;

    public RemoveGroupMemberHandler(IUnitOfWork uow) => _uow = uow;

    public async Task Handle(RemoveGroupMemberCommand cmd, CancellationToken ct)
    {
        var member = await _uow.Repository<GroupMember>().GetByIdAsync(cmd.MemberId, ct)
            ?? throw new NotFoundException("GroupMember", cmd.MemberId);

        _uow.Repository<GroupMember>().SoftDelete(member);
        await _uow.SaveChangesAsync(ct);
    }
}

// ─── Update Group ─────────────────────────────────────────────────────────────

public record UpdateAssignmentGroupCommand(
    Guid Id,
    string Name,
    string? Description,
    string? Email,
    string DefaultStrategy,
    int MaxConcurrentPerMember) : IRequest;

public class UpdateAssignmentGroupHandler : IRequestHandler<UpdateAssignmentGroupCommand>
{
    private readonly IUnitOfWork _uow;

    public UpdateAssignmentGroupHandler(IUnitOfWork uow) => _uow = uow;

    public async Task Handle(UpdateAssignmentGroupCommand cmd, CancellationToken ct)
    {
        var group = await _uow.Repository<AssignmentGroup>().GetByIdAsync(cmd.Id, ct)
            ?? throw new NotFoundException("AssignmentGroup", cmd.Id);

        if (!Enum.TryParse<AssignmentStrategy>(cmd.DefaultStrategy, true, out var strategy))
            throw new BusinessException($"Unknown assignment strategy '{cmd.DefaultStrategy}'.");

        group.Name                   = cmd.Name;
        group.Description            = cmd.Description;
        group.Email                  = cmd.Email;
        group.DefaultStrategy        = strategy;
        group.MaxConcurrentPerMember = cmd.MaxConcurrentPerMember;
        group.UpdatedAt              = DateTime.UtcNow;

        _uow.Repository<AssignmentGroup>().Update(group);
        await _uow.SaveChangesAsync(ct);
    }
}
