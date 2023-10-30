// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.BackgroundWorkers;

/// <summary>
/// Manages scheduled and recurring background tasks using timers.
/// Supports one-time execution and recurring schedules with customizable intervals.
/// </summary>
public class ScheduledTaskManager : IDisposable
{
    private readonly Dictionary<string, ScheduledTask> _scheduledTasks = new();
    private readonly ILogger<ScheduledTaskManager> _logger;
    private readonly object _lockObj = new();

    public ScheduledTaskManager(ILogger<ScheduledTaskManager>? logger = null)
    {
        _logger = logger ?? new ConsoleLogger<ScheduledTaskManager>();
    }

    /// <summary>
    /// Schedules a one-time task to run after a specific delay.
    /// </summary>
    public string ScheduleOnce(Func<Task> taskFunc, TimeSpan delay, string? taskName = null)
    {
        var id = Guid.NewGuid().ToString();
        taskName ??= $"OneTimeTask_{id[..8]}";

        var task = new ScheduledTask
        {
            Id = id,
            Name = taskName,
            TaskFunc = taskFunc,
            IsRecurring = false,
            Interval = delay,
            CreatedAt = DateTime.UtcNow
        };

        var timer = new System.Timers.Timer(delay.TotalMilliseconds)
        {
            AutoReset = false,
            Enabled = true
        };

        timer.Elapsed += async (_, _) => await ExecuteTaskAsync(task, timer);

        lock (_lockObj)
        {
            task.Timer = timer;
            _scheduledTasks[id] = task;
        }

        _logger.LogInformation("One-time task scheduled: {TaskName} (ID: {TaskId}), will run in {DelayMs}ms",
            taskName, id, delay.TotalMilliseconds);

        return id;
    }

    /// <summary>
    /// Schedules a recurring task to run at regular intervals.
    /// </summary>
    public string ScheduleRecurring(Func<Task> taskFunc, TimeSpan interval, string? taskName = null)
    {
        if (interval.TotalMilliseconds < 100)
        {
            _logger.LogWarning("Scheduled task interval is very short ({IntervalMs}ms), this may cause performance issues",
                interval.TotalMilliseconds);
        }

        var id = Guid.NewGuid().ToString();
        taskName ??= $"RecurringTask_{id[..8]}";

        var task = new ScheduledTask
        {
            Id = id,
            Name = taskName,
            TaskFunc = taskFunc,
            IsRecurring = true,
            Interval = interval,
            CreatedAt = DateTime.UtcNow
        };

        var timer = new System.Timers.Timer(interval.TotalMilliseconds)
        {
            AutoReset = true,
            Enabled = true
        };

        timer.Elapsed += async (_, _) => await ExecuteTaskAsync(task, timer);

        lock (_lockObj)
        {
            task.Timer = timer;
            _scheduledTasks[id] = task;
        }

        _logger.LogInformation("Recurring task scheduled: {TaskName} (ID: {TaskId}), interval: {IntervalMs}ms",
            taskName, id, interval.TotalMilliseconds);

        return id;
    }

    /// <summary>
    /// Cancels a scheduled task by ID.
    /// </summary>
    public bool CancelTask(string taskId)
    {
        lock (_lockObj)
        {
            if (_scheduledTasks.TryGetValue(taskId, out var task))
            {
                task.Timer?.Stop();
                task.Timer?.Dispose();
                _scheduledTasks.Remove(taskId);

                _logger.LogInformation("Task cancelled: {TaskName} (ID: {TaskId})", task.Name, taskId);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets all scheduled tasks.
    /// </summary>
    public IEnumerable<ScheduledTask> GetAllTasks()
    {
        lock (_lockObj)
        {
            return _scheduledTasks.Values.ToList();
        }
    }

    /// <summary>
    /// Gets a scheduled task by ID.
    /// </summary>
    public ScheduledTask? GetTask(string taskId)
    {
        lock (_lockObj)
        {
            _scheduledTasks.TryGetValue(taskId, out var task);
            return task;
        }
    }

    /// <summary>
    /// Stops all scheduled tasks.
    /// </summary>
    public void StopAll()
    {
        lock (_lockObj)
        {
            foreach (var task in _scheduledTasks.Values)
            {
                task.Timer?.Stop();
                task.Timer?.Dispose();
            }

            _scheduledTasks.Clear();
        }

        _logger.LogInformation("All scheduled tasks stopped");
    }

    private async Task ExecuteTaskAsync(ScheduledTask task, System.Timers.Timer timer)
    {
        try
        {
            task.LastExecutedAt = DateTime.UtcNow;
            task.ExecutionCount++;

            _logger.LogDebug("Executing scheduled task: {TaskName} (ID: {TaskId}, Execution #{Count})",
                task.Name, task.Id, task.ExecutionCount);

            if (task.TaskFunc != null)
            {
                await task.TaskFunc();
            }

            task.LastSuccessAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            task.LastErrorAt = DateTime.UtcNow;
            task.LastError = ex.Message;

            _logger.LogError(ex, "Error executing scheduled task: {TaskName} (ID: {TaskId})",
                task.Name, task.Id);
        }
        finally
        {
            // Stop one-time tasks after execution
            if (!task.IsRecurring)
            {
                timer.Stop();
                lock (_lockObj)
                {
                    _scheduledTasks.Remove(task.Id);
                }
            }
        }
    }

    public void Dispose()
    {
        StopAll();
    }
}

/// <summary>
/// Represents a scheduled task.
/// </summary>
public class ScheduledTask
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Func<Task>? TaskFunc { get; set; }
    public bool IsRecurring { get; set; }
    public TimeSpan Interval { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastExecutedAt { get; set; }
    public DateTime? LastSuccessAt { get; set; }
    public DateTime? LastErrorAt { get; set; }
    public string? LastError { get; set; }
    public int ExecutionCount { get; set; }

    internal System.Timers.Timer? Timer { get; set; }
}
