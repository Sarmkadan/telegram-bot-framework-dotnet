# BroadcastOptions

The `BroadcastOptions` class configures the behavior of bulk message broadcasting operations within the Telegram Bot Framework. It provides fine-grained control over throughput limits, concurrency, retry policies, error handling strategies, and message formatting, allowing developers to balance speed with reliability while adhering to Telegram API rate limits.

## API

### MessagesPerSecond
```csharp
public int MessagesPerSecond
```
Defines the maximum number of messages the broadcaster attempts to send per second. This value acts as a rate limiter to prevent triggering Telegram's server-side flood control mechanisms. Setting this too high may result in transient `429 Too Many Requests` errors regardless of other concurrency settings.

### MaxConcurrency
```csharp
public int MaxConcurrency
```
Specifies the maximum number of simultaneous API requests allowed during the broadcast operation. This controls the degree of parallelism; a higher value increases throughput but consumes more system resources and network sockets. This limit is enforced in addition to `MessagesPerSecond`.

### MaxRetryAttempts
```csharp
public int MaxRetryAttempts
```
Determines the number of times the framework will attempt to resend a failed message before marking it as permanently failed. This applies to transient errors such as network timeouts or temporary server unavailability. A value of `0` disables retries.

### RetryDelay
```csharp
public TimeSpan RetryDelay
```
Sets the fixed time interval to wait between retry attempts for a specific failed message. This delay helps mitigate immediate re-triggering of rate limits or server stress. It is applied between each of the `MaxRetryAttempts`.

### ContinueOnError
```csharp
public bool ContinueOnError
```
Indicates whether the broadcast operation should proceed to subsequent recipients after encountering an error with a specific message. If `false`, the entire broadcast operation halts immediately upon the first unrecoverable error. If `true`, errors are logged or aggregated, and the process continues for remaining targets.

### MessageFormatter
```csharp
public Func<string, long, string>? MessageFormatter
```
An optional delegate used to dynamically generate or modify the message content for each recipient.
*   **Parameters**:
    *   `string template`: The original message template provided to the broadcast method.
    *   `long chatId`: The unique identifier of the target chat.
*   **Returns**: A `string` containing the final message content to be sent.
*   **Throws**: Exceptions thrown within the formatter function will be treated as message processing errors and handled according to `ContinueOnError` and retry settings.

### BatchDelay
```csharp
public TimeSpan? BatchDelay
```
Defines an optional time delay to insert between processing batches of messages. If `null`, no artificial delay is added between batches beyond what is required by `MessagesPerSecond`. This can be useful for implementing "cool-down" periods during very large broadcasts to further reduce the risk of account restrictions.

## Usage

### Example 1: Conservative Broadcast with Retries
This configuration prioritizes reliability over speed, suitable for critical notifications where delivery is paramount. It limits concurrency, enables retries with a delay, and ensures the process stops if a systemic error occurs.

```csharp
var options = new BroadcastOptions
{
    MessagesPerSecond = 5,
    MaxConcurrency = 2,
    MaxRetryAttempts = 3,
    RetryDelay = TimeSpan.FromSeconds(2),
    ContinueOnError = false,
    MessageFormatter = null,
    BatchDelay = null
};

await broadcaster.BroadcastAsync(chatIds, "Critical System Update", options);
```

### Example 2: High-Throughput Broadcast with Personalization
This configuration optimizes for speed while personalizing messages. It allows higher concurrency and skips individual failures to ensure the rest of the list is processed. A custom formatter injects the user's ID into the message.

```csharp
var options = new BroadcastOptions
{
    MessagesPerSecond = 20,
    MaxConcurrency = 10,
    MaxRetryAttempts = 1,
    RetryDelay = TimeSpan.FromMilliseconds(500),
    ContinueOnError = true,
    MessageFormatter = (template, chatId) => $"{template} (Ref: {chatId})",
    BatchDelay = TimeSpan.FromMinutes(1)
};

await broadcaster.BroadcastAsync(chatIds, "Hello User", options);
```

## Notes

*   **Rate Limiting Interaction**: `MessagesPerSecond` and `MaxConcurrency` work in tandem. Even if `MaxConcurrency` is high, the effective throughput will not exceed `MessagesPerSecond`. Conversely, a low `MaxConcurrency` will bottleneck the rate even if `MessagesPerSecond` is set high.
*   **Error Propagation**: When `ContinueOnError` is set to `false`, a single persistent failure (after exhausting `MaxRetryAttempts`) will abort the entire operation. The `BroadcastResult` will indicate partial completion up to the point of failure.
*   **Thread Safety**: The `MessageFormatter` delegate must be thread-safe, as it may be invoked concurrently by multiple tasks depending on the `MaxConcurrency` setting. Avoid capturing mutable state without proper synchronization within the formatter.
*   **Retry Logic**: The `RetryDelay` is applied per message attempt, not globally. If multiple messages fail simultaneously, their retry timers operate independently.
*   **Null Handling**: If `MessageFormatter` is `null`, the original template string is sent without modification. If `BatchDelay` is `null`, batching logic relies solely on the rate limiter.
