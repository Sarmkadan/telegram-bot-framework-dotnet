#nullable enable

using System;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Extension methods for <see cref="EventBusExtensionsTests"/> to provide useful testing utilities.
/// </summary>
public static class EventBusExtensionsTestsExtensions
{
    /// <summary>
    /// Determines whether the event is registered when the event type is registered.
    /// </summary>
    /// <param name="tests">The event bus extensions tests instance.</param>
    /// <returns>True if the event registration test passes; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="tests"/> is null.</exception>
    public static bool IsEventRegisteredWhenEventTypeIsRegistered(this EventBusExtensionsTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);
        try
        {
            tests.IsEventRegistered_WhenEventTypeRegistered_ReturnsTrue();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Determines whether the event is not registered when the event type is not registered.
    /// </summary>
    /// <param name="tests">The event bus extensions tests instance.</param>
    /// <returns>True if the event registration test passes; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="tests"/> is null.</exception>
    public static bool IsEventRegisteredWhenEventTypeIsNotRegistered(this EventBusExtensionsTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);
        try
        {
            tests.IsEventRegistered_WhenEventTypeNotRegistered_ReturnsFalse();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Determines whether the total subscriber count is zero when no handlers are present.
    /// </summary>
    /// <param name="tests">The event bus extensions tests instance.</param>
    /// <returns>True if the subscriber count test passes; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="tests"/> is null.</exception>
    public static bool GetTotalSubscriberCountWhenNoHandlers(this EventBusExtensionsTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);
        try
        {
            tests.GetTotalSubscriberCount_WhenNoHandlers_ReturnsZero();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Determines whether the total subscriber count is one when a single handler is present.
    /// </param>
    /// <param name="tests">The event bus extensions tests instance.</param>
    /// <returns>True if the subscriber count test passes; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="tests"/> is null.</exception>
    public static bool GetTotalSubscriberCountWithSingleHandler(this EventBusExtensionsTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);
        try
        {
            tests.GetTotalSubscriberCount_WithSingleHandler_ReturnsOne();
            return true;
        }
        catch
        {
            return false;
        }
    }
}