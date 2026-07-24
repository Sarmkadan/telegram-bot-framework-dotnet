#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Caching;

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

/// <summary>
/// In-memory cache provider using concurrent dictionaries.
/// Suitable for single-instance deployments and development.
/// Automatically removes expired entries on access.
/// </summary>
public sealed class LocalCacheProvider : ICacheProvider
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly ILogger<LocalCacheProvider>? _logger;
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
        if (string.IsNullOrWhiteSpace(key))
            return Task.FromResult<T?>(default);

        if (_cache.TryGetValue(key, out var entry))
        {
            // Check expiration
            if (entry.ExpiredAt.HasValue && DateTime.UtcNow > entry.ExpiredAt)
            {
                _cache.TryRemove(key, out _);
                Interlocked.Increment(ref _missCount);
                return Task.FromResult<T?>(default);
            }

            Interlocked.Increment(ref _hitCount);

            try
            {
                return Task.FromResult((T?)entry.Value);
            }
            catch
            {
                return Task.FromResult<T?>(default);
            }
        }

        Interlocked.Increment(ref _missCount);
        return Task.FromResult<T?>(default);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            return Task.CompletedTask;

        var entry = new CacheEntry
        {
            Value = value,
            CreatedAt = DateTime.UtcNow,
            ExpiredAt = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : null
        };

        _cache[key] = entry;
        Interlocked.Increment(ref _setCount);

        if (_logger?.IsEnabled(LogLevel.Debug) == true)
        {
            _logger.LogDebug("Cache entry set - Key: {Key}, Expiration: {ExpirationMs}ms",
                key, expiration?.TotalMilliseconds);
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return Task.CompletedTask;

        if (_cache.TryRemove(key, out _))
        {
            Interlocked.Increment(ref _removeCount);
            if (_logger?.IsEnabled(LogLevel.Debug) == true)
            {
                _logger.LogDebug("Cache entry removed - Key: {Key}", key);
            }
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return Task.FromResult(false);

        if (_cache.TryGetValue(key, out var entry))
        {
            // Check expiration
            if (entry.ExpiredAt.HasValue && DateTime.UtcNow > entry.ExpiredAt)
            {
                _cache.TryRemove(key, out _);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        var existing = await GetAsync<T>(key).ConfigureAwait(false);
        if (existing  is not null)
            return existing;

        var value = await factory().ConfigureAwait(false);
        await SetAsync(key, value, expiration).ConfigureAwait(false);
        return value;
    }

    public Task FlushAsync()
    {
        _cache.Clear();
        Interlocked.Exchange(ref _hitCount, 0);
        Interlocked.Exchange(ref _missCount, 0);
        Interlocked.Exchange(ref _setCount, 0);
        Interlocked.Exchange(ref _removeCount, 0);
        return Task.CompletedTask;
    }

    public Task<CacheStatistics> GetStatisticsAsync()
    {
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

        return Task.FromResult(stats);
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

    private class CacheEntry
    {
        public object? Value { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiredAt { get; set; }
    }
}