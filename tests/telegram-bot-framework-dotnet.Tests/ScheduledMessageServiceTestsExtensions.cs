using System;
using System.Threading.Tasks;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Extension methods that simplify working with <see cref="ScheduledMessageServiceTests"/> in test suites.
/// </summary>
public static class ScheduledMessageServiceTestsExtensions
{
    /// <summary>
    /// Executes all scheduling‑related test cases sequentially.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
    public static async Task RunAllScheduleMessageAsyncTests(this ScheduledMessageServiceTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        await tests.ScheduleMessageAsync_WithFutureTime_SchedulesSuccessfully();
        await tests.ScheduleMessageAsync_WithDelay_SchedulesSuccessfully();
        await tests.ScheduleMessageAsync_InvalidChatId_ThrowsArgumentException();
        await tests.ScheduleMessageAsync_EmptyText_ThrowsArgumentException();
        await tests.ScheduleMessageAsync_PastTime_ThrowsArgumentException();
    }

    /// <summary>
    /// Executes all sending‑related test cases sequentially.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
    public static async Task RunAllSendScheduledMessageAsyncTests(this ScheduledMessageServiceTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        await tests.SendScheduledMessageAsync_SuccessfulSend_MarksAsSent();
        await tests.SendScheduledMessageAsync_FailedSend_RetriesAndEventuallyFails();
        await tests.SendScheduledMessageAsync_PersistentFailure_MarksAsFailed();
    }

    /// <summary>
    /// Executes all cancellation‑related test cases.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
    public static void RunAllCancellationTests(this ScheduledMessageServiceTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        tests.CancelScheduledMessage_CancelsSuccessfully();
        tests.CancelScheduledMessage_InvalidId_ReturnsFalse();
    }

    /// <summary>
    /// Disposes the test instance and ensures cleanup logic runs.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
    public static void DisposeAll(this ScheduledMessageServiceTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        tests.Dispose();
        tests.Dispose_CleansUpResources();
    }
}
