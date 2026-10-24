#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;

namespace TelegramBotFramework.Events;

/// <summary>
/// Invokes the handlers of a single event with per-handler error isolation, shared by the event bus implementations.
/// Every handler runs even when another one throws; failures are logged, published as <see cref="HandlerFailedEvent"/>,
/// and then surfaced according to <see cref="EventDispatchOptions.ThrowOnHandlerFailure"/>.
/// </summary>
internal static class EventHandlerDispatcher
{
    /// <summary>
    /// Dispatches an event to the given handlers.
    /// </summary>
    /// <typeparam name="TEvent">The type of event being dispatched.</typeparam>
    /// <param name="handlers">The handlers to invoke, in subscription order.</param>
    /// <param name="event">The event to dispatch.</param>
    /// <param name="options">The dispatch options.</param>
    /// <param name="logger">The logger used to report handler failures.</param>
    /// <param name="publishFailureAsync">Publishes a <see cref="HandlerFailedEvent"/> to the bus.</param>
    /// <returns>A task that completes once every handler has finished.</returns>
    /// <exception cref="InvalidOperationException">The <see cref="EventDispatchOptions.Mode"/> is not supported.</exception>
    /// <exception cref="Exception">A single handler failed and <see cref="EventDispatchOptions.ThrowOnHandlerFailure"/> is <c>true</c>.</exception>
    /// <exception cref="AggregateException">Several handlers failed and <see cref="EventDispatchOptions.ThrowOnHandlerFailure"/> is <c>true</c>.</exception>
    public static async Task DispatchAsync<TEvent>(
        IReadOnlyList<IEventHandler<TEvent>> handlers,
        TEvent @event,
        EventDispatchOptions options,
        ILogger logger,
        Func<HandlerFailedEvent, Task> publishFailureAsync)
        where TEvent : class, IEvent
    {
        ArgumentNullException.ThrowIfNull(handlers);
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(publishFailureAsync);

        IReadOnlyList<HandlerFailedEvent> failures = options.Mode switch
        {
            EventDispatchMode.Sequential => await InvokeSequentiallyAsync(handlers, @event, logger).ConfigureAwait(false),
            EventDispatchMode.Parallel => await InvokeInParallelAsync(handlers, @event, options.MaxDegreeOfParallelism, logger)
                .ConfigureAwait(false),
            _ => throw new InvalidOperationException($"Unsupported event dispatch mode '{options.Mode}'.")
        };

        if (failures.Count == 0)
        {
            return;
        }

        // A failing HandlerFailedEvent handler must not publish yet another HandlerFailedEvent, which would recurse forever.
        if (@event is not HandlerFailedEvent)
        {
            foreach (var failure in failures)
            {
                await publishFailureAsync(failure).ConfigureAwait(false);
            }
        }

        if (options.ThrowOnHandlerFailure)
        {
            ThrowFailures(failures);
        }
    }

    private static async Task<IReadOnlyList<HandlerFailedEvent>> InvokeSequentiallyAsync<TEvent>(
        IReadOnlyList<IEventHandler<TEvent>> handlers,
        TEvent @event,
        ILogger logger)
        where TEvent : class, IEvent
    {
        var failures = new List<HandlerFailedEvent>();

        foreach (var handler in handlers)
        {
            if (await InvokeHandlerAsync(handler, @event, logger).ConfigureAwait(false) is { } failure)
            {
                failures.Add(failure);
            }
        }

        return failures;
    }

    private static async Task<IReadOnlyList<HandlerFailedEvent>> InvokeInParallelAsync<TEvent>(
        IReadOnlyList<IEventHandler<TEvent>> handlers,
        TEvent @event,
        int maxDegreeOfParallelism,
        ILogger logger)
        where TEvent : class, IEvent
    {
        var failures = new ConcurrentQueue<HandlerFailedEvent>();
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism };

        await Parallel.ForEachAsync(handlers, parallelOptions, async (handler, _) =>
        {
            if (await InvokeHandlerAsync(handler, @event, logger).ConfigureAwait(false) is { } failure)
            {
                failures.Enqueue(failure);
            }
        }).ConfigureAwait(false);

        return failures.ToArray();
    }

    /// <summary>
    /// Invokes one handler, converting any exception it throws (synchronous or asynchronous) into a failure record.
    /// </summary>
    private static async Task<HandlerFailedEvent?> InvokeHandlerAsync<TEvent>(
        IEventHandler<TEvent> handler,
        TEvent @event,
        ILogger logger)
        where TEvent : class, IEvent
    {
        var handlerName = handler.GetHandlerName();

        try
        {
            await handler.HandleAsync(@event).ConfigureAwait(false);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Handler {HandlerName} failed while handling event {EventType} with ID {CorrelationId}",
                handlerName, @event.EventType, @event.CorrelationId);
            return new HandlerFailedEvent(handlerName, @event.EventType, ex, @event.CorrelationId);
        }
    }

    /// <summary>
    /// Rethrows the collected failures: the original exception when there is exactly one, otherwise an aggregate.
    /// </summary>
    private static void ThrowFailures(IReadOnlyList<HandlerFailedEvent> failures)
    {
        switch (failures)
        {
            case []:
                return;
            case [var single]:
                ExceptionDispatchInfo.Throw(single.Exception);
                return;
            default:
                throw new AggregateException(failures.Select(failure => failure.Exception));
        }
    }
}
