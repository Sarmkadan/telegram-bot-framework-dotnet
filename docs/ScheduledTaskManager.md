# ScheduledTaskManager

Provides scheduling service tasks** in the `telegram-bot-framework-dotnet`. The manager allows you to schedule one‑time and recurring tasks, query their stateful component responsible for executing user‑provided asynchronous work items at specific times or intervals. It maintains an internal collection of `ScheduledTask` objects, each representing a unit of work with metadata such as execution count, last run timestamps, and error information. The manager exposes methods to schedule tasks, cancel individual or all scheduled work.

## API

### ScheduleOnce

string ScheduleOnce(Func<Task> taskFunc, TimeSpan delay, string? id = null, string? name = null)
```
- **Purpose** – Registers a task that runs a single time after the specified delay.
- **Parameters**  
  - `taskFunc`: The asynchronous work to execute.  
  - `delay`: TimeSpan indicating when the task should first run (must be non‑negative).  
  - `id` (optional): User‑supplied identifier; if null the manager generates a GUID‑based ID.  
  - `name` (optional): Descriptive label for the task.
- **Return value** – The identifier of the scheduled task.
- **Exceptions**  
  - `ArgumentNullException` if `taskFunc` is null.  
  - `ArgumentOutOfRangeException` if `delay` is negative.  
  - `ObjectDisposedException` if the manager has been disposed.

### ScheduleRecurring

```csharp
string ScheduleRecurring(Func<Task> taskFunc, TimeSpan interval, string? id = null, string? name = null, bool runOnStart = false)
```
- **Purpose** – Registers a task that repeats every `interval`.
- **Parameters**  
  - `taskFunc`: The asynchronous work to execute on each tick.  
  - `interval`: TimeSpan between successive executions (must be positive).  
  - `id` (optional): Identifier for the task; auto‑generated if null.  
  - `name` (optional): Descriptive label.  
  - `runOnStart` (optional): If true, the task is invoked immediately before the first interval.
- **Return value** – The identifier of the scheduled task.
- **Exceptions**  
  - `ArgumentNullException` if `taskFunc` is null.  
  - `ArgumentOutOfRangeException` if `interval` is not positive.  
  - `ObjectDisposedException` if the manager has been disposed.

### CancelTask

```csharp
bool CancelTask(string taskId)
```
- **Purpose** – Attempts to cancel a scheduled task by its identifier.
- **Parameters**  
  - `taskId`: The ID of the task to cancel (must not be null).
- **Return value** – `true` if the task was found and cancelled; `false` if no matching task exists.
- **Exceptions**  
  - `ArgumentNullException` if `taskId` is null.  
  - `ObjectDisposedException` if the manager has been disposed.

### GetAllTasks

```csharp
IEnumerable<ScheduledTask> GetAllTasks()
```
- **Purpose** – Returns a snapshot of all currently scheduled tasks.
- **Parameters** – None.
- **Return value** – An enumerable of `ScheduledTask` objects representing the manager’s internal state at the moment of the call.
- **Exceptions**  
  - `ObjectDisposedException` if the manager has been disposed.

### GetTask

```csharp
ScheduledTask? GetTask(string taskId)
```
- **Purpose** – Retrieves a specific task by its identifier.
- **Parameters**  
  - `taskId`: The ID of the task to locate (must not be null).
- **Return value** – The matching `ScheduledTask` instance, or `null` if no task with the given ID exists.
- **Exceptions**  
  - `ArgumentNullException` if `taskId` is null.  
  - `ObjectDisposedException` if the manager has been disposed.

### StopAll

```csharp
void StopAll()
```
- **Purpose** – Cancels every scheduled task and prevents further executions.
- **Parameters** – None.
- **Return value** – None.
- **Exceptions**  
  - `ObjectDisposedException` if the manager has been disposed.

### Dispose

```csharp
void Dispose()
```
- **Purpose** – Releases all resources held by the manager (cancels pending tasks, stops timers, etc.).
- **Parameters** – None.
- **Return value** – None.
- **Remarks** – Calling `Dispose` multiple times is safe; after disposal any further call to scheduling or query methods throws `ObjectDisposedException`.

## ScheduledTask properties

The `ScheduledTask` objects returned by `GetAllTasks` and `GetTask` expose the following read‑only members:

| Member | Type | Description |
|--------|------|-------------|
| `Id` | `string` | Unique identifier of the task (set at creation). |
| `Name` | `string` | Human‑readable name supplied when scheduling (may be null). |
| `TaskFunc` | `Func<Task>?` | The delegate that performs the work; null only if the task has been cancelled and the delegate released. |
| `IsRecurring` | `bool` | `true` for tasks scheduled with `ScheduleRecurring`; `false` for one‑time tasks. |
| `Interval` | `TimeSpan` | For recurring tasks, the period between executions; for one‑time tasks, `TimeSpan.Zero`. |
| `CreatedAt` | `DateTime` | UTC timestamp when the task was first scheduled. |
| `LastExecutedAt` | `DateTime?` | UTC timestamp of the most recent invocation (null if never run). |
| `LastSuccessAt` | `DateTime?` | UTC timestamp of the most recent successful execution (null if never succeeded). |
| `LastErrorAt` | `DateTime?` | UTC timestamp of the most recent execution that threw an exception (null if no errors). |
| `LastError` | `string?` | Message or exception details from the last failed execution (null if no errors). |
| `ExecutionCount` | `int` | Total number of times the task’s delegate has been invoked. |

## Usage

### Example 1 – Scheduling a one‑time notification

```csharp
using System;
using System.Threading.Tasks;
using TelegramBotFramework; // namespace containing ScheduledTaskManager

var manager = new ScheduledTaskManager();

// Schedule a task that runs after 10 seconds
string taskId = manager.ScheduleOnce(
    taskFunc: async () =>
    {
        await Console.Out.WriteLineAsync("Reminder: check the webhook.");
    },
    delay: TimeSpan.FromSeconds(10),
    id: "reminder-001",
    name: "Webhook reminder"
);

Console.WriteLine($"Scheduled task {taskId}");

// Later, if needed, cancel it before it fires
// bool cancelled = manager.CancelTask(taskId);

// Clean up when the application shuts down
manager.Dispose();
```

### Example 2 – Managing a recurring polling job

```csharp
using System;
using System.Threading.Tasks;
using TelegramBotFramework;

var manager = new ScheduledTaskManager();

// Start a job that polls an external API every minute, invoking immediately
string pollId = manager.ScheduleRecurring(
    taskFunc: async () =>
    {
        var data = await FetchUpdatesAsync(); // user‑defined method
        await ProcessUpdatesAsync(data);
    },
    interval: TimeSpan.FromMinutes(1),
    id: "poller-job",
    name: "Update poller",
    runOnStart: true
);

Console.WriteLine($"Poller started with ID {pollId}");

// After some runtime, decide to stop the poller
await Task.Delay(TimeSpan.FromHours(1)); // simulate work
bool stopped = manager.CancelTask(pollId);
Console.WriteLine($"Poller cancelled: {stopped}");

// Ensure all background timers are disposed
manager.Dispose();
```

## Notes

- **Thread safety** – All public methods of `ScheduledTaskManager` are safe to invoke concurrently from multiple threads. Internal state is protected by locks, and `GetAllTasks` returns a snapshot to avoid enumeration races.
- **Disposal** – Once `Dispose` has been called, any further attempt to schedule, query, or modify tasks results in an `ObjectDisposedException`. Calling `Dispose` while a task delegate is currently executing does not interrupt that execution; the delegate will run to completion, after which the manager’s resources are released.
- **Identifier handling** – If `id` is omitted, the manager generates a unique identifier based on `Guid.NewGuid()`. Supplying an explicit ID that already exists will replace the existing task (the former task is cancelled automatically).
- **Error tracking** – When a task’s delegate throws, the manager records the exception message in `LastError`, updates `LastErrorAt` to the current UTC time, and increments `ExecutionCount`. The task continues to run according to its schedule unless explicitly cancelled.
- **Recurring tasks with `runOnStart`** – Setting `runOnStart: true` causes the delegate to be invoked synchronously during the call to `ScheduleRecurring` before the first interval timer starts. Any exception thrown in this initial run is treated like any other execution error.
- **Memory usage** – The manager retains a reference to each `ScheduledTask` until it is cancelled or the manager is disposed. Long‑running applications should cancel tasks that are no longer needed to prevent unbounded growth.
