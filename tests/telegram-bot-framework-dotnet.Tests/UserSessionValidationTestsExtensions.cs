using System;
using System.Collections.Generic;
using TelegramBotFramework.Models;
using Xunit;

/// <summary>
/// Extension methods that simplify writing tests for <see cref="UserSessionValidationTests"/>.
/// </summary>
namespace TelegramBotFramework.Tests
{
    public static class UserSessionValidationTestsExtensions
    {
        /// <summary>
        /// Creates a <see cref="UserSession"/> instance that satisfies all validation rules.
        /// </summary>
        /// <param name="_">The test class instance (unused).</param>
        /// <returns>A valid <see cref="UserSession"/>.</returns>
        public static UserSession CreateValidUserSession(this UserSessionValidationTests _)
            => new()
            {
                SessionId = "valid-session",
                UserId = 12345L,
                ChatId = 67890L,
                CurrentContext = "default",
                CurrentMenuId = "main",
                CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                LastActivityAt = DateTime.UtcNow.AddMinutes(-1),
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

        /// <summary>
        /// Validates the supplied <see cref="UserSession"/> using the production validation logic.
        /// </summary>
        /// <param name="_">The test class instance (unused).</param>
        /// <param name="session">The session to validate.</param>
        /// <returns>An <see cref="IReadOnlyList{T}"/> of validation error messages.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="session"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> ValidateSession(this UserSessionValidationTests _, UserSession session)
        {
            ArgumentNullException.ThrowIfNull(session);
            return UserSessionValidation.ValidateSession(session);
        }

        /// <summary>
        /// Asserts that the supplied <see cref="UserSession"/> passes validation (i.e., returns no errors).
        /// </summary>
        /// <param name="_">The test class instance (unused).</param>
        /// <param name="session">The session to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="session"/> is <c>null</c>.</exception>
        public static void AssertSessionIsValid(this UserSessionValidationTests _, UserSession session)
        {
            ArgumentNullException.ThrowIfNull(session);
            var errors = UserSessionValidation.ValidateSession(session);
            Assert.Empty(errors);
        }

        /// <summary>
        /// Asserts that the supplied <see cref="UserSession"/> fails validation with at least one error.
        /// </summary>
        /// <param name="_">The test class instance (unused).</param>
        /// <param name="session">The session to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="session"/> is <c>null</c>.</exception>
        public static void AssertSessionHasErrors(this UserSessionValidationTests _, UserSession session)
        {
            ArgumentNullException.ThrowIfNull(session);
            var errors = UserSessionValidation.ValidateSession(session);
            Assert.NotEmpty(errors);
        }
    }
}
