using System;
using System.Collections.Generic;
using TelegramBotFramework.BackgroundWorkers;

namespace TelegramBotFramework.BackgroundWorkers;

/// <summary>
/// Builder for <see cref="TaskStatistics"/>.
/// </summary>
public sealed class ScheduledTaskManagerExtensionsBuilder
{
    private readonly TaskStatistics _instance = new();

    /// <summary>
    /// Sets the total tasks count.
    /// </summary>
    /// <param name="totalTasks">The total tasks count.</param>
    /// <returns>The builder instance.</returns>
    public ScheduledTaskManagerExtensionsBuilder WithTotalTasks(int totalTasks)
    {
        _instance.TotalTasks = totalTasks;
        return this;
    }

    /// <summary>
    /// Sets the running tasks count.
    /// </summary>
    /// <param name="runningTasks">The running tasks count.</param>
    /// <returns>The builder instance.</returns>
    public ScheduledTaskManagerExtensionsBuilder WithRunningTasks(int runningTasks)
    {
        _instance.RunningTasks = runningTasks;
        return this;
    }

    /// <summary>
    /// Sets the failed tasks count.
    /// </summary>
    /// <param name="failedTasks">The failed tasks count.</param>
    /// <returns>The builder instance.</returns>
    public ScheduledTaskManagerExtensionsBuilder WithFailedTasks(int failedTasks)
    {
        _instance.FailedTasks = failedTasks;
        return this;
    }

    /// <summary>
    /// Sets the successful tasks count.
    /// </summary>
    /// <param name="successfulTasks">The successful tasks count.</param>
    /// <returns>The builder instance.</returns>
    public ScheduledTaskManagerExtensionsBuilder WithSuccessfulTasks(int successfulTasks)
    {
        _instance.SuccessfulTasks = successfulTasks;
        return this;
    }

    /// <summary>
    /// Sets the total executions count.
    /// </summary>
    /// <param name="totalExecutions">The total executions count.</param>
    /// <returns>The builder instance.</returns>
    public ScheduledTaskManagerExtensionsBuilder WithTotalExecutions(int totalExecutions)
    {
        _instance.TotalExecutions = totalExecutions;
        return this;
    }

    /// <summary>
    /// Sets the tasks collection.
    /// </summary>
    /// <param name="tasks">The tasks collection.</param>
    /// <returns>The builder instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tasks"/> is <see langword="null"/>.</exception>
    public ScheduledTaskManagerExtensionsBuilder WithTasks(IReadOnlyList<ScheduledTask> tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks);
        _instance.Tasks = tasks;
        return this;
    }

    /// <summary>
    /// Builds a configured <see cref="TaskStatistics"/> instance.
    /// </summary>
    /// <returns>The configured <see cref="TaskStatistics"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown if required properties are missing.</exception>
    public TaskStatistics Build()
    {
        if (_instance.Tasks == null)
        {
            throw new ArgumentException("Tasks collection is required.", nameof(_instance.Tasks));
        }
        return _instance;
    }

    /// <summary>
    /// Creates a builder from an existing <see cref="TaskStatistics"/> instance.
    /// </summary>
    /// <param name="template">The template instance to copy from.</param>
    /// <returns>A new builder instance pre-filled from the template.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="template"/> is <see langword="null"/>.</exception>
    public static ScheduledTaskManagerExtensionsBuilder From(TaskStatistics template)
    {
        ArgumentNullException.ThrowIfNull(template);
        var builder = new ScheduledTaskManagerExtensionsBuilder();
        builder._instance.TotalTasks = template.TotalTasks;
        builder._instance.RunningTasks = template.RunningTasks;
        builder._instance.FailedTasks = template.FailedTasks;
        builder._instance.SuccessfulTasks = template.SuccessfulTasks;
        builder._instance.TotalExecutions = template.TotalExecutions;
        builder._instance.Tasks = template.Tasks;
        return builder;
    }
}
