using System.ComponentModel.DataAnnotations;

namespace TelegramBotFramework.Models
{
    /// <summary>
    /// Options for the Telegram Bot Framework .NET.
    /// </summary>
    public class TelegramBotFrameworkDotnetOptions : ITelegramBotFrameworkDotnetOptions, IEquatable<TelegramBotFrameworkDotnetOptions>
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

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
        public bool Equals(TelegramBotFrameworkDotnetOptions? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return BotToken == other.BotToken &&
                   BotUsername == other.BotUsername &&
                   DatabaseConnectionString == other.DatabaseConnectionString &&
                   SessionTimeoutMinutes == other.SessionTimeoutMinutes &&
                   MessageProcessingTimeoutSeconds == other.MessageProcessingTimeoutSeconds &&
                   MaxConcurrentRequests == other.MaxConcurrentRequests &&
                   EnableLogging == other.EnableLogging &&
                   EnableRateLimiting == other.EnableRateLimiting;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TelegramBotFrameworkDotnetOptions)obj);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(BotToken, BotUsername, DatabaseConnectionString, SessionTimeoutMinutes, MessageProcessingTimeoutSeconds, MaxConcurrentRequests, EnableLogging, EnableRateLimiting);
        }

        /// <summary>
        /// Returns a value that indicates whether the values of two <see cref="TelegramBotFrameworkDotnetOptions"/> objects are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if the <paramref name="left"/> and <paramref name="right"/> parameters have the same value; otherwise, false.</returns>
        public static bool operator ==(TelegramBotFrameworkDotnetOptions? left, TelegramBotFrameworkDotnetOptions? right)
        {
            if (ReferenceEquals(left, null))
                return ReferenceEquals(right, null);
            return left.Equals(right);
        }

        /// <summary>
        /// Returns a value that indicates whether the values of two <see cref="TelegramBotFrameworkDotnetOptions"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, false.</returns>
        public static bool operator !=(TelegramBotFrameworkDotnetOptions? left, TelegramBotFrameworkDotnetOptions? right)
        {
            return !(left == right);
        }
    }
}
