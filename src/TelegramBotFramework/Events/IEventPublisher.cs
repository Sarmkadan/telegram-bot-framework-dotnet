using TelegramBotFramework.Integration;

namespace TelegramBotFramework.Events
{
    /// <summary>
    /// Publishes bot events to the event bus.
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// Creates an event publisher that attaches the specified correlation ID to the events it publishes.
        /// </summary>
        /// <param name="correlationId">The correlation ID used to link related events together.</param>
        /// <returns>An <see cref="EventPublisher"/> bound to the given correlation ID.</returns>
        EventPublisher WithCorrelationId(string correlationId);

        /// <summary>
        /// Publishes an event indicating that a message was received from a chat.
        /// </summary>
        /// <param name="chatId">The identifier of the chat the message was received in.</param>
        /// <param name="userId">The identifier of the user who sent the message.</param>
        /// <param name="messageText">The text of the message, or <see langword="null"/> if the message has no text.</param>
        /// <returns>A task that represents the asynchronous publish operation.</returns>
        Task PublishMessageReceivedAsync(long chatId, long userId, string? messageText);

        /// <summary>
        /// Publishes an event indicating that a command was executed.
        /// </summary>
        /// <param name="commandName">The name of the executed command.</param>
        /// <param name="userId">The identifier of the user who executed the command.</param>
        /// <param name="arguments">The arguments passed to the command, or <see langword="null"/> if there were none.</param>
        /// <param name="success">A value indicating whether the command completed successfully.</param>
        /// <param name="errorMessage">The error message if the command failed; otherwise, <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous publish operation.</returns>
        Task PublishCommandExecutedAsync(string commandName, long userId, string? arguments, bool success, string? errorMessage = null);

        /// <summary>
        /// Publishes an event indicating that the bot state has changed.
        /// </summary>
        /// <param name="previousState">The state the bot was in before the change.</param>
        /// <param name="newState">The state the bot transitioned to.</param>
        /// <param name="reason">The reason for the state change, or <see langword="null"/> if no reason was given.</param>
        /// <returns>A task that represents the asynchronous publish operation.</returns>
        Task PublishBotStateChangedAsync(string previousState, string newState, string? reason = null);

        /// <summary>
        /// Publishes the specified event to the event bus.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event being published.</typeparam>
        /// <param name="event">The event to publish.</param>
        /// <returns>A task that represents the asynchronous publish operation.</returns>
        Task PublishAsync<TEvent>(TEvent @event) where TEvent : class, IEvent;
    }
}