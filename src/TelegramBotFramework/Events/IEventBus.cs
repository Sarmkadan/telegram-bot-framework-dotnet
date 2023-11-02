#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

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
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : class, IEvent;

    /// <summary>
    /// Clears all subscribers.
    /// </summary>
    void Clear();

    /// <summary>
    /// Gets the number of subscribers for an event type.
    /// </summary>
    int GetSubscriberCount<TEvent>() where TEvent : class, IEvent;
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

    protected EventBase(string? correlationId = null)
    {
        CorrelationId = correlationId ?? Guid.NewGuid().ToString();
    }
}