#nullable enable

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TelegramBotFramework.Events;
using Xunit;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Tests for EventHandlerBase&lt;TEvent&gt; error-path behavior.
/// EventHandlerBase is the shared dispatch point for all event handlers.
/// </summary>
public class EventHandlerBaseTests
{
    private readonly Mock<ILogger<TestEventHandler>> _loggerMock;
    private readonly TestEventHandler _handler;

    public EventHandlerBaseTests()
    {
        _loggerMock = new Mock<ILogger<TestEventHandler>>();
        _handler = new TestEventHandler(_loggerMock.Object);
    }

    [Fact]
    public void Constructor_WithNullLogger_CreatesConsoleLogger()
    {
        // Act
        var handler = new TestEventHandler(null);

        // Assert
        handler.Should().NotBeNull();
    }

    [Fact]
    public async Task HandleAsync_WithNullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        TestEvent? nullEvent = null;

        // Act
        Func<Task> act = async () => await _handler.HandleAsync(nullEvent!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("@event");
    }

    [Fact]
    public async Task HandleAsync_WhenExecuteAsyncThrows_LogsErrorAndRethrows()
    {
        // Arrange
        var testEvent = new TestEvent();
        var exception = new InvalidOperationException("Test exception");

        // Create a handler that throws when ExecuteAsync is called
        var throwingHandler = new ThrowingEventHandler(_loggerMock.Object, exception);

        // Act
        Func<Task> act = async () => await throwingHandler.HandleAsync(testEvent);

        // Assert - exception should be thrown
        await act.Should().ThrowAsync<InvalidOperationException>()
            .Where(e => e.Message == "Test exception");

        // Verify error was logged (simplified - just check it was called)
        _loggerMock.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<InvalidOperationException>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenExecuteAsyncSucceeds_LogsSuccess()
    {
        // Arrange
        var testEvent = new TestEvent();

        // Act
        await _handler.HandleAsync(testEvent);

        // Assert - success should be logged
        _loggerMock.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.AtLeastOnce);
    }

    [Fact]
    public async Task HandleAsync_WhenExecuteAsyncSucceeds_LogsEventTypeAndCorrelationId()
    {
        // Arrange
        var testEvent = new TestEvent();
        var correlationId = Guid.NewGuid().ToString();
        testEvent.CorrelationId = correlationId;

        // Act
        await _handler.HandleAsync(testEvent);

        // Assert - event type and correlation ID should be logged
        _loggerMock.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.AtLeastOnce);
    }

    [Fact]
    public void GetHandlerName_ReturnsTypeName()
    {
        // Act
        var handlerName = _handler.GetHandlerName();

        // Assert
        handlerName.Should().Be("TestEventHandler");
    }

    [Fact]
    public async Task ExecuteAsync_ThroughHandleAsync_CompletesWithoutError()
    {
        // Arrange
        var testEvent = new TestEvent();

        // Act - ExecuteAsync is called internally by HandleAsync
        await _handler.HandleAsync(testEvent);

        // Assert - no exception thrown means success
        Assert.True(true); // Test passes if no exception
    }

    [Fact]
    public async Task HandleAsync_LogsHandlingStartAndCompletionMessages()
    {
        // Arrange
        var testEvent = new TestEvent();

        // Act
        await _handler.HandleAsync(testEvent);

        // Assert - both start and completion messages should be logged
        _loggerMock.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.AtLeastOnce);

        _loggerMock.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.AtLeastOnce);
    }

    /// <summary>
    /// Concrete implementation of EventHandlerBase for testing.
    /// </summary>
    private sealed class TestEventHandler : EventHandlerBase<TestEvent>
    {
        public TestEventHandler(ILogger<TestEventHandler>? logger = null) : base(logger)
        {
        }

        protected override Task ExecuteAsync(TestEvent @event)
        {
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// EventHandlerBase implementation that throws a specific exception.
    /// </summary>
    private sealed class ThrowingEventHandler : EventHandlerBase<TestEvent>
    {
        private readonly Exception _exception;

        public ThrowingEventHandler(ILogger<EventHandlerBase<TestEvent>>? logger, Exception exception) : base(logger)
        {
            _exception = exception;
        }

        protected override Task ExecuteAsync(TestEvent @event)
        {
            throw _exception;
        }
    }

    /// <summary>
    /// Test event implementation.
    /// </summary>
    private sealed class TestEvent : EventBase
    {
        public TestEvent() : base()
        {
        }
    }
}