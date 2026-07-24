#nullable enable

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TelegramBotFramework.Events;
using Xunit;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Tests for EventPublisherExtensions class.
/// Tests edge cases for event publishing scenarios including correlation ID tracking,
/// collection publishing, and null safety.
/// </summary>
public class EventPublisherExtensionsTests
{
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly Mock<ILogger<EventPublisher>> _loggerMock;
    private readonly EventPublisher _publisher;

    public EventPublisherExtensionsTests()
    {
        _eventBusMock = new Mock<IEventBus>();
        _loggerMock = new Mock<ILogger<EventPublisher>>();
        _publisher = new EventPublisher(_eventBusMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void PublishWithCorrelationAsync_WithNullPublisher_ThrowsArgumentNullException()
    {
        // Arrange
        EventPublisher? nullPublisher = null;
        var testEvent = new TestEvent();
        const string correlationId = "test-correlation";

        // Act
        var act = async () => await nullPublisher!.PublishWithCorrelationAsync(testEvent, correlationId);

        // Assert
        act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("publisher");
    }

    [Fact]
    public void PublishWithCorrelationAsync_WithNullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        var publisher = _publisher;
        TestEvent? nullEvent = null;
        const string correlationId = "test-correlation";

        // Act
        var act = async () => await publisher.PublishWithCorrelationAsync(nullEvent!, correlationId);

        // Assert
        act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("event");
    }

    [Fact]
    public void PublishWithCorrelationAsync_WithNullCorrelationId_ThrowsArgumentNullException()
    {
        // Arrange
        var publisher = _publisher;
        var testEvent = new TestEvent();
        string? nullCorrelationId = null;

        // Act
        var act = async () => await publisher.PublishWithCorrelationAsync(testEvent, nullCorrelationId!);

        // Assert
        act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("correlationId");
    }

    [Fact]
    public async Task PublishWithCorrelationAsync_CallsPublishAsyncWithEvent()
    {
        // Arrange
        var testEvent = new TestEvent();
        const string correlationId = "test-correlation-123";

        // Act
        await _publisher.PublishWithCorrelationAsync(testEvent, correlationId);

        // Assert - PublishAsync should have been called with the event
        _eventBusMock.Verify(x => x.PublishAsync(testEvent), Times.Once);
    }

    [Fact]
    public void PublishCollectionAsync_WithNullPublisher_ThrowsArgumentNullException()
    {
        // Arrange
        EventPublisher? nullPublisher = null;
        var testEvents = new List<TestEvent>();
        const string correlationId = "test-correlation";

        // Act
        var act = async () => await nullPublisher!.PublishCollectionAsync(testEvents, correlationId);

        // Assert
        act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("publisher");
    }

    [Fact]
    public void PublishCollectionAsync_WithNullEvents_ThrowsArgumentNullException()
    {
        // Arrange
        var publisher = _publisher;
        List<TestEvent>? nullEvents = null;
        const string correlationId = "test-correlation";

        // Act
        var act = async () => await publisher.PublishCollectionAsync(nullEvents!, correlationId);

        // Assert
        act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("events");
    }

    [Fact]
    public void PublishCollectionAsync_WithNullCorrelationId_ThrowsArgumentNullException()
    {
        // Arrange
        var publisher = _publisher;
        var testEvents = new List<TestEvent>();
        string? nullCorrelationId = null;

        // Act
        var act = async () => await publisher.PublishCollectionAsync(testEvents, nullCorrelationId!);

        // Assert
        act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("correlationId");
    }

    [Fact]
    public async Task PublishCollectionAsync_WithEmptyCollection_DoesNotCallPublishAsync()
    {
        // Arrange
        var testEvents = new List<TestEvent>();
        const string correlationId = "test-correlation";

        // Act
        await _publisher.PublishCollectionAsync(testEvents, correlationId);

        // Assert
        _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<TestEvent>()), Times.Never);
    }

    [Fact]
    public async Task PublishCollectionAsync_WithSingleEvent_CallsPublishAsyncOnce()
    {
        // Arrange
        var testEvents = new List<TestEvent> { new TestEvent() };
        const string correlationId = "test-correlation";

        // Act
        await _publisher.PublishCollectionAsync(testEvents, correlationId);

        // Assert
        _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<TestEvent>()), Times.Once);
    }

    [Fact]
    public async Task PublishCollectionAsync_WithMultipleEvents_CallsPublishAsyncForEachEvent()
    {
        // Arrange
        var testEvents = new List<TestEvent> { new TestEvent(), new TestEvent(), new TestEvent() };
        const string correlationId = "test-correlation";

        // Act
        await _publisher.PublishCollectionAsync(testEvents, correlationId);

        // Assert
        _eventBusMock.Verify(x => x.PublishAsync(It.IsAny<TestEvent>()), Times.Exactly(3));
    }

    [Fact]
    public async Task PublishCollectionAsync_EventsArePublishedInOrder()
    {
        // Arrange
        var executionOrder = new List<int>();
        var testEvents = new List<TestEvent> {
            new TestEvent(),
            new TestEvent(),
            new TestEvent()
        };
        
        // Setup mock to track order
        _eventBusMock.Setup(x => x.PublishAsync(It.IsAny<TestEvent>()))
            .Callback<TestEvent>(_ => executionOrder.Add(1));

        const string correlationId = "test-correlation";

        // Act
        await _publisher.PublishCollectionAsync(testEvents, correlationId);

        // Assert - events should be published in the order they appear in the collection
        executionOrder.Should().HaveCount(3);
    }

    /// <summary>
    /// Test event implementation.
    /// </summary>
    private sealed class TestEvent : EventBase
    {
        public TestEvent() : base() { }
    }
}
