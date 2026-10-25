namespace TelegramBotFramework.BackgroundWorkers
{
    /// <summary>
    /// Defines a worker that executes queued background tasks with a bounded level of concurrency.
    /// </summary>
    public interface IBackgroundTaskWorker : IDisposable
    {
        /// <summary>
        /// Gets the unique identifier of the worker.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the human-readable name of the worker.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets or sets the task delegate associated with this worker, or <c>null</c> if none is set.
        /// </summary>
        Func<CancellationToken, Task>? TaskFunc { get; set; }

        /// <summary>
        /// Gets the UTC date and time when the worker was queued.
        /// </summary>
        DateTime QueuedAt { get; }

        /// <summary>
        /// Gets the UTC date and time when the worker started processing, or <c>null</c> if it has not started yet.
        /// </summary>
        DateTime? StartedAt { get; }

        /// <summary>
        /// Gets the UTC date and time when the worker completed, or <c>null</c> if it has not completed yet.
        /// </summary>
        DateTime? CompletedAt { get; }

        /// <summary>
        /// Gets the number of tasks currently waiting in the queue.
        /// </summary>
        int QueuedTaskCount { get; }

        /// <summary>
        /// Gets the number of tasks currently being executed.
        /// </summary>
        int RunningTaskCount { get; }

        /// <summary>
        /// Gets the maximum number of tasks that may be executed concurrently.
        /// </summary>
        int MaxConcurrentTasks { get; }

        /// <summary>
        /// Adds a task to the queue for background execution.
        /// </summary>
        /// <param name="taskFunc">The delegate to execute, receiving a cancellation token.</param>
        /// <param name="taskName">A name used to identify the task in logs. Defaults to <c>"UnnamedTask"</c>.</param>
        void QueueTask(Func<CancellationToken, Task> taskFunc, string taskName = "UnnamedTask");

        /// <summary>
        /// Starts processing queued tasks.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the worker, cancelling pending processing, and waits for it to finish.
        /// </summary>
        /// <param name="timeout">The maximum time to wait for the worker to stop. Defaults to 30 seconds when <c>null</c>.</param>
        /// <returns>A task that completes when the worker has stopped or the timeout has elapsed.</returns>
        Task StopAsync(TimeSpan? timeout = null);

        /// <summary>
        /// Gets a snapshot of the worker's current statistics.
        /// </summary>
        /// <returns>A <see cref="WorkerStatistics"/> instance describing the worker's current state.</returns>
        WorkerStatistics GetStatistics();
    }
}
