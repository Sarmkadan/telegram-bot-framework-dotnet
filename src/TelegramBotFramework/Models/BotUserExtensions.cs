using System;
using System.Collections.Generic;
using System.Globalization;
using TelegramBotFramework.Models;

namespace TelegramBotFramework.Models
{
    /// <summary>
    /// Extensions for <see cref="BotUser"/>.
    /// </summary>
    public static class BotUserExtensions
    {
        /// <summary>
        /// Gets a display name for the bot user, prioritizing <see cref="BotUser.FirstName"/> and <see cref="BotUser.LastName"/>.
        /// </summary>
        /// <param name="user">The bot user.</param>
        /// <returns>A display name for the bot user.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="user"/> is null.</exception>
        public static string GetDisplayName(this BotUser? user)
        {
            ArgumentNullException.ThrowIfNull(user);

            return string.IsNullOrEmpty(user.FirstName) ? 
                user.Username ?? string.Empty : 
                string.IsNullOrEmpty(user.LastName) ? 
                    user.FirstName : 
                    $"{user.FirstName} {user.LastName}";
        }

        /// <summary>
        /// Determines whether the bot user is active based on <see cref="BotUser.LastActivityAt"/>.
        /// </summary>
        /// <param name="user">The bot user.</param>
        /// <param name="inactiveThreshold">The threshold for considering a user inactive.</param>
        /// <returns>True if the bot user is active; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="user"/> is null.</exception>
        public static bool IsActive(this BotUser? user, TimeSpan inactiveThreshold)
        {
            ArgumentNullException.ThrowIfNull(user);

            return user.LastActivityAt.HasValue && 
                (DateTime.UtcNow - user.LastActivityAt.Value) < inactiveThreshold;
        }

        /// <summary>
        /// Gets a metadata value for the bot user.
        /// </summary>
        /// <param name="user">The bot user.</param>
        /// <param name="key">The metadata key.</param>
        /// <returns>The metadata value, or null if not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="user"/> is null or <paramref name="key"/> is null or empty.</exception>
        public static string? GetMetadataValue(this BotUser? user, string key)
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentException.ThrowIfNullOrEmpty(key);

            return user.Metadata?.TryGetValue(key, out string? value) == true ? value : null;
        }
    }
}
