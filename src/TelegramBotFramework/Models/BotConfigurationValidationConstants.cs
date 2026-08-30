namespace TelegramBotFramework.Models
{
    /// <summary>
    /// Constants for <see cref="BotConfigurationValidation"/>.
    /// </summary>
    internal static class BotConfigurationValidationConstants
    {
        // BotToken validation
        public const string BotTokenNullOrWhitespace = "BotToken must not be null or whitespace.";
        public const string BotTokenMinLength = "BotToken must be at least 2 characters long.";

        // BotUsername validation
        public const string BotUsernameNullOrWhitespace = "BotUsername must not be null or whitespace.";
        public const string BotUsernameMinLength = "BotUsername must be at least 2 characters long.";

        // OwnerId validation
        public const string OwnerIdMustBePositive = "OwnerId must be a positive number if specified.";

        // DatabaseConnectionString validation
        public const string DatabaseConnectionStringNullOrWhitespace = "DatabaseConnectionString must not be null or whitespace.";

        // SessionTimeoutMinutes validation
        public const string SessionTimeoutMinutesMin = "SessionTimeoutMinutes must be at least 1.";

        // MessageProcessingTimeoutSeconds validation
        public const string MessageProcessingTimeoutSecondsMin = "MessageProcessingTimeoutSeconds must be at least 1.";

        // MaxConcurrentRequests validation
        public const string MaxConcurrentRequestsMin = "MaxConcurrentRequests must be at least 1.";

        // RateLimitPerMinute validation
        public const string RateLimitPerMinuteMinWhenEnabled = "RateLimitPerMinute must be at least 1 when rate limiting is enabled.";

        // WebhookUrl validation
        public const string WebhookUrlMustBeValidAbsoluteUri = "WebhookUrl must be a valid absolute URI when specified.";
        public const string WebhookUrlMustUseHttps = "WebhookUrl must use HTTPS protocol.";

        // WebhookSecret validation
        public const string WebhookSecretNullOrWhitespaceWhenEnabled = "WebhookSecret must not be null or whitespace when webhook is enabled.";

        // AdminIds validation
        public const string AdminIdMustBePositive = "AdminIds[{0}] must be a positive number.";

        // LocalizationLanguage validation
        public const string LocalizationLanguageNullOrWhitespace = "LocalizationLanguage must not be null or whitespace.";
        public const string LocalizationLanguageMustBeTwoLetterIso = "LocalizationLanguage must be a 2-letter ISO language code.";

        // Magic numbers
        public const int MinBotTokenUsernameLength = 2;
        public const int MinPositiveInteger = 1;
        public const int LocalizationLanguageLength = 2;
    }
}