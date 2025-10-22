namespace Modules.Common.Application.Caching;

/// <summary>
/// Interface to mark queries that should be cached.
/// Defines how to generate a cache key and the expiration time.
/// </summary>
public interface ICacheableQuery
{
    /// <summary>
    /// Gets the unique key for caching this query's result.
    /// </summary>
    string CacheKey { get; }

    /// <summary>
    /// Gets the expiration time for the cache entry.
    /// Null means use a default expiration.
    /// </summary>
    TimeSpan? Expiration { get; }
}