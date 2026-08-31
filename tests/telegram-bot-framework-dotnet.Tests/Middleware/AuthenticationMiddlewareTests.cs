#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Tests for AuthenticationMiddleware class
// =============================================================================

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using TelegramBotFramework.Models;
using Xunit;

namespace TelegramBotFramework.Middleware.Tests;

/// <summary>
/// Tests for the AuthenticationMiddleware class.
/// </summary>
public sealed class AuthenticationMiddlewareTests : IAuthenticationMiddlewareTests
{
    private readonly Mock<ILogger<AuthenticationMiddleware>> _loggerMock;

    public AuthenticationMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<AuthenticationMiddleware>>();
    }

    /// <summary>
    /// Tests that middleware with valid Bearer token passes authentication.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenBearerTokenValid_PassesAuthentication()
    {
        // Arrange
        var config = new BotConfiguration { ApiKey = AuthenticationMiddlewareTestsConstants.ValidApiKey };
        var middleware = new AuthenticationMiddleware(
            next: async (context) =>
            {
                context.Items[AuthenticationMiddlewareTestsConstants.AuthenticatedItemKey] = true;
                await Task.CompletedTask;
            },
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Headers[AuthenticationMiddlewareTestsConstants.AuthorizationHeader] = AuthenticationMiddlewareTestsConstants.BearerPrefix + AuthenticationMiddlewareTestsConstants.ValidApiKey;
        context.Request.Path = AuthenticationMiddlewareTestsConstants.ApiTestPath;

        // Act
        await middleware.InvokeAsync(context, config);

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeOk);
        context.Items.Should().ContainKey(AuthenticationMiddlewareTestsConstants.AuthenticatedAtItemKey);
    }

    /// <summary>
    /// Tests that middleware with valid X-API-Key header passes authentication.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenXApiKeyValid_PassesAuthentication()
    {
        // Arrange
        var config = new BotConfiguration { ApiKey = AuthenticationMiddlewareTestsConstants.ValidApiKey };
        var middleware = new AuthenticationMiddleware(
            next: async (context) =>
            {
                context.Items[AuthenticationMiddlewareTestsConstants.AuthenticatedItemKey] = true;
                await Task.CompletedTask;
            },
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Headers[AuthenticationMiddlewareTestsConstants.XApiKeyHeader] = AuthenticationMiddlewareTestsConstants.ValidApiKey;
        context.Request.Path = AuthenticationMiddlewareTestsConstants.ApiTestPath;

        // Act
        await middleware.InvokeAsync(context, config);

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeOk);
        context.Items.Should().ContainKey(AuthenticationMiddlewareTestsConstants.AuthenticatedAtItemKey);
    }

    /// <summary>
    /// Tests that middleware with valid api_key query parameter passes authentication.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenQueryApiKeyValid_PassesAuthentication()
    {
        // Arrange
        var config = new BotConfiguration { ApiKey = AuthenticationMiddlewareTestsConstants.ValidApiKey };
        var middleware = new AuthenticationMiddleware(
            next: async (context) =>
            {
                context.Items[AuthenticationMiddlewareTestsConstants.AuthenticatedItemKey] = true;
                await Task.CompletedTask;
            },
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            [AuthenticationMiddlewareTestsConstants.ApiKeyQueryParam] = AuthenticationMiddlewareTestsConstants.ValidApiKey
        });
        context.Request.Path = AuthenticationMiddlewareTestsConstants.ApiTestPath;

        // Act
        await middleware.InvokeAsync(context, config);

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeOk);
        context.Items.Should().ContainKey(AuthenticationMiddlewareTestsConstants.AuthenticatedAtItemKey);
    }

    /// <summary>
    /// Tests that middleware with invalid Bearer token returns 401.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenBearerTokenInvalid_Returns401()
    {
        // Arrange
        var config = new BotConfiguration { ApiKey = AuthenticationMiddlewareTestsConstants.CorrectApiKey };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Headers[AuthenticationMiddlewareTestsConstants.AuthorizationHeader] = AuthenticationMiddlewareTestsConstants.BearerPrefix + AuthenticationMiddlewareTestsConstants.InvalidApiKey;
        context.Request.Path = AuthenticationMiddlewareTestsConstants.ApiTestPath;

        // Act
        await middleware.InvokeAsync(context, config);

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeUnauthorized);
        context.Items.Should().NotContainKey(AuthenticationMiddlewareTestsConstants.AuthenticatedAtItemKey);
    }

    /// <summary>
    /// Tests that middleware with missing Authorization header returns 401.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenAuthorizationHeaderMissing_Returns401()
    {
        // Arrange
        var config = new BotConfiguration { ApiKey = AuthenticationMiddlewareTestsConstants.AlternativeValidApiKey };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Path = AuthenticationMiddlewareTestsConstants.ApiTestPath;

        // Act
        await middleware.InvokeAsync(context, config);

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeUnauthorized);
        context.Items.Should().NotContainKey(AuthenticationMiddlewareTestsConstants.AuthenticatedAtItemKey);
    }

    /// <summary>
    /// Tests that middleware with null ApiKey in config returns 401.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenApiKeyNull_Returns401()
    {
        // Arrange
        var config = new BotConfiguration { ApiKey = null };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Headers[AuthenticationMiddlewareTestsConstants.AuthorizationHeader] = AuthenticationMiddlewareTestsConstants.BearerPrefix + AuthenticationMiddlewareTestsConstants.TestKey;
        context.Request.Path = AuthenticationMiddlewareTestsConstants.ApiTestPath;

        // Act
        await middleware.InvokeAsync(context, config);

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeUnauthorized);
        context.Items.Should().NotContainKey(AuthenticationMiddlewareTestsConstants.AuthenticatedAtItemKey);
    }

    /// <summary>
    /// Tests that middleware with empty ApiKey in config returns 401.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenApiKeyEmpty_Returns401()
    {
        // Arrange
        var config = new BotConfiguration { ApiKey = string.Empty };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Headers[AuthenticationMiddlewareTestsConstants.AuthorizationHeader] = AuthenticationMiddlewareTestsConstants.BearerPrefix + AuthenticationMiddlewareTestsConstants.TestKey;
        context.Request.Path = AuthenticationMiddlewareTestsConstants.ApiTestPath;

        // Act
        await middleware.InvokeAsync(context, config);

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeUnauthorized);
        context.Items.Should().NotContainKey(AuthenticationMiddlewareTestsConstants.AuthenticatedAtItemKey);
    }

    /// <summary>
    /// Tests that middleware with whitespace ApiKey in config returns 401.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenApiKeyWhitespace_Returns401()
    {
        // Arrange
        var config = new BotConfiguration { ApiKey = AuthenticationMiddlewareTestsConstants.WhitespaceApiKey };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Headers[AuthenticationMiddlewareTestsConstants.AuthorizationHeader] = AuthenticationMiddlewareTestsConstants.BearerPrefix + AuthenticationMiddlewareTestsConstants.TestKey;
        context.Request.Path = AuthenticationMiddlewareTestsConstants.ApiTestPath;

        // Act
        await middleware.InvokeAsync(context, config);

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeUnauthorized);
        context.Items.Should().NotContainKey(AuthenticationMiddlewareTestsConstants.AuthenticatedAtItemKey);
    }

    /// <summary>
    /// Tests that middleware with null HttpContext.Request.Path does not throw.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenPathIsNull_DoesNotThrow()
    {
        // Arrange
        var config = new BotConfiguration { ApiKey = AuthenticationMiddlewareTestsConstants.AlternativeValidApiKey };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Headers[AuthenticationMiddlewareTestsConstants.AuthorizationHeader] = AuthenticationMiddlewareTestsConstants.BearerPrefix + AuthenticationMiddlewareTestsConstants.TestKey;
        context.Request.Path = null;

        // Act
        var act = () => middleware.InvokeAsync(context, config);

        // Assert
        await act.Should().NotThrowAsync();
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeOk);
        context.Items.Should().ContainKey(AuthenticationMiddlewareTestsConstants.AuthenticatedAtItemKey);
    }

    /// <summary>
    /// Tests that middleware with missing Authorization header does not throw.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenAuthorizationHeaderMissing_DoesNotThrow()
    {
        // Arrange
        var config = new BotConfiguration { ApiKey = AuthenticationMiddlewareTestsConstants.AlternativeValidApiKey };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Path = AuthenticationMiddlewareTestsConstants.ApiTestPath;

        // Act
        var act = () => middleware.InvokeAsync(context, config);

        // Assert
        await act.Should().NotThrowAsync();
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeUnauthorized);
    }

    /// <summary>
    /// Tests that middleware with public endpoint does not require authentication.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenPublicEndpoint_DoesNotRequireAuthentication()
    {
        // Arrange
        var config = new BotConfiguration { ApiKey = null };
        var middleware = new AuthenticationMiddleware(
            next: async (context) =>
            {
                context.Items[AuthenticationMiddlewareTestsConstants.PublicEndpointItemKey] = true;
                await Task.CompletedTask;
            },
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Path = AuthenticationMiddlewareTestsConstants.HealthPath;

        // Act
        await middleware.InvokeAsync(context, config);

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeOk);
        context.Items.Should().ContainKey(AuthenticationMiddlewareTestsConstants.PublicEndpointItemKey);
    }

    /// <summary>
    /// Tests that middleware with /api/webhook is a public endpoint.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenWebhookEndpoint_PublicEndpoint()
    {
        // Arrange
        var config = new BotConfiguration { ApiKey = AuthenticationMiddlewareTestsConstants.SomeKey };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Path = AuthenticationMiddlewareTestsConstants.WebhookPath;

        // Act
        await middleware.InvokeAsync(context, config);

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeOk);
        context.Items.Should().NotContainKey(AuthenticationMiddlewareTestsConstants.AuthenticatedAtItemKey);
    }

    /// <summary>
    /// Tests that middleware with /swagger is a public endpoint.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenSwaggerEndpoint_PublicEndpoint()
    {
        // Arrange
        var config = new BotConfiguration { ApiKey = AuthenticationMiddlewareTestsConstants.SomeKey };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Path = AuthenticationMiddlewareTestsConstants.SwaggerPath;

        // Act
        await middleware.InvokeAsync(context, config);

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeOk);
        context.Items.Should().NotContainKey(AuthenticationMiddlewareTestsConstants.AuthenticatedAtItemKey);
    }

    /// <summary>
    /// Tests that middleware with /api/v1/bot/update is a public endpoint.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenBotUpdateEndpoint_PublicEndpoint()
    {
        // Arrange
        var config = new BotConfiguration { ApiKey = AuthenticationMiddlewareTestsConstants.SomeKey };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Path = AuthenticationMiddlewareTestsConstants.BotUpdatePath;

        // Act
        await middleware.InvokeAsync(context, config);

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeOk);
        context.Items.Should().NotContainKey(AuthenticationMiddlewareTestsConstants.AuthenticatedAtItemKey);
    }

    /// <summary>
    /// Tests that middleware with case-insensitive public endpoint matching.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenPublicEndpointCaseInsensitive_PassesWithoutAuth()
    {
        // Arrange
        var config = new BotConfiguration { ApiKey = AuthenticationMiddlewareTestsConstants.SomeKey };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Path = AuthenticationMiddlewareTestsConstants.HealthPathUppercase;

        // Act
        await middleware.InvokeAsync(context, config);

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeOk);
    }

    /// <summary>
    /// Tests that middleware with Bearer token case-sensitive comparison.
    /// </n>
    [Fact]
    public async Task InvokeAsync_WhenBearerTokenCaseSensitive_FailsOnCaseMismatch()
    {
        // Arrange
        var config = new BotConfiguration { ApiKey = AuthenticationMiddlewareTestsConstants.CaseSensitiveKey };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Headers[AuthenticationMiddlewareTestsConstants.AuthorizationHeader] = AuthenticationMiddlewareTestsConstants.BearerPrefix + AuthenticationMiddlewareTestsConstants.CaseMismatchKey; // lowercase
        context.Request.Path = AuthenticationMiddlewareTestsConstants.ApiTestPath;

        // Act
        await middleware.InvokeAsync(context, config);

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeUnauthorized);
    }

    /// <summary>
    /// Tests that middleware with X-API-Key header case-sensitive comparison.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenXApiKeyCaseSensitive_FailsOnCaseMismatch()
    {
        // Arrange
        var config = new BotConfiguration { ApiKey = AuthenticationMiddlewareTestsConstants.CaseSensitiveKey };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Headers[AuthenticationMiddlewareTestsConstants.XApiKeyHeader] = AuthenticationMiddlewareTestsConstants.CaseMismatchKey; // lowercase
        context.Request.Path = AuthenticationMiddlewareTestsConstants.ApiTestPath;

        // Act
        await middleware.InvokeAsync(context, config);

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeUnauthorized);
    }

    /// <summary>
    /// Tests that middleware with api_key query parameter case-sensitive comparison.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenQueryApiKeyCaseSensitive_FailsOnCaseMismatch()
    {
        // Arrange
        var config = new BotConfiguration { ApiKey = AuthenticationMiddlewareTestsConstants.CaseSensitiveKey };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            [AuthenticationMiddlewareTestsConstants.ApiKeyQueryParam] = AuthenticationMiddlewareTestsConstants.CaseMismatchKey
        });
        context.Request.Path = AuthenticationMiddlewareTestsConstants.ApiTestPath;

        // Act
        await middleware.InvokeAsync(context, config);

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeUnauthorized);
    }
}
