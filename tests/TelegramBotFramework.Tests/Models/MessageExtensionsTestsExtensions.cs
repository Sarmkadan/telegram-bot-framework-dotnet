using System;

/// <summary>
/// Provides extension methods for <see cref="MessageExtensionsTests"/> that allow
/// programmatic execution of its test methods and reporting of their success.
/// </summary>
namespace TelegramBotFramework.Tests.Models
{
    /// <summary>
    /// Extension methods for <see cref="MessageExtensionsTests"/>.
    /// </summary>
    public static class MessageExtensionsTestsExtensions
    {
        /// <summary>
        /// Executes the appropriate <c>IsCommand</c> test based on the expected command state
        /// and returns <c>true</c> if the test completes without throwing an exception.
        /// </summary>
        /// <param name="test">The <see cref="MessageExtensionsTests"/> instance on which to run the test.</param>
        /// <param name="expectCommand"><c>true</c> to run the test that expects a command; <c>false</c> to run the test that expects no command.</param>
        /// <returns><c>true</c> if the selected test method finishes without throwing; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is <c>null</c>.</exception>
        public static bool RunIsCommandTest(this MessageExtensionsTests test, bool expectCommand)
        {
            ArgumentNullException.ThrowIfNull(test);

            try
            {
                if (expectCommand)
                {
                    test.IsCommand_MessageIsCommand_ReturnsTrue();
                }
                else
                {
                    test.IsCommand_MessageIsNotCommand_ReturnsFalse();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Executes the <c>HasAttachments</c> test and returns <c>true</c> if the test completes without throwing an exception.
        /// </summary>
        /// <param name="test">The <see cref="MessageExtensionsTests"/> instance on which to run the test.</param>
        /// <returns><c>true</c> if the test method finishes without throwing; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is <c>null</c>.</exception>
        public static bool RunHasAttachmentsTest(this MessageExtensionsTests test)
        {
            ArgumentNullException.ThrowIfNull(test);

            try
            {
                test.HasAttachments_MessageHasAttachments_ReturnsTrue();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Executes the <c>GetTypeString</c> test and returns <c>true</c> if the test completes without throwing an exception.
        /// </summary>
        /// <param name="test">The <see cref="MessageExtensionsTests"/> instance on which to run the test.</param>
        /// <returns><c>true</c> if the test method finishes without throwing; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is <c>null</c>.</exception>
        public static bool RunGetTypeStringTest(this MessageExtensionsTests test)
        {
            ArgumentNullException.ThrowIfNull(test);

            try
            {
                test.GetTypeString_MessageHasType_ReturnsTypeString();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Executes the <c>IsReply</c> test and returns <c>true</c> if the test completes without throwing an exception.
        /// </summary>
        /// <param name="test">The <see cref="MessageExtensionsTests"/> instance on which to run the test.</param>
        /// <returns><c>true</c> if the test method finishes without throwing; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is <c>null</c>.</exception>
        public static bool RunIsReplyTest(this MessageExtensionsTests test)
        {
            ArgumentNullException.ThrowIfNull(test);

            try
            {
                test.IsReply_MessageIsReply_ReturnsTrue();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
