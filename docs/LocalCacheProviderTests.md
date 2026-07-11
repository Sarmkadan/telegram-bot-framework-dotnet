# LocalCacheProviderTests

This class contains unit tests for the `LocalCacheProvider` implementation, covering its core caching operations such as storing, retrieving, removing, and checking existence of entries, as well as expiration, factory-based creation, cache statistics, and full flushing. Additionally, it includes tests for an associated event bus (exposed via the `EventBusTests` property), verifying subscription, unsubscription, publishing, and clearing of event handlers.

## API

### `public async Task SetAsync_ThenGetAsync_ReturnsStoredValue`
- **Purpose**: Verifies that a value stored via `SetAsync` can be retrieved with `GetAsync`.
- **Parameters**: None.
- **Return value**: `Task` (test passes if assertion succeeds).
- **Throws**: `Xunit.Sdk.XunitException` if the retrieved value does not match the stored value.

### `public async Task GetAsync_WhenKeyDoesNotExist_ReturnsDefault`
- **Purpose**: Ensures `GetAsync` returns the default value (e.g., `null` or `0`) when the key is not present in the cache.
- **Parameters**: None.
- **Return value**: `Task`.
- **Throws**: `Xunit.Sdk.XunitException` if the returned value is not the expected default.

### `public async Task GetAsync_WhenEntryHasExpired_ReturnsDefault`
- **Purpose**: Confirms that an expired cache entry is treated as absent, returning the default value.
- **Parameters**: None.
- **Return value**: `Task`.
- **Throws**: `Xunit.Sdk.XunitException` if the expired entry is still returned.

### `public async Task GetAsync_WhenEntryNotExpired_ReturnsValue`
- **Purpose**: Validates that a non-expired entry is returned correctly.
- **Parameters**: None.
- **Return value**: `Task`.
- **Throws**: `Xunit.Sdk.XunitException` if the value is missing or incorrect.

### `public async Task RemoveAsync_ExistingKey_MakesValueUnavailable`
- **Purpose**: Checks that after removing an existing key, `GetAsync` returns the default value.
- **Parameters**: None.
- **Return value**: `Task`.
- **Throws**: `Xunit.Sdk.XunitException` if the value remains accessible.

### `public async Task ExistsAsync_WhenKeyPresent_ReturnsTrue`
- **Purpose**: Verifies that `ExistsAsync` returns `true` for a key that has been stored and is not expired.
- **Parameters**: None.
- **Return value**: `Task`.
- **Throws**: `Xunit.Sdk.XunitException` if `ExistsAsync` returns `false`.

### `public async Task ExistsAsync_WhenKeyNotPresent_ReturnsFalse`
- **Purpose**: Confirms `ExistsAsync` returns `false` for a key that was never stored.
- **Parameters**: None.
- **Return value**: `Task`.
- **Throws**: `Xunit.Sdk.XunitException` if `ExistsAsync` returns `true`.

### `public async Task ExistsAsync_WhenEntryExpired_ReturnsFalse`
- **Purpose**: Ensures that an expired entry is reported as non-existent by `ExistsAsync`.
- **Parameters**: None.
- **Return value**: `Task`.
- **Throws**: `Xunit.Sdk.XunitException` if `ExistsAsync` returns `true`.

### `public async Task GetOrCreateAsync_WhenKeyMissing_InvokesFactoryAndPersistsResult`
- **Purpose**: Tests that `GetOrCreateAsync` calls the provided factory function when the key is absent, stores the result, and returns it.
- **Parameters**: None.
- **Return value**: `Task`.
- **Throws**: `Xunit.Sdk.XunitException` if the factory is not invoked or the value is not persisted.

### `public async Task GetOrCreateAsync_WhenKeyExists_SkipsFactoryAndReturnsCached`
- **Purpose**: Verifies that `GetOrCreateAsync` does not invoke the factory when the key already exists and returns the cached value.
- **Parameters**: None.
- **Return value**: `Task`.
- **Throws**: `Xunit.Sdk.XunitException` if the factory is called or the returned value differs from the cached one.

### `public async Task FlushAsync_ClearsAllCachedEntries`
- **Purpose**: Confirms that `FlushAsync` removes all entries from the cache, making `ExistsAsync` return `false` for previously stored keys.
- **Parameters**: None.
- **Return value**: `Task`.
- **Throws**: `Xunit.Sdk.XunitException` if any entry remains after flushing.

### `public async Task GetStatisticsAsync_TracksCacheHitsAndMisses`
- **Purpose**: Validates that `GetStatisticsAsync` returns accurate counts of cache hits and misses after a sequence of operations.
- **Parameters**: None.
- **Return value**: `Task`.
- **Throws**: `Xunit.Sdk.XunitException` if the statistics do not match expected values.

### `public EventBusTests`
- **Purpose**: Provides access to a nested test suite for the event bus component, exposing methods to verify subscription, unsubscription, publishing, and clearing of event handlers.
- **Type**: `EventBusTests` (a class containing the following test methods).
- **Parameters**: None (property getter).
- **Return value**: An instance of `EventBusTests`.
- **Throws**: None.

### `public void Subscribe_RegistersHandlerAndReflectsInSubscriberCount`
- **Purpose**: Ensures that subscribing a handler increases the subscriber count for that event type.
- **Parameters**: None.
- **Return value**: `void`.
- **Throws**: `Xunit.Sdk.XunitException` if the count is not incremented.

### `public void Subscribe_MultipleHandlers_AllCountedCorrectly`
- **Purpose**: Verifies that subscribing multiple handlers for the same event type correctly reflects the total count.
- **Parameters**: None.
- **Return value**: `void`.
- **Throws**: `Xunit.Sdk.XunitException` if the count is incorrect.

### `public void Unsubscribe_RemovesHandlerAndDecrementsCount`
- **Purpose**: Confirms that unsubscribing a handler reduces the subscriber count and the handler is no longer invoked.
- **Parameters**: None.
- **Return value**: `void`.
- **Throws**: `Xunit.Sdk.XunitException` if the count does not decrement or the handler is still called.

### `public async Task PublishAsync_WithSubscribedHandler_InvokesHandlerWithCorrectPayload`
- **Purpose**: Tests that publishing an event invokes the subscribed handler with the exact payload provided.
- **Parameters**: None.
- **Return value**: `Task`.
- **Throws**: `Xunit.Sdk.XunitException` if the handler is not called or receives wrong data.

### `public async Task PublishAsync_WithMultipleHandlers_InvokesAllHandlers`
- **Purpose**: Verifies that publishing an event invokes all subscribed handlers for that event type.
- **Parameters**: None.
- **Return value**: `Task`.
- **Throws**: `Xunit.Sdk.XunitException` if any handler is skipped.

### `public async Task PublishAsync_WithNoSubscribers_CompletesWithoutThrowing`
- **Purpose**: Ensures that publishing an event with no subscribers does not throw an exception.
- **Parameters**: None.
- **Return value**: `Task`.
- **Throws**: `Xunit.Sdk.XunitException` if an exception is thrown.

### `public void Clear_RemovesAllSubscriptionsAcrossEventTypes`
- **Purpose**: Confirms that clearing all subscriptions removes handlers for every event type, leaving the subscriber count at zero.
- **Parameters**: None.
- **Return value**: `void`.
- **Throws**: `Xunit.Sdk.XunitException` if any subscription remains.

## Usage

The following examples demonstrate typical usage patterns for the components tested by `LocalCacheProviderTests`.

**Example 1: Using LocalCacheProvider for caching with expiration**

```csharp
using System;
using System.Threading.Tasks;
using YourNamespace.Caching;

public class ExampleUsage
{
    public async Task CacheExample()
    {
        var cache = new LocalCacheProvider();
        string key = "user:123";
        string value = "John Doe";

        // Store with a 5-second expiration
        await cache.SetAsync(key, value, TimeSpan.FromSeconds(5));

        // Retrieve immediately
        var cached = await cache.GetAsync<string>(key);
        Console.WriteLine(cached); // Output: John Doe

        // Wait for expiration
        await Task.Delay(TimeSpan.FromSeconds(6));
        var expired = await cache.GetAsync<string>(key);
        Console.WriteLine(expired == null); // Output: True

        // Use GetOrCreateAsync to lazily populate
        var result = await cache.GetOrCreateAsync(key, () => Task.FromResult("Jane Doe"), TimeSpan.FromMinutes(1));
        Console.WriteLine(result); // Output: Jane Doe
    }
}
```

**Example 2: Using the event bus for publish/subscribe**

```csharp
using System;
using System.Threading.Tasks;
using YourNamespace.Events;

public class EventExample
{
    public async Task PublishSubscribeExample()
    {
        var eventBus = new EventBus(); // Assuming EventBus is the tested class
        string receivedPayload = null;

        // Subscribe a handler
        eventBus.Subscribe<string>(payload => receivedPayload = payload);

        // Publish an event
        await eventBus.PublishAsync("Hello, World!");

        Console.WriteLine(receivedPayload); // Output: Hello, World!

        // Unsubscribe
        eventBus.Unsubscribe<string>(payload => receivedPayload = payload);
        await eventBus.PublishAsync("Second message");
        Console.WriteLine(receivedPayload); // Still "Hello, World!" because handler was removed
    }
}
```

## Notes

- **Edge cases**: The tests cover scenarios where keys do not exist, entries have expired (both absolute and sliding expiration are assumed to be tested), and factory functions are skipped when a cached value is present. The `FlushAsync` test ensures complete removal of all entries, including those with different expiration times.
- **Thread safety**: The `LocalCacheProvider` implementation is not guaranteed to be thread-safe. These tests are designed for single-threaded execution. Concurrent access may lead to race conditions, especially during expiration checks and factory invocation in `GetOrCreateAsync`. For production use, consider wrapping the cache with synchronization primitives or using a thread-safe alternative.
- **Event bus tests**: The `EventBusTests` methods assume that subscriptions are stored per event type and that `Clear` removes all handlers regardless of type. The `PublishAsync` tests verify that handlers are invoked synchronously (or asynchronously depending on implementation) and that no exceptions are thrown when no handlers exist.
- **Test isolation**: Each test method sets up its own cache or event bus instance to avoid state leakage. The `EventBusTests` property likely returns a new instance of the nested test class, ensuring independent state for each test run.
