namespace TelegramBotFramework.ConversationFlow
{
    /// <summary>
    /// Constants for magic values used in ConversationFlowOptionsExtensions.
    /// </summary>
    internal static class ConversationFlowOptionsExtensionsConstants
    {
        /// <summary>
        /// Error message for when timeout is less than or equal to zero.
        /// </summary>
        public const string DefaultFlowTimeoutMustBeGreaterThanZero = "Timeout must be greater than zero.";

        /// <summary>
        /// Error message for when max active flows per user is less than 1.
        /// </summary>
        public const string MaxActiveFlowsPerUserAtLeastOne = "There must be at least one active flow per user.";

        /// <summary>
        /// Error message for when max active flows per user is less than 1 (in validation).
        /// </summary>
        public const string MaxActiveFlowsPerUserMustBeAtLeastOne = "MaxActiveFlowsPerUser must be at least 1.";

        /// <summary>
        /// Error message for when max history per user is negative.
        /// </summary>
        public const string MaxHistoryPerUserCannotBeNegative = "MaxHistoryPerUser cannot be negative.";

        /// <summary>
        /// Error message for when cleanup interval minutes is less than 1.
        /// </summary>
        public const string CleanupIntervalMinutesMustBeAtLeastOne = "CleanupIntervalMinutes must be at least 1.";

        /// <summary>
        /// Error message for when abort keyword is empty but provided.
        /// </summary>
        public const string AbortKeywordCannotBeEmptyWhenProvided = "AbortKeyword cannot be empty when provided.";

        /// <summary>
        /// Error message for when abort acknowledgement message is empty but provided.
        /// </summary>
        public const string AbortAcknowledgementMessageCannotBeEmptyWhenProvided = "AbortAcknowledgementMessage cannot be empty when provided.";
    }
}