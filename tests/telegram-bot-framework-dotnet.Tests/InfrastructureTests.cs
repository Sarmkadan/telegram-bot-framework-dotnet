// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TelegramBotFramework.Caching;
using TelegramBotFramework.Events;
using TelegramBotFramework.Strategies;
using Xunit;

namespace TelegramBotFramework.Tests;

public class LocalCacheProviderTests
{
    private readonly LocalCacheProvider _cache = new();

    [Fact]
    public async Task SetAsync_ThenGetAsync_ReturnsStoredValue()
    {
        await _cache.SetAsync("greeting", "hello");

        var result = await _cache.GetAsync<string>("greeting");

        result.Should().Be("hello");
    }

    [Fact]
    public async Task GetAsync_WhenKeyDoesNotExist_ReturnsDefault()
    {
        var result = await _cache.GetAsync<string>("missing-key");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_WhenEntryHasExpired_ReturnsDefault()
    {
        await _cache.SetAsync("expiring", "value", TimeSpan.FromMilliseconds(1));
        await Task.Delay(50);

        var result = await _cache.GetAsync<string>("expiring");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_WhenEntryNotExpired_ReturnsValue()
    {
        await _cache.SetAsync("persistent", "alive", TimeSpan.FromHours(1));

        var result = await _cache.GetAsync<string>("persistent");

        result.Should().Be("alive");
    }

    [Fact]
    public async Task RemoveAsync_ExistingKey_MakesValueUnavailable()
    {
        await _cache.SetAsync("toRemove", 42);

        await _cache.RemoveAsync("toRemove");

        var exists = await _cache.ExistsAsync("toRemove");
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WhenKeyPresent_ReturnsTrue()
    {
        await _cache.SetAsync("present", true);

        var exists = await _cache.ExistsAsync("present");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenKeyNotPresent_ReturnsFalse()
    {
        var exists = await _cache.ExistsAsync("not-there");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WhenEntryExpired_ReturnsFalse()
    {
        await _cache.SetAsync("gone-soon", "x", TimeSpan.FromMilliseconds(1));
        await Task.Delay(50);

        var exists = await _cache.ExistsAsync("gone-soon");

        exists.Should().BeFalse();
    }

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

    [Fact]
    public async Task GetOrCreateAsync_WhenKeyExists_SkipsFactoryAndReturnsCached()
    {
        await _cache.SetAsync("existing", "cached-value");
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

    [Fact]
    public async Task FlushAsync_ClearsAllCachedEntries()
    {
        await _cache.SetAsync("a", 1);
        await _cache.SetAsync("b", 2);
        await _cache.SetAsync("c", 3);

        await _cache.FlushAsync();

        var stats = await _cache.GetStatisticsAsync();
        stats.ItemCount.Should().Be(0);
    }

    [Fact]
    public async Task GetStatisticsAsync_TracksCacheHitsAndMisses()
    {
        await _cache.SetAsync("tracked", "x");
        await _cache.GetAsync<string>("tracked");
        await _cache.GetAsync<string>("non-existent");

        var stats = await _cache.GetStatisticsAsync();

        stats.HitCount.Should().BeGreaterThanOrEqualTo(1);
        stats.MissCount.Should().BeGreaterThanOrEqualTo(1);
        stats.SetCount.Should().BeGreaterThanOrEqualTo(1);
    }
}

public class EventBusTests
{
    private readonly Mock<ILogger<EventBus>> _mockLogger = new();
    private readonly EventBus _bus;

    public EventBusTests()
    {
        _bus = new EventBus(_mockLogger.Object);
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
    public void Unsubscribe_RemovesHandlerAndDecrementsCount()
    {
        var handler = new TestMessageHandler();
        _bus.Subscribe<MessageReceivedEvent>(handler);

        _bus.Unsubscribe<MessageReceivedEvent>(handler);

        _bus.GetSubscriberCount<MessageReceivedEvent>().Should().Be(0);
    }

    [Fact]
    public async Task PublishAsync_WithSubscribedHandler_InvokesHandlerWithCorrectPayload()
    {
        var handler = new TestMessageHandler();
        _bus.Subscribe<MessageReceivedEvent>(handler);
        var evt = new MessageReceivedEvent(chatId: 100L, userId: 200L, messageText: "Hello");

        await _bus.PublishAsync(evt);

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

        await _bus.PublishAsync(new MessageReceivedEvent(1, 2, "broadcast"));

        handler1.Received.Should().HaveCount(1);
        handler2.Received.Should().HaveCount(1);
    }

    [Fact]
    public async Task PublishAsync_WithNoSubscribers_CompletesWithoutThrowing()
    {
        var act = async () => await _bus.PublishAsync(new MessageReceivedEvent(1, 2, "hi"));

        await act.Should().NotThrowAsync();
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
    public async Task PublishAsync_LogsAtLeastOneInformationMessage()
    {
        _bus.Subscribe<MessageReceivedEvent>(new TestMessageHandler());

        await _bus.PublishAsync(new MessageReceivedEvent(1, 2, "log-test"));

        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
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
}

public class SlidingWindowStrategyTests
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

public class FixedWindowStrategyTests
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

public class TokenBucketStrategyTests
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
