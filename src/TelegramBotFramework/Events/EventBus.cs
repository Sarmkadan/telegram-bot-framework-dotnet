#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Events;

using System.Collections.Concurrent;

/// <summary>
/// In-process publish-subscribe event bus implementation.
/// Manages event subscriptions and broadcasts events to all registered handlers.
/// Thread-safe for concurrent operations.
/// </summary>
public sealed class EventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<object>> _subscribers = new();
    private readonly ILogger<EventBus> _logger;
    private readonly object _syncLock = new();

    public EventBus(ILogger<EventBus>? logger = null)
    {
        _logger = logger ?? new ConsoleLogger<EventBus>();
    }

    public void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : class, IEvent
    {
        if (handler  is null)
            throw new ArgumentNullException(nameof(handler));

        var eventType = typeof(TEvent);
        var handlers = _subscribers.GetOrAdd(eventType, _ => new List<object>());

        lock (_syncLock)
        {
            handlers.Add(handler);
        }

        _logger.LogInformation("Handler {HandlerName} subscribed to {EventType}",
            handler.GetHandlerName(), eventType.Name);
    }

    public void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : class, IEvent
    {
        if (handler  is null)
            return;

        var eventType = typeof(TEvent);

        if (_subscribers.TryGetValue(eventType, out var handlers))
        {
            lock (_syncLock)
            {
                handlers.Remove(handler);
            }

            _logger.LogInformation("Handler {HandlerName} unsubscribed from {EventType}",
                handler.GetHandlerName(), eventType.Name);
        }
    }

    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : class, IEvent
    {
        if (@event  is null)
            throw new ArgumentNullException(nameof(@event));

        var eventType = typeof(TEvent);

        _logger.LogInformation("Publishing event {EventType} with ID {CorrelationId}",
            eventType.Name, @event.CorrelationId);

        if (!_subscribers.TryGetValue(eventType, out var handlers) || handlers.Count == 0)
        {
            _logger.LogWarning("No subscribers for event {EventType}", eventType.Name);
            return;
        }

        // Create a copy of handlers list to avoid modification during iteration
        List<object> handlersCopy;
        lock (_syncLock)
        {
            handlersCopy = new List<object>(handlers);
        }

        var tasks = new List<Task>();

        foreach (var handler in handlersCopy)
        {
            // Use reflection to call the handler
            var handleMethod = handler.GetType()
                .GetMethod("HandleAsync", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            if (handleMethod  is not null)
            {
                try
                {
                    var task = (Task)handleMethod.Invoke(handler, new object[] { @event })!;
                    tasks.Add(task);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error invoking handler for event {EventType}",
                        eventType.Name);
                }
            }
        }

        // Wait for all handlers to complete
        try
        {
            await Task.WhenAll(tasks).ConfigureAwait(false);
            _logger.LogInformation("Event {EventType} published to {Count} handlers successfully",
                eventType.Name, handlersCopy.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "One or more event handlers failed for {EventType}",
                eventType.Name);
            throw;
        }
    }

    public void Clear()
    {
        lock (_syncLock)
        {
            _subscribers.Clear();
        }

        _logger.LogInformation("Event bus cleared, all subscriptions removed");
    }

    public int GetSubscriberCount<TEvent>() where TEvent : class, IEvent
    {
        var eventType = typeof(TEvent);

        if (_subscribers.TryGetValue(eventType, out var handlers))
        {
            lock (_syncLock)
            {
                return handlers.Count;
            }
        }

        return 0;
    }

    /// <summary>
    /// Gets all registered event types.
    /// </summary>
    public IEnumerable<Type> GetRegisteredEventTypes()
    {
        return _subscribers.Keys;
    }
}