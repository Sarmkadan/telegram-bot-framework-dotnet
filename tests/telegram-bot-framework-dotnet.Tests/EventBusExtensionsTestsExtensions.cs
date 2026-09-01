#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramBotFramework.Events;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Extension methods for <see cref="EventBusExtensionsTests"/> to simplify common test scenarios.
/// </summary>
public static class EventBusExtensionsTestsExtensions
{
    /// <summary>
    /// Subscribes a handler that captures all handled events for later verification.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <typeparam name="TEvent">The type of event to handle.</typeparam>
    /// <returns>A list that will contain all handled events of type TEvent.</returns>
    public static IList<TEvent> SubscribeAndCaptureEvents<TEvent>(this EventBusExtensionsTests tests) where TEvent : class, IEvent
    {
        ArgumentNullException.ThrowIfNull(tests);

        var handledEvents = new List<TEvent>();
        var handler = new DelegateEventHandler<TEvent>(async @event =>
        {
            handledEvents.Add(@event);
            return Task.CompletedTask;
        });

        tests._eventBus.Subscribe(handler);
        return handledEvents;
    }

    /// <summary>
    /// Subscribes multiple handlers that capture handled events for later verification.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="handlerCount">The number of handlers to subscribe.</param>
    /// <typeparam name="TEvent">The type of event to handle.</typeparam>
    /// <returns>An array of lists, where each list contains events handled by the corresponding handler.</returns>
    public static IList<TEvent>[] SubscribeAndCaptureEvents<TEvent>(this EventBusExtensionsTests tests, int handlerCount) where TEvent : class, IEvent
    {
        ArgumentNullException.ThrowIfNull(tests);
        if (handlerCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(handlerCount));

        var handledEvents = new List<TEvent>[handlerCount];
        for (int i = 0; i < handlerCount; i++)
        {
            var handlerIndex = i; // Capture for closure
            handledEvents[i] = new List<TEvent>();
            var handler = new DelegateEventHandler<TEvent>(async @event =>
            {
                handledEvents[handlerIndex].Add(@event);
                return Task.CompletedTask;
            });

            tests._eventBus.Subscribe(handler);
        }

        return handledEvents;
    }

    /// <summary>
    /// Publishes multiple events of the same type and returns the published events.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="eventCount">The number of events to publish.</param>
    /// <typeparam name="TEvent">The type of event to publish.</typeparam>
    /// <returns>A list of the published events.</returns>
    public static async Task<IList<TEvent>> PublishManyAsync<TEvent>(this EventBusExtensionsTests tests, int eventCount) where TEvent : class, IEvent, new()
    {
        ArgumentNullException.ThrowIfNull(tests);
        if (eventCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(eventCount));

        var events = new List<TEvent>();
        for (int i = 0; i < eventCount; i++)
        {
            var @event = new TEvent();
            events.Add(@event);
            await tests._eventBus.PublishAsync(@event);
        }

        return events;
    }

    /// <summary>
    /// Verifies that the event bus has the expected number of subscribers for a specific event type.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="expectedCount">The expected number of subscribers.</param>
    /// <typeparam name="TEvent">The type of event to check.</typeparam>
    public static void ShouldHaveSubscriberCount<TEvent>(this EventBusExtensionsTests tests, int expectedCount) where TEvent : class, IEvent
    {
        ArgumentNullException.ThrowIfNull(tests);
        if (expectedCount < 0)
            throw new ArgumentOutOfRangeException(nameof(expectedCount));

        var actualCount = tests._eventBus.GetSubscriberCount<TEvent>();
        actualCount.Should().Be(expectedCount);
    }
}