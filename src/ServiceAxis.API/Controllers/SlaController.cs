using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAxis.Application.Features.Sla.Commands;
using ServiceAxis.Application.Features.Sla.Queries;

namespace ServiceAxis.API.Controllers;

/// <summary>
/// SLA Management API — define and manage Service Level Agreements and priority-tier policies.
/// Read operations require AgentUp. Write operations require AdminOnly.
/// </summary>
[Authorize]
public class SlaController : BaseApiController
{
    private readonly IMediator _mediator;

    public SlaController(IMediator mediator) => _mediator = mediator;

    // ─── Definitions ─────────────────────────────────────────────────────────

    /// <summary>Returns a paged list of SLA definitions, optionally filtered by table.</summary>
    [HttpGet]
    [Authorize(Policy = "AgentUp")]
    public async Task<IActionResult> List(
        [FromQuery] string? tableName = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new ListSlaDefinitionsQuery(tableName, page, pageSize), ct);
        return Ok(result);
    }

    /// <summary>Returns a single SLA definition with all its priority-tier policies.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "AgentUp")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSlaDefinitionQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Creates a new SLA definition (no policies yet — add via POST /{id}/policies).</summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create(
        [FromBody] CreateSlaDefinitionRequest request,
        CancellationToken ct)
    {
        var id = await _mediator.Send(new CreateSlaDefinitionCommand(
            request.Name, request.Description, request.TableName,
            request.Type, request.ScheduleType,
            request.BusinessStartHour, request.BusinessEndHour,
            request.WorkingDaysJson, request.TenantId), ct);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    /// <summary>Updates SLA definition metadata (name, description, hours, active flag).</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateSlaDefinitionRequest request,
        CancellationToken ct)
    {
        await _mediator.Send(new UpdateSlaDefinitionCommand(
            id, request.Name, request.Description,
            request.BusinessStartHour, request.BusinessEndHour, request.IsActive), ct);
        return NoContent();
    }

    // ─── Policies (Priority Tiers) ────────────────────────────────────────────

    /// <summary>Adds a priority-tier policy to an SLA definition (e.g. Critical = 15 min response).</summary>
    [HttpPost("{id:guid}/policies")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> AddPolicy(
        Guid id,
        [FromBody] AddSlaPolicyRequest request,
        CancellationToken ct)
    {
        var policyId = await _mediator.Send(new AddSlaPolicyCommand(
            id, request.Priority,
            request.ResponseTimeMinutes, request.ResolutionTimeMinutes,
            request.WarningThresholdPercent, request.NotifyOnBreach,
            request.EscalateOnBreach), ct);
        return Ok(new { policyId });
    }
}

// ─── Request DTOs ─────────────────────────────────────────────────────────────

public record CreateSlaDefinitionRequest(
    string Name,
    string? Description,
    string TableName,
    string Type,
    string ScheduleType,
    int BusinessStartHour,
    int BusinessEndHour,
    string WorkingDaysJson,
    Guid? TenantId);

public record UpdateSlaDefinitionRequest(
    string Name,
    string? Description,
    int BusinessStartHour,
    int BusinessEndHour,
    bool IsActive);

public record AddSlaPolicyRequest(
    string Priority,
    int ResponseTimeMinutes,
    int ResolutionTimeMinutes,
    int WarningThresholdPercent,
    bool NotifyOnBreach,
    bool EscalateOnBreach);
