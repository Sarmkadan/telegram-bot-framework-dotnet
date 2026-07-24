#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Collections.Concurrent;

namespace TelegramBotFramework.Events;

/// <summary>
/// In-memory implementation of the event bus for decoupled communication.
/// </summary>
public sealed class InMemoryEventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<object>> _subscribers = new();

    /// <summary>
    /// Subscribes a handler to an event type.
    /// </summary>
    public void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : class, IEvent
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        var eventType = typeof(TEvent);
        _subscribers.AddOrUpdate(
            eventType,
            _ => new List<object> { handler },
            (_, existingHandlers) =>
            {
                if (!existingHandlers.Contains(handler))
                {
                    existingHandlers.Add(handler);
                }
                return existingHandlers;
            });
    }

    /// <summary>
    /// Unsubscribes a handler from an event type.
    /// </summary>
    public void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : class, IEvent
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        var eventType = typeof(TEvent);
        if (_subscribers.TryGetValue(eventType, out var handlers))
        {
            handlers.Remove(handler);
            if (handlers.Count == 0)
            {
                _subscribers.TryRemove(eventType, out _);
            }
        }
    }

    /// <summary>
    /// Publishes an event to all registered subscribers.
    /// </summary>
    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : class, IEvent
    {
        if (@event is null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        var eventType = typeof(TEvent);
        if (_subscribers.TryGetValue(eventType, out var handlers))
        {
            // Create a copy to avoid issues if handlers are modified during iteration
            var handlersCopy = handlers.ToArray();

            foreach (var handler in handlersCopy)
            {
                if (handler is IEventHandler<TEvent> typedHandler)
                {
                    await typedHandler.HandleAsync(@event).ConfigureAwait(false);
                }
            }
        }
    }

    /// <summary>
    /// Clears all subscribers.
    /// </summary>
    public void Clear()
    {
        _subscribers.Clear();
    }

    /// <summary>
    /// Gets the number of subscribers for an event type.
    /// </summary>
    public int GetSubscriberCount<TEvent>() where TEvent : class, IEvent
    {
        var eventType = typeof(TEvent);
        if (_subscribers.TryGetValue(eventType, out var handlers))
        {
            return handlers.Count;
        }
        return 0;
    }
}