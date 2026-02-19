using MediatR;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Common;
using ServiceAxis.Shared.Exceptions;

namespace ServiceAxis.Application.Features.Records.Queries;

// ─── Get Single Record ────────────────────────────────────────────────────────

public record GetRecordQuery(string TableName, Guid RecordId) : IRequest<RecordDetailDto>;

public record RecordDetailDto(
    Guid Id,
    string TableName,
    string? RecordNumber,
    string State,
    string? ShortDescription,
    int? Priority,
    string? AssignedToUserId,
    Guid? AssignmentGroupId,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    Dictionary<string, string?> Fields);

public class GetRecordHandler : IRequestHandler<GetRecordQuery, RecordDetailDto>
{
    private readonly IRecordRepository _records;

    public GetRecordHandler(IRecordRepository records) => _records = records;

    public async Task<RecordDetailDto> Handle(GetRecordQuery q, CancellationToken ct)
    {
        var record = await _records.GetWithValuesAsync(q.RecordId, ct)
            ?? throw new NotFoundException($"Record '{q.RecordId}' not found.");

        var fieldValues = record.Values.ToDictionary(
            v => v.Field.FieldName,
            v => v.Value);

        return new RecordDetailDto(
            record.Id,
            record.Table.Name,
            record.RecordNumber,
            record.State,
            record.ShortDescription,
            record.Priority,
            record.AssignedToUserId,
            record.AssignmentGroupId,
            record.CreatedAt,
            record.UpdatedAt,
            fieldValues);
    }
}

// ─── List Records ─────────────────────────────────────────────────────────────

public record ListRecordsQuery(
    string TableName,
    int Page = 1,
    int PageSize = 20,
    string? State = null,
    string? AssignedToUserId = null) : IRequest<PagedResult<RecordSummaryDto>>;

public record RecordSummaryDto(
    Guid Id,
    string? RecordNumber,
    string State,
    string? ShortDescription,
    int? Priority,
    string? AssignedToUserId,
    DateTime CreatedAt);

public class ListRecordsHandler : IRequestHandler<ListRecordsQuery, PagedResult<RecordSummaryDto>>
{
    private readonly IRecordRepository _records;

    public ListRecordsHandler(IRecordRepository records) => _records = records;

    public async Task<PagedResult<RecordSummaryDto>> Handle(ListRecordsQuery q, CancellationToken ct)
    {
        var paged = await _records.GetByTableAsync(q.TableName, q.Page, q.PageSize, q.State, q.AssignedToUserId, ct);

        return new PagedResult<RecordSummaryDto>
        {
            Items = paged.Items.Select(r => new RecordSummaryDto(
                r.Id, r.RecordNumber, r.State, r.ShortDescription, r.Priority, r.AssignedToUserId, r.CreatedAt)).ToList(),
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize   = paged.PageSize
        };
    }
}
