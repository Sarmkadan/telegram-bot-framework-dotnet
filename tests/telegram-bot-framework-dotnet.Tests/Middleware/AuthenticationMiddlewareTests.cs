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
public sealed class AuthenticationMiddlewareTests
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
        var config = new BotConfiguration { ApiKey = "test-secret-key-123" };
        var middleware = new AuthenticationMiddleware(
            next: async (context) =>
            {
                context.Items["Authenticated"] = true;
                await Task.CompletedTask;
            },
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = "Bearer test-secret-key-123";
        context.Request.Path = "/api/test";

        // Act
        await middleware.InvokeAsync(context, config);

        // Assert
        context.Response.StatusCode.Should().Be(200);
        context.Items.Should().ContainKey("AuthenticatedAt");
    }

    /// <summary>
    /// Tests that middleware with valid X-API-Key header passes authentication.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenXApiKeyValid_PassesAuthentication()
    {
        // Arrange
        var config = new BotConfiguration { ApiKey = "test-secret-key-123" };
        var middleware = new AuthenticationMiddleware(
            next: async (context) =>
            {
                context.Items["Authenticated"] = true;
                await Task.CompletedTask;
            },
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Headers["X-API-Key"] = "test-secret-key-123";
        context.Request.Path = "/api/test";

        // Act
        await middleware.InvokeAsync(context, config);

        // Assert
        context.Response.StatusCode.Should().Be(200);
        context.Items.Should().ContainKey("AuthenticatedAt");
    }

    /// <summary>
    /// Tests that middleware with valid api_key query parameter passes authentication.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenQueryApiKeyValid_PassesAuthentication()
    {
        // Arrange
        var config = new BotConfiguration { ApiKey = "test-secret-key-123" };
        var middleware = new AuthenticationMiddleware(
            next: async (context) =>
            {
                context.Items["Authenticated"] = true;
                await Task.CompletedTask;
            },
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            ["api_key"] = "test-secret-key-123"
        });
        context.Request.Path = "/api/test";

        // Act
        await middleware.InvokeAsync(context, config);

        // Assert
        context.Response.StatusCode.Should().Be(200);
        context.Items.Should().ContainKey("AuthenticatedAt");
    }

    /// <summary>
    /// Tests that middleware with invalid Bearer token returns 401.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenBearerTokenInvalid_Returns401()
    {
        // Arrange
        var config = new BotConfiguration { ApiKey = "correct-secret-key" };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = "Bearer wrong-secret-key";
        context.Request.Path = "/api/test";

        // Act
        await middleware.InvokeAsync(context, config);

        // Assert
        context.Response.StatusCode.Should().Be(401);
        context.Items.Should().NotContainKey("AuthenticatedAt");
    }

    /// <summary>
    /// Tests that middleware with missing Authorization header returns 401.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenAuthorizationHeaderMissing_Returns401()
    {
        // Arrange
        var config = new BotConfiguration { ApiKey = "test-secret-key" };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";

        // Act
        await middleware.InvokeAsync(context, config);

        // Assert
        context.Response.StatusCode.Should().Be(401);
        context.Items.Should().NotContainKey("AuthenticatedAt");
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
        context.Request.Headers.Authorization = "Bearer test-key";
        context.Request.Path = "/api/test";

        // Act
        await middleware.InvokeAsync(context, config);

        // Assert
        context.Response.StatusCode.Should().Be(401);
        context.Items.Should().NotContainKey("AuthenticatedAt");
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
        context.Request.Headers.Authorization = "Bearer test-key";
        context.Request.Path = "/api/test";

        // Act
        await middleware.InvokeAsync(context, config);

        // Assert
        context.Response.StatusCode.Should().Be(401);
        context.Items.Should().NotContainKey("AuthenticatedAt");
    }

    /// <summary>
    /// Tests that middleware with whitespace ApiKey in config returns 401.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenApiKeyWhitespace_Returns401()
    {
        // Arrange
        var config = new BotConfiguration { ApiKey = "   " };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = "Bearer test-key";
        context.Request.Path = "/api/test";

        // Act
        await middleware.InvokeAsync(context, config);

        // Assert
        context.Response.StatusCode.Should().Be(401);
        context.Items.Should().NotContainKey("AuthenticatedAt");
    }

    /// <summary>
    /// Tests that middleware with null HttpContext.Request.Path does not throw.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenPathIsNull_DoesNotThrow()
    {
        // Arrange
        var config = new BotConfiguration { ApiKey = "test-secret-key" };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = "Bearer test-secret-key";
        context.Request.Path = null;

        // Act
        var act = () => middleware.InvokeAsync(context, config);

        // Assert
        await act.Should().NotThrowAsync();
        context.Response.StatusCode.Should().Be(200);
        context.Items.Should().ContainKey("AuthenticatedAt");
    }

    /// <summary>
    /// Tests that middleware with missing Authorization header does not throw.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenAuthorizationHeaderMissing_DoesNotThrow()
    {
        // Arrange
        var config = new BotConfiguration { ApiKey = "test-secret-key" };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";

        // Act
        var act = () => middleware.InvokeAsync(context, config);

        // Assert
        await act.Should().NotThrowAsync();
        context.Response.StatusCode.Should().Be(401);
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
                context.Items["PublicEndpoint"] = true;
                await Task.CompletedTask;
            },
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Path = "/health";

        // Act
        await middleware.InvokeAsync(context, config);

        // Assert
        context.Response.StatusCode.Should().Be(200);
        context.Items.Should().ContainKey("PublicEndpoint");
    }

    /// <summary>
    /// Tests that middleware with /api/webhook is a public endpoint.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenWebhookEndpoint_PublicEndpoint()
    {
        // Arrange
        var config = new BotConfiguration { ApiKey = "some-key" };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/webhook";

        // Act
        await middleware.InvokeAsync(context, config);

        // Assert
        context.Response.StatusCode.Should().Be(200);
        context.Items.Should().NotContainKey("AuthenticatedAt");
    }

    /// <summary>
    /// Tests that middleware with /swagger is a public endpoint.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenSwaggerEndpoint_PublicEndpoint()
    {
        // Arrange
        var config = new BotConfiguration { ApiKey = "some-key" };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Path = "/swagger/index.html";

        // Act
        await middleware.InvokeAsync(context, config);

        // Assert
        context.Response.StatusCode.Should().Be(200);
        context.Items.Should().NotContainKey("AuthenticatedAt");
    }

    /// <summary>
    /// Tests that middleware with /api/v1/bot/update is a public endpoint.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenBotUpdateEndpoint_PublicEndpoint()
    {
        // Arrange
        var config = new BotConfiguration { ApiKey = "some-key" };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/bot/update";

        // Act
        await middleware.InvokeAsync(context, config);

        // Assert
        context.Response.StatusCode.Should().Be(200);
        context.Items.Should().NotContainKey("AuthenticatedAt");
    }

    /// <summary>
    /// Tests that middleware with case-insensitive public endpoint matching.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenPublicEndpointCaseInsensitive_PassesWithoutAuth()
    {
        // Arrange
        var config = new BotConfiguration { ApiKey = "some-key" };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Path = "/HEALTH";

        // Act
        await middleware.InvokeAsync(context, config);

        // Assert
        context.Response.StatusCode.Should().Be(200);
    }

    /// <summary>
    /// Tests that middleware with Bearer token case-sensitive comparison.
    /// </n>
    [Fact]
    public async Task InvokeAsync_WhenBearerTokenCaseSensitive_FailsOnCaseMismatch()
    {
        // Arrange
        var config = new BotConfiguration { ApiKey = "TestKey123" };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = "Bearer testkey123"; // lowercase
        context.Request.Path = "/api/test";

        // Act
        await middleware.InvokeAsync(context, config);

        // Assert
        context.Response.StatusCode.Should().Be(401);
    }

    /// <summary>
    /// Tests that middleware with X-API-Key header case-sensitive comparison.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenXApiKeyCaseSensitive_FailsOnCaseMismatch()
    {
        // Arrange
        var config = new BotConfiguration { ApiKey = "TestKey123" };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Headers["X-API-Key"] = "testkey123"; // lowercase
        context.Request.Path = "/api/test";

        // Act
        await middleware.InvokeAsync(context, config);

        // Assert
        context.Response.StatusCode.Should().Be(401);
    }

    /// <summary>
    /// Tests that middleware with api_key query parameter case-sensitive comparison.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenQueryApiKeyCaseSensitive_FailsOnCaseMismatch()
    {
        // Arrange
        var config = new BotConfiguration { ApiKey = "TestKey123" };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            ["api_key"] = "testkey123"
        });
        context.Request.Path = "/api/test";

        // Act
        await middleware.InvokeAsync(context, config);

        // Assert
        context.Response.StatusCode.Should().Be(401);
    }
}
