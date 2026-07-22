#nullable enable

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TelegramBotFramework.Events;
using Xunit;

namespace TelegramBotFramework.Tests;

public class EventPublisherTests
{
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly Mock<ILogger<EventPublisher>> _loggerMock;
    private readonly EventPublisher _publisher;

    public EventPublisherTests()
    {
        _eventBusMock = new Mock<IEventBus>();
        _loggerMock = new Mock<ILogger<EventPublisher>>();
        _publisher = new EventPublisher(_eventBusMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void Constructor_WithNullEventBus_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new EventPublisher(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("eventBus");
    }

    [Fact]
    public void Constructor_WithNullLogger_CreatesConsoleLogger()
    {
        // Act
        var publisher = new EventPublisher(_eventBusMock.Object, null);

        // Assert
        publisher.Should().NotBeNull();
    }

    [Fact]
    public void WithCorrelationId_SetsCorrelationIdAndReturnsPublisher()
    {
        // Arrange
        const string expectedCorrelationId = "test-correlation-123";

        // Act
        var result = _publisher.WithCorrelationId(expectedCorrelationId);

        // Assert
        result.Should().BeSameAs(_publisher);

        // Verify by calling a method that uses correlation ID
        _eventBusMock.Reset();
        _publisher.PublishMessageReceivedAsync(123, 456, "test");

        _eventBusMock.Verify(x => x.PublishAsync(It.Is<MessageReceivedEvent>(e =>
            e.CorrelationId == expectedCorrelationId)),
            Times.Once);
    }

    [Fact]
    public void WithCorrelationId_MultipleCalls_OverwritesPreviousValue()
    {
        // Arrange
        const string firstCorrelationId = "first-id";
        const string secondCorrelationId = "second-id";

        // Act
        _publisher.WithCorrelationId(firstCorrelationId);
        _publisher.WithCorrelationId(secondCorrelationId);

        // Assert
        _eventBusMock.Reset();
        _publisher.PublishMessageReceivedAsync(123, 456, "test");

        _eventBusMock.Verify(x => x.PublishAsync(It.Is<MessageReceivedEvent>(e =>
            e.CorrelationId == secondCorrelationId)),
            Times.Once);
    }

    [Fact]
    public async Task PublishMessageReceivedAsync_CallsEventBusWithCorrectEvent()
    {
        // Arrange
        const long chatId = 12345;
        const long userId = 67890;
        const string messageText = "Hello, world!";

        // Act
        await _publisher.PublishMessageReceivedAsync(chatId, userId, messageText);

        // Assert
        _eventBusMock.Verify(x => x.PublishAsync(It.Is<MessageReceivedEvent>(e =>
            e.ChatId == chatId &&
            e.UserId == userId &&
            e.MessageText == messageText &&
            e.EventType == "MessageReceivedEvent")),
            Times.Once);
    }

    [Fact]
    public async Task PublishMessageReceivedAsync_WithNullMessageText_SetsMessageTextToNull()
    {
        // Arrange
        const long chatId = 12345;
        const long userId = 67890;

        // Act
        await _publisher.PublishMessageReceivedAsync(chatId, userId, null);

        // Assert
        _eventBusMock.Verify(x => x.PublishAsync(It.Is<MessageReceivedEvent>(e =>
            e.MessageText == null)),
            Times.Once);
    }

    [Fact]
    public async Task PublishMessageReceivedAsync_WithEmptyMessageText_SetsMessageTextToEmpty()
    {
        // Arrange
        const long chatId = 12345;
        const long userId = 67890;
        const string emptyMessage = "";

        // Act
        await _publisher.PublishMessageReceivedAsync(chatId, userId, emptyMessage);

        // Assert
        _eventBusMock.Verify(x => x.PublishAsync(It.Is<MessageReceivedEvent>(e =>
            e.MessageText == emptyMessage)),
            Times.Once);
    }

    [Fact]
    public async Task PublishMessageReceivedAsync_WithCorrelationId_SetsCorrelationIdOnEvent()
    {
        // Arrange
        const string correlationId = "test-correlation";
        const long chatId = 12345;
        const long userId = 67890;
        const string messageText = "test message";

        _publisher.WithCorrelationId(correlationId);

        // Act
        await _publisher.PublishMessageReceivedAsync(chatId, userId, messageText);

        // Assert
        _eventBusMock.Verify(x => x.PublishAsync(It.Is<MessageReceivedEvent>(e =>
            e.CorrelationId == correlationId)),
            Times.Once);
    }

    [Fact]
    public async Task PublishCommandExecutedAsync_CallsEventBusWithCorrectEvent()
    {
        // Arrange
        const string commandName = "start";
        const long userId = 12345;
        const string arguments = "arg1 arg2";
        const bool success = true;
        const string errorMessage = null;

        // Act
        await _publisher.PublishCommandExecutedAsync(commandName, userId, arguments, success, errorMessage);

        // Assert
        _eventBusMock.Verify(x => x.PublishAsync(It.Is<CommandExecutedEvent>(e =>
            e.CommandName == commandName &&
            e.UserId == userId &&
            e.Arguments == arguments &&
            e.Success == success &&
            e.ErrorMessage == errorMessage &&
            e.EventType == "CommandExecutedEvent")),
            Times.Once);
    }

    [Fact]
    public async Task PublishCommandExecutedAsync_WithErrorMessage_SetsErrorMessage()
    {
        // Arrange
        const string commandName = "start";
        const long userId = 12345;
        const string arguments = "arg1 arg2";
        const bool success = false;
        const string errorMessage = "Command failed";

        // Act
        await _publisher.PublishCommandExecutedAsync(commandName, userId, arguments, success, errorMessage);

        // Assert
        _eventBusMock.Verify(x => x.PublishAsync(It.Is<CommandExecutedEvent>(e =>
            e.Success == success &&
            e.ErrorMessage == errorMessage)),
            Times.Once);
    }

    [Fact]
    public async Task PublishCommandExecutedAsync_WithNullArguments_SetsArgumentsToNull()
    {
        // Arrange
        const string commandName = "start";
        const long userId = 12345;
        const string? arguments = null;
        const bool success = true;

        // Act
        await _publisher.PublishCommandExecutedAsync(commandName, userId, arguments, success);

        // Assert
        _eventBusMock.Verify(x => x.PublishAsync(It.Is<CommandExecutedEvent>(e =>
            e.Arguments == null)),
            Times.Once);
    }

    [Fact]
    public async Task PublishCommandExecutedAsync_WithEmptyArguments_SetsArgumentsToEmpty()
    {
        // Arrange
        const string commandName = "start";
        const long userId = 12345;
        const string emptyArguments = "";
        const bool success = true;

        // Act
        await _publisher.PublishCommandExecutedAsync(commandName, userId, emptyArguments, success);

        // Assert
        _eventBusMock.Verify(x => x.PublishAsync(It.Is<CommandExecutedEvent>(e =>
            e.Arguments == emptyArguments)),
            Times.Once);
    }

    [Fact]
    public async Task PublishCommandExecutedAsync_WithCorrelationId_SetsCorrelationIdOnEvent()
    {
        // Arrange
        const string correlationId = "test-correlation";
        const string commandName = "start";
        const long userId = 12345;
        const bool success = true;

        _publisher.WithCorrelationId(correlationId);

        // Act
        await _publisher.PublishCommandExecutedAsync(commandName, userId, null, success);

        // Assert
        _eventBusMock.Verify(x => x.PublishAsync(It.Is<CommandExecutedEvent>(e =>
            e.CorrelationId == correlationId)),
            Times.Once);
    }

    [Fact]
    public async Task PublishBotStateChangedAsync_CallsEventBusWithCorrectEvent()
    {
        // Arrange
        const string previousState = "Idle";
        const string newState = "Active";
        const string? reason = "User triggered action";

        // Act
        await _publisher.PublishBotStateChangedAsync(previousState, newState, reason);

        // Assert
        _eventBusMock.Verify(x => x.PublishAsync(It.Is<BotStateChangedEvent>(e =>
            e.PreviousState == previousState &&
            e.NewState == newState &&
            e.Reason == reason &&
            e.EventType == "BotStateChangedEvent")),
            Times.Once);
    }

    [Fact]
    public async Task PublishBotStateChangedAsync_WithNullReason_SetsReasonToNull()
    {
        // Arrange
        const string previousState = "Idle";
        const string newState = "Active";

        // Act
        await _publisher.PublishBotStateChangedAsync(previousState, newState);

        // Assert
        _eventBusMock.Verify(x => x.PublishAsync(It.Is<BotStateChangedEvent>(e =>
            e.Reason == null)),
            Times.Once);
    }

    [Fact]
    public async Task PublishBotStateChangedAsync_WithEmptyReason_SetsReasonToEmpty()
    {
        // Arrange
        const string previousState = "Idle";
        const string newState = "Active";
        const string emptyReason = "";

        // Act
        await _publisher.PublishBotStateChangedAsync(previousState, newState, emptyReason);

        // Assert
        _eventBusMock.Verify(x => x.PublishAsync(It.Is<BotStateChangedEvent>(e =>
            e.Reason == emptyReason)),
            Times.Once);
    }

    [Fact]
    public async Task PublishBotStateChangedAsync_WithCorrelationId_SetsCorrelationIdOnEvent()
    {
        // Arrange
        const string correlationId = "test-correlation";
        const string previousState = "Idle";
        const string newState = "Active";

        _publisher.WithCorrelationId(correlationId);

        // Act
        await _publisher.PublishBotStateChangedAsync(previousState, newState);

        // Assert
        _eventBusMock.Verify(x => x.PublishAsync(It.Is<BotStateChangedEvent>(e =>
            e.CorrelationId == correlationId)),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_GenericMethod_CallsEventBusWithCorrectEvent()
    {
        // Arrange
        var testEvent = new TestEvent();

        // Act
        await _publisher.PublishAsync(testEvent);

        // Assert
        _eventBusMock.Verify(x => x.PublishAsync(testEvent), Times.Once);
    }

    [Fact]
    public void LoggingMessageEventHandler_CanBeCreated()
    {
        // Act
        var handler = new LoggingMessageEventHandler();

        // Assert
        handler.Should().NotBeNull();
    }

    [Fact]
    public void LoggingMessageEventHandler_CanBeCreatedWithLogger()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LoggingMessageEventHandler>>();

        // Act
        var handler = new LoggingMessageEventHandler(loggerMock.Object);

        // Assert
        handler.Should().NotBeNull();
    }

    [Fact]
    public void LoggingCommandEventHandler_CanBeCreated()
    {
        // Act
        var handler = new LoggingCommandEventHandler();

        // Assert
        handler.Should().NotBeNull();
    }

    [Fact]
    public void LoggingCommandEventHandler_CanBeCreatedWithLogger()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LoggingCommandEventHandler>>();

        // Act
        var handler = new LoggingCommandEventHandler(loggerMock.Object);

        // Assert
        handler.Should().NotBeNull();
    }

    // Test event for generic PublishAsync testing
    private class TestEvent : EventBase
    {
        public TestEvent() : base() { }
    }
}