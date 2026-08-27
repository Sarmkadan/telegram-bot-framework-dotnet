#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using TelegramBotFramework.Integration;

namespace TelegramBotFramework.BackgroundWorkers;

/// <summary>
/// Manages scheduled and recurring background tasks using timers.
/// Supports one-time execution and recurring schedules with customizable intervals.
/// </summary>
public interface IScheduledTaskManager
{
    /// <summary>
    /// Schedules a one-time task to run after a specific delay.
    /// </summary>
    string ScheduleOnce(Func<Task> taskFunc, TimeSpan delay, string? taskName = null);

    /// <summary>
    /// Schedules a recurring task to run at regular intervals.
    /// </summary>
    string ScheduleRecurring(Func<Task> taskFunc, TimeSpan interval, string? taskName = null);

    /// <summary>
    /// Cancels a scheduled task by ID.
    /// </summary>
    bool CancelTask(string taskId);

    /// <summary>
    /// Gets all scheduled tasks.
    /// </summary>
    IEnumerable<ScheduledTask> GetAllTasks();

    /// <summary>
    /// Gets a scheduled task by ID.
    /// </summary>
    ScheduledTask? GetTask(string taskId);

    /// <summary>
    /// Stops all scheduled tasks.
    /// </summary>
    void StopAll();

    /// <summary>
    /// Releases all resources used by the ScheduledTaskManager.
    /// </summary>
    void Dispose();
}