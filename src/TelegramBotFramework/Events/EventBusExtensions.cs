using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace TelegramBotFramework.Events
{
    /// <summary>
    /// Extension methods that add useful capabilities to <see cref="EventBus"/>.
    /// </summary>
    public static class EventBusExtensions
    {
        /// <summary>
        /// Determines whether the specified event type is registered in the bus.
        /// </summary>
        /// <typeparam name="TEvent">The event type to check.</typeparam>
        /// <param name="bus">The <see cref="EventBus"/> instance.</param>
        /// <returns><c>true</c> if the event type is registered; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bus"/> is <c>null</c>.</exception>
        public static bool IsEventRegistered<TEvent>(this EventBus bus)
        {
            ArgumentNullException.ThrowIfNull(bus);
            return bus.GetRegisteredEventTypes().Contains(typeof(TEvent));
        }

        /// <summary>
        /// Calculates the total number of subscribers across all registered event types.
        /// </summary>
        /// <param name="bus">The <see cref="EventBus"/> instance.</param>
        /// <returns>The sum of subscriber counts for every registered event type.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bus"/> is <c>null</c>.</exception>
        public static int GetTotalSubscriberCount(this EventBus bus)
        {
            ArgumentNullException.ThrowIfNull(bus);

            return bus.GetRegisteredEventTypes()
                .Select(type => bus.GetSubscriberCount(type))
                .Sum();
        }

        /// <summary>
        /// Gets the subscriber count for a specific event type.
        /// </summary>
        /// <param name="bus">The <see cref="EventBus"/> instance.</param>
        /// <param name="eventType">The event type to check.</param>
        /// <returns>The number of subscribers for the specified event type, or 0 if not found.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bus"/> or <paramref name="eventType"/> is <c>null</c>.</exception>
        private static int GetSubscriberCount(this EventBus bus, Type eventType)
        {
            ArgumentNullException.ThrowIfNull(bus);
            ArgumentNullException.ThrowIfNull(eventType);

            var method = typeof(EventBus).GetMethod(
                nameof(EventBus.GetSubscriberCount),
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { typeof(Type) },
                null);

            return method is null
                ? 0
                : (int)method.Invoke(bus, new object[] { eventType })!;
        }

        /// <summary>
        /// Publishes a collection of events concurrently. Each event is published using its runtime type.
        /// </summary>
        /// <param name="bus">The <see cref="EventBus"/> instance.</param>
        /// <param name="events">The events to publish.</param>
        /// <returns>A <see cref="Task"/> that completes when all publish operations have finished.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bus"/> or <paramref name="events"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="events"/> contains a <c>null</c> element.</exception>
        public static Task PublishManyAsync(this EventBus bus, IEnumerable<object> events)
        {
            ArgumentNullException.ThrowIfNull(bus);
            ArgumentNullException.ThrowIfNull(events);

            // Materialize the sequence to avoid multiple enumeration
            var eventList = events as IList<object> ?? events.ToList();

            // No events to publish – return a completed task
            if (eventList.Count == 0)
            {
                return Task.CompletedTask;
            }

            // Validate all events are non-null
            if (eventList.Any(e => e is null))
            {
                throw new ArgumentException("Event collection contains null elements.", nameof(events));
            }

            var publishTasks = eventList
                .Select(e => bus.PublishAsync(e.GetType(), e))
                .ToList();

            return Task.WhenAll(publishTasks);
        }

        /// <summary>
        /// Publishes an event using its runtime type.
        /// </summary>
        /// <param name="bus">The <see cref="EventBus"/> instance.</param>
        /// <param name="eventType">The runtime type of the event.</param>
        /// <param name="event">The event to publish.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bus"/>, <paramref name="eventType"/>, or <paramref name="event"/> is <c>null</c>.</exception>
        private static Task PublishAsync(this EventBus bus, Type eventType, object @event)
        {
            ArgumentNullException.ThrowIfNull(bus);
            ArgumentNullException.ThrowIfNull(eventType);
            ArgumentNullException.ThrowIfNull(@event);

            var method = typeof(EventBus).GetMethod(
                nameof(EventBus.PublishAsync),
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { eventType },
                null);

            if (method is null)
            {
                throw new InvalidOperationException(
                    $"No PublishAsync method found for event type {eventType.FullName}.");
            }

            return (Task)method.Invoke(bus, new[] { @event })!;
        }
    }
}
