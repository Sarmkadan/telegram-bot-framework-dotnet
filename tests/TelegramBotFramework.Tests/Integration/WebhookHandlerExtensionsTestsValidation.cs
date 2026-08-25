using System;
using System.Collections.Generic;
using System.Linq;

namespace TelegramBotFramework.Tests.Integration
{
    /// <summary>
    /// Contains validation methods for <see cref="WebhookHandlerExtensionsTests"/>.
    /// </summary>
    public static class WebhookHandlerExtensionsTestsValidation
    {
        /// <summary>
        /// Validates the specified <see cref="WebhookHandlerExtensionsTests"/> instance and returns a list of validation errors.
        /// </summary>
        /// <param name="value">The <see cref="WebhookHandlerExtensionsTests"/> instance to validate.</param>
        /// <returns>A read-only list of validation error messages. Empty if the instance is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
        public static IReadOnlyList<string> Validate(WebhookHandlerExtensionsTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            // WebhookHandlerExtensionsTests has no state to validate; it only contains test methods.
            // Therefore, there are no validation rules to apply.
            return Array.Empty<string>();
        }

        /// <summary>
        /// Determines whether the specified <see cref="WebhookHandlerExtensionsTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The <see cref="WebhookHandlerExtensionsTests"/> instance to validate.</param>
        /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(WebhookHandlerExtensionsTests value)
        {
            // Guard clause for null is handled by Validate, but we can also check here for efficiency.
            if (value is null)
                return false;

            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="WebhookHandlerExtensionsTests"/> instance is valid.
        /// Throws an <see cref="ArgumentException"/> if the instance is invalid.
        /// </summary>
        /// <param name="value">The <see cref="WebhookHandlerExtensionsTests"/> instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the instance is invalid, with a message listing the validation errors.</exception>
        public static void EnsureValid(WebhookHandlerExtensionsTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = Validate(value);
            if (problems.Any())
            {
                throw new ArgumentException(string.Join(Environment.NewLine, problems));
            }
        }
    }
}