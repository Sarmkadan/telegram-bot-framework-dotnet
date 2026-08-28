#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TelegramBotFramework.Middleware;

/// <summary>
/// Middleware for structured logging of HTTP requests and responses.
/// Logs request/response metadata including duration, status codes, and user context.
/// </summary>
public sealed class HttpLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HttpLoggingMiddleware> _logger;

    public HttpLoggingMiddleware(RequestDelegate next, ILogger<HttpLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        var correlationId = GetOrCreateCorrelationId(context);

        // Create a logging scope only when any of the levels we use are enabled.
        // This avoids allocating the dictionary (and any ToString calls) when logging is disabled.
        IDisposable? scope = null;
        if (_logger.IsEnabled(LogLevel.Information) ||
            _logger.IsEnabled(LogLevel.Warning) ||
            _logger.IsEnabled(LogLevel.Error))
        {
            scope = _logger.BeginScope(CreateLoggingScope(context, correlationId));
        }

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
            // Dispose the scope if it was created.
            scope?.Dispose();
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

    private static IDictionary<string, object?> CreateLoggingScope(HttpContext context, string correlationId)
    {
        var scope = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["CorrelationId"] = correlationId
        };

        // Optional Telegram‑specific identifiers – added to the scope if present.
        if (context.Request.Headers.TryGetValue("X-User-ID", out var userId))
        {
            scope["UserId"] = userId.ToString();
        }

        if (context.Request.Headers.TryGetValue("X-Chat-ID", out var chatId))
        {
            scope["ChatId"] = chatId.ToString();
        }

        if (context.Request.Headers.TryGetValue("X-Update-ID", out var updateId))
        {
            scope["UpdateId"] = updateId.ToString();
        }

        return scope;
    }

    private void LogRequestStart(HttpContext context, string correlationId)
    {
        // Guard against unnecessary work when the level is disabled.
        if (!_logger.IsEnabled(LogLevel.Information))
            return;

        var request = context.Request;
        _logger.LogInformation(
            "HTTP Request started - CorrelationID: {CorrelationId}, Method: {Method}, Path: {Path}, IP: {IP}",
            correlationId,
            request.Method,
            request.Path,
            context.Connection.RemoteIpAddress);
    }

    private void LogRequestComplete(HttpContext context, string correlationId, DateTime startTime)
    {
        var elapsed = DateTime.UtcNow - startTime;
        var response = context.Response;

        // Determine log level based on status code.
        var logLevel = response.StatusCode >= 500 ? LogLevel.Error :
                       response.StatusCode >= 400 ? LogLevel.Warning :
                       LogLevel.Information;

        // Guard against logging when the determined level is disabled.
        if (!_logger.IsEnabled(logLevel))
            return;

        _logger.Log(
            logLevel,
            "HTTP Request completed - CorrelationID: {CorrelationId}, StatusCode: {StatusCode}, " +
            "Duration: {DurationMs}ms, ContentType: {ContentType}",
            correlationId,
            response.StatusCode,
            elapsed.TotalMilliseconds,
            response.ContentType);
    }

    private void LogException(Exception ex, string correlationId)
    {
        // Guard against logging when error level is disabled.
        if (!_logger.IsEnabled(LogLevel.Error))
            return;

        _logger.LogError(
            ex,
            "HTTP Request failed - CorrelationID: {CorrelationId}, Exception: {ExceptionType}",
            correlationId,
            ex.GetType().Name);
    }
}
