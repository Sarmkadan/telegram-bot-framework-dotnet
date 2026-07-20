#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;

namespace TelegramBotFramework.Services;

/// <summary>
/// Service for scheduling messages to be sent at specific times.
/// </summary>
public interface IScheduledMessageService : IDisposable
{
    /// <summary>
    /// Schedules a message to be sent at a specific time.
    /// </summary>
    /// <param name="chatId">The chat identifier where the message should be sent</param>
    /// <param name="text">The message text to send</param>
    /// <param name="sendAt">The date and time when the message should be sent</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A unique identifier for the scheduled message</returns>
    Task<string> ScheduleMessageAsync(long chatId, string text, DateTimeOffset sendAt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Schedules a message to be sent after a delay.
    /// </summary>
    /// <param name="chatId">The chat identifier where the message should be sent</param>
    /// <param name="text">The message text to send</param>
    /// <param name="delay">The delay before sending the message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A unique identifier for the scheduled message</returns>
    Task<string> ScheduleMessageAsync(long chatId, string text, TimeSpan delay, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a scheduled message by its ID.
    /// </summary>
    /// <param name="messageId">The scheduled message identifier</param>
    /// <returns>True if the message was found and cancelled, false otherwise</returns>
    bool CancelScheduledMessage(string messageId);

    /// <summary>
    /// Gets all scheduled messages.
    /// </summary>
    /// <returns>Collection of scheduled messages</returns>
    IEnumerable<ScheduledMessage> GetAllScheduledMessages();

    /// <summary>
    /// Gets a scheduled message by its ID.
    /// </summary>
    /// <param name="messageId">The scheduled message identifier</param>
    /// <returns>The scheduled message if found, null otherwise</returns>
    ScheduledMessage? GetScheduledMessage(string messageId);

    /// <summary>
    /// Gets all scheduled messages for a specific chat.
    /// </summary>
    /// <param name="chatId">The chat identifier</param>
    /// <returns>Collection of scheduled messages for the chat</returns>
    IEnumerable<ScheduledMessage> GetScheduledMessagesForChat(long chatId);
}

/// <summary>
/// Represents a scheduled message.
/// </summary>
public sealed class ScheduledMessage
{
    public string Id { get; set; } = string.Empty;
    public long ChatId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTimeOffset ScheduledTime { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsCancelled { get; set; }
    public bool IsSent { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTimeOffset? NextAttemptTime { get; set; }
    public int AttemptCount { get; set; }
}