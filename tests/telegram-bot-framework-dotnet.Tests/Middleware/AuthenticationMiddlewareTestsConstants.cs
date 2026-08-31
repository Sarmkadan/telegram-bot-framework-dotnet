#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// Constants for AuthenticationMiddlewareTests
// =============================================================================

namespace TelegramBotFramework.Middleware.Tests;

/// <summary>
/// Constants used in AuthenticationMiddlewareTests to avoid magic values.
/// </summary>
internal static class AuthenticationMiddlewareTestsConstants
{
    // Test API keys
    public const string ValidApiKey = "test-secret-key-123";
    public const string AlternativeValidApiKey = "test-secret-key";
    public const string CorrectApiKey = "correct-secret-key";
    public const string InvalidApiKey = "wrong-secret-key";
    public const string TestKey = "test-key";
    public const string WhitespaceApiKey = "   ";
    public const string CaseSensitiveKey = "TestKey123";
    public const string CaseMismatchKey = "testkey123";

    // HTTP headers and query parameters
    public const string AuthorizationHeader = "Authorization";
    public const string XApiKeyHeader = "X-API-Key";
    public const string ApiKeyQueryParam = "api_key";
    public const string BearerPrefix = "Bearer ";

    // Test paths
    public const string ApiTestPath = "/api/test";
    public const string HealthPath = "/health";
    public const string WebhookPath = "/api/webhook";
    public const string SwaggerPath = "/swagger/index.html";
    public const string BotUpdatePath = "/api/v1/bot/update";
    public const string HealthPathUppercase = "/HEALTH";

    // Context item keys
    public const string AuthenticatedItemKey = "Authenticated";
    public const string AuthenticatedAtItemKey = "AuthenticatedAt";
    public const string PublicEndpointItemKey = "PublicEndpoint";

    // HTTP status codes
    public const int StatusCodeOk = 200;
    public const int StatusCodeUnauthorized = 401;

    // Test API key for public endpoint tests
    public const string SomeKey = "some-key";
}
