using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace TelegramBotFramework.Services
{
    public static class LocalizationServiceValidation
    {
        /// <summary>
        /// Validates the specified <see cref="LocalizationService"/> instance.
        /// </summary>
        /// <param name="value">The localization service to validate.</param>
        /// <returns>A list of validation errors; empty if the service is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Null check via ArgumentNullException.ThrowIfNull")]
        public static IReadOnlyList<string> Validate(this LocalizationService? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Check for null or empty strings
            if (string.IsNullOrWhiteSpace(value.GetTemplate("key", "en")))
            {
                errors.Add("Template for key 'key' in language 'en' is null or empty.");
            }

            // Check for out-of-range numbers
            // NOTE: There are no numbers in LocalizationService

            // Check for default dates
            // NOTE: There are no dates in LocalizationService

            return errors;
        }

        /// <summary>
        /// Determines whether the specified <see cref="LocalizationService"/> instance is valid.
        /// </summary>
        /// <param name="value">The localization service to check.</param>
        /// <returns>True if the service is valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this LocalizationService? value)
        {
            return value is not null && Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="LocalizationService"/> instance is valid, throwing an exception if not.
        /// </summary>
        /// <param name="value">The localization service to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the service is invalid.</exception>
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Null check via ArgumentNullException.ThrowIfNull")]
        public static void EnsureValid(this LocalizationService? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = Validate(value);
            if (errors.Count > 0)
            {
                throw new ArgumentException("Localization service is invalid: " + string.Join("; ", errors));
            }
        }
    }
}
