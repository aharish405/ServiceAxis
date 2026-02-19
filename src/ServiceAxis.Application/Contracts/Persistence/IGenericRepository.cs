using ServiceAxis.Domain.Common;
using System.Linq.Expressions;

namespace ServiceAxis.Application.Contracts.Persistence;

/// <summary>
/// Generic repository contract defining standard CRUD and query operations.
/// Implementations live in the Infrastructure layer.
/// </summary>
/// <typeparam name="T">Domain entity that derives from <see cref="BaseEntity"/>.</typeparam>
public interface IGenericRepository<T> where T : BaseEntity
{
    // ---------- Queries ----------
    IQueryable<T> AsQueryable();

    /// <summary>Returns all active entities (IsActive = true) in the set.</summary>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Finds entities matching the given predicate.</summary>
    Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>Returns a single entity by its primary key, or null if not found.</summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Returns a paged result set.</summary>
    Task<PagedResult<T>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    /// <summary>Returns true if any entity matches the predicate.</summary>
    Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    // ---------- Commands ----------

    /// <summary>Adds a new entity to the context (not yet persisted).</summary>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing entity in the context (not yet persisted).</summary>
    void Update(T entity);

    /// <summary>Hard-deletes an entity from the context (not yet persisted).</summary>
    void Delete(T entity);

    /// <summary>Soft-deletes an entity by setting IsActive = false (not yet persisted).</summary>
    void SoftDelete(T entity);
}
