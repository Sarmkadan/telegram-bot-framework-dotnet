# ICacheProvider
The `ICacheProvider` type is designed to provide a standardized interface for interacting with cache systems, allowing for the retrieval and manipulation of cached data in a uniform manner. It exposes various metrics and properties that offer insights into the cache's performance and current state, enabling more informed decisions about cache utilization and optimization.

## API
* `public long HitCount`: Gets the number of successful cache retrievals. This property does not take any parameters and returns the total count of cache hits as a `long` value. It does not throw any exceptions.
* `public long MissCount`: Gets the number of unsuccessful cache retrievals. This property does not take any parameters and returns the total count of cache misses as a `long` value. It does not throw any exceptions.
* `public long SetCount`: Gets the number of times data has been added to the cache. This property does not take any parameters and returns the total count of cache sets as a `long` value. It does not throw any exceptions.
* `public long RemoveCount`: Gets the number of times data has been removed from the cache. This property does not take any parameters and returns the total count of cache removals as a `long` value. It does not throw any exceptions.
* `public int ItemCount`: Gets the current number of items stored in the cache. This property does not take any parameters and returns the count of cache items as an `int` value. It does not throw any exceptions.
* `public long MemoryBytes`: Gets the total amount of memory used by the cache in bytes. This property does not take any parameters and returns the memory usage in bytes as a `long` value. It does not throw any exceptions.

## Usage
The following examples demonstrate how to use the `ICacheProvider` interface to monitor and manage cache performance:
```csharp
// Example 1: Basic cache metrics retrieval
ICacheProvider cacheProvider = new CacheProvider();
Console.WriteLine($"Cache Hits: {cacheProvider.HitCount}");
Console.WriteLine($"Cache Misses: {cacheProvider.MissCount}");
Console.WriteLine($"Cache Size: {cacheProvider.ItemCount} items");
Console.WriteLine($"Memory Usage: {cacheProvider.MemoryBytes} bytes");

// Example 2: Monitoring cache performance over time
ICacheProvider cache = new CacheProvider();
while (true)
{
    Console.WriteLine($"Cache Hits: {cache.HitCount}, Misses: {cache.MissCount}, Size: {cache.ItemCount} items");
    Thread.Sleep(1000); // Wait for 1 second before checking again
}
```

## Notes
When using the `ICacheProvider` interface, consider the following points:
- The `HitCount`, `MissCount`, `SetCount`, and `RemoveCount` properties are cumulative and will continue to increment as the cache is used.
- The `ItemCount` property reflects the current number of items in the cache and may fluctuate as items are added or removed.
- The `MemoryBytes` property provides an estimate of the cache's memory usage and may not reflect the exact amount of memory used due to various factors such as compression or encoding.
- Implementations of `ICacheProvider` should ensure thread-safety to prevent concurrent modifications from interfering with metric accuracy.
- Edge cases, such as cache overflow or eviction policies, are implementation-dependent and may not be directly reflected in the `ICacheProvider` interface.
