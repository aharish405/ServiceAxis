using Microsoft.EntityFrameworkCore;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Common;
using ServiceAxis.Domain.Entities.Platform;
using ServiceAxis.Infrastructure.Persistence;

namespace ServiceAxis.Infrastructure.Persistence.Repositories;

public class SysTableRepository : GenericRepository<SysTable>, ISysTableRepository
{
    public SysTableRepository(ServiceAxisDbContext db) : base(db) { }

    public async Task<SysTable?> GetByNameAsync(string name, CancellationToken ct = default) =>
        await Db.SysTables.FirstOrDefaultAsync(t => t.Name == name.ToLowerInvariant() && t.IsActive, ct);

    public async Task<SysTable?> GetWithFieldsAsync(string name, CancellationToken ct = default) =>
        await Db.SysTables
            .Include(t => t.Fields.Where(f => f.IsActive))
            .FirstOrDefaultAsync(t => t.Name == name.ToLowerInvariant() && t.IsActive, ct);

    public async Task<bool> ExistsAsync(string name, CancellationToken ct = default) =>
        await Db.SysTables.AnyAsync(t => t.Name == name.ToLowerInvariant(), ct);

    public async Task<PagedResult<SysTable>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var total = await Db.SysTables.CountAsync(t => t.IsActive, ct);
        var items = await Db.SysTables
            .Where(t => t.IsActive)
            .Include(t => t.Fields)
            .OrderBy(t => t.DisplayName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<SysTable>
        {
            Items      = items,
            TotalCount = total,
            PageNumber = page,
            PageSize   = pageSize
        };
    }
}

public class SysFieldRepository : GenericRepository<SysField>, ISysFieldRepository
{
    public SysFieldRepository(ServiceAxisDbContext db) : base(db) { }

    public async Task<IReadOnlyList<SysField>> GetByTableAsync(Guid tableId, CancellationToken ct = default) =>
        await Db.SysFields
            .Where(f => f.TableId == tableId && f.IsActive)
            .OrderBy(f => f.DisplayOrder)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<SysField>> GetByTableNameAsync(string tableName, CancellationToken ct = default) =>
        await Db.SysFields
            .Include(f => f.Table)
            .Where(f => f.Table.Name == tableName.ToLowerInvariant() && f.IsActive)
            .OrderBy(f => f.DisplayOrder)
            .ToListAsync(ct);

    public async Task<SysField?> GetByTableAndNameAsync(Guid tableId, string fieldName, CancellationToken ct = default) =>
        await Db.SysFields.FirstOrDefaultAsync(
            f => f.TableId == tableId && f.FieldName == fieldName.ToLowerInvariant(), ct);
}
