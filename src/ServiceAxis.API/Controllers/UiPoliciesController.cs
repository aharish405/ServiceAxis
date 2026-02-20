using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAxis.Application.Features.Forms.Commands;
using ServiceAxis.Domain.Enums;
using System.Text.Json;

namespace ServiceAxis.API.Controllers;

[Authorize(Roles = "SuperAdmin,Admin")]
[ApiController]
[Route("api/v1/ui-policies")]
public class UiPoliciesController : BaseApiController
{
    private readonly ISender _sender;

    public UiPoliciesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePolicy([FromBody] CreateUiPolicyRequest request, CancellationToken ct)
    {
        var command = new CreateUiPolicyCommand(
            request.TableId,
            request.Name,
            request.Description,
            request.ExecutionOrder,
            request.IsActive,
            request.FormContext,
            request.Conditions,
            request.Actions
        );

        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(CreatePolicy), new { id }, new { id, message = "UI Policy created successfully." });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdatePolicy(Guid id, [FromBody] UpdateUiPolicyRequest request, CancellationToken ct)
    {
        var command = new UpdateUiPolicyCommand(
            id,
            request.Name,
            request.Description,
            request.ExecutionOrder,
            request.IsActive,
            request.FormContext,
            request.Version,
            request.Conditions,
            request.Actions
        );

        await _sender.Send(command, ct);
        return Ok(new { message = "UI Policy updated successfully." });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePolicy(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteUiPolicyCommand(id), ct);
        return NoContent();
    }
}

public record CreateUiPolicyRequest(
    Guid TableId,
    string Name,
    string? Description,
    int ExecutionOrder,
    bool IsActive,
    FormContextType FormContext,
    List<CreateUiPolicyConditionDto> Conditions,
    List<CreateUiPolicyActionDto> Actions);

public record UpdateUiPolicyRequest(
    string Name,
    string? Description,
    int ExecutionOrder,
    bool IsActive,
    FormContextType FormContext,
    int Version,
    List<CreateUiPolicyConditionDto> Conditions,
    List<CreateUiPolicyActionDto> Actions);
