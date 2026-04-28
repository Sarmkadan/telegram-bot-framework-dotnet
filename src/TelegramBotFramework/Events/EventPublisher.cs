#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Events;

/// <summary>
/// Helper class for publishing events to the event bus.
/// Provides convenience methods and ensures consistent event publishing.
/// </summary>
public sealed class EventPublisher
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<EventPublisher> _logger;
    private string? _correlationId;

    public EventPublisher(IEventBus eventBus, ILogger<EventPublisher>? logger = null)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? new ConsoleLogger<EventPublisher>();
    }

    /// <summary>
    /// Sets the correlation ID for tracking related events.
    /// </summary>
    public EventPublisher WithCorrelationId(string correlationId)
    {
        _correlationId = correlationId;
        return this;
    }

    /// <summary>
    /// Publishes a message received event.
    /// </summary>
    public async Task PublishMessageReceivedAsync(long chatId, long userId, string? messageText)
    {
        var @event = new MessageReceivedEvent(chatId, userId, messageText, _correlationId);
        await _eventBus.PublishAsync(@event).ConfigureAwait(false);
    }

    /// <summary>
    /// Publishes a command executed event.
    /// </summary>
    public async Task PublishCommandExecutedAsync(string commandName, long userId, string? arguments, bool success, string? errorMessage = null)
    {
        var @event = new CommandExecutedEvent(commandName, userId, arguments, success, errorMessage, _correlationId);
        await _eventBus.PublishAsync(@event).ConfigureAwait(false);
    }

    /// <summary>
    /// Publishes a bot state changed event.
    /// </summary>
    public async Task PublishBotStateChangedAsync(string previousState, string newState, string? reason = null)
    {
        var @event = new BotStateChangedEvent(previousState, newState, reason, _correlationId);
        await _eventBus.PublishAsync(@event).ConfigureAwait(false);
    }

    /// <summary>
    /// Publishes a custom event.
    /// </summary>
    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : class, IEvent
    {
        await _eventBus.PublishAsync(@event).ConfigureAwait(false);
    }
}

/// <summary>
/// Example event handler for message received events.
/// </summary>
public sealed class LoggingMessageEventHandler : EventHandlerBase<MessageReceivedEvent>
{
    public LoggingMessageEventHandler(ILogger<LoggingMessageEventHandler>? logger = null) : base(logger) { }

    protected override Task ExecuteAsync(MessageReceivedEvent @event)
    {
        _logger.LogInformation("Message received from user {UserId} in chat {ChatId}: {Message}",
            @event.UserId, @event.ChatId, @event.MessageText);

        return Task.CompletedTask;
    }
}

/// <summary>
/// Example event handler for command executed events.
/// </summary>
public sealed class LoggingCommandEventHandler : EventHandlerBase<CommandExecutedEvent>
{
    public LoggingCommandEventHandler(ILogger<LoggingCommandEventHandler>? logger = null) : base(logger) { }

    protected override Task ExecuteAsync(CommandExecutedEvent @event)
    {
        var status = @event.Success ? "succeeded" : "failed";
        var message = $"Command {status}: /{@event.CommandName} by user {@event.UserId}";

        if (@event.ErrorMessage  is not null)
            message += $" - Error: {@event.ErrorMessage}";

        _logger.LogInformation(message);

        return Task.CompletedTask;
    }
}