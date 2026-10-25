# MetricsMiddleware

`MetricsMiddleware` is a sealed `IEventMiddleware` that measures how long each event takes to process and how often processing succeeds or fails. It wraps the rest of the event pipeline, so it sees the time spent in every later middleware and in the event handlers. The class is declared in `src/TelegramBotFramework/Events/MetricsMiddleware.cs`.

> **Note:** The statistics are kept per event type, but they are not exposed through any public method or property. The middleware reports them only through the logger: a debug line for each event and an information line with the aggregated numbers every 100 events of a given type. To read the numbers in an application, configure a logging provider that captures `Information` messages from `MetricsMiddleware`.

## Purpose

Use the middleware to answer three questions about event processing:

- How many events of each type were processed?
- How many of them succeeded and how many threw an exception?
- What is the average processing time per event type?

Each event type has its own counters, identified by `IEvent.EventType`. Events without a `CorrelationId` are logged with the correlation ID `unknown`.

## Public API

### Constructor

```csharp
public MetricsMiddleware(ILogger<MetricsMiddleware>? logger = null)
```

| Parameter | Description |
|-----------|-------------|
| `logger` | Logger used for all output. When `null`, a `ConsoleLogger<MetricsMiddleware>` is created and writes to `Console.Out`. |

### MiddlewareName

```csharp
public string MiddlewareName { get; }
```

Returns the constant string `"MetricsMiddleware"`. It is used for logging and debugging.

### InvokeAsync

```csharp
public async Task InvokeAsync(IEvent evt, Func<Task> next)
```

Implements `IEventMiddleware.InvokeAsync`. It throws `ArgumentNullException` when `evt` or `next` is `null`. Otherwise it:

1. Reads `evt.EventType` and `evt.CorrelationId` (the latter defaults to `"unknown"`).
2. Records the start time with `DateTime.UtcNow` and increments `TotalProcessed` for the event type.
3. Logs a debug message: `Metrics: Starting event {EventType} with correlation {CorrelationId}`.
4. Awaits `next()` with `ConfigureAwait(false)`.
5. On success, increments `SuccessCount`, adds the elapsed time to `TotalProcessingTime`, and logs a debug message.
6. On exception, increments `FailureCount`, adds the elapsed time, logs an error with the exception, and rethrows the original exception. The middleware never swallows errors.
7. In a `finally` block, calls the aggregate logger when `TotalProcessed` is a multiple of 100.

The middleware does not change the event or the result of `next`. Its only side effects are the counters and log output.

## Metrics collected

For each event type, the middleware keeps the following values in memory:

| Field | Meaning |
|-------|---------|
| `TotalProcessed` | Number of times processing started for this event type. |
| `SuccessCount` | Number of runs where `next()` completed without an exception. |
| `FailureCount` | Number of runs where `next()` threw. |
| `TotalProcessingTime` | Sum of elapsed wall-clock time across finished runs. |

The average reported in logs is `TotalProcessingTime / TotalProcessed` in milliseconds.

## Log output

| Level | Message | When |
|-------|---------|------|
| Debug | `Metrics: Starting event {EventType} with correlation {CorrelationId}` | Before each event is processed. |
| Debug | `Metrics: Completed event {EventType} successfully` | After a successful run. |
| Error | `Metrics: Failed to process event {EventType}` | After a failed run, with the exception attached. |
| Information | `Event Metrics - {EventType}: Total=..., Success=..., Failures=..., AvgTimeMs=...` | Every 100th event of the type. |

The `Information` line is the only summary the middleware produces, so it is the line to monitor.

## Usage

Register the middleware with the event bus. Middleware runs in the order it is registered, so register `MetricsMiddleware` before the middleware whose time you want to measure.

```csharp
using Microsoft.Extensions.Logging;
using TelegramBotFramework.Events;

ILogger<MetricsMiddleware> metricsLogger = loggerFactory.CreateLogger<MetricsMiddleware>();

var bus = new EventBus();
bus.RegisterMiddleware(new MetricsMiddleware(metricsLogger));

bus.Subscribe(new OrderPlacedHandler());

await bus.PublishAsync(new OrderPlacedEvent
{
    CorrelationId = Guid.NewGuid().ToString("N")
});
```

`UseMiddleware` from `EventBusExtensions` is an equivalent way to register the middleware on an `IEventBus`:

```csharp
IEventBus bus = new EventBus();
bus.UseMiddleware(new MetricsMiddleware());
```

In this form the middleware falls back to `ConsoleLogger`, so debug messages are printed as well as the summaries.

If you need the middleware with dependency injection, register it as a singleton. It holds the per-type counters, so a new instance per event would reset them.

## Behavior and limitations

- **Per-type counters are kept in memory.** They are not persisted and reset when the process restarts.
- **Thread safety is partial.** The dictionary lookup is guarded by a lock, but the counters are updated outside that lock. Under concurrent publishing, increments can be lost and the reported numbers can be slightly low.
- **Average time can be understated while events are in flight.** `TotalProcessed` is incremented when an event starts, but its duration is added only when it finishes. The average can therefore be lower than the true value while runs are still in progress.
- **Timing uses `DateTime.UtcNow`.** The measurement is wall-clock time and has the resolution of `DateTime`, not a high-resolution `Stopwatch`.
- **Aggregate logs are periodic.** A summary is written only when the count reaches a multiple of 100, so event types with fewer than 100 events produce no summary.
- **Debug messages depend on the logger.** Starting and completion messages are logged at `Debug`. With a standard logger, they are filtered by the configured minimum level. With the `ConsoleLogger` fallback, every level is written.
- **Errors are logged and rethrown.** The middleware logs each failure at `Error` level. The exception then continues through the pipeline, so it is also handled by whatever surrounds the event bus.

## Related types

- `IEventMiddleware` - the interface this class implements.
- `EventBus` / `IEventBus.RegisterMiddleware` - runs the middleware pipeline around handlers.
- `EventBusExtensions.UseMiddleware` - fluent registration helper.
- `ConsoleLogger<T>` - the fallback logger used when no logger is passed.
