# LocalCacheProvider

`LocalCacheProvider` is an in-process, time-aware key-value cache implementation designed for transient data storage within a single application instance. It supports asynchronous operations for storing, retrieving, invalidating, and querying arbitrary serializable objects, with automatic expiration tracking per entry. The provider exposes cache statistics and a bulk flush capability, making it suitable for scenarios such as session state, rate-limiting counters, or short-lived Telegram bot dialog context where distributed persistence is not required.

## API

### `public LocalCacheProvider`
Constructor. Initializes a new empty cache instance with no pre-existing entries. No configuration parameters are exposed at this level; the underlying storage and expiration sweep behaviour is self-contained.

### `public Task<T?> GetAsync<T>(string key)`
Retrieves the cached value associated with the specified key.
- **Parameters:** `key` – the unique string identifier for the cache entry.
- **Returns:** A task whose result is the cached object cast to `T`, or `null` if the key is not present or the entry has expired.
- **Exceptions:** Throws `ArgumentNullException` when `key` is `null`. May throw `InvalidCastException` if the stored object cannot be cast to `T`.

### `public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)`
Stores a value in the cache under the given key, optionally with a time-to-live duration.
- **Parameters:**
  - `key` – unique identifier for the entry.
  - `value` – the object to cache; must be serializable if the implementation requires it.
  - `ttl` – optional lifetime after which the entry is considered expired. A `null` value indicates no expiration.
- **Returns:** A task representing the completion of the write operation.
- **Exceptions:** Throws `ArgumentNullException` when `key` is `null`.

### `public Task RemoveAsync(string key)`
Removes the entry identified by `key` from the cache, if it exists.
- **Parameters:** `key` – the unique identifier of the entry to remove.
- **Returns:** A task that completes once the entry is removed. No error is raised if the key does not exist.
- **Exceptions:** Throws `ArgumentNullException` when `key` is `null`.

### `public Task<bool> ExistsAsync(string key)`
Determines whether a non-expired entry exists for the given key.
- **Parameters:** `key` – the unique identifier to check.
- **Returns:** A task whose result is `true` if a non-expired entry is present; otherwise `false`.
- **Exceptions:** Throws `ArgumentNullException` when `key` is `null`.

### `public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? ttl = null)`
Atomically retrieves an existing non-expired value, or creates and stores a new one using the provided factory if the key is absent or expired.
- **Parameters:**
  - `key` – unique identifier for the entry.
  - `factory` – an asynchronous delegate that produces the value to cache when a miss occurs.
  - `ttl` – optional expiration duration applied to a newly created entry.
- **Returns:** A task whose result is the existing or newly created value of type `T`.
- **Exceptions:** Throws `ArgumentNullException` when `key` or `factory` is `null`. Exceptions thrown by the factory propagate to the caller; the cache remains unchanged in that case.

### `public Task FlushAsync()`
Removes all entries from the cache immediately, regardless of their expiration state.
- **Returns:** A task that completes once the cache is empty.

### `public Task<CacheStatistics> GetStatisticsAsync()`
Returns a snapshot of current cache metrics.
- **Returns:** A task whose result is a `CacheStatistics` object containing properties such as total entry count, hit/miss ratios, and other relevant counters.

### `public object? Value`
Gets the raw cached object for the most recently accessed entry during an internal operation. This property is populated as a side effect of certain retrieval methods and reflects the last deserialized item. Its value is `null` before any retrieval has occurred or when the last operation was a miss.

### `public DateTime CreatedAt`
Gets the creation timestamp of the most recently accessed entry. This value corresponds to the moment the entry was originally stored via `SetAsync` or `GetOrCreateAsync`. It is updated as a side effect of retrieval operations and defaults to `DateTime.MinValue` when no entry has been accessed.

### `public DateTime? ExpiredAt`
Gets the expiration timestamp of the most recently accessed entry, if a TTL was specified. A `null` value indicates the entry has no expiration. This property is populated as a side effect of retrieval operations and is `null` before any retrieval or when the last access was a miss.

## Usage

### Example 1: Basic store, retrieve, and existence check
```csharp
var cache = new LocalCacheProvider();

// Store a value with a 10-minute TTL
await cache.SetAsync("user:42:state", new { Step = "awaiting_input" }, TimeSpan.FromMinutes(10));

// Check existence before retrieval
if (await cache.ExistsAsync("user:42:state"))
{
    var state = await cache.GetAsync<dynamic>("user:42:state");
    Console.WriteLine($"Current step: {state?.Step}");
}

// Remove when processing is complete
await cache.RemoveAsync("user:42:state");
```

### Example 2: Get-or-create pattern with factory
```csharp
var cache = new LocalCacheProvider();

async Task<RateLimitCounter> BuildCounter()
{
    // Simulate expensive initialization
    await Task.Delay(50);
    return new RateLimitCounter { Count = 0, WindowStart = DateTime.UtcNow };
}

// Atomically fetch or create a rate-limit counter valid for 60 seconds
var counter = await cache.GetOrCreateAsync(
    "ratelimit:chat:987",
    BuildCounter,
    TimeSpan.FromSeconds(60)
);

counter.Count++;
Console.WriteLine($"Requests in window: {counter.Count}");
```

## Notes

- **Thread safety:** All public methods are designed for concurrent access. The `GetOrCreateAsync` method ensures that the factory is invoked at most once for a given key, preventing duplicate creation under race conditions. The `Value`, `CreatedAt`, and `ExpiredAt` properties reflect the outcome of the last retrieval operation on the calling instance and are not synchronized across concurrent callers; they should be read immediately after the corresponding `GetAsync` or `GetOrCreateAsync` call on the same logical execution path.
- **Expiration behaviour:** Expired entries are not immediately evicted from the underlying storage on all code paths. An entry past its `ExpiredAt` timestamp is treated as a miss by `GetAsync`, `ExistsAsync`, and `GetOrCreateAsync`, but it may still occupy memory until a background sweep or explicit `FlushAsync`/`RemoveAsync` call cleans it up. Do not rely on instantaneous memory reclamation upon expiration.
- **Property side effects:** `Value`, `CreatedAt`, and `ExpiredAt` are populated only after a successful retrieval (hit). After a miss, their previous values remain unchanged. After `SetAsync` or `RemoveAsync`, these properties are not updated. Reading them without a preceding retrieval yields default or stale data.
- **Statistics:** `GetStatisticsAsync` provides a point-in-time snapshot. Rapid successive calls may return different values if concurrent mutations are in flight.
- **Serialization:** Stored values must be serializable if the underlying implementation uses serialization for deep-copy semantics. Reference-type values stored without serialization are shared between the cache and the caller; mutations to the retrieved object affect the cached copy and vice versa.
