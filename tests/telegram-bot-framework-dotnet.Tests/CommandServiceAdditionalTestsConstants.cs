#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using TelegramBotFramework.Models;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Constants for CommandServiceAdditionalTests to avoid magic values.
/// </summary>
internal static class CommandServiceAdditionalTestsConstants
{
    // Command names (without slash prefix)
    public const string AdminCommandName = "admin";
    public const string HelpCommandName = "help";
    public const string DisabledCommandName = "disabled";
    public const string TestCommandName = "test";
    public const string ModeratorCommandName = "moderator";
    public const string NonExistentCommandName = "nonexistent";

    // Command inputs (with slash prefix as used in messages)
    public const string TestCommandInput = "/test";
    public const string NonExistentCommandInput = "/nonexistent";
    public const string DisabledCommandInput = "/disabled";

    // Handler type for test commands
    public const string TestHandlerType = "Test.Handler";

    // Test user and chat identifiers
    public const long TestUserId = 12345;
    public const long TestChatId = -123456789;
    public const string TestUserFirstName = "Test";
    public const int TestMessageId = 1;

    // Expected command counts for different user roles
    public const int AdminAvailableCommandCount = 2;    // admin + help commands
    public const int UserAvailableCommandCount = 1;     // only help command
    public const int ModeratorAvailableCommandCount = 2; // help + moderator commands

    // Execution count values
    public const int FirstExecutionCount = 1;
    public const int InitialExecutionCount = 0;
    public const int ExpectedExecutionCountAfterIncrement = 1;
    public const int ExecutionCountForGetTest = 5;
    public const int MissingCommandExecutionCount = 0;

    // Error messages
    public const string DisabledErrorText = "Command is disabled";
    public const string InsufficientPermissionsErrorText = "Insufficient permissions";

    // Rate limiting
    public const int RateLimitPerMinute = 5;
    public const long FirstRateLimitUserId = 11111;
    public const long SecondRateLimitUserId = 22222;
}