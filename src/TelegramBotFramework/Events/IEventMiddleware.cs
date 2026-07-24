#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace TelegramBotFramework.Events;

/// <summary>
/// Middleware for event processing pipeline.
/// Provides interception points for event handling, allowing for logging,
/// authentication, metrics, throttling, and other cross-cutting concerns.
/// </summary>
public interface IEventMiddleware
{
    /// <summary>
    /// Gets the name of this middleware for logging/debugging.
    /// </summary>
    string MiddlewareName { get; }

    /// <summary>
    /// Invokes the middleware with the given event and next handler in the pipeline.
    /// </summary>
    /// <param name="evt">The event being processed.</param>
    /// <param name="next">The next middleware or handler in the pipeline.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task InvokeAsync(IEvent evt, Func<Task> next);
}

/// <summary>
/// Base class for event middleware with common functionality.
/// </summary>
/// <typeparam name="TEvent">The type of event this middleware processes.</typeparam>
public abstract class EventMiddlewareBase<TEvent> : IEventMiddleware where TEvent : class, IEvent
{
    /// <summary>
    /// Gets the name of this middleware for logging/debugging.
    /// </summary>
    public virtual string MiddlewareName => GetType().Name;

    /// <summary>
    /// Invokes the middleware with type-safe event handling.
    /// </summary>
    /// <param name="evt">The event being processed.</param>
    /// <param name="next">The next middleware or handler in the pipeline.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(IEvent evt, Func<Task> next)
    {
        if (evt is null)
            throw new ArgumentNullException(nameof(evt));

        if (next is null)
            throw new ArgumentNullException(nameof(next));

        // Type-safe pre-processing
        await PreProcessAsync((TEvent)evt).ConfigureAwait(false);

        // Continue to next middleware or handler
        await next().ConfigureAwait(false);

        // Type-safe post-processing
        await PostProcessAsync((TEvent)evt).ConfigureAwait(false);
    }

    /// <summary>
    /// Pre-processing logic executed before the next middleware/handler.
    /// Override this method to implement pre-processing logic.
    /// </summary>
    /// <param name="evt">The event being processed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task PreProcessAsync(TEvent evt) => Task.CompletedTask;

    /// <summary>
    /// Post-processing logic executed after the next middleware/handler completes.
    /// Override this method to implement post-processing logic.
    /// </summary>
    /// <param name="evt">The event being processed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task PostProcessAsync(TEvent evt) => Task.CompletedTask;
}

/// <summary>
/// Middleware that executes logic before and after event handlers.
/// Can be used for logging, metrics, authentication, throttling, etc.
/// </summary>
public sealed class EventProcessingMiddleware : IEventMiddleware
{
    private readonly ILogger<EventProcessingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventProcessingMiddleware"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public EventProcessingMiddleware(ILogger<EventProcessingMiddleware>? logger = null)
    {
        _logger = logger ?? new ConsoleLogger<EventProcessingMiddleware>();
    }

    /// <summary>
    /// Gets the name of this middleware for logging/debugging.
    /// </summary>
    public string MiddlewareName => nameof(EventProcessingMiddleware);

    /// <summary>
    /// Invokes the middleware with logging around event processing.
    /// </summary>
    /// <param name="evt">The event being processed.</param>
    /// <param name="next">The next middleware or handler in the pipeline.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(IEvent evt, Func<Task> next)
    {
        if (evt is null)
            throw new ArgumentNullException(nameof(evt));

        if (next is null)
            throw new ArgumentNullException(nameof(next));

        _logger.LogInformation("Event middleware: Starting processing for {EventType} with ID {CorrelationId}",
            evt.EventType, evt.CorrelationId);

        try
        {
            await next().ConfigureAwait(false);
            _logger.LogInformation("Event middleware: Completed processing for {EventType}", evt.EventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Event middleware: Error processing {EventType}", evt.EventType);
            throw;
        }
    }
}