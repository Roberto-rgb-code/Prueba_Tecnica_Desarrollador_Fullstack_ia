using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using UserService.Application.DTOs;
using UserService.Application.Services;

namespace UserService.Infrastructure.Caching;

public class RedisUserCache : IUserCache
{
    private const string CacheKey = "users:all";
    private readonly IDistributedCache _cache;

    public RedisUserCache(IDistributedCache cache) => _cache = cache;

    public async Task<IReadOnlyList<UserDto>?> GetListAsync(CancellationToken cancellationToken = default)
    {
        var json = await _cache.GetStringAsync(CacheKey, cancellationToken);
        return json is null ? null : JsonSerializer.Deserialize<List<UserDto>>(json);
    }

    public async Task SetListAsync(IReadOnlyList<UserDto> users, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(users);
        await _cache.SetStringAsync(CacheKey, json, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        }, cancellationToken);
    }

    public Task InvalidateAsync(CancellationToken cancellationToken = default) =>
        _cache.RemoveAsync(CacheKey, cancellationToken);
}
