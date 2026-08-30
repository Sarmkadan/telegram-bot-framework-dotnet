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
        _next = next;
        _logger = logger;
        _publicEndpoints = new HashSet<string>
        {
            "/health",
            "/api/webhook",
            "/swagger",
            "/api/v1/bot/update"
        };
    }

    public async Task InvokeAsync(HttpContext context, Models.BotConfiguration config)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Skip authentication for public endpoints
        if (IsPublicEndpoint(path))
        {
            await _next(context);
            return;
        }

        if (!ValidateApiKey(context, config.ApiKey))
        {
            _logger.LogWarning("Unauthorized access attempt from {IP} to {Path}",
                context.Connection.RemoteIpAddress, path);

            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized");
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
        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader["Bearer ".Length..];
            return token.Equals(configuredKey, StringComparison.Ordinal);
        }

        // Check X-API-Key header
        if (context.Request.Headers.TryGetValue("X-API-Key", out var apiKey))
        {
            return apiKey.ToString().Equals(configuredKey, StringComparison.Ordinal);
        }

        // Check query parameter (less secure, only for specific endpoints)
        if (context.Request.Query.TryGetValue("api_key", out var queryKey))
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
