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
/// <summary>
/// 
/// </summary>
public sealed class HttpErrorHandlingMiddleware : IHttpErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HttpErrorHandlingMiddleware> _logger;

    /// <summary>
    /// 
    /// </summary>
    public HttpErrorHandlingMiddleware(RequestDelegate next, ILogger<HttpErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// 
    /// </summary>
    public string ErrorCode { get; set; } = string.Empty;
    /// <summary>
    /// 
    /// </summary>
    public string Message { get; set; } = string.Empty;
    /// <summary>
    /// 
    /// </summary>
    public DateTime Timestamp { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string Path { get; set; } = string.Empty;
    /// <summary>
    /// 
    /// </summary>
    public string TraceId { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation("Starting to process request {Path} with TraceId {TraceId}", context.Request.Path, context.TraceIdentifier);

        try
        {
            await _next(context);
            _logger.LogInformation("Finished processing request {Path} with TraceId {TraceId}", context.Request.Path, context.TraceIdentifier);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning("Request {Path} with TraceId {TraceId} is falling back to the error response path", context.Request.Path, context.TraceIdentifier);
            _logger.LogError(ex, "Failed to process request {Path} with TraceId {TraceId}", context.Request.Path, context.TraceIdentifier);
            await HandleExceptionAsync(context, ex);
            _logger.LogInformation("Finished processing failed request {Path} with TraceId {TraceId} and StatusCode {StatusCode}", context.Request.Path, context.TraceIdentifier, context.Response.StatusCode);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogInformation("Handling exception for request {Path} with TraceId {TraceId}", context.Request.Path, context.TraceIdentifier);
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

        _logger.LogInformation("Returning error response {StatusCode} with ErrorCode {ErrorCode} for request {Path}", statusCode, errorCode, context.Request.Path);
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
/// <summary>
/// 
/// </summary>
public sealed class HttpErrorResponse
{
    /// <summary>
    /// 
    /// </summary>
    public string ErrorCode { get; set; } = string.Empty;
    /// <summary>
    /// 
    /// </summary>
    public string Message { get; set; } = string.Empty;
    /// <summary>
    /// 
    /// </summary>
    public DateTime Timestamp { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string Path { get; set; } = string.Empty;
    /// <summary>
    /// 
    /// </summary>
    public string TraceId { get; set; } = string.Empty;
}