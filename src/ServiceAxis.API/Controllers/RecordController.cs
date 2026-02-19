using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceAxis.Application.Features.Records.Commands;
using ServiceAxis.Application.Features.Records.Queries;
using ServiceAxis.Application.Features.Activity.Commands;
using ServiceAxis.Application.Features.Activity.Queries;

namespace ServiceAxis.API.Controllers;

/// <summary>
/// Dynamic Records API — create, read, update, delete, and collaborate on any platform table record.
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/records")]
public class RecordController : ControllerBase
{
    private readonly ISender _sender;

    public RecordController(ISender sender) => _sender = sender;

    // ─── Core CRUD ─────────────────────────────────────────────────────────────

    [HttpPost("{table}")]
    public async Task<IActionResult> Create(string table, [FromBody] Dictionary<string, string?> values)
    {
        var command = new CreateRecordCommand(table, values, null);
        var result = await _sender.Send(command);
        return CreatedAtAction(nameof(Get), new { table, id = result.Id }, result);
    }

    [HttpGet("{table}/{id:guid}")]
    public async Task<IActionResult> Get(string table, Guid id)
    {
        var result = await _sender.Send(new GetRecordQuery(table, id));
        return Ok(result);
    }

    [HttpPut("{table}/{id:guid}")]
    public async Task<IActionResult> Update(string table, Guid id, [FromBody] Dictionary<string, string?> values)
    {
        var result = await _sender.Send(new UpdateRecordCommand(id, table, values));
        return Ok(result);
    }

    [HttpDelete("{table}/{id:guid}")]
    [Authorize(Policy = "ManagerUp")]
    public async Task<IActionResult> Delete(string table, Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteRecordCommand(table, id), ct);
        return NoContent();
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

    // ─── Assignment ────────────────────────────────────────────────────────────

    /// <summary>Manually assigns a record to a specific user and/or assignment group.</summary>
    [HttpPost("{table}/{id:guid}/assign")]
    [Authorize(Policy = "AgentUp")]
    public async Task<IActionResult> Assign(
        string table,
        Guid id,
        [FromBody] AssignRequest request,
        CancellationToken ct)
    {
        await _sender.Send(new AssignRecordCommand(table, id, request.UserId, request.GroupId), ct);
        return Ok(new { message = "Record assigned successfully." });
    }

    // ─── Collaboration & Activity Stream ───────────────────────────────────────

    /// <summary>
    /// Returns the paginated activity timeline for a record — including field changes,
    /// comments, work notes, attachment events, and workflow events — ordered newest first.
    /// </summary>
    /// <remarks>
    /// Sample response:
    /// <code>
    /// {
    ///   "items": [
    ///     { "type": "FieldChanged", "createdBy": "admin", "createdAt": "2026-02-19T10:00:00Z",
    ///       "changes": [{ "field": "priority", "old": "3", "new": "1" }] },
    ///     { "type": "CommentAdded", "createdBy": "jsmith", "createdAt": "2026-02-19T09:55:00Z",
    ///       "comment": { "text": "Investigating now.", "isInternal": false } }
    ///   ]
    /// }
    /// </code>
    /// </remarks>
    [HttpGet("{table}/{id:guid}/activities")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> GetActivities(
        string table,
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetActivityTimelineQuery(table, id, page, pageSize), ct);
        return Ok(result);
    }

    /// <summary>
    /// Adds a comment or work note to a record. Work notes (IsInternal=true) are only visible
    /// to users with the <c>platform.activity.internal_notes</c> permission.
    /// </summary>
    [HttpPost("{table}/{id:guid}/comments")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AddComment(
        string table,
        Guid id,
        [FromBody] AddCommentRequest request,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(new AddCommentCommand(table, id, request.Text, request.IsInternal), ct);
        return Ok(new { success = result });
    }

    /// <summary>
    /// Uploads a file attachment to a record. Stores via <c>IFileStorageProvider</c> (local disk by default;
    /// swap to Azure Blob / S3 by replacing the DI registration). Automatically creates an
    /// <c>AttachmentAdded</c> activity entry on success.
    /// </summary>
    [HttpPost("{table}/{id:guid}/attachments")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UploadAttachment(
        string table,
        Guid id,
        IFormFile file,
        CancellationToken ct = default)
    {
        if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

        using var stream = file.OpenReadStream();
        var result = await _sender.Send(new AddAttachmentCommand(
            table,
            id,
            file.FileName,
            file.ContentType,
            file.Length,
            stream), ct);

        return Ok(new { attachmentId = result });
    }
}

public record AddCommentRequest(string Text, bool IsInternal);
public record AssignRequest(string? UserId, Guid? GroupId);
