using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAxis.Application.Features.Forms.Queries;
using ServiceAxis.Domain.Enums;

namespace ServiceAxis.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/ui-rules")]
public class DynamicUIController : BaseApiController
{
    private readonly ISender _sender;

    public DynamicUIController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Fetches the completely aggregated dynamic UI metadata payload (form layout + rules) 
    /// for a specific table. This endpoint is called heavily by the Form rendering engine.
    /// </summary>
    [HttpGet("{tableId:guid}")]
    public async Task<IActionResult> GetUiMetadata(Guid tableId, [FromQuery] string context = "default", CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetUiMetadataQuery(tableId, context), ct);
        return Ok(result);
    }
}
