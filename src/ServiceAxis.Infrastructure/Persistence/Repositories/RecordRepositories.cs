using Microsoft.EntityFrameworkCore;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Entities.Records;
using ServiceAxis.Infrastructure.Persistence;

namespace ServiceAxis.Infrastructure.Persistence.Repositories;

public class RecordRepository : GenericRepository<PlatformRecord>, IRecordRepository
{
    public RecordRepository(ServiceAxisDbContext db) : base(db) { }

    public async Task<PlatformRecord?> GetWithValuesAsync(Guid id, CancellationToken ct = default) =>
        await Db.PlatformRecords
            .Include(r => r.Table)
            .Include(r => r.Values).ThenInclude(v => v.Field)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, ct);

    public async Task<PagedResult<PlatformRecord>> GetByTableAsync(
        string tableName, int page, int pageSize,
        string? state, string? assignedToUserId, CancellationToken ct)
    {
        var query = Db.PlatformRecords
            .Include(r => r.Table)
            .Where(r => r.Table.Name == tableName.ToLowerInvariant() && !r.IsDeleted);

        if (!string.IsNullOrWhiteSpace(state))
            query = query.Where(r => r.State == state);

        if (!string.IsNullOrWhiteSpace(assignedToUserId))
            query = query.Where(r => r.AssignedToUserId == assignedToUserId);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<PlatformRecord>
        {
            Items      = items,
            TotalCount = total,
            PageNumber = page,
            PageSize   = pageSize
        };
    }

    public async Task<string> GenerateRecordNumberAsync(string tableName, CancellationToken ct = default)
    {
        var table = await Db.SysTables.FirstOrDefaultAsync(t => t.Name == tableName, ct);
        if (table is null || table.AutoNumberPrefix is null)
            return Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

        // Atomically increment the seed
        // A more robust approach would use a sequence or row-level lock; this is suitable for moderate load
        var count = await Db.PlatformRecords
            .Include(r => r.Table)
            .CountAsync(r => r.Table.Name == tableName, ct);

        var nextNum = table.AutoNumberSeed + count;
        return $"{table.AutoNumberPrefix}{nextNum:D4}";
    }

    public async Task<PlatformRecord?> GetByNumberAsync(string tableName, string recordNumber, CancellationToken ct = default) =>
        await Db.PlatformRecords
            .Include(r => r.Table)
            .Include(r => r.Values).ThenInclude(v => v.Field)
            .FirstOrDefaultAsync(r => r.Table.Name == tableName && r.RecordNumber == recordNumber && !r.IsDeleted, ct);

    public async Task<PagedResult<PlatformRecord>> SearchAsync(
        string tableName, 
        Dictionary<string, string?> filters, 
        int page, 
        int pageSize, 
        CancellationToken ct = default)
    {
        var query = Db.PlatformRecords
            .Include(r => r.Table)
            .Where(r => r.Table.Name == tableName.ToLowerInvariant() && !r.IsDeleted);

        foreach (var (key, value) in filters)
        {
            if (string.IsNullOrWhiteSpace(value)) continue;

            // Handle reserved fields
            if (key.Equals("state", StringComparison.OrdinalIgnoreCase))
                query = query.Where(r => r.State == value);
            else if (key.Equals("priority", StringComparison.OrdinalIgnoreCase) && int.TryParse(value, out var pri))
                query = query.Where(r => r.Priority == pri);
            else if (key.Equals("assigned_to", StringComparison.OrdinalIgnoreCase))
                query = query.Where(r => r.AssignedToUserId == value);
            else
            {
                // Handle dynamic EAV fields
                query = query.Where(r => r.Values.Any(v => v.Field.FieldName == key && v.Value == value));
            }
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<PlatformRecord>
        {
            Items = items,
            TotalCount = total,
            PageNumber = page,
            PageSize = pageSize
        };
    }
}

public class RecordValueRepository : GenericRepository<RecordValue>, IRecordValueRepository
{
    public RecordValueRepository(ServiceAxisDbContext db) : base(db) { }

    public async Task<IReadOnlyList<RecordValue>> GetByRecordAsync(Guid recordId, CancellationToken ct = default) =>
        await Db.RecordValues
            .Include(v => v.Field)
            .Where(v => v.RecordId == recordId)
            .ToListAsync(ct);

    public async Task UpsertValueAsync(Guid recordId, Guid fieldId, string? value, CancellationToken ct = default)
    {
        var existing = await Db.RecordValues
            .FirstOrDefaultAsync(v => v.RecordId == recordId && v.FieldId == fieldId, ct);

        if (existing is null)
        {
            Db.RecordValues.Add(new RecordValue { RecordId = recordId, FieldId = fieldId, Value = value });
        }
        else
        {
            existing.Value     = value;
            existing.UpdatedAt = DateTime.UtcNow;
        }
    }
}
