#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace TelegramBotFramework.Services;

/// <summary>
/// Represents progress information for a broadcast operation.
/// </summary>
public sealed class BroadcastProgress : IBroadcastProgress
{
    /// <summary>
    /// Total number of chats in the broadcast.
    /// </summary>
    public int TotalChats { get; }

    /// <summary>
    /// Number of chats successfully processed so far.
    /// </summary>
    public int ProcessedCount { get; }

    /// <summary>
    /// Number of chats successfully sent.
    /// </summary>
    public int SuccessCount { get; }

    /// <summary>
    /// Number of chats that failed.
    /// </summary>
    public int FailedCount { get; }

    /// <summary>
    /// Current progress percentage (0-100).
    /// </summary>
    public int ProgressPercentage => TotalChats > 0 ? (int)((double)ProcessedCount / TotalChats * 100) : 0;

    /// <summary>
    /// List of failed chat IDs with their error messages.
    /// </summary>
    public IReadOnlyList<FailedChat> Failures { get; }

    /// <summary>
    /// Whether the operation is complete.
    /// </summary>
    public bool IsComplete => ProcessedCount >= TotalChats;

    /// <summary>
    /// Elapsed time since the broadcast started.
    /// </summary>
    public TimeSpan ElapsedTime { get; }

    /// <summary>
    /// Estimated time remaining.
    /// </summary>
    public TimeSpan? EstimatedTimeRemaining { get; }

    /// <summary>
    /// Current messages per second rate.
    /// </summary>
    public double CurrentMessagesPerSecond { get; }

    public BroadcastProgress(
        int totalChats,
        int processedCount,
        int successCount,
        int failedCount,
        IReadOnlyList<FailedChat> failures,
        TimeSpan elapsedTime,
        TimeSpan? estimatedTimeRemaining,
        double currentMessagesPerSecond)
    {
        TotalChats = totalChats;
        ProcessedCount = processedCount;
        SuccessCount = successCount;
        FailedCount = failedCount;
        Failures = failures;
        ElapsedTime = elapsedTime;
        EstimatedTimeRemaining = estimatedTimeRemaining;
        CurrentMessagesPerSecond = currentMessagesPerSecond;
    }
}

/// <summary>
/// Represents a failed chat with error information.
/// </summary>
public sealed class FailedChat
{
    /// <summary>
    /// The chat ID that failed.
    /// </summary>
    public long ChatId { get; }

    /// <summary>
    /// Error message describing the failure.
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// Number of retry attempts made.
    /// </summary>
    public int RetryAttempts { get; }

    public FailedChat(long chatId, string errorMessage, int retryAttempts)
    {
        ChatId = chatId;
        ErrorMessage = errorMessage;
        RetryAttempts = retryAttempts;
    }
}
