#nullable enable
using System;
using System.Collections.Generic;

namespace TelegramBotFramework.Strategies.Tests
{
    /// <summary>
    /// Validation helpers for <see cref="InMemoryRateLimitingStrategyTests"/>.
    /// </summary>
    public static class InMemoryRateLimitingStrategyTestsValidation
    {
        /// <summary>
        /// Validates the <see cref="InMemoryRateLimitingStrategyTests"/> instance and returns a list of validation problems.
        /// </summary>
        /// <param name="value">The <see cref="InMemoryRateLimitingStrategyTests"/> instance to validate.</param>
        /// <returns>A read-only list of human-readable validation problems, or empty if the instance is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this InMemoryRateLimitingStrategyTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            // InMemoryRateLimitingStrategyTests contains only test methods, no state to validate
            // We can only check that the instance itself is not null
            return Array.Empty<string>();
        }

        /// <summary>
        /// Determines whether the <see cref="InMemoryRateLimitingStrategyTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The <see cref="InMemoryRateLimitingStrategyTests"/> instance to validate.</param>
        /// <returns>true if the instance is valid; otherwise, false.</returns>
        public static bool IsValid(this InMemoryRateLimitingStrategyTests value)
        {
            try
            {
                Validate(value);
                return true;
            }
            catch (ArgumentNullException)
            {
                return false;
            }
        }

        /// <summary>
        /// Ensures that the <see cref="InMemoryRateLimitingStrategyTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The <see cref="InMemoryRateLimitingStrategyTests"/> instance to validate.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">If the instance is invalid, with a list of validation errors.</exception>
        public static void EnsureValid(this InMemoryRateLimitingStrategyTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = Validate(value);
            if (errors.Any())
            {
                throw new ArgumentException(
                    "The " + nameof(InMemoryRateLimitingStrategyTests) + " instance is invalid. Errors: " + string.Join("; ", errors),
                    nameof(value));
            }
        }
    }
}