# BroadcastResult

Represents the outcome of a broadcast operation that sends a message to multiple Telegram chats, including counts of successful and failed deliveries along with details of any failures.

## API

### Properties

- **TotalChats** `public int`
  The total number of chats targeted by the broadcast operation.

- **SuccessCount** `public int`
  The number of chats where the message was successfully delivered.

- **FailedCount** `public int`
  The number of chats where the message delivery failed.

- **SuccessfulChatIds** `public IReadOnlyList<long>`
  A read-only list of chat identifiers (`long`) for chats where delivery succeeded. Never `null`.

- **Failures** `public IReadOnlyList<FailedChat>`
  A read-only list of `FailedChat` objects describing each failed delivery attempt. Never `null`.

- **Summary** `public string?`
  An optional human-readable summary of the broadcast outcome. May be `null` if no summary is provided.

### Constructors

- **BroadcastResult** `public`
  Constructs a new `BroadcastResult` instance. This constructor is intended for internal use or advanced scenarios; prefer the static factory methods (`Success`, `Failure`, `Mixed`) for typical usage.

### Static Methods

- **Success** `public static BroadcastResult Success(int totalChats, IReadOnlyList<long> successfulChatIds, string? summary = null)`
  Creates a `BroadcastResult` indicating all deliveries succeeded.
  - `totalChats`: Total number of chats targeted.
  - `successfulChatIds`: Non-empty list of successful chat IDs.
  - `summary`: Optional summary string.
  - Returns a `BroadcastResult` with `SuccessCount == totalChats`, `FailedCount == 0`, and empty `Failures`.

- **Failure** `public static BroadcastResult Failure(int totalChats, IReadOnlyList<FailedChat> failures, string? summary = null)`
  Creates a `BroadcastResult` indicating all deliveries failed.
  - `totalChats`: Total number of chats targeted.
  - `failures`: Non-empty list of `FailedChat` objects.
  - `summary`: Optional summary string.
  - Returns a `BroadcastResult` with `SuccessCount == 0`, `FailedCount == totalChats`, and `Failures` containing all failures.

- **Mixed** `public static BroadcastResult Mixed(int totalChats, int successCount, int failedCount, IReadOnlyList<long> successfulChatIds, IReadOnlyList<FailedChat> failures, string? summary = null)`
  Creates a `BroadcastResult` for a mixed outcome with both successes and failures.
  - `totalChats`: Total number of chats targeted (must equal `successCount + failedCount`).
  - `successCount`: Number of successful deliveries.
  - `failedCount`: Number of failed deliveries.
  - `successfulChatIds`: List of successful chat IDs (count must match `successCount`).
  - `failures`: List of `FailedChat` objects (count must match `failedCount`).
  - `summary`: Optional summary string.
  - Returns a `BroadcastResult` combining successes and failures.

## Usage

```csharp
// Example 1: Successful broadcast
var result = BroadcastResult.Success(
    totalChats: 5,
    successfulChatIds: new List<long> { 123, 456, 789, 101, 112 },
    summary: "All messages delivered successfully."
);

Console.WriteLine($"Success: {result.SuccessCount}/{result.TotalChats}");
Console.WriteLine($"Failed: {result.FailedCount}");
Console.WriteLine($"Summary: {result.Summary}");

// Example 2: Mixed outcome with detailed failures
var failures = new List<FailedChat>
{
    new FailedChat(234, "Chat not found."),
    new FailedChat(345, "Bot blocked by user.")
};

var mixedResult = BroadcastResult.Mixed(
    totalChats: 4,
    successCount: 2,
    failedCount: 2,
    successfulChatIds: new List<long> { 567, 678 },
    failures: failures,
    summary: "Partial delivery due to user restrictions."
);

Console.WriteLine($"Success: {mixedResult.SuccessCount}/{mixedResult.TotalChats}");
Console.WriteLine($"Failed: {mixedResult.FailedCount}");
foreach (var failure in mixedResult.Failures)
{
    Console.WriteLine($"Chat {failure.ChatId} failed: {failure.Reason}");
}
```

## Notes

- Thread safety: This type is immutable and safe for concurrent reads. All properties are read-only and collections are exposed as `IReadOnlyList<T>`, so no defensive copying is required when accessing them.
- Empty collections: `SuccessfulChatIds` and `Failures` are never `null`; they are empty lists when there are no successes or failures, respectively.
- Validation: The static factory methods (`Success`, `Failure`, `Mixed`) validate input counts and list sizes. Passing inconsistent values (e.g., `successCount + failedCount != totalChats`) will throw an `ArgumentException`.
