#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Middleware;

/// <summary>
/// Middleware for structured logging of HTTP requests and responses.
/// Logs request/response metadata including duration, status codes, and user context.
/// </summary>
public sealed class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        var correlationId = GetOrCreateCorrelationId(context);
        var originalBodyStream = context.Response.Body;

        try
        {
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            LogRequestStart(context, correlationId);

            await _next(context);

            LogRequestComplete(context, correlationId, startTime);

            await memoryStream.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            LogException(ex, correlationId);
            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        const string headerName = "X-Correlation-ID";

        if (context.Request.Headers.TryGetValue(headerName, out var correlationId))
        {
            return correlationId.ToString();
        }

        var newCorrelationId = Guid.NewGuid().ToString();
        context.Response.Headers[headerName] = newCorrelationId;
        return newCorrelationId;
    }

    private void LogRequestStart(HttpContext context, string correlationId)
    {
        var request = context.Request;
        _logger.LogInformation(
            "HTTP Request started - CorrelationID: {CorrelationId}, Method: {Method}, Path: {Path}, IP: {IP}",
            correlationId,
            request.Method,
            request.Path,
            context.Connection.RemoteIpAddress
        );
    }

    private void LogRequestComplete(HttpContext context, string correlationId, DateTime startTime)
    {
        var elapsed = DateTime.UtcNow - startTime;
        var response = context.Response;

        // Log based on status code severity
        var logLevel = response.StatusCode >= 500 ? LogLevel.Error :
                       response.StatusCode >= 400 ? LogLevel.Warning :
                       LogLevel.Information;

        _logger.Log(
            logLevel,
            "HTTP Request completed - CorrelationID: {CorrelationId}, StatusCode: {StatusCode}, " +
            "Duration: {DurationMs}ms, ContentType: {ContentType}",
            correlationId,
            response.StatusCode,
            elapsed.TotalMilliseconds,
            response.ContentType
        );
    }

    private void LogException(Exception ex, string correlationId)
    {
        _logger.LogError(
            ex,
            "HTTP Request failed - CorrelationID: {CorrelationId}, Exception: {ExceptionType}",
            correlationId,
            ex.GetType().Name
        );
    }
}