using System.ComponentModel.DataAnnotations;

namespace TelegramBotFramework.Models
{
    /// <summary>
    /// Options for the Telegram Bot Framework .NET.
    /// </summary>
    public class TelegramBotFrameworkDotnetOptions
    {
        /// <summary>
        /// The token for the Telegram bot.
        /// </summary>
        /// <value>
        /// The bot token.
        /// </value>
        [Required]
        public string BotToken { get; set; } = null!;

        /// <summary>
        /// The username for the Telegram bot.
        /// </summary>
        /// <value>
        /// The bot username.
        /// </value>
        [Required]
        public string BotUsername { get; set; } = null!;

        /// <summary>
        /// The connection string for the database.
        /// </summary>
        /// <value>
        /// The database connection string.
        /// </value>
        [Url]
        public string? DatabaseConnectionString { get; set; }

        /// <summary>
        /// The timeout in minutes for user sessions.
        /// </summary>
        /// <value>
        /// The session timeout in minutes.
        /// </value>
        /// <remarks>
        /// The session will be automatically expired after this time.
        /// </remarks>
        [Range(1, 60)]
        public int SessionTimeoutMinutes { get; set; } = 30;

        /// <summary>
        /// The timeout in seconds for message processing.
        /// </summary>
        /// <value>
        /// The message processing timeout in seconds.
        /// </value>
        /// <remarks>
        /// The message will be automatically discarded after this time.
        /// </remarks>
        [Range(1, 300)]
        public int MessageProcessingTimeoutSeconds { get; set; } = 10;

        /// <summary>
        /// The maximum number of concurrent requests.
        /// </summary>
        /// <value>
        /// The maximum concurrent requests.
        /// </value>
        [Range(1, 100)]
        public int MaxConcurrentRequests { get; set; } = 10;

        /// <summary>
        /// Whether to enable logging.
        /// </summary>
        /// <value>
        /// True if logging is enabled; otherwise, false.
        /// </value>
        public bool EnableLogging { get; set; } = true;

        /// <summary>
        /// Whether to enable rate limiting.
        /// </summary>
        /// <value>
        /// True if rate limiting is enabled; otherwise, false.
        /// </value>
        public bool EnableRateLimiting { get; set; } = true;

        /// <summary>
        /// The rate limit per minute.
        /// </summary>
        /// <value>
        /// The rate limit per minute.
        /// </value>
        /// <remarks>
        /// The bot will be automatically rate limited after this number of requests per minute.
        /// </remarks>
        [Range(1, 600)]
        public int RateLimitPerMinute { get; set; } = 30;
    }
}
