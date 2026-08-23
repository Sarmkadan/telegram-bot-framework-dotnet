using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace TelegramBotFramework.Services
{
    /// <summary>
    /// Provides validation helpers for <see cref="RateLimitStats"/>.
    /// </summary>
    public static class RateLimitStatsValidation
    {
        /// <summary>
        /// Validates the given <paramref name="value"/> and returns a list of issues.
        /// </summary>
        /// <param name="value">The <see cref="RateLimitStats"/> to validate.</param>
        /// <returns>A list of human-readable issues. Empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this RateLimitStats value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var issues = new List<string>();

            if (value.MessagesPerSecond <= 0)
            {
                issues.Add("MessagesPerSecond must be greater than zero.");
            }

            if (value.MaxConcurrency <= 0)
            {
                issues.Add("MaxConcurrency must be greater than zero.");
            }

            if (value.TotalMessagesSent < 0)
            {
                issues.Add("TotalMessagesSent cannot be negative.");
            }

            if (value.TotalMessagesFailed < 0)
            {
                issues.Add("TotalMessagesFailed cannot be negative.");
            }

            if (value.AverageMessagesPerSecond < 0)
            {
                issues.Add("AverageMessagesPerSecond cannot be negative.");
            }

            if (value.CurrentConcurrency < 0)
            {
                issues.Add("CurrentConcurrency cannot be negative.");
            }

            if (value.Timestamp == DateTime.MinValue || value.Timestamp == DateTime.MaxValue)
            {
                issues.Add("Timestamp must be a valid date/time (not default/min/max values).");
            }

            return issues;
        }

        /// <summary>
        /// Checks if the given <paramref name="value"/> is valid.
        /// </summary>
        /// <param name="value">The <see cref="RateLimitStats"/> to check.</param>
        /// <returns>True if valid, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static bool IsValid(this RateLimitStats value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures the given <paramref name="value"/> is valid, throwing if not.
        /// </summary>
        /// <param name="value">The <see cref="RateLimitStats"/> to validate.</param>
        /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static void EnsureValid(this RateLimitStats value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var issues = value.Validate();
            if (issues.Count > 0)
            {
                throw new ArgumentException(string.Join("\n", issues), "value");
            }
        }
    }
}
