#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Caching;

using System.Text.Json;
using TelegramBotFramework.Integration;

/// <summary>
/// Abstract base for distributed cache providers (Redis, Memcached, etc).
/// Provides serialization/deserialization and common cache operations.
/// Subclass this for specific distributed cache implementations.
/// </summary>
public abstract class DistributedCacheProvider : ICacheProvider, IDistributedCacheProvider
{
    protected readonly ILogger<DistributedCacheProvider> _logger;

    protected DistributedCacheProvider(ILogger<DistributedCacheProvider>? logger = null)
    {
        _logger = logger ?? new ConsoleLogger<DistributedCacheProvider>();
    }

    public virtual async Task<T?> GetAsync<T>(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        if (string.IsNullOrWhiteSpace(key))
            return default;

        try
        {
            var value = await GetValueAsync(key).ConfigureAwait(false);
            if (value  is null)
                return default;

            return JsonSerializer.Deserialize<T>(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value from distributed cache: {Key}", key);
            return default;
        }
    }

    public virtual async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        if (string.IsNullOrWhiteSpace(key))
            return;

        try
        {
            var json = JsonSerializer.Serialize(value);
            await SetValueAsync(key, json, expiration).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in distributed cache: {Key}", key);
        }
    }

    public virtual async Task RemoveAsync(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        if (string.IsNullOrWhiteSpace(key))
            return;

        try
        {
            await RemoveValueAsync(key).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing value from distributed cache: {Key}", key);
        }
    }

    Task<bool> ICacheProvider.ExistsAsync(string key, CancellationToken cancellationToken) => ExistsAsync(key, cancellationToken);

    Task<bool> IDistributedCacheProvider.ExistsAsync(string key, CancellationToken cancellationToken) => ExistsAsync(key, cancellationToken);

    public virtual async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(key))
            return false;

        try
        {
            return await KeyExistsAsync(key, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence in distributed cache: {Key}", key);
            return false;
        }
    }

    public virtual async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(factory);

        var existing = await GetAsync<T>(key).ConfigureAwait(false);
        if (existing  is not null)
            return existing;

        var value = await factory().ConfigureAwait(false);
        await SetAsync(key, value, expiration).ConfigureAwait(false);
        return value;
    }

    public virtual async Task FlushAsync()
    {
        try
        {
            await FlushAllAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error flushing distributed cache");
        }
    }

    Task<CacheStatistics> ICacheProvider.GetStatisticsAsync(CancellationToken cancellationToken) => GetStatisticsAsync(cancellationToken);

    Task<CacheStatistics> IDistributedCacheProvider.GetStatisticsAsync(CancellationToken cancellationToken) => GetStatisticsAsync(cancellationToken);

    public virtual async Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            return await GetStatsAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting distributed cache statistics");
            return new CacheStatistics();
        }
    }

    /// <summary>
    /// Subclasses must implement this to get a value from the distributed cache.
    /// </summary>
    protected abstract Task<string?> GetValueAsync(string key);

    /// <summary>
    /// Subclasses must implement this to set a value in the distributed cache.
    /// </summary>
    protected abstract Task SetValueAsync(string key, string value, TimeSpan? expiration);

    /// <summary>
    /// Subclasses must implement this to remove a value from the distributed cache.
    /// </summary>
    protected abstract Task RemoveValueAsync(string key);

    /// <summary>
    /// Subclasses must implement this to check if a key exists in the distributed cache.
    /// </summary>
    protected abstract Task<bool> KeyExistsAsync(string key);

    protected virtual Task<bool> KeyExistsAsync(string key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return KeyExistsAsync(key);
    }

    /// <summary>
    /// Subclasses must implement this to flush all cache entries.
    /// </summary>
    protected abstract Task FlushAllAsync();

    /// <summary>
    /// Subclasses should override this to provide cache statistics.
    /// </summary>
    protected virtual Task<CacheStatistics> GetStatsAsync()
    {
        return Task.FromResult(new CacheStatistics());
    }

    protected virtual Task<CacheStatistics> GetStatsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return GetStatsAsync();
    }
}

/// <summary>
/// No-operation distributed cache provider for testing/fallback scenarios.
/// Useful as a fallback when distributed cache is unavailable.
/// </summary>
public sealed class NoOpCacheProvider : ICacheProvider
{
    public Task<T?> GetAsync<T>(string key) => Task.FromResult<T?>(default);
    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) => Task.CompletedTask;
    public Task RemoveAsync(string key) => Task.CompletedTask;
    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default) => Task.FromResult(false);

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        return await factory().ConfigureAwait(false);
    }

    public Task FlushAsync() => Task.CompletedTask;
    public Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default) => Task.FromResult(new CacheStatistics());
}
