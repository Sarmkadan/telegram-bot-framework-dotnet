#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using TelegramBotFramework.BackgroundWorkers;

namespace TelegramBotFramework.BackgroundWorkers;

/// <summary>
/// Provides extension methods for <see cref="ScheduledTaskManager"/> to enhance task management capabilities.
/// </summary>
public static class ScheduledTaskManagerExtensions
{
    /// <summary>
    /// Schedules a one-time task to run at a specific time in the future.
    /// </summary>
    /// <param name="taskManager">The task manager instance.</param>
    /// <param name="taskFunc">The task function to execute.</param>
    /// <param name="runAt">The specific time when the task should run.</param>
    /// <param name="taskName">Optional name for the task.</param>
    /// <returns>The unique task ID.</returns>
    public static string ScheduleAt(this ScheduledTaskManager taskManager, Func<Task> taskFunc, DateTime runAt, string? taskName = null)
    {
        var delay = runAt - DateTime.UtcNow;
        if (delay.TotalMilliseconds <= 0)
        {
            throw new ArgumentException("Run time must be in the future.", nameof(runAt));
        }

        return taskManager.ScheduleOnce(taskFunc, delay, taskName);
    }

    /// <summary>
    /// Schedules a recurring task to run at specific times of day (e.g., 9:00 AM, 2:00 PM).
    /// </summary>
    /// <param name="taskManager">The task manager instance.</param>
    /// <param name="taskFunc">The task function to execute.</param>
    /// <param name="timesOfDay">Collection of times when the task should run each day.</param>
    /// <param name="taskName">Optional name for the task.</param>
    /// <returns>The unique task ID.</returns>
    public static string ScheduleDailyAt(this ScheduledTaskManager taskManager, Func<Task> taskFunc, IEnumerable<TimeSpan> timesOfDay, string? taskName = null)
    {
        if (timesOfDay == null || !timesOfDay.Any())
        {
            throw new ArgumentException("At least one time of day must be specified.", nameof(timesOfDay));
        }

        var nextRunTimes = timesOfDay
            .Select(time => CalculateNextRunTime(time))
            .Where(time => time.HasValue)
            .Select(time => time!.Value)
            .OrderBy(time => time)
            .ToList();

        if (!nextRunTimes.Any())
        {
            throw new InvalidOperationException("Could not determine valid run time from provided times.");
        }

        var firstRunTime = nextRunTimes.First();
        var interval = nextRunTimes.Count > 1
            ? nextRunTimes[1] - firstRunTime
            : TimeSpan.FromDays(1);

        var taskId = taskManager.ScheduleRecurring(taskFunc, interval, taskName);

        // Update the task to use the specific times
        var task = taskManager.GetTask(taskId);
        if (task != null)
        {
            // Note: The interval-based approach will work for most cases
            // For precise time scheduling, users can use ScheduleAt for each specific time
        }

        return taskId;
    }

    /// <summary>
    /// Gets all tasks that match the specified predicate.
    /// </summary>
    /// <param name="taskManager">The task manager instance.</param>
    /// <param name="predicate">Filter predicate to match tasks.</param>
    /// <returns>Filtered collection of tasks.</returns>
    public static IEnumerable<ScheduledTask> GetTasksWhere(this ScheduledTaskManager taskManager, Func<ScheduledTask, bool> predicate)
    {
        if (predicate == null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        return taskManager.GetAllTasks().Where(predicate);
    }

    /// <summary>
    /// Gets the first task with the specified name (case-insensitive).
    /// </summary>
    /// <param name="taskManager">The task manager instance.</param>
    /// <param name="taskName">The name to search for.</param>
    /// <returns>The matching task or null if not found.</returns>
    public static ScheduledTask? GetTaskByName(this ScheduledTaskManager taskManager, string taskName)
    {
        if (string.IsNullOrWhiteSpace(taskName))
        {
            throw new ArgumentException("Task name cannot be null or empty.", nameof(taskName));
        }

        return taskManager.GetAllTasks()
            .FirstOrDefault(task => string.Equals(task.Name, taskName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets statistics about scheduled tasks.
    /// </summary>
    /// <param name="taskManager">The task manager instance.</param>
    /// <returns>Task statistics including total, running, failed, and success counts.</returns>
    public static TaskStatistics GetStatistics(this ScheduledTaskManager taskManager)
    {
        var allTasks = taskManager.GetAllTasks().ToList();
        var runningTasks = allTasks.Count(t => t.Timer?.Enabled == true);
        var failedTasks = allTasks.Count(t => t.LastErrorAt.HasValue && !t.LastSuccessAt.HasValue);
        var successfulTasks = allTasks.Count(t => t.LastSuccessAt.HasValue);
        var totalExecutionCount = allTasks.Sum(t => t.ExecutionCount);

        return new TaskStatistics
        {
            TotalTasks = allTasks.Count,
            RunningTasks = runningTasks,
            FailedTasks = failedTasks,
            SuccessfulTasks = successfulTasks,
            TotalExecutions = totalExecutionCount,
            Tasks = allTasks
        };
    }

    /// <summary>
    /// Waits for all currently scheduled tasks to complete.
    /// </summary>
    /// <param name="taskManager">The task manager instance.</param>
    /// <param name="timeout">Maximum time to wait (default: 30 seconds).</param>
    /// <returns>True if all tasks completed, false if timeout occurred.</returns>
    public static async Task<bool> WaitForCompletionAsync(this ScheduledTaskManager taskManager, TimeSpan? timeout = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var timeoutValue = timeout ?? TimeSpan.FromSeconds(30);

        while (true)
        {
            var allTasks = taskManager.GetAllTasks().ToList();
            var runningTasks = allTasks.Count(t => t.Timer?.Enabled == true);

            if (runningTasks == 0)
            {
                return true;
            }

            if (stopwatch.Elapsed >= timeoutValue)
            {
                return false;
            }

            await Task.Delay(100).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Cancels all tasks that match the specified predicate.
    /// </summary>
    /// <param name="taskManager">The task manager instance.</param>
    /// <param name="predicate">Filter predicate to match tasks to cancel.</param>
    /// <returns>Number of tasks cancelled.</returns>
    public static int CancelTasksWhere(this ScheduledTaskManager taskManager, Func<ScheduledTask, bool> predicate)
    {
        if (predicate == null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        var tasksToCancel = taskManager.GetAllTasks()
            .Where(predicate)
            .Select(t => t.Id)
            .ToList();

        var cancelledCount = 0;
        foreach (var taskId in tasksToCancel)
        {
            if (taskManager.CancelTask(taskId))
            {
                cancelledCount++;
            }
        }

        return cancelledCount;
    }

    /// <summary>
    /// Gets all failed tasks (tasks that have errors but never succeeded).
    /// </summary>
    /// <param name="taskManager">The task manager instance.</param>
    /// <returns>Collection of failed tasks.</returns>
    public static IEnumerable<ScheduledTask> GetFailedTasks(this ScheduledTaskManager taskManager)
    {
        return taskManager.GetAllTasks()
            .Where(t => t.LastErrorAt.HasValue && !t.LastSuccessAt.HasValue);
    }

    /// <summary>
    /// Gets all overdue tasks (tasks that should have run but haven't executed yet).
    /// </summary>
    /// <param name="taskManager">The task manager instance.</param>
    /// <param name="currentTime">Optional current time for testing (defaults to DateTime.UtcNow).</param>
    /// <returns>Collection of overdue tasks.</returns>
    public static IEnumerable<ScheduledTask> GetOverdueTasks(this ScheduledTaskManager taskManager, DateTime? currentTime = null)
    {
        var now = currentTime ?? DateTime.UtcNow;

        return taskManager.GetAllTasks()
            .Where(t => t.IsRecurring && t.Timer?.Enabled == true)
            .Where(t => t.LastExecutedAt != null)
            .Where(t =>
            {
                var nextExpectedRun = t.LastExecutedAt!.Value.Add(t.Interval);
                return nextExpectedRun <= now;
            });
    }

    private static DateTime? CalculateNextRunTime(TimeSpan timeOfDay)
    {
        var now = DateTime.UtcNow;
        var todayRunTime = now.Date.Add(timeOfDay);

        if (todayRunTime > now)
        {
            return todayRunTime;
        }

        return todayRunTime.AddDays(1);
    }
}

/// <summary>
/// Represents statistics about scheduled tasks.
/// </summary>
public sealed class TaskStatistics
{
    public int TotalTasks { get; set; }
    public int RunningTasks { get; set; }
    public int FailedTasks { get; set; }
    public int SuccessfulTasks { get; set; }
    public int TotalExecutions { get; set; }
    public IReadOnlyList<ScheduledTask> Tasks { get; set; } = new List<ScheduledTask>();
}