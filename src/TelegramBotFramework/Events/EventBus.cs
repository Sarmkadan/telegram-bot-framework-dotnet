#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace TelegramBotFramework.Events;

using System.Collections.Concurrent;
using TelegramBotFramework.Integration;

/// <summary>
/// In-process publish-subscribe event bus implementation.
/// Manages event subscriptions and broadcasts events to all registered handlers.
/// Thread-safe for concurrent operations.
/// </summary>
public sealed class EventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<object>> _subscribers = new();
    private readonly List<IEventMiddleware> _middleware = new();
    private readonly ILogger<EventBus> _logger;
    private readonly object _syncLock = new();

    public EventBus(ILogger<EventBus>? logger = null)
    {
        _logger = logger ?? new ConsoleLogger<EventBus>();
    }

    public void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : class, IEvent
    {
        if (handler is null)
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
        if (handler is null)
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
        if (@event is null)
            throw new ArgumentNullException(nameof(@event));

        var eventType = typeof(TEvent);

        _logger.LogInformation("Publishing event {EventType} with ID {CorrelationId}",
            eventType.Name, @event.CorrelationId);

        if (!_subscribers.TryGetValue(eventType, out var handlers) || handlers.Count == 0)
        {
            _logger.LogWarning("No subscribers for event {EventType}", eventType.Name);
            return;
        }

        // Get middleware in order
        List<IEventMiddleware> middlewareList;
        lock (_syncLock)
        {
            middlewareList = _middleware.ToList();
        }

        // Create a copy of handlers list to avoid modification during iteration
        List<object> handlersCopy;
        lock (_syncLock)
        {
            handlersCopy = new List<object>(handlers);
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

        // Create the handler invocation function
        async Task InvokeHandlers()
        {
            var tasks = new List<Task>();

            foreach (var handler in handlersCopy)
            {
                // Use reflection to call the handler
                var handleMethod = handler.GetType()
                    .GetMethod("HandleAsync", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                if (handleMethod is not null)
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

        // Execute middleware pipeline starting from the first middleware
        // The chain will be: Middleware[0] -> Middleware[1] -> ... -> Handlers
        var middlewareChain = BuildMiddlewareChain(0);
        await middlewareChain().ConfigureAwait(false);
    }

    public void Clear()
    {
        lock (_syncLock)
        {
            _subscribers.Clear();
        }

        _logger.LogInformation("Event bus cleared, all subscriptions removed");
    }

    public void RegisterMiddleware(IEventMiddleware middleware)
    {
        if (middleware is null)
            throw new ArgumentNullException(nameof(middleware));

        lock (_syncLock)
        {
            if (!_middleware.Contains(middleware))
            {
                _middleware.Add(middleware);
            }
        }

        _logger.LogInformation("Middleware {MiddlewareName} registered", middleware.MiddlewareName);
    }

    public IEnumerable<IEventMiddleware> GetMiddleware()
    {
        lock (_syncLock)
        {
            return _middleware.ToList();
        }
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