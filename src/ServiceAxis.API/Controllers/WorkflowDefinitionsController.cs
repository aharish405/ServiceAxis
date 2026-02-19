using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAxis.Application.Features.Workflow.Commands;
using ServiceAxis.Application.Features.Workflow.Queries;

namespace ServiceAxis.API.Controllers;

/// <summary>
/// Workflow Definitions API — manage blueprints, steps, transitions, and running instances.
/// Read operations require AgentUp policy. Write operations require ManagerUp policy.
/// </summary>
[Authorize]
public class WorkflowDefinitionsController : BaseApiController
{
    private readonly IMediator _mediator;

    public WorkflowDefinitionsController(IMediator mediator) => _mediator = mediator;

    // ─── Definitions ─────────────────────────────────────────────────────────

    /// <summary>Returns a paged list of workflow definitions.</summary>
    [HttpGet]
    [Authorize(Policy = "AgentUp")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize   = 10,
        [FromQuery] string? category = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetWorkflowDefinitionsQuery(pageNumber, pageSize, category), ct);
        return Ok(result);
    }

    /// <summary>Returns a single workflow definition with its full step and transition graph.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "AgentUp")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetWorkflowDefinitionQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Creates a new workflow definition (draft, not published).</summary>
    [HttpPost]
    [Authorize(Policy = "ManagerUp")]
    [ProducesResponseType(typeof(CreateWorkflowDefinitionResult), 201)]
    public async Task<IActionResult> Create(
        [FromBody] CreateWorkflowDefinitionRequest request,
        CancellationToken ct)
    {
        var command = new CreateWorkflowDefinitionCommand(
            request.Code, request.Name, request.Description,
            request.Category, request.TenantId);
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // ─── Steps ───────────────────────────────────────────────────────────────

    /// <summary>Lists all steps for a workflow definition.</summary>
    [HttpGet("{id:guid}/steps")]
    [Authorize(Policy = "AgentUp")]
    public async Task<IActionResult> GetSteps(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetWorkflowStepsQuery(id), ct);
        return Ok(result);
    }

    /// <summary>Adds a new step to an existing workflow definition.</summary>
    [HttpPost("{id:guid}/steps")]
    [Authorize(Policy = "ManagerUp")]
    public async Task<IActionResult> AddStep(
        Guid id,
        [FromBody] AddStepRequest request,
        CancellationToken ct)
    {
        var command = new AddWorkflowStepCommand(
            id, request.Code, request.Name, request.StepType,
            request.Order, request.IsInitial, request.IsTerminal,
            request.RequiredRole, request.Configuration);
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    /// <summary>Updates an existing workflow step.</summary>
    [HttpPut("{id:guid}/steps/{stepId:guid}")]
    [Authorize(Policy = "ManagerUp")]
    public async Task<IActionResult> UpdateStep(
        Guid id,
        Guid stepId,
        [FromBody] UpdateStepRequest request,
        CancellationToken ct)
    {
        var command = new UpdateWorkflowStepCommand(
            stepId, request.Name, request.StepType, request.Order,
            request.IsTerminal, request.RequiredRole, request.Configuration);
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    /// <summary>Soft-deletes a workflow step.</summary>
    [HttpDelete("{id:guid}/steps/{stepId:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteStep(Guid id, Guid stepId, CancellationToken ct)
    {
        await _mediator.Send(new DeleteWorkflowStepCommand(stepId), ct);
        return NoContent();
    }

    // ─── Transitions ──────────────────────────────────────────────────────────

    /// <summary>Lists all transitions for a workflow definition.</summary>
    [HttpGet("{id:guid}/transitions")]
    [Authorize(Policy = "AgentUp")]
    public async Task<IActionResult> GetTransitions(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetWorkflowTransitionsQuery(id), ct);
        return Ok(result);
    }

    /// <summary>Adds a directed transition between two steps.</summary>
    [HttpPost("{id:guid}/transitions")]
    [Authorize(Policy = "ManagerUp")]
    public async Task<IActionResult> AddTransition(
        Guid id,
        [FromBody] AddTransitionRequest request,
        CancellationToken ct)
    {
        var command = new AddWorkflowTransitionCommand(
            id, request.FromStepId, request.ToStepId,
            request.TriggerEvent, request.Condition, request.Priority);
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    /// <summary>Removes a workflow transition.</summary>
    [HttpDelete("{id:guid}/transitions/{transitionId:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteTransition(Guid id, Guid transitionId, CancellationToken ct)
    {
        await _mediator.Send(new DeleteWorkflowTransitionCommand(transitionId), ct);
        return NoContent();
    }

    // ─── Instances ────────────────────────────────────────────────────────────

    /// <summary>Lists all running and historical workflow instances for a definition.</summary>
    [HttpGet("{id:guid}/instances")]
    [Authorize(Policy = "AgentUp")]
    public async Task<IActionResult> GetInstances(
        Guid id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize   = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetWorkflowInstancesQuery(id, pageNumber, pageSize), ct);
        return Ok(result);
    }
}

// ─── Request DTOs ────────────────────────────────────────────────────────────

public record CreateWorkflowDefinitionRequest(
    string Code,
    string Name,
    string? Description,
    string Category,
    Guid? TenantId);


public record AddStepRequest(
    string Code,
    string Name,
    string StepType,
    int Order,
    bool IsInitial,
    bool IsTerminal,
    string? RequiredRole,
    string? Configuration);

public record UpdateStepRequest(
    string Name,
    string StepType,
    int Order,
    bool IsTerminal,
    string? RequiredRole,
    string? Configuration);

public record AddTransitionRequest(
    Guid FromStepId,
    Guid ToStepId,
    string TriggerEvent,
    string? Condition,
    int Priority);
