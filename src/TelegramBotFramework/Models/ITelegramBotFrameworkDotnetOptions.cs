namespace TelegramBotFramework.Models
{
    /// <summary>
    /// Options for the Telegram Bot Framework .NET.
    /// </summary>
    public interface ITelegramBotFrameworkDotnetOptions
    {
        /// <summary>
        /// The token for the Telegram bot.
        /// </summary>
        /// <value>
        /// The bot token.
        /// </value>
        string BotToken { get; set; }

        /// <summary>
        /// The username for the Telegram bot.
        /// </summary>
        /// <value>
        /// The bot username.
        /// </value>
        string BotUsername { get; set; }

        /// <summary>
        /// The connection string for the database.
        /// </summary>
        /// <value>
        /// The database connection string.
        /// </value>
        string? DatabaseConnectionString { get; set; }

        /// <summary>
        /// The timeout in minutes for user sessions.
        /// </summary>
        /// <value>
        /// The session timeout in minutes.
        /// </value>
        /// <remarks>
        /// The session will be automatically expired after this time.
        /// </remarks>
        int SessionTimeoutMinutes { get; set; }

        /// <summary>
        /// The timeout in seconds for message processing.
        /// </summary>
        /// <value>
        /// The message processing timeout in seconds.
        /// </value>
        /// <remarks>
        /// The message will be automatically discarded after this time.
        /// </remarks>
        int MessageProcessingTimeoutSeconds { get; set; }

        /// <summary>
        /// The maximum number of concurrent requests.
        /// </summary>
        /// <value>
        /// The maximum concurrent requests.
        /// </value>
        int MaxConcurrentRequests { get; set; }

        /// <summary>
        /// Whether to enable logging.
        /// </summary>
        /// <value>
        /// True if logging is enabled; otherwise, false.
        /// </value>
        bool EnableLogging { get; set; }

        /// <summary>
        /// Whether to enable rate limiting.
        /// </summary>
        /// <value>
        /// True if rate limiting is enabled; otherwise, false.
        /// </value>
        bool EnableRateLimiting { get; set; }

        /// <summary>
        /// The rate limit per minute.
        /// </summary>
        /// <value>
        /// The rate limit per minute.
        /// </value>
        /// <remarks>
        /// The bot will be automatically rate limited after this number of requests per minute.
        /// </remarks>
        int RateLimitPerMinute { get; set; }
    }
}