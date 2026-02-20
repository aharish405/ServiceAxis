using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAxis.Application.Features.Metadata.Commands;
using ServiceAxis.Application.Features.Metadata.Queries;

namespace ServiceAxis.API.Controllers;

/// <summary>
/// Metadata API — schema-driven table and field management.
/// Read operations are public (platform configuration). Write operations require AdminOnly.
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/metadata")]
public class MetadataController : ControllerBase
{
    private readonly ISender _sender;

    public MetadataController(ISender sender) => _sender = sender;

    // ─── Tables ───────────────────────────────────────────────────────────────

    /// <summary>Returns a paged list of all registered platform tables.</summary>
    [AllowAnonymous]
    [HttpGet("tables")]
    public async Task<IActionResult> ListTables([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _sender.Send(new ListSysTablesQuery(page, pageSize));
        return Ok(result);
    }

    /// <summary>Returns the full schema (table + all fields) for a specific table.</summary>
    [AllowAnonymous]
    [HttpGet("tables/{name}")]
    public async Task<IActionResult> GetTableSchema(string name)
    {
        var result = await _sender.Send(new GetTableSchemaQuery(name));
        return Ok(result);
    }

    /// <summary>Registers a new platform table. Requires AdminOnly access.</summary>
    [HttpPost("tables")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateTable([FromBody] CreateTableRequest request, CancellationToken ct)
    {
        var result = await _sender.Send(new CreateSysTableCommand(
            request.Name, request.DisplayName, request.SchemaName,
            request.Description, request.Icon, request.AutoNumberPrefix,
            request.AuditEnabled, request.AllowAttachments,
            request.ParentTableId, request.TenantId), ct);
        return CreatedAtAction(nameof(GetTableSchema), new { name = result.Name }, result);
    }

    // ─── Fields ───────────────────────────────────────────────────────────────

    /// <summary>Adds a new field to an existing table. Requires AdminOnly access.</summary>
    [HttpPost("tables/{name}/fields")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> AddField(
        string name,
        [FromBody] AddFieldRequest request,
        CancellationToken ct)
    {
        var result = await _sender.Send(new AddSysFieldCommand(
            name, request.FieldName, request.DisplayName, request.DataType,
            request.IsRequired, request.DefaultValue, request.IsSearchable,
            request.ChoiceOptions, request.LookupTableName,
            request.DisplayOrder, request.HelpText, request.TenantId), ct);
        return Ok(result);
    }

    // ─── Forms ────────────────────────────────────────────────────────────────

    /// <summary>Returns the JSON form schema for a table and context (used by frontend auto-renderer).</summary>
    [AllowAnonymous]
    [HttpGet("forms/{table}")]
    public async Task<IActionResult> GetFormSchema(string table, [FromQuery] string context = "default")
    {
        var result = await _sender.Send(new GetFormSchemaQuery(table, context));
        return Ok(result);
    }
}

// ─── Request DTOs ─────────────────────────────────────────────────────────────

public record CreateTableRequest(
    string Name,
    string DisplayName,
    string SchemaName,
    string? Description,
    string? Icon,
    string? AutoNumberPrefix,
    bool AuditEnabled,
    bool AllowAttachments,
    Guid? ParentTableId,
    Guid? TenantId);

public record AddFieldRequest(
    string FieldName,
    string DisplayName,
    string DataType,
    bool IsRequired,
    string? DefaultValue,
    bool IsSearchable,
    string? ChoiceOptions,
    string? LookupTableName,
    int DisplayOrder,
    string? HelpText,
    Guid? TenantId);

