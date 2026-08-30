namespace TelegramBotFramework.Models
{
    internal static class TelegramBotFrameworkDotnetOptionsValidationExtensionsConstants
    {
        // String literals for error messages
        public const string BotTokenNullOrWhitespaceError = "BotToken cannot be null or whitespace.";
        public const string BotUsernameNullOrWhitespaceError = "BotUsername cannot be null or whitespace.";
        public const string DatabaseConnectionStringEmptyError = "DatabaseConnectionString cannot be empty if specified.";
        public const string SessionTimeoutMinutesOutOfRangeErrorFormat = "SessionTimeoutMinutes must be between {0} and {1} inclusive, but was {2}.";
        public const string MessageProcessingTimeoutSecondsOutOfRangeErrorFormat = "MessageProcessingTimeoutSeconds must be between {0} and {1} inclusive, but was {2}.";
        public const string MaxConcurrentRequestsOutOfRangeErrorFormat = "MaxConcurrentRequests must be between {0} and {1} inclusive, but was {2}.";
        public const string RateLimitPerMinuteOutOfRangeErrorFormat = "RateLimitPerMinute must be between {0} and {1} inclusive, but was {2}.";
        public const string EnsureOptionsInvalidErrorFormat = "TelegramBotFrameworkDotnetOptions is invalid. Validation errors:\n{0}";

        // Numeric bounds
        public const int MinSessionTimeoutMinutes = 1;
        public const int MaxSessionTimeoutMinutes = 60;
        public const int MinMessageProcessingTimeoutSeconds = 1;
        public const int MaxMessageProcessingTimeoutSeconds = 300;
        public const int MinMaxConcurrentRequests = 1;
        public const int MaxMaxConcurrentRequests = 100;
        public const int MinRateLimitPerMinute = 1;
        public const int MaxRateLimitPerMinute = 600;
    }
}