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
                problems.Add(BotConfigurationValidationConstants.BotTokenNullOrWhitespace);
            }
            else if (value.BotToken.Trim().Length < BotConfigurationValidationConstants.MinBotTokenUsernameLength)
            {
                problems.Add(BotConfigurationValidationConstants.BotTokenMinLength);
            }

            // BotUsername validation
            if (string.IsNullOrWhiteSpace(value.BotUsername))
            {
                problems.Add(BotConfigurationValidationConstants.BotUsernameNullOrWhitespace);
            }
            else if (value.BotUsername.Trim().Length < BotConfigurationValidationConstants.MinBotTokenUsernameLength)
            {
                problems.Add(BotConfigurationValidationConstants.BotUsernameMinLength);
            }

            // OwnerId validation (if specified)
            if (value.OwnerId.HasValue && value.OwnerId.Value <= 0)
            {
                problems.Add(BotConfigurationValidationConstants.OwnerIdMustBePositive);
            }

            // DatabaseConnectionString validation (if specified)
            if (string.IsNullOrWhiteSpace(value.DatabaseConnectionString))
            {
                problems.Add(BotConfigurationValidationConstants.DatabaseConnectionStringNullOrWhitespace);
            }

            // SessionTimeoutMinutes validation
            if (value.SessionTimeoutMinutes < BotConfigurationValidationConstants.MinPositiveInteger)
            {
                problems.Add(BotConfigurationValidationConstants.SessionTimeoutMinutesMin);
            }

            // MessageProcessingTimeoutSeconds validation
            if (value.MessageProcessingTimeoutSeconds < BotConfigurationValidationConstants.MinPositiveInteger)
            {
                problems.Add(BotConfigurationValidationConstants.MessageProcessingTimeoutSecondsMin);
            }

            // MaxConcurrentRequests validation
            if (value.MaxConcurrentRequests < BotConfigurationValidationConstants.MinPositiveInteger)
            {
                problems.Add(BotConfigurationValidationConstants.MaxConcurrentRequestsMin);
            }

            // RateLimitPerMinute validation (if rate limiting is enabled)
            if (value.EnableRateLimiting && value.RateLimitPerMinute < BotConfigurationValidationConstants.MinPositiveInteger)
            {
                problems.Add(BotConfigurationValidationConstants.RateLimitPerMinuteMinWhenEnabled);
            }

            // WebhookUrl validation (if webhook is enabled and URL is specified)
            if (value.EnableWebhook && !string.IsNullOrWhiteSpace(value.WebhookUrl))
            {
                if (!Uri.TryCreate(value.WebhookUrl, UriKind.Absolute, out var uri))
                {
                    problems.Add(BotConfigurationValidationConstants.WebhookUrlMustBeValidAbsoluteUri);
                }
                else if (uri.Scheme != Uri.UriSchemeHttps)
                {
                    problems.Add(BotConfigurationValidationConstants.WebhookUrlMustUseHttps);
                }
            }

            // WebhookSecret validation (if webhook is enabled and secret is specified)
            if (value.EnableWebhook && string.IsNullOrWhiteSpace(value.WebhookSecret))
            {
                problems.Add(BotConfigurationValidationConstants.WebhookSecretNullOrWhitespaceWhenEnabled);
            }

            // AdminIds validation
            if (value.AdminIds is not null)
            {
                for (var i = 0; i < value.AdminIds.Count; i++)
                {
                    if (value.AdminIds[i] <= 0)
                    {
                        problems.Add(string.Format(BotConfigurationValidationConstants.AdminIdMustBePositive, i));
                    }
                }
            }

            // LocalizationLanguage validation (if specified)
            if (string.IsNullOrWhiteSpace(value.LocalizationLanguage))
            {
                problems.Add(BotConfigurationValidationConstants.LocalizationLanguageNullOrWhitespace);
            }
            else if (value.LocalizationLanguage.Length != BotConfigurationValidationConstants.LocalizationLanguageLength)
            {
                problems.Add(BotConfigurationValidationConstants.LocalizationLanguageMustBeTwoLetterIso);
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