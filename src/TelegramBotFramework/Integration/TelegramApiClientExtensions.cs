using System;
using System.Threading.Tasks;
using TelegramBotFramework.Utilities;

namespace TelegramBotFramework.Integration
{
    /// <summary>
    /// Provides extension methods for <see cref="TelegramApiClient"/> to simplify common Telegram Bot API operations.
    /// </summary>
    public static class TelegramApiClientExtensions
    {
        /// <summary>
        /// Sends a message with inline keyboard buttons
        /// </summary>
        /// <param name="client">The Telegram API client</param>
        /// <param name="chatId">Target chat identifier</param>
        /// <param name="text">Message text</param>
        /// <param name="buttonLabels">2D array of button labels (rows x columns)</param>
        /// <returns>True if successful, false otherwise</returns>
        /// <exception cref="ArgumentNullException"><paramref name="client"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="chatId"/> is invalid or <paramref name="text"/> is null or whitespace or <paramref name="buttonLabels"/> is null</exception>
        public static async Task<bool> SendMessageWithButtonsAsync(
            this TelegramApiClient client,
            long chatId,
            string text,
            string[][] buttonLabels)
        {
            ArgumentNullException.ThrowIfNull(client);

            if (!ValidationUtility.IsValidTelegramChatId(chatId))
            {
                throw new ArgumentException("Invalid chat ID", nameof(chatId));
            }

            ArgumentException.ThrowIfNullOrWhiteSpace(text, nameof(text));

            ArgumentNullException.ThrowIfNull(buttonLabels);

            return await client.SendMessageWithButtonsAsync(
                chatId,
                text,
                buttonLabels
            ).ConfigureAwait(false);
        }

        /// <summary>
        /// Edits an existing message with new content
        /// </summary>
        /// <param name="client">The Telegram API client</param>
        /// <param name="chatId">Target chat identifier</param>
        /// <param name="messageId">Message identifier to edit</param>
        /// <param name="newText">New message text</param>
        /// <returns>True if successful, false otherwise</returns>
        /// <exception cref="ArgumentNullException"><paramref name="client"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="chatId"/> is invalid, <paramref name="messageId"/> is not positive, or <paramref name="newText"/> is null or whitespace</exception>
        public static async Task<bool> EditMessageTextAsync(
            this TelegramApiClient client,
            long chatId,
            int messageId,
            string newText)
        {
            ArgumentNullException.ThrowIfNull(client);

            if (!ValidationUtility.IsValidTelegramChatId(chatId))
            {
                throw new ArgumentException("Invalid chat ID", nameof(chatId));
            }

            if (messageId <= 0)
            {
                throw new ArgumentException("Message ID must be positive", nameof(messageId));
            }

            ArgumentException.ThrowIfNullOrWhiteSpace(newText, nameof(newText));

            return await client.EditMessageAsync(
                chatId,
                messageId,
                newText
            ).ConfigureAwait(false);
        }

        /// <summary>
        /// Answers a callback query with optional notification text
        /// </summary>
        /// <param name="client">The Telegram API client</param>
        /// <param name="callbackQueryId">Callback query identifier</param>
        /// <param name="text">Optional text to show to user</param>
        /// <returns>True if successful, false otherwise</returns>
        /// <exception cref="ArgumentNullException"><paramref name="client"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="callbackQueryId"/> is null or whitespace</exception>
        public static async Task<bool> AnswerCallbackQueryWithTextAsync(
            this TelegramApiClient client,
            string callbackQueryId,
            string? text = null)
        {
            ArgumentNullException.ThrowIfNull(client);

            ArgumentException.ThrowIfNullOrWhiteSpace(callbackQueryId, nameof(callbackQueryId));

            return await client.AnswerCallbackQueryAsync(
                callbackQueryId,
                text
            ).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets information about the bot
        /// </summary>
        /// <param name="client">The Telegram API client</param>
        /// <returns>Bot information if successful, null otherwise</returns>
        /// <exception cref="ArgumentNullException"><paramref name="client"/> is null</exception>
        public static async Task<string?> GetBotInformationAsync(this TelegramApiClient client)
        {
            ArgumentNullException.ThrowIfNull(client);

            return await client.GetMeAsync().ConfigureAwait(false);
        }
    }
}