using MediatR;
using Microsoft.AspNetCore.Mvc;
using ServiceAxis.Application.Features.Metadata.Queries;

namespace ServiceAxis.API.Controllers;

[ApiController]
[Route("api/metadata")]
public class MetadataController : ControllerBase
{
    private readonly ISender _sender;

    public MetadataController(ISender sender) => _sender = sender;

    [HttpGet("tables")]
    public async Task<IActionResult> ListTables([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _sender.Send(new ListSysTablesQuery(page, pageSize));
        return Ok(result);
    }

    [HttpGet("tables/{name}")]
    public async Task<IActionResult> GetTableSchema(string name)
    {
        var result = await _sender.Send(new GetTableSchemaQuery(name));
        return Ok(result);
    }

    [HttpGet("forms/{table}")]
    public async Task<IActionResult> GetFormSchema(string table, [FromQuery] string context = "default")
    {
        var result = await _sender.Send(new GetFormSchemaQuery(table, context));
        return Ok(result);
    }
}
