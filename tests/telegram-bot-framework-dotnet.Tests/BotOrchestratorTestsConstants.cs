namespace TelegramBotFramework.Tests;

/// <summary>
/// Constants used in BotOrchestrator tests to avoid magic values.
/// </summary>
internal static class BotOrchestratorTestsConstants
{
    // Test user data
    public const string TestFirstName = "John";
    public const string TestLastName = "Doe";
    public const long TestUserId = 123;
    public const long TestChatId = 456;
    public const int TestMessageId = 1;

    // Test strings
    public const string TestBotToken = "test-token";
    public const string TestBotUsername = "TestBot";
    public const string TestSessionId = "session-123";
    public const string TestGreeting = "Hello";
    public const string TestStartCommand = "/start";
    public const string TestTestCommand = "/test";
    public const string TestNonexistentCommand = "nonexistent";
    public const string TestNonexistentMenu = "nonexistent";
    public const string TestMainMenu = "main";
    public const string TestSubmenu = "submenu";
    public const string TestUnknownAction = "unknown";
    public const string TestCommandName = "start";
    public const string EmptyString = "";
    public const string ValidationFailedMessage = "Validation failed";
    public const string CommandNotFoundFormat = "Command '{0}' not found";
    public const string TestCommandWithParams = "/start param1 param2";
    public const string TestCommandOnly = "/start";
    public const string TestNoSlashCommand = "start";

    // Test numbers
    public const int UnknownButtonActionValue = 999;
}