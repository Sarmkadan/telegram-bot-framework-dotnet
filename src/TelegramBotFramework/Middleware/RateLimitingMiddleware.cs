#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Middleware;

using System.Collections.Concurrent;

/// <summary>
/// Rate limiting middleware that prevents abuse by tracking request counts per IP/user.
/// Uses sliding window algorithm to enforce request quotas.
/// </summary>
public sealed class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RateLimitingOptions _options;
    private readonly ConcurrentDictionary<string, RequestWindow> _requestWindows;

    public RateLimitingMiddleware(RequestDelegate next, RateLimitingOptions? options = null)
    {
        _next = next;
        _options = options ?? new RateLimitingOptions();
        _requestWindows = new ConcurrentDictionary<string, RequestWindow>();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_options.Enabled)
        {
            await _next(context);
            return;
        }

        var identifier = GetClientIdentifier(context);
        var window = _requestWindows.AddOrUpdate(identifier, _ => new RequestWindow(),
            (_, existing) =>
            {
                if (DateTime.UtcNow - existing.WindowStart > _options.WindowDuration)
                {
                    return new RequestWindow();
                }
                return existing;
            });

        if (window.RequestCount >= _options.RequestsPerWindow)
        {
            context.Response.StatusCode = 429; // Too Many Requests
            context.Response.Headers["Retry-After"] = ((int)(_options.WindowDuration - (DateTime.UtcNow - window.WindowStart)).TotalSeconds).ToString();
            await context.Response.WriteAsync("Rate limit exceeded");
            return;
        }

        window.RequestCount++;
        context.Items["RateLimitRemaining"] = _options.RequestsPerWindow - window.RequestCount;

        await _next(context);
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        // Prefer X-Forwarded-For for proxied requests
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            return forwardedFor.ToString().Split(',')[0].Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private class RequestWindow
    {
        public DateTime WindowStart { get; } = DateTime.UtcNow;
        public int RequestCount { get; set; }
    }
}

/// <summary>
/// Configuration options for rate limiting behavior.
/// </summary>
public sealed class RateLimitingOptions
{
    public bool Enabled { get; set; } = true;
    public int RequestsPerWindow { get; set; } = 100;
    public TimeSpan WindowDuration { get; set; } = TimeSpan.FromMinutes(1);
}