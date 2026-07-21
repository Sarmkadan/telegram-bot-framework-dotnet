# RateLimitStats

The `RateLimitStats` type provides a snapshot of the current operational metrics regarding message throughput and concurrency limits within the Telegram Bot Framework. It aggregates real-time data on sent and failed messages, calculates average transmission rates, and exposes the current and maximum concurrency levels to assist developers in monitoring bot performance and adhering to Telegram's rate limiting policies.

## API

### `MessagesPerSecond`
```csharp
public int MessagesPerSecond { get; }
```
Represents the instantaneous rate of messages being processed per second at the time of the snapshot. This value is an integer approximation of the current throughput.

### `MaxConcurrency`
```csharp
public int MaxConcurrency { get; }
```
Indicates the configured maximum number of concurrent message operations allowed by the framework. This value remains constant unless the bot's configuration is dynamically altered.

### `TotalMessagesSent`
```csharp
public long TotalMessagesSent { get; }
```
The cumulative count of messages successfully transmitted since the bot instance started or statistics were last reset. This counter increments upon successful acknowledgment from the Telegram API.

### `TotalMessagesFailed`
```csharp
public long TotalMessagesFailed { get; }
```
The cumulative count of message operations that resulted in an exception or failure response. This includes network errors, API errors, and serialization issues.

### `AverageMessagesPerSecond`
```csharp
public double AverageMessagesPerSecond { get; }
```
Calculates the mean rate of message transmission over the lifetime of the current statistics session. This provides a smoother metric than `MessagesPerSecond` for trend analysis.

### `CurrentConcurrency`
```csharp
public int CurrentConcurrency { get; }
```
Shows the number of message operations currently in progress. This value fluctuates between 0 and `MaxConcurrency` depending on the load.

### `Timestamp`
```csharp
public DateTime Timestamp { get; }
```
The precise date and time when this statistics snapshot was generated. This is critical for calculating time-deltas when comparing multiple snapshots.

### `RateLimitStats` (Constructor)
```csharp
public RateLimitStats()
```
Initializes a new instance of the `RateLimitStats` class. The constructor creates a snapshot with default values (typically zeros for counters and the current system time for `Timestamp`) which are then populated by the framework's monitoring services.

## Usage

### Monitoring Bot Health
The following example demonstrates how to retrieve current statistics and log a warning if the failure rate exceeds a specific threshold or if the bot is operating at maximum concurrency.

```csharp
using Telegram.Bot.Framework.Abstractions;

public void CheckBotHealth(RateLimitStats stats)
{
    if (stats.CurrentConcurrency >= stats.MaxConcurrency)
    {
        Console.WriteLine($"Warning: Bot is operating at maximum concurrency ({stats.MaxConcurrency}).");
    }

    if (stats.TotalMessagesSent > 0)
    {
        double failureRate = (double)stats.TotalMessagesFailed / (stats.TotalMessagesSent + stats.TotalMessagesFailed);
        if (failureRate > 0.05) // 5% failure threshold
        {
            Console.WriteLine($"Alert: High failure rate detected: {failureRate:P2}");
        }
    }
    
    Console.WriteLine($"Snapshot taken at: {stats.Timestamp}");
}
```

### Calculating Throughput Trends
This example illustrates comparing two snapshots to determine the actual message throughput over a specific time interval, utilizing the `Timestamp` and `TotalMessagesSent` properties.

```csharp
using System;

public double CalculateIntervalThroughput(RateLimitStats startStats, RateLimitStats endStats)
{
    if (endStats.Timestamp <= startStats.Timestamp)
    {
        throw new ArgumentException("End timestamp must be later than start timestamp.");
    }

    long messagesDelta = endStats.TotalMessagesSent - startStats.TotalMessagesSent;
    double timeDeltaSeconds = (endStats.Timestamp - startStats.Timestamp).TotalSeconds;

    if (timeDeltaSeconds == 0)
    {
        return 0;
    }

    return messagesDelta / timeDeltaSeconds;
}
```

## Notes

*   **Snapshot Immutability**: Instances of `RateLimitStats` represent a point-in-time snapshot. The properties are read-only; to get updated values, a new instance must be requested from the monitoring service.
*   **Thread Safety**: The properties within this class are primitive value types or immutable structs (`DateTime`). Reading these properties is inherently thread-safe. However, the logic generating these stats (not shown here) must ensure atomic reads of the underlying counters to prevent torn reads, though this class itself exposes only the finalized values.
*   **Zero-Division Risks**: When calculating derived metrics manually (as shown in the Usage examples), consumers must guard against division by zero, particularly when `TotalMessagesSent` is 0 or when the `Timestamp` difference between two snapshots is negligible.
*   **Integer Precision**: `MessagesPerSecond` is an `int`, which may truncate fractional values for low-throughput scenarios. For precise analysis, `AverageMessagesPerSecond` (double) or manual calculation using `TotalMessagesSent` and `Timestamp` is recommended.
