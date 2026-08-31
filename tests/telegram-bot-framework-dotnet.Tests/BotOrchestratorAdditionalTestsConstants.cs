#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using TelegramBotFramework.Tests;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Contains constant values used in BotOrchestratorAdditionalTests.
/// </summary>
internal static class BotOrchestratorAdditionalTestsConstants
{
    /// <summary>
    /// Test user ID.
    /// </>
    public const long TestUserId = 123;

    /// <summary>
    /// Test chat ID.
    /// </summary>
    public const long TestChatId = 456;

    /// <summary>
    /// Test message ID.
    /// </summary>
    public const int TestMessageId = 1;

    /// <summary>
    /// Test session ID.
    /// </summary>
    public const string TestSessionId = "session-123";

    /// <summary>
    /// Test first name.
    /// </summary>
    public const string TestFirstName = "John";

    /// <summary>
    /// Test last name.
    /// </summary>
    public const string TestLastName = "Doe";

    /// <summary>
    /// Test bot token.
    /// </summary>
    public const string TestBotToken = "test-token";

    /// <summary>
    /// Test bot username.
    /// </summary>
    public const string TestBotUsername = "TestBot";

    /// <summary>
    /// Test menu ID.
    /// </summary>
    public const string TestMenuId = "main";

    /// <summary>
    /// Maximum message length for testing.
    /// </summary>
    public const int MaxMessageLength = 4000;

    /// <summary>
    /// Test command name.
    /// </summary>
    public const string TestCommandName = "test";

    /// <summary>
    /// Non-existent command name.
    /// </summary>
    public const string NonExistentCommandName = "nonexistent";

    /// <summary>
    /// Test parameter key.
    /// </summary>
    public const string TestParamKey = "param1";

    /// <summary>
    /// Test parameter value.
    /// </summary>
    public const string TestParamValue = "value1";

    /// <summary>
    /// Test parameter key 2.
    /// </summary>
    public const string TestParamKey2 = "param2";

    /// <summary>
    /// Test parameter value 2.
    /// </summary>
    public const int TestParamValue2 = 123;

    /// <summary>
    /// Test URL for menu button.
    /// </summary>
    public const string TestUrl = "https://example.com";

    /// <summary>
    /// Test inline query for menu button.
    /// </summary>
    public const string TestInlineQuery = "inline_query";
}