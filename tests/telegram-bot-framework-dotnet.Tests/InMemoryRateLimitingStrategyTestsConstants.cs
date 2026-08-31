namespace TelegramBotFramework.Strategies.Tests;

/// <summary>
/// Constants for InMemoryRateLimitingStrategyTests.
/// </summary>
internal static class InMemoryRateLimitingStrategyTestsConstants
{
    public const string TestUserIdentifier = "test-user";
    public const string AsyncTestKey = "test-key";
    public const string FirstUserIdentifier = "user1";
    public const string SecondUserIdentifier = "user2";

    /// <summary>
    /// Default request limit for rate limiting strategy tests.
    /// </summary>
    public const int DefaultRequestLimit = 30;

    /// <summary>
    /// Async test request limit for rate limiting strategy tests.
    /// </summary>
    public const int AsyncTestRequestLimit = 10;

    /// <summary>
    /// Default interval in seconds for rate limiting strategy tests.
    /// </summary>
    public const int DefaultIntervalInSeconds = 60;

    /// <summary>
    /// Async test interval in seconds for rate limiting strategy tests.
    /// </summary>
    public const int AsyncTestIntervalInSeconds = 30;

    /// <summary>
    /// Just inside window in seconds for rate limiting strategy tests.
    /// </summary>
    public const int JustInsideWindowInSeconds = 59;

    /// <summary>
    /// Just outside window in seconds for rate limiting strategy tests.
    /// </summary>
    public const int JustOutsideWindowInSeconds = 61;

    public static readonly TimeSpan DefaultInterval =
        TimeSpan.FromSeconds(DefaultIntervalInSeconds);

    public static readonly TimeSpan AsyncTestInterval =
        TimeSpan.FromSeconds(AsyncTestIntervalInSeconds);

    public static readonly TimeSpan JustInsideWindow =
        TimeSpan.FromSeconds(JustInsideWindowInSeconds);

    public static readonly TimeSpan JustOutsideWindow =
        TimeSpan.FromSeconds(JustOutsideWindowInSeconds);
}
