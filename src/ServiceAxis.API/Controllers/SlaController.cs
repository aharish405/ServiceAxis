using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAxis.Application.Contracts.Infrastructure;

namespace ServiceAxis.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/records")] // Align with record APIs
public class SlaController : BaseApiController
{
    private readonly ISlaService _slaService;

    public SlaController(ISlaService slaService)
    {
        _slaService = slaService;
    }

    /// <summary>
    /// Gets the current status of all SLA timers for a specific record.
    /// </summary>
    [HttpGet("{table}/{id}/sla")]
    public async Task<IActionResult> GetRecordSlaStatus(string table, Guid id, CancellationToken ct)
    {
        // Table name is not strictly needed if ID is unique Guid, but ISlaService methods usually take ID.
        // My GetRecordSlaStatusAsync only takes RecordId.
        // So I ignore table (or validate it if I want).
        
        var status = await _slaService.GetRecordSlaStatusAsync(id, ct);
        return Ok(status);
    }
}
