#nullable enable

namespace TelegramBotFramework.Middleware;

/// <summary>
/// Constants for <see cref="HttpErrorHandlingMiddlewareValidation"/>.
/// </summary>
internal static class HttpErrorHandlingMiddlewareValidationConstants
{
    public const string ErrorCodeMustNotBeNullOrEmpty = "ErrorCode must not be null or empty.";
    public const string MessageMustNotBeNullOrEmpty = "Message must not be null or empty.";
    public const string TimestampMustBeSetToNonDefaultValue = "Timestamp must be set to a non-default value.";
    public const string PathMustNotBeNullOrEmpty = "Path must not be null or empty.";
    public const string TraceIdMustNotBeNullOrEmpty = "TraceId must not be null or empty.";
    public const string HttpErrorHandlingMiddlewareIsInvalidProblems = "HttpErrorHandlingMiddleware is invalid. Problems: ";
}