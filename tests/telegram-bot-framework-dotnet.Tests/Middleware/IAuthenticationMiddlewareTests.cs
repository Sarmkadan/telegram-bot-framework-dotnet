#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// Interface for AuthenticationMiddlewareTests
// =============================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TelegramBotFramework.Models;

namespace TelegramBotFramework.Middleware.Tests;

/// <summary>
/// Interface for the AuthenticationMiddlewareTests class.
/// </summary>
public interface IAuthenticationMiddlewareTests
{
    Task InvokeAsync_WhenBearerTokenValid_PassesAuthentication();
    Task InvokeAsync_WhenXApiKeyValid_PassesAuthentication();
    Task InvokeAsync_WhenQueryApiKeyValid_PassesAuthentication();
    Task InvokeAsync_WhenBearerTokenInvalid_Returns401();
    Task InvokeAsync_WhenAuthorizationHeaderMissing_Returns401();
    Task InvokeAsync_WhenApiKeyNull_Returns401();
    Task InvokeAsync_WhenApiKeyEmpty_Returns401();
    Task InvokeAsync_WhenApiKeyWhitespace_Returns401();
    Task InvokeAsync_WhenPathIsNull_DoesNotThrow();
    Task InvokeAsync_WhenAuthorizationHeaderMissing_DoesNotThrow();
    Task InvokeAsync_WhenPublicEndpoint_DoesNotRequireAuthentication();
    Task InvokeAsync_WhenWebhookEndpoint_PublicEndpoint();
    Task InvokeAsync_WhenSwaggerEndpoint_PublicEndpoint();
    Task InvokeAsync_WhenBotUpdateEndpoint_PublicEndpoint();
    Task InvokeAsync_WhenPublicEndpointCaseInsensitive_PassesWithoutAuth();
    Task InvokeAsync_WhenBearerTokenCaseSensitive_FailsOnCaseMismatch();
    Task InvokeAsync_WhenXApiKeyCaseSensitive_FailsOnCaseMismatch();
    Task InvokeAsync_WhenQueryApiKeyCaseSensitive_FailsOnCaseMismatch();
}