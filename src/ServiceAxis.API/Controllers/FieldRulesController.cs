using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAxis.Application.Features.Forms.Commands;
using ServiceAxis.Domain.Enums;
using System.Text.Json;

namespace ServiceAxis.API.Controllers;

[Authorize(Roles = "SuperAdmin,Admin")]
[ApiController]
[Route("api/v1/field-rules")]
public class FieldRulesController : BaseApiController
{
    private readonly ISender _sender;

    public FieldRulesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> CreateFieldRule([FromBody] CreateFieldRuleRequest request, CancellationToken ct)
    {
        var command = new CreateFieldRuleCommand(
            request.TableId,
            request.Name,
            request.TriggerFieldId,
            request.Condition,
            request.TargetFieldId,
            request.ActionType,
            request.ValueExpression,
            request.ExecutionOrder,
            request.IsActive
        );

        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(CreateFieldRule), new { id }, new { id, message = "Field Rule created successfully." });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateFieldRule(Guid id, [FromBody] UpdateFieldRuleRequest request, CancellationToken ct)
    {
        var command = new UpdateFieldRuleCommand(
            id,
            request.Name,
            request.TriggerFieldId,
            request.Condition,
            request.TargetFieldId,
            request.ActionType,
            request.ValueExpression,
            request.ExecutionOrder,
            request.IsActive,
            request.Version
        );

        await _sender.Send(command, ct);
        return Ok(new { message = "Field Rule updated successfully." });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteFieldRule(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteFieldRuleCommand(id), ct);
        return NoContent();
    }
}

public record CreateFieldRuleRequest(
    Guid TableId,
    string Name,
    Guid? TriggerFieldId,
    JsonDocument Condition,
    Guid TargetFieldId,
    FieldRuleActionType ActionType,
    string ValueExpression,
    int ExecutionOrder,
    bool IsActive);

public record UpdateFieldRuleRequest(
    string Name,
    Guid? TriggerFieldId,
    JsonDocument Condition,
    Guid TargetFieldId,
    FieldRuleActionType ActionType,
    string ValueExpression,
    int ExecutionOrder,
    bool IsActive,
    int Version);
