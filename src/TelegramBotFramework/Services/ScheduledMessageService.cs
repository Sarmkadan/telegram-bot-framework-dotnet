#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using Microsoft.Extensions.Logging;
using TelegramBotFramework.Integration;
using Microsoft.Extensions.Logging.Console;

namespace TelegramBotFramework.Services;

/// <summary>
/// Service for scheduling messages to be sent at specific times using background timers.
/// </summary>
public sealed class ScheduledMessageService : IScheduledMessageService
{
    private readonly ITelegramApiClient _telegramApiClient;
    private readonly ILogger<ScheduledMessageService> _logger;
    private readonly Dictionary<string, ScheduledMessage> _scheduledMessages = new();
    private readonly Dictionary<string, System.Threading.Timer> _timers = new();
    private readonly object _lockObj = new();
    private readonly TimeSpan _defaultRetryDelay = TimeSpan.FromSeconds(30);
    private readonly int _maxRetryAttempts = 3;

    public ScheduledMessageService(
        ITelegramApiClient telegramApiClient,
        ILogger<ScheduledMessageService>? logger = null)
    {
        _telegramApiClient = telegramApiClient ?? throw new ArgumentNullException(nameof(telegramApiClient));
        _logger = logger ?? new ConsoleLogger<ScheduledMessageService>();
    }

    public async Task<string> ScheduleMessageAsync(long chatId, string text, DateTimeOffset sendAt, CancellationToken cancellationToken = default)
    {
        if (chatId <= 0)
            throw new ArgumentException("Chat ID must be positive", nameof(chatId));

        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Message text cannot be empty", nameof(text));

        if (sendAt <= DateTimeOffset.UtcNow)
            throw new ArgumentException("Send time must be in the future", nameof(sendAt));

        var messageId = Guid.NewGuid().ToString();
        var delay = sendAt - DateTimeOffset.UtcNow;

        var scheduledMessage = new ScheduledMessage
        {
            Id = messageId,
            ChatId = chatId,
            Text = text,
            ScheduledTime = sendAt,
            CreatedAt = DateTimeOffset.UtcNow,
            NextAttemptTime = sendAt,
            AttemptCount = 0
        };

        lock (_lockObj)
        {
            _scheduledMessages[messageId] = scheduledMessage;
        }

        _logger.LogInformation("Scheduled message created: {MessageId} for chat {ChatId} at {ScheduledTime}",
            messageId, chatId, sendAt);

        // Schedule the timer
        var timer = new System.Threading.Timer(async _ => await SendScheduledMessageAsync(messageId, cancellationToken).ConfigureAwait(false),
            null, delay, Timeout.InfiniteTimeSpan);

        lock (_lockObj)
        {
            _timers[messageId] = timer;
        }

        return messageId;
    }

    public async Task<string> ScheduleMessageAsync(long chatId, string text, TimeSpan delay, CancellationToken cancellationToken = default)
    {
        if (chatId <= 0)
            throw new ArgumentException("Chat ID must be positive", nameof(chatId));

        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Message text cannot be empty", nameof(text));

        if (delay <= TimeSpan.Zero)
            throw new ArgumentException("Delay must be positive", nameof(delay));

        var messageId = Guid.NewGuid().ToString();
        var sendAt = DateTimeOffset.UtcNow.Add(delay);

        var scheduledMessage = new ScheduledMessage
        {
            Id = messageId,
            ChatId = chatId,
            Text = text,
            ScheduledTime = sendAt,
            CreatedAt = DateTimeOffset.UtcNow,
            NextAttemptTime = sendAt,
            AttemptCount = 0
        };

        lock (_lockObj)
        {
            _scheduledMessages[messageId] = scheduledMessage;
        }

        _logger.LogInformation("Scheduled message created: {MessageId} for chat {ChatId} in {Delay}ms",
            messageId, chatId, delay.TotalMilliseconds);

        // Schedule the timer
        var timer = new System.Threading.Timer(async _ => await SendScheduledMessageAsync(messageId, cancellationToken).ConfigureAwait(false),
            null, delay, Timeout.InfiniteTimeSpan);

        lock (_lockObj)
        {
            _timers[messageId] = timer;
        }

        return messageId;
    }

    public bool CancelScheduledMessage(string messageId)
    {
        if (string.IsNullOrWhiteSpace(messageId))
            return false;

        lock (_lockObj)
        {
            if (_scheduledMessages.TryGetValue(messageId, out var message))
            {
                if (_timers.TryGetValue(messageId, out var timer))
                {
                    timer.Dispose();
                    _timers.Remove(messageId);
                }

                message.IsCancelled = true;
                _scheduledMessages[messageId] = message;

                _logger.LogInformation("Cancelled scheduled message: {MessageId}", messageId);
                return true;
            }
        }

        return false;
    }

    public IEnumerable<ScheduledMessage> GetAllScheduledMessages()
    {
        lock (_lockObj)
        {
            return _scheduledMessages.Values.ToList();
        }
    }

    public ScheduledMessage? GetScheduledMessage(string messageId)
    {
        if (string.IsNullOrWhiteSpace(messageId))
            return null;

        lock (_lockObj)
        {
            _scheduledMessages.TryGetValue(messageId, out var message);
            return message;
        }
    }

    public IEnumerable<ScheduledMessage> GetScheduledMessagesForChat(long chatId)
    {
        if (chatId <= 0)
            return Enumerable.Empty<ScheduledMessage>();

        lock (_lockObj)
        {
            return _scheduledMessages.Values
                .Where(m => m.ChatId == chatId && !m.IsCancelled && !m.IsSent)
                .ToList();
        }
    }

    private async Task SendScheduledMessageAsync(string messageId, CancellationToken cancellationToken)
    {
        ScheduledMessage? message = null;
        System.Threading.Timer? timer = null;

        try
        {
            lock (_lockObj)
            {
                if (!_scheduledMessages.TryGetValue(messageId, out message) || message.IsCancelled)
                {
                    _logger.LogDebug("Scheduled message {MessageId} was cancelled or not found", messageId);
                    return;
                }

                if (message.IsSent)
                {
                    _logger.LogDebug("Scheduled message {MessageId} was already sent", messageId);
                    return;
                }

                // Get the timer for this message
                if (_timers.TryGetValue(messageId, out timer))
                {
                    _timers.Remove(messageId);
                }
            }

            message.AttemptCount++;
            message.NextAttemptTime = DateTimeOffset.UtcNow.Add(_defaultRetryDelay);

            _logger.LogInformation("Attempting to send scheduled message {MessageId} (attempt {AttemptCount}/{MaxAttempts})",
                messageId, message.AttemptCount, _maxRetryAttempts);

            // Send the message via Telegram API
            var success = await _telegramApiClient.SendMessageAsync(message.ChatId, message.Text).ConfigureAwait(false);

            if (success)
            {
                lock (_lockObj)
                {
                    if (_scheduledMessages.TryGetValue(messageId, out var updatedMessage))
                    {
                        updatedMessage.IsSent = true;
                        updatedMessage.SentAt = DateTimeOffset.UtcNow;
                        _scheduledMessages[messageId] = updatedMessage;
                    }
                }

                _logger.LogInformation("Successfully sent scheduled message {MessageId} to chat {ChatId}",
                    messageId, message.ChatId);
            }
            else
            {
                HandleSendFailure(message, messageId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending scheduled message {MessageId}", messageId);

            if (message != null)
            {
                lock (_lockObj)
                {
                    if (_scheduledMessages.TryGetValue(messageId, out var updatedMessage))
                    {
                        updatedMessage.ErrorMessage = ex.Message;
                        _scheduledMessages[messageId] = updatedMessage;
                    }
                }

                // Schedule retry if not exceeded max attempts
                if (message.AttemptCount < _maxRetryAttempts)
                {
                    ScheduleRetry(messageId, message);
                }
                else
                {
                    _logger.LogError("Max retry attempts ({MaxAttempts}) exceeded for message {MessageId}",
                        _maxRetryAttempts, messageId);
                }
            }
        }
        finally
        {
            timer?.Dispose();
        }
    }

    private void HandleSendFailure(ScheduledMessage message, string messageId)
    {
        _logger.LogWarning("Failed to send scheduled message {MessageId} on attempt {AttemptCount}",
            messageId, message.AttemptCount);

        if (message.AttemptCount < _maxRetryAttempts)
        {
            ScheduleRetry(messageId, message);
        }
        else
        {
            lock (_lockObj)
            {
                if (_scheduledMessages.TryGetValue(messageId, out var updatedMessage))
                {
                    updatedMessage.ErrorMessage = "Max retry attempts exceeded";
                    _scheduledMessages[messageId] = updatedMessage;
                }
            }
            _logger.LogError("Max retry attempts ({MaxAttempts}) exceeded for message {MessageId}",
                _maxRetryAttempts, messageId);
        }
    }

    private void ScheduleRetry(string messageId, ScheduledMessage message)
    {
        var retryDelay = _defaultRetryDelay;
        var retryTimer = new System.Threading.Timer(async _ => await SendScheduledMessageAsync(messageId, CancellationToken.None).ConfigureAwait(false),
            null, retryDelay, Timeout.InfiniteTimeSpan);

        lock (_lockObj)
        {
            if (_timers.ContainsKey(messageId))
            {
                _timers[messageId].Dispose();
            }
            _timers[messageId] = retryTimer;
        }

        _logger.LogInformation("Scheduled retry for message {MessageId} in {RetryDelay}ms (attempt {AttemptCount}/{MaxAttempts})",
            messageId, retryDelay.TotalMilliseconds, message.AttemptCount + 1, _maxRetryAttempts);
    }

    public void Dispose()
    {
        lock (_lockObj)
        {
            foreach (var timer in _timers.Values)
            {
                timer.Dispose();
            }
            _timers.Clear();
            _scheduledMessages.Clear();
        }
    }
}