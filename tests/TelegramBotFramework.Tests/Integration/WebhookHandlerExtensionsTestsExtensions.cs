using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TelegramBotFramework.Tests.Integration
{
    /// <summary>
    /// Extension methods that provide higher‑level helpers for <see cref="WebhookHandlerExtensionsTests"/>.
    /// </summary>
    public static class WebhookHandlerExtensionsTestsExtensions
    {
        /// <summary>
        /// Executes <c>GetMessageText_UpdateIsNull_ThrowsArgumentNullException</c> and returns <c>true</c>
        /// if the expected <see cref="ArgumentNullException"/> is thrown.
        /// </summary>
        /// <param name="test">The test instance to operate on.</param>
        /// <returns>
        /// <c>true</c> when the method throws <see cref="ArgumentNullException"/>; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">When <paramref name="test"/> is <c>null</c>.</exception>
        public static bool ThrowsArgumentNullOnUpdateIsNull(this WebhookHandlerExtensionsTests test)
        {
            ArgumentNullException.ThrowIfNull(test);

            try
            {
                test.GetMessageText_UpdateIsNull_ThrowsArgumentNullException();
                // If no exception is thrown the test failed.
                return false;
            }
            catch (ArgumentNullException)
            {
                // Expected exception.
                return true;
            }
        }

        /// <summary>
        /// Runs the two message‑text related test methods and returns a read‑only list of
        /// human‑readable results.
        /// </summary>
        /// <param name="test">The test instance to operate on.</param>
        /// <returns>
        /// An <see cref="IReadOnlyList{T}"/> containing a result string for each executed test.
        /// </returns>
        /// <exception cref="ArgumentNullException">When <paramref name="test"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> RunMessageTextTests(this WebhookHandlerExtensionsTests test)
        {
            ArgumentNullException.ThrowIfNull(test);

            var results = new List<string>();

            try
            {
                test.GetMessageText_MessageIsNull_ReturnsNull();
                results.Add("GetMessageText_MessageIsNull_ReturnsNull passed");
            }
            catch (Exception ex)
            {
                results.Add($"GetMessageText_MessageIsNull_ReturnsNull failed: {ex.Message}");
            }

            try
            {
                test.GetMessageText_UpdateIsNull_ThrowsArgumentNullException();
                results.Add("GetMessageText_UpdateIsNull_ThrowsArgumentNullException passed");
            }
            catch (Exception ex)
            {
                results.Add($"GetMessageText_UpdateIsNull_ThrowsArgumentNullException failed: {ex.Message}");
            }

            return new ReadOnlyCollection<string>(results);
        }

        /// <summary>
        /// Checks callback‑data handling based on the expected match condition.
        /// </summary>
        /// <param name="test">The test instance to operate on.</param>
        /// <param name="expectMatch">
        /// <c>true</c> to verify that matching callback data returns <c>true</c>;
        /// <c>false</c> to verify that non‑matching data returns <c>false</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the underlying test method completes without throwing; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">When <paramref name="test"/> is <c>null</c>.</exception>
        public static bool VerifyCallbackData(this WebhookHandlerExtensionsTests test, bool expectMatch)
        {
            ArgumentNullException.ThrowIfNull(test);

            try
            {
                if (expectMatch)
                {
                    test.HasCallbackData_CallbackDataMatches_ReturnsTrue();
                }
                else
                {
                    test.HasCallbackData_CallbackDataDoesNotMatch_ReturnsFalse();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
