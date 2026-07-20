#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace TelegramBotFramework.Services;

/// <summary>
/// Service for broadcasting messages to multiple chat IDs with configurable rate limiting.
/// </summary>
public interface IBroadcastService
{
    /// <summary>
    /// Broadcasts a message to multiple chat IDs with configurable rate limiting.
    /// </summary>
    /// <param name="chatIds">List of chat IDs to send the message to</param>
    /// <param name="messageText">The message text to send</param>
    /// <param name="options">Broadcast configuration options</param>
    /// <param name="progressCallback">Optional callback to receive progress updates</param>
    /// <param name="cancellationToken">Cancellation token for cancellation support</param>
    /// <returns>Broadcast result containing success/failure information for each chat</returns>
    Task<BroadcastResult> BroadcastAsync(
        IReadOnlyList<long> chatIds,
        string messageText,
        BroadcastOptions? options = null,
        Func<BroadcastProgress, Task>? progressCallback = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Broadcasts a message to multiple users with configurable rate limiting.
    /// </summary>
    /// <param name="users">List of users to send the message to</param>
    /// <param name="messageText">The message text to send</param>
    /// <param name="options">Broadcast configuration options</param>
    /// <param name="progressCallback">Optional callback to receive progress updates</param>
    /// <param name="cancellationToken">Cancellation token for cancellation support</param>
    /// <returns>Broadcast result containing success/failure information for each user</returns>
    Task<BroadcastResult> BroadcastToUsersAsync(
        IReadOnlyList<Models.BotUser> users,
        string messageText,
        BroadcastOptions? options = null,
        Func<BroadcastProgress, Task>? progressCallback = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current rate limiting statistics.
    /// </summary>
    /// <returns>Rate limiting statistics</returns>
    RateLimitStats GetRateLimitStats();
}
