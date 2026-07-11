# ScheduledTaskManagerExtensions

Provides extension members for working with scheduled tasks in the Telegram Bot Framework for .NET. These members allow you to schedule tasks, query their state, retrieve statistics, and manage cancellation in a fluent, LINQ‑friendly manner.

## API

### ScheduleAt
```csharp
public static string ScheduleAt
```
**Purpose:** Returns a predefined schedule expression string that represents a one‑time execution at a specific point in time.  
**Return value:** A string suitable for passing to scheduling APIs.  
**Throws:** None.

### ScheduleDailyAt
```csharp
public static string ScheduleDailyAt
```
**Purpose:** Returns a predefined schedule expression string that represents a daily recurrence at a specific time of day.  
**Return value:** A string suitable for passing to scheduling APIs.  
**Throws:** None.

### GetTasksWhere
```csharp
public static IEnumerable<ScheduledTask> GetTasksWhere
```
**Purpose:** Produces an enumerable of `ScheduledTask` objects that satisfy the internal filter logic of the method.  
**Return value:** An `IEnumerable<ScheduledTask>` containing the matching tasks.  
**Throws:** None.

### GetTaskByName
```csharp
public static ScheduledTask? GetTaskByName
```
**Purpose:** Attempts to locate a scheduled task by its identifier.  
**Return value:** The `ScheduledTask` with the requested name, or `null` if no such task exists.  
**Throws:** None.

### GetStatistics
```csharp
public static TaskStatistics GetStatistics
```
**Purpose:** Retrieves aggregated statistics about all scheduled tasks.  
**Return value:** A `TaskStatistics` instance containing counts and timing information.  
**Throws:** None.

### WaitForCompletionAsync
```csharp
public static async Task<bool> WaitForCompletionAsync
```
**Purpose:** Asynchronously waits until all currently tracked tasks have finished execution.  
**Return value:** `true` if all tasks completed successfully; `false` if the wait was terminated early or an error occurred.  
**Throws:** May propagate `OperationCanceledException` if the waiting token is canceled, or any exception thrown by the underlying task execution logic.

### CancelTasksWhere
```csharp
public static int CancelTasksWhere
```
**Purpose:** Cancels all tasks that meet the internal cancellation predicate and returns the number of tasks that were cancelled.  
**Return value:** An `int` indicating how many tasks were cancelled.  
**Throws:** None.

### GetFailedTasks
```csharp
public static IEnumerable<ScheduledTask> GetFailedTasks
```
**Purpose:** Returns a collection of tasks that have entered a failed state.  
**Return value:** An `IEnumerable<ScheduledTask>` containing the failed tasks.  
**Throws:** None.

### GetOverdueTasks
```csharp
public static IEnumerable<ScheduledTask> GetOverdueTasks
```
**Purpose:** Returns a collection of tasks whose scheduled execution time has passed without being run.  
**Return value:** An `IEnumerable<ScheduledTask>` containing the overdue tasks.  
**Throws:** None.

### TotalTasks
```csharp
public int TotalTasks
```
**Purpose:** Gets the total number of tasks currently managed by the associated `ScheduledTaskManager`.  
**Return value:** An `int` representing the count of all tasks.  
**Throws:** None.

### RunningTasks
```csharp
public int RunningTasks
```
**Purpose:** Gets the number of tasks that are currently executing.  
**Return value:** An `int` representing the count of running tasks.  
**Throws:** None.

### FailedTasks
```csharp
public int FailedTasks
```
**Purpose:** Gets the number of tasks that have failed since the manager was created or last reset.  
**Return value:** An `int` representing the count of failed tasks.  
**Throws:** None.

### SuccessfulTasks
```csharp
public int SuccessfulTasks
```
**Purpose:** Gets the number of tasks that have completed successfully.  
**Return value:** An `int` representing the count of successful tasks.  
**Throws:** None.

### TotalExecutions
```csharp
public int TotalExecutions
```
**Purpose:** Gets the total number of task executions that have occurred (including retries).  
**Return value:** An `int` representing the cumulative execution count.  
**Throws:** None.

### Tasks
```csharp
public IReadOnlyList<ScheduledTask> Tasks
```
**Purpose:** Provides read‑only access to the collection of all scheduled tasks.  
**Return value:** An `IReadOnlyList<ScheduledTask>` that reflects the current set of tasks.  
**Throws:** None.

## Usage

### Scheduling a daily task and awaiting completion
```csharp
var manager = new ScheduledTaskManager();

// Schedule a task to run every day at 02:30 AM
manager.ScheduleJob(
    "DailyBackup",
    ScheduledTaskManagerExtensions.ScheduleDailyAt,
    () => BackupDatabase());

// Wait for all scheduled tasks to finish (useful during shutdown)
bool allDone = await ScheduledTaskManagerExtensions.WaitForCompletionAsync;
if (!allDone)
{
    Logger.Warn("Some tasks did not complete before shutdown.");
}
```

### Querying failed tasks and cancelling overdue work
```csharp
var manager = new ScheduledTaskManager();
// ... schedule various tasks ...

// Retrieve tasks that have failed
IEnumerable<ScheduledTask> failed = ScheduledTaskManagerExtensions.GetFailedTasks;
foreach (var task in failed)
{
    Logger.Error($"Task '{task.Name}' failed: {task.LastError}");
}

// Cancel any tasks that are overdue (past their scheduled start time)
int cancelled = ScheduledTaskManagerExtensions.CancelTasksWhere;
Logger.Info($"Cancelled {cancelled} overdue tasks.");
```

## Notes

- The static extension members operate on the implicit `ScheduledTaskManager` instance on which they are invoked; they do not maintain internal state themselves.  
- Instance properties (`TotalTasks`, `RunningTasks`, etc.) reflect a snapshot of the manager’s state at the moment of access. Concurrent modifications to the task collection may cause these values to change between reads.  
- Enumerating the collections returned by `GetTasksWhere`, `GetFailedTasks`, or `GetOverdueTasks` while the underlying task list is being modified can result in undefined behavior; it is advisable to synchronize access or materialize the results (e.g., `.ToList()`) before iteration.  
- `WaitForCompletionAsync` does not cancel running tasks; it merely awaits their natural completion. If a timeout or cancellation is required, combine it with a `CancellationTokenSource` and handle `OperationCanceledException` appropriately.  
- The strings returned by `ScheduleAt` and `ScheduleDailyAt` are intended to be consumed by the framework’s scheduling parsers; altering their format may lead to scheduling failures.  
- All members are thread‑safe for concurrent read operations; write operations (e.g., scheduling, cancelling) should be guarded by external synchronization if multiple threads may invoke them simultaneously on the same manager instance.
