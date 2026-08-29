#nullable enable

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TelegramBotFramework.Events;
using Xunit;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Tests for EventBusExtensions class.
/// Tests edge cases for event publishing scenarios including zero handlers, exception handling,
/// and middleware execution.
/// </summary>
public class EventBusExtensionsTests : IEventBusExtensionsTests
{
    private readonly Mock<ILogger<EventBus>> _loggerMock;
    private readonly EventBus _eventBus;

    public EventBusExtensionsTests()
    {
        _loggerMock = new Mock<ILogger<EventBus>>();
        _eventBus = new EventBus(_loggerMock.Object);
    }

    [Fact]
    public void IsEventRegistered_WithNullBus_ThrowsArgumentNullException()
    {
        // Arrange
        EventBus? nullBus = null;

        // Act
        var act = () => nullBus!.IsEventRegistered<TestEvent>();

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("bus");
    }

    [Fact]
    public void IsEventRegistered_WhenEventTypeNotRegistered_ReturnsFalse()
    {
        // Act
        var result = _eventBus.IsEventRegistered<TestEvent>();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsEventRegistered_WhenEventTypeRegistered_ReturnsTrue()
    {
        // Arrange
        var handler = new TestEventHandler();
        _eventBus.Subscribe(handler);

        // Act
        var result = _eventBus.IsEventRegistered<TestEvent>();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetTotalSubscriberCount_WithNullBus_ThrowsArgumentNullException()
    {
        // Arrange
        EventBus? nullBus = null;

        // Act
        var act = () => nullBus!.GetTotalSubscriberCount();

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("bus");
    }

    [Fact]
    public void GetTotalSubscriberCount_WhenNoHandlers_ReturnsZero()
    {
        // Act
        var result = _eventBus.GetTotalSubscriberCount();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetTotalSubscriberCount_WithSingleHandler_ReturnsOne()
    {
        // Arrange
        var handler = new TestEventHandler();
        _eventBus.Subscribe(handler);

        // Act
        var result = _eventBus.GetTotalSubscriberCount();

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void GetTotalSubscriberCount_WithMultipleHandlers_ReturnsCorrectCount()
    {
        // Arrange
        var handler1 = new TestEventHandler();
        var handler2 = new TestEventHandler();
        var handler3 = new TestEventHandler();
        _eventBus.Subscribe(handler1);
        _eventBus.Subscribe(handler2);
        _eventBus.Subscribe(handler3);

        // Act
        var result = _eventBus.GetTotalSubscriberCount();

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public void GetTotalSubscriberCount_WithMultipleEventTypes_ReturnsSumOfAllHandlers()
    {
        // Arrange
        var handler1 = new TestEventHandler();
        var handler2 = new AnotherTestEventHandler();
        _eventBus.Subscribe(handler1);
        _eventBus.Subscribe(handler2);

        // Act
        var result = _eventBus.GetTotalSubscriberCount();

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public void UseMiddleware_WithNullBus_ThrowsArgumentNullException()
    {
        // Arrange
        IEventBus? nullBus = null;
        var middleware = new TestMiddleware();

        // Act
        var act = () => nullBus!.UseMiddleware(middleware);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("bus");
    }

    [Fact]
    public void UseMiddleware_WithNullMiddleware_ThrowsArgumentNullException()
    {
        // Arrange
        var bus = _eventBus;
        IEventMiddleware? nullMiddleware = null;

        // Act
        var act = () => bus.UseMiddleware(nullMiddleware!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("middleware");
    }

    [Fact]
    public void UseMiddleware_WithSingleMiddleware_ReturnsBusForChaining()
    {
        // Arrange
        var bus = _eventBus;
        var middleware = new TestMiddleware();

        // Act
        var result = bus.UseMiddleware(middleware);

        // Assert
        result.Should().BeSameAs(bus);
        _eventBus.GetMiddleware().Should().Contain(middleware);
    }

    [Fact]
    public void UseMiddleware_WithMultipleMiddleware_ReturnsBusForChaining()
    {
        // Arrange
        var bus = _eventBus;
        var middleware1 = new TestMiddleware();
        var middleware2 = new TestMiddleware();
        var middleware3 = new TestMiddleware();

        // Act
        var result = bus.UseMiddleware(middleware1, middleware2, middleware3);

        // Assert
        result.Should().BeSameAs(bus);
        _eventBus.GetMiddleware().Should().HaveCount(3);
        _eventBus.GetMiddleware().Should().Contain(middleware1);
        _eventBus.GetMiddleware().Should().Contain(middleware2);
        _eventBus.GetMiddleware().Should().Contain(middleware3);
    }

    [Fact]
    public void UseMiddleware_WithDuplicateMiddleware_DoesNotAddDuplicate()
    {
        // Arrange
        var bus = _eventBus;
        var middleware = new TestMiddleware();
        bus.UseMiddleware(middleware);

        // Act - add the same middleware again
        var result = bus.UseMiddleware(middleware);

        // Assert
        result.Should().BeSameAs(bus);
        _eventBus.GetMiddleware().Should().HaveCount(1);
    }

    [Fact]
    public async Task PublishAsync_WithNoHandlers_ShouldNotThrow()
    {
        // Arrange
        var testEvent = new TestEvent();

        // Act - should not throw even with no handlers
        var act = async () => await _eventBus.PublishAsync(testEvent);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PublishAsync_WithNoHandlers_ShouldLogWarning()
    {
        // Arrange
        var testEvent = new TestEvent();

        // Act
        await _eventBus.PublishAsync(testEvent);

        // Assert
        _loggerMock.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No subscribers for event")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WithMultipleHandlers_AllHandlersAreInvoked()
    {
        // Arrange
        var handler1Invoked = false;
        var handler2Invoked = false;
        var handler3Invoked = false;

        var handler1 = new DelegateEventHandler<TestEvent>(_ =>
        {
            handler1Invoked = true;
            return Task.CompletedTask;
        });

        var handler2 = new DelegateEventHandler<TestEvent>(_ =>
        {
            handler2Invoked = true;
            return Task.CompletedTask;
        });

        var handler3 = new DelegateEventHandler<TestEvent>(_ =>
        {
            handler3Invoked = true;
            return Task.CompletedTask;
        });

        _eventBus.Subscribe(handler1);
        _eventBus.Subscribe(handler2);
        _eventBus.Subscribe(handler3);

        var testEvent = new TestEvent();

        // Act
        await _eventBus.PublishAsync(testEvent);

        // Assert - all handlers should have been invoked
        handler1Invoked.Should().BeTrue();
        handler2Invoked.Should().BeTrue();
        handler3Invoked.Should().BeTrue();
    }

    [Fact]
    public async Task PublishAsync_WhenOneHandlerThrows_OtherHandlersStillExecute()
    {
        // Arrange
        var handler1Invoked = false;
        var handler2Invoked = false;
        var handler3Invoked = false;

        var handler1 = new DelegateEventHandler<TestEvent>(_ =>
        {
            handler1Invoked = true;
            return Task.CompletedTask;
        });

        var handler2 = new ThrowingEventHandler();

        var handler3 = new DelegateEventHandler<TestEvent>(_ =>
        {
            handler3Invoked = true;
            return Task.CompletedTask;
        });

        _eventBus.Subscribe(handler1);
        _eventBus.Subscribe(handler2);
        _eventBus.Subscribe(handler3);

        var testEvent = new TestEvent();

        // Act - should not throw, all handlers should execute
        var act = async () => await _eventBus.PublishAsync(testEvent);

        // Assert - should not throw
        await act.Should().NotThrowAsync();

        // Assert - all handlers should have been invoked (exception doesn't stop remaining handlers)
        handler1Invoked.Should().BeTrue();
        handler2Invoked.Should().BeTrue();
        handler3Invoked.Should().BeTrue();
    }

    [Fact]
    public async Task PublishAsync_WithMiddleware_ExecutesMiddlewareInOrder()
    {
        // Arrange
        var executionOrder = new List<string>();

        var middleware1 = new DelegateMiddleware(async (_, next) =>
        {
            executionOrder.Add("Middleware1-Before");
            await next();
            executionOrder.Add("Middleware1-After");
        });

        var middleware2 = new DelegateMiddleware(async (_, next) =>
        {
            executionOrder.Add("Middleware2-Before");
            await next();
            executionOrder.Add("Middleware2-After");
        });

        var handlerInvoked = false;
        var handler = new DelegateEventHandler<TestEvent>(_ =>
        {
            handlerInvoked = true;
            return Task.CompletedTask;
        });

        _eventBus.UseMiddleware(middleware1, middleware2);
        _eventBus.Subscribe(handler);

        var testEvent = new TestEvent();

        // Act
        await _eventBus.PublishAsync(testEvent);

        // Assert - middleware should execute in correct order
        executionOrder.Should().BeEquivalentTo([
            "Middleware1-Before",
            "Middleware2-Before",
            "Middleware1-After",
            "Middleware2-After"
        ]);
        handlerInvoked.Should().BeTrue();
    }

    /// <summary>
    /// Test middleware implementation that delegates to a function.
    /// </summary>
    private sealed class DelegateMiddleware : IEventMiddleware
    {
        private readonly Func<IEvent, Func<Task>, Task> _invokeFunc;

        public DelegateMiddleware(Func<IEvent, Func<Task>, Task> invokeFunc)
        {
            _invokeFunc = invokeFunc;
            MiddlewareName = "DelegateMiddleware";
        }

        public string MiddlewareName { get; }

        public Task InvokeAsync(IEvent evt, Func<Task> next) => _invokeFunc(evt, next);
    }

    /// <summary>
    /// Test event handler implementation.
    /// </summary>
    private sealed class TestEventHandler : EventHandlerBase<TestEvent>
    {
        public TestEventHandler(ILogger<TestEventHandler>? logger = null) : base(logger) { }

        protected override Task ExecuteAsync(TestEvent @event) => Task.CompletedTask;
    }

    /// <summary>
    /// Another test event handler for testing multiple event types.
    /// </summary>
    private sealed class AnotherTestEventHandler : EventHandlerBase<AnotherTestEvent>
    {
        public AnotherTestEventHandler(ILogger<AnotherTestEventHandler>? logger = null) : base(logger) { }

        protected override Task ExecuteAsync(AnotherTestEvent @event) => Task.CompletedTask;
    }

    /// <summary>
    /// Test middleware implementation.
    /// </summary>
    private sealed class TestMiddleware : IEventMiddleware
    {
        public string MiddlewareName => nameof(TestMiddleware);

        public Task InvokeAsync(IEvent evt, Func<Task> next) => next();
    }

    /// <summary>
    /// Delegate-based event handler for testing.
    /// </summary>
    private sealed class DelegateEventHandler<TEvent> : IEventHandler<TEvent> where TEvent : class, IEvent
    {
        private readonly Func<TEvent, Task> _handler;

        public DelegateEventHandler(Func<TEvent, Task> handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public Task HandleAsync(TEvent @event) => _handler(@event);
    }

    /// <summary>
    /// Test event implementation.
    /// </summary>
    private sealed class TestEvent : EventBase
    {
        public TestEvent() : base() { }
    }

    /// <summary>
    /// Another test event implementation.
    /// </summary>
    private sealed class AnotherTestEvent : EventBase
    {
        public AnotherTestEvent() : base() { }
    }

    /// <summary>
    /// Event handler that throws an exception.
    /// </summary>
    private sealed class ThrowingEventHandler : EventHandlerBase<TestEvent>
    {
        public ThrowingEventHandler(ILogger<ThrowingEventHandler>? logger = null) : base(logger) { }

        protected override Task ExecuteAsync(TestEvent @event)
        {
            throw new InvalidOperationException("Handler failed");
        }
    }
}
