using System;
using System.Collections.Generic;

namespace TelegramBotFramework.Models
{
    /// <summary>
    /// Provides validation helpers for <see cref="BotConfiguration"/> instances.
    /// </summary>
    public static class BotConfigurationValidation
    {
        /// <summary>
        /// Validates the specified <see cref="BotConfiguration"/> instance.
        /// </summary>
        /// <param name="value">The configuration to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> ValidateConfiguration(this BotConfiguration value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // BotToken validation
            if (string.IsNullOrWhiteSpace(value.BotToken))
            {
                problems.Add("BotToken must not be null or whitespace.");
            }
            else if (value.BotToken.Trim().Length < 2)
            {
                problems.Add("BotToken must be at least 2 characters long.");
            }

            // BotUsername validation
            if (string.IsNullOrWhiteSpace(value.BotUsername))
            {
                problems.Add("BotUsername must not be null or whitespace.");
            }
            else if (value.BotUsername.Trim().Length < 2)
            {
                problems.Add("BotUsername must be at least 2 characters long.");
            }

            // OwnerId validation (if specified)
            if (value.OwnerId.HasValue && value.OwnerId.Value <= 0)
            {
                problems.Add("OwnerId must be a positive number if specified.");
            }

            // DatabaseConnectionString validation (if specified)
            if (string.IsNullOrWhiteSpace(value.DatabaseConnectionString))
            {
                problems.Add("DatabaseConnectionString must not be null or whitespace.");
            }

            // SessionTimeoutMinutes validation
            if (value.SessionTimeoutMinutes < 1)
            {
                problems.Add("SessionTimeoutMinutes must be at least 1.");
            }

            // MessageProcessingTimeoutSeconds validation
            if (value.MessageProcessingTimeoutSeconds < 1)
            {
                problems.Add("MessageProcessingTimeoutSeconds must be at least 1.");
            }

            // MaxConcurrentRequests validation
            if (value.MaxConcurrentRequests < 1)
            {
                problems.Add("MaxConcurrentRequests must be at least 1.");
            }

            // RateLimitPerMinute validation (if rate limiting is enabled)
            if (value.EnableRateLimiting && value.RateLimitPerMinute < 1)
            {
                problems.Add("RateLimitPerMinute must be at least 1 when rate limiting is enabled.");
            }

            // WebhookUrl validation (if webhook is enabled and URL is specified)
            if (value.EnableWebhook && !string.IsNullOrWhiteSpace(value.WebhookUrl))
            {
                if (!Uri.TryCreate(value.WebhookUrl, UriKind.Absolute, out var uri))
                {
                    problems.Add("WebhookUrl must be a valid absolute URI when specified.");
                }
                else if (uri.Scheme != Uri.UriSchemeHttps)
                {
                    problems.Add("WebhookUrl must use HTTPS protocol.");
                }
            }

            // WebhookSecret validation (if webhook is enabled and secret is specified)
            if (value.EnableWebhook && string.IsNullOrWhiteSpace(value.WebhookSecret))
            {
                problems.Add("WebhookSecret must not be null or whitespace when webhook is enabled.");
            }

            // AdminIds validation
            if (value.AdminIds is not null)
            {
                for (var i = 0; i < value.AdminIds.Count; i++)
                {
                    if (value.AdminIds[i] <= 0)
                    {
                        problems.Add($"AdminIds[{i}] must be a positive number.");
                    }
                }
            }

            // LocalizationLanguage validation (if specified)
            if (string.IsNullOrWhiteSpace(value.LocalizationLanguage))
            {
                problems.Add("LocalizationLanguage must not be null or whitespace.");
            }
            else if (value.LocalizationLanguage.Length != 2)
            {
                problems.Add("LocalizationLanguage must be a 2-letter ISO language code.");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="BotConfiguration"/> is valid.
        /// </summary>
        /// <param name="value">The configuration to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValidConfiguration(this BotConfiguration value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return value.ValidateConfiguration().Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="BotConfiguration"/> is valid, throwing an exception if not.
        /// </summary>
        /// <param name="value">The configuration to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing a list of problems.</exception>
        public static void EnsureValidConfiguration(this BotConfiguration value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.ValidateConfiguration();
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"BotConfiguration is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
            }
        }
    }
}