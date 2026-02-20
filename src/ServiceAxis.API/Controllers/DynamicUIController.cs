using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAxis.Application.Features.Forms.Queries;
using ServiceAxis.Application.Features.Forms.Commands;
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
    [AllowAnonymous]
    [HttpGet("{identifier}")]
    public async Task<IActionResult> GetUiMetadata(string identifier, [FromQuery] string context = "default", CancellationToken ct = default)
    {
        Guid tableId;
        if (!Guid.TryParse(identifier, out tableId))
        {
            // Resolve table by code (name)
            var metadataCache = HttpContext.RequestServices.GetRequiredService<ServiceAxis.Application.Contracts.Infrastructure.IMetadataCache>();
            var table = await metadataCache.GetTableAsync(identifier, ct);
            if (table == null) return NotFound($"Table with identifier '{identifier}' not found.");
            tableId = table.Id;
        }

        var result = await _sender.Send(new GetUiMetadataQuery(tableId, context), ct);
        return Ok(result);
    }

    /// <summary>
    /// Save or overwrite the Form Layout (Sections and Fields) for a specific table.
    /// This is invoked by the Form Builder UI Studio upon Publish.
    /// </summary>
    [HttpPost("layout/{tableId:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> SaveFormLayout(Guid tableId, [FromBody] SaveFormLayoutRequest request, CancellationToken ct = default)
    {
        var cmd = new SaveFormLayoutCommand(tableId, request.Context, request.Sections);
        await _sender.Send(cmd, ct);
        return Ok(new { Message = "Form layout saved successfully." });
    }
}

public record SaveFormLayoutRequest(
    string Context,
    List<SaveFormSectionDto> Sections);
