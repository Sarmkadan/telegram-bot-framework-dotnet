using System;

namespace TelegramBotFramework.Models
{
    /// <summary>
    /// Provides extension methods for <see cref="TelegramBotFrameworkDotnetOptions"/>.
    /// </summary>
    public static class TelegramBotFrameworkDotnetOptionsExtensions
    {
        /// <summary>
        /// Validates the essential configuration properties to ensure the bot can function correctly.
        /// </summary>
        /// <param name="options">The options instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if BotToken or BotUsername is null or empty.</exception>
        public static void Validate(this TelegramBotFrameworkDotnetOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            if (string.IsNullOrWhiteSpace(options.BotToken))
            {
                throw new InvalidOperationException(TelegramBotFrameworkDotnetOptionsExtensionsConstants.BotTokenNullOrEmptyExceptionMessage);
            }

            if (string.IsNullOrWhiteSpace(options.BotUsername))
            {
                throw new InvalidOperationException(TelegramBotFrameworkDotnetOptionsExtensionsConstants.BotUsernameNullOrEmptyExceptionMessage);
            }
        }

        /// <summary>
        /// Converts the session timeout minutes into a <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="options">The options instance.</param>
        /// <returns>A TimeSpan representing the session timeout duration.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
        public static TimeSpan GetSessionTimeout(this TelegramBotFrameworkDotnetOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            return TimeSpan.FromMinutes(options.SessionTimeoutMinutes);
        }

        /// <summary>
        /// Converts the message processing timeout seconds into a <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="options">The options instance.</param>
        /// <returns>A TimeSpan representing the message processing timeout duration.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
        public static TimeSpan GetMessageProcessingTimeout(this TelegramBotFrameworkDotnetOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            return TimeSpan.FromSeconds(options.MessageProcessingTimeoutSeconds);
        }

        /// <summary>
        /// Determines whether a database connection string has been configured.
        /// </summary>
        /// <param name="options">The options instance.</param>
        /// <returns>True if a database connection string is present; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
        public static bool HasDatabaseConfigured(this TelegramBotFrameworkDotnetOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            return !string.IsNullOrWhiteSpace(options.DatabaseConnectionString);
        }
    }
}