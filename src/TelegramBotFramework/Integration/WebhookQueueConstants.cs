#nullable enable

namespace TelegramBotFramework.Integration;

/// <summary>
/// Defaults and limits for the webhook update queue.
/// </summary>
internal static class WebhookQueueConstants
{
    /// <summary>Default number of updates the queue can hold.</summary>
    public const int DefaultQueueCapacity = 1000;

    /// <summary>Largest accepted queue capacity.</summary>
    public const int MaxQueueCapacity = 100_000;

    /// <summary>Default number of background workers that process queued updates.</summary>
    public const int DefaultQueueWorkerCount = 4;

    /// <summary>Largest accepted number of background workers.</summary>
    public const int MaxQueueWorkerCount = 64;

    /// <summary>Message used when the queue capacity is outside the accepted range.</summary>
    public const string InvalidQueueCapacityMessage = "QueueCapacity must be between 1 and 100000.";

    /// <summary>Message used when the queue worker count is outside the accepted range.</summary>
    public const string InvalidQueueWorkerCountMessage = "QueueWorkerCount must be between 1 and 64.";

    /// <summary>Message used when the queue full policy is not a defined value.</summary>
    public const string InvalidQueueFullPolicyMessage = "QueueFullPolicy is not a defined value.";

    /// <summary>Log message emitted when an update is rejected because the queue is full.</summary>
    public const string LogQueueFullRejected =
        "Webhook update queue is full (capacity {QueueCapacity}); rejecting update {UpdateId}";

    /// <summary>Log message emitted when the oldest queued update is evicted to make room.</summary>
    public const string LogQueueFullDroppedOldest =
        "Webhook update queue is full (capacity {QueueCapacity}); dropped oldest queued update {DroppedUpdateId}";

    /// <summary>Log message emitted when shutdown leaves queued updates unprocessed.</summary>
    public const string LogQueueDrainIncomplete =
        "Webhook update queue stopped with {QueueDepth} update(s) not processed";
}
