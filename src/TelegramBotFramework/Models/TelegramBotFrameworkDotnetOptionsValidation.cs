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
                errors.Add("BotToken cannot be null or whitespace.");
            }

            // Validate BotUsername
            if (string.IsNullOrWhiteSpace(value.BotUsername))
            {
                errors.Add("BotUsername cannot be null or whitespace.");
            }

            // Validate DatabaseConnectionString (optional)
            if (value.DatabaseConnectionString is not null && string.IsNullOrWhiteSpace(value.DatabaseConnectionString))
            {
                errors.Add("DatabaseConnectionString cannot be empty if specified.");
            }

            // Validate SessionTimeoutMinutes
            if (value.SessionTimeoutMinutes is < 1 or > 60)
            {
                errors.Add($"SessionTimeoutMinutes must be between 1 and 60 inclusive, but was {value.SessionTimeoutMinutes}.");
            }

            // Validate MessageProcessingTimeoutSeconds
            if (value.MessageProcessingTimeoutSeconds is < 1 or > 300)
            {
                errors.Add($"MessageProcessingTimeoutSeconds must be between 1 and 300 inclusive, but was {value.MessageProcessingTimeoutSeconds}.");
            }

            // Validate MaxConcurrentRequests
            if (value.MaxConcurrentRequests is < 1 or > 100)
            {
                errors.Add($"MaxConcurrentRequests must be between 1 and 100 inclusive, but was {value.MaxConcurrentRequests}.");
            }

            // Validate RateLimitPerMinute
            if (value.RateLimitPerMinute is < 1 or > 600)
            {
                errors.Add($"RateLimitPerMinute must be between 1 and 600 inclusive, but was {value.RateLimitPerMinute}.");
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
                    $"TelegramBotFrameworkDotnetOptions is invalid. Validation errors:\n{string.Join("\n", errors)}",
                    nameof(value));
            }
        }
    }
}