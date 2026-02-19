using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAxis.Application.Features.Workflow.Commands;
using ServiceAxis.Application.Features.Workflow.Queries;

namespace ServiceAxis.API.Controllers;

/// <summary>
/// Workflow Definitions API — manage the blueprints for all platform workflows.
/// Requires at least the Manager role to write; Viewer role to read.
/// </summary>
[Authorize]
public class WorkflowDefinitionsController : BaseApiController
{
    private readonly IMediator _mediator;

    public WorkflowDefinitionsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Returns a paged list of workflow definitions.
    /// </summary>
    /// <param name="pageNumber">Page number (default 1).</param>
    /// <param name="pageSize">Items per page (default 10).</param>
    /// <param name="category">Optional category filter.</param>
    [HttpGet]
    [Authorize(Policy = "AgentUp")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize   = 10,
        [FromQuery] string? category = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetWorkflowDefinitionsQuery(pageNumber, pageSize, category), ct);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new workflow definition.
    /// </summary>
    /// <param name="request">Workflow definition data.</param>
    [HttpPost]
    [Authorize(Policy = "ManagerUp")]
    [ProducesResponseType(typeof(CreateWorkflowDefinitionResult), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Create(
        [FromBody] CreateWorkflowDefinitionRequest request,
        CancellationToken ct)
    {
        var command = new CreateWorkflowDefinitionCommand(
            request.Code,
            request.Name,
            request.Description,
            request.Category,
            request.TenantId);

        var result = await _mediator.Send(command, ct);
        return Created(result, "Workflow definition created.");
    }
}

/// <summary>Request DTO for creating a workflow definition.</summary>
public record CreateWorkflowDefinitionRequest(
    string Code,
    string Name,
    string? Description,
    string Category,
    Guid? TenantId);
