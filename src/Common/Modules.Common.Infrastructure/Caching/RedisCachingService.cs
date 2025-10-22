using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging; // For logging
using Modules.Common.Application.Caching; // Use ICachingService
using StackExchange.Redis; // Use Redis types
using System;
using System.Text.Json; // For serialization
using System.Threading;
using System.Threading.Tasks;

namespace Modules.Common.Infrastructure.Caching;

public class RedisCachingService(
    IConnectionMultiplexer redis, // Inject Redis connection
    ILogger<RedisCachingService> logger) : ICachingService
{
    private readonly StackExchange.Redis.IDatabase _database = redis.GetDatabase();
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(5); // Default cache time

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var redisValue = await _database.StringGetAsync(key);
            if (redisValue.IsNullOrEmpty)
            {
                logger.LogDebug("Cache MISS for key: {CacheKey}", key);
                return default; // Return default (null for classes) if not found
            }

            logger.LogDebug("Cache HIT for key: {CacheKey}", key);
            // Deserialize from JSON
            return JsonSerializer.Deserialize<T>(redisValue!);
        }
        catch (RedisException ex)
        {
            logger.LogError(ex, "Redis error getting key {CacheKey}. Returning default.", key);
            return default; // Treat Redis error as cache miss
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var expiry = expiration ?? _defaultExpiration;
            var serializedValue = JsonSerializer.Serialize(value);
            var set = await _database.StringSetAsync(key, serializedValue, expiry);

            if (set)
            {
                logger.LogDebug("Cache SET for key: {CacheKey}, Expiration: {Expiry}", key, expiry);
            }
            else
            {
                logger.LogWarning("Cache SET FAILED for key: {CacheKey}", key);
            }

        }
        catch (RedisException ex)
        {
            logger.LogError(ex, "Redis error setting key {CacheKey}.", key);
            // Don't throw, allow application to continue without cache
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var removed = await _database.KeyDeleteAsync(key);
            if (removed)
            {
                logger.LogDebug("Cache REMOVED for key: {CacheKey}", key);
            }
        }
        catch (RedisException ex)
        {
            logger.LogError(ex, "Redis error removing key {CacheKey}.", key);
        }
    }
}