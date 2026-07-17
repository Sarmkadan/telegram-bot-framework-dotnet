#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Events;

/// <summary>
/// Extension methods for <see cref="EventPublisher"/> that provide additional convenience functionality
/// for common event publishing scenarios.
/// </summary>
public static class EventPublisherExtensions
{
    /// <summary>
    /// Publishes a message received event with the specified message text.
    /// </summary>
    /// <param name="publisher">The event publisher instance.</param>
    /// <param name="chatId">The chat identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="messageText">The message text. If <see langword="null"/> or empty, an empty string will be used.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="publisher"/> is <see langword="null"/>.</exception>
    public static async Task PublishMessageReceivedAsync(this EventPublisher publisher, long chatId, long userId, string? messageText = null)
    {
        ArgumentNullException.ThrowIfNull(publisher);

        await publisher.PublishMessageReceivedAsync(chatId, userId, messageText ?? string.Empty).ConfigureAwait(false);
    }

    /// <summary>
    /// Publishes a command executed event with the specified parameters.
    /// </summary>
    /// <param name="publisher">The event publisher instance.</param>
    /// <param name="commandName">The name of the command that was executed.</param>
    /// <param name="userId">The user identifier who executed the command.</param>
    /// <param name="arguments">The command arguments. Can be <see langword="null"/> if no arguments were provided.</param>
    /// <param name="success">Whether the command execution was successful.</param>
    /// <param name="errorMessage">Optional error message if the command failed. Can be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="publisher"/> or <paramref name="commandName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="commandName"/> is empty or whitespace.</exception>
    public static async Task PublishCommandExecutedAsync(
        this EventPublisher publisher,
        string commandName,
        long userId,
        string? arguments = null,
        bool success = true,
        string? errorMessage = null)
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentNullException.ThrowIfNull(commandName);

        if (string.IsNullOrWhiteSpace(commandName))
        {
            throw new ArgumentException("Command name cannot be empty or whitespace.", nameof(commandName));
        }

        await publisher.PublishCommandExecutedAsync(commandName, userId, arguments, success, errorMessage).ConfigureAwait(false);
    }

    /// <summary>
    /// Publishes a bot state changed event with the specified state names.
    /// </summary>
    /// <param name="publisher">The event publisher instance.</param>
    /// <param name="newState">The new state the bot transitioned to.</param>
    /// <param name="previousState">The previous state the bot was in. Can be <see langword="null"/> if there was no previous state.</param>
    /// <param name="reason">Optional reason for the state change. Can be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="publisher"/> or <paramref name="newState"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="newState"/> is empty or whitespace.</exception>
    public static async Task PublishBotStateChangedAsync(
        this EventPublisher publisher,
        string newState,
        string? previousState = null,
        string? reason = null)
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentNullException.ThrowIfNull(newState);

        if (string.IsNullOrWhiteSpace(newState))
        {
            throw new ArgumentException("New state cannot be empty or whitespace.", nameof(newState));
        }

        await publisher.PublishBotStateChangedAsync(previousState ?? string.Empty, newState, reason).ConfigureAwait(false);
    }

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