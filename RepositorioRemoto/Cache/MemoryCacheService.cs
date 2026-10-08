using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RepositorioRemoto.Config;

namespace RepositorioRemoto.Cache;

/// <summary>
/// Implementación de ICacheService con la caché en memoria del proceso.
/// </summary>
public class MemoryCacheService(IMemoryCache cache, IOptions<CacheConfig> options) : ICacheService
{
    private readonly TimeSpan _defaultExpiration =
        TimeSpan.FromSeconds(options.Value.ExpirationSeconds);

    /// <inheritdoc />
    public Task<T?> GetAsync<T>(string key) where T : class
    {
        cache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    /// <inheritdoc />
    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        var opciones = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(expiration ?? _defaultExpiration);

        cache.Set(key, value, opciones);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> RemoveAsync(string key)
    {
        if (!cache.TryGetValue(key, out _))
        {
            return Task.FromResult(false);
        }

        cache.Remove(key);
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task ClearAsync()
    {
        if (cache is MemoryCache memoryCache)
        {
            memoryCache.Clear();
        }

        return Task.CompletedTask;
    }
}