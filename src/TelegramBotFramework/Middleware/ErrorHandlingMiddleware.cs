#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using TelegramBotFramework.Constants;

namespace TelegramBotFramework.Middleware;

/// <summary>
/// Global error handling middleware that catches all unhandled exceptions
/// and returns consistent error responses to clients.
/// </summary>
public sealed class HttpErrorHandlingMiddleware : IHttpErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HttpErrorHandlingMiddleware> _logger;

    public HttpErrorHandlingMiddleware(RequestDelegate next, ILogger<HttpErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public string ErrorCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Path { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, HttpErrorHandlingMiddlewareConstants.UnhandledExceptionLogMessage);

        context.Response.ContentType = ApiConstants.ContentTypeJson;

        var (statusCode, errorCode, message) = MapException(exception);
        context.Response.StatusCode = statusCode;

        var response = new HttpErrorResponse
        {
            ErrorCode = errorCode,
            Message = message,
            Timestamp = DateTime.UtcNow,
            Path = context.Request.Path,
            TraceId = context.TraceIdentifier
        };

        return context.Response.WriteAsJsonAsync(response);
    }

    // Maps exceptions to appropriate HTTP status codes and error messages
    private static (int StatusCode, string ErrorCode, string Message) MapException(Exception ex)
    {
        return ex switch
        {
            ArgumentNullException => (HttpErrorHandlingMiddlewareConstants.BadRequestStatusCode, HttpErrorConstants.InvalidArgumentErrorCode, HttpErrorConstants.NullArgumentMessage),
            ArgumentException => (HttpErrorHandlingMiddlewareConstants.BadRequestStatusCode, HttpErrorConstants.InvalidArgumentErrorCode, ex.Message),
            InvalidOperationException => (HttpErrorHandlingMiddlewareConstants.ConflictStatusCode, HttpErrorConstants.InvalidStateErrorCode, ex.Message),
            TimeoutException => (HttpErrorHandlingMiddlewareConstants.RequestTimeoutStatusCode, HttpErrorConstants.RequestTimeoutErrorCode, HttpErrorConstants.RequestTimeoutMessage),
            NotImplementedException => (HttpErrorHandlingMiddlewareConstants.NotImplementedStatusCode, HttpErrorConstants.NotImplementedErrorCode, HttpErrorConstants.NotImplementedMessage),
            Exceptions.BotFrameworkException bfe => (HttpErrorHandlingMiddlewareConstants.InternalServerErrorStatusCode, bfe.ErrorCode ?? HttpErrorConstants.BotFrameworkErrorCode, bfe.Message),
            _ => (HttpErrorHandlingMiddlewareConstants.InternalServerErrorStatusCode, HttpErrorConstants.InternalErrorCode, HttpErrorConstants.InternalErrorMessage)
        };
    }
}

/// <summary>
/// Standard error response structure for API clients.
/// </summary>
public sealed class HttpErrorResponse
{
    public string ErrorCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Path { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
}
