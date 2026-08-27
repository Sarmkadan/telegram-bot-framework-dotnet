#nullable enable

namespace TelegramBotFramework.Services;

/// <summary>
/// Statistics about rate limiting and broadcast performance.
/// </summary>
public interface IRateLimitStats
{
    /// <summary>
    /// Configured messages per second limit.
    /// </summary>
    int MessagesPerSecond { get; }

    /// <summary>
    /// Configured maximum concurrency.
    /// </summary>
    int MaxConcurrency { get; }

    /// <summary>
    /// Total messages sent so far.
    /// </summary>
    long TotalMessagesSent { get; }

    /// <summary>
    /// Total messages failed.
    /// </summary>
    long TotalMessagesFailed { get; }

    /// <summary>
    /// Average messages per second over the last minute.
    /// </summary>
    double AverageMessagesPerSecond { get; }

    /// <summary>
    /// Current concurrency level (number of active sends).
    /// </summary>
    int CurrentConcurrency { get; }

    /// <summary>
    /// Timestamp of when statistics were collected.
    /// </summary>
    DateTime Timestamp { get; }
}