using ServiceAxis.Domain.Entities.Forms;
using ServiceAxis.Domain.Entities.Platform;

namespace ServiceAxis.Application.Contracts.Infrastructure;

public interface IMetadataCache
{
    Task<SysTable?> GetTableAsync(string tableName, CancellationToken ct = default);
    Task<SysTable?> GetTableByIdAsync(Guid tableId, CancellationToken ct = default);
    Task<SysField?> GetFieldAsync(Guid fieldId, CancellationToken ct = default);
    Task<FormDefinition?> GetFormAsync(string tableName, string context = "default", CancellationToken ct = default);
    Task<List<SysTable>> GetAllTablesAsync(CancellationToken ct = default);
    void InvalidateTable(string tableName);
    void InvalidateAll();
}
