using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAxis.Application.Features.Forms.Commands;
using ServiceAxis.Domain.Enums;
using System.Text.Json;

namespace ServiceAxis.API.Controllers;

[Authorize(Roles = "SuperAdmin,Admin")]
[ApiController]
[Route("api/v1/client-scripts")]
public class ClientScriptsController : BaseApiController
{
    private readonly ISender _sender;

    public ClientScriptsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> CreateClientScript([FromBody] CreateClientScriptRequest request, CancellationToken ct)
    {
        var command = new CreateClientScriptCommand(
            request.TableId,
            request.Name,
            request.Description,
            request.EventType,
            request.TriggerFieldId,
            request.ScriptCode,
            request.ExecutionOrder,
            request.IsActive
        );

        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(CreateClientScript), new { id }, new { id, message = "Client Script created successfully." });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateClientScript(Guid id, [FromBody] UpdateClientScriptRequest request, CancellationToken ct)
    {
        var command = new UpdateClientScriptCommand(
            id,
            request.Name,
            request.Description,
            request.EventType,
            request.TriggerFieldId,
            request.ScriptCode,
            request.ExecutionOrder,
            request.IsActive,
            request.Version
        );

        await _sender.Send(command, ct);
        return Ok(new { message = "Client Script updated successfully." });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteClientScript(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteClientScriptCommand(id), ct);
        return NoContent();
    }
}

public record CreateClientScriptRequest(
    Guid TableId,
    string Name,
    string? Description,
    ClientScriptEventType EventType,
    Guid? TriggerFieldId,
    string ScriptCode,
    int ExecutionOrder,
    bool IsActive);

public record UpdateClientScriptRequest(
    string Name,
    string? Description,
    ClientScriptEventType EventType,
    Guid? TriggerFieldId,
    string ScriptCode,
    int ExecutionOrder,
    bool IsActive,
    int Version);
