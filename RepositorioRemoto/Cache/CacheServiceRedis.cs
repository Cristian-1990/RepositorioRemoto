using System.Text.Json;
using StackExchange.Redis;

namespace RepositorioRemoto.Cache;

public class CacheServiceRedis(IConnectionMultiplexer redis): ICacheService
{
    private readonly IDatabase _database = redis.GetDatabase();
    
    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var value = await _database.StringGetAsync(key);
        var json = value.ToString();
        if (value.IsNullOrEmpty)
        {
            return null;
        }

        return JsonSerializer.Deserialize<T>(json);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        var json = JsonSerializer.Serialize(value);
        await _database.StringSetAsync(key, json, expiration);
    }

    public async Task<bool> RemoveAsync(string key)
    {
        return await _database.KeyDeleteAsync(key);
    }

    public async Task ClearAsync()
    {
        var endpoint = redis.GetEndPoints();
        var server = redis.GetServer(endpoint.First());
        
        var key = server.Keys(pattern: "user:*");

        foreach (var keyValue in key)
        {
            await _database.KeyDeleteAsync(keyValue);
        }
    }
}