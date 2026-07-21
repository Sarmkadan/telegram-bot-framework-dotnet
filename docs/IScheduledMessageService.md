# IScheduledMessageService

Represents a scheduled message within the framework. Instances of this type are returned by the scheduling service and provide a snapshot of a message that has been queued for future delivery. The interface exposes the current state of the schedule, including delivery target, content, timing, and execution history. It is intended to be read‑only after creation; modifications to the schedule (e.g., cancellation) are performed through the scheduling service itself.

## API

### `string Id`
- **Type:** `string`  
- **Description:** A unique identifier for this scheduled message. The value is assigned when the schedule is created and remains constant for the lifetime of the schedule.

### `long ChatId`
- **Type:** `long`  
- **Description:** The Telegram chat identifier to which the message will be sent. This value is set at creation time and cannot be changed.

### `string Text`
- **Type:** `string`  
- **Description:** The text content of the scheduled message. May be `null` or empty if the message consists only of non‑text elements (e.g., media). The value is fixed when the schedule is created.

### `DateTimeOffset ScheduledTime`
- **Type:** `DateTimeOffset`  
- **Description:** The intended time at which the message should be sent. The scheduling service will attempt to deliver the message as close to this time as possible, subject to internal queue processing.

### `DateTimeOffset CreatedAt`
- **Type:** `DateTimeOffset`  
- **Description:** The UTC timestamp when this schedule was created. This is set by the scheduling service at the moment the schedule is persisted.

### `bool IsCancelled`
- **Type:** `bool`  
- **Description:** Indicates whether the schedule has been cancelled. Once set to `true`, the message will never be sent, even if the scheduled time has passed. A cancelled schedule cannot be re‑activated.

### `bool IsSent`
- **Type:** `bool`  
- **Description:** Indicates whether the message has been successfully sent. This property is set to `true` only after the Telegram API confirms delivery. It remains `false` if the message is still pending, cancelled, or has failed permanently.

### `DateTimeOffset? SentAt`
- **Type:** `DateTimeOffset?`  
- **Description:** The UTC timestamp when the message was actually sent. This value is `null` until the message is successfully delivered. After successful delivery, it contains the exact time of the send operation.

### `string? ErrorMessage`
- **Type:** `string?`  
- **Description:** If the last delivery attempt failed, this property contains a human‑readable error description. It is `null` when no error has occurred (i.e., the message has not yet been attempted or was sent successfully). The error message is overwritten on each new attempt.

### `DateTimeOffset? NextAttemptTime`
- **Type:** `DateTimeOffset?`  
- **Description:** The UTC timestamp of the next scheduled retry attempt. This value is `null` when the message has been sent successfully, cancelled, or when no further retries are configured. For pending messages, it indicates when the next delivery attempt will be made.

### `int AttemptCount`
- **Type:** `int`  
- **Description:** The number of delivery attempts that have been made so far. This counter starts at `0` and increments each time the scheduling service attempts to send the message, regardless of success or failure.

## Usage

### Example 1: Inspecting a scheduled message after retrieval

```csharp
// Assume 'schedule' is an instance of IScheduledMessageService obtained from the scheduling service.
Console.WriteLine($"Schedule ID: {schedule.Id}");
Console.WriteLine($"Target chat: {schedule.ChatId}");
Console.WriteLine($"Scheduled for: {schedule.ScheduledTime:yyyy-MM-dd HH:mm:ss} UTC");
Console.WriteLine($"Created at: {schedule.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC");
Console.WriteLine($"Cancelled: {schedule.IsCancelled}");
Console.WriteLine($"Sent: {schedule.IsSent}");

if (schedule.IsSent)
{
    Console.WriteLine($"Sent at: {schedule.SentAt:yyyy-MM-dd HH:mm:ss} UTC");
}
else if (!schedule.IsCancelled)
{
    Console.WriteLine($"Next attempt: {schedule.NextAttemptTime:yyyy-MM-dd HH:mm:ss} UTC");
    Console.WriteLine($"Attempts so far: {schedule.AttemptCount}");
    if (!string.IsNullOrEmpty(schedule.ErrorMessage))
    {
        Console.WriteLine($"Last error: {schedule.ErrorMessage}");
    }
}
```

### Example 2: Checking delivery status and handling failures

```csharp
public void ProcessSchedule(IScheduledMessageService schedule)
{
    if (schedule.IsSent)
    {
        // Message was delivered successfully – no further action needed.
        return;
    }

    if (schedule.IsCancelled)
    {
        // Schedule was cancelled – ignore.
        return;
    }

    // Message is still pending or has failed.
    if (schedule.AttemptCount > 0 && !string.IsNullOrEmpty(schedule.ErrorMessage))
    {
        // Log the failure for monitoring.
        Log.Warning($"Schedule {schedule.Id} failed on attempt {schedule.AttemptCount}: {schedule.ErrorMessage}");
    }

    // Optionally, reschedule or notify an administrator if too many attempts.
    if (schedule.AttemptCount >= 5)
    {
        NotifyAdmin($"Schedule {schedule.Id} has exceeded maximum retry attempts.");
    }
}
```

## Notes

- **Thread safety:** Instances of `IScheduledMessageService` are designed to be immutable after creation. All property values are set once by the scheduling service and do not change except for the following properties, which are updated atomically by the service: `IsSent`, `SentAt`, `ErrorMessage`, `NextAttemptTime`, and `AttemptCount`. Reading these properties concurrently from multiple threads is safe; however, the values may change between reads. For a consistent snapshot, consider capturing the instance reference and reading all properties within a short time window.
- **Nullability:** `Text`, `ErrorMessage`, `SentAt`, and `NextAttemptTime` are nullable. Code that consumes these properties should check for `null` before using them, especially when formatting or passing to non‑nullable contexts.
- **Edge cases:**  
  - A schedule may have `IsSent == false` and `IsCancelled == false` even after the `ScheduledTime` has passed, if the service is still retrying or if the queue is backlogged.  
  - `AttemptCount` can be `0` even when `IsSent == false` if the first attempt has not yet been made.  
  - `ErrorMessage` is cleared (set to `null`) after a successful send, but `AttemptCount` is not reset.  
  - `NextAttemptTime` may be `null` even when `IsSent == false` if the service has exhausted all retry policies and will not attempt again. In that case the schedule is considered permanently failed, though `IsCancelled` remains `false`.
