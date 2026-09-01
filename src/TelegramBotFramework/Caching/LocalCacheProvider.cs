#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Caching;

using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.Logging;

/// <summary>
/// In-memory cache provider using concurrent dictionaries.
/// Suitable for single-instance deployments and development.
/// Automatically removes expired entries on access.
/// </summary>
public sealed class LocalCacheProvider : ICacheProvider, ILocalCacheProvider, IEquatable<LocalCacheProvider>
{
    private const int CleanupBatchSize = 32;
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly object _cleanupLock = new();
    private readonly ILogger<LocalCacheProvider>? _logger;
    private IEnumerator<KeyValuePair<string, CacheEntry>>? _cleanupEnumerator;
    private long _hitCount = 0;
    private long _missCount = 0;
    private long _setCount = 0;
    private long _removeCount = 0;

    public LocalCacheProvider(ILogger<LocalCacheProvider>? logger = null)
    {
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        _logger?.LogInformation("GetAsync called with key {Key}", key);

        if (string.IsNullOrWhiteSpace(key))
        {
            _logger?.LogWarning("GetAsync received an invalid cache key");
            return Task.FromResult<T?>(default);
        }

        _logger?.LogInformation("Getting cache entry for key {Key}", key);

        if (_cache.TryGetValue(key, out var entry))
        {
            // Check expiration
            if (entry.ExpiredAt.HasValue && DateTime.UtcNow > entry.ExpiredAt)
            {
                _cache.TryRemove(key, out _);
                Interlocked.Increment(ref _missCount);
                _logger?.LogInformation("Cache entry expired and removed for key {Key}", key);
                _logger?.LogInformation("GetAsync completed for key {Key} with cache miss", key);
                return Task.FromResult<T?>(default);
            }

            Interlocked.Increment(ref _hitCount);
            _logger?.LogInformation("Cache hit for key {Key}", key);

            try
            {
                _logger?.LogInformation("GetAsync completed for key {Key} with cache hit", key);
                return Task.FromResult((T?)entry.Value);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to cast cache value for key {Key}", key);
                _logger?.LogWarning("GetAsync is returning the default value for key {Key} after a cache value type mismatch", key);
                return Task.FromResult<T?>(default);
            }
        }

        Interlocked.Increment(ref _missCount);
        _logger?.LogInformation("Cache miss for key {Key}", key);
        _logger?.LogInformation("GetAsync completed for key {Key} with cache miss", key);
        return Task.FromResult<T?>(default);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        _logger?.LogInformation("SetAsync called with key {Key} and expiration {Expiration}", key, expiration);

        if (string.IsNullOrWhiteSpace(key))
        {
            _logger?.LogWarning("SetAsync received an invalid cache key");
            return Task.CompletedTask;
        }

        _logger?.LogInformation("Setting cache entry for key {Key}", key);

        var entry = new CacheEntry
        {
            Value = value,
            CreatedAt = DateTime.UtcNow,
            ExpiredAt = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : null
        };

        _cache[key] = entry;
        Interlocked.Increment(ref _setCount);
        CleanupExpiredEntries();

        if (_logger?.IsEnabled(LogLevel.Debug) == true)
        {
            _logger.LogDebug("Cache entry set - Key: {Key}, Expiration: {ExpirationMs}ms",
                key, expiration?.TotalMilliseconds);
        }

        _logger?.LogInformation("SetAsync completed for key {Key}", key);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _logger?.LogInformation("RemoveAsync called with key {Key}", key);

        if (string.IsNullOrWhiteSpace(key))
        {
            _logger?.LogWarning("RemoveAsync received an invalid cache key");
            return Task.CompletedTask;
        }

        if (_cache.TryRemove(key, out _))
        {
            Interlocked.Increment(ref _removeCount);
            if (_logger?.IsEnabled(LogLevel.Debug) == true)
            {
                _logger.LogDebug("Cache entry removed - Key: {Key}", key);
            }
        }
        else
        {
            _logger?.LogWarning("RemoveAsync could not find cache entry for key {Key}", key);
        }

        _logger?.LogInformation("RemoveAsync completed for key {Key}", key);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("ExistsAsync called with key {Key}", key);
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(key))
        {
            _logger?.LogWarning("ExistsAsync received an invalid cache key");
            return Task.FromResult(false);
        }

        if (_cache.TryGetValue(key, out var entry))
        {
            // Check expiration
            if (entry.ExpiredAt.HasValue && DateTime.UtcNow > entry.ExpiredAt)
            {
                _cache.TryRemove(key, out _);
                _logger?.LogWarning("ExistsAsync found and removed expired cache entry for key {Key}", key);
                _logger?.LogInformation("ExistsAsync completed for key {Key} with result {Exists}", key, false);
                return Task.FromResult(false);
            }

            _logger?.LogInformation("ExistsAsync completed for key {Key} with result {Exists}", key, true);
            return Task.FromResult(true);
        }

        _logger?.LogInformation("ExistsAsync completed for key {Key} with result {Exists}", key, false);
        return Task.FromResult(false);
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        _logger?.LogInformation("GetOrCreateAsync called with key {Key} and expiration {Expiration}", key, expiration);
        var existing = await GetAsync<T>(key).ConfigureAwait(false);
        if (existing  is not null)
        {
            _logger?.LogInformation("GetOrCreateAsync completed for key {Key} using cached value", key);
            return existing;
        }

        _logger?.LogWarning("GetOrCreateAsync is creating a value for key {Key} because no cached value was available", key);
        var value = await factory().ConfigureAwait(false);
        await SetAsync(key, value, expiration).ConfigureAwait(false);
        _logger?.LogInformation("GetOrCreateAsync completed for key {Key} using created value", key);
        return value;
    }

    public Task FlushAsync()
    {
        _logger?.LogInformation("FlushAsync called with {ItemCount} cached items", _cache.Count);
        _cache.Clear();
        lock (_cleanupLock)
        {
            _cleanupEnumerator?.Dispose();
            _cleanupEnumerator = null;
        }
        Interlocked.Exchange(ref _hitCount, 0);
        Interlocked.Exchange(ref _missCount, 0);
        Interlocked.Exchange(ref _setCount, 0);
        Interlocked.Exchange(ref _removeCount, 0);
        _logger?.LogInformation("FlushAsync completed");
        return Task.CompletedTask;
    }

    public Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("GetStatisticsAsync called");
        cancellationToken.ThrowIfCancellationRequested();

        // Clean up expired entries while gathering stats
        var expiredKeys = _cache
            .Where(kvp => kvp.Value.ExpiredAt.HasValue && DateTime.UtcNow > kvp.Value.ExpiredAt)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _cache.TryRemove(key, out _);
        }

        var stats = new CacheStatistics
        {
            HitCount = Interlocked.Read(ref _hitCount),
            MissCount = Interlocked.Read(ref _missCount),
            SetCount = Interlocked.Read(ref _setCount),
            RemoveCount = Interlocked.Read(ref _removeCount),
            ItemCount = _cache.Count,
            MemoryBytes = EstimateMemoryUsage()
        };

        _logger?.LogInformation(
            "GetStatisticsAsync completed with {ItemCount} items and {ExpiredItemCount} expired items removed",
            stats.ItemCount,
            expiredKeys.Count);
        return Task.FromResult(stats);
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>true if the current object is equal to the <paramref name="other">parameter</paramref>; otherwise, false.</returns>
    public bool Equals(LocalCacheProvider? other)
    {
        if (ReferenceEquals(other, null))
            return false;

        if (ReferenceEquals(this, other))
            return true;

        // Compare cache contents
        if (_cache.Count != other._cache.Count)
            return false;

        // Compare each cache entry
        foreach (var kvp in _cache)
        {
            if (!other._cache.TryGetValue(kvp.Key, out var otherEntry))
                return false;

            if (!object.Equals(kvp.Value.Value, otherEntry.Value))
                return false;

            if (kvp.Value.CreatedAt != otherEntry.CreatedAt)
                return false;

            if (kvp.Value.ExpiredAt != otherEntry.ExpiredAt)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(obj, null))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != GetType())
            return false;

        return Equals((LocalCacheProvider)obj);
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        var hash = 0;
        foreach (var kvp in _cache.OrderBy(kvp => kvp.Key)) // Order by key for consistent hash
        {
            hash = HashCode.Combine(
                hash,
                kvp.Key,
                kvp.Value.Value,
                kvp.Value.CreatedAt,
                kvp.Value.ExpiredAt);
        }

        return hash;
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    /// <param name="left">Left operand.</param>
    /// <param name="right">Right operand.</param>
    /// <returns>true if operands are equal; otherwise, false.</returns>
    public static bool operator ==(LocalCacheProvider? left, LocalCacheProvider? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            return false;

        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator.
    /// </summary>
    /// <param name="left">Left operand.</param>
    /// <param name="right">Right operand.</param>
    /// <returns>true if operands are not equal; otherwise, false.</returns>
    public static bool operator !=(LocalCacheProvider? left, LocalCacheProvider? right)
    {
        return !(left == right);
    }

    private long EstimateMemoryUsage()
    {
        long total = 0;
        foreach (var entry in _cache.Values)
        {
            // Marshal.SizeOf throws for non-blittable types (i.e. almost any cached
            // reference type), so only use cheap safe heuristics here.
            total += entry.Value switch
            {
                null => 0,
                string str => System.Text.Encoding.UTF8.GetByteCount(str) + 24,
                byte[] bytes => bytes.LongLength + 24,
                System.Collections.ICollection collection => collection.Count * 64L + 24,
                var value when value.GetType().IsPrimitive => 16,
                _ => 128 // Rough default for arbitrary reference types
            };
        }
        return total;
    }

    private void CleanupExpiredEntries()
    {
        lock (_cleanupLock)
        {
            _cleanupEnumerator ??= _cache.GetEnumerator();
            var now = DateTime.UtcNow;

            for (var scanned = 0; scanned < CleanupBatchSize; scanned++)
            {
                if (!_cleanupEnumerator.MoveNext())
                {
                    _cleanupEnumerator.Dispose();
                    _cleanupEnumerator = null;
                    break;
                }

                var entry = _cleanupEnumerator.Current;
                if (entry.Value.ExpiredAt.HasValue && now > entry.Value.ExpiredAt)
                {
                    _cache.TryRemove(entry.Key, out _);
                }
            }
        }
    }

    private class CacheEntry
    {
        public object? Value { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiredAt { get; set; }
    }
}
