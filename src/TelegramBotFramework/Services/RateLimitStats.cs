#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace TelegramBotFramework.Services;

/// <summary>
/// Statistics about rate limiting and broadcast performance.
/// </summary>
public sealed class RateLimitStats : IRateLimitStats
{
    /// <summary>
    /// Configured messages per second limit.
    /// </summary>
    public int MessagesPerSecond { get; }

    /// <summary>
    /// Configured maximum concurrency.
    /// </summary>
    public int MaxConcurrency { get; }

    /// <summary>
    /// Total messages sent so far.
    /// </summary>
    public long TotalMessagesSent { get; }

    /// <summary>
    /// Total messages failed.
    /// </summary>
    public long TotalMessagesFailed { get; }

    /// <summary>
    /// Average messages per second over the last minute.
    /// </summary>
    public double AverageMessagesPerSecond { get; }

    /// <summary>
    /// Current concurrency level (number of active sends).
    /// </summary>
    public int CurrentConcurrency { get; }

    /// <summary>
    /// Timestamp of when statistics were collected.
    /// </summary>
    public DateTime Timestamp { get; }

    public RateLimitStats(
        int messagesPerSecond,
        int maxConcurrency,
        long totalMessagesSent,
        long totalMessagesFailed,
        double averageMessagesPerSecond,
        int currentConcurrency)
    {
        MessagesPerSecond = messagesPerSecond;
        MaxConcurrency = maxConcurrency;
        TotalMessagesSent = totalMessagesSent;
        TotalMessagesFailed = totalMessagesFailed;
        AverageMessagesPerSecond = averageMessagesPerSecond;
        CurrentConcurrency = currentConcurrency;
        Timestamp = DateTime.UtcNow;
    }
}
