#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace TelegramBotFramework.Caching;

using System.Diagnostics;

/// <summary>
/// Extension methods for <see cref="LocalCacheProvider"/> providing additional caching utilities.
/// </summary>
public static class LocalCacheProviderExtensions
{
    /// <summary>
    /// Attempts to get a value from cache, and returns a boolean indicating success.
    /// </summary>
    /// <typeparam name="T">The type of value to retrieve.</typeparam>
    /// <param name="provider">The cache provider instance.</param>
    /// <param name="key">The cache key.</param>
    /// <returns>A tuple containing success status and the retrieved value if successful.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="provider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
    public static async Task<(bool Success, T? Value)> TryGetAsync<T>(this LocalCacheProvider provider, string key)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(key);

        var result = await provider.GetAsync<T>(key).ConfigureAwait(false);
        return (result is not null, result);
    }

    /// <summary>
    /// Gets a value from cache or creates it using a factory function if not found.
    /// </summary>
    /// <typeparam name="T">The type of value to retrieve.</typeparam>
    /// <param name="provider">The cache provider instance.</param>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">The factory function to create the value if not in cache.</param>
    /// <param name="expiration">Optional expiration time span.</param>
    /// <returns>The cached or newly created value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="provider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="factory"/> is <see langword="null"/>.</exception>
    public static async Task<T> GetOrCreateAsync<T>(this LocalCacheProvider provider, string key, Func<T> factory, TimeSpan? expiration = null)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(factory);

        var existing = await provider.GetAsync<T>(key).ConfigureAwait(false);
        if (existing is not null)
        {
            return existing;
        }

        var value = factory();
        await provider.SetAsync(key, value, expiration).ConfigureAwait(false);
        return value;
    }

    /// <summary>
    /// Gets multiple values from cache in a single operation.
    /// </summary>
    /// <typeparam name="T">The type of values to retrieve.</typeparam>
    /// <param name="provider">The cache provider instance.</param>
    /// <param name="keys">The collection of cache keys.</param>
    /// <returns>A dictionary mapping keys to their cached values (or default if not found).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="provider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="keys"/> is <see langword="null"/>.</exception>
    public static async Task<Dictionary<string, T?>> GetManyAsync<T>(this LocalCacheProvider provider, IEnumerable<string> keys)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(keys);

        var result = new Dictionary<string, T?>();
        foreach (var key in keys)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                var value = await provider.GetAsync<T>(key).ConfigureAwait(false);
                result[key] = value;
            }
        }

        return result;
    }

    /// <summary>
    /// Sets multiple values in cache in a single operation.
    /// </summary>
    /// <typeparam name="T">The type of values to store.</typeparam>
    /// <param name="provider">The cache provider instance.</param>
    /// <param name="values">A dictionary mapping keys to their values.</param>
    /// <param name="expiration">Optional expiration time span for all entries.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="provider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="values"/> is <see langword="null"/>.</exception>
    public static async Task SetManyAsync<T>(this LocalCacheProvider provider, Dictionary<string, T> values, TimeSpan? expiration = null)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(values);

        foreach (var kvp in values)
        {
            if (!string.IsNullOrWhiteSpace(kvp.Key))
            {
                await provider.SetAsync(kvp.Key, kvp.Value, expiration).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Removes multiple keys from cache in a single operation.
    /// </summary>
    /// <param name="provider">The cache provider instance.</param>
    /// <param name="keys">The collection of keys to remove.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="provider"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="keys"/> is <see langword="null"/>.</exception>
    public static async Task RemoveManyAsync(this LocalCacheProvider provider, IEnumerable<string> keys)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(keys);

        foreach (var key in keys)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                await provider.RemoveAsync(key).ConfigureAwait(false);
            }
        }
    }


}