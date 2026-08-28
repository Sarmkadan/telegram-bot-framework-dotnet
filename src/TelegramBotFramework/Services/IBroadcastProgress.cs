#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace TelegramBotFramework.Services;

/// <summary>
/// Represents progress information for a broadcast operation.
/// </summary>
public interface IBroadcastProgress
{
    /// <summary>
    /// Total number of chats in the broadcast.
    /// </summary>
    int TotalChats { get; }

    /// <summary>
    /// Number of chats successfully processed so far.
    /// </summary>
    int ProcessedCount { get; }

    /// <summary>
    /// Number of chats successfully sent.
    /// </summary>
    int SuccessCount { get; }

    /// <summary>
    /// Number of chats that failed.
    /// </summary>
    int FailedCount { get; }

    /// <summary>
    /// Current progress percentage (0-100).
    /// </summary>
    int ProgressPercentage { get; }

    /// <summary>
    /// Whether the operation is complete.
    /// </summary>
    bool IsComplete { get; }

    /// <summary>
    /// List of failed chat IDs with their error messages.
    /// </summary>
    IReadOnlyList<FailedChat> Failures { get; }

    /// <summary>
    /// Elapsed time since the broadcast started.
    /// </summary>
    TimeSpan ElapsedTime { get; }

    /// <summary>
    /// Estimated time remaining.
    /// </summary>
    TimeSpan? EstimatedTimeRemaining { get; }

    /// <summary>
    /// Current messages per second rate.
    /// </summary>
    double CurrentMessagesPerSecond { get; }
}