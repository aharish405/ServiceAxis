using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAxis.Application.Features.Assignment.Commands;
using ServiceAxis.Application.Features.Assignment.Queries;

namespace ServiceAxis.API.Controllers;

/// <summary>
/// Assignment Groups API — manage agent groups, their members, and routing queues.
/// Groups determine how work items are distributed across agents.
/// </summary>
[Authorize]
public class AssignmentGroupsController : BaseApiController
{
    private readonly IMediator _mediator;

    public AssignmentGroupsController(IMediator mediator) => _mediator = mediator;

    // ─── Groups ───────────────────────────────────────────────────────────────

    /// <summary>Returns a paged list of all active assignment groups.</summary>
    [HttpGet]
    [Authorize(Policy = "AgentUp")]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new ListAssignmentGroupsQuery(page, pageSize), ct);
        return Ok(result);
    }

    /// <summary>Returns a group's full detail including all members, roles, and workloads.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "AgentUp")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAssignmentGroupQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Creates a new assignment group.</summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create(
        [FromBody] CreateGroupRequest request,
        CancellationToken ct)
    {
        var id = await _mediator.Send(new CreateAssignmentGroupCommand(
            request.Name, request.Description, request.Email,
            request.DefaultStrategy, request.MaxConcurrentPerMember,
            request.TenantId), ct);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    /// <summary>Updates an assignment group's name, strategy, and capacity settings.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateGroupRequest request,
        CancellationToken ct)
    {
        await _mediator.Send(new UpdateAssignmentGroupCommand(
            id, request.Name, request.Description, request.Email,
            request.DefaultStrategy, request.MaxConcurrentPerMember), ct);
        return NoContent();
    }

    // ─── Members ──────────────────────────────────────────────────────────────

    /// <summary>Adds a user to an assignment group with a specified role.</summary>
    [HttpPost("{id:guid}/members")]
    [Authorize(Policy = "ManagerUp")]
    public async Task<IActionResult> AddMember(
        Guid id,
        [FromBody] AddMemberRequest request,
        CancellationToken ct)
    {
        var memberId = await _mediator.Send(
            new AddGroupMemberCommand(id, request.UserId, request.Role), ct);
        return Ok(new { memberId });
    }

    /// <summary>Removes (soft-deletes) a member from an assignment group.</summary>
    [HttpDelete("{id:guid}/members/{memberId:guid}")]
    [Authorize(Policy = "ManagerUp")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid memberId, CancellationToken ct)
    {
        await _mediator.Send(new RemoveGroupMemberCommand(memberId), ct);
        return NoContent();
    }
}

// ─── Request DTOs ─────────────────────────────────────────────────────────────

public record CreateGroupRequest(
    string Name,
    string? Description,
    string? Email,
    string DefaultStrategy,
    int MaxConcurrentPerMember,
    Guid? TenantId);

public record UpdateGroupRequest(
    string Name,
    string? Description,
    string? Email,
    string DefaultStrategy,
    int MaxConcurrentPerMember);

public record AddMemberRequest(string UserId, string Role);
