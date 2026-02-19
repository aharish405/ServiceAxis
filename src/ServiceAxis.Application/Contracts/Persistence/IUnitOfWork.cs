namespace ServiceAxis.Application.Contracts.Persistence;

/// <summary>
/// Unit of Work contract that coordinates multiple repository operations in a single transaction.
/// Call <see cref="SaveChangesAsync"/> once all mutations are staged.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    /// <summary>Gets the repository for the given entity type.</summary>
    IGenericRepository<T> Repository<T>() where T : Domain.Common.BaseEntity;

    /// <summary>Commits all staged changes to the database.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Begins a new database transaction.</summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Commits the current database transaction.</summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Rolls back the current database transaction.</summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
