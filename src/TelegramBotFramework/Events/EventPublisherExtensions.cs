#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace TelegramBotFramework.Events;

/// <summary>
/// Extension methods for <see cref="EventPublisher"/> that provide additional convenience functionality
/// for common event publishing scenarios.
/// </summary>
public static class EventPublisherExtensions
{
    /// <summary>
    /// Publishes an event with correlation ID tracking automatically set.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to publish.</typeparam>
    /// <param name="publisher">The event publisher instance.</param>
    /// <param name="event">The event to publish.</param>
    /// <param name="correlationId">The correlation ID to associate with the event.</param>
    /// <returns>A task that represents the asynchronous publish operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="publisher"/>, <paramref name="event"/>, or <paramref name="correlationId"/> is <see langword="null"/>.</exception>
    public static async Task PublishWithCorrelationAsync<TEvent>(
        this EventPublisher publisher,
        TEvent @event,
        string correlationId) where TEvent : class, IEvent
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(correlationId);

        await publisher
            .WithCorrelationId(correlationId)
            .PublishAsync(@event)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Publishes a collection of events in sequence with correlation ID tracking.
    /// </summary>
    /// <typeparam name="TEvent">The type of events in the collection.</typeparam>
    /// <param name="publisher">The event publisher instance.</param>
    /// <param name="events">The collection of events to publish.</param>
    /// <param name="correlationId">The correlation ID to associate with all events.</param>
    /// <returns>A task that represents the asynchronous publish operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="publisher"/>, <paramref name="events"/>, or <paramref name="correlationId"/> is <see langword="null"/>.</exception>
    public static async Task PublishCollectionAsync<TEvent>(
        this EventPublisher publisher,
        IEnumerable<TEvent> events,
        string correlationId) where TEvent : class, IEvent
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentNullException.ThrowIfNull(events);
        ArgumentNullException.ThrowIfNull(correlationId);

        foreach (var @event in events)
        {
            await publisher
                .WithCorrelationId(correlationId)
                .PublishAsync(@event)
                .ConfigureAwait(false);
        }
    }
}