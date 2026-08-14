using System;
using System.Threading;
using System.Threading.Tasks;
using TelegramBotFramework.BackgroundWorkers;
using Xunit;

namespace TelegramBotFramework.Tests;

public class BackgroundTaskWorkerTests
{
    private const int DefaultMaxConcurrent = 2;
    private readonly TimeSpan _shortTimeout = TimeSpan.FromSeconds(2);

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        using var worker = new BackgroundTaskWorker(DefaultMaxConcurrent);
        Assert.NotNull(worker);
    }

    [Fact]
    public void QueueTask_NullDelegate_ThrowsArgumentNullException()
    {
        using var worker = new BackgroundTaskWorker();
        Assert.Throws<ArgumentNullException>(() => worker.QueueTask(null!));
    }

    [Fact]
    public void QueueTask_IncrementsQueuedCount()
    {
        using var worker = new BackgroundTaskWorker();
        var initialStats = worker.GetStatistics();
        Assert.Equal(0, initialStats.QueuedTaskCount);

        worker.QueueTask(_ => Task.CompletedTask, "TestTask");
        var afterStats = worker.GetStatistics();
        Assert.Equal(1, afterStats.QueuedTaskCount);
    }

    [Fact]
    public async Task Start_ExecutesQueuedTask()
    {
        using var worker = new BackgroundTaskWorker();
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        worker.QueueTask(async ct =>
        {
            // Simulate some work
            await Task.Delay(10, ct);
            tcs.SetResult(true);
        }, "ExecTask");

        worker.Start();

        // Wait for the task to signal completion
        var completed = await Task.WhenAny(tcs.Task, Task.Delay(_shortTimeout));
        Assert.Equal(tcs.Task, completed);
        Assert.True(tcs.Task.Result);

        // Give the worker a moment to update its internal counters
        await Task.Delay(10);
        var stats = worker.GetStatistics();
        Assert.Equal(0, stats.QueuedTaskCount);
        Assert.Equal(0, stats.RunningTaskCount);
    }

    [Fact]
    public async Task StopAsync_CancelsLongRunningTask()
    {
        using var worker = new BackgroundTaskWorker();
        var cancellationObserved = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        worker.QueueTask(async ct =>
        {
            try
            {
                // Wait indefinitely until cancelled
                await Task.Delay(TimeSpan.FromSeconds(30), ct);
            }
            catch (OperationCanceledException)
            {
                cancellationObserved.SetResult(true);
                throw;
            }
        }, "LongRunning");

        worker.Start();

        // Give the task a moment to start
        await Task.Delay(10);
        await worker.StopAsync(_shortTimeout);

        // The task should have observed cancellation
        var observed = await Task.WhenAny(cancellationObserved.Task, Task.Delay(_shortTimeout));
        Assert.Equal(cancellationObserved.Task, observed);
        Assert.True(cancellationObserved.Task.Result);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var worker = new BackgroundTaskWorker();
        worker.Dispose();

        // Second dispose should not throw
        var exception = Record.Exception(() => worker.Dispose());
        Assert.Null(exception);
    }
}
