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
        /// <returns>The result of the send; its value is true when the message was sent</returns>
        /// <exception cref="ArgumentNullException"><paramref name="client"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="chatId"/> is invalid or <paramref name="text"/> is null or whitespace or <paramref name="buttonLabels"/> is null</exception>
        public static async Task<ApiResult<bool>> SendMessageWithButtonsAsync(
            this ITelegramApiClient client,
            long chatId,
            string text,
            string[][] buttonLabels)
        {
            ArgumentNullException.ThrowIfNull(client);

            if (!ValidationUtility.IsValidTelegramChatId(chatId))
            {
                throw new ArgumentException(TelegramApiClientExtensionsConstants.InvalidChatIdError, nameof(chatId));
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
        /// <returns>The result of the edit; its value is true when the message was edited</returns>
        /// <exception cref="ArgumentNullException"><paramref name="client"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="chatId"/> is invalid, <paramref name="messageId"/> is not positive, or <paramref name="newText"/> is null or whitespace</exception>
        public static async Task<ApiResult<bool>> EditMessageTextAsync(
            this ITelegramApiClient client,
            long chatId,
            int messageId,
            string newText)
        {
            ArgumentNullException.ThrowIfNull(client);

            if (!ValidationUtility.IsValidTelegramChatId(chatId))
            {
                throw new ArgumentException(TelegramApiClientExtensionsConstants.InvalidChatIdError, nameof(chatId));
            }

            if (messageId < TelegramApiClientExtensionsConstants.MinimumMessageId)
            {
                throw new ArgumentException(TelegramApiClientExtensionsConstants.MessageIdMustBePositiveError, nameof(messageId));
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
        /// <returns>The result of the answer; its value is true when Telegram accepts it</returns>
        /// <exception cref="ArgumentNullException"><paramref name="client"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="callbackQueryId"/> is null or whitespace</exception>
        public static async Task<ApiResult<bool>> AnswerCallbackQueryWithTextAsync(
            this ITelegramApiClient client,
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
        /// <returns>The result carrying the raw bot information JSON on success; otherwise a failed result</returns>
        /// <exception cref="ArgumentNullException"><paramref name="client"/> is null</exception>
        public static async Task<ApiResult<string>> GetBotInformationAsync(this ITelegramApiClient client)
        {
            ArgumentNullException.ThrowIfNull(client);

            return await client.GetMeAsync().ConfigureAwait(false);
        }
    }
}
