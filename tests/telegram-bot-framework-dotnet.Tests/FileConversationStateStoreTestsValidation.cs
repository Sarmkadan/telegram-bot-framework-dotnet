using System;
using System.Collections.Generic;
using System.Linq;

namespace TelegramBotFramework.Tests
{
    /// <summary>
    /// Validation helpers for <see cref="FileConversationStateStoreTests"/>.
    /// </summary>
    public static class FileConversationStateStoreTestsValidation
    {
        /// <summary>
        /// Validates the <see cref="FileConversationStateStoreTests"/> instance and returns a list of validation problems.
        /// </summary>
        /// <param name="value">The <see cref="FileConversationStateStoreTests"/> instance to validate.</param>
        /// <returns>A read-only list of human-readable validation problems, or empty if the instance is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this FileConversationStateStoreTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            // FileConversationStateStoreTests contains only test methods and private
            // test scaffolding (temp directory, store, logger); there is no public
            // state to validate beyond the instance itself being non-null.
            return Array.Empty<string>();
        }

        /// <summary>
        /// Determines whether the <see cref="FileConversationStateStoreTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The <see cref="FileConversationStateStoreTests"/> instance to validate.</param>
        /// <returns>true if the instance is valid; otherwise, false.</returns>
        public static bool IsValid(this FileConversationStateStoreTests value)
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
        /// Ensures that the <see cref="FileConversationStateStoreTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The <see cref="FileConversationStateStoreTests"/> instance to validate.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">If the instance is invalid, with a list of validation errors.</exception>
        public static void EnsureValid(this FileConversationStateStoreTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = Validate(value);
            if (errors.Any())
            {
                throw new ArgumentException(
                    "The " + nameof(FileConversationStateStoreTests) + " instance is invalid. Errors: " + string.Join("; ", errors),
                    nameof(value));
            }
        }
    }
}