using System;
using System.Collections.Generic;

namespace TelegramBotFramework.Models
{
    /// <summary>
    /// Fluent builder for <see cref="BotConfiguration"/> that validates on build.
    /// </summary>
    public sealed class BotConfigurationBuilder
    {
        private readonly BotConfiguration _config = new();

        public BotConfigurationBuilder SetBotToken(string token)
        {
            _config.BotToken = token;
            return this;
        }

        public BotConfigurationBuilder SetBotUsername(string username)
        {
            _config.BotUsername = username;
            return this;
        }

        public BotConfigurationBuilder SetOwnerId(long? ownerId)
        {
            _config.OwnerId = ownerId;
            return this;
        }

        public BotConfigurationBuilder SetDatabaseConnectionString(string connectionString)
        {
            _config.DatabaseConnectionString = connectionString;
            return this;
        }

        public BotConfigurationBuilder SetSessionTimeoutMinutes(int minutes)
        {
            _config.SessionTimeoutMinutes = minutes;
            return this;
        }

        public BotConfigurationBuilder SetMessageProcessingTimeoutSeconds(int seconds)
        {
            _config.MessageProcessingTimeoutSeconds = seconds;
            return this;
        }

        public BotConfigurationBuilder EnableLogging(bool enable = true)
        {
            _config.EnableLogging = enable;
            return this;
        }

        public BotConfigurationBuilder SetLogLevel(LogLevel level)
        {
            _config.LogLevel = level;
            return this;
        }

        public BotConfigurationBuilder SetMaxConcurrentRequests(int max)
        {
            _config.MaxConcurrentRequests = max;
            return this;
        }

        public BotConfigurationBuilder EnableWebhook(bool enable = true)
        {
            _config.EnableWebhook = enable;
            return this;
        }

        public BotConfigurationBuilder SetApiKey(string? apiKey)
        {
            _config.ApiKey = apiKey;
            return this;
        }

        public BotConfigurationBuilder SetWebhookUrl(string? url)
        {
            _config.WebhookUrl = url;
            return this;
        }

        public BotConfigurationBuilder SetWebhookSecret(string? secret)
        {
            _config.WebhookSecret = secret;
            return this;
        }

        public BotConfigurationBuilder AddCustomSetting(string key, string value)
        {
            _config.CustomSettings[key] = value;
            return this;
        }

        public BotConfigurationBuilder SetCustomSettings(Dictionary<string, string> settings)
        {
            _config.CustomSettings = settings ?? new();
            return this;
        }

        public BotConfigurationBuilder AddAdminId(long adminId)
        {
            _config.AdminIds.Add(adminId);
            return this;
        }

        public BotConfigurationBuilder SetAdminIds(List<long> adminIds)
        {
            _config.AdminIds = adminIds ?? new();
            return this;
        }

        public BotConfigurationBuilder EnableRateLimiting(bool enable = true)
        {
            _config.EnableRateLimiting = enable;
            return this;
        }

        public BotConfigurationBuilder SetRateLimitPerMinute(int limit)
        {
            _config.RateLimitPerMinute = limit;
            return this;
        }

        public BotConfigurationBuilder SetLocalizationLanguage(string? language)
        {
            _config.LocalizationLanguage = language;
            return this;
        }

        public BotConfiguration Build()
        {
            var problems = _config.ValidateConfiguration();
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"BotConfiguration is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
            }
            return _config;
        }
    }
}
