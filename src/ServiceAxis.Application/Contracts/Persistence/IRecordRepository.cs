using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Entities.Records;

namespace ServiceAxis.Application.Contracts.Persistence;

/// <summary>
/// Repository for the universal record engine.
/// </summary>
public interface IRecordRepository : IGenericRepository<PlatformRecord>
{
    Task<PlatformRecord?> GetWithValuesAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<PlatformRecord>> GetByTableAsync(
        string tableName, int page, int pageSize,
        string? state = null, string? assignedToUserId = null,
        CancellationToken ct = default);
    Task<string> GenerateRecordNumberAsync(string tableName, CancellationToken ct = default);
    Task<PlatformRecord?> GetByNumberAsync(string tableName, string recordNumber, CancellationToken ct = default);
    Task<PagedResult<PlatformRecord>> SearchAsync(
        string tableName, 
        Dictionary<string, string?> filters, 
        int page, 
        int pageSize, 
        CancellationToken ct = default);
}

/// <summary>
/// Repository for EAV field values.
/// </summary>
public interface IRecordValueRepository : IGenericRepository<RecordValue>
{
    Task<IReadOnlyList<RecordValue>> GetByRecordAsync(Guid recordId, CancellationToken ct = default);
    Task UpsertValueAsync(Guid recordId, Guid fieldId, string? value, CancellationToken ct = default);
}
