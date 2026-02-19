using MediatR;
using Microsoft.AspNetCore.Mvc;
using ServiceAxis.Application.Features.Records.Commands;
using ServiceAxis.Application.Features.Records.Queries;

namespace ServiceAxis.API.Controllers;

[ApiController]
[Route("api/records")]
public class RecordController : ControllerBase
{
    private readonly ISender _sender;

    public RecordController(ISender sender) => _sender = sender;

    [HttpPost("{table}")]
    public async Task<IActionResult> Create(string table, [FromBody] Dictionary<string, string?> values)
    {
        var command = new CreateRecordCommand(table, values, null);
        var result = await _sender.Send(command);
        return CreatedAtAction(nameof(Get), new { table, id = result.Id }, result);
    }

    [HttpGet("{table}/{id}")]
    public async Task<IActionResult> Get(string table, Guid id)
    {
        var result = await _sender.Send(new GetRecordQuery(table, id));
        return Ok(result);
    }

    [HttpPut("{table}/{id}")]
    public async Task<IActionResult> Update(string table, Guid id, [FromBody] Dictionary<string, string?> values)
    {
        var result = await _sender.Send(new UpdateRecordCommand(id, table, values));
        return Ok(result);
    }
    
    [HttpGet("{table}")]
    public async Task<IActionResult> List(
        string table, 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20,
        [FromQuery] string? state = null,
        [FromQuery] string? assignedTo = null)
    {
        var result = await _sender.Send(new ListRecordsQuery(table, page, pageSize, state, assignedTo));
        return Ok(result);
    }
}
