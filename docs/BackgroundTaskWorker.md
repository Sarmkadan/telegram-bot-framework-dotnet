# BackgroundTaskWorker

The `BackgroundTaskWorker` type provides a simple, reusable component for executing asynchronous work items with configurable concurrency limits. It maintains an internal queue, starts a configurable number of parallel workers, and exposes statistics and lifecycle controls for monitoring and graceful shutdown.

## API

### BackgroundTaskWorker()
Initializes a new instance of `BackgroundTaskWorker`. The instance is created in a stopped state with default values for `Id`, `Name`, and `MaxConcurrentTasks`. These values should be configured before calling `Start`.  
**Throws:**  
- `ObjectDisposedException` if the instance has already been disposed (cannot happen immediately after construction but is noted for completeness).

### QueueTask(Func<CancellationToken, Task> task)
Enqueues a user‑provided asynchronous operation for later execution. The delegate receives a `CancellationToken` that is signaled when `StopAsync` is invoked.  
**Parameters**  
- `task`: The work to perform; must not be `null`.  
**Return:** `void`.  
**Throws:**  
- `ArgumentNullException` if `task` is `null`.  
- `ObjectDisposedException` if the worker has been disposed.  
- `InvalidOperationException` if `Start` has not been called.

### Start()
Begins processing queued tasks according to the current `MaxConcurrentTasks` setting. After this method returns, the worker will dequeue and execute tasks until `StopAsync` is called or the worker is disposed.  
**Parameters:** none.  
**Return:** `void`.  
**Throws:**  
- `InvalidOperationException` if the worker is already started.  
- `ObjectDisposedException` if the worker has been disposed.

### StopAsync()
Signals the worker to stop accepting new tasks, waits for all currently running tasks to complete, and then transitions the worker to a stopped state.  
**Parameters:** none.  
**Return:** A `Task` that completes when the worker has fully stopped.  
**Throws:**  
- `ObjectDisposedException` if the worker has been disposed.

### GetStatistics()
Returns a snapshot of the worker’s current runtime statistics.  
**Parameters:** none.  
**Return:** A `WorkerStatistics` instance containing the latest values for queued, running, and completed task counts, as well as timestamps.  
**Throws:**  
- `ObjectDisuredException` if the worker has been disposed.

### Dispose()
Releases any unmanaged resources and cancels any pending operations. After disposal, the worker cannot be restarted.  
**Parameters:** none.  
**Return:** `void`.  
**Throws:** none.

### Id
Gets or sets a unique identifier for the worker instance. It is intended for logging or diagnostics and should be set before calling `Start`. Changing the value after `Start` has no effect on ongoing operations.  
**Type:** `string`.

### Name
Gets or sets a descriptive name for the worker. Like `Id`, it is used for diagnostics and should be configured prior to `Start`.  
**Type:** `string`.

### TaskFunc
Gets or sets the default delegate used when a task is queued without explicitly providing one (if the API permits overloads). If `null`, calling `QueueTask` with a missing delegate will result in an `ArgumentNullException`.  
**Type:** `Func<CancellationToken, Task>?`.

### QueuedAt
Gets the UTC date and time when the worker instance was created (or when the first task was queued, depending on implementation). This value is immutable after construction.  
**Type:** `DateTime`.

### StartedAt
Gets the UTC date and time when `Start()` was invoked, or `null` if the worker has not yet been started.  
**Type:** `DateTime?`.

### CompletedAt
Gets the UTC date and time when `StopAsync()` completed, or `null` if the worker is still running or has not been stopped.  
**Type:** `DateTime?`.

### QueuedTaskCount
Gets the current number of tasks waiting in the internal queue to be executed.  
**Type:** `int`.

### RunningTaskCount
Gets the current number of tasks that are actively executing.  
**Type:** `int`.

### MaxConcurrentTasks
Gets or sets the maximum number of tasks that may run concurrently. The value must be greater than zero. Changing this property while the worker is started affects only newly queued tasks; already running tasks continue unaffected.  
**Type:** `int`.

## Usage

### Example 1: Basic worker with default settings
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;

// Create a worker, give it an identity, and start it.
var worker = new BackgroundTaskWorker
{
    Id = "worker-01",
    Name = "DemoWorker",
    MaxConcurrentTasks = 3
};
worker.Start();

// Queue some simple work.
for (int i = 0; i < 10; i++)
{
    int iteration = i;
    worker.QueueTask(_ =>
    {
        Console.WriteLine($"Task {iteration} started on {Thread.CurrentThread.ManagedThreadId}");
        // Simulate work.
        return Task.Delay(TimeSpan.FromSeconds(2));
    });
}

// Allow time for processing, then stop gracefully.
await Task.Delay(TimeSpan.FromSeconds(30));
await worker.StopAsync();
worker.Dispose();
```

### Example 2: Custom task function and statistics monitoring
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;

// Define a reusable task delegate.
Func<CancellationToken, Task> processItem = async token =>
{
    // Example: fetch data from a remote service.
    await Task.Delay(TimeSpan.FromMilliseconds(500), token);
    // Simulate occasional failure.
    if (Random.Shared.NextDouble() < 0.1)
        throw new InvalidOperationException("Random failure");
};

var worker = new BackgroundTaskWorker
{
    Id = "stats-worker",
    Name = "StatisticsExample",
    MaxConcurrentTasks = 5,
    TaskFunc = processItem   // optional default
};
worker.Start();

// Queue tasks using the default TaskFunc.
for (int i = 0; i < 20; i++)
{
    worker.QueueTask(null); // uses worker.TaskFunc
}

// Periodically check statistics while work proceeds.
while (worker.RunningTaskCount > 0 || worker.QueuedTaskCount > 0)
{
    var stats = worker.GetStatistics();
    Console.WriteLine(
        $"Queued: {stats.QueuedTaskCount}, Running: {stats.RunningTaskCount}, " +
        $"Completed: {stats.CompletedTaskCount}");
    await Task.Delay(TimeSpan.FromSeconds(2));
}

await worker.StopAsync();
worker.Dispose();
```

## Notes
- **Thread safety:** All public methods (`QueueTask`, `Start`, `StopAsync`, `Dispose`, `GetStatistics`) are safe to call from multiple threads concurrently. Property reads are also thread‑safe; however, properties that are intended to be configured (`Id`, `Name`, `MaxConcurrentTasks`, `TaskFunc`) should only be modified before `Start` is invoked, as changes after startup may be ignored or cause undefined behavior.
- **Exception handling:** Exceptions thrown inside a user‑provided task are captured internally and do not cause the worker to stop; they are made available via the `WorkerStatistics` (if the implementation exposes failed counts) or can be observed through any logging mechanism you attach.
- **Disposal:** Calling `Dispose` while tasks are executing will cancel pending operations via the internal cancellation token; in‑flight tasks will receive a cancellation request and should observe it to terminate promptly.
- **Maximum concurrency:** Setting `MaxConcurrentTasks` to a value less than 1 results in an `ArgumentOutOfRangeException` when the property is set (if validated). The default value is implementation‑specific but is intended to be a sensible fallback (e.g., `Environment.ProcessorCount`).
- **Timestamps:** `QueuedAt` is set at construction; `StartedAt` is set when `Start` returns successfully; `CompletedAt` is set when the task returned by `StopAsync` completes. If `StopAsync` is never called, `CompletedAt` remains `null`.
