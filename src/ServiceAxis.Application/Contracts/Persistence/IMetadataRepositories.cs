using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Entities.Platform;

namespace ServiceAxis.Application.Contracts.Persistence;

/// <summary>
/// Repository for the metadata table registry.
/// </summary>
public interface ISysTableRepository : IGenericRepository<SysTable>
{
    Task<SysTable?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<SysTable?> GetWithFieldsAsync(string name, CancellationToken ct = default);
    Task<bool> ExistsAsync(string name, CancellationToken ct = default);
    Task<PagedResult<SysTable>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
}

/// <summary>
/// Repository for dynamic field definitions.
/// </summary>
public interface ISysFieldRepository : IGenericRepository<SysField>
{
    Task<IReadOnlyList<SysField>> GetByTableAsync(Guid tableId, CancellationToken ct = default);
    Task<IReadOnlyList<SysField>> GetByTableNameAsync(string tableName, CancellationToken ct = default);
    Task<SysField?> GetByTableAndNameAsync(Guid tableId, string fieldName, CancellationToken ct = default);
}
