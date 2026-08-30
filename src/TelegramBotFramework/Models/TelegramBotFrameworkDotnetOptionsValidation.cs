using System;
using System.Collections.Generic;

namespace TelegramBotFramework.Models
{
    /// <summary>
    /// Provides comprehensive validation helpers for <see cref="TelegramBotFrameworkDotnetOptions"/> instances.
    /// </summary>
    public static class TelegramBotFrameworkDotnetOptionsValidationExtensions
    {
        /// <summary>
        /// Validates the specified options instance.
        /// </summary>
        /// <param name="value">The options to validate.</param>
        /// <returns>A list of validation errors; empty if validation succeeds.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> ValidateOptions(this TelegramBotFrameworkDotnetOptions value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate BotToken
            if (string.IsNullOrWhiteSpace(value.BotToken))
            {
                errors.Add(TelegramBotFrameworkDotnetOptionsValidationExtensionsConstants.BotTokenNullOrWhitespaceError);
            }

            // Validate BotUsername
            if (string.IsNullOrWhiteSpace(value.BotUsername))
            {
                errors.Add(TelegramBotFrameworkDotnetOptionsValidationExtensionsConstants.BotUsernameNullOrWhitespaceError);
            }

            // Validate DatabaseConnectionString (optional)
            if (value.DatabaseConnectionString is not null && string.IsNullOrWhiteSpace(value.DatabaseConnectionString))
            {
                errors.Add(TelegramBotFrameworkDotnetOptionsValidationExtensionsConstants.DatabaseConnectionStringEmptyError);
            }

            // Validate SessionTimeoutMinutes
            if (value.SessionTimeoutMinutes is < TelegramBotFrameworkDotnetOptionsValidationExtensionsConstants.MinSessionTimeoutMinutes or > TelegramBotFrameworkDotnetOptionsValidationExtensionsConstants.MaxSessionTimeoutMinutes)
            {
                errors.Add(string.Format(TelegramBotFrameworkDotnetOptionsValidationExtensionsConstants.SessionTimeoutMinutesOutOfRangeErrorFormat, TelegramBotFrameworkDotnetOptionsValidationExtensionsConstants.MinSessionTimeoutMinutes, TelegramBotFrameworkDotnetOptionsValidationExtensionsConstants.MaxSessionTimeoutMinutes, value.SessionTimeoutMinutes));
            }

            // Validate MessageProcessingTimeoutSeconds
            if (value.MessageProcessingTimeoutSeconds is < TelegramBotFrameworkDotnetOptionsValidationExtensionsConstants.MinMessageProcessingTimeoutSeconds or > TelegramBotFrameworkDotnetOptionsValidationExtensionsConstants.MaxMessageProcessingTimeoutSeconds)
            {
                errors.Add(string.Format(TelegramBotFrameworkDotnetOptionsValidationExtensionsConstants.MessageProcessingTimeoutSecondsOutOfRangeErrorFormat, TelegramBotFrameworkDotnetOptionsValidationExtensionsConstants.MinMessageProcessingTimeoutSeconds, TelegramBotFrameworkDotnetOptionsValidationExtensionsConstants.MaxMessageProcessingTimeoutSeconds, value.MessageProcessingTimeoutSeconds));
            }

            // Validate MaxConcurrentRequests
            if (value.MaxConcurrentRequests is < TelegramBotFrameworkDotnetOptionsValidationExtensionsConstants.MinMaxConcurrentRequests or > TelegramBotFrameworkDotnetOptionsValidationExtensionsConstants.MaxMaxConcurrentRequests)
            {
                errors.Add(string.Format(TelegramBotFrameworkDotnetOptionsValidationExtensionsConstants.MaxConcurrentRequestsOutOfRangeErrorFormat, TelegramBotFrameworkDotnetOptionsValidationExtensionsConstants.MinMaxConcurrentRequests, TelegramBotFrameworkDotnetOptionsValidationExtensionsConstants.MaxMaxConcurrentRequests, value.MaxConcurrentRequests));
            }

            // Validate RateLimitPerMinute
            if (value.RateLimitPerMinute is < TelegramBotFrameworkDotnetOptionsValidationExtensionsConstants.MinRateLimitPerMinute or > TelegramBotFrameworkDotnetOptionsValidationExtensionsConstants.MaxRateLimitPerMinute)
            {
                errors.Add(string.Format(TelegramBotFrameworkDotnetOptionsValidationExtensionsConstants.RateLimitPerMinuteOutOfRangeErrorFormat, TelegramBotFrameworkDotnetOptionsValidationExtensionsConstants.MinRateLimitPerMinute, TelegramBotFrameworkDotnetOptionsValidationExtensionsConstants.MaxRateLimitPerMinute, value.RateLimitPerMinute));
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified options instance is valid.
        /// </summary>
        /// <param name="value">The options to check.</param>
        /// <returns>True if the options are valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static bool IsValidOptions(this TelegramBotFrameworkDotnetOptions value)
            => value.ValidateOptions().Count == 0;

        /// <summary>
        /// Validates the specified options instance and throws an exception if it is invalid.
        /// </summary>
        /// <param name="value">The options to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the options are invalid, containing a list of validation errors.</exception>
        public static void EnsureOptionsValid(this TelegramBotFrameworkDotnetOptions value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.ValidateOptions();
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    string.Format(TelegramBotFrameworkDotnetOptionsValidationExtensionsConstants.EnsureOptionsInvalidErrorFormat, string.Join("\n", errors)),
                    nameof(value));
            }
        }
    }
}