#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Integration;

/// <summary>
/// Outcome of offering a received update to the webhook update queue.
/// </summary>
public enum WebhookEnqueueResult
{
    /// <summary>The update was queued for background processing.</summary>
    Accepted = 0,

    /// <summary>The queue was full and the policy is <see cref="WebhookQueueFullPolicy.Reject"/>; the update was not queued.</summary>
    Rejected = 1,

    /// <summary>The queue has been closed because the service is stopping; the update was not queued.</summary>
    Closed = 2
}
