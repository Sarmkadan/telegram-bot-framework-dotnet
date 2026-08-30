#nullable enable
namespace TelegramBotFramework.Middleware;

/// <summary>
/// Constants for AuthenticationMiddleware.
/// </summary>
internal static class AuthenticationMiddlewareConstants
{
    public const string HealthEndpoint = "/health";
    public const string ApiWebhookEndpoint = "/api/webhook";
    public const string SwaggerEndpoint = "/swagger";
    public const string BotUpdateEndpoint = "/api/v1/bot/update";
    public const string BearerScheme = "Bearer ";
    public const string ApiKeyHeader = "X-API-Key";
    public const string ApiKeyQueryParameter = "api_key";
    public const string UnauthorizedMessage = "Unauthorized";
    public const string UnauthorizedLogMessage = "Unauthorized access attempt from {IP} to {Path}";
    public const int UnauthorizedStatusCode = 401;
}