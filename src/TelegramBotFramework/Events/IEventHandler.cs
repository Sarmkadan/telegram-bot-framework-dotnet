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
/// Base class for message-related events containing common message payload fields.
/// </summary>
public abstract class MessageEventBase : EventBase
{
    /// <summary>
    /// Gets or sets the chat identifier.
    /// </summary>
    public long ChatId { get; set; }

    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Gets or sets the message text.
    /// </summary>
    public string? MessageText { get; set; }

    /// <summary>
    /// Gets or sets the message timestamp.
    /// </summary>
    public DateTime MessageTimestamp { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageEventBase"/> class.
    /// </summary>
    /// <param name="chatId">The chat identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="messageText">The message text.</param>
    /// <param name="correlationId">The correlation identifier.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="correlationId"/> is null or empty.</exception>
    protected MessageEventBase(long chatId, long userId, string? messageText, string? correlationId = null)
        : base(correlationId)
    {
        ChatId = chatId;
        UserId = userId;
        MessageText = messageText;
        MessageTimestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Example: Message received event
/// </summary>
public sealed class MessageReceivedEvent : MessageEventBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageReceivedEvent"/> class.
    /// </summary>
    /// <param name="chatId">The chat identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="messageText">The message text.</param>
    /// <param name="correlationId">The correlation identifier.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="correlationId"/> is null or empty.</exception>
    public MessageReceivedEvent(long chatId, long userId, string? messageText, string? correlationId = null)
        : base(chatId, userId, messageText, correlationId)
    {
    }
}

/// <summary>
/// Example: Message edited event
/// </summary>
public sealed class MessageEditedEvent : MessageEventBase
{
    /// <summary>
    /// Gets or sets the edited timestamp.
    /// </summary>
    public DateTime? EditedTimestamp { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageEditedEvent"/> class.
    /// </summary>
    /// <param name="chatId">The chat identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="messageText">The message text.</param>
    /// <param name="editedTimestamp">The edited timestamp.</param>
    /// <param name="correlationId">The correlation identifier.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="correlationId"/> is null or empty.</exception>
    public MessageEditedEvent(long chatId, long userId, string? messageText, DateTime? editedTimestamp = null, string? correlationId = null)
        : base(chatId, userId, messageText, correlationId)
    {
        EditedTimestamp = editedTimestamp;
    }
}

/// <summary>
/// Example: User command executed event
/// </summary>
public sealed class CommandExecutedEvent : EventBase
{
    /// <summary>
    /// Gets or sets the command name.
    /// </summary>
    public string CommandName { get; set; }

    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Gets or sets the arguments.
    /// </summary>
    public string? Arguments { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the command execution was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the error message if the command failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandExecutedEvent"/> class.
    /// </summary>
    /// <param name="commandName">The command name.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="arguments">The arguments.</param>
    /// <param name="success">Whether the command execution was successful.</param>
    /// <param name="errorMessage">The error message if the command failed.</param>
    /// <param name="correlationId">The correlation identifier.</param>
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
    /// <summary>
    /// Gets or sets the previous state.
    /// </summary>
    public string PreviousState { get; set; }

    /// <summary>
    /// Gets or sets the new state.
    /// </summary>
    public string NewState { get; set; }

    /// <summary>
    /// Gets or sets the reason for the state change.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BotStateChangedEvent"/> class.
    /// </summary>
    /// <param name="previousState">The previous state.</param>
    /// <param name="newState">The new state.</param>
    /// <param name="reason">The reason for the state change.</param>
    /// <param name="correlationId">The correlation identifier.</param>
    public BotStateChangedEvent(string previousState, string newState, string? reason = null, string? correlationId = null)
        : base(correlationId)
    {
        PreviousState = previousState;
        NewState = newState;
        Reason = reason;
    }
}