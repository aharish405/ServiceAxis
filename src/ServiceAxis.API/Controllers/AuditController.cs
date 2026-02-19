using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAxis.Application.Features.Audit.Queries;

namespace ServiceAxis.API.Controllers;

/// <summary>
/// Audit Log API — immutable, compliance-ready event trail for every significant platform action.
/// Read-only; write access is via the internal audit interceptor only.
/// </summary>
[Authorize]
public class AuditController : BaseApiController
{
    private readonly IMediator _mediator;

    public AuditController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Returns a paged list of audit log entries with optional filtering.
    /// Supports date range, module, entity type/ID, and action-type filters.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "ManagerUp")]
    public async Task<IActionResult> List(
        [FromQuery] string?   module     = null,
        [FromQuery] string?   entityType = null,
        [FromQuery] string?   entityId   = null,
        [FromQuery] string?   action     = null,
        [FromQuery] DateTime? from       = null,
        [FromQuery] DateTime? to         = null,
        [FromQuery] int       page       = 1,
        [FromQuery] int       pageSize   = 50,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new ListAuditLogsQuery(module, entityType, entityId, action, from, to, page, pageSize), ct);
        return Ok(result);
    }
}
