#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Caching;

/// <summary>
/// Interface for cache providers (in-memory, distributed, etc).
/// Provides abstraction for caching implementation details.
/// </summary>
public interface ICacheProvider
{
    /// <summary>
    /// Gets a value from cache by key.
    /// </summary>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// Sets a value in cache with optional expiration.
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

    /// <summary>
    /// Removes a value from cache.
    /// </summary>
    Task RemoveAsync(string key);

    /// <summary>
    /// Checks if a key exists in cache.
    /// </summary>
    Task<bool> ExistsAsync(string key);

    /// <summary>
    /// Gets value from cache or calls factory if not present.
    /// </summary>
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);

    /// <summary>
    /// Clears all cache entries.
    /// </summary>
    Task FlushAsync();

    /// <summary>
    /// Gets cache statistics for monitoring.
    /// </summary>
    Task<CacheStatistics> GetStatisticsAsync();
}

/// <summary>
/// Statistics about cache performance.
/// </summary>
public sealed class CacheStatistics
{
    public long HitCount { get; set; }
    public long MissCount { get; set; }
    public long SetCount { get; set; }
    public long RemoveCount { get; set; }
    public int ItemCount { get; set; }
    public long MemoryBytes { get; set; }

    public double HitRate => (HitCount + MissCount) > 0
        ? (double)HitCount / (HitCount + MissCount) * 100
        : 0;
}