using MediatR;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Entities.Assignment;
using ServiceAxis.Domain.Enums;
using ServiceAxis.Shared.Exceptions;

namespace ServiceAxis.Application.Features.Assignment.Queries;

// ─── List Assignment Groups ───────────────────────────────────────────────────

public record ListAssignmentGroupsQuery(int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<AssignmentGroupDto>>;

public record AssignmentGroupDto(
    Guid Id,
    string Name,
    string? Description,
    string? Email,
    string DefaultStrategy,
    int MaxConcurrentPerMember,
    int MemberCount,
    bool IsActive,
    DateTime CreatedAt);

public class ListAssignmentGroupsHandler
    : IRequestHandler<ListAssignmentGroupsQuery, PagedResult<AssignmentGroupDto>>
{
    private readonly IUnitOfWork _uow;

    public ListAssignmentGroupsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<AssignmentGroupDto>> Handle(ListAssignmentGroupsQuery q, CancellationToken ct)
    {
        var paged = await _uow.Repository<AssignmentGroup>()
            .GetPagedAsync(q.Page, q.PageSize, g => g.IsActive, ct);

        var dtos = new List<AssignmentGroupDto>();
        foreach (var g in paged.Items)
        {
            var memberCount = (await _uow.Repository<GroupMember>()
                .FindAsync(m => m.GroupId == g.Id && m.IsActive, ct)).Count;
            dtos.Add(new AssignmentGroupDto(g.Id, g.Name, g.Description, g.Email,
                g.DefaultStrategy.ToString(), g.MaxConcurrentPerMember, memberCount, g.IsActive, g.CreatedAt));
        }

        return new PagedResult<AssignmentGroupDto>
        {
            Items      = dtos,
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize   = paged.PageSize
        };
    }
}

// ─── Get Group Detail ─────────────────────────────────────────────────────────

public record GetAssignmentGroupQuery(Guid Id) : IRequest<AssignmentGroupDetailDto?>;

public record AssignmentGroupDetailDto(
    Guid Id,
    string Name,
    string? Description,
    string? Email,
    string DefaultStrategy,
    int MaxConcurrentPerMember,
    IReadOnlyList<GroupMemberDto> Members);

public record GroupMemberDto(
    Guid Id,
    string UserId,
    string Role,
    bool IsAvailable,
    int ActiveItemCount);

public class GetAssignmentGroupHandler
    : IRequestHandler<GetAssignmentGroupQuery, AssignmentGroupDetailDto?>
{
    private readonly IUnitOfWork _uow;

    public GetAssignmentGroupHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<AssignmentGroupDetailDto?> Handle(GetAssignmentGroupQuery q, CancellationToken ct)
    {
        var group = await _uow.Repository<AssignmentGroup>().GetByIdAsync(q.Id, ct);
        if (group is null) return null;

        var members = await _uow.Repository<GroupMember>()
            .FindAsync(m => m.GroupId == q.Id && m.IsActive, ct);

        return new AssignmentGroupDetailDto(
            group.Id, group.Name, group.Description, group.Email,
            group.DefaultStrategy.ToString(), group.MaxConcurrentPerMember,
            members.Select(m => new GroupMemberDto(
                m.Id, m.UserId, m.Role.ToString(),
                m.IsAvailable, m.ActiveItemCount)).ToList());
    }
}
