# ScheduledTaskManagerBuilder

`ScheduledTaskManagerBuilder` is a fluent builder that collects the properties of a scheduled task and validates them in `Build()`. The class is declared in `src/TelegramBotFramework/BackgroundWorkers/ScheduledTaskManagerBuilder.cs` as `public sealed`.

> **Warning:** `Build()` validates the values you set, but it does not apply them. It returns a new, empty `ScheduledTaskManager`. The `Id`, `Name`, `TaskFunc`, `IsRecurring`, `Interval`, and timestamp values you pass to the builder are not copied to the result. Do not rely on the builder to produce a configured task manager. Use the builder only as a validation step, and schedule the work on the returned manager with `ScheduleOnce` or `ScheduleRecurring`, as shown in the [usage example](#usage-example).

## Purpose

The builder is meant to catch invalid task settings early. Each setter rejects some invalid input immediately, and `Build()` checks that the required values are present:

- `Id` and `Name` must be non-empty.
- A task function must be set.
- `Interval` must be positive.

Because the builder keeps its own state, the same builder can be reused. Each call to `Build()` creates a new `ScheduledTaskManager`.

## Builder methods

All setters return the same builder instance, so calls can be chained.

| Method | Parameter | Validation at call time | Stored in builder |
|---|---|---|---|
| `WithId(string id)` | Task ID | Throws `ArgumentException` if null, empty, or whitespace | Yes |
| `WithName(string name)` | Task name | Throws `ArgumentException` if null, empty, or whitespace | Yes |
| `WithTaskFunc(Func<Task> taskFunc)` | Task body | Throws `ArgumentNullException` if `null` | Yes |
| `WithIsRecurring(bool isRecurring)` | Recurring flag | None | Yes |
| `WithInterval(TimeSpan interval)` | Interval | Throws `ArgumentException` if `<= TimeSpan.Zero` | Yes |
| `WithCreatedAt(DateTime createdAt)` | Creation timestamp | None | Yes |
| `WithLastExecutedAt(DateTime? lastExecutedAt)` | Last execution timestamp | None | Yes |
| `WithLastSuccessAt(DateTime? lastSuccessAt)` | Last success timestamp | None | Yes |
| `WithLastErrorAt(DateTime? lastErrorAt)` | Last error timestamp | None | Yes |
| `WithLastError(string? lastError)` | Last error message | None | Yes |

The `WithCreatedAt`, `WithLastExecutedAt`, `WithLastSuccessAt`, `WithLastErrorAt`, and `WithLastError` values are stored but never read. `Build()` does not use them.

### From

```csharp
public static ScheduledTaskManagerBuilder From(ScheduledTaskManager template)
```

Throws `ArgumentNullException` if `template` is `null`. Otherwise it returns a new, empty builder. It does not copy any values from `template`. The code comments say this is intentional, because `ScheduledTaskManager` does not expose its internal tasks.

### Build

```csharp
public ScheduledTaskManager Build()
```

Checks the stored values in this order and throws `ArgumentException` for the first failure:

1. `Id` is empty or whitespace: `Task ID is required.`
2. `Name` is empty or whitespace: `Task name is required.`
3. The task function is `null`: `Task function is required.`
4. `Interval` is `<= TimeSpan.Zero`: `Interval must be positive.`

If all checks pass, it returns `new ScheduledTaskManager()`, which uses the default constructor and a `ConsoleLogger` fallback for logging. No values from the builder are applied to the result.

Note that `WithInterval` is only checked when `Build()` runs. If you never call `WithInterval`, `Build()` throws `Interval must be positive.`. This is the case even when `IsRecurring` is `false`.

## Usage example

The example below validates the task settings with the builder, then schedules the work on the manager that `Build()` returns. The values in the builder are not passed to the scheduler, so the scheduling call repeats the same values.

```csharp
using System;
using System.Threading.Tasks;
using TelegramBotFramework.BackgroundWorkers;

var builder = new ScheduledTaskManagerBuilder()
    .WithId("cleanup-sessions")
    .WithName("Session cleanup")
    .WithTaskFunc(() => CleanupExpiredSessionsAsync())
    .WithIsRecurring(true)
    .WithInterval(TimeSpan.FromMinutes(15));

// Throws ArgumentException if any required value is missing or invalid.
using ScheduledTaskManager manager = builder.Build();

// The builder's values are not applied to 'manager', so schedule the task explicitly.
string taskId = manager.ScheduleRecurring(
    () => CleanupExpiredSessionsAsync(),
    TimeSpan.FromMinutes(15),
    "Session cleanup");

ScheduledTask? task = manager.GetTask(taskId);

static Task CleanupExpiredSessionsAsync()
{
    // Application-specific cleanup logic.
    return Task.CompletedTask;
}
```

`ScheduledTaskManager` implements `IDisposable`, so `using` stops all of its timers when the scope ends.

## See also

- `src/TelegramBotFramework/BackgroundWorkers/ScheduledTaskManager.cs`: the manager that `Build()` returns, with `ScheduleOnce`, `ScheduleRecurring`, `CancelTask`, `GetTask`, and `StopAll`.
- `src/TelegramBotFramework/BackgroundWorkers/ScheduledTaskManagerValidation.cs`: `Validate()` extension for `ScheduledTask` instances. It does not validate the builder.
