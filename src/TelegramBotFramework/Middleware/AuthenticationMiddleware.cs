#nullable enable
// Pipeline order: authentication must run before authorization.
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Middleware;

/// <summary>
/// API key authentication middleware that validates requests against stored API keys.
/// Supports per-endpoint authentication configuration and multiple key formats.
/// </summary>
public sealed class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;
    private readonly HashSet<string> _publicEndpoints;

    public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(logger);

        _next = next;
        _logger = logger;
        _publicEndpoints = new HashSet<string>
        {
            AuthenticationMiddlewareConstants.HealthEndpoint,
            AuthenticationMiddlewareConstants.ApiWebhookEndpoint,
            AuthenticationMiddlewareConstants.SwaggerEndpoint,
            AuthenticationMiddlewareConstants.BotUpdateEndpoint
        };
    }

    public async Task InvokeAsync(HttpContext context, Models.BotConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(config);

        var path = context.Request.Path.Value ?? string.Empty;

        // Skip authentication for public endpoints
        if (IsPublicEndpoint(path))
        {
            await _next(context);
            return;
        }

        if (!ValidateApiKey(context, config.ApiKey))
        {
            _logger.LogWarning(AuthenticationMiddlewareConstants.UnauthorizedLogMessage,
                context.Connection.RemoteIpAddress, path);

            context.Response.StatusCode = AuthenticationMiddlewareConstants.UnauthorizedStatusCode;
            await context.Response.WriteAsync(AuthenticationMiddlewareConstants.UnauthorizedMessage);
            return;
        }

        context.Items["AuthenticatedAt"] = DateTime.UtcNow;
        await _next(context);
    }

    private bool ValidateApiKey(HttpContext context, string? configuredKey)
    {
        if (string.IsNullOrEmpty(configuredKey))
            return false;

        // Check Authorization header (Bearer scheme)
        var authHeader = context.Request.Headers.Authorization.ToString();
        if (authHeader.StartsWith(AuthenticationMiddlewareConstants.BearerScheme, StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader[AuthenticationMiddlewareConstants.BearerScheme.Length..];
            return token.Equals(configuredKey, StringComparison.Ordinal);
        }

        // Check X-API-Key header
        if (context.Request.Headers.TryGetValue(AuthenticationMiddlewareConstants.ApiKeyHeader, out var apiKey))
        {
            return apiKey.ToString().Equals(configuredKey, StringComparison.Ordinal);
        }

        // Check query parameter (less secure, only for specific endpoints)
        if (context.Request.Query.TryGetValue(AuthenticationMiddlewareConstants.ApiKeyQueryParameter, out var queryKey))
        {
            return queryKey.ToString().Equals(configuredKey, StringComparison.Ordinal);
        }

        return false;
    }

    private bool IsPublicEndpoint(string path)
    {
        return _publicEndpoints.Any(endpoint =>
            path.StartsWith(endpoint, StringComparison.OrdinalIgnoreCase));
    }
}
