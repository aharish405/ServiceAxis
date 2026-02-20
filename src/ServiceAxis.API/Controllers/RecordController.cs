using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAxis.Application.Features.Records.Commands;
using ServiceAxis.Application.Features.Records.Queries;
using ServiceAxis.Application.Features.Activity.Commands;
using ServiceAxis.Application.Features.Activity.Queries;

namespace ServiceAxis.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/records")]
public class RecordController : BaseApiController
{
    private readonly ISender _sender;

    public RecordController(ISender sender) => _sender = sender;

    [AllowAnonymous]
    [HttpPost("{table}")]
    public async Task<IActionResult> Create(string table, [FromBody] Dictionary<string, string?> values, [FromQuery] Guid? tenantId, CancellationToken ct)
    {
        var result = await _sender.Send(new CreateRecordCommand(table, values, tenantId), ct);
        return Created(result);
    }

    [AllowAnonymous]
    [HttpGet("{table}/{id:guid}")]
    public async Task<IActionResult> Get(string table, Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetRecordQuery(table, id), ct);
        return result is null ? NotFound($"Record '{id}' not found.") : Ok(result);
    }

    [HttpPut("{table}/{id:guid}")]
    public async Task<IActionResult> Update(string table, Guid id, [FromBody] Dictionary<string, string?> values, CancellationToken ct)
    {
        var result = await _sender.Send(new UpdateRecordCommand(id, table, values), ct);
        return Ok(result, "Record updated successfully.");
    }

    [HttpDelete("{table}/{id:guid}")]
    [Authorize(Policy = "ManagerUp")]
    public async Task<IActionResult> Delete(string table, Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteRecordCommand(table, id), ct);
        return Ok<object>(null!, "Record deleted successfully.");
    }

    [AllowAnonymous]
    [HttpGet("{table}")]
    public async Task<IActionResult> List(string table, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var filters = HttpContext.Request.Query
            .Where(q => q.Key != "page" && q.Key != "pageSize")
            .ToDictionary(q => q.Key, q => (string?)q.Value.ToString());

        var result = await _sender.Send(new ListRecordsQuery(table, page, pageSize, filters), ct);
        return Ok(result);
    }

    [HttpPost("{table}/{id:guid}/assign")]
    public async Task<IActionResult> Assign(string table, Guid id, [FromBody] AssignRecordRequest request, CancellationToken ct)
    {
        await _sender.Send(new AssignRecordCommand(table, id, request.UserId, request.GroupId), ct);
        return Ok<object>(new { id }, "Record assigned successfully.");
    }

    [AllowAnonymous]
    [HttpGet("{table}/{id:guid}/activities")]
    public async Task<IActionResult> GetActivities(string table, Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetActivityTimelineQuery(table, id, page, pageSize), ct);
        return Ok(result);
    }

    [HttpPost("{table}/{id:guid}/comments")]
    public async Task<IActionResult> AddComment(string table, Guid id, [FromBody] AddCommentRequest request, CancellationToken ct)
    {
        await _sender.Send(new AddCommentCommand(table, id, request.Comment, request.IsInternal), ct);
        return Ok<object>(new { id }, "Comment added.");
    }

    [HttpPost("{table}/{id:guid}/attachments")]
    public async Task<IActionResult> AddAttachment(string table, Guid id, IFormFile file, CancellationToken ct)
    {
        using var stream = file.OpenReadStream();
        var attachmentId = await _sender.Send(new AddAttachmentCommand(table, id, file.FileName, file.ContentType, file.Length, stream), ct);
        return Ok(new { id = attachmentId }, "File attached.");
    }
}

public record AssignRecordRequest(string? UserId, Guid? GroupId);
public record AddCommentRequest(string Comment, bool IsInternal = false);
