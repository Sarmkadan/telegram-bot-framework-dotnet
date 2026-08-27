#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace TelegramBotFramework.Services;

/// <summary>
/// Result of a broadcast operation containing success/failure information.
/// </summary>
public sealed class BroadcastResult : IBroadcastResult
{
    /// <summary>
    /// Total number of chats in the broadcast.
    /// </summary>
    public int TotalChats { get; }

    /// <summary>
    /// Number of chats successfully sent.
    /// </summary>
    public int SuccessCount { get; }

    /// <summary>
    /// Number of chats that failed.
    /// </summary>
    public int FailedCount { get; }

    /// <summary>
    /// Number of chats processed (successful + failed).
    /// </summary>
    public int ProcessedCount => SuccessCount + FailedCount;

    /// <summary>
    /// Whether all messages were sent successfully.
    /// </summary>
    public bool AllSuccessful => FailedCount == 0;

    /// <summary>
    /// List of successfully sent chat IDs.
    /// </summary>
    public IReadOnlyList<long> SuccessfulChatIds { get; }

    /// <summary>
    /// List of failed chat IDs with their error messages.
    /// </summary>
    public IReadOnlyList<FailedChat> Failures { get; }

    /// <summary>
    /// Optional summary message.
    /// </summary>
    public string? Summary { get; }

    public BroadcastResult(
        int totalChats,
        int successCount,
        int failedCount,
        IReadOnlyList<long> successfulChatIds,
        IReadOnlyList<FailedChat> failures,
        string? summary = null)
    {
        TotalChats = totalChats;
        SuccessCount = successCount;
        FailedCount = failedCount;
        SuccessfulChatIds = successfulChatIds;
        Failures = failures;
        Summary = summary;
    }

    /// <summary>
    /// Creates a success result.
    /// </summary>
    public static BroadcastResult Success(
        int totalChats,
        IReadOnlyList<long> successfulChatIds,
        string? summary = null) =>
        new BroadcastResult(
            totalChats,
            successfulChatIds.Count,
            0,
            successfulChatIds,
            Array.Empty<FailedChat>(),
            summary);

    /// <summary>
    /// Creates a failure result.
    /// </summary>
    public static BroadcastResult Failure(
        int totalChats,
        IReadOnlyList<FailedChat> failures,
        string? summary = null) =>
        new BroadcastResult(
            totalChats,
            0,
            failures.Count,
            Array.Empty<long>(),
            failures,
            summary);

    /// <summary>
    /// Creates a mixed result with both successes and failures.
    /// </summary>
    public static BroadcastResult Mixed(
        int totalChats,
        IReadOnlyList<long> successfulChatIds,
        IReadOnlyList<FailedChat> failures,
        string? summary = null) =>
        new BroadcastResult(
            totalChats,
            successfulChatIds.Count,
            failures.Count,
            successfulChatIds,
            failures,
            summary);
}
