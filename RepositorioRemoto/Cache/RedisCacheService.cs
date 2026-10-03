using System.Text.Json;
using Microsoft.Extensions.Options;
using RepositorioRemoto.Config;
using StackExchange.Redis;

namespace RepositorioRemoto.Cache;

/// <summary>
/// Implementación de ICacheService con Redis.
/// Es la del perfil Prod: la caché vive fuera del proceso y se puede compartir
/// entre varias instancias. Patrón de la solución 06-Redis-Cache.
/// </summary>
public class RedisCacheService(IConnectionMultiplexer redis, IOptions<CacheConfig> options) : ICacheService
{
    private readonly IDatabase _db = redis.GetDatabase();

    private readonly TimeSpan _defaultExpiration =
        TimeSpan.FromSeconds(options.Value.ExpirationSeconds);

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var value = await _db.StringGetAsync(key);
        if (value.IsNullOrEmpty)
        {
            return null;
        }

        return JsonSerializer.Deserialize<T>((string)value!);
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        var json = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, json, expiration ?? _defaultExpiration);
    }

    /// <inheritdoc />
    public async Task<bool> RemoveAsync(string key)
    {
        return await _db.KeyDeleteAsync(key);
    }

    /// <inheritdoc />
    public async Task ClearAsync()
    {
        foreach (var endpoint in redis.GetEndPoints())
        {
            var server = redis.GetServer(endpoint);
            await server.FlushDatabaseAsync();
        }
    }
}