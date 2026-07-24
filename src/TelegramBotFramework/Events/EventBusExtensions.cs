using System;
using System.Collections.Generic;
using System.Linq;

namespace TelegramBotFramework.Events
{
    /// <summary>
    /// Extension methods that add useful capabilities to <see cref="EventBus"/> and <see cref="IEventBus"/>.
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
        /// Registers middleware to be executed around event handling.
        /// Middleware is executed in the order it's registered.
        /// </summary>
        /// <param name="bus">The event bus instance.</param>
        /// <param name="middleware">The middleware to register.</param>
        /// <returns>The event bus instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bus"/> or <paramref name="middleware"/> is <c>null</c>.</exception>
        public static IEventBus UseMiddleware(this IEventBus bus, IEventMiddleware middleware)
        {
            ArgumentNullException.ThrowIfNull(bus);
            ArgumentNullException.ThrowIfNull(middleware);

            bus.RegisterMiddleware(middleware);
            return bus;
        }

        /// <summary>
        /// Registers multiple middleware components to be executed around event handling.
        /// Middleware is executed in the order it's provided.
        /// </summary>
        /// <param name="bus">The event bus instance.</param>
        /// <param name="middleware">The middleware components to register.</param>
        /// <returns>The event bus instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bus"/> or <paramref name="middleware"/> is <c>null</c>.</exception>
        public static IEventBus UseMiddleware(this IEventBus bus, params IEventMiddleware[] middleware)
        {
            ArgumentNullException.ThrowIfNull(bus);
            ArgumentNullException.ThrowIfNull(middleware);

            foreach (var m in middleware)
            {
                bus.RegisterMiddleware(m);
            }

            return bus;
        }
    }
}