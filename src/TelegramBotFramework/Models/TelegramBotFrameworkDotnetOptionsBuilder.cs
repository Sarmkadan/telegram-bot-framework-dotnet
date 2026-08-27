namespace TelegramBotFramework.Models
{
    /// <summary>
    /// Builder for <see cref="TelegramBotFrameworkDotnetOptions"/>.
    /// </summary>
    public class TelegramBotFrameworkDotnetOptionsBuilder
    {
        private string _botToken = null!;
        private string _botUsername = null!;
        private string? _databaseConnectionString;
        private int _sessionTimeoutMinutes = 30;
        private int _messageProcessingTimeoutSeconds = 10;
        private int _maxConcurrentRequests = 10;
        private bool _enableLogging = true;
        private bool _enableRateLimiting = true;
        private int _rateLimitPerMinute = 30;

        /// <summary>
        /// Creates a new instance of the builder with default values.
        /// </summary>
        public TelegramBotFrameworkDotnetOptionsBuilder()
        {
        }

        /// <summary>
        /// Sets the bot token.
        /// </summary>
        /// <param name="botToken">The bot token.</param>
        /// <returns>The builder instance for chaining.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="botToken"/> is null.</exception>
        public TelegramBotFrameworkDotnetOptionsBuilder WithBotToken(string botToken)
        {
            ArgumentNullException.ThrowIfNull(botToken);
            _botToken = botToken;
            return this;
        }

        /// <summary>
        /// Sets the bot username.
        /// </summary>
        /// <param name="botUsername">The bot username.</param>
        /// <returns>The builder instance for chaining.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="botUsername"/> is null.</exception>
        public TelegramBotFrameworkDotnetOptionsBuilder WithBotUsername(string botUsername)
        {
            ArgumentNullException.ThrowIfNull(botUsername);
            _botUsername = botUsername;
            return this;
        }

        /// <summary>
        /// Sets the database connection string.
        /// </summary>
        /// <param name="databaseConnectionString">The database connection string.</param>
        /// <returns>The builder instance for chaining.</returns>
        public TelegramBotFrameworkDotnetOptionsBuilder WithDatabaseConnectionString(string? databaseConnectionString)
        {
            _databaseConnectionString = databaseConnectionString;
            return this;
        }

        /// <summary>
        /// Sets the session timeout in minutes.
        /// </summary>
        /// <param name="sessionTimeoutMinutes">The session timeout in minutes.</param>
        /// <returns>The builder instance for chaining.</returns>
        public TelegramBotFrameworkDotnetOptionsBuilder WithSessionTimeoutMinutes(int sessionTimeoutMinutes)
        {
            _sessionTimeoutMinutes = sessionTimeoutMinutes;
            return this;
        }

        /// <summary>
        /// Sets the message processing timeout in seconds.
        /// </summary>
        /// <param name="messageProcessingTimeoutSeconds">The message processing timeout in seconds.</param>
        /// <returns>The builder instance for chaining.</returns>
        public TelegramBotFrameworkDotnetOptionsBuilder WithMessageProcessingTimeoutSeconds(int messageProcessingTimeoutSeconds)
        {
            _messageProcessingTimeoutSeconds = messageProcessingTimeoutSeconds;
            return this;
        }

        /// <summary>
        /// Sets the maximum number of concurrent requests.
        /// </summary>
        /// <param name="maxConcurrentRequests">The maximum number of concurrent requests.</param>
        /// <returns>The builder instance for chaining.</returns>
        public TelegramBotFrameworkDotnetOptionsBuilder WithMaxConcurrentRequests(int maxConcurrentRequests)
        {
            _maxConcurrentRequests = maxConcurrentRequests;
            return this;
        }

        /// <summary>
        /// Sets whether logging is enabled.
        /// </summary>
        /// <param name="enableLogging">True if logging is enabled; otherwise, false.</param>
        /// <returns>The builder instance for chaining.</returns>
        public TelegramBotFrameworkDotnetOptionsBuilder WithEnableLogging(bool enableLogging)
        {
            _enableLogging = enableLogging;
            return this;
        }

        /// <summary>
        /// Sets whether rate limiting is enabled.
        /// </summary>
        /// <param name="enableRateLimiting">True if rate limiting is enabled; otherwise, false.</param>
        /// <returns>The builder instance for chaining.</returns>
        public TelegramBotFrameworkDotnetOptionsBuilder WithEnableRateLimiting(bool enableRateLimiting)
        {
            _enableRateLimiting = enableRateLimiting;
            return this;
        }

        /// <summary>
        /// Sets the rate limit per minute.
        /// </summary>
        /// <param name="rateLimitPerMinute">The rate limit per minute.</param>
        /// <returns>The builder instance for chaining.</returns>
        public TelegramBotFrameworkDotnetOptionsBuilder WithRateLimitPerMinute(int rateLimitPerMinute)
        {
            _rateLimitPerMinute = rateLimitPerMinute;
            return this;
        }

        /// <summary>
        /// Creates a builder pre-filled with values from an existing options instance.
        /// </summary>
        /// <param name="template">The options instance to copy values from.</param>
        /// <returns>A builder instance with values from the template.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="template"/> is null.</exception>
        public static TelegramBotFrameworkDotnetOptionsBuilder From(TelegramBotFrameworkDotnetOptions template)
        {
            ArgumentNullException.ThrowIfNull(template);

            return new TelegramBotFrameworkDotnetOptionsBuilder
            {
                _botToken = template.BotToken,
                _botUsername = template.BotUsername,
                _databaseConnectionString = template.DatabaseConnectionString,
                _sessionTimeoutMinutes = template.SessionTimeoutMinutes,
                _messageProcessingTimeoutSeconds = template.MessageProcessingTimeoutSeconds,
                _maxConcurrentRequests = template.MaxConcurrentRequests,
                _enableLogging = template.EnableLogging,
                _enableRateLimiting = template.EnableRateLimiting,
                _rateLimitPerMinute = template.RateLimitPerMinute
            };
        }

        /// <summary>
        /// Builds the <see cref="TelegramBotFrameworkDotnetOptions"/> instance.
        /// </summary>
        /// <returns>A configured <see cref="TelegramBotFrameworkDotnetOptions"/> instance.</returns>
        /// <exception cref="ArgumentException">If required properties are missing.</exception>
        public TelegramBotFrameworkDotnetOptions Build()
        {
            if (string.IsNullOrEmpty(_botToken))
            {
                throw new ArgumentException("Bot token is required.", nameof(_botToken));
            }

            if (string.IsNullOrEmpty(_botUsername))
            {
                throw new ArgumentException("Bot username is required.", nameof(_botUsername));
            }

            return new TelegramBotFrameworkDotnetOptions
            {
                BotToken = _botToken,
                BotUsername = _botUsername,
                DatabaseConnectionString = _databaseConnectionString,
                SessionTimeoutMinutes = _sessionTimeoutMinutes,
                MessageProcessingTimeoutSeconds = _messageProcessingTimeoutSeconds,
                MaxConcurrentRequests = _maxConcurrentRequests,
                EnableLogging = _enableLogging,
                EnableRateLimiting = _enableRateLimiting,
                RateLimitPerMinute = _rateLimitPerMinute
            };
        }
    }
}