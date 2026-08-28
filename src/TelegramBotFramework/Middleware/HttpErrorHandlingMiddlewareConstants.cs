#nullable enable

namespace TelegramBotFramework.Middleware;

/// <summary>
/// Contains constant values used in <see cref="HttpErrorHandlingMiddleware"/>.
/// </summary>
internal static class HttpErrorHandlingMiddlewareConstants
{
    public const string UnhandledExceptionLogMessage = "Unhandled exception in request processing";

    public const int BadRequestStatusCode = 400;
    public const int ConflictStatusCode = 409;
    public const int RequestTimeoutStatusCode = 408;
    public const int NotImplementedStatusCode = 501;
    public const int InternalServerErrorStatusCode = 500;
}