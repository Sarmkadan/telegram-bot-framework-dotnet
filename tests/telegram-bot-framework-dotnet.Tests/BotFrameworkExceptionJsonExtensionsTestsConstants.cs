namespace TelegramBotFramework.Tests;

internal static class BotFrameworkExceptionJsonExtensionsTestsConstants
{
    public const string TestErrorMessage = "Test error message";
    public const string DeserializedTestErrorMessage = "Test error";
    public const string TestErrorCode = "TEST_ERROR";
    public const string Newline = "\n";
    public const string MalformedJson = "{ invalid json";
    public const string SessionFailedMessage = "Session failed";
    public const string SessionErrorCode = "SESSION_ERROR";
    public const string SessionErrorJsonProperty = "sessionError";
    public const string CommandFailedMessage = "Command failed";
    public const string TestCommand = "test-command";
    public const string TestErrorCode001 = "TEST_ERROR_001";
    public const string CamelCaseTestMessage = "Camel case test";
    public const string CamelCaseTestErrorCode = "CAMEL_TEST";
    public const string ValidExceptionMessage = "Valid exception";
    public const string ValidErrorCode = "VALID_001";
    public const string InvalidStructureJson = "{\"invalid\":\"structure\"}";
    public const string CamelCaseJson = "{\"message\":\"Camel case test\",\"errorCode\":\"CAMEL_TEST\"}";
    public const string CommandExecutionErrorJsonProperty = "commandExecutionError";
    public const string EmptyJson = "";
    public const string WhitespaceJson = "   ";
}
