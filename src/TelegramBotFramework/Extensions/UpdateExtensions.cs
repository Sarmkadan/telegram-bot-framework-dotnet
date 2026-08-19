using System;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBotFramework.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="Telegram.Bot.Types.Update"/> to simplify
    /// extraction of common data such as chat identifiers, user identifiers, text payloads,
    /// command parsing and update‑type classification.
    /// </summary>
    public static class UpdateExtensions
    {
        #region Chat / User / Text extraction

        /// <summary>
        /// Retrieves the chat identifier associated with the update, if any.
        /// </summary>
        /// <param name="update">The Telegram update.</param>
        /// <returns>The chat identifier, or <c>null</c> when the update does not contain a chat.</returns>
        public static long? GetChatId(this Update update)
        {
            if (update == null) throw new ArgumentNullException(nameof(update));

            // Message based updates
            var chatId = update.Message?.Chat?.Id
                ?? update.EditedMessage?.Chat?.Id
                ?? update.ChannelPost?.Chat?.Id
                ?? update.EditedChannelPost?.Chat?.Id;

            // Callback query updates contain a message
            if (chatId == null && update.CallbackQuery?.Message?.Chat?.Id is long cbChatId)
                chatId = cbChatId;

            // Inline query updates do not have a chat
            return chatId;
        }

        /// <summary>
        /// Retrieves the user identifier associated with the update, if any.
        /// </summary>
        /// <param name="update">The Telegram update.</param>
        /// <returns>The user identifier, or <c>null</c> when the update does not contain a user.</returns>
        public static long? GetUserId(this Update update)
        {
            if (update == null) throw new ArgumentNullException(nameof(update));

            var userId = update.Message?.From?.Id
                ?? update.EditedMessage?.From?.Id
                ?? update.ChannelPost?.From?.Id
                ?? update.EditedChannelPost?.From?.Id
                ?? update.CallbackQuery?.From?.Id
                ?? update.InlineQuery?.From?.Id
                ?? update.ChosenInlineResult?.From?.Id
                ?? update.PollAnswer?.User?.Id;

            return userId;
        }

        /// <summary>
        /// Retrieves the textual payload of the update, if any.
        /// </summary>
        /// <param name="update">The Telegram update.</param>
        /// <returns>The text, or <c>null</c> when the update does not contain textual data.</returns>
        public static string? GetText(this Update update)
        {
            if (update == null) throw new ArgumentNullException(nameof(update));

            // Direct message text
            var text = update.Message?.Text
                ?? update.EditedMessage?.Text
                ?? update.ChannelPost?.Text
                ?? update.EditedChannelPost?.Text;

            // Callback query data
            if (string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(update.CallbackQuery?.Data))
                text = update.CallbackQuery.Data;

            // Inline query query text
            if (string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(update.InlineQuery?.Query))
                text = update.InlineQuery.Query;

            // Chosen inline result query text
            if (string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(update.ChosenInlineResult?.Query))
                text = update.ChosenInlineResult.Query;

            return text;
        }

        /// <summary>
        /// Determines whether the textual payload of the update represents a bot command.
        /// </summary>
        /// <param name="update">The Telegram update.</param>
        /// <param name="command">
        /// When the method returns <c>true</c>, receives the command name without the leading '/'.
        /// </param>
        /// <param name="args">
        /// When the method returns <c>true</c>, receives the arguments part of the command (trimmed),
        /// or an empty string if no arguments are present.
        /// </param>
        /// <returns><c>true</c> if the text starts with a '/' and therefore is a command; otherwise <c>false</c>.</returns>
        public static bool IsCommand(this Update update, out string command, out string args)
        {
            command = string.Empty;
            args = string.Empty;

            var text = update.GetText();
            if (string.IsNullOrWhiteSpace(text) || !text.StartsWith("/", StringComparison.Ordinal))
                return false;

            // Remove the leading '/' and split on whitespace
            var trimmed = text[1..].Trim();
            var parts = trimmed.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);

            command = parts[0];
            args = parts.Length > 1 ? parts[1].Trim() : string.Empty;

            return true;
        }

        #endregion

        #region Update‑type classification helpers

        /// <summary>
        /// Returns <c>true</c> if the update is a standard message.
        /// </summary>
        public static bool IsMessage(this Update update) =>
            update?.Type == UpdateType.Message;

        /// <summary>
        /// Returns <c>true</c> if the update is an edited message.
        /// </summary>
        public static bool IsEditedMessage(this Update update) =>
            update?.Type == UpdateType.EditedMessage;

        /// <summary>
        /// Returns <c>true</c> if the update is a channel post.
        /// </summary>
        public static bool IsChannelPost(this Update update) =>
            update?.Type == UpdateType.ChannelPost;

        /// <summary>
        /// Returns <c>true</c> if the update is an edited channel post.
        /// </summary>
        public static bool IsEditedChannelPost(this Update update) =>
            update?.Type == UpdateType.EditedChannelPost;

        /// <summary>
        /// Returns <c>true</c> if the update contains an inline query.
        /// </summary>
        public static bool IsInlineQuery(this Update update) =>
            update?.Type == UpdateType.InlineQuery;

        /// <summary>
        /// Returns <c>true</c> if the update contains a chosen inline result.
        /// </summary>
        public static bool IsChosenInlineResult(this Update update) =>
            update?.Type == UpdateType.ChosenInlineResult;

        /// <summary>
        /// Returns <c>true</c> if the update contains a callback query.
        /// </summary>
        public static bool IsCallbackQuery(this Update update) =>
            update?.Type == UpdateType.CallbackQuery;

        /// <summary>
        /// Returns <c>true</c> if the update contains a poll answer.
        /// </summary>
        public static bool IsPollAnswer(this Update update) =>
            update?.Type == UpdateType.PollAnswer;

        /// <summary>
        /// Returns <c>true</c> if the update contains a pre‑checkout query.
        /// </summary>
        public static bool IsPreCheckoutQuery(this Update update) =>
            update?.Type == UpdateType.PreCheckoutQuery;

        /// <summary>
        /// Returns <c>true</c> if the update contains a shipping query.
        /// </summary>
        public static bool IsShippingQuery(this Update update) =>
            update?.Type == UpdateType.ShippingQuery;

        #endregion
    }
}
