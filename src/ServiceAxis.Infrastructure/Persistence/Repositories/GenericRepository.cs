using Microsoft.EntityFrameworkCore;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Common;
using System.Linq.Expressions;

namespace ServiceAxis.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic repository implementation backed by EF Core.
/// All queries are async and respect the <see cref="BaseEntity.IsActive"/> soft-delete flag
/// when using <see cref="GetAllAsync"/> and <see cref="GetPagedAsync"/>.
/// </summary>
public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly ServiceAxisDbContext Db;
    protected readonly DbSet<T> DbSet;

    public GenericRepository(ServiceAxisDbContext context)
    {
        Db = context;
        DbSet = context.Set<T>();
    }

    // ── Queries ───────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await DbSet.Where(e => e.IsActive).AsNoTracking().ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default) =>
        await DbSet.Where(predicate).AsNoTracking().ToListAsync(cancellationToken);

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<PagedResult<T>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = DbSet.AsNoTracking();

        if (predicate is not null)
            query = query.Where(predicate);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>
        {
            Items      = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize   = pageSize
        };
    }

    public async Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default) =>
        await DbSet.AnyAsync(predicate, cancellationToken);

    // ── Commands ──────────────────────────────────────────────────────────

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public void Update(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        DbSet.Update(entity);
    }

    public void Delete(T entity) => DbSet.Remove(entity);

    public void SoftDelete(T entity)
    {
        entity.IsActive  = false;
        entity.UpdatedAt = DateTime.UtcNow;
        DbSet.Update(entity);
    }
}
