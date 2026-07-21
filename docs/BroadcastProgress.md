# BroadcastProgress

`BroadcastProgress` is a value type used to report the progress of a broadcast operation (e.g., sending messages to multiple chats) in the `telegram-bot-framework-dotnet` library. It provides real-time metrics on the operation's status, including counts of processed chats, success/failure rates, timing information, and failure details.

## API

### `TotalChats`
- **Type**: `int`
- **Description**: The total number of chats targeted by the broadcast operation. This value is set at the start of the operation and remains constant.
- **Notes**: Will never be negative. If the operation targets no chats, this value is `0`.

### `ProcessedCount`
- **Type**: `int`
- **Description**: The number of chats that have been processed so far (successfully or unsuccessfully). Increments monotonically during the operation.
- **Notes**: Will never exceed `TotalChats`. If `TotalChats` is `0`, this value remains `0`.

### `SuccessCount`
- **Type**: `int`
- **Description**: The number of chats that were successfully processed (i.e., messages sent without errors). Increments when a chat is processed without failure.
- **Notes**: Will never exceed `ProcessedCount`. If no chats succeeded, this value is `0`.

### `FailedCount`
- **Type**: `int`
- **Description**: The number of chats that failed to be processed. Increments when a chat encounters an error during processing.
- **Notes**: Will never exceed `ProcessedCount`. If no chats failed, this value is `0`.

### `Failures`
- **Type**: `IReadOnlyList<FailedChat>`
- **Description**: A read-only collection of `FailedChat` objects representing chats that failed during processing. Includes details such as `ChatId`, `ErrorMessage`, and `RetryAttempts`.
- **Notes**: Empty if no failures occurred. The list is immutable and reflects the state at the time of access.

### `ElapsedTime`
- **Type**: `TimeSpan`
- **Description**: The total time elapsed since the broadcast operation started.
- **Notes**: Always non-negative. May be `TimeSpan.Zero` if the operation just started.

### `EstimatedTimeRemaining`
- **Type**: `TimeSpan?`
- **Description**: An estimate of the remaining time until the broadcast operation completes, or `null` if the estimate cannot be calculated (e.g., insufficient data or early stage of the operation).
- **Notes**: If not `null`, the value is non-negative. May fluctuate during the operation.

### `CurrentMessagesPerSecond`
- **Type**: `double`
- **Description**: The current rate of messages being processed per second, based on recent activity.
- **Notes**: Non-negative. May be `0` if no messages are being processed at the current moment.

### `BroadcastProgress`
- **Type**: `public BroadcastProgress`
- **Description**: Constructor for `BroadcastProgress`. Initializes a new instance with the provided progress metrics.
- **Parameters**:
  - `totalChats`: The total number of chats targeted.
  - `processedCount`: The number of chats processed so far.
  - `successCount`: The number of chats processed successfully.
  - `failedCount`: The number of chats that failed.
  - `failures`: A collection of `FailedChat` objects for failed chats.
  - `elapsedTime`: The time elapsed since the operation started.
  - `estimatedTimeRemaining`: Optional estimate of remaining time.
  - `currentMessagesPerSecond`: The current processing rate.
- **Notes**: Throws `ArgumentOutOfRangeException` if `processedCount`, `successCount`, or `failedCount` exceed `totalChats`, or if any count is negative. Throws `ArgumentNullException` if `failures` is `null`.

### `ChatId`
- **Type**: `long`
- **Description**: The unique identifier of the chat associated with a failure (used in `FailedChat`).
- **Notes**: Represents a valid Telegram chat ID. Negative values are invalid and indicate a system or internal error.

### `ErrorMessage`
- **Type**: `string`
- **Description**: The error message associated with a failure (used in `FailedChat`).
- **Notes**: Non-null and non-empty when a failure occurs. May contain technical details about the error.

### `RetryAttempts`
- **Type**: `int`
- **Description**: The number of retry attempts made for a failed chat (used in `FailedChat`).
- **Notes**: Non-negative. `0` indicates no retries were attempted.

### `FailedChat`
- **Type**: `public FailedChat`
- **Description**: A record representing a chat that failed during broadcast processing. Contains `ChatId`, `ErrorMessage`, and `RetryAttempts`.
- **Notes**: Immutable. Throws `ArgumentOutOfRangeException` if `RetryAttempts` is negative. Throws `ArgumentException` if `ErrorMessage` is null or empty.

## Usage

### Example 1: Monitoring Broadcast Progress
```csharp
using var cts = new CancellationTokenSource();
var progress = new BroadcastProgress(
    totalChats: 100,
    processedCount: 45,
    successCount: 40,
    failedCount: 5,
    failures: new List<FailedChat>
    {
        new FailedChat(123456789L, "Chat not found", 2),
        new FailedChat(987654321L, "Rate limit exceeded", 0)
    },
    elapsedTime: TimeSpan.FromSeconds(30),
    estimatedTimeRemaining: TimeSpan.FromSeconds(45),
    currentMessagesPerSecond: 1.5
);

Console.WriteLine($"Progress: {progress.ProcessedCount}/{progress.TotalChats} chats processed");
Console.WriteLine($"Success: {progress.SuccessCount}, Failed: {progress.FailedCount}");
Console.WriteLine($"Failures: {progress.Failures.Count}");
foreach (var failure in progress.Failures)
{
    Console.WriteLine($"- Chat {failure.ChatId}: {failure.ErrorMessage} (Retries: {failure.RetryAttempts})");
}
```

### Example 2: Updating Progress During Broadcast
```csharp
public void UpdateProgress(BroadcastProgress progress)
{
    if (progress.ProcessedCount > 0)
    {
        double completion = (double)progress.ProcessedCount / progress.TotalChats * 100;
        Console.WriteLine($"Completion: {completion:F2}%");
    }

    if (progress.Failures.Count > 0)
    {
        Console.WriteLine($"Encountered {progress.Failures.Count} failures. Retrying...");
        RetryFailedChats(progress.Failures);
    }
}
```

## Notes

- **Thread Safety**: `BroadcastProgress` is an immutable value type. All public members are read-only or provide read-only access to collections. Instances can be safely shared across threads without synchronization.
- **Edge Cases**:
  - If `TotalChats` is `0`, all count-based metrics (`ProcessedCount`, `SuccessCount`, `FailedCount`) should be `0`, and `Failures` should be empty.
  - `EstimatedTimeRemaining` may be `null` during early stages of a broadcast or if the operation's rate is highly variable.
  - `CurrentMessagesPerSecond` may temporarily drop to `0` if processing stalls (e.g., due to rate limits or network issues).
- **Validation**: The constructor enforces invariants (e.g., counts cannot exceed `TotalChats`). Invalid inputs throw exceptions immediately.
