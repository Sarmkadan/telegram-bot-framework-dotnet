using System;

namespace TelegramBotFramework.Middleware.Tests;

/// <summary>
/// Provides extension methods for <see cref="AuthorizationMiddlewareTests"/>.
/// </summary>
public static class AuthorizationMiddlewareTestsExtensions
{
    /// <summary>
    /// Validates the priority return value of the authorization middleware test.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is null.</exception>
    public static void VerifyAuthorizationPriority(this AuthorizationMiddlewareTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);
        tests.Priority_ReturnsCorrectValue();
    }

    /// <summary>
    /// Executes all constructor requirement tests.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is null.</exception>
    public static void RunAllConstructorTests(this AuthorizationMiddlewareTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);
        tests.Constructor_WhenCommandServiceNull_Throws();
        tests.Constructor_WhenUserServiceNull_Throws();
        tests.Constructor_WhenLoggerNull_Throws();
    }
}
