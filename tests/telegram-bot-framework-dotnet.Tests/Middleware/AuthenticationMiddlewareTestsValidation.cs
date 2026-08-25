#nullable enable
using System;
using System.Collections.Generic;

namespace TelegramBotFramework.Middleware.Tests
{
    /// <summary>
    /// Validation helpers for <see cref="AuthenticationMiddlewareTests"/>.
    /// </summary>
    public static class AuthenticationMiddlewareTestsValidation
    {
        /// <summary>
        /// Validates the <see cref="AuthenticationMiddlewareTests"/> instance and returns a list of validation errors.
        /// </summary>
        /// <param name="value">The <see cref="AuthenticationMiddlewareTests"/> instance to validate.</param>
        /// <returns>A list of validation error messages. Empty if the instance is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this AuthenticationMiddlewareTests value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return Array.Empty<string>();
        }

        /// <summary>
        /// Determines whether the specified <see cref="AuthenticationMiddlewareTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The <see cref="AuthenticationMiddlewareTests"/> instance to validate.</param>
        /// <returns>true if the instance is valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static bool IsValid(this AuthenticationMiddlewareTests value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return true;
        }

        /// <summary>
        /// Ensures that the specified <see cref="AuthenticationMiddlewareTests"/> instance is valid.
        /// Throws an <see cref="ArgumentException"/> if the instance is invalid.
        /// </summary>
        /// <param name="value">The <see cref="AuthenticationMiddlewareTests"/> instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the instance is invalid.</exception>
        public static void EnsureValid(this AuthenticationMiddlewareTests value)
        {
            ArgumentNullException.ThrowIfNull(value);
            // No validation logic required as there are no validateable members.
        }
    }
}