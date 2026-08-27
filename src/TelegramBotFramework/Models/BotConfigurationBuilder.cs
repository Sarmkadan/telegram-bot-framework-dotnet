using System;

namespace TelegramBotFramework.Models
{
    /// <summary>
    /// Fluent builder for <see cref="BotConfiguration"/> that validates on build.
    /// </summary>
    public sealed class BotConfigurationBuilder
    {
        private readonly BotConfiguration _config = new();

        /// <summary>
        /// Pre-fills the builder from an existing <see cref="BotConfiguration"/> instance.
        /// </summary>
        /// <param name="template">The configuration to copy from.</param>
        /// <returns>The builder instance for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is null.</exception>
        public static BotConfigurationBuilder From(BotConfiguration template)
        {
            ArgumentNullException.ThrowIfNull(template);

            var builder = new BotConfigurationBuilder();
            builder._config.BotToken = template.BotToken;
            builder._config.BotUsername = template.BotUsername;
            builder._config.OwnerId = template.OwnerId;
            builder._config.DatabaseConnectionString = template.DatabaseConnectionString;
            builder._config.SessionTimeoutMinutes = template.SessionTimeoutMinutes;
            builder._config.MessageProcessingTimeoutSeconds = template.MessageProcessingTimeoutSeconds;
            builder._config.EnableLogging = template.EnableLogging;
            builder._config.LogLevel = template.LogLevel;
            builder._config.MaxConcurrentRequests = template.MaxConcurrentRequests;
            builder._config.EnableWebhook = template.EnableWebhook;
            return builder;
        }

        /// <summary>
        /// Sets the bot token.
        /// </summary>
        /// <param name="token">The bot token.</param>
        /// <returns>The builder instance for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="token"/> is null or empty.</exception>
        public BotConfigurationBuilder WithBotToken(string token)
        {
            ArgumentException.ThrowIfNullOrEmpty(token);
            _config.BotToken = token;
            return this;
        }

        /// <summary>
        /// Sets the bot username.
        /// </summary>
        /// <param name="username">The bot username.</param>
        /// <returns>The builder instance for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="username"/> is null or empty.</exception>
        public BotConfigurationBuilder WithBotUsername(string? username)
        {
            ArgumentException.ThrowIfNullOrEmpty(username);
            _config.BotUsername = username;
            return this;
        }

        /// <summary>
        /// Sets the owner ID.
        /// </summary>
        /// <param name="ownerId">The owner ID.</param>
        /// <returns>The builder instance for chaining.</returns>
        public BotConfigurationBuilder WithOwnerId(long? ownerId)
        {
            _config.OwnerId = ownerId;
            return this;
        }

        /// <summary>
        /// Sets the database connection string.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <returns>The builder instance for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="connectionString"/> is null or empty.</exception>
        public BotConfigurationBuilder WithDatabaseConnectionString(string connectionString)
        {
            ArgumentException.ThrowIfNullOrEmpty(connectionString);
            _config.DatabaseConnectionString = connectionString;
            return this;
        }

        /// <summary>
        /// Sets the session timeout in minutes.
        /// </summary>
        /// <param name="minutes">The timeout in minutes.</param>
        /// <returns>The builder instance for chaining.</returns>
        public BotConfigurationBuilder WithSessionTimeoutMinutes(int minutes)
        {
            _config.SessionTimeoutMinutes = minutes;
            return this;
        }

        /// <summary>
        /// Sets the message processing timeout in seconds.
        /// </summary>
        /// <param name="seconds">The timeout in seconds.</param>
        /// <returns>The builder instance for chaining.</returns>
        public BotConfigurationBuilder WithMessageProcessingTimeoutSeconds(int seconds)
        {
            _config.MessageProcessingTimeoutSeconds = seconds;
            return this;
        }

        /// <summary>
        /// Enables or disables logging.
        /// </summary>
        /// <param name="enable">Whether to enable logging.</param>
        /// <returns>The builder instance for chaining.</returns>
        public BotConfigurationBuilder WithEnableLogging(bool enable = true)
        {
            _config.EnableLogging = enable;
            return this;
        }

        /// <summary>
        /// Sets the log level.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <returns>The builder instance for chaining.</returns>
        public BotConfigurationBuilder WithLogLevel(LogLevel level)
        {
            _config.LogLevel = level;
            return this;
        }

        /// <summary>
        /// Sets the maximum number of concurrent requests.
        /// </summary>
        /// <param name="max">The maximum concurrent requests.</param>
        /// <returns>The builder instance for chaining.</returns>
        public BotConfigurationBuilder WithMaxConcurrentRequests(int max)
        {
            _config.MaxConcurrentRequests = max;
            return this;
        }

        /// <summary>
        /// Enables or disables the webhook.
        /// </summary>
        /// <param name="enable">Whether to enable the webhook.</param>
        /// <returns>The builder instance for chaining.</returns>
        public BotConfigurationBuilder WithEnableWebhook(bool enable = true)
        {
            _config.EnableWebhook = enable;
            return this;
        }

        /// <summary>
        /// Builds and returns the configured <see cref="BotConfiguration"/> instance.
        /// </summary>
        /// <returns>The configured <see cref="BotConfiguration"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when required properties are missing.</exception>
        public BotConfiguration Build()
        {
            ArgumentException.ThrowIfNullOrEmpty(_config.BotToken, nameof(_config.BotToken));
            return _config;
        }
    }
}
