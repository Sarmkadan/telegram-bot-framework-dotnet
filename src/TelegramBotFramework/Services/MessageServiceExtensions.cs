#nullable enable

namespace TelegramBotFramework.Services;

/// <summary>
/// Extension methods for <see cref="MessageService"/> providing additional message processing utilities.
/// </summary>
public static class MessageServiceExtensions
{
    /// <summary>
    /// Creates and processes a new message in a single call.
    /// </summary>
    /// <param name="messageService">The message service instance.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="content">The message content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created and processed message.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="messageService"/> or <paramref name="content"/> is null.</exception>
    public static async Task<Models.Message> CreateAndProcessMessageAsync(
        this MessageService messageService,
        long userId,
        string content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(messageService);
        ArgumentNullException.ThrowIfNull(content);

        var message = new Models.Message
        {
            UserId = userId,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        return await messageService.ProcessIncomingMessageAsync(message, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a message by its ID or returns null if not found.
    /// </summary>
    /// <param name="messageService">The message service instance.</param>
    /// <param name="messageId">The message identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The message if found, otherwise null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="messageService"/> is null.</exception>
    public static async Task<Models.Message?> TryGetMessageAsync(
        this MessageService messageService,
        long messageId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(messageService);

        return await messageService.GetMessageAsync(messageId, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets all messages for a user and filters them by content.
    /// </summary>
    /// <param name="messageService">The message service instance.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="contentFilter">Optional content filter to search for specific messages.</param>
    /// <param name="limit">Maximum number of messages to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Filtered list of user messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="messageService"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="limit"/> is less than 1.</exception>
    public static async Task<IList<Models.Message>> GetUserMessagesByContentAsync(
        this MessageService messageService,
        long userId,
        string? contentFilter = null,
        int limit = MessageServiceExtensionsConstants.DefaultMessageLimit,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(messageService);
        ArgumentOutOfRangeException.ThrowIfLessThan(limit, MessageServiceExtensionsConstants.MinimumMessageLimit);

        var userMessages = await messageService.GetUserMessagesAsync(userId, limit, cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(contentFilter))
        {
            return userMessages;
        }

        return userMessages
            .Where(m => m.Content.Contains(contentFilter, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Gets the count of messages with a specific status.
    /// </summary>
    /// <param name="messageService">The message service instance.</param>
    /// <param name="status">The message status to count.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Count of messages with the specified status.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="messageService"/> is null.</exception>
    public static async Task<int> GetMessageCountByStatusAsync(
        this MessageService messageService,
        Models.MessageStatus status,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(messageService);

        var messages = await messageService.GetUserMessagesAsync(0, MessageServiceExtensionsConstants.MaximumMessageLimit, cancellationToken).ConfigureAwait(false);

        return messages.Count(m => m.Status == status);
    }

    /// <summary>
    /// Marks multiple messages as processed in a single batch operation.
    /// </summary>
    /// <param name="messageService">The message service instance.</param>
    /// <param name="messageIds">Collection of message identifiers to mark as processed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if all messages were successfully marked, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="messageService"/> or <paramref name="messageIds"/> is null.</exception>
    public static async Task<bool> MarkMessagesAsProcessedAsync(
        this MessageService messageService,
        IEnumerable<long> messageIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(messageService);
        ArgumentNullException.ThrowIfNull(messageIds);

        var allSuccess = true;

        foreach (var messageId in messageIds)
        {
            var success = await messageService.MarkAsProcessedAsync(messageId, cancellationToken).ConfigureAwait(false);
            if (!success)
            {
                allSuccess = false;
            }
        }

        return allSuccess;
    }
}