#nullable enable

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TelegramBotFramework.Events;
using Xunit;

namespace TelegramBotFramework.Tests;

public class EventPublisherTests : IEventPublisherTests
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
        _loggerMock.Object.LogInformation("Starting {TestName}", nameof(Constructor_WithNullEventBus_ThrowsArgumentNullException));
        _loggerMock.Object.LogWarning("Verifying fallback validation for a null {DependencyName}", "eventBus");

        // Act
        var act = () => new EventPublisher(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("eventBus");
        _loggerMock.Object.LogInformation("Completed {TestName}", nameof(Constructor_WithNullEventBus_ThrowsArgumentNullException));
    }

    [Fact]
    public void Constructor_WithNullLogger_CreatesConsoleLogger()
    {
        _loggerMock.Object.LogInformation("Starting {TestName}", nameof(Constructor_WithNullLogger_CreatesConsoleLogger));
        _loggerMock.Object.LogWarning("Verifying fallback logger creation when {DependencyName} is null", "logger");

        // Act
        var publisher = new EventPublisher(_eventBusMock.Object, null);

        // Assert
        publisher.Should().NotBeNull();
        _loggerMock.Object.LogInformation("Completed {TestName}", nameof(Constructor_WithNullLogger_CreatesConsoleLogger));
    }

    [Fact]
    public void WithCorrelationId_SetsCorrelationIdAndReturnsPublisher()
    {
        // Arrange
        const string expectedCorrelationId = EventPublisherTestsConstants.TestCorrelationId;
        _loggerMock.Object.LogInformation("Starting {TestName} with correlation ID {CorrelationId}", nameof(WithCorrelationId_SetsCorrelationIdAndReturnsPublisher), expectedCorrelationId);

        // Act
        var result = _publisher.WithCorrelationId(expectedCorrelationId);

        // Assert
        result.Should().BeSameAs(_publisher);

        // Verify by calling a method that uses correlation ID
        _eventBusMock.Reset();
        _publisher.PublishMessageReceivedAsync(EventPublisherTestsConstants.TestChatId2, EventPublisherTestsConstants.TestUserId3, EventPublisherTestsConstants.TestMessageShort);

        _eventBusMock.Verify(x => x.PublishAsync(It.Is<MessageReceivedEvent>(e =>
            e.CorrelationId == expectedCorrelationId)),
            Times.Once);
        _loggerMock.Object.LogInformation("Completed {TestName} with correlation ID {CorrelationId}", nameof(WithCorrelationId_SetsCorrelationIdAndReturnsPublisher), expectedCorrelationId);
    }

    [Fact]
    public void WithCorrelationId_MultipleCalls_OverwritesPreviousValue()
    {
        // Arrange
        const string firstCorrelationId = EventPublisherTestsConstants.FirstCorrelationId;
        const string secondCorrelationId = EventPublisherTestsConstants.SecondCorrelationId;
        _loggerMock.Object.LogInformation("Starting {TestName} with first correlation ID {FirstCorrelationId} and second correlation ID {SecondCorrelationId}", nameof(WithCorrelationId_MultipleCalls_OverwritesPreviousValue), firstCorrelationId, secondCorrelationId);
        _loggerMock.Object.LogWarning("Overwriting correlation ID {FirstCorrelationId} with {SecondCorrelationId}", firstCorrelationId, secondCorrelationId);

        // Act
        _publisher.WithCorrelationId(firstCorrelationId);
        _publisher.WithCorrelationId(secondCorrelationId);

        // Assert
        _eventBusMock.Reset();
        _publisher.PublishMessageReceivedAsync(123, 456, "test");

        _eventBusMock.Verify(x => x.PublishAsync(It.Is<MessageReceivedEvent>(e =>
            e.CorrelationId == secondCorrelationId)),
            Times.Once);
        _loggerMock.Object.LogInformation("Completed {TestName} with active correlation ID {CorrelationId}", nameof(WithCorrelationId_MultipleCalls_OverwritesPreviousValue), secondCorrelationId);
    }

    [Fact]
    public async Task PublishMessageReceivedAsync_CallsEventBusWithCorrectEvent()
    {
        // Arrange
        const long chatId = EventPublisherTestsConstants.TestChatId;
        const long userId = EventPublisherTestsConstants.TestUserId;
        const string messageText = EventPublisherTestsConstants.TestMessage;
        _loggerMock.Object.LogInformation("Starting {TestName} for chat {ChatId} and user {UserId}", nameof(PublishMessageReceivedAsync_CallsEventBusWithCorrectEvent), chatId, userId);

        // Act
        await _publisher.PublishMessageReceivedAsync(chatId, userId, messageText);

        // Assert
        _eventBusMock.Verify(x => x.PublishAsync(It.Is<MessageReceivedEvent>(e =>
            e.ChatId == chatId &&
            e.UserId == userId &&
            e.MessageText == messageText &&
            e.EventType == EventPublisherTestsConstants.MessageReceivedEventType)),
            Times.Once);
        _loggerMock.Object.LogInformation("Completed {TestName} for chat {ChatId} and user {UserId}", nameof(PublishMessageReceivedAsync_CallsEventBusWithCorrectEvent), chatId, userId);
    }

    [Fact]
    public async Task PublishMessageReceivedAsync_WithNullMessageText_SetsMessageTextToNull()
    {
        // Arrange
        const long chatId = EventPublisherTestsConstants.TestChatId;
        const long userId = EventPublisherTestsConstants.TestUserId;
        _loggerMock.Object.LogInformation("Starting {TestName} for chat {ChatId} and user {UserId}", nameof(PublishMessageReceivedAsync_WithNullMessageText_SetsMessageTextToNull), chatId, userId);
        _loggerMock.Object.LogWarning("Publishing message with null text for chat {ChatId} and user {UserId}", chatId, userId);

        // Act
        await _publisher.PublishMessageReceivedAsync(chatId, userId, null);

        // Assert
        _eventBusMock.Verify(x => x.PublishAsync(It.Is<MessageReceivedEvent>(e =>
            e.MessageText == null)),
            Times.Once);
        _loggerMock.Object.LogInformation("Completed {TestName} for chat {ChatId} and user {UserId}", nameof(PublishMessageReceivedAsync_WithNullMessageText_SetsMessageTextToNull), chatId, userId);
    }

    [Fact]
    public async Task PublishMessageReceivedAsync_WithEmptyMessageText_SetsMessageTextToEmpty()
    {
        // Arrange
        const long chatId = EventPublisherTestsConstants.TestChatId;
        const long userId = EventPublisherTestsConstants.TestUserId;
        const string emptyMessage = EventPublisherTestsConstants.EmptyMessage;
        _loggerMock.Object.LogInformation("Starting {TestName} for chat {ChatId} and user {UserId}", nameof(PublishMessageReceivedAsync_WithEmptyMessageText_SetsMessageTextToEmpty), chatId, userId);
        _loggerMock.Object.LogWarning("Publishing message with empty text for chat {ChatId} and user {UserId}", chatId, userId);

        // Act
        await _publisher.PublishMessageReceivedAsync(chatId, userId, emptyMessage);

        // Assert
        _eventBusMock.Verify(x => x.PublishAsync(It.Is<MessageReceivedEvent>(e =>
            e.MessageText == emptyMessage)),
            Times.Once);
        _loggerMock.Object.LogInformation("Completed {TestName} for chat {ChatId} and user {UserId}", nameof(PublishMessageReceivedAsync_WithEmptyMessageText_SetsMessageTextToEmpty), chatId, userId);
    }

    [Fact]
    public async Task PublishMessageReceivedAsync_WithCorrelationId_SetsCorrelationIdOnEvent()
    {
        // Arrange
        const string correlationId = EventPublisherTestsConstants.TestCorrelation;
        const long chatId = EventPublisherTestsConstants.TestChatId;
        const long userId = EventPublisherTestsConstants.TestUserId;
        const string messageText = EventPublisherTestsConstants.TestMessageText;
        _loggerMock.Object.LogInformation("Starting {TestName} with correlation ID {CorrelationId}", nameof(PublishMessageReceivedAsync_WithCorrelationId_SetsCorrelationIdOnEvent), correlationId);

        _publisher.WithCorrelationId(correlationId);

        // Act
        await _publisher.PublishMessageReceivedAsync(chatId, userId, messageText);

        // Assert
        _eventBusMock.Verify(x => x.PublishAsync(It.Is<MessageReceivedEvent>(e =>
            e.CorrelationId == correlationId)),
            Times.Once);
        _loggerMock.Object.LogInformation("Completed {TestName} with correlation ID {CorrelationId}", nameof(PublishMessageReceivedAsync_WithCorrelationId_SetsCorrelationIdOnEvent), correlationId);
    }

    [Fact]
    public async Task PublishCommandExecutedAsync_CallsEventBusWithCorrectEvent()
    {
        // Arrange
        const string commandName = EventPublisherTestsConstants.TestCommandName;
        const long userId = EventPublisherTestsConstants.TestUserId2;
        const string arguments = EventPublisherTestsConstants.TestArguments;
        const bool success = true;
        const string errorMessage = null;
        _loggerMock.Object.LogInformation("Starting {TestName} for command {CommandName}, user {UserId}, and success {Success}", nameof(PublishCommandExecutedAsync_CallsEventBusWithCorrectEvent), commandName, userId, success);

        // Act
        await _publisher.PublishCommandExecutedAsync(commandName, userId, arguments, success, errorMessage);

        // Assert
        _eventBusMock.Verify(x => x.PublishAsync(It.Is<CommandExecutedEvent>(e =>
            e.CommandName == commandName &&
            e.UserId == userId &&
            e.Arguments == arguments &&
            e.Success == success &&
            e.ErrorMessage == errorMessage &&
            e.EventType == EventPublisherTestsConstants.CommandExecutedEventType)),
            Times.Once);
        _loggerMock.Object.LogInformation("Completed {TestName} for command {CommandName} and user {UserId}", nameof(PublishCommandExecutedAsync_CallsEventBusWithCorrectEvent), commandName, userId);
    }

    [Fact]
    public async Task PublishCommandExecutedAsync_WithErrorMessage_SetsErrorMessage()
    {
        // Arrange
        const string commandName = EventPublisherTestsConstants.TestCommandName;
        const long userId = EventPublisherTestsConstants.TestUserId2;
        const string arguments = EventPublisherTestsConstants.TestArguments;
        const bool success = false;
        const string errorMessage = EventPublisherTestsConstants.TestErrorMessage;
        _loggerMock.Object.LogInformation("Starting {TestName} for command {CommandName} and user {UserId}", nameof(PublishCommandExecutedAsync_WithErrorMessage_SetsErrorMessage), commandName, userId);
        _loggerMock.Object.LogWarning("Publishing failed command {CommandName} for user {UserId} with error {ErrorMessage}", commandName, userId, errorMessage);

        // Act
        await _publisher.PublishCommandExecutedAsync(commandName, userId, arguments, success, errorMessage);

        // Assert
        _eventBusMock.Verify(x => x.PublishAsync(It.Is<CommandExecutedEvent>(e =>
            e.Success == success &&
            e.ErrorMessage == errorMessage)),
            Times.Once);
        _loggerMock.Object.LogInformation("Completed {TestName} for command {CommandName} and user {UserId}", nameof(PublishCommandExecutedAsync_WithErrorMessage_SetsErrorMessage), commandName, userId);
    }

    [Fact]
    public async Task PublishCommandExecutedAsync_WithNullArguments_SetsArgumentsToNull()
    {
        // Arrange
        const string commandName = EventPublisherTestsConstants.TestCommandName;
        const long userId = EventPublisherTestsConstants.TestUserId2;
        const string? arguments = null;
        const bool success = true;
        _loggerMock.Object.LogInformation("Starting {TestName} for command {CommandName} and user {UserId}", nameof(PublishCommandExecutedAsync_WithNullArguments_SetsArgumentsToNull), commandName, userId);
        _loggerMock.Object.LogWarning("Publishing command {CommandName} for user {UserId} with null arguments", commandName, userId);

        // Act
        await _publisher.PublishCommandExecutedAsync(commandName, userId, arguments, success);

        // Assert
        _eventBusMock.Verify(x => x.PublishAsync(It.Is<CommandExecutedEvent>(e =>
            e.Arguments == null)),
            Times.Once);
        _loggerMock.Object.LogInformation("Completed {TestName} for command {CommandName} and user {UserId}", nameof(PublishCommandExecutedAsync_WithNullArguments_SetsArgumentsToNull), commandName, userId);
    }

    [Fact]
    public async Task PublishCommandExecutedAsync_WithEmptyArguments_SetsArgumentsToEmpty()
    {
        // Arrange
        const string commandName = EventPublisherTestsConstants.TestCommandName;
        const long userId = EventPublisherTestsConstants.TestUserId2;
        const string emptyArguments = EventPublisherTestsConstants.EmptyMessage;
        const bool success = true;
        _loggerMock.Object.LogInformation("Starting {TestName} for command {CommandName} and user {UserId}", nameof(PublishCommandExecutedAsync_WithEmptyArguments_SetsArgumentsToEmpty), commandName, userId);
        _loggerMock.Object.LogWarning("Publishing command {CommandName} for user {UserId} with empty arguments", commandName, userId);

        // Act
        await _publisher.PublishCommandExecutedAsync(commandName, userId, emptyArguments, success);

        // Assert
        _eventBusMock.Verify(x => x.PublishAsync(It.Is<CommandExecutedEvent>(e =>
            e.Arguments == emptyArguments)),
            Times.Once);
        _loggerMock.Object.LogInformation("Completed {TestName} for command {CommandName} and user {UserId}", nameof(PublishCommandExecutedAsync_WithEmptyArguments_SetsArgumentsToEmpty), commandName, userId);
    }

    [Fact]
    public async Task PublishCommandExecutedAsync_WithCorrelationId_SetsCorrelationIdOnEvent()
    {
        // Arrange
        const string correlationId = EventPublisherTestsConstants.TestCorrelation;
        const string commandName = EventPublisherTestsConstants.TestCommandName;
        const long userId = EventPublisherTestsConstants.TestUserId2;
        const bool success = true;
        _loggerMock.Object.LogInformation("Starting {TestName} for command {CommandName}, user {UserId}, and correlation ID {CorrelationId}", nameof(PublishCommandExecutedAsync_WithCorrelationId_SetsCorrelationIdOnEvent), commandName, userId, correlationId);

        _publisher.WithCorrelationId(correlationId);

        // Act
        await _publisher.PublishCommandExecutedAsync(commandName, userId, null, success);

        // Assert
        _eventBusMock.Verify(x => x.PublishAsync(It.Is<CommandExecutedEvent>(e =>
            e.CorrelationId == correlationId)),
            Times.Once);
        _loggerMock.Object.LogInformation("Completed {TestName} with correlation ID {CorrelationId}", nameof(PublishCommandExecutedAsync_WithCorrelationId_SetsCorrelationIdOnEvent), correlationId);
    }

    [Fact]
    public async Task PublishBotStateChangedAsync_CallsEventBusWithCorrectEvent()
    {
        // Arrange
        const string previousState = EventPublisherTestsConstants.TestPreviousState;
        const string newState = EventPublisherTestsConstants.TestNewState;
        const string? reason = EventPublisherTestsConstants.TestReason;
        _loggerMock.Object.LogInformation("Starting {TestName} for state transition from {PreviousState} to {NewState}", nameof(PublishBotStateChangedAsync_CallsEventBusWithCorrectEvent), previousState, newState);

        // Act
        await _publisher.PublishBotStateChangedAsync(previousState, newState, reason);

        // Assert
        _eventBusMock.Verify(x => x.PublishAsync(It.Is<BotStateChangedEvent>(e =>
            e.PreviousState == previousState &&
            e.NewState == newState &&
            e.Reason == reason &&
            e.EventType == EventPublisherTestsConstants.BotStateChangedEventType)),
            Times.Once);
        _loggerMock.Object.LogInformation("Completed {TestName} for state transition from {PreviousState} to {NewState}", nameof(PublishBotStateChangedAsync_CallsEventBusWithCorrectEvent), previousState, newState);
    }

    [Fact]
    public async Task PublishBotStateChangedAsync_WithNullReason_SetsReasonToNull()
    {
        // Arrange
        const string previousState = EventPublisherTestsConstants.TestPreviousState;
        const string newState = EventPublisherTestsConstants.TestNewState;
        _loggerMock.Object.LogInformation("Starting {TestName} for state transition from {PreviousState} to {NewState}", nameof(PublishBotStateChangedAsync_WithNullReason_SetsReasonToNull), previousState, newState);
        _loggerMock.Object.LogWarning("Publishing state transition from {PreviousState} to {NewState} without a reason", previousState, newState);

        // Act
        await _publisher.PublishBotStateChangedAsync(previousState, newState);

        // Assert
        _eventBusMock.Verify(x => x.PublishAsync(It.Is<BotStateChangedEvent>(e =>
            e.Reason == null)),
            Times.Once);
        _loggerMock.Object.LogInformation("Completed {TestName} for state transition from {PreviousState} to {NewState}", nameof(PublishBotStateChangedAsync_WithNullReason_SetsReasonToNull), previousState, newState);
    }

    [Fact]
    public async Task PublishBotStateChangedAsync_WithEmptyReason_SetsReasonToEmpty()
    {
        // Arrange
        const string previousState = EventPublisherTestsConstants.TestPreviousState;
        const string newState = EventPublisherTestsConstants.TestNewState;
        const string emptyReason = EventPublisherTestsConstants.EmptyReason;
        _loggerMock.Object.LogInformation("Starting {TestName} for state transition from {PreviousState} to {NewState}", nameof(PublishBotStateChangedAsync_WithEmptyReason_SetsReasonToEmpty), previousState, newState);
        _loggerMock.Object.LogWarning("Publishing state transition from {PreviousState} to {NewState} with an empty reason", previousState, newState);

        // Act
        await _publisher.PublishBotStateChangedAsync(previousState, newState, emptyReason);

        // Assert
        _eventBusMock.Verify(x => x.PublishAsync(It.Is<BotStateChangedEvent>(e =>
            e.Reason == emptyReason)),
            Times.Once);
        _loggerMock.Object.LogInformation("Completed {TestName} for state transition from {PreviousState} to {NewState}", nameof(PublishBotStateChangedAsync_WithEmptyReason_SetsReasonToEmpty), previousState, newState);
    }

    [Fact]
    public async Task PublishBotStateChangedAsync_WithCorrelationId_SetsCorrelationIdOnEvent()
    {
        // Arrange
        const string correlationId = EventPublisherTestsConstants.TestCorrelation;
        const string previousState = EventPublisherTestsConstants.TestPreviousState;
        const string newState = EventPublisherTestsConstants.TestNewState;
        _loggerMock.Object.LogInformation("Starting {TestName} with correlation ID {CorrelationId} for state transition from {PreviousState} to {NewState}", nameof(PublishBotStateChangedAsync_WithCorrelationId_SetsCorrelationIdOnEvent), correlationId, previousState, newState);

        _publisher.WithCorrelationId(correlationId);

        // Act
        await _publisher.PublishBotStateChangedAsync(previousState, newState);

        // Assert
        _eventBusMock.Verify(x => x.PublishAsync(It.Is<BotStateChangedEvent>(e =>
            e.CorrelationId == correlationId)),
            Times.Once);
        _loggerMock.Object.LogInformation("Completed {TestName} with correlation ID {CorrelationId}", nameof(PublishBotStateChangedAsync_WithCorrelationId_SetsCorrelationIdOnEvent), correlationId);
    }

    [Fact]
    public async Task PublishAsync_GenericMethod_CallsEventBusWithCorrectEvent()
    {
        // Arrange
        var testEvent = new TestEvent();
        _loggerMock.Object.LogInformation("Starting {TestName} for event type {EventType}", nameof(PublishAsync_GenericMethod_CallsEventBusWithCorrectEvent), testEvent.GetType().Name);

        // Act
        await _publisher.PublishAsync(testEvent);

        // Assert
        _eventBusMock.Verify(x => x.PublishAsync(testEvent), Times.Once);
        _loggerMock.Object.LogInformation("Completed {TestName} for event type {EventType}", nameof(PublishAsync_GenericMethod_CallsEventBusWithCorrectEvent), testEvent.GetType().Name);
    }

    [Fact]
    public void LoggingMessageEventHandler_CanBeCreated()
    {
        _loggerMock.Object.LogInformation("Starting {TestName} without an explicit handler logger", nameof(LoggingMessageEventHandler_CanBeCreated));
        _loggerMock.Object.LogWarning("Creating {HandlerType} with its fallback logger", nameof(LoggingMessageEventHandler));

        // Act
        var handler = new LoggingMessageEventHandler();

        // Assert
        handler.Should().NotBeNull();
        _loggerMock.Object.LogInformation("Completed {TestName}", nameof(LoggingMessageEventHandler_CanBeCreated));
    }

    [Fact]
    public void LoggingMessageEventHandler_CanBeCreatedWithLogger()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LoggingMessageEventHandler>>();
        _loggerMock.Object.LogInformation("Starting {TestName} with an explicit logger for {HandlerType}", nameof(LoggingMessageEventHandler_CanBeCreatedWithLogger), nameof(LoggingMessageEventHandler));

        // Act
        var handler = new LoggingMessageEventHandler(loggerMock.Object);

        // Assert
        handler.Should().NotBeNull();
        _loggerMock.Object.LogInformation("Completed {TestName}", nameof(LoggingMessageEventHandler_CanBeCreatedWithLogger));
    }

    [Fact]
    public void LoggingCommandEventHandler_CanBeCreated()
    {
        _loggerMock.Object.LogInformation("Starting {TestName} without an explicit handler logger", nameof(LoggingCommandEventHandler_CanBeCreated));
        _loggerMock.Object.LogWarning("Creating {HandlerType} with its fallback logger", nameof(LoggingCommandEventHandler));

        // Act
        var handler = new LoggingCommandEventHandler();

        // Assert
        handler.Should().NotBeNull();
        _loggerMock.Object.LogInformation("Completed {TestName}", nameof(LoggingCommandEventHandler_CanBeCreated));
    }

    [Fact]
    public void LoggingCommandEventHandler_CanBeCreatedWithLogger()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LoggingCommandEventHandler>>();
        _loggerMock.Object.LogInformation("Starting {TestName} with an explicit logger for {HandlerType}", nameof(LoggingCommandEventHandler_CanBeCreatedWithLogger), nameof(LoggingCommandEventHandler));

        // Act
        var handler = new LoggingCommandEventHandler(loggerMock.Object);

        // Assert
        handler.Should().NotBeNull();
        _loggerMock.Object.LogInformation("Completed {TestName}", nameof(LoggingCommandEventHandler_CanBeCreatedWithLogger));
    }

    // Test event for generic PublishAsync testing
    private class TestEvent : EventBase
    {
        public TestEvent() : base() { }
    }
}
