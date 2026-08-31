#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Tests;

/// <summary>
/// Constants used in ValidationUtilityTests to avoid magic values.
/// </summary>
internal static class ValidationUtilityTestsConstants
{
    public const string EmptyValue = "";
    public const string WhitespaceValue = "   ";

    // Telegram identifiers
    public const long MinimumPositiveId = 1L;
    public const long PositiveUserId = 100L;
    public const long LargeUserId = 999999999L;
    public const long BillionUserId = 1000000000L;
    public const long ZeroId = 0L;
    public const long NegativeId = -1L;
    public const long NegativeUserId = -100L;
    public const long LargeNegativeId = -999999999L;
    public const long PositiveChatId = 12345L;
    public const long NegativeChatId = -100500L;

    // URLs
    public const string SecureExampleUrl = "https://example.com";
    public const string ExampleUrl = "http://example.com";
    public const string UrlWithPathQueryAndFragment = "https://sub.example.com/path?query=value#fragment";
    public const string LocalhostUrlWithPort = "http://localhost:5000";
    public const string IpUrlWithPort = "https://192.168.1.1:8080";
    public const string FtpUrl = "ftp://files.example.com";
    public const string InvalidUrl = "not-a-url";
    public const string UrlWithoutScheme = "example.com";
    public const string IncompleteHttpUrl = "http://";
    public const string IncompleteHttpsUrl = "https://";

    // Phone number lengths
    public const string TenDigitPhoneNumber = "1234567890";
    public const string NineDigitPhoneNumber = "123456789";

    // Repeated validation inputs
    public const string DuplicateInvalidPassword = "NoSpecial1";
    public const string Hello = "hello";
    public const string Hi = "hi";
    public const string SingleCharacter = "a";

    // Length validation boundaries
    public const int ZeroLength = 0;
    public const int MinimumLength = 1;
    public const int ShortMinimumLength = 3;
    public const int ShortMaximumLength = 5;
    public const int StandardMaximumLength = 10;
}
