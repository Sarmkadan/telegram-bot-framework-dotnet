#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using TelegramBotFramework.Integration;

namespace TelegramBotFramework.BackgroundWorkers;

/// <summary>
/// Background task worker for executing long-running operations without blocking requests.
/// Uses a queue to manage tasks and workers for execution.
/// </summary>
public sealed class BackgroundTaskWorker : IBackgroundTaskWorker, IDisposable, IEquatable<BackgroundTaskWorker>
{
    private readonly Queue<BackgroundTask> _taskQueue = new();
    private readonly SemaphoreSlim _taskAvailable;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly ILogger<BackgroundTaskWorker> _logger;
    private readonly int _maxConcurrentTasks;
    private int _runningTasks = 0;
    private Task? _workerTask;

    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Func<CancellationToken, Task>? TaskFunc { get; set; }
    public DateTime QueuedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int QueuedTaskCount { get; set; }
    public int RunningTaskCount { get; set; }
    public int MaxConcurrentTasks { get; set; }

    public BackgroundTaskWorker(int maxConcurrentTasks = 4, ILogger<BackgroundTaskWorker>? logger = null)
    {
        _maxConcurrentTasks = maxConcurrentTasks;
        _logger = logger ?? new ConsoleLogger<BackgroundTaskWorker>();
        _taskAvailable = new SemaphoreSlim(0);
        _cancellationTokenSource = new CancellationTokenSource();
    }

    /// <summary>
    /// Queues a background task for execution.
    /// </summary>
    public void QueueTask(Func<CancellationToken, Task> taskFunc, string taskName = "UnnamedTask")
    {
        if (taskFunc  is null)
            throw new ArgumentNullException(nameof(taskFunc));

        var task = new BackgroundTask
        {
            Id = Guid.NewGuid().ToString(),
            Name = taskName,
            TaskFunc = taskFunc,
            QueuedAt = DateTime.UtcNow
        };

        lock (_taskQueue)
        {
            _taskQueue.Enqueue(task);
        }

        _taskAvailable.Release();
        _logger.LogInformation("Background task queued: {TaskName} (ID: {TaskId})", taskName, task.Id);
    }

    /// <summary>
    /// Starts the background worker.
    /// </summary>
    public void Start()
    {
        if (_workerTask  is not null && !_workerTask.IsCompleted)
        {
            _logger.LogWarning("Background worker is already running");
            return;
        }

        _workerTask = Task.Run(() => ProcessTasksAsync(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
        _logger.LogInformation("Background task worker started with max {MaxConcurrent} concurrent tasks", _maxConcurrentTasks);
    }

    /// <summary>
    /// Stops the background worker gracefully.
    /// </summary>
    public async Task StopAsync(TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(30);

        _logger.LogInformation("Stopping background task worker...");
        _cancellationTokenSource.Cancel();

        if (_workerTask  is not null)
        {
            try
            {
                await _workerTask.WaitAsync(timeout.Value).ConfigureAwait(false);
            }
            catch (TimeoutException)
            {
                _logger.LogWarning("Background worker did not stop within timeout period");
            }
        }

        _logger.LogInformation("Background task worker stopped");
    }

    /// <summary>
    /// Gets current worker statistics.
    /// </summary>
    public WorkerStatistics GetStatistics()
    {
        lock (_taskQueue)
        {
            return new WorkerStatistics
            {
                QueuedTaskCount = _taskQueue.Count,
                RunningTaskCount = _runningTasks,
                MaxConcurrentTasks = _maxConcurrentTasks
            };
        }
    }

    private async Task ProcessTasksAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Wait for a task to be available
                await _taskAvailable.WaitAsync(cancellationToken).ConfigureAwait(false);

                BackgroundTask? task = null;
                lock (_taskQueue)
                {
                    if (_taskQueue.Count > 0)
                    {
                        task = _taskQueue.Dequeue();
                    }
                }

                if (task  is not null && _runningTasks < _maxConcurrentTasks)
                {
                    Interlocked.Increment(ref _runningTasks);

                    // Fire and forget, but log errors
                    _ = ExecuteTaskSafelyAsync(task, cancellationToken).ContinueWith(_ =>
                    {
                        Interlocked.Decrement(ref _runningTasks);
                    });
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in background task processing");
            }
        }
    }

    private async Task ExecuteTaskSafelyAsync(BackgroundTask task, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Executing background task: {TaskName} (ID: {TaskId})", task.Name, task.Id);
            task.StartedAt = DateTime.UtcNow;

            if (task.TaskFunc is null)
                throw new InvalidOperationException($"Background task '{task.Name}' does not have a task delegate.");

            await task.TaskFunc(cancellationToken).ConfigureAwait(false);

            task.CompletedAt = DateTime.UtcNow;
            _logger.LogInformation("Background task completed: {TaskName} (ID: {TaskId}, Duration: {DurationMs}ms)",
                task.Name, task.Id, (task.CompletedAt.Value - task.StartedAt.Value).TotalMilliseconds);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Background task cancelled: {TaskName} (ID: {TaskId})", task.Name, task.Id);
        }
        catch (Exception ex)
        {
            task.CompletedAt = DateTime.UtcNow;
            _logger.LogError(ex, "Error executing background task: {TaskName} (ID: {TaskId})", task.Name, task.Id);
        }
    }

    public void Dispose()
    {
        _taskAvailable?.Dispose();
        _cancellationTokenSource?.Dispose();
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.</returns>
    public bool Equals(BackgroundTaskWorker? other)
    {
        if (other is null)
            return false;

        return Id == other.Id
            && Name == other.Name
            && EqualityComparer<Func<CancellationToken, Task>?>.Default.Equals(TaskFunc, other.TaskFunc)
            && QueuedAt == other.QueuedAt
            && StartedAt == other.StartedAt
            && CompletedAt == other.CompletedAt
            && QueuedTaskCount == other.QueuedTaskCount
            && RunningTaskCount == other.RunningTaskCount;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as BackgroundTaskWorker);
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name, TaskFunc, QueuedAt, StartedAt, CompletedAt, QueuedTaskCount, RunningTaskCount);
    }

    /// <summary>
    /// Determines whether two specified objects are equal.
    /// </summary>
    /// <param name="left">The first object to compare.</param>
    /// <param name="right">The second object to compare.</param>
    /// <returns>true if the specified objects are equal; otherwise, false.</returns>
    public static bool operator ==(BackgroundTaskWorker? left, BackgroundTaskWorker? right)
    {
        return EqualityComparer<BackgroundTaskWorker>.Default.Equals(left, right);
    }

    /// <summary>
    /// Determines whether two specified objects are not equal.
    /// </summary>
    /// <param name="left">The first object to compare.</param>
    /// <param name="right">The second object to compare.</param>
    /// <returns>true if the specified objects are not equal; otherwise, false.</returns>
    public static bool operator !=(BackgroundTaskWorker? left, BackgroundTaskWorker? right)
    {
        return !(left == right);
    }
}

/// <summary>
/// Represents a background task to be executed.
/// </summary>
public sealed class BackgroundTask
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Func<CancellationToken, Task>? TaskFunc { get; set; }
    public DateTime QueuedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

/// <summary>
/// Statistics about the background task worker.
/// </summary>
public sealed class WorkerStatistics
{
    public int QueuedTaskCount { get; set; }
    public int RunningTaskCount { get; set; }
    public int MaxConcurrentTasks { get; set; }
}
