#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Services;

/// <summary>
/// Result of a broadcast operation containing success/failure information.
/// </summary>
public interface IBroadcastResult
{
    /// <summary>
    /// Total number of chats in the broadcast.
    /// </summary>
    int TotalChats { get; }

    /// <summary>
    /// Number of chats successfully sent.
    /// </summary>
    int SuccessCount { get; }

    /// <summary>
    /// Number of chats that failed.
    /// </summary>
    int FailedCount { get; }

    /// <summary>
    /// List of successfully sent chat IDs.
    /// </summary>
    IReadOnlyList<long> SuccessfulChatIds { get; }

    /// <summary>
    /// List of failed chat IDs with their error messages.
    /// </summary>
    IReadOnlyList<FailedChat> Failures { get; }

    /// <summary>
    /// Optional summary message.
    /// </summary>
    string? Summary { get; }
}