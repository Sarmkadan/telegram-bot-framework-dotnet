#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using TelegramBotFramework.Integration;

namespace TelegramBotFramework.Events;

/// <summary>
/// Helper class for publishing events to the event bus.
/// Provides convenience methods and ensures consistent event publishing.
/// </summary>
public sealed class EventPublisher : IEventPublisher
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<EventPublisher> _logger;
    private string? _correlationId;

    public EventPublisher(IEventBus eventBus, ILogger<EventPublisher>? logger = null)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? new ConsoleLogger<EventPublisher>();
        _logger.LogInformation("EventPublisher initialized");
    }

    /// <summary>
    /// Sets the correlation ID for tracking related events.
    /// </summary>
    public EventPublisher WithCorrelationId(string correlationId)
    {
        _logger.LogInformation("Setting correlation id to {CorrelationId}", correlationId);
        _correlationId = correlationId;
        _logger.LogInformation("WithCorrelationId completed with correlation id {CorrelationId}", correlationId);
        _logger.LogInformation("Correlation id set");
        return this;
    }

    /// <summary>
    /// Publishes a message received event.
    /// </summary>
    public async Task PublishMessageReceivedAsync(long chatId, long userId, string? messageText)
    {
        _logger.LogInformation("Publishing message received event for chat {ChatId}, user {UserId}", chatId, userId);
        try
        {
            var @event = new MessageReceivedEvent(chatId, userId, messageText, _correlationId);
            await _eventBus.PublishAsync(@event).ConfigureAwait(false);
            _logger.LogInformation("PublishMessageReceivedAsync completed for chat {ChatId}, user {UserId}", chatId, userId);
            _logger.LogInformation("Message received event published");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message received event for chat {ChatId}, user {UserId}", chatId, userId);
            throw;
        }
    }

    /// <summary>
    /// Publishes a command executed event.
    /// </summary>
    public async Task PublishCommandExecutedAsync(string commandName, long userId, string? arguments, bool success, string? errorMessage = null)
    {
        _logger.LogInformation("Publishing command executed event for command {CommandName} by user {UserId}", commandName, userId);
        try
        {
            var @event = new CommandExecutedEvent(commandName, userId, arguments, success, errorMessage, _correlationId);
            await _eventBus.PublishAsync(@event).ConfigureAwait(false);
            _logger.LogInformation("PublishCommandExecutedAsync completed for command {CommandName} by user {UserId} with success {Success}", commandName, userId, success);
            _logger.LogInformation("Command executed event published");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish command executed event for command {CommandName} by user {UserId}", commandName, userId);
            throw;
        }
    }

    /// <summary>
    /// Publishes a bot state changed event.
    /// </summary>
    public async Task PublishBotStateChangedAsync(string previousState, string newState, string? reason = null)
    {
        _logger.LogInformation("Publishing bot state changed event from {PreviousState} to {NewState}", previousState, newState);
        try
        {
            var @event = new BotStateChangedEvent(previousState, newState, reason, _correlationId);
            await _eventBus.PublishAsync(@event).ConfigureAwait(false);
            _logger.LogInformation("PublishBotStateChangedAsync completed from {PreviousState} to {NewState}", previousState, newState);
            _logger.LogInformation("Bot state changed event published");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish bot state changed event from {PreviousState} to {NewState}", previousState, newState);
            throw;
        }
    }

    /// <summary>
    /// Publishes a custom event.
    /// </summary>
    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : class, IEvent
    {
        _logger.LogInformation("Publishing custom event of type {EventType}", typeof(TEvent).Name);
        try
        {
            await _eventBus.PublishAsync(@event).ConfigureAwait(false);
            _logger.LogInformation("PublishAsync completed for event type {EventType}", typeof(TEvent).Name);
            _logger.LogInformation("Custom event of type {EventType} published", typeof(TEvent).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish custom event of type {EventType}", typeof(TEvent).Name);
            throw;
        }
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
