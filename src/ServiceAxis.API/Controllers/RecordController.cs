using MediatR;
using Microsoft.AspNetCore.Mvc;
using ServiceAxis.Application.Features.Records.Commands;
using ServiceAxis.Application.Features.Records.Queries;
using ServiceAxis.Application.Features.Activity.Commands;
using ServiceAxis.Application.Features.Activity.Queries;

namespace ServiceAxis.API.Controllers;

[ApiController]
[Route("api/v1/records")]
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
        [FromQuery] int pageSize = 20)
    {
        // Extract all query parameters except page/pageSize as filters
        var filters = HttpContext.Request.Query
            .Where(q => q.Key != "page" && q.Key != "pageSize")
            .ToDictionary(q => q.Key, q => (string?)q.Value.ToString());

        var result = await _sender.Send(new ListRecordsQuery(table, page, pageSize, filters));
        return Ok(result);
    }

    // ─── Collaboration & Activity Stream ───

    [HttpGet("{table}/{id}/activities")]
    public async Task<IActionResult> GetActivities(string table, Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _sender.Send(new GetActivityTimelineQuery(table, id, page, pageSize));
        return Ok(result);
    }

    [HttpPost("{table}/{id}/comments")]
    public async Task<IActionResult> AddComment(string table, Guid id, [FromBody] AddCommentRequest request)
    {
        var result = await _sender.Send(new AddCommentCommand(table, id, request.Text, request.IsInternal));
        return Ok(new { success = result });
    }

    [HttpPost("{table}/{id}/attachments")]
    public async Task<IActionResult> UploadAttachment(string table, Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

        using var stream = file.OpenReadStream();
        var result = await _sender.Send(new AddAttachmentCommand(
            table, 
            id, 
            file.FileName, 
            file.ContentType, 
            file.Length, 
            stream));

        return Ok(new { attachmentId = result });
    }
}

public record AddCommentRequest(string Text, bool IsInternal);

