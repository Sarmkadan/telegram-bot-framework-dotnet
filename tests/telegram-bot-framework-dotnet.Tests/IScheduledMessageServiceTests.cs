namespace TelegramBotFramework.Tests;

/// <summary>
/// Interface for ScheduledMessageServiceTests.
/// </summary>
public interface IScheduledMessageServiceTests
{
    void Dispose();
    Task ScheduleMessageAsync_WithFutureTime_SchedulesSuccessfully();
    Task ScheduleMessageAsync_WithDelay_SchedulesSuccessfully();
    Task ScheduleMessageAsync_InvalidChatId_ThrowsArgumentException();
    Task ScheduleMessageAsync_EmptyText_ThrowsArgumentException();
    Task ScheduleMessageAsync_PastTime_ThrowsArgumentException();
    void CancelScheduledMessage_CancelsSuccessfully();
    void CancelScheduledMessage_InvalidId_ReturnsFalse();
    void GetAllScheduledMessages_ReturnsAllMessages();
    void GetScheduledMessagesForChat_ReturnsChatMessages();
    Task SendScheduledMessageAsync_SuccessfulSend_MarksAsSent();
    Task SendScheduledMessageAsync_FailedSend_RetriesAndEventuallyFails();
    Task SendScheduledMessageAsync_PersistentFailure_MarksAsFailed();
    void Dispose_CleansUpResources();
}