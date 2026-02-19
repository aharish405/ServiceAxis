using System.Text.Json.Serialization;
using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Enums;

namespace ServiceAxis.Application.Contracts.Infrastructure;

public interface IActivityService
{
    Task LogActivityAsync(
        Guid tableId, 
        Guid recordId, 
        ActivityType type, 
        string? message, 
        bool isSystem = true,
        IEnumerable<(string FieldName, Guid? FieldId, string? OldValue, string? NewValue)>? changes = null,
        CancellationToken ct = default);

    Task AddCommentAsync(Guid recordId, string text, bool isInternal, CancellationToken ct = default);
    
    Task<PagedResult<ActivityTimelineDto>> GetTimelineAsync(Guid recordId, int page = 1, int pageSize = 20, CancellationToken ct = default);
}

public record ActivityTimelineDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("type")] ActivityType Type,
    [property: JsonPropertyName("message")] string? Message,
    [property: JsonPropertyName("createdBy")] string? CreatedBy,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt,
    [property: JsonPropertyName("isSystem")] bool IsSystem,
    [property: JsonPropertyName("changes")] IReadOnlyList<FieldChangeDto>? Changes = null,
    [property: JsonPropertyName("comment")] CommentDto? Comment = null);

public record FieldChangeDto(
    [property: JsonPropertyName("field")] string FieldName, 
    [property: JsonPropertyName("old")] string? OldValue, 
    [property: JsonPropertyName("new")] string? NewValue);

public record CommentDto(
    [property: JsonPropertyName("text")] string Text, 
    [property: JsonPropertyName("isInternal")] bool IsInternal, 
    [property: JsonPropertyName("createdBy")] string? CreatedBy);
