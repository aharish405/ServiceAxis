using ServiceAxis.Application.Contracts.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace ServiceAxis.Infrastructure.Services;

/// <summary>
/// In-memory cache implementation of <see cref="ICacheService"/>.
/// Replace with a Redis-backed implementation in production by swapping the DI registration.
/// </summary>
internal sealed class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    public MemoryCacheService(IMemoryCache cache) => _cache = cache;

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        _cache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(30)
        };
        _cache.Set(key, value, options);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key, Func<Task<T>> factory, TimeSpan? expiry = null,
        CancellationToken cancellationToken = default) where T : class
    {
        if (_cache.TryGetValue(key, out T? value) && value is not null)
            return value;

        value = await factory();
        await SetAsync(key, value, expiry, cancellationToken);
        return value;
    }
}
