using System;
using System.Threading.Tasks;
using TelegramBotFramework.Utilities;

namespace TelegramBotFramework.Integration
{
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
        public static async Task<bool> SendMessageWithButtonsAsync(
            this TelegramApiClient client,
            long chatId,
            string text,
            string[][] buttonLabels)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (!ValidationUtility.IsValidTelegramChatId(chatId))
            {
                throw new ArgumentException("Invalid chat ID", nameof(chatId));
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Message text cannot be null or whitespace", nameof(text));
            }

            if (buttonLabels is null)
            {
                throw new ArgumentNullException(nameof(buttonLabels));
            }

            try
            {
                var result = await client.SendMessageWithButtonsAsync(
                    chatId,
                    text,
                    buttonLabels
                ).ConfigureAwait(false);

                return result;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Edits an existing message with new content
        /// </summary>
        /// <param name="client">The Telegram API client</param>
        /// <param name="chatId">Target chat identifier</param>
        /// <param name="messageId">Message identifier to edit</param>
        /// <param name="newText">New message text</param>
        /// <returns>True if successful, false otherwise</returns>
        public static async Task<bool> EditMessageTextAsync(
            this TelegramApiClient client,
            long chatId,
            int messageId,
            string newText)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (!ValidationUtility.IsValidTelegramChatId(chatId))
            {
                throw new ArgumentException("Invalid chat ID", nameof(chatId));
            }

            if (messageId <= 0)
            {
                throw new ArgumentException("Message ID must be positive", nameof(messageId));
            }

            if (string.IsNullOrWhiteSpace(newText))
            {
                throw new ArgumentException("Message text cannot be null or whitespace", nameof(newText));
            }

            try
            {
                var result = await client.EditMessageAsync(
                    chatId,
                    messageId,
                    newText
                ).ConfigureAwait(false);

                return result;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Answers a callback query with optional notification text
        /// </summary>
        /// <param name="client">The Telegram API client</param>
        /// <param name="callbackQueryId">Callback query identifier</param>
        /// <param name="text">Optional text to show to user</param>
        /// <returns>True if successful, false otherwise</returns>
        public static async Task<bool> AnswerCallbackQueryWithTextAsync(
            this TelegramApiClient client,
            string callbackQueryId,
            string? text = null)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (string.IsNullOrWhiteSpace(callbackQueryId))
            {
                throw new ArgumentException("Callback query ID cannot be null or whitespace", nameof(callbackQueryId));
            }

            try
            {
                var result = await client.AnswerCallbackQueryAsync(
                    callbackQueryId,
                    text
                ).ConfigureAwait(false);

                return result;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets information about the bot
        /// </summary>
        /// <param name="client">The Telegram API client</param>
        /// <returns>Bot information if successful, null otherwise</returns>
        public static async Task<string?> GetBotInformationAsync(this TelegramApiClient client)
        {
            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            try
            {
                var botInfo = await client.GetMeAsync().ConfigureAwait(false);
                return botInfo;
            }
            catch
            {
                return null;
            }
        }
    }
}