namespace TelegramBotFramework.Tests;

internal static class BotFrameworkExceptionTestsConstants
{
    public const string TestMessage = "test message";
    public const string TestErrorCode = "TEST_ERROR";
    public const string InnerExceptionMessage = "inner";
    public const string ExecutionFailedMessage = "execution failed";
    public const string TestCommandName = "test_command";
    public const string CommandExecutionErrorCode = "COMMAND_EXECUTION_ERROR";
    public const string CommandNotFoundErrorCode = "COMMAND_NOT_FOUND";
    public const long PermissionTestUserId = 123L;
    public const string AdminPermission = "admin";
    public const string TestSessionId = "session_abc";
    public const string SessionFailedMessage = "session failed";
    public const string InsufficientPermissionErrorCode = "INSUFFICIENT_PERMISSION";
    public const string SessionErrorCode = "SESSION_ERROR";
    public const long UserExceptionTestUserId = 456L;
    public const int RetryAfterSeconds = 30;
    public const string UserFailedMessage = "user failed";
    public const string UserErrorCode = "USER_ERROR";
    public const string RateLimitExceededErrorCode = "RATE_LIMIT_EXCEEDED";
    public const string ConfigurationErrorMessage = "config error";
    public const long TestUpdateId = 789L;
    public const string ConfigurationErrorCode = "CONFIGURATION_ERROR";
    public const string DuplicateUpdateErrorCode = "DUPLICATE_UPDATE";
}
