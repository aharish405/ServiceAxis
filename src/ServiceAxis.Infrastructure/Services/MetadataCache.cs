using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ServiceAxis.Application.Contracts.Infrastructure;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Entities.Forms;
using ServiceAxis.Domain.Entities.Platform;
using ServiceAxis.Application.Contracts.Identity;

namespace ServiceAxis.Infrastructure.Services;

public class MetadataCache : IMetadataCache
{
    private readonly IMemoryCache _cache;
    private readonly ICurrentUserService _currentUser;
    private readonly ISysTableRepository _tables;
    private readonly IFormRepository _forms;
    private readonly ILogger<MetadataCache> _logger;

    private const string TableKeyPrefix = "mt_table_";
    private const string FormKeyPrefix = "mt_form_";
    private const string AllTablesKey = "mt_all_tables";
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromHours(1);

    public MetadataCache(
        IMemoryCache cache, 
        ICurrentUserService currentUser,
        ISysTableRepository tables, 
        IFormRepository forms,
        ILogger<MetadataCache> logger)
    {
        _cache = cache;
        _currentUser = currentUser;
        _tables = tables;
        _forms = forms;
        _logger = logger;
    }

    public async Task<SysTable?> GetTableAsync(string tableName, CancellationToken ct = default)
    {
        var tenantId = _currentUser.TenantId?.ToString() ?? "global";
        var key = $"{TableKeyPrefix}{tenantId}_{tableName.ToLowerInvariant()}";
        
        if (!_cache.TryGetValue(key, out SysTable? table))
        {
            _logger.LogDebug("Cache miss for table metadata: {Table}", tableName);
            table = await _tables.GetWithFieldsAsync(tableName, ct);
            
            if (table != null)
            {
                _cache.Set(key, table, DefaultExpiration);
            }
        }

        return table;
    }

    public async Task<FormDefinition?> GetFormAsync(string tableName, string context = "default", CancellationToken ct = default)
    {
        var tenantId = _currentUser.TenantId?.ToString() ?? "global";
        var key = $"{FormKeyPrefix}{tenantId}_{tableName.ToLowerInvariant()}_{context.ToLowerInvariant()}";

        if (!_cache.TryGetValue(key, out FormDefinition? form))
        {
            _logger.LogDebug("Cache miss for form metadata: {Table} ({Context})", tableName, context);
            form = await _forms.GetFormWithSectionsAsync(tableName, context, ct);

            if (form != null)
            {
                _cache.Set(key, form, DefaultExpiration);
            }
        }

        return form;
    }

    public async Task<List<SysTable>> GetAllTablesAsync(CancellationToken ct = default)
    {
        var tenantId = _currentUser.TenantId?.ToString() ?? "global";
        var cacheKey = $"{AllTablesKey}_{tenantId}";

        if (!_cache.TryGetValue(cacheKey, out List<SysTable>? tables))
        {
            _logger.LogDebug("Cache miss for all tables list");
            var pagedResult = await _tables.GetPagedAsync(1, 1000, ct);
            tables = pagedResult.Items.ToList();
            _cache.Set(cacheKey, tables, DefaultExpiration);
        }

        return tables ?? new List<SysTable>();
    }

    public void InvalidateTable(string tableName)
    {
        var tenantId = _currentUser.TenantId?.ToString() ?? "global";
        var tableKey = $"{TableKeyPrefix}{tenantId}_{tableName.ToLowerInvariant()}";
        var listKey = $"{AllTablesKey}_{tenantId}";

        _cache.Remove(tableKey);
        _cache.Remove(listKey);
        
        _logger.LogInformation("Invalidated cache for table: {Table}", tableName);
    }

    public void InvalidateAll()
    {
        // For simplicity in V1, we let it expire or explicitly invalidate tables
    }
}
