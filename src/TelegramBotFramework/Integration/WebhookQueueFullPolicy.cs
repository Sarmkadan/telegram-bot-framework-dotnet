#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Integration;

/// <summary>
/// Determines what happens to an incoming webhook update when the update queue is full.
/// </summary>
public enum WebhookQueueFullPolicy
{
    /// <summary>
    /// Rejects the incoming update and answers the request with <c>429 Too Many Requests</c>,
    /// so Telegram redelivers it later. No update is lost. This is the default.
    /// </summary>
    Reject = 0,

    /// <summary>
    /// Evicts the oldest queued update to make room for the incoming one and answers <c>200 OK</c>.
    /// Evicted updates are counted in <see cref="WebhookInfo.UpdatesDropped"/> and are not processed.
    /// </summary>
    DropOldest = 1
}
