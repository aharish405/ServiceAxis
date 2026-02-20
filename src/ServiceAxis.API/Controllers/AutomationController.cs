using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAxis.Application.Features.Automation;

namespace ServiceAxis.API.Controllers;

[Authorize(Roles = "SuperAdmin,Admin")] // Platform automation requires admin privileges
[ApiController]
[Route("api/v1/automation/rules")]
public class AutomationController : BaseApiController
{
    private readonly ISender _sender;

    public AutomationController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetRules([FromQuery] Guid? tableId, [FromQuery] bool? isActive, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetAutomationRulesQuery(tableId, isActive, page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRuleById(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetAutomationRuleByIdQuery(id), ct);
        return result is null ? NotFound($"Rule '{id}' not found.") : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRule([FromBody] CreateAutomationRuleCommand command, CancellationToken ct)
    {
        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetRuleById), new { id }, new { id, message = "Rule created successfully." });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateRule(Guid id, [FromBody] UpdateAutomationRuleRequest request, CancellationToken ct)
    {
        var command = new UpdateAutomationRuleCommand(
            id,
            request.Name,
            request.Description,
            request.ExecutionMode,
            request.StopProcessingOnMatch,
            request.IsActive,
            request.Triggers,
            request.Conditions,
            request.Actions);

        await _sender.Send(command, ct);
        return Ok(new { message = "Rule updated successfully." });
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> ActivateRule(Guid id, [FromBody] ActivateRuleRequest request, CancellationToken ct)
    {
        await _sender.Send(new ActivateAutomationRuleCommand(id, request.IsActive), ct);
        return Ok(new { message = request.IsActive ? "Rule activated." : "Rule deactivated." });
    }
}

public record UpdateAutomationRuleRequest(
    string Name,
    string? Description,
    Domain.Enums.AutomationExecutionMode ExecutionMode,
    bool StopProcessingOnMatch,
    bool IsActive,
    List<CreateAutomationTriggerDto> Triggers,
    List<CreateAutomationConditionDto> Conditions,
    List<CreateAutomationActionDto> Actions);

public record ActivateRuleRequest(bool IsActive);
