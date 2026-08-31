namespace TelegramBotFramework.Tests;

/// <summary>
/// Constants for CommandServiceTests.
/// </summary>
internal static class CommandServiceTestsConstants
{
    /// <summary>
    /// The test command name with leading slash.
    /// </summary>
    public const string TestCommandNameWithSlash = "/test";

    /// <summary>
    /// The test command name without leading slash.
    /// </summary>
    public const string TestCommandNameWithoutSlash = "test";

    /// <summary>
    /// The unknown command name with leading slash.
    /// </>
    public const string UnknownCommandNameWithSlash = "/unknown";

    /// <summary>
    /// The unknown command name without leading slash.
    /// </summary>
    public const string UnknownCommandNameWithoutSlash = "unknown";

    /// <summary>
    /// The disabled command name.
    /// </summary>
    public const string DisabledCommandName = "/disabled";

    /// <summary>
    /// The admin command name.
    /// </summary>
    public const string AdminCommandName = "/admin";

    /// <summary>
    /// The invalid command name (missing leading slash).
    /// </summary>
    public const string InvalidCommandName = "invalid";

    /// <summary>
    /// The default user ID used in tests.
    /// </summary>
    public const long DefaultUserId = 1;

    /// <summary>
    /// The alternative user ID used in tests.
    /// </summary>
    public const long AlternativeUserId = 2;

    /// <summary>
    /// The default chat ID used in tests.
    /// </summary>
    public const long DefaultChatId = 1;
}