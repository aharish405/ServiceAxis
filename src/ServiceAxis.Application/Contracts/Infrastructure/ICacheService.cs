namespace ServiceAxis.Application.Contracts.Infrastructure;

/// <summary>
/// Abstraction for cache read/write operations.
/// The default implementation can use Redis or an in-memory fallback.
/// </summary>
public interface ICacheService
{
    /// <summary>Retrieves a value from cache.</summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>Stores a value in cache with an optional sliding expiration.</summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>Removes a key from the cache.</summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>Returns a value if it exists, otherwise calls the factory, stores it, and returns it.</summary>
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class;
}
