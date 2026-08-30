namespace TelegramBotFramework.Exceptions;

/// <summary>
/// Constants for BotFrameworkException error codes.
/// </summary>
internal static class BotFrameworkExceptionConstants
{
    public const string CommandNotFoundMessageFormat = "Command '{0}' not found";
    public const string InsufficientPermissionMessageFormat = "User {0} does not have required permissions";
    public const string RateLimitExceededMessage = "Rate limit exceeded";

    public const string CommandExecutionError = "COMMAND_EXECUTION_ERROR";
    public const string CommandNotFound = "COMMAND_NOT_FOUND";
    public const string InsufficientPermission = "INSUFFICIENT_PERMISSION";
    public const string SessionError = "SESSION_ERROR";
    public const string UserError = "USER_ERROR";
    public const string RateLimitExceeded = "RATE_LIMIT_EXCEEDED";
    public const string ConfigurationError = "CONFIGURATION_ERROR";
    public const string DuplicateUpdate = "DUPLICATE_UPDATE";
}
