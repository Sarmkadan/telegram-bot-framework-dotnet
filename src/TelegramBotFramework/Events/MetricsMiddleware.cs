#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace TelegramBotFramework.Events;

/// <summary>
/// Middleware for collecting metrics and monitoring event processing performance.
/// Tracks event processing times, counts, and success rates.
/// </summary>
public sealed class MetricsMiddleware : IEventMiddleware
{
    private readonly ILogger<MetricsMiddleware> _logger;
    private readonly Dictionary<string, EventMetrics> _eventMetrics = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MetricsMiddleware"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public MetricsMiddleware(ILogger<MetricsMiddleware>? logger = null)
    {
        _logger = logger ?? new ConsoleLogger<MetricsMiddleware>();
    }

    /// <summary>
    /// Gets the name of this middleware for logging/debugging.
    /// </summary>
    public string MiddlewareName => nameof(MetricsMiddleware);

    /// <summary>
    /// Invokes the middleware with metrics collection around event processing.
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

        var eventType = evt.EventType;
        var correlationId = evt.CorrelationId ?? "unknown";

        // Start timing
        var startTime = DateTime.UtcNow;
        var metrics = GetMetrics(eventType);
        metrics.TotalProcessed++;

        _logger.LogDebug("Metrics: Starting event {EventType} with correlation {CorrelationId}",
            eventType, correlationId);

        try
        {
            await next().ConfigureAwait(false);
            metrics.SuccessCount++;
            metrics.TotalProcessingTime += DateTime.UtcNow - startTime;
            _logger.LogDebug("Metrics: Completed event {EventType} successfully", eventType);
        }
        catch (Exception ex)
        {
            metrics.FailureCount++;
            metrics.TotalProcessingTime += DateTime.UtcNow - startTime;
            _logger.LogError(ex, "Metrics: Failed to process event {EventType}", eventType);
            throw;
        }
        finally
        {
            // Log metrics periodically
            if (metrics.TotalProcessed % 100 == 0)
            {
                LogMetrics(metrics);
            }
        }
    }

    /// <summary>
    /// Gets or creates metrics for a specific event type.
    /// </summary>
    /// <param name="eventType">The event type name.</param>
    /// <returns>The metrics for the event type.</returns>
    private EventMetrics GetMetrics(string eventType)
    {
        lock (_eventMetrics)
        {
            if (!_eventMetrics.TryGetValue(eventType, out var metrics))
            {
                metrics = new EventMetrics(eventType);
                _eventMetrics[eventType] = metrics;
            }
            return metrics;
        }
    }

    /// <summary>
    /// Logs aggregated metrics.
    /// </summary>
    /// <param name="metrics">The metrics to log.</param>
    private void LogMetrics(EventMetrics metrics)
    {
        var avgTime = metrics.TotalProcessed > 0
            ? metrics.TotalProcessingTime.TotalMilliseconds / metrics.TotalProcessed
            : 0;

        _logger.LogInformation("Event Metrics - {EventType}: Total={TotalProcessed}, Success={SuccessCount}, Failures={FailureCount}, AvgTimeMs={AvgTime:F2}",
            metrics.EventType,
            metrics.TotalProcessed,
            metrics.SuccessCount,
            metrics.FailureCount,
            avgTime);
    }

    /// <summary>
    /// Metrics data structure for tracking event processing statistics.
    /// </summary>
    private sealed class EventMetrics
    {
        public string EventType { get; }
        public int TotalProcessed { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public TimeSpan TotalProcessingTime { get; set; }

        public EventMetrics(string eventType)
        {
            EventType = eventType;
        }
    }
}