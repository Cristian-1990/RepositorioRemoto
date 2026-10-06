using Microsoft.Extensions.Caching.Memory;

namespace RepositorioRemoto.Cache;

public class CacheService(IMemoryCache cache): ICacheService
{
    public Task<T?> GetAsync<T>(string key) where T : class
    {
        cache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        if ( expiration.HasValue)
        {
            cache.Set(key, value, expiration.Value);
        }
        else
        {
            cache.Set(key, value);
        }
        return Task.CompletedTask;
    }

    public Task<bool> RemoveAsync(string key)
    {
        cache.Remove(key);
        return Task.FromResult(true);
    }

    public Task ClearAsync()
    {
        if (cache is MemoryCache memoryCache)
        {
            memoryCache.Clear();
        }
        return Task.CompletedTask;
    }
}