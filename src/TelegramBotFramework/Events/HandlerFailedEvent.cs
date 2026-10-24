#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace TelegramBotFramework.Events;

/// <summary>
/// Published to the event bus when a handler throws while processing an event.
/// Lets observers react to handler faults without the faulting handler stopping the dispatch of other handlers.
/// </summary>
public sealed class HandlerFailedEvent : EventBase
{
    /// <summary>
    /// Gets the name of the handler that threw.
    /// </summary>
    public string HandlerName { get; }

    /// <summary>
    /// Gets the type name of the event the handler was processing.
    /// </summary>
    public string FailedEventType { get; }

    /// <summary>
    /// Gets the exception thrown by the handler.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HandlerFailedEvent"/> class.
    /// </summary>
    /// <param name="handlerName">The name of the handler that threw.</param>
    /// <param name="failedEventType">The type name of the event the handler was processing.</param>
    /// <param name="exception">The exception thrown by the handler.</param>
    /// <param name="correlationId">The correlation identifier, typically copied from the failed event.</param>
    /// <exception cref="ArgumentException"><paramref name="handlerName"/> or <paramref name="failedEventType"/> is <c>null</c> or empty.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <c>null</c>.</exception>
    public HandlerFailedEvent(string handlerName, string failedEventType, Exception exception, string? correlationId = null)
    : base(correlationId)
    {
        ArgumentException.ThrowIfNullOrEmpty(handlerName);
        ArgumentException.ThrowIfNullOrEmpty(failedEventType);
        ArgumentNullException.ThrowIfNull(exception);

        HandlerName = handlerName;
        FailedEventType = failedEventType;
        Exception = exception;
    }
}
