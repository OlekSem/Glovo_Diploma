using System.Text.Json;

using GlovoBackEnd.Services.Interfaces;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Core.Services;

public class RedisService  : IRedisService
{
    private readonly IDatabase _db;
    private readonly IConnectionMultiplexer redis;

    public RedisService(IConnectionMultiplexer redis)
    {
        this.redis = redis;
        _db = redis.GetDatabase();
    }
    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(key);
        if (value.IsNullOrEmpty)
            return default;
        return JsonSerializer.Deserialize<T>((string)value!);
    }

    public async Task SetAsync<T>(string key, T data, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(data);
        await _db.StringSetAsync(key, json, expiry, When.Always, CommandFlags.None);
    }

    public async Task RemoveAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
    }
    public async Task RemoveByPrefixAsync(string prefix)
    {
        
        var endpoints = redis.GetEndPoints();

        foreach (var endpoint in endpoints)
        {
            var server = redis.GetServer(endpoint);
            if (!server.IsConnected || server.IsReplica) continue;
            await foreach (var key in server.KeysAsync(pattern: $"{prefix}*"))
            {
                await _db.KeyDeleteAsync(key);
            }
        }
    }
}