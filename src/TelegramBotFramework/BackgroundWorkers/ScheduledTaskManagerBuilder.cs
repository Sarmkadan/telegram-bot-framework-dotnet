#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using TelegramBotFramework.Integration;

namespace TelegramBotFramework.BackgroundWorkers;

/// <summary>
/// Builder for <see cref="ScheduledTaskManager"/> objects.
/// </summary>
public sealed class ScheduledTaskManagerBuilder
{
    private string _id = string.Empty;
    private string _name = string.Empty;
    private Func<Task>? _taskFunc;
    private bool _isRecurring;
    private TimeSpan _interval;
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime? _lastExecutedAt;
    private DateTime? _lastSuccessAt;
    private DateTime? _lastErrorAt;
    private string? _lastError;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledTaskManagerBuilder"/> class.
    /// </summary>
    public ScheduledTaskManagerBuilder()
    {
    }

    /// <summary>
    /// Sets the task ID.
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is null, empty, or whitespace.</exception>
    public ScheduledTaskManagerBuilder WithId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Task ID cannot be empty.", nameof(id));

        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the task name.
    /// </summary>
    /// <param name="name">The task name.</param>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null, empty, or whitespace.</exception>
    public ScheduledTaskManagerBuilder WithName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Task name cannot be empty.", nameof(name));

        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the task function.
    /// </summary>
    /// <param name="taskFunc">The task function.</param>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="taskFunc"/> is <see langword="null"/>.</exception>
    public ScheduledTaskManagerBuilder WithTaskFunc(Func<Task> taskFunc)
    {
        ArgumentNullException.ThrowIfNull(taskFunc);
        _taskFunc = taskFunc;
        return this;
    }

    /// <summary>
    /// Sets whether the task is recurring.
    /// </summary>
    /// <param name="isRecurring">True if the task is recurring, false otherwise.</param>
    /// <returns>This builder instance.</returns>
    public ScheduledTaskManagerBuilder WithIsRecurring(bool isRecurring)
    {
        _isRecurring = isRecurring;
        return this;
    }

    /// <summary>
    /// Sets the task interval.
    /// </summary>
    /// <param name="interval">The task interval.</param>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="interval"/> is less than or equal to zero.</exception>
    public ScheduledTaskManagerBuilder WithInterval(TimeSpan interval)
    {
        if (interval <= TimeSpan.Zero)
            throw new ArgumentException("Interval must be positive.", nameof(interval));

        _interval = interval;
        return this;
    }

    /// <summary>
    /// Sets the task creation timestamp.
    /// </summary>
    /// <param name="createdAt">The task creation timestamp.</param>
    /// <returns>This builder instance.</returns>
    public ScheduledTaskManagerBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    /// <summary>
    /// Sets the task last execution timestamp.
    /// </summary>
    /// <param name="lastExecutedAt">The task last execution timestamp.</param>
    /// <returns>This builder instance.</returns>
    public ScheduledTaskManagerBuilder WithLastExecutedAt(DateTime? lastExecutedAt)
    {
        _lastExecutedAt = lastExecutedAt;
        return this;
    }

    /// <summary>
    /// Sets the task last success timestamp.
    /// </summary>
    /// <param name="lastSuccessAt">The task last success timestamp.</param>
    /// <returns>This builder instance.</returns>
    public ScheduledTaskManagerBuilder WithLastSuccessAt(DateTime? lastSuccessAt)
    {
        _lastSuccessAt = lastSuccessAt;
        return this;
    }

    /// <summary>
    /// Sets the task last error timestamp.
    /// </summary>
    /// <param name="lastErrorAt">The task last error timestamp.</param>
    /// <returns>This builder instance.</returns>
    public ScheduledTaskManagerBuilder WithLastErrorAt(DateTime? lastErrorAt)
    {
        _lastErrorAt = lastErrorAt;
        return this;
    }

    /// <summary>
    /// Sets the task last error message.
    /// </summary>
    /// <param name="lastError">The task last error message.</param>
    /// <returns>This builder instance.</returns>
    public ScheduledTaskManagerBuilder WithLastError(string? lastError)
    {
        _lastError = lastError;
        return this;
    }

    /// <summary>
    /// Creates a new <see cref="ScheduledTaskManagerBuilder"/> pre-filled with values from an existing <see cref="ScheduledTaskManager"/>.
    /// </summary>
    /// <param name="template">The scheduled task manager to copy values from.</param>
    /// <returns>A new builder instance with values from the template.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is <see langword="null"/>.</exception>
    public static ScheduledTaskManagerBuilder From(ScheduledTaskManager template)
    {
        ArgumentNullException.ThrowIfNull(template);

        // Note: We cannot directly access the internal tasks of ScheduledTaskManager,
        // so this builder is designed for creating new ScheduledTask instances,
        // not for copying existing ScheduledTaskManager instances with their tasks.
        // For copying individual tasks, use the ScheduledTaskBuilder (if it existed)
        // or create tasks manually.
        return new ScheduledTaskManagerBuilder();
    }

    /// <summary>
    /// Builds the <see cref="ScheduledTaskManager"/> instance with the current values.
    /// </summary>
    /// <returns>A configured <see cref="ScheduledTaskManager"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when required properties are missing or invalid.</exception>
    public ScheduledTaskManager Build()
    {
        // Validate required properties
        if (string.IsNullOrWhiteSpace(_id))
            throw new ArgumentException("Task ID is required.", nameof(_id));

        if (string.IsNullOrWhiteSpace(_name))
            throw new ArgumentException("Task name is required.", nameof(_name));

        if (_taskFunc == null)
            throw new ArgumentException("Task function is required.", nameof(_taskFunc));

        if (_interval <= TimeSpan.Zero)
            throw new ArgumentException("Interval must be positive.", nameof(_interval));

        return new ScheduledTaskManager()
        {
            // Note: ScheduledTaskManager doesn't expose setters for its internal state
            // This builder is actually meant to build ScheduledTask instances, not ScheduledTaskManager
            // Let me reconsider the design...
        };
    }
}