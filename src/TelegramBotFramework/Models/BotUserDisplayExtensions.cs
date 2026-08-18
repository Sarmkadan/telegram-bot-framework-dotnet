#nullable enable
using System;

namespace TelegramBotFramework.Models;

/// <summary>
/// Extension methods for <see cref="BotUser"/> related to display names and activity status.
/// </summary>
public static class BotUserDisplayExtensions
{
    /// <summary>
    /// Gets the user's display name, prioritizing first and last name, and falling back to username.
    /// </summary>
    /// <param name="user">The bot user.</param>
    /// <returns>The formatted display name.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="user"/> is null.</exception>
    public static string GetDisplayName(this BotUser? user)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (!string.IsNullOrWhiteSpace(user.FirstName) && !string.IsNullOrWhiteSpace(user.LastName))
        {
            return $"{user.FirstName} {user.LastName}";
        }

        if (!string.IsNullOrWhiteSpace(user.FirstName))
        {
            return user.FirstName;
        }

        return user.Username ?? "Unknown";
    }

    /// <summary>
    /// Generates a Telegram Markdown mention for the user.
    /// </summary>
    /// <param name="user">The bot user.</param>
    /// <returns>A Markdown formatted mention string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="user"/> is null.</exception>
    public static string GetMentionMarkdown(this BotUser? user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var displayName = user.GetDisplayName();
        return $"[{displayName}](tg://user?id={user.TelegramId})";
    }

    /// <summary>
    /// Determines whether the user has been active within the specified time threshold.
    /// </summary>
    /// <param name="user">The bot user.</param>
    /// <param name="threshold">The maximum allowed time since the last activity.</param>
    /// <returns>True if the user was active within the threshold; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="user"/> is null.</exception>
    public static bool IsRecentlyActive(this BotUser? user, TimeSpan threshold)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (!user.LastActivityAt.HasValue)
        {
            return false;
        }

        return (DateTime.UtcNow - user.LastActivityAt.Value) <= threshold;
    }
}
