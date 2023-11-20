// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Middleware;

using System.Text;

/// <summary>
/// Middleware that validates incoming request bodies against expected schemas.
/// Provides early validation before reaching controllers, improving error handling.
/// </summary>
public class RequestValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestValidationMiddleware> _logger;

    public RequestValidationMiddleware(RequestDelegate next, ILogger<RequestValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only validate POST, PUT, PATCH requests with body content
        if (context.Request.Method is not ("POST" or "PUT" or "PATCH"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.ContentLength.HasValue || context.Request.ContentLength == 0)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Request body is required");
            return;
        }

        var contentType = context.Request.ContentType?.ToLower() ?? string.Empty;

        // Validate content-type header
        if (!contentType.Contains("application/json") && !contentType.Contains("application/x-www-form-urlencoded"))
        {
            context.Response.StatusCode = 415; // Unsupported Media Type
            await context.Response.WriteAsync("Content-Type must be application/json or application/x-www-form-urlencoded");
            return;
        }

        // Validate content length doesn't exceed maximum (5 MB default)
        const long maxContentLength = 5 * 1024 * 1024;
        if (context.Request.ContentLength > maxContentLength)
        {
            context.Response.StatusCode = 413; // Payload Too Large
            await context.Response.WriteAsync("Request body exceeds maximum allowed size");
            return;
        }

        // Enable request body buffering for potential re-reads
        context.Request.EnableBuffering();

        // Read and validate body format
        var bodyContent = await ReadBodyAsync(context.Request);

        if (!string.IsNullOrWhiteSpace(bodyContent) && contentType.Contains("application/json"))
        {
            if (!IsValidJson(bodyContent))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid JSON in request body");
                return;
            }
        }

        // Reset stream position for controller to read
        context.Request.Body.Position = 0;

        _logger.LogDebug("Request validation passed for {Method} {Path}", context.Request.Method, context.Request.Path);

        await _next(context);
    }

    private static async Task<string> ReadBodyAsync(HttpRequest request)
    {
        request.Body.Position = 0;
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }

    private static bool IsValidJson(string content)
    {
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(content);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
