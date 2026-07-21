#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using TelegramBotFramework.Integration;

namespace TelegramBotFramework.Events;

/// <summary>
/// Interface for event handlers that process specific event types.
/// Implementations should handle events synchronously or asynchronously.
/// </summary>
/// <typeparam name="TEvent">The type of event this handler processes</typeparam>
public interface IEventHandler<in TEvent> where TEvent : class, IEvent
{
    /// <summary>
    /// Handles the event.
    /// </summary>
    Task HandleAsync(TEvent @event);

    /// <summary>
    /// Gets the name of this handler for logging/debugging.
    /// </summary>
    string GetHandlerName() => GetType().Name;
}

/// <summary>
/// Base class for event handlers with common logging functionality.
/// </summary>
public abstract class EventHandlerBase<TEvent> : IEventHandler<TEvent> where TEvent : class, IEvent
{
    protected readonly ILogger<EventHandlerBase<TEvent>> _logger;

    protected EventHandlerBase(ILogger<EventHandlerBase<TEvent>>? logger = null)
    {
        _logger = logger ?? new ConsoleLogger<EventHandlerBase<TEvent>>();
    }

    public async Task HandleAsync(TEvent @event)
    {
        try
        {
            _logger.LogInformation("Handling event {EventType} with ID {CorrelationId}",
                @event.EventType, @event.CorrelationId);

            await ExecuteAsync(@event).ConfigureAwait(false);

            _logger.LogInformation("Event {EventType} handled successfully", @event.EventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling event {EventType}", @event.EventType);
            throw;
        }
    }

    /// <summary>
    /// Subclasses override this to implement event handling logic.
    /// </summary>
    protected abstract Task ExecuteAsync(TEvent @event);

    public virtual string GetHandlerName() => GetType().Name;
}

/// <summary>
/// Example: Message received event
/// </summary>
public sealed class MessageReceivedEvent : EventBase
{
    public long ChatId { get; set; }
    public long UserId { get; set; }
    public string? MessageText { get; set; }
    public DateTime MessageTimestamp { get; set; }

    public MessageReceivedEvent(long chatId, long userId, string? messageText, string? correlationId = null)
        : base(correlationId)
    {
        ChatId = chatId;
        UserId = userId;
        MessageText = messageText;
        MessageTimestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Example: Message edited event
/// </summary>
public sealed class MessageEditedEvent : EventBase
{
    public long ChatId { get; set; }
    public long UserId { get; set; }
    public string? MessageText { get; set; }
    public DateTime MessageTimestamp { get; set; }
    public DateTime? EditedTimestamp { get; set; }

    public MessageEditedEvent(long chatId, long userId, string? messageText, DateTime? editedTimestamp = null, string? correlationId = null)
        : base(correlationId)
    {
        ChatId = chatId;
        UserId = userId;
        MessageText = messageText;
        MessageTimestamp = DateTime.UtcNow;
        EditedTimestamp = editedTimestamp;
    }
}

/// <summary>
/// Example: User command executed event
/// </summary>
public sealed class CommandExecutedEvent : EventBase
{
    public string CommandName { get; set; }
    public long UserId { get; set; }
    public string? Arguments { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    public CommandExecutedEvent(string commandName, long userId, string? arguments, bool success, string? errorMessage = null, string? correlationId = null)
        : base(correlationId)
    {
        CommandName = commandName;
        UserId = userId;
        Arguments = arguments;
        Success = success;
        ErrorMessage = errorMessage;
    }
}

/// <summary>
/// Example: Bot state changed event
/// </summary>
public sealed class BotStateChangedEvent : EventBase
{
    public string PreviousState { get; set; }
    public string NewState { get; set; }
    public string? Reason { get; set; }

    public BotStateChangedEvent(string previousState, string newState, string? reason = null, string? correlationId = null)
        : base(correlationId)
    {
        PreviousState = previousState;
        NewState = newState;
        Reason = reason;
    }
}