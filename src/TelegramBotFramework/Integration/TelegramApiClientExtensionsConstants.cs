namespace TelegramBotFramework.Integration
{
    /// <summary>
    /// Constants used by <see cref="TelegramApiClientExtensions"/>.
    /// </summary>
    internal static class TelegramApiClientExtensionsConstants
    {
        /// <summary>
        /// Error message used when a chat identifier is invalid.
        /// </summary>
        public const string InvalidChatIdError = "Invalid chat ID";

        /// <summary>
        /// Error message used when a message identifier is not positive.
        /// </summary>
        public const string MessageIdMustBePositiveError = "Message ID must be positive";

        /// <summary>
        /// The smallest valid Telegram message identifier.
        /// </summary>
        public const int MinimumMessageId = 1;
    }
}
