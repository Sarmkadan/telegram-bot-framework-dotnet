#nullable enable

namespace TelegramBotFramework.Models;

/// <summary>
/// Provides extension methods for the <see cref="Message"/> class.
/// </summary>
public static class MessageExtensions
{
    /// <summary>
    /// Determines whether the message is a command.
    /// </summary>
    /// <param name="message">The message to check.</param>
    /// <returns>True if the message represents a command; otherwise, false.</returns>
    public static bool IsCommand(this Message message)
    {
        return message.CommandName != null && message.CommandName.StartsWith('/');
    }

    /// <summary>
    /// Determines whether the message has attachments.
    /// </summary>
    /// <param name="message">The message to check.</param>
    /// <returns>True if the message has attachments; otherwise, false.</returns>
    public static bool HasAttachments(this Message message)
    {
        return message.AttachmentUrls != null && message.AttachmentUrls.Count > 0;
    }

    /// <summary>
    /// Gets the message type as a lowercase string representation.
    /// </summary>
    /// <param name="message">The message to get type for.</param>
    /// <returns>Lowercase string representation of the message type.</returns>
    public static string GetTypeString(this Message message)
    {
        return message.Type.ToString().ToLowerInvariant();
    }

    /// <summary>
    /// Determines whether the message is a reply to another message.
    /// </summary>
    /// <param name="message">The message to check.</param>
    /// <returns>True if the message is a reply; otherwise, false.</returns>
    public static bool IsReply(this Message message)
    {
        return message.ReplyToMessageId.HasValue && message.ReplyToMessageId > 0;
    }
}