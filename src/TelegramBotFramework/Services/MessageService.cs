#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Services;

/// <summary>
/// Implementation of message processing service.
/// </summary>
public sealed class MessageService : IMessageService
{
    private readonly Repositories.IMessageRepository _messageRepository;
    private readonly Microsoft.Extensions.Logging.ILogger<MessageService> _logger;

    public MessageService(
        Repositories.IMessageRepository messageRepository,
        Microsoft.Extensions.Logging.ILogger<MessageService> logger)
    {
        _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
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
        if (message  is null)
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
        if (message  is null)
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
}