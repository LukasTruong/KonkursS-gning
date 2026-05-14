using System.Text.Json;
using StackExchange.Redis;

namespace KonkursCheck.Infrastructure.Caching;

public static class CacheKeys
{
    public static string Search(string hash) => $"search:{hash}";
    public static string Person(string id) => $"person:{id}";
    public static string PersonBankruptcies(string id) => $"person:{id}:bankruptcies";
    public static string Company(string cvr) => $"company:{cvr}";
    public static string CompanyPersons(string cvr) => $"company:{cvr}:persons";
}

public class RedisCacheService : IRedisCacheService
{
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromHours(24);
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly IDatabase _db;
    private readonly IServer _server;

    public RedisCacheService(IConnectionMultiplexer mux)
    {
        _db = mux.GetDatabase();
        _server = mux.GetServer(mux.GetEndPoints().First());
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var value = await _db.StringGetAsync(key);
        if (value.IsNullOrEmpty) return null;
        return JsonSerializer.Deserialize<T>(value!, JsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null) where T : class
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        await _db.StringSetAsync(key, json, ttl ?? DefaultTtl);
    }

    public Task RemoveAsync(string key) => _db.KeyDeleteAsync(key);

    public async Task RemoveByPatternAsync(string pattern)
    {
        var keys = _server.Keys(pattern: pattern).ToArray();
        if (keys.Length > 0)
            await _db.KeyDeleteAsync(keys);
    }
}
