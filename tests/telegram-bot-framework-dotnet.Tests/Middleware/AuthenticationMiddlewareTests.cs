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
        _loggerMock.Object.LogInformation("Starting test: InvokeAsync_WhenBearerTokenValid_PassesAuthentication");
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

        _loggerMock.Object.LogInformation("Test arranged with valid Bearer token: {Token}", AuthenticationMiddlewareTestsConstants.ValidApiKey);

        // Act
        try
        {
            await middleware.InvokeAsync(context, config);
            _loggerMock.Object.LogInformation("Middleware invocation completed successfully");
        }
        catch (Exception ex)
        {
            _loggerMock.Object.LogError(ex, "Error occurred during middleware invocation");
            throw;
        }

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeOk);
        context.Items.Should().ContainKey(AuthenticationMiddlewareTestsConstants.AuthenticatedAtItemKey);
        _loggerMock.Object.LogInformation("Test passed: Valid Bearer token correctly authenticated");
    }

    /// <summary>
    /// Tests that middleware with valid X-API-Key header passes authentication.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenXApiKeyValid_PassesAuthentication()
    {
        // Arrange
        _loggerMock.Object.LogInformation("Starting test: InvokeAsync_WhenXApiKeyValid_PassesAuthentication");
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

        _loggerMock.Object.LogInformation("Test arranged with valid X-API-Key header: {ApiKey}", AuthenticationMiddlewareTestsConstants.ValidApiKey);

        // Act
        try
        {
            await middleware.InvokeAsync(context, config);
            _loggerMock.Object.LogInformation("Middleware invocation completed successfully");
        }
        catch (Exception ex)
        {
            _loggerMock.Object.LogError(ex, "Error occurred during middleware invocation");
            throw;
        }

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeOk);
        context.Items.Should().ContainKey(AuthenticationMiddlewareTestsConstants.AuthenticatedAtItemKey);
        _loggerMock.Object.LogInformation("Test passed: Valid X-API-Key header correctly authenticated");
    }

    /// <summary>
    /// Tests that middleware with valid api_key query parameter passes authentication.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenQueryApiKeyValid_PassesAuthentication()
    {
        // Arrange
        _loggerMock.Object.LogInformation("Starting test: InvokeAsync_WhenQueryApiKeyValid_PassesAuthentication");
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

        _loggerMock.Object.LogInformation("Test arranged with valid api_key query parameter: {ApiKey}", AuthenticationMiddlewareTestsConstants.ValidApiKey);

        // Act
        try
        {
            await middleware.InvokeAsync(context, config);
            _loggerMock.Object.LogInformation("Middleware invocation completed successfully");
        }
        catch (Exception ex)
        {
            _loggerMock.Object.LogError(ex, "Error occurred during middleware invocation");
            throw;
        }

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeOk);
        context.Items.Should().ContainKey(AuthenticationMiddlewareTestsConstants.AuthenticatedAtItemKey);
        _loggerMock.Object.LogInformation("Test passed: Valid api_key query parameter correctly authenticated");
    }

    /// <summary>
    /// Tests that middleware with invalid Bearer token returns 401.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenBearerTokenInvalid_Returns401()
    {
        // Arrange
        _loggerMock.Object.LogInformation("Starting test: InvokeAsync_WhenBearerTokenInvalid_Returns401");
        var config = new BotConfiguration { ApiKey = AuthenticationMiddlewareTestsConstants.CorrectApiKey };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Headers[AuthenticationMiddlewareTestsConstants.AuthorizationHeader] = AuthenticationMiddlewareTestsConstants.BearerPrefix + AuthenticationMiddlewareTestsConstants.InvalidApiKey;
        context.Request.Path = AuthenticationMiddlewareTestsConstants.ApiTestPath;

        _loggerMock.Object.LogInformation("Test arranged with invalid Bearer token: {InvalidToken}", AuthenticationMiddlewareTestsConstants.InvalidApiKey);

        // Act
        try
        {
            await middleware.InvokeAsync(context, config);
            _loggerMock.Object.LogInformation("Middleware invocation completed");
        }
        catch (Exception ex)
        {
            _loggerMock.Object.LogError(ex, "Error occurred during middleware invocation");
            throw;
        }

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeUnauthorized);
        context.Items.Should().NotContainKey(AuthenticationMiddlewareTestsConstants.AuthenticatedAtItemKey);
        _loggerMock.Object.LogInformation("Test passed: Invalid Bearer token correctly returned 401");
    }

    /// <summary>
    /// Tests that middleware with missing Authorization header returns 401.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenAuthorizationHeaderMissing_Returns401()
    {
        // Arrange
        _loggerMock.Object.LogInformation("Starting test: InvokeAsync_WhenAuthorizationHeaderMissing_Returns401");
        var config = new BotConfiguration { ApiKey = AuthenticationMiddlewareTestsConstants.AlternativeValidApiKey };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Path = AuthenticationMiddlewareTestsConstants.ApiTestPath;

        _loggerMock.Object.LogInformation("Test arranged with missing Authorization header");

        // Act
        try
        {
            await middleware.InvokeAsync(context, config);
            _loggerMock.Object.LogInformation("Middleware invocation completed");
        }
        catch (Exception ex)
        {
            _loggerMock.Object.LogError(ex, "Error occurred during middleware invocation");
            throw;
        }

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeUnauthorized);
        context.Items.Should().NotContainKey(AuthenticationMiddlewareTestsConstants.AuthenticatedAtItemKey);
        _loggerMock.Object.LogInformation("Test passed: Missing Authorization header correctly returned 401");
    }

    /// <summary>
    /// Tests that middleware with null ApiKey in config returns 401.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenApiKeyNull_Returns401()
    {
        // Arrange
        _loggerMock.Object.LogInformation("Starting test: InvokeAsync_WhenApiKeyNull_Returns401");
        var config = new BotConfiguration { ApiKey = null };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Headers[AuthenticationMiddlewareTestsConstants.AuthorizationHeader] = AuthenticationMiddlewareTestsConstants.BearerPrefix + AuthenticationMiddlewareTestsConstants.TestKey;
        context.Request.Path = AuthenticationMiddlewareTestsConstants.ApiTestPath;

        _loggerMock.Object.LogInformation("Test arranged with null ApiKey in config");

        // Act
        try
        {
            await middleware.InvokeAsync(context, config);
            _loggerMock.Object.LogInformation("Middleware invocation completed");
        }
        catch (Exception ex)
        {
            _loggerMock.Object.LogError(ex, "Error occurred during middleware invocation");
            throw;
        }

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeUnauthorized);
        context.Items.Should().NotContainKey(AuthenticationMiddlewareTestsConstants.AuthenticatedAtItemKey);
        _loggerMock.Object.LogInformation("Test passed: Null ApiKey in config correctly returned 401");
    }

    /// <summary>
    /// Tests that middleware with empty ApiKey in config returns 401.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenApiKeyEmpty_Returns401()
    {
        // Arrange
        _loggerMock.Object.LogInformation("Starting test: InvokeAsync_WhenApiKeyEmpty_Returns401");
        var config = new BotConfiguration { ApiKey = string.Empty };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Headers[AuthenticationMiddlewareTestsConstants.AuthorizationHeader] = AuthenticationMiddlewareTestsConstants.BearerPrefix + AuthenticationMiddlewareTestsConstants.TestKey;
        context.Request.Path = AuthenticationMiddlewareTestsConstants.ApiTestPath;

        _loggerMock.Object.LogInformation("Test arranged with empty ApiKey in config");

        // Act
        try
        {
            await middleware.InvokeAsync(context, config);
            _loggerMock.Object.LogInformation("Middleware invocation completed");
        }
        catch (Exception ex)
        {
            _loggerMock.Object.LogError(ex, "Error occurred during middleware invocation");
            throw;
        }

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeUnauthorized);
        context.Items.Should().NotContainKey(AuthenticationMiddlewareTestsConstants.AuthenticatedAtItemKey);
        _loggerMock.Object.LogInformation("Test passed: Empty ApiKey in config correctly returned 401");
    }

    /// <summary>
    /// Tests that middleware with whitespace ApiKey in config returns 401.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenApiKeyWhitespace_Returns401()
    {
        // Arrange
        _loggerMock.Object.LogInformation("Starting test: InvokeAsync_WhenApiKeyWhitespace_Returns401");
        var config = new BotConfiguration { ApiKey = AuthenticationMiddlewareTestsConstants.WhitespaceApiKey };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Headers[AuthenticationMiddlewareTestsConstants.AuthorizationHeader] = AuthenticationMiddlewareTestsConstants.BearerPrefix + AuthenticationMiddlewareTestsConstants.TestKey;
        context.Request.Path = AuthenticationMiddlewareTestsConstants.ApiTestPath;

        _loggerMock.Object.LogInformation("Test arranged with whitespace ApiKey in config: '{WhitespaceKey}'", AuthenticationMiddlewareTestsConstants.WhitespaceApiKey);

        // Act
        try
        {
            await middleware.InvokeAsync(context, config);
            _loggerMock.Object.LogInformation("Middleware invocation completed");
        }
        catch (Exception ex)
        {
            _loggerMock.Object.LogError(ex, "Error occurred during middleware invocation");
            throw;
        }

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeUnauthorized);
        context.Items.Should().NotContainKey(AuthenticationMiddlewareTestsConstants.AuthenticatedAtItemKey);
        _loggerMock.Object.LogInformation("Test passed: Whitespace ApiKey in config correctly returned 401");
    }

    /// <summary>
    /// Tests that middleware with null HttpContext.Request.Path does not throw.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenPathIsNull_DoesNotThrow()
    {
        // Arrange
        _loggerMock.Object.LogInformation("Starting test: InvokeAsync_WhenPathIsNull_DoesNotThrow");
        var config = new BotConfiguration { ApiKey = AuthenticationMiddlewareTestsConstants.AlternativeValidApiKey };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Headers[AuthenticationMiddlewareTestsConstants.AuthorizationHeader] = AuthenticationMiddlewareTestsConstants.BearerPrefix + AuthenticationMiddlewareTestsConstants.TestKey;
        context.Request.Path = null;

        _loggerMock.Object.LogInformation("Test arranged with null Request.Path");

        // Act
        try
        {
            var act = () => middleware.InvokeAsync(context, config);
            await act.Should().NotThrowAsync();
            _loggerMock.Object.LogInformation("Middleware invocation did not throw as expected");
        }
        catch (Exception ex)
        {
            _loggerMock.Object.LogError(ex, "Unexpected error occurred during middleware invocation");
            throw;
        }

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeOk);
        context.Items.Should().ContainKey(AuthenticationMiddlewareTestsConstants.AuthenticatedAtItemKey);
        _loggerMock.Object.LogInformation("Test passed: Null Request.Path handled correctly without throwing");
    }

    /// <summary>
    /// Tests that middleware with missing Authorization header does not throw.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenAuthorizationHeaderMissing_DoesNotThrow()
    {
        // Arrange
        _loggerMock.Object.LogInformation("Starting test: InvokeAsync_WhenAuthorizationHeaderMissing_DoesNotThrow");
        var config = new BotConfiguration { ApiKey = AuthenticationMiddlewareTestsConstants.AlternativeValidApiKey };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Path = AuthenticationMiddlewareTestsConstants.ApiTestPath;

        _loggerMock.Object.LogInformation("Test arranged with missing Authorization header");

        // Act
        try
        {
            var act = () => middleware.InvokeAsync(context, config);
            await act.Should().NotThrowAsync();
            _loggerMock.Object.LogInformation("Middleware invocation did not throw as expected");
        }
        catch (Exception ex)
        {
            _loggerMock.Object.LogError(ex, "Unexpected error occurred during middleware invocation");
            throw;
        }

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeUnauthorized);
        _loggerMock.Object.LogInformation("Test passed: Missing Authorization header handled correctly without throwing");
    }

    /// <summary>
    /// Tests that middleware with public endpoint does not require authentication.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenPublicEndpoint_DoesNotRequireAuthentication()
    {
        // Arrange
        _loggerMock.Object.LogInformation("Starting test: InvokeAsync_WhenPublicEndpoint_DoesNotRequireAuthentication");
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

        _loggerMock.Object.LogInformation("Test arranged with public endpoint path: {Path}", AuthenticationMiddlewareTestsConstants.HealthPath);

        // Act
        try
        {
            await middleware.InvokeAsync(context, config);
            _loggerMock.Object.LogInformation("Middleware invocation completed successfully");
        }
        catch (Exception ex)
        {
            _loggerMock.Object.LogError(ex, "Error occurred during middleware invocation");
            throw;
        }

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeOk);
        context.Items.Should().ContainKey(AuthenticationMiddlewareTestsConstants.PublicEndpointItemKey);
        _loggerMock.Object.LogInformation("Test passed: Public endpoint correctly allowed access without authentication");
    }

    /// <summary>
    /// Tests that middleware with /api/webhook is a public endpoint.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenWebhookEndpoint_PublicEndpoint()
    {
        // Arrange
        _loggerMock.Object.LogInformation("Starting test: InvokeAsync_WhenWebhookEndpoint_PublicEndpoint");
        var config = new BotConfiguration { ApiKey = AuthenticationMiddlewareTestsConstants.SomeKey };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Path = AuthenticationMiddlewareTestsConstants.WebhookPath;

        _loggerMock.Object.LogInformation("Test arranged with webhook endpoint path: {Path}", AuthenticationMiddlewareTestsConstants.WebhookPath);

        // Act
        try
        {
            await middleware.InvokeAsync(context, config);
            _loggerMock.Object.LogInformation("Middleware invocation completed successfully");
        }
        catch (Exception ex)
        {
            _loggerMock.Object.LogError(ex, "Error occurred during middleware invocation");
            throw;
        }

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeOk);
        context.Items.Should().NotContainKey(AuthenticationMiddlewareTestsConstants.AuthenticatedAtItemKey);
        _loggerMock.Object.LogInformation("Test passed: Webhook endpoint correctly allowed access without authentication");
    }

    /// <summary>
    /// Tests that middleware with /swagger is a public endpoint.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenSwaggerEndpoint_PublicEndpoint()
    {
        // Arrange
        _loggerMock.Object.LogInformation("Starting test: InvokeAsync_WhenSwaggerEndpoint_PublicEndpoint");
        var config = new BotConfiguration { ApiKey = AuthenticationMiddlewareTestsConstants.SomeKey };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Path = AuthenticationMiddlewareTestsConstants.SwaggerPath;

        _loggerMock.Object.LogInformation("Test arranged with swagger endpoint path: {Path}", AuthenticationMiddlewareTestsConstants.SwaggerPath);

        // Act
        try
        {
            await middleware.InvokeAsync(context, config);
            _loggerMock.Object.LogInformation("Middleware invocation completed successfully");
        }
        catch (Exception ex)
        {
            _loggerMock.Object.LogError(ex, "Error occurred during middleware invocation");
            throw;
        }

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeOk);
        context.Items.Should().NotContainKey(AuthenticationMiddlewareTestsConstants.AuthenticatedAtItemKey);
        _loggerMock.Object.LogInformation("Test passed: Swagger endpoint correctly allowed access without authentication");
    }

    /// <summary>
    /// Tests that middleware with /api/v1/bot/update is a public endpoint.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenBotUpdateEndpoint_PublicEndpoint()
    {
        // Arrange
        _loggerMock.Object.LogInformation("Starting test: InvokeAsync_WhenBotUpdateEndpoint_PublicEndpoint");
        var config = new BotConfiguration { ApiKey = AuthenticationMiddlewareTestsConstants.SomeKey };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Path = AuthenticationMiddlewareTestsConstants.BotUpdatePath;

        _loggerMock.Object.LogInformation("Test arranged with bot update endpoint path: {Path}", AuthenticationMiddlewareTestsConstants.BotUpdatePath);

        // Act
        try
        {
            await middleware.InvokeAsync(context, config);
            _loggerMock.Object.LogInformation("Middleware invocation completed successfully");
        }
        catch (Exception ex)
        {
            _loggerMock.Object.LogError(ex, "Error occurred during middleware invocation");
            throw;
        }

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeOk);
        context.Items.Should().NotContainKey(AuthenticationMiddlewareTestsConstants.AuthenticatedAtItemKey);
        _loggerMock.Object.LogInformation("Test passed: Bot update endpoint correctly allowed access without authentication");
    }

    /// <summary>
    /// Tests that middleware with case-insensitive public endpoint matching.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenPublicEndpointCaseInsensitive_PassesWithoutAuth()
    {
        // Arrange
        _loggerMock.Object.LogInformation("Starting test: InvokeAsync_WhenPublicEndpointCaseInsensitive_PassesWithoutAuth");
        var config = new BotConfiguration { ApiKey = AuthenticationMiddlewareTestsConstants.SomeKey };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Path = AuthenticationMiddlewareTestsConstants.HealthPathUppercase;

        _loggerMock.Object.LogInformation("Test arranged with uppercase public endpoint path: {Path}", AuthenticationMiddlewareTestsConstants.HealthPathUppercase);

        // Act
        try
        {
            await middleware.InvokeAsync(context, config);
            _loggerMock.Object.LogInformation("Middleware invocation completed successfully");
        }
        catch (Exception ex)
        {
            _loggerMock.Object.LogError(ex, "Error occurred during middleware invocation");
            throw;
        }

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeOk);
        _loggerMock.Object.LogInformation("Test passed: Case-insensitive public endpoint correctly allowed access without authentication");
    }

    /// <summary>
    /// Tests that middleware with Bearer token case-sensitive comparison.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenBearerTokenCaseSensitive_FailsOnCaseMismatch()
    {
        // Arrange
        _loggerMock.Object.LogInformation("Starting test: InvokeAsync_WhenBearerTokenCaseSensitive_FailsOnCaseMismatch");
        var config = new BotConfiguration { ApiKey = AuthenticationMiddlewareTestsConstants.CaseSensitiveKey };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Headers[AuthenticationMiddlewareTestsConstants.AuthorizationHeader] = AuthenticationMiddlewareTestsConstants.BearerPrefix + AuthenticationMiddlewareTestsConstants.CaseMismatchKey; // lowercase
        context.Request.Path = AuthenticationMiddlewareTestsConstants.ApiTestPath;

        _loggerMock.Object.LogInformation("Test arranged with case-sensitive Bearer token comparison: expected={Expected}, actual={Actual}",
            AuthenticationMiddlewareTestsConstants.CaseSensitiveKey, AuthenticationMiddlewareTestsConstants.CaseMismatchKey);

        // Act
        try
        {
            await middleware.InvokeAsync(context, config);
            _loggerMock.Object.LogInformation("Middleware invocation completed");
        }
        catch (Exception ex)
        {
            _loggerMock.Object.LogError(ex, "Error occurred during middleware invocation");
            throw;
        }

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeUnauthorized);
        _loggerMock.Object.LogInformation("Test passed: Case-sensitive Bearer token correctly failed on case mismatch");
    }

    /// <summary>
    /// Tests that middleware with X-API-Key header case-sensitive comparison.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenXApiKeyCaseSensitive_FailsOnCaseMismatch()
    {
        // Arrange
        _loggerMock.Object.LogInformation("Starting test: InvokeAsync_WhenXApiKeyCaseSensitive_FailsOnCaseMismatch");
        var config = new BotConfiguration { ApiKey = AuthenticationMiddlewareTestsConstants.CaseSensitiveKey };
        var middleware = new AuthenticationMiddleware(
            next: async (context) => await Task.CompletedTask,
            logger: _loggerMock.Object
        );

        var context = new DefaultHttpContext();
        context.Request.Headers[AuthenticationMiddlewareTestsConstants.XApiKeyHeader] = AuthenticationMiddlewareTestsConstants.CaseMismatchKey; // lowercase
        context.Request.Path = AuthenticationMiddlewareTestsConstants.ApiTestPath;

        _loggerMock.Object.LogInformation("Test arranged with case-sensitive X-API-Key header comparison: expected={Expected}, actual={Actual}",
            AuthenticationMiddlewareTestsConstants.CaseSensitiveKey, AuthenticationMiddlewareTestsConstants.CaseMismatchKey);

        // Act
        try
        {
            await middleware.InvokeAsync(context, config);
            _loggerMock.Object.LogInformation("Middleware invocation completed");
        }
        catch (Exception ex)
        {
            _loggerMock.Object.LogError(ex, "Error occurred during middleware invocation");
            throw;
        }

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeUnauthorized);
        _loggerMock.Object.LogInformation("Test passed: Case-sensitive X-API-Key header correctly failed on case mismatch");
    }

    /// <summary>
    /// Tests that middleware with api_key query parameter case-sensitive comparison.
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WhenQueryApiKeyCaseSensitive_FailsOnCaseMismatch()
    {
        // Arrange
        _loggerMock.Object.LogInformation("Starting test: InvokeAsync_WhenQueryApiKeyCaseSensitive_FailsOnCaseMismatch");
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

        _loggerMock.Object.LogInformation("Test arranged with case-sensitive api_key query parameter comparison: expected={Expected}, actual={Actual}",
            AuthenticationMiddlewareTestsConstants.CaseSensitiveKey, AuthenticationMiddlewareTestsConstants.CaseMismatchKey);

        // Act
        try
        {
            await middleware.InvokeAsync(context, config);
            _loggerMock.Object.LogInformation("Middleware invocation completed");
        }
        catch (Exception ex)
        {
            _loggerMock.Object.LogError(ex, "Error occurred during middleware invocation");
            throw;
        }

        // Assert
        context.Response.StatusCode.Should().Be(AuthenticationMiddlewareTestsConstants.StatusCodeUnauthorized);
        _loggerMock.Object.LogInformation("Test passed: Case-sensitive api_key query parameter correctly failed on case mismatch");
    }
}
