using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAxis.Application.Contracts.Infrastructure;
using ServiceAxis.Application.Features.Assignment.Commands;
using ServiceAxis.Application.Features.Assignment.Queries;

namespace ServiceAxis.API.Controllers;

/// <summary>
/// Manages record lifecycle states and transitions.
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/records")]
public class LifecycleController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IStateMachineService _sm;

    public LifecycleController(ISender sender, IStateMachineService sm)
    {
        _sender = sender;
        _sm = sm;
    }

    /// <summary>
    /// Returns all available state transitions for a specific record, based on its
    /// current state and the caller's RBAC roles.
    /// </summary>
    [HttpGet("{table}/{id:guid}/transitions")]
    [ProducesResponseType(typeof(List<AvailableTransitionDto>), 200)]
    public async Task<IActionResult> GetTransitions(string table, Guid id)
    {
        // Get caller roles (Implementation detail: assumption that roles are proper claims)
        // If using Identity, we get roles from User.Claims
        var roles = User.FindAll("role").Select(c => c.Value).ToList();
        
        // Also include potential virtual roles or groups if needed, 
        // but for now standard Identity roles suffice.

        var result = await _sender.Send(new GetAvailableTransitionsQuery(id, roles));
        return Ok(result);
    }

    /// <summary>
    /// Executes a state transition (e.g. resolve, close, re-open).
    /// </summary>
    [HttpPost("{table}/{id:guid}/state")]
    [ProducesResponseType(typeof(StateChangeResult), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> ChangeState(
        string table,
        Guid id,
        [FromBody] ChangeStateRequest request)
    {
        var roles = User.FindAll("role").Select(c => c.Value).ToList();
        
        // request.TargetStateId is required
        var result = await _sender.Send(new ChangeStateCommand(id, request.TargetStateId, roles));
        return Ok(result);
    }

    /// <summary>
    /// Returns the full lifecycle definition (all states) for a table.
    /// Useful for building UI process flow diagrams or kanban columns.
    /// </summary>
    [HttpGet("{table}/states")]
    [ProducesResponseType(typeof(List<StateDefinitionDto>), 200)]
    public async Task<IActionResult> GetTableStates(string table)
    {
        var result = await _sender.Send(new GetTableStatesQuery(table));
        return Ok(result);
    }
}

public record ChangeStateRequest(Guid TargetStateId, string? Comment);
