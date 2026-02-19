using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAxis.Application.Features.Notifications.Commands;
using ServiceAxis.Application.Features.Notifications.Queries;

namespace ServiceAxis.API.Controllers;

/// <summary>
/// Notification Templates API — manage the email/SMS/in-app message templates used by the platform.
/// Templates support {{variable}} substitution rendered at dispatch time.
/// </summary>
[Authorize]
public class NotificationTemplatesController : BaseApiController
{
    private readonly IMediator _mediator;

    public NotificationTemplatesController(IMediator mediator) => _mediator = mediator;

    /// <summary>Lists notification templates with optional filters.</summary>
    [HttpGet]
    [Authorize(Policy = "AgentUp")]
    public async Task<IActionResult> List(
        [FromQuery] string? tableName    = null,
        [FromQuery] string? triggerEvent = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new ListNotificationTemplatesQuery(tableName, triggerEvent, page, pageSize), ct);
        return Ok(result);
    }

    /// <summary>Returns the full content of a single notification template including body.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "AgentUp")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetNotificationTemplateQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Creates a new notification template. Code must be globally unique.</summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create(
        [FromBody] CreateNotificationTemplateRequest request,
        CancellationToken ct)
    {
        var id = await _mediator.Send(new CreateNotificationTemplateCommand(
            request.Code, request.Name, request.Description,
            request.Subject, request.Body,
            request.TriggerEvent, request.TableName,
            request.TenantId), ct);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    /// <summary>Updates a notification template. System templates cannot be modified.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateNotificationTemplateRequest request,
        CancellationToken ct)
    {
        await _mediator.Send(new UpdateNotificationTemplateCommand(
            id, request.Name, request.Description,
            request.Subject, request.Body,
            request.IsActive), ct);
        return NoContent();
    }

    /// <summary>Soft-deletes a notification template. System templates cannot be deleted.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteNotificationTemplateCommand(id), ct);
        return NoContent();
    }
}

// ─── Request DTOs ─────────────────────────────────────────────────────────────

public record CreateNotificationTemplateRequest(
    string Code,
    string Name,
    string? Description,
    string Subject,
    string Body,
    string TriggerEvent,
    string? TableName,
    Guid? TenantId);

public record UpdateNotificationTemplateRequest(
    string Name,
    string? Description,
    string Subject,
    string Body,
    bool IsActive);
