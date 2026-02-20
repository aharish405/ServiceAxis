using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceAxis.Application.Features.Configuration.Commands;
using ServiceAxis.Application.Features.Configuration.Models;
using ServiceAxis.Application.Features.Configuration.Queries;
using System.Text.Json;

namespace ServiceAxis.API.Controllers;

[Authorize(Roles = "SuperAdmin")] // Strict authorization mapping for deploy configurations.
[ApiController]
[Route("api/v1/config/packages")]
public class ConfigurationPackagesController : BaseApiController
{
    private readonly ISender _sender;

    public ConfigurationPackagesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Exports the holistic dynamic UI configurations for a specified Table (e.g., 'Incident').
    /// The resulting JSON payload abstracts GUIDs utilizing unique Names serving cross-environment mobility.
    /// </summary>
    [HttpGet("export/table/{tableName}")]
    public async Task<IActionResult> ExportTableConfig(string tableName, CancellationToken ct)
    {
        var result = await _sender.Send(new ExportTableConfigQuery(tableName), ct);
        
        var options = new JsonSerializerOptions { WriteIndented = true };
        var jsonText = JsonSerializer.Serialize(result, options);
        var bytes = System.Text.Encoding.UTF8.GetBytes(jsonText);
        
        return File(bytes, "application/json", $"ServiceAxis_ConfigExport_{tableName}_{DateTime.UtcNow:yyyyMMdd}.json");
    }

    /// <summary>
    /// Evaluates an inbound JSON configuration package. By default assumes `dryRun=true` 
    /// explicitly logging dependencies (tables + fields) returning structural diffs versus immediate deployment execution.
    /// Setting `dryRun=false` natively persists the dependencies triggering Form invalidation sweeps.
    /// </summary>
    [HttpPost("import")]
    public async Task<IActionResult> ImportConfigPackage([FromBody] ConfigPackageDto package, [FromQuery] bool dryRun = true, CancellationToken ct = default)
    {
        var command = new ImportConfigPackageCommand(package, dryRun);
        var result = await _sender.Send(command, ct);

        if (!result.Success && !result.IsDryRun)
        {
            return BadRequest(new
            {
                Message = "Import execution failed due to critical dependency misalignments natively resolved within the DryRun validation constraints.",
                result.Errors,
                result.Messages
            });
        }

        return Ok(result);
    }
}
