#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace TelegramBotFramework.Services;

/// <summary>
/// Implementation of message processing service.
/// </summary>
public sealed class MessageService : IMessageService
{
    private readonly Repositories.IMessageRepository _messageRepository;
    private readonly Integration.ITelegramApiClient _telegramApiClient;
    private readonly Microsoft.Extensions.Logging.ILogger<MessageService> _logger;

    public MessageService(
        Repositories.IMessageRepository messageRepository,
        Integration.ITelegramApiClient telegramApiClient,
        Microsoft.Extensions.Logging.ILogger<MessageService> logger)
    {
        _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
        _telegramApiClient = telegramApiClient ?? throw new ArgumentNullException(nameof(telegramApiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Models.Message> ProcessIncomingMessageAsync(
        Models.Message message,
        CancellationToken cancellationToken = default)
    {
        message.Validate();
        message.Status = Models.MessageStatus.Processing;
        var created = await _messageRepository.CreateAsync(message, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Message received from user {UserId}: {MessageContent}", message.UserId, message.Content);
        return created;
    }

    public async Task<Models.Message?> GetMessageAsync(long messageId, CancellationToken cancellationToken = default)
    {
        return await _messageRepository.GetByIdAsync(messageId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IList<Models.Message>> GetUserMessagesAsync(
        long userId,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var userMessages = await _messageRepository.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
        return userMessages
        .OrderByDescending(m => m.CreatedAt)
        .Take(limit)
        .ToList();
    }

    public async Task<IList<Models.Message>> GetFailedMessagesAsync(
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var failedMessages = await _messageRepository.GetByStatusAsync(Models.MessageStatus.Failed, cancellationToken).ConfigureAwait(false);
        return failedMessages
        .OrderByDescending(m => m.CreatedAt)
        .Take(limit)
        .ToList();
    }

    public async Task<bool> MarkAsProcessedAsync(long messageId, CancellationToken cancellationToken = default)
    {
        var message = await _messageRepository.GetByIdAsync(messageId, cancellationToken).ConfigureAwait(false);
        if (message is null)
        {
            return false;
        }

        message.MarkAsProcessed();
        await _messageRepository.UpdateAsync(message, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Message marked as processed: {MessageId}", messageId);
        return true;
    }

    public async Task<bool> MarkAsFailedAsync(long messageId, string errorMessage, CancellationToken cancellationToken = default)
    {
        var message = await _messageRepository.GetByIdAsync(messageId, cancellationToken).ConfigureAwait(false);
        if (message is null)
        {
            return false;
        }

        message.MarkAsFailed(errorMessage);
        await _messageRepository.UpdateAsync(message, cancellationToken).ConfigureAwait(false);
        _logger.LogWarning("Message marked as failed: {MessageId} - {Error}", messageId, errorMessage);
        return true;
    }

    public async Task<int> GetUnprocessedMessageCountAsync(CancellationToken cancellationToken = default)
    {
        var processingMessages = await _messageRepository.GetByStatusAsync(Models.MessageStatus.Processing, cancellationToken).ConfigureAwait(false);
        var receivedMessages = await _messageRepository.GetByStatusAsync(Models.MessageStatus.Received, cancellationToken).ConfigureAwait(false);
        return processingMessages.Count + receivedMessages.Count;
    }

    public async Task ArchiveOldMessagesAsync(int daysOld = 30, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
        var allMessages = await _messageRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var messagesForArchiving = allMessages
        .Where(m => m.CreatedAt < cutoffDate && m.Status != Models.MessageStatus.Processing)
        .ToList();

        foreach (var message in messagesForArchiving)
        {
            message.Status = Models.MessageStatus.Archived;
            await _messageRepository.UpdateAsync(message, cancellationToken).ConfigureAwait(false);
        }

        _logger.LogInformation("Archived {Count} messages older than {Days} days", messagesForArchiving.Count, daysOld);
    }

    /// <summary>
    /// Sends a poll to a chat using the bot's message sending pipeline.
    /// </summary>
    /// <param name="chatId">Target chat identifier</param>
    /// <param name="question">Poll question (1-256 characters)</param>
    /// <param name="options">List of answer options (2-10 options, each 1-100 characters)</param>
    /// <param name="allowsMultipleAnswers">Whether users can select multiple answers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created message entity if successful, null otherwise</returns>
    public async Task<Models.Message?> SendPollAsync(
        long chatId,
        string question,
        string[] options,
        bool allowsMultipleAnswers = false,
        CancellationToken cancellationToken = default)
    {
        // Validate inputs
        if (chatId <= 0)
            throw new ArgumentException("Chat ID must be positive", nameof(chatId));

        if (string.IsNullOrWhiteSpace(question) || question.Length > 256)
            throw new ArgumentException("Question must be 1-256 characters", nameof(question));

        if (options == null || options.Length < 2 || options.Length > 10)
            throw new ArgumentException("Must provide 2-10 options", nameof(options));

        if (options.Any(o => string.IsNullOrWhiteSpace(o) || o.Length > 100))
            throw new ArgumentException("Each option must be 1-100 characters", nameof(options));

        try
        {
            // Send poll using the bot's API client
            var messageId = await _telegramApiClient.SendPollAsync(
                chatId,
                question,
                options,
                allowsMultipleAnswers
            ).ConfigureAwait(false);

            if (messageId.HasValue)
            {
                // Create and store message record
                var message = new Models.Message
                {
                    ChatId = chatId,
                    Content = question,
                    Type = Models.MessageType.Poll,
                    Status = Models.MessageStatus.Processed,
                    Metadata = new Dictionary<string, object>
                    {
                        { "poll_type", "quiz" },
                        { "options", options },
                        { "allows_multiple_answers", allowsMultipleAnswers },
                        { "message_id", messageId.Value }
                    }
                };

                var created = await _messageRepository.CreateAsync(message, cancellationToken).ConfigureAwait(false);
                _logger.LogInformation("Poll sent to chat {ChatId}: {Question}", chatId, question);
                return created;
            }

            _logger.LogWarning("Failed to send poll to chat {ChatId}", chatId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending poll to chat {ChatId}", chatId);
            return null;
        }
    }

    /// <summary>
    /// Sends a media group (album) to a chat.
    /// </summary>
    /// <param name="chatId">Target chat identifier</param>
    /// <param name="items">List of media items (2-10 items)</param>
    /// <param name="caption">Optional caption for the media group</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of created message entities if successful, null otherwise</returns>
    public async Task<IList<Models.Message>?> SendMediaGroupAsync(
        long chatId,
        IList<Integration.MediaGroupItem> items,
        string? caption = null,
        CancellationToken cancellationToken = default)
    {
        // Validate inputs
        if (chatId <= 0)
            throw new ArgumentException("Chat ID must be positive", nameof(chatId));

        if (items == null || items.Count < 2 || items.Count > 10)
            throw new ArgumentException("Must provide 2-10 media items", nameof(items));

        if (items.Any(item => item == null || string.IsNullOrWhiteSpace(item.FileIdOrUrl)))
            throw new ArgumentException("Each item must have a valid FileIdOrUrl", nameof(items));

        try
        {
            // Send media group using the bot's API client
            var messageIds = await _telegramApiClient.SendMediaGroupAsync(chatId, items, cancellationToken).ConfigureAwait(false);

            if (messageIds != null && messageIds.Count > 0)
            {
                // Create and store message records for each media item
                var messages = new List<Models.Message>();

                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    var messageId = messageIds[i];

                    var message = new Models.Message
                    {
                        ChatId = chatId,
                        Content = caption ?? string.Empty,
                        Type = GetMessageType(item.Type),
                        Status = Models.MessageStatus.Processed,
                        Metadata = new Dictionary<string, object>
                        {
                            { "media_type", item.Type.ToString().ToLower() },
                            { "file_id_or_url", item.FileIdOrUrl },
                            { "message_id", messageId },
                            { "position", i }
                        }
                    };

                    // Store caption from parameter if provided, otherwise use item caption
                    if (!string.IsNullOrEmpty(caption))
                    {
                        message.Metadata["caption"] = caption;
                    }
                    else if (!string.IsNullOrEmpty(item.Caption))
                    {
                        message.Metadata["caption"] = item.Caption;
                    }

                    var created = await _messageRepository.CreateAsync(message, cancellationToken).ConfigureAwait(false);
                    messages.Add(created);
                }

                _logger.LogInformation("Media group sent to chat {ChatId} with {Count} items", chatId, items.Count);
                return messages;
            }

            _logger.LogWarning("Failed to send media group to chat {ChatId}", chatId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending media group to chat {ChatId}", chatId);
            return null;
        }
    }

    private Models.MessageType GetMessageType(Integration.MediaType mediaType)
    {
        return mediaType switch
        {
            Integration.MediaType.Photo => Models.MessageType.Photo,
            Integration.MediaType.Video => Models.MessageType.Video,
            Integration.MediaType.Audio => Models.MessageType.Audio,
            Integration.MediaType.Document => Models.MessageType.Document,
            _ => Models.MessageType.Photo
        };
    }
}