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
    private readonly List<IEventMiddleware> _middleware = new();

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
            // Get middleware in order
            List<IEventMiddleware> middlewareList = _middleware.ToList();

            // Create a copy to avoid issues if handlers are modified during iteration
            var handlersCopy = handlers.ToArray();

            // Create the handler invocation function
            async Task InvokeHandlers()
            {
                foreach (var handler in handlersCopy)
                {
                    if (handler is IEventHandler<TEvent> typedHandler)
                    {
                        await typedHandler.HandleAsync(@event).ConfigureAwait(false);
                    }
                }
            }

            // Create a recursive middleware invocation function
            // This creates a chain: Middleware1 -> Middleware2 -> ... -> Handlers
            Func<Task> BuildMiddlewareChain(int index)
            {
                if (index >= middlewareList.Count)
                {
                    // Base case: invoke all handlers
                    return InvokeHandlers;
                }

                // Get the current middleware
                var currentMiddleware = middlewareList[index];
                var nextMiddleware = BuildMiddlewareChain(index + 1);

                // Return a function that invokes this middleware with the next function
                return () => currentMiddleware.InvokeAsync(@event, nextMiddleware);
            }

            // Execute middleware pipeline starting from the first middleware
            // The chain will be: Middleware[0] -> Middleware[1] -> ... -> Handlers
            var middlewareChain = BuildMiddlewareChain(0);
            await middlewareChain().ConfigureAwait(false);
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
    /// Registers middleware to be executed around event handling.
    /// Middleware is executed in the order it's registered.
    /// </summary>
    /// <param name="middleware">The middleware to register.</param>
    public void RegisterMiddleware(IEventMiddleware middleware)
    {
        if (middleware is null)
        {
            throw new ArgumentNullException(nameof(middleware));
        }

        if (!_middleware.Contains(middleware))
        {
            _middleware.Add(middleware);
        }
    }

    /// <summary>
    /// Gets all registered middleware in the order they were registered.
    /// </summary>
    /// <returns>An enumerable of middleware instances.</returns>
    public IEnumerable<IEventMiddleware> GetMiddleware()
    {
        return _middleware.ToList();
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

    /// <summary>
    /// Context for middleware execution that provides the next delegate.
    /// </summary>
    /// <typeparam name="TEvent">The type of event being processed.</typeparam>
    private sealed class MiddlewareContext<TEvent> where TEvent : class, IEvent
    {
        private readonly TEvent _event;
        private readonly Func<Task> _handlerInvoker;

        public MiddlewareContext(TEvent @event, Func<Task> handlerInvoker)
        {
            _event = @event ?? throw new ArgumentNullException(nameof(@event));
            _handlerInvoker = handlerInvoker ?? throw new ArgumentNullException(nameof(handlerInvoker));
        }

        public async Task Next()
        {
            // This will be called by middleware to continue to the next middleware or handlers
            await _handlerInvoker().ConfigureAwait(false);
        }
    }
}