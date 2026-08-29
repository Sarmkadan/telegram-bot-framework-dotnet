using System;

namespace TelegramBotFramework.Extensions;

/// <summary>
/// Extension methods for Telegram chat ID classification and formatting.
/// </summary>
public static class ChatIdExtensions
{
    /// <summary>
    /// Returns true if the chat ID represents a group (basic group, not supergroup or channel).
    /// </summary>
    /// <param name="chatId">The Telegram chat ID.</param>
    /// <returns>True if the chat is a basic group, false otherwise.</returns>
    public static bool IsGroup(this long chatId)
    {
        // Basic groups are negative and do not start with -100
        return chatId < 0 && !chatId.ToString().StartsWith("-100");
    }

    /// <summary>
    /// Returns true if the chat ID represents a channel or supergroup.
    /// </summary>
    /// <param name="chatId">The Telegram chat ID.</param>
    /// <returns>True if the chat is a channel or supergroup, false otherwise.</returns>
    public static bool IsChannel(this long chatId)
    {
        // Channels and supergroups are negative and start with -100
        return chatId < 0 && chatId.ToString().StartsWith("-100");
    }

    /// <summary>
    /// Returns true if the chat ID represents a private chat.
    /// </summary>
    /// <param name="chatId">The Telegram chat ID.</param>
    /// <returns>True if the chat is a private chat, false otherwise.</returns>
    public static bool IsPrivate(this long chatId)
    {
        // Private chats have positive IDs
        return chatId > 0;
    }

    /// <summary>
    /// Converts the chat ID to its string representation as used in Telegram.
    /// </summary>
    /// <param name="chatId">The Telegram chat ID.</param>
    /// <returns>The string representation of the chat ID.</returns>
    public static string ToTelegramString(this long chatId)
    {
        return chatId.ToString();
    }
}