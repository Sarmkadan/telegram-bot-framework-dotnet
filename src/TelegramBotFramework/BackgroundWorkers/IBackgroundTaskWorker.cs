namespace TelegramBotFramework.BackgroundWorkers
{
    public interface IBackgroundTaskWorker : IDisposable
    {
        string Id { get; }
        string Name { get; }
        Func<CancellationToken, Task>? TaskFunc { get; set; }
        DateTime QueuedAt { get; }
        DateTime? StartedAt { get; }
        DateTime? CompletedAt { get; }
        int QueuedTaskCount { get; }
        int RunningTaskCount { get; }
        int MaxConcurrentTasks { get; }

        void QueueTask(Func<CancellationToken, Task> taskFunc, string taskName = "UnnamedTask");
        void Start();
        Task StopAsync(TimeSpan? timeout = null);
        WorkerStatistics GetStatistics();
            }
}
