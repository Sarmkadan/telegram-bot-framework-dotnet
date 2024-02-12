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
                .Select(type =>
                {
                    // Resolve the generic method GetSubscriberCount<TEvent>()
                    MethodInfo? genericMethod = typeof(EventBus)
                        .GetMethod(nameof(EventBus.GetSubscriberCount), BindingFlags.Public | BindingFlags.Instance)?
                        .MakeGenericMethod(type);

                    // If the method cannot be resolved, treat the count as zero.
                    return genericMethod is null ? 0 : (int)genericMethod.Invoke(bus, null)!;
                })
                .Sum();
        }

        /// <summary>
        /// Publishes a collection of events concurrently. Each event is published using its runtime type.
        /// </summary>
        /// <param name="bus">The <see cref="EventBus"/> instance.</param>
        /// <param name="events">The events to publish.</param>
        /// <returns>A <see cref="Task"/> that completes when all publish operations have finished.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bus"/> or <paramref name="events"/> is <c>null</c>.</exception>
        public static Task PublishManyAsync(this EventBus bus, IEnumerable<object> events)
        {
            ArgumentNullException.ThrowIfNull(bus);
            ArgumentNullException.ThrowIfNull(events);

            // Materialise the sequence to avoid multiple enumeration.
            var eventList = events as IList<object> ?? events.ToList();

            // No events to publish – return a completed task.
            if (eventList.Count == 0)
                return Task.CompletedTask;

            IEnumerable<Task> publishTasks = eventList.Select(e =>
            {
                Type eventType = e.GetType();

                // Resolve the generic method PublishAsync<TEvent>(TEvent @event)
                MethodInfo? genericMethod = typeof(EventBus)
                    .GetMethod(nameof(EventBus.PublishAsync), BindingFlags.Public | BindingFlags.Instance)?
                    .MakeGenericMethod(eventType);

                // The method is guaranteed to exist; invoke it and cast the result to Task.
                return (Task)genericMethod!.Invoke(bus, new[] { e })!;
            });

            return Task.WhenAll(publishTasks);
        }
    }
}
