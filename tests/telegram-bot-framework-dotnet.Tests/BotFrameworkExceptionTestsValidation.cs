using System;
using System.Collections.Generic;
using System.Linq;

namespace TelegramBotFramework.Tests
{
    /// <summary>
    /// Validation helpers for <see cref="BotFrameworkExceptionTests"/>.
    /// </summary>
    public static class BotFrameworkExceptionTestsValidation
    {
        /// <summary>
        /// Validates the specified <see cref="BotFrameworkExceptionTests"/> instance and returns a list of human-readable problems.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns>A read-only list of problem messages, or empty if the instance is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(BotFrameworkExceptionTests value)
        {
            ArgumentNullException.ThrowIfNull(value);
            // BotFrameworkExceptionTests has no state to validate; it only contains test methods.
            return Array.Empty<string>();
        }

        /// <summary>
        /// Determines whether the specified <see cref="BotFrameworkExceptionTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns>true if the instance is valid; otherwise, false.</returns>
        public static bool IsValid(BotFrameworkExceptionTests value)
        {
            // If value is null, IsValid should return false (but Validate will throw).
            try
            {
                return Validate(value).Count == 0;
            }
            catch (ArgumentNullException)
            {
                return false;
            }
        }

        /// <summary>
        /// Ensures that the specified <see cref="BotFrameworkExceptionTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the instance has validation problems.</exception>
        public static void EnsureValid(BotFrameworkExceptionTests value)
        {
            ArgumentNullException.ThrowIfNull(value);
            var problems = Validate(value);
            if (problems.Count > 0)
            {
                throw new ArgumentException(string.Join(Environment.NewLine, problems), nameof(value));
            }
        }
    }
}