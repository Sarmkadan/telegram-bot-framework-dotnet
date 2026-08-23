using System;
using System.Collections.Generic;
using System.Globalization;

namespace TelegramBotFramework.Services
{
    public static class BroadcastResultValidation
    {
        /// <summary>
        /// Validates the broadcast result and returns a list of validation errors.
        /// </summary>
        /// <param name="value">The broadcast result to validate.</param>
        /// <returns>A list of error messages if validation fails; otherwise an empty list.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this BroadcastResult value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate TotalChats
            if (value.TotalChats < 0)
            {
                errors.Add("TotalChats must be a non-negative integer.");
            }

            // Validate SuccessCount and FailedCount
            if (value.SuccessCount < 0 || value.SuccessCount > value.TotalChats)
            {
                errors.Add("SuccessCount must be between 0 and TotalChats.");
            }

            if (value.FailedCount < 0 || value.FailedCount > value.TotalChats)
            {
                errors.Add("FailedCount must be between 0 and TotalChats.");
            }

            if (value.SuccessCount + value.FailedCount != value.TotalChats)
            {
                errors.Add("SuccessCount + FailedCount must equal TotalChats.");
            }

            // Validate SuccessfulChatIds
            if (value.SuccessfulChatIds == null)
            {
                errors.Add("SuccessfulChatIds cannot be null.");
            }
            else if (value.SuccessfulChatIds.Count != value.SuccessCount)
            {
                errors.Add("SuccessfulChatIds count must match SuccessCount.");
            }

            // Validate Failures
            if (value.Failures == null)
            {
                errors.Add("Failures cannot be null.");
            }
            else if (value.Failures.Count != value.FailedCount)
            {
                errors.Add("Failures count must match FailedCount.");
            }

            // Validate Summary
            if (value.Summary != null)
            {
                if (string.IsNullOrWhiteSpace(value.Summary))
                {
                    errors.Add("Summary cannot be empty or whitespace.");
                }
            }

            return errors;
        }

        /// <summary>
        /// Checks if the broadcast result is valid.
        /// </summary>
        /// <param name="value">The broadcast result to validate.</param>
        /// <returns>True if valid; otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static bool IsValid(this BroadcastResult value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures the broadcast result is valid, throwing an exception if not.
        /// </summary>
        /// <param name="value">The broadcast result to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when validation fails, listing all errors.</exception>
        public static void EnsureValid(this BroadcastResult value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();
            if (errors.Count > 0)
            {
                throw new ArgumentException(string.Join(Environment.NewLine, errors), "value");
            }
        }
    }
}