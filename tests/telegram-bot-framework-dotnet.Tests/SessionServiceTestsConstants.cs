#nullable enable

namespace TelegramBotFramework.Tests;

/// <summary>
/// Constants used in SessionServiceTests to avoid magic strings and numbers.
/// </summary>
internal static class SessionServiceTestsConstants
{
    // Test session IDs
    public const string TestSessionId = "session-123";
    public const string NonExistentSessionId = "nonexistent";
    public const string SessionIdPrefix = "session_";

    // Test user and chat IDs
    public const long TestUserId = 123;
    public const long TestChatId = 456;

    // Test menu IDs
    public const string OldMenuId = "old_menu";
    public const string NewMenuId = "new_menu";

    // Test session identifiers for lists
    public const string SessionId1 = "session-1";
    public const string SessionId2 = "session-2";
    public const string SessionId3 = "session-3";

    // Test timeouts
    public static readonly TimeSpan OneHourTimeout = TimeSpan.FromHours(1);
    public static readonly TimeSpan TwentyFourHoursTimeout = TimeSpan.FromHours(24);
    public static readonly TimeSpan OneMinuteTimeout = TimeSpan.FromMinutes(1);
    public static readonly TimeSpan SixtyMinutesTimeout = TimeSpan.FromMinutes(60);

    // Test interaction counts
    public const int InitialInteractionCount = 5;
    public const int UpdatedInteractionCount = 6;
}