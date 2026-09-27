#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// Extension methods for AuthenticationMiddlewareTests
// =============================================================================

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using TelegramBotFramework.Models;

namespace TelegramBotFramework.Middleware.Tests;

/// <summary>
/// Extension methods for AuthenticationMiddlewareTests to reduce boilerplate and improve test readability.
/// </summary>
public static class AuthenticationMiddlewareTestsExtensions
{
    /// <summary>
    /// Creates a test AuthenticationMiddleware with a mock logger and default next delegate.
    /// </summary>
    /// <param name="tests">The AuthenticationMiddlewareTests instance.</param>
    /// <param name="next">Optional custom next delegate. If null, uses a delegate that sets AuthenticatedItemKey.</param>
    /// <returns>A configured AuthenticationMiddleware instance.</returns>
    public static AuthenticationMiddleware CreateTestMiddleware(
        this AuthenticationMiddlewareTests tests,
        Func<HttpContext, Task>? next = null)
    {
        ArgumentNullException.ThrowIfNull(tests);

        return new AuthenticationMiddleware(
            next: next ?? (async (context) =>
            {
                context.Items[AuthenticationMiddlewareTestsConstants.AuthenticatedItemKey] = true;
                await Task.CompletedTask;
            }),
            logger: tests._loggerMock.Object);
    }

    /// <summary>
    /// Creates a default HttpContext for testing with the specified path.
    /// </summary>
    /// <param name="tests">The AuthenticationMiddlewareTests instance.</param>
    /// <param name="path">The request path for the test context.</param>
    /// <returns>A configured DefaultHttpContext instance.</returns>
    public static DefaultHttpContext CreateTestContext(
        this AuthenticationMiddlewareTests tests,
        string path)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentException.ThrowIfNullOrEmpty(path);

        return new DefaultHttpContext { Request = { Path = path } };
    }

    /// <summary>
    /// Asserts that the middleware invocation resulted in successful authentication (200 OK and authenticated flag set).
    /// </summary>
    /// <param name="tests">The AuthenticationMiddlewareTests instance.</param>
    /// <param name="context">The HttpContext to assert against.</param>
    public static void AssertAuthenticationSuccess(
        this AuthenticationMiddlewareTests tests,
        HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(context);

        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeOk);
        context.Items.Should().ContainKey(AuthenticationMiddlewareTestsConstants.AuthenticatedAtItemKey);
    }

    /// <summary>
    /// Asserts that the middleware invocation resulted in authentication failure (401 Unauthorized and no authenticated flag).
    /// </summary>
    /// <param name="tests">The AuthenticationMiddlewareTests instance.</param>
    /// <param name="context">The HttpContext to assert against.</param>
    public static void AssertAuthenticationFailure(
        this AuthenticationMiddlewareTests tests,
        HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(context);

        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeUnauthorized);
        context.Items.Should().NotContainKey(AuthenticationMiddlewareTestsConstants.AuthenticatedAtItemKey);
    }

    /// <summary>
    /// Asserts that the middleware invocation resulted in public endpoint access (200 OK and public endpoint flag set).
    /// </summary>
    /// <param name="tests">The AuthenticationMiddlewareTests instance.</param>
    /// <param name="context">The HttpContext to assert against.</param>
    public static void AssertPublicEndpointAccess(
        this AuthenticationMiddlewareTests tests,
        HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(context);

        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeOk);
        context.Items.Should().ContainKey(AuthenticationMiddlewareTestsConstants.PublicEndpointItemKey);
    }
}