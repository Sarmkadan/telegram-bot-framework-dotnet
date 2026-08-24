using System;
using System.Collections.Generic;
using System.Linq;

namespace TelegramBotFramework.Tests
{
    /// <summary>
    /// Validation helpers for <see cref="BotFrameworkExceptionJsonExtensionsTests"/>.
    /// </summary>
    public static class BotFrameworkExceptionJsonExtensionsTestsValidation
    {
        /// <summary>
        /// Validates the specified <see cref="BotFrameworkExceptionJsonExtensionsTests"/> instance and returns a list of validation problems.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns>A read-only list of validation problems, or an empty list if the instance is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this BotFrameworkExceptionJsonExtensionsTests value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return Array.Empty<string>();
        }

        /// <summary>
        /// Determines whether the specified <see cref="BotFrameworkExceptionJsonExtensionsTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns>true if the instance is valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static bool IsValid(this BotFrameworkExceptionJsonExtensionsTests value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return true;
        }

        /// <summary>
        /// Ensures that the specified <see cref="BotFrameworkExceptionJsonExtensionsTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the instance is invalid, containing the validation problems in its message.</exception>
        public static void EnsureValid(this BotFrameworkExceptionJsonExtensionsTests value)
        {
            ArgumentNullException.ThrowIfNull(value);
            var problems = value.Validate();
            if (problems.Count > 0)
            {
                throw new ArgumentException(string.Join(Environment.NewLine, problems));
            }
        }
    }
}