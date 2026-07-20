#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;

namespace TelegramBotFramework.Services;

/// <summary>
/// Extension methods for <see cref="ScheduledMessageService"/> providing additional scheduling utilities.
/// </summary>
public static class ScheduledMessageServiceExtensions
{
    /// <summary>
    /// Schedules a message to be sent at a specific time.
    /// </summary>
    /// <param name="service">The scheduled message service instance</param>
    /// <param name="chatId">The chat identifier where the message should be sent</param>
    /// <param name="text">The message text to send</param>
    /// <param name="sendAt">The date and time when the message should be sent</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A unique identifier for the scheduled message</returns>
    public static Task<string> ScheduleMessageAsync(
        this IScheduledMessageService service,
        long chatId,
        string text,
        DateTime sendAt,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(text);

        return service.ScheduleMessageAsync(chatId, text, new DateTimeOffset(sendAt), cancellationToken);
    }

    /// <summary>
    /// Schedules a message to be sent after a delay.
    /// </summary>
    /// <param name="service">The scheduled message service instance</param>
    /// <param name="chatId">The chat identifier where the message should be sent</param>
    /// <param name="text">The message text to send</param>
    /// <param name="delay">The delay before sending the message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A unique identifier for the scheduled message</returns>
    public static Task<string> ScheduleMessageAsync(
        this IScheduledMessageService service,
        long chatId,
        string text,
        TimeSpan delay,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(text);

        return service.ScheduleMessageAsync(chatId, text, delay, cancellationToken);
    }

    /// <summary>
    /// Gets all scheduled messages for a specific chat.
    /// </summary>
    /// <param name="service">The scheduled message service instance</param>
    /// <param name="chatId">The chat identifier</param>
    /// <returns>Collection of scheduled messages for the chat</returns>
    public static IEnumerable<ScheduledMessage> GetScheduledMessagesForChat(
        this IScheduledMessageService service,
        long chatId)
    {
        ArgumentNullException.ThrowIfNull(service);

        return service.GetScheduledMessagesForChat(chatId);
    }

    /// <summary>
    /// Gets a scheduled message by its ID.
    /// </summary>
    /// <param name="service">The scheduled message service instance</param>
    /// <param name="messageId">The scheduled message identifier</param>
    /// <returns>The scheduled message if found, null otherwise</returns>
    public static ScheduledMessage? GetScheduledMessage(
        this IScheduledMessageService service,
        string messageId)
    {
        ArgumentNullException.ThrowIfNull(service);

        return service.GetScheduledMessage(messageId);
    }
}