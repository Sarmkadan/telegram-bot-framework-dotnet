# DistributedCacheProvider

`DistributedCacheProvider` is an abstraction for a distributed cache used throughout the **telegram-bot-framework-dotnet** project. It offers asynchronous and synchronous methods for retrieving, storing, and managing cached values, as well as utilities for cache statistics and existence checks. Implementations typically wrap a concrete distributed cache (e.g., Redis, Memcached) and expose a consistent API to the rest of the framework.

## API

| Member | Description |
|--------|-------------|
| `Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)` | Retrieves the cached value associated with **key** and deserializes it to type `T`. Returns `null` if the key does not exist. Throws `ArgumentNullException` if **key** is `null` or empty, and may propagate exceptions from the underlying cache provider. |
| `Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null, CancellationToken cancellationToken = default)` | Stores **value** under **key**. If **absoluteExpirationRelativeToNow** is supplied, the entry expires after the given time span; otherwise the provider’s default expiration applies. Throws `ArgumentNullException` for a null **key**, and `ArgumentException` if **value** is `null` for reference types. |
| `Task RemoveAsync(string key, CancellationToken cancellationToken = default)` | Deletes the cache entry identified by **key**. No error is thrown if the key does not exist. Throws `ArgumentNullException` when **key** is `null` or empty. |
| `Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)` | Returns `true` if an entry for **key** exists in the cache; otherwise `false`. Throws `ArgumentNullException` for a null or empty **key**. |
| `Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absoluteExpirationRelativeToNow = null, CancellationToken cancellationToken = default)` | Attempts to retrieve the cached value for **key**. If the key is missing, invokes **factory** to produce the value, stores it using `SetAsync`, and returns the newly created value. Propagates any exception thrown by **factory**. Throws `ArgumentNullException` if **key** or **factory** is `null`. |
| `Task FlushAsync(CancellationToken cancellationToken = default)` | Clears all entries from the underlying distributed cache. Use with caution as it affects all consumers of the cache. May throw exceptions related to connectivity or permission issues. |
| `Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)` | Retrieves runtime statistics such as hit count, miss count, and item count. The exact contents depend on the concrete cache implementation. May throw if the provider does not support statistics collection. |
| `Task<T?> GetAsync<T>(string key)` | Synchronous‑style overload that forwards to the asynchronous version with a default `CancellationToken`. |
| `Task SetAsync<T>(string key, T value)` | Synchronous‑style overload that forwards to the asynchronous version with default expiration and `CancellationToken`. |
| `Task RemoveAsync(string key)` | Synchronous‑style overload that forwards to the asynchronous version. |
| `Task<bool> ExistsAsync(string key)` | Synchronous‑style overload that forwards to the asynchronous version. |
| `Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory)` | Synchronous‑style overload that forwards to the asynchronous version without expiration or cancellation token. |
| `Task FlushAsync()` | Synchronous‑style overload that forwards to the asynchronous version. |
| `Task<CacheStatistics> GetStatisticsAsync()` | Synchronous‑style overload that forwards to the asynchronous version. |

**General error handling**  
All members validate required arguments and throw `ArgumentNullException` (or `ArgumentException` where appropriate) before contacting the underlying cache. Runtime exceptions from the cache (e.g., network failures) are not caught internally and will propagate to the caller.

## Usage

### Example 1 – Caching a Telegram user profile

```csharp
public class UserProfileService
{
    private readonly DistributedCacheProvider _cache;
    private readonly ITelegramApi _api;

    public UserProfileService(DistributedCacheProvider cache, ITelegramApi api)
    {
        _cache = cache;
        _api = api;
    }

    public async Task<UserProfile> GetUserProfileAsync(long userId, CancellationToken ct = default)
    {
        string cacheKey = $"user:{userId}:profile";

        // Try to get the profile from cache, otherwise fetch from Telegram API and cache it.
        return await _cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                var profile = await _api.GetUserProfileAsync(userId, ct);
                return profile;
            },
            absoluteExpirationRelativeToNow: TimeSpan.FromHours(1),
            cancellationToken: ct);
    }

    public async Task InvalidateUserProfileAsync(long userId, CancellationToken ct = default)
    {
        string cacheKey = $"user:{userId}:profile";
        await _cache.RemoveAsync(cacheKey, ct);
    }
}
```

### Example 2 – Monitoring cache health and statistics

```csharp
public class CacheHealthChecker
{
    private readonly DistributedCacheProvider _cache;

    public CacheHealthChecker(DistributedCacheProvider cache)
    {
        _cache = cache;
    }

    public async Task LogStatisticsAsync()
    {
        CacheStatistics stats = await _cache.GetStatisticsAsync();

        Console.WriteLine($"Cache Hits: {stats.Hits}");
        Console.WriteLine($"Cache Misses: {stats.Misses}");
        Console.WriteLine($"Items Stored: {stats.ItemCount}");
    }

    public async Task<bool> EnsureKeyExistsAsync(string key)
    {
        bool exists = await _cache.ExistsAsync(key);
        if (!exists)
        {
            Console.WriteLine($"Cache key '{key}' is missing.");
        }
        return exists;
    }
}
```

## Notes

* **Thread‑safety** – All public members are designed to be safe for concurrent use. Underlying cache implementations typically handle synchronization; `DistributedCacheProvider` does not maintain mutable state per call.  
* **Expiration semantics** – When `absoluteExpirationRelativeToNow` is omitted, the provider’s default TTL applies. Some implementations may also support sliding expiration, but this API surface only exposes absolute expiration.  
* **Null values** – The generic `GetAsync<T>` returns `null` for reference types when the key is absent. For value types, `T?` enables nullable return (`null` indicates a miss). Storing a `null` value is not permitted; attempting to do so results in `ArgumentException`.  
* **Factory execution** – `GetOrCreateAsync` guarantees that the **factory** delegate is invoked at most once per missing key, even under concurrent calls, provided the underlying cache implementation offers atomic “add if not exists” semantics. If the cache does not guarantee atomicity, duplicate factory executions are possible.  
* **Cancellation** – Overloads without a `CancellationToken` use `CancellationToken.None`. Callers needing cooperative cancellation should prefer the overloads that accept a token.  
* **Performance** – Synchronous‑style overloads are thin wrappers around the asynchronous core; they do not block the thread and should be used only when an async signature is inconvenient.  
* **Flush semantics** – `FlushAsync` removes **all** entries across the entire distributed cache cluster. In multi‑tenant scenarios this operation may affect unrelated data; use with explicit intent.  
* **Statistics availability** – Not all cache back‑ends expose statistics. When unsupported, `GetStatisticsAsync` may throw `NotSupportedException` or return a `CacheStatistics` instance with zeroed counters.  

---
