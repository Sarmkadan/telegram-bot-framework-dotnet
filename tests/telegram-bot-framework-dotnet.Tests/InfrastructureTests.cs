#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TelegramBotFramework.Caching;
using TelegramBotFramework.ConversationFlow;
using TelegramBotFramework.Events;
using TelegramBotFramework.Services;
using TelegramBotFramework.Strategies;
using Xunit;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Unit tests for <see cref="LocalCacheProvider"/> which provides in-memory caching functionality
/// for Telegram bot framework components.
/// </summary>
/// <remarks>
/// Tests cover basic CRUD operations, expiration behavior, cache statistics tracking,
/// and thread-safe operations on the cache provider.
/// </remarks>
public sealed class LocalCacheProviderTests : ILocalCacheProviderTests
{
    private readonly LocalCacheProvider _cache = new();

    /// <summary>
    /// Tests that a value stored with SetAsync can be retrieved with GetAsync and returns the expected value.
    /// </summary>
    /// <returns>The stored value from the cache.</returns>
    [Fact]
    public async Task SetAsync_ThenGetAsync_ReturnsStoredValue()
    {
        await _cache.SetAsync("greeting", "hello").ConfigureAwait(false);

        var result = await _cache.GetAsync<string>("greeting").ConfigureAwait(false);

        result.Should().Be("hello");
    }

    /// <summary>
    /// Tests that GetAsync returns null when the requested key does not exist in the cache.
    /// </summary>
    /// <returns>The default value (null) when the key does not exist.</returns>
    [Fact]
    public async Task GetAsync_WhenKeyDoesNotExist_ReturnsDefault()
    {
        var result = await _cache.GetAsync<string>("missing-key").ConfigureAwait(false);

        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that GetAsync returns null when the cache entry has expired based on the specified expiration time.
    /// </summary>
    /// <returns>The default value (null) when the cache entry has expired.</returns>
    [Fact]
    public async Task GetAsync_WhenEntryHasExpired_ReturnsDefault()
    {
        await _cache.SetAsync("expiring", "value", TimeSpan.FromMilliseconds(1)).ConfigureAwait(false);
        await Task.Delay(50).ConfigureAwait(false);

        var result = await _cache.GetAsync<string>("expiring").ConfigureAwait(false);

        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that GetAsync returns the cached value when the entry has not yet expired.
    /// </summary>
    /// <returns>The cached value when the entry has not yet expired.</returns>
    [Fact]
    public async Task GetAsync_WhenEntryNotExpired_ReturnsValue()
    {
        await _cache.SetAsync("persistent", "alive", TimeSpan.FromHours(1)).ConfigureAwait(false);

        var result = await _cache.GetAsync<string>("persistent").ConfigureAwait(false);

        result.Should().Be("alive");
    }

    /// <summary>
    /// Tests that RemoveAsync removes the specified key from the cache, making it unavailable for subsequent operations.
    /// </summary>
    /// <returns>True if the key was successfully removed from the cache.</returns>
    [Fact]
    public async Task RemoveAsync_ExistingKey_MakesValueUnavailable()
    {
        await _cache.SetAsync("toRemove", 42).ConfigureAwait(false);

        await _cache.RemoveAsync("toRemove").ConfigureAwait(false);

        var exists = await _cache.ExistsAsync("toRemove").ConfigureAwait(false);
        exists.Should().BeFalse();
    }

    /// <summary>
    /// Tests that ExistsAsync returns true when the specified key exists in the cache.
    /// </summary>
    /// <returns>True if the specified key exists in the cache.</returns>
    [Fact]
    public async Task ExistsAsync_WhenKeyPresent_ReturnsTrue()
    {
        await _cache.SetAsync("present", true).ConfigureAwait(false);

        var exists = await _cache.ExistsAsync("present").ConfigureAwait(false);

        exists.Should().BeTrue();
    }

    /// <summary>
    /// Tests that ExistsAsync returns false when the specified key does not exist in the cache.
    /// </summary>
    /// <returns>False if the specified key does not exist in the cache.</returns>
    [Fact]
    public async Task ExistsAsync_WhenKeyNotPresent_ReturnsFalse()
    {
        var exists = await _cache.ExistsAsync("not-there").ConfigureAwait(false);

        exists.Should().BeFalse();
    }

    /// <summary>
    /// Tests that ExistsAsync returns false when the cache entry has expired based on the specified expiration time.
    /// </summary>
    /// <returns>False if the cache entry has expired.</returns>
    [Fact]
    public async Task ExistsAsync_WhenEntryExpired_ReturnsFalse()
    {
        await _cache.SetAsync("gone-soon", "x", TimeSpan.FromMilliseconds(1)).ConfigureAwait(false);
        await Task.Delay(50).ConfigureAwait(false);

        var exists = await _cache.ExistsAsync("gone-soon").ConfigureAwait(false);

        exists.Should().BeFalse();
    }

    /// <summary>
    /// Tests that GetOrCreateAsync invokes the factory function and persists the result when the key is missing from the cache.
    /// </summary>
    /// <returns>The value created by the factory function and persisted in the cache.</returns>
    [Fact]
    public async Task GetOrCreateAsync_WhenKeyMissing_InvokesFactoryAndPersistsResult()
    {
        int callCount = 0;

        var value = await _cache.GetOrCreateAsync("new-key", async () =>
        {
            callCount++;
            await Task.CompletedTask;
            return "created";
        });

        value.Should().Be("created");
        callCount.Should().Be(1);
        (await _cache.GetAsync<string>("new-key")).Should().Be("created");
    }

    /// <summary>
    /// Tests that GetOrCreateAsync returns the cached value and skips invoking the factory function when the key already exists in the cache.
    /// </summary>
    /// <returns>The cached value without invoking the factory function.</returns>
    [Fact]
    public async Task GetOrCreateAsync_WhenKeyExists_SkipsFactoryAndReturnsCached()
    {
        await _cache.SetAsync("existing", "cached-value").ConfigureAwait(false);
        int callCount = 0;

        var value = await _cache.GetOrCreateAsync("existing", async () =>
        {
            callCount++;
            await Task.CompletedTask;
            return "should-not-be-used";
        });

        callCount.Should().Be(0);
        value.Should().Be("cached-value");
    }

    /// <summary>
    /// Tests that FlushAsync clears all cached entries from the cache storage.
    /// </summary>
    /// <returns>The updated cache statistics showing zero items after flushing.</returns>
    [Fact]
    public async Task FlushAsync_ClearsAllCachedEntries()
    {
        await _cache.SetAsync("a", 1).ConfigureAwait(false);
        await _cache.SetAsync("b", 2).ConfigureAwait(false);
        await _cache.SetAsync("c", 3).ConfigureAwait(false);

        await _cache.FlushAsync().ConfigureAwait(false);

        var stats = await _cache.GetStatisticsAsync().ConfigureAwait(false);
        stats.ItemCount.Should().Be(0);
    }

    /// <summary>
    /// Tests that GetStatisticsAsync tracks cache hits, misses, and set operations correctly.
    /// </summary>
    /// <returns>The cache statistics including hit count, miss count, and set count.</returns>
    [Fact]
    public async Task GetStatisticsAsync_TracksCacheHitsAndMisses()
    {
        await _cache.SetAsync("tracked", "x").ConfigureAwait(false);
        await _cache.GetAsync<string>("tracked").ConfigureAwait(false);
        await _cache.GetAsync<string>("non-existent").ConfigureAwait(false);

        var stats = await _cache.GetStatisticsAsync().ConfigureAwait(false);

        stats.HitCount.Should().BeGreaterThanOrEqualTo(1);
        stats.MissCount.Should().BeGreaterThanOrEqualTo(1);
        stats.SetCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetAsync_WithNullKey_ReturnsDefaultValue()
    {
        // Act
        var result = await _cache.GetAsync<string>(null!).ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_WithEmptyKey_ReturnsDefaultValue()
    {
        // Act
        var result = await _cache.GetAsync<string>(string.Empty).ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_WithWhitespaceKey_ReturnsDefaultValue()
    {
        // Act
        var result = await _cache.GetAsync<string>("   ").ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_WithNullKey_DoesNotThrow()
    {
        // Act & Assert
        await _cache.SetAsync<object>(null!, new object()).ConfigureAwait(false);

        // Should not throw
        var stats = await _cache.GetStatisticsAsync().ConfigureAwait(false);
        stats.SetCount.Should().Be(0); // No entries should be set
    }

    [Fact]
    public async Task SetAsync_WithEmptyKey_DoesNotThrow()
    {
        // Act & Assert
        await _cache.SetAsync(string.Empty, "value").ConfigureAwait(false);

        // Should not throw
        var stats = await _cache.GetStatisticsAsync().ConfigureAwait(false);
        stats.SetCount.Should().Be(0);
    }

    [Fact]
    public async Task SetAsync_WithWhitespaceKey_DoesNotThrow()
    {
        // Act & Assert
        await _cache.SetAsync("   ", "value").ConfigureAwait(false);

        // Should not throw
        var stats = await _cache.GetStatisticsAsync().ConfigureAwait(false);
        stats.SetCount.Should().Be(0);
    }

    [Fact]
    public async Task SetAsync_WithValidKey_StoresValue()
    {
        // Arrange
        const string key = "valid_key";
        const string value = "test_value";

        // Act
        await _cache.SetAsync(key, value).ConfigureAwait(false);

        // Assert
        var result = await _cache.GetAsync<string>(key).ConfigureAwait(false);
        result.Should().Be(value);

        var stats = await _cache.GetStatisticsAsync().ConfigureAwait(false);
        stats.SetCount.Should().Be(1);
        stats.ItemCount.Should().Be(1);
    }

    [Fact]
    public async Task SetAsync_WithExpiration_StoresValueWithExpiration()
    {
        // Arrange
        const string key = "expiring_key";
        const string value = "expiring_value";
        var expiration = TimeSpan.FromMilliseconds(100);

        // Act
        await _cache.SetAsync(key, value, expiration).ConfigureAwait(false);

        // Assert - value should be retrievable immediately
        var result = await _cache.GetAsync<string>(key).ConfigureAwait(false);
        result.Should().Be(value);
    }

    [Fact]
    public async Task ExpiryAfterTTL_ValueExpiresAfterTimeSpan()
    {
        // Arrange
        const string key = "expiry_test_key";
        const string value = "expiry_test_value";
        var expiration = TimeSpan.FromMilliseconds(50);

        await _cache.SetAsync(key, value, expiration).ConfigureAwait(false);

        // Act - wait for expiration
        await Task.Delay(100).ConfigureAwait(false);

        // Assert - value should no longer be retrievable
        var result = await _cache.GetAsync<string>(key).ConfigureAwait(false);
        result.Should().BeNull();

        var stats = await _cache.GetStatisticsAsync().ConfigureAwait(false);
        stats.ItemCount.Should().Be(0);
    }

    [Fact]
    public async Task OverwriteExistingKey_UpdatesValue()
    {
        // Arrange
        const string key = "overwrite_key";
        const string initialValue = "initial_value";
        const string updatedValue = "updated_value";

        await _cache.SetAsync(key, initialValue).ConfigureAwait(false);
        var initialResult = await _cache.GetAsync<string>(key).ConfigureAwait(false);
        initialResult.Should().Be(initialValue);

        // Act - overwrite with new value
        await _cache.SetAsync(key, updatedValue).ConfigureAwait(false);

        // Assert - should return updated value
        var updatedResult = await _cache.GetAsync<string>(key).ConfigureAwait(false);
        updatedResult.Should().Be(updatedValue);

        var stats = await _cache.GetStatisticsAsync().ConfigureAwait(false);
        stats.SetCount.Should().Be(2); // Two sets: initial and update
    }

    [Fact]
    public async Task RemoveAsync_WithNonExistentKey_DoesNotThrow()
    {
        // Act & Assert
        await _cache.RemoveAsync("nonexistent_key").ConfigureAwait(false);

        // Should not throw
        var stats = await _cache.GetStatisticsAsync().ConfigureAwait(false);
        stats.RemoveCount.Should().Be(0);
    }

    [Fact]
    public async Task NullValues_HandledCorrectly()
    {
        // Arrange
        const string key = "null_value_key";

        // Act - set null value
        await _cache.SetAsync<string>(key, null).ConfigureAwait(false);

        // Assert - should be able to retrieve null
        var result = await _cache.GetAsync<string>(key).ConfigureAwait(false);
        result.Should().BeNull();

        // Verify it exists
        var exists = await _cache.ExistsAsync(key).ConfigureAwait(false);
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ComplexObjectValues_AreStoredAndRetrieved()
    {
        // Arrange
        const string key = "complex_object_key";
        var complexObject = new TestCacheObject
        {
            Id = 123,
            Name = "Test Object",
            Values = new List<int> { 1, 2, 3 },
            Metadata = new Dictionary<string, string>
            {
                { "key1", "value1" },
                { "key2", "value2" }
            }
        };

        // Act
        await _cache.SetAsync(key, complexObject).ConfigureAwait(false);
        var result = await _cache.GetAsync<TestCacheObject>(key).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(complexObject.Id);
        result.Name.Should().Be(complexObject.Name);
        result.Values.Should().BeEquivalentTo(complexObject.Values);
        result.Metadata.Should().BeEquivalentTo(complexObject.Metadata);
    }

    [Fact]
    public async Task RemoveAsync_WithExistingKey_RemovesValue()
    {
        // Arrange
        const string key = "remove_key";
        const string value = "remove_value";
        await _cache.SetAsync(key, value).ConfigureAwait(false);

        // Verify value exists
        var beforeRemove = await _cache.GetAsync<string>(key).ConfigureAwait(false);
        beforeRemove.Should().Be(value);

        // Act
        await _cache.RemoveAsync(key).ConfigureAwait(false);

        // Assert - value should no longer exist
        var afterRemove = await _cache.GetAsync<string>(key).ConfigureAwait(false);
        afterRemove.Should().BeNull();

        var stats = await _cache.GetStatisticsAsync().ConfigureAwait(false);
        stats.RemoveCount.Should().Be(1);
        stats.ItemCount.Should().Be(0);
    }

    [Fact]
    public async Task GetOrCreateAsync_WithExpiration_StoresValueWithExpiration()
    {
        // Arrange
        const string key = "get_or_create_expiring_key";

        // Act - first call creates with expiration
        var result1 = await _cache.GetOrCreateAsync(key, () => Task.FromResult("value1"), TimeSpan.FromMilliseconds(50)).ConfigureAwait(false);

        // Assert - result should be correct
        result1.Should().Be("value1");

        // Verify it was cached
        var cached1 = await _cache.GetAsync<string>(key).ConfigureAwait(false);
        cached1.Should().Be("value1");

        // Wait for expiration
        await Task.Delay(100).ConfigureAwait(false);

        // Value should no longer be retrievable
        var expiredValue = await _cache.GetAsync<string>(key).ConfigureAwait(false);
        expiredValue.Should().BeNull();

        // Factory should be called again since value expired
        var result2 = await _cache.GetOrCreateAsync(key, () => Task.FromResult("value2")).ConfigureAwait(false);
        result2.Should().Be("value2");

        // Verify the new value was cached
        var cached2 = await _cache.GetAsync<string>(key).ConfigureAwait(false);
        cached2.Should().Be("value2");
    }

    private class TestCacheObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<int> Values { get; set; } = new();
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}

public sealed class EventBusTests
{
    private readonly Mock<ILogger<EventBus>> _mockLogger = new();
    private readonly EventBus _bus;

    public EventBusTests()
    {
        _bus = new EventBus(_mockLogger.Object);
    }

    [Fact]
    public void Subscribe_WithNullHandler_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _bus.Subscribe<MessageReceivedEvent>(null!));
    }

    [Fact]
    public void Subscribe_RegistersHandlerAndReflectsInSubscriberCount()
    {
        var handler = new TestMessageHandler();

        _bus.Subscribe<MessageReceivedEvent>(handler);

        _bus.GetSubscriberCount<MessageReceivedEvent>().Should().Be(1);
    }

    [Fact]
    public void Subscribe_MultipleHandlers_AllCountedCorrectly()
    {
        _bus.Subscribe<MessageReceivedEvent>(new TestMessageHandler());
        _bus.Subscribe<MessageReceivedEvent>(new TestMessageHandler());

        _bus.GetSubscriberCount<MessageReceivedEvent>().Should().Be(2);
    }

    [Fact]
    public void Subscribe_HandlersForDifferentEvents_AddsSeparately()
    {
        _bus.Subscribe<MessageReceivedEvent>(new TestMessageHandler());
        _bus.Subscribe<CommandExecutedEvent>(new TestCommandHandler());

        _bus.GetSubscriberCount<MessageReceivedEvent>().Should().Be(1);
        _bus.GetSubscriberCount<CommandExecutedEvent>().Should().Be(1);
    }

    [Fact]
    public void Unsubscribe_WithNullHandler_DoesNotThrow()
    {
        _bus.Unsubscribe<MessageReceivedEvent>(null!);
    }

    [Fact]
    public void Unsubscribe_WithNonSubscribedHandler_DoesNotThrow()
    {
        var handler = new TestMessageHandler();
        _bus.Unsubscribe<MessageReceivedEvent>(handler);

        _bus.GetSubscriberCount<MessageReceivedEvent>().Should().Be(0);
    }

    [Fact]
    public void Unsubscribe_RemovesHandlerAndDecrementsCount()
    {
        var handler = new TestMessageHandler();
        _bus.Subscribe<MessageReceivedEvent>(handler);

        _bus.Unsubscribe<MessageReceivedEvent>(handler);

        _bus.GetSubscriberCount<MessageReceivedEvent>().Should().Be(0);
    }

    [Fact]
    public void Unsubscribe_RemovesOnlySpecifiedHandler()
    {
        var handler1 = new TestMessageHandler();
        var handler2 = new TestMessageHandler();
        _bus.Subscribe<MessageReceivedEvent>(handler1);
        _bus.Subscribe<MessageReceivedEvent>(handler2);

        _bus.Unsubscribe<MessageReceivedEvent>(handler1);

        _bus.GetSubscriberCount<MessageReceivedEvent>().Should().Be(1);
    }

    [Fact]
    public async Task PublishAsync_WithNullEvent_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _bus.PublishAsync<MessageReceivedEvent>(null!)).ConfigureAwait(false);
    }

    [Fact]
    public async Task PublishAsync_WithSubscribedHandler_InvokesHandlerWithCorrectPayload()
    {
        var handler = new TestMessageHandler();
        _bus.Subscribe<MessageReceivedEvent>(handler);
        var evt = new MessageReceivedEvent(chatId: 100L, userId: 200L, messageText: "Hello");

        await _bus.PublishAsync(evt).ConfigureAwait(false);

        handler.Received.Should().HaveCount(1);
        handler.Received[0].MessageText.Should().Be("Hello");
        handler.Received[0].ChatId.Should().Be(100L);
    }

    [Fact]
    public async Task PublishAsync_WithMultipleHandlers_InvokesAllHandlers()
    {
        var handler1 = new TestMessageHandler();
        var handler2 = new TestMessageHandler();
        _bus.Subscribe<MessageReceivedEvent>(handler1);
        _bus.Subscribe<MessageReceivedEvent>(handler2);

        await _bus.PublishAsync(new MessageReceivedEvent(1, 2, "broadcast")).ConfigureAwait(false);

        handler1.Received.Should().HaveCount(1);
        handler2.Received.Should().HaveCount(1);
    }

    [Fact]
    public async Task PublishAsync_WithNoSubscribers_CompletesWithoutThrowing()
    {
        var act = async () => await _bus.PublishAsync(new MessageReceivedEvent(1, 2, "hi")).ConfigureAwait(false);

        await act.Should().NotThrowAsync().ConfigureAwait(false);
    }

    [Fact]
    public async Task PublishAsync_WithHandlerThatThrows_PropagatesException()
    {
        var handler = new FailingTestHandler();
        _bus.Subscribe<MessageReceivedEvent>(handler);
        var evt = new MessageReceivedEvent(1, 2, "fail");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _bus.PublishAsync(evt)).ConfigureAwait(false);
    }

    [Fact]
    public async Task PublishAsync_WithMultipleHandlersAndOneFails_StillInvokesOtherHandlers()
    {
        var handler1 = new TestMessageHandler();
        var handler2 = new FailingTestHandler();
        var handler3 = new TestMessageHandler();
        _bus.Subscribe<MessageReceivedEvent>(handler1);
        _bus.Subscribe<MessageReceivedEvent>(handler2);
        _bus.Subscribe<MessageReceivedEvent>(handler3);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _bus.PublishAsync(new MessageReceivedEvent(1, 2, "mixed"))).ConfigureAwait(false);

        handler1.Received.Should().HaveCount(1);
        handler3.Received.Should().HaveCount(1);
    }

    [Fact]
    public void Clear_RemovesAllSubscriptionsAcrossEventTypes()
    {
        _bus.Subscribe<MessageReceivedEvent>(new TestMessageHandler());
        _bus.Subscribe<CommandExecutedEvent>(new TestCommandHandler());

        _bus.Clear();

        _bus.GetSubscriberCount<MessageReceivedEvent>().Should().Be(0);
        _bus.GetSubscriberCount<CommandExecutedEvent>().Should().Be(0);
    }

    [Fact]
    public void GetSubscriberCount_ForEventWithNoSubscribers_ReturnsZero()
    {
        _bus.GetSubscriberCount<MessageReceivedEvent>().Should().Be(0);
    }

    [Fact]
    public void GetSubscriberCount_AfterSubscribe_ReturnsCorrectCount()
    {
        _bus.Subscribe<MessageReceivedEvent>(new TestMessageHandler());
        _bus.Subscribe<MessageReceivedEvent>(new TestMessageHandler());
        _bus.Subscribe<CommandExecutedEvent>(new TestCommandHandler());

        _bus.GetSubscriberCount<MessageReceivedEvent>().Should().Be(2);
        _bus.GetSubscriberCount<CommandExecutedEvent>().Should().Be(1);
    }

    [Fact]
    public void GetRegisteredEventTypes_ReturnsAllRegisteredTypes()
    {
        _bus.Subscribe<MessageReceivedEvent>(new TestMessageHandler());
        _bus.Subscribe<CommandExecutedEvent>(new TestCommandHandler());

        var types = _bus.GetRegisteredEventTypes().ToList();
        types.Should().HaveCount(2);
        types.Should().Contain(typeof(MessageReceivedEvent));
        types.Should().Contain(typeof(CommandExecutedEvent));
    }

    [Fact]
    public void GetRegisteredEventTypes_WhenNoSubscribers_ReturnsEmptyCollection()
    {
        var types = _bus.GetRegisteredEventTypes();
        types.Should().BeEmpty();
    }

    [Fact]
    public async Task PublishAsync_EventWithCorrelationId_PassesCorrelationIdToHandlers()
    {
        var correlationId = Guid.NewGuid().ToString();
        var handler = new CorrelationTestHandler();
        var evt = new MessageReceivedEvent(1, 2, "test") { CorrelationId = correlationId };

        _bus.Subscribe<MessageReceivedEvent>(handler);
        await _bus.PublishAsync(evt).ConfigureAwait(false);

        handler.LastCorrelationId.Should().Be(correlationId);
    }

    [Fact]
    public async Task PublishAsync_ConcurrentPublishOperations_HandlesCorrectly()
    {
        var handler = new TestMessageHandler();
        _bus.Subscribe<MessageReceivedEvent>(handler);

        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            var evt = new MessageReceivedEvent(i, i + 100, $"Message {i}");
            tasks.Add(_bus.PublishAsync(evt));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

        handler.Received.Should().HaveCount(10);
    }

    [Fact]
    public void PublishAsync_LogsAtLeastOneInformationMessage()
    {
        _bus.Subscribe<MessageReceivedEvent>(new TestMessageHandler());

        _bus.PublishAsync(new MessageReceivedEvent(1, 2, "log-test"));

        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public void PublishAsync_WithNoSubscribers_LogsWarning()
    {
        var evt = new MessageReceivedEvent(1, 2, "no-handlers");

        _bus.PublishAsync(evt);

        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("MessageReceivedEvent")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    private sealed class TestMessageHandler : IEventHandler<MessageReceivedEvent>
    {
        public List<MessageReceivedEvent> Received { get; } = new();

        public Task HandleAsync(MessageReceivedEvent @event)
        {
            Received.Add(@event);
            return Task.CompletedTask;
        }
    }

    private sealed class TestCommandHandler : IEventHandler<CommandExecutedEvent>
    {
        public Task HandleAsync(CommandExecutedEvent @event) => Task.CompletedTask;
    }

    private sealed class FailingTestHandler : IEventHandler<MessageReceivedEvent>
    {
        public Task HandleAsync(MessageReceivedEvent @event) => Task.FromException<InvalidOperationException>(new InvalidOperationException("Handler failed"));
    }

    private sealed class CorrelationTestHandler : IEventHandler<MessageReceivedEvent>
    {
        public string? LastCorrelationId { get; private set; }

        public Task HandleAsync(MessageReceivedEvent @event)
        {
            LastCorrelationId = @event.CorrelationId;
            return Task.CompletedTask;
        }
    }
}public sealed class SlidingWindowStrategyTests
{
    [Fact]
    public void IsRequestAllowed_WhenFirstRequest_ReturnsTrue()
    {
        var strategy = new SlidingWindowStrategy(5, TimeSpan.FromMinutes(1));

        strategy.IsRequestAllowed("user1").Should().BeTrue();
    }

    [Fact]
    public void IsRequestAllowed_AfterExhaustingAllowance_ReturnsFalse()
    {
        var strategy = new SlidingWindowStrategy(3, TimeSpan.FromMinutes(1));
        for (int i = 0; i < 3; i++)
            strategy.IsRequestAllowed("user1");

        strategy.IsRequestAllowed("user1").Should().BeFalse();
    }

    [Fact]
    public void IsRequestAllowed_DifferentIdentifiers_TrackedIndependently()
    {
        var strategy = new SlidingWindowStrategy(1, TimeSpan.FromMinutes(1));
        strategy.IsRequestAllowed("userA");

        strategy.IsRequestAllowed("userB").Should().BeTrue();
    }

    [Fact]
    public void GetRemainingRequests_ForUnknownIdentifier_ReturnsMaxRequests()
    {
        var strategy = new SlidingWindowStrategy(10, TimeSpan.FromMinutes(1));

        var remaining = strategy.GetRemainingRequests("brand-new-user");

        remaining.Should().Be(10);
    }

    [Fact]
    public void GetRemainingRequests_AfterTwoRequests_DecreasesCorrectly()
    {
        var strategy = new SlidingWindowStrategy(5, TimeSpan.FromMinutes(1));
        strategy.IsRequestAllowed("user1");
        strategy.IsRequestAllowed("user1");

        var remaining = strategy.GetRemainingRequests("user1");

        remaining.Should().Be(3);
    }
}

public sealed class FixedWindowStrategyTests
{
    [Fact]
    public void IsRequestAllowed_FirstRequest_ReturnsTrue()
    {
        var strategy = new FixedWindowStrategy(5, TimeSpan.FromMinutes(1));

        strategy.IsRequestAllowed("client1").Should().BeTrue();
    }

    [Fact]
    public void IsRequestAllowed_AfterExhaustingWindow_ReturnsFalse()
    {
        var strategy = new FixedWindowStrategy(2, TimeSpan.FromMinutes(1));
        strategy.IsRequestAllowed("client1");
        strategy.IsRequestAllowed("client1");

        strategy.IsRequestAllowed("client1").Should().BeFalse();
    }

    [Fact]
    public void GetRemainingRequests_ForNewIdentifier_ReturnsMaxConfigured()
    {
        var strategy = new FixedWindowStrategy(7, TimeSpan.FromMinutes(1));

        strategy.GetRemainingRequests("newcomer").Should().Be(7);
    }

    [Fact]
    public void GetRemainingRequests_AfterSomeRequests_DecrementsCorrectly()
    {
        var strategy = new FixedWindowStrategy(5, TimeSpan.FromMinutes(1));
        strategy.IsRequestAllowed("c1");
        strategy.IsRequestAllowed("c1");

        strategy.GetRemainingRequests("c1").Should().Be(3);
    }

    [Fact]
    public void IsRequestAllowed_DifferentIdentifiers_HaveIndependentWindows()
    {
        var strategy = new FixedWindowStrategy(1, TimeSpan.FromMinutes(1));
        strategy.IsRequestAllowed("clientA");

        strategy.IsRequestAllowed("clientB").Should().BeTrue();
    }
}

public sealed class TokenBucketStrategyTests
{
    [Fact]
    public void IsRequestAllowed_InitiallyWithFullBucket_ReturnsTrue()
    {
        var strategy = new TokenBucketStrategy(bucketCapacity: 10, tokensPerSecond: 1);

        strategy.IsRequestAllowed("user1").Should().BeTrue();
    }

    [Fact]
    public void GetRemainingRequests_ForUnknownIdentifier_ReturnsBucketCapacity()
    {
        var strategy = new TokenBucketStrategy(bucketCapacity: 5, tokensPerSecond: 1);

        strategy.GetRemainingRequests("new-user").Should().Be(5);
    }

    [Fact]
    public void IsRequestAllowed_AfterDepletingBucket_ReturnsFalse()
    {
        var strategy = new TokenBucketStrategy(bucketCapacity: 3, tokensPerSecond: 0);
        for (int i = 0; i < 3; i++)
            strategy.IsRequestAllowed("user");

        strategy.IsRequestAllowed("user").Should().BeFalse();
    }
}

public sealed class ConversationFlowEngineTests
{
    private readonly Mock<ISessionService> _mockSessionService = new();
    private readonly Mock<IEventBus> _mockEventBus = new();
    private readonly Mock<ILogger<ConversationFlowEngine>> _mockLogger = new();
    private readonly ConversationFlowOptions _options = new();
    private readonly ConversationFlowEngine _engine;

    private const long TestUserId = 123;
    private const long TestChatId = 456;
    private const string TestFlowId = "testFlow";

    public ConversationFlowEngineTests()
    {
        _engine = new ConversationFlowEngine(
            _options,
            _mockSessionService.Object,
            _mockEventBus.Object,
            _mockLogger.Object);

        // Setup mock session service
        _mockSessionService
            .Setup(s => s.GetActiveSessionAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((long userId, CancellationToken ct) => new TelegramBotFramework.Models.UserSession
            {
                SessionId = Guid.NewGuid().ToString(),
                UserId = userId,
                ChatId = TestChatId,
                LastActivityAt = DateTime.UtcNow
            });
        _mockSessionService
            .Setup(s => s.UpdateSessionContextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Register a simple flow
        var flow = new FlowDefinition
        {
            FlowId = TestFlowId,
            Name = "Test Flow",
            InitialStepId = "step1",
            Steps = new List<FlowStep>
            {
                new() { StepId = "step1", Prompt = "Prompt 1", InputType = FlowInputType.Text, DefaultNextStepId = "step2" },
                new() { StepId = "step2", Prompt = "Prompt 2", InputType = FlowInputType.Text, DefaultNextStepId = "step3" },
                new() { StepId = "step3", Prompt = "Prompt 3", InputType = FlowInputType.Text, IsTerminal = true }
            }
        };
        _engine.RegisterFlowAsync(flow).Wait();
    }

    [Fact]
    public async Task ProcessInputAsync_ConcurrentUpdatesForSameUser_HistoryIsConsistent()
    {
        const int numberOfConcurrentCalls = 100;
        const string inputPrefix = "input_";

        // Start the flow
        await _engine.StartFlowAsync(TestUserId, TestChatId, TestFlowId).ConfigureAwait(false);

        var tasks = new List<Task>();
        for (int i = 0; i < numberOfConcurrentCalls; i++)
        {
            var input = $"{inputPrefix}{i}";
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await _engine.ProcessInputAsync(TestUserId, input).ConfigureAwait(false);
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("has no active conversation flow"))
                {
                    // Expected race condition — flow may have completed before this call.
                }
            }));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

        var history = (await _engine.GetFlowHistoryAsync(TestUserId, numberOfConcurrentCalls).ConfigureAwait(false)).ToList();

        history.Should().NotBeEmpty();
        history.Count.Should().BeLessOrEqualTo(numberOfConcurrentCalls);

        // Verify history entries are internally consistent (no corrupted entries)
        history.All(s => s.UserId == TestUserId).Should().BeTrue();
    }

        [Fact]
        public async Task StartFlowAsync_ValidFlowId_ReturnsFlowState()
        {
            // Act
            var flowState = await _engine.StartFlowAsync(TestUserId, TestChatId, TestFlowId).ConfigureAwait(false);

            // Assert
            flowState.Should().NotBeNull();
            flowState.FlowId.Should().Be(TestFlowId);
            flowState.CurrentStepId.Should().Be("step1");
            flowState.Status.Should().Be(FlowStateStatus.WaitingForInput);
            flowState.UserId.Should().Be(TestUserId);
            flowState.ChatId.Should().Be(TestChatId);
        }

        [Fact]
        public async Task StartFlowAsync_InvalidFlowId_ThrowsInvalidOperationException()
        {
            // Arrange
            var invalidFlowId = "nonExistentFlow";

            // Act
            Func<Task> act = () => _engine.StartFlowAsync(TestUserId, TestChatId, invalidFlowId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task StartFlowAsync_AbortsExistingFlowForUser()
        {
            // Arrange - start a different flow first
            var existingFlowId = "existingFlow";
            var existingFlow = new FlowDefinition
            {
                FlowId = existingFlowId,
                Name = "Existing Flow",
                InitialStepId = "step1",
                Steps = new List<FlowStep>
                {
                    new() { StepId = "step1", Prompt = "Existing prompt", InputType = FlowInputType.Text, IsTerminal = true }
                }
            };
            await _engine.RegisterFlowAsync(existingFlow);
            await _engine.StartFlowAsync(TestUserId, TestChatId, existingFlowId).ConfigureAwait(false);

            // Act - start a new flow, should abort the old one
            var newFlowState = await _engine.StartFlowAsync(TestUserId, TestChatId, TestFlowId).ConfigureAwait(false);

            // Assert
            newFlowState.Should().NotBeNull();
            newFlowState.FlowId.Should().Be(TestFlowId);
        }

        [Fact]
        public async Task ProcessInputAsync_ValidInput_AdvancesToNextStep()
        {
            // Arrange - start flow
            await _engine.StartFlowAsync(TestUserId, TestChatId, TestFlowId).ConfigureAwait(false);

            // Act - process first step
            var result = await _engine.ProcessInputAsync(TestUserId, "valid input").ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.IsCompleted.Should().BeFalse();
            result.FlowState.CurrentStepId.Should().Be("step2");
            result.Prompt.Should().Be("Prompt 2");
        }

        [Fact]
        public async Task ProcessInputAsync_InvalidInput_ReturnsValidationError()
        {
            // Arrange - start flow
            await _engine.StartFlowAsync(TestUserId, TestChatId, TestFlowId).ConfigureAwait(false);

            // Act - provide empty input (should fail validation)
            var result = await _engine.ProcessInputAsync(TestUserId, "").ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.IsCompleted.Should().BeFalse();
            result.ValidationError.Should().NotBeNullOrEmpty();
            result.Prompt.Should().Contain("Input cannot be empty");
        }

        [Fact]
        public async Task ProcessInputAsync_EmptyInputKeepsCurrentStep()
        {
            // Arrange - start flow
            var initialState = await _engine.StartFlowAsync(TestUserId, TestChatId, TestFlowId).ConfigureAwait(false);
            var initialStepId = initialState.CurrentStepId;

            // Act - provide invalid input
            await _engine.ProcessInputAsync(TestUserId, "").ConfigureAwait(false);

            // Assert - step should remain the same
            var currentState = await _engine.GetActiveFlowStateAsync(TestUserId).ConfigureAwait(false);
            currentState.Should().NotBeNull();
            currentState!.CurrentStepId.Should().Be(initialStepId);
        }

        [Fact]
        public async Task CompleteFlow_ReachesTerminalStep()
        {
            // Arrange - start flow
            await _engine.StartFlowAsync(TestUserId, TestChatId, TestFlowId).ConfigureAwait(false);

            // Act - complete all steps
            var result1 = await _engine.ProcessInputAsync(TestUserId, "data1").ConfigureAwait(false);
            result1.IsCompleted.Should().BeFalse();

            var result2 = await _engine.ProcessInputAsync(TestUserId, "data2").ConfigureAwait(false);
            result2.IsCompleted.Should().BeFalse();

            var result3 = await _engine.ProcessInputAsync(TestUserId, "data3").ConfigureAwait(false);
            result3.IsCompleted.Should().BeTrue();

            // Assert
            result3.IsValid.Should().BeTrue();
            result3.IsCompleted.Should().BeTrue();
            result3.Prompt.Should().Be("Completed.");
        }

        [Fact]
        public async Task AbortFlowAsync_CancelsActiveFlow()
        {
            // Arrange - start flow
            await _engine.StartFlowAsync(TestUserId, TestChatId, TestFlowId).ConfigureAwait(false);

            // Act
            await _engine.AbortFlowAsync(TestUserId, "Test abort").ConfigureAwait(false);

            // Assert
            var state = await _engine.GetActiveFlowStateAsync(TestUserId).ConfigureAwait(false);
            state.Should().BeNull();
        }

        [Fact]
        public async Task GetActiveFlowStateAsync_UserHasActiveFlow_ReturnsState()
        {
            // Arrange - start flow
            var expectedState = await _engine.StartFlowAsync(TestUserId, TestChatId, TestFlowId).ConfigureAwait(false);

            // Act
            var state = await _engine.GetActiveFlowStateAsync(TestUserId).ConfigureAwait(false);

            // Assert
            state.Should().NotBeNull();
            state!.UserId.Should().Be(TestUserId);
            state.FlowId.Should().Be(TestFlowId);
            state.Should().BeEquivalentTo(expectedState);
        }

        [Fact]
        public async Task GetActiveFlowStateAsync_UserHasNoActiveFlow_ReturnsNull()
        {
            // Act
            var state = await _engine.GetActiveFlowStateAsync(999).ConfigureAwait(false);

            // Assert
            state.Should().BeNull();
        }

        [Fact]
        public async Task IsUserInFlowAsync_UserHasActiveFlow_ReturnsTrue()
        {
            // Arrange - start flow
            await _engine.StartFlowAsync(TestUserId, TestChatId, TestFlowId).ConfigureAwait(false);

            // Act
            var isInFlow = await _engine.IsUserInFlowAsync(TestUserId).ConfigureAwait(false);

            // Assert
            isInFlow.Should().BeTrue();
        }

        [Fact]
        public async Task IsUserInFlowAsync_UserHasNoActiveFlow_ReturnsFalse()
        {
            // Act
            var isInFlow = await _engine.IsUserInFlowAsync(999).ConfigureAwait(false);

            // Assert
            isInFlow.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllFlowsAsync_ReturnsRegisteredFlows()
        {
            // Act
            var flows = await _engine.GetAllFlowsAsync().ConfigureAwait(false);

            // Assert
            flows.Should().NotBeEmpty();
            flows.Should().ContainSingle(f => f.FlowId == TestFlowId);
        }

        [Fact]
        public async Task GetFlowAsync_ExistingFlowId_ReturnsFlowDefinition()
        {
            // Act
            var flow = await _engine.GetFlowAsync(TestFlowId).ConfigureAwait(false);

            // Assert
            flow.Should().NotBeNull();
            flow!.FlowId.Should().Be(TestFlowId);
        }

        [Fact]
        public async Task GetFlowAsync_NonExistentFlowId_ReturnsNull()
        {
            // Act
            var flow = await _engine.GetFlowAsync("nonExistentFlow").ConfigureAwait(false);

            // Assert
            flow.Should().BeNull();
        }

        [Fact]
        public async Task UnregisterFlowAsync_RemovesFlowDefinition()
        {
            // Act
            await _engine.UnregisterFlowAsync(TestFlowId).ConfigureAwait(false);

            // Assert
            var flow = await _engine.GetFlowAsync(TestFlowId).ConfigureAwait(false);
            flow.Should().BeNull();
        }
    }
