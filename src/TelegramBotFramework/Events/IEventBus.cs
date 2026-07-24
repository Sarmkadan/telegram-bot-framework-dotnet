#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace TelegramBotFramework.Events;

/// <summary>
/// Pub-Sub event bus for decoupled communication between components.
/// Allows publishers to emit events and subscribers to listen for them.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Subscribes a handler to an event type.
    /// </summary>
    void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : class, IEvent;

    /// <summary>
    /// Unsubscribes a handler from an event type.
    /// </summary>
    void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : class, IEvent;

    /// <summary>
    /// Publishes an event to all registered subscribers.
    /// </summary>
    /// <param name="event">The event to publish.</param>
    /// <typeparam name="TEvent">The type of event.</typeparam>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : class, IEvent;

    /// <summary>
    /// Clears all subscribers.
    /// </summary>
    void Clear();

    /// <summary>
    /// Gets the number of subscribers for an event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of event.</typeparam>
    /// <returns>The number of subscribers.</returns>
    int GetSubscriberCount<TEvent>() where TEvent : class, IEvent;

    /// <summary>
    /// Registers middleware to be executed around event handling.
    /// Middleware is executed in the order it's registered.
    /// </summary>
    /// <param name="middleware">The middleware to register.</param>
    void RegisterMiddleware(IEventMiddleware middleware);

    /// <summary>
    /// Gets all registered middleware in the order they were registered.
    /// </summary>
    /// <returns>An enumerable of middleware instances.</returns>
    IEnumerable<IEventMiddleware> GetMiddleware();
}

/// <summary>
/// Base interface for all events in the system.
/// </summary>
public interface IEvent
{
    string EventType { get; }
    DateTime OccurredAt { get; }
    string? CorrelationId { get; }
}

/// <summary>
/// Base class for events with common properties.
/// </summary>
public abstract class EventBase : IEvent
{
    public string EventType => GetType().Name;
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Validates that the specified string value is not null or whitespace.
    /// </summary>
    /// <param name="value">The string value to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is null or whitespace.</exception>
    protected static void ValidateStringNotNullOrWhiteSpace(string? value, string paramName)
    {
        ArgumentException.ThrowIfNullOrEmpty(value, paramName);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("String cannot be whitespace.", paramName);
        }
    }

    /// <summary>
    /// Validates that the specified object reference is not null.
    /// </summary>
    /// <param name="value">The object reference to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    protected static void ValidateNotNull(object? value, string paramName)
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
    }

    protected EventBase(string? correlationId = null)
    {
        CorrelationId = correlationId ?? Guid.NewGuid().ToString();
    }
}