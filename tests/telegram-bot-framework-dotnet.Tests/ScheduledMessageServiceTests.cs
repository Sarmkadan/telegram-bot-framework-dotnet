#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using Microsoft.Extensions.Logging;
using Moq;
using TelegramBotFramework.Integration;
using TelegramBotFramework.Services;
using Xunit;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Tests for <see cref="ScheduledMessageService"/>.
/// </summary>
public sealed class ScheduledMessageServiceTests : IDisposable, IScheduledMessageServiceTests
{
    private readonly Mock<ITelegramApiClient> _mockTelegramApiClient;
    private readonly Mock<ILogger<ScheduledMessageService>> _mockLogger;
    private readonly ScheduledMessageService _service;
    private readonly CancellationTokenSource _cts = new();

    public ScheduledMessageServiceTests()
    {
        _mockTelegramApiClient = new Mock<ITelegramApiClient>();
        _mockLogger = new Mock<ILogger<ScheduledMessageService>>();

        _service = new ScheduledMessageService(
            _mockTelegramApiClient.Object,
            _mockLogger.Object);
    }

    public void Dispose()
    {
        _mockLogger.Object.LogInformation("Dispose called for scheduled message service tests");
        _cts.Cancel();
        _cts.Dispose();
        _service.Dispose();
        _mockLogger.Object.LogInformation("Dispose completed for scheduled message service tests");
    }

    [Fact]
    public async Task ScheduleMessageAsync_WithFutureTime_SchedulesSuccessfully()
    {
        // Arrange
        var chatId = 12345L;
        var text = "Hello, World!";
        var sendAt = DateTimeOffset.UtcNow.AddSeconds(2);

        _mockLogger.Object.LogInformation(
            "ScheduleMessageAsync_WithFutureTime_SchedulesSuccessfully called with {ChatId} and {SendAt}",
            chatId,
            sendAt);

        _mockTelegramApiClient
            .Setup(x => x.SendMessageAsync(chatId, text))
            .ReturnsAsync(true);

        // Act
        var messageId = await _service.ScheduleMessageAsync(chatId, text, sendAt, _cts.Token);

        // Assert
        Assert.NotNull(messageId);
        Assert.NotEmpty(messageId);

        var scheduledMessage = _service.GetScheduledMessage(messageId);
        Assert.NotNull(scheduledMessage);
        Assert.Equal(chatId, scheduledMessage.ChatId);
        Assert.Equal(text, scheduledMessage.Text);
        Assert.Equal(sendAt, scheduledMessage.ScheduledTime);
        Assert.False(scheduledMessage.IsCancelled);
        Assert.False(scheduledMessage.IsSent);

        // Wait for message to be sent (with timeout)
        await Task.Delay(TimeSpan.FromSeconds(3), _cts.Token);

        var updatedMessage = _service.GetScheduledMessage(messageId);
        Assert.NotNull(updatedMessage);
        Assert.True(updatedMessage.IsSent);
        Assert.NotNull(updatedMessage.SentAt);
        _mockLogger.Object.LogInformation(
            "ScheduleMessageAsync_WithFutureTime_SchedulesSuccessfully completed with {MessageId}",
            messageId);
    }

    [Fact]
    public async Task ScheduleMessageAsync_WithDelay_SchedulesSuccessfully()
    {
        // Arrange
        var chatId = 12345L;
        var text = "Delayed message";
        var delay = TimeSpan.FromSeconds(1);

        _mockLogger.Object.LogInformation(
            "ScheduleMessageAsync_WithDelay_SchedulesSuccessfully called with {ChatId} and {Delay}",
            chatId,
            delay);

        _mockTelegramApiClient
            .Setup(x => x.SendMessageAsync(chatId, text))
            .ReturnsAsync(true);

        // Act
        var messageId = await _service.ScheduleMessageAsync(chatId, text, delay, _cts.Token);

        // Assert
        Assert.NotNull(messageId);
        Assert.NotEmpty(messageId);

        var scheduledMessage = _service.GetScheduledMessage(messageId);
        Assert.NotNull(scheduledMessage);
        Assert.Equal(chatId, scheduledMessage.ChatId);
        Assert.True(scheduledMessage.ScheduledTime > DateTimeOffset.UtcNow);
        _mockLogger.Object.LogInformation(
            "ScheduleMessageAsync_WithDelay_SchedulesSuccessfully completed with {MessageId}",
            messageId);
    }

    [Fact]
    public async Task ScheduleMessageAsync_InvalidChatId_ThrowsArgumentException()
    {
        // Arrange
        var chatId = -1L;
        var text = "Test message";
        var sendAt = DateTimeOffset.UtcNow.AddMinutes(1);

        _mockLogger.Object.LogInformation(
            "ScheduleMessageAsync_InvalidChatId_ThrowsArgumentException called with {ChatId} and {SendAt}",
            chatId,
            sendAt);
        _mockLogger.Object.LogWarning("Scheduling will use the invalid chat ID path for {ChatId}", chatId);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ScheduleMessageAsync(chatId, text, sendAt, _cts.Token));
        _mockLogger.Object.LogInformation(
            "ScheduleMessageAsync_InvalidChatId_ThrowsArgumentException completed for {ChatId}",
            chatId);
    }

    [Fact]
    public async Task ScheduleMessageAsync_EmptyText_ThrowsArgumentException()
    {
        // Arrange
        var chatId = 12345L;
        var text = string.Empty;
        var sendAt = DateTimeOffset.UtcNow.AddMinutes(1);

        _mockLogger.Object.LogInformation(
            "ScheduleMessageAsync_EmptyText_ThrowsArgumentException called with {ChatId} and {TextLength}",
            chatId,
            text.Length);
        _mockLogger.Object.LogWarning("Scheduling will use the empty text path for {ChatId}", chatId);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ScheduleMessageAsync(chatId, text, sendAt, _cts.Token));
        _mockLogger.Object.LogInformation(
            "ScheduleMessageAsync_EmptyText_ThrowsArgumentException completed for {ChatId}",
            chatId);
    }

    [Fact]
    public async Task ScheduleMessageAsync_PastTime_ThrowsArgumentException()
    {
        // Arrange
        var chatId = 12345L;
        var text = "Test message";
        var sendAt = DateTimeOffset.UtcNow.AddMinutes(-1);

        _mockLogger.Object.LogInformation(
            "ScheduleMessageAsync_PastTime_ThrowsArgumentException called with {ChatId} and {SendAt}",
            chatId,
            sendAt);
        _mockLogger.Object.LogWarning("Scheduling will use the past time path for {ChatId} at {SendAt}", chatId, sendAt);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ScheduleMessageAsync(chatId, text, sendAt, _cts.Token));
        _mockLogger.Object.LogInformation(
            "ScheduleMessageAsync_PastTime_ThrowsArgumentException completed for {ChatId}",
            chatId);
    }

    [Fact]
    public void CancelScheduledMessage_CancelsSuccessfully()
    {
        // Arrange
        var chatId = 12345L;
        var text = "Message to cancel";
        var sendAt = DateTimeOffset.UtcNow.AddSeconds(5);

        _mockLogger.Object.LogInformation(
            "CancelScheduledMessage_CancelsSuccessfully called with {ChatId} and {SendAt}",
            chatId,
            sendAt);

        var messageId = _service.ScheduleMessageAsync(chatId, text, sendAt).Result;
        Assert.NotNull(messageId);

        // Act
        var cancelled = _service.CancelScheduledMessage(messageId);

        // Assert
        Assert.True(cancelled);

        var scheduledMessage = _service.GetScheduledMessage(messageId);
        Assert.NotNull(scheduledMessage);
        Assert.True(scheduledMessage.IsCancelled);
        _mockLogger.Object.LogInformation(
            "CancelScheduledMessage_CancelsSuccessfully completed with {MessageId}",
            messageId);
    }

    [Fact]
    public void CancelScheduledMessage_InvalidId_ReturnsFalse()
    {
        _mockLogger.Object.LogInformation("CancelScheduledMessage_InvalidId_ReturnsFalse called with {MessageId}", "invalid-id");
        _mockLogger.Object.LogWarning("Cancelling scheduled message will use the missing message fallback for {MessageId}", "invalid-id");

        // Act
        var cancelled = _service.CancelScheduledMessage("invalid-id");

        // Assert
        Assert.False(cancelled);
        _mockLogger.Object.LogInformation("CancelScheduledMessage_InvalidId_ReturnsFalse completed with {Cancelled}", cancelled);
    }

    [Fact]
    public void GetAllScheduledMessages_ReturnsAllMessages()
    {
        // Arrange
        var chatId1 = 12345L;
        var chatId2 = 67890L;
        var text1 = "Message 1";
        var text2 = "Message 2";
        var sendAt = DateTimeOffset.UtcNow.AddMinutes(1);

        _mockLogger.Object.LogInformation(
            "GetAllScheduledMessages_ReturnsAllMessages called with {FirstChatId}, {SecondChatId}, and {SendAt}",
            chatId1,
            chatId2,
            sendAt);

        var messageId1 = _service.ScheduleMessageAsync(chatId1, text1, sendAt).Result;
        var messageId2 = _service.ScheduleMessageAsync(chatId2, text2, sendAt).Result;

        // Act
        var allMessages = _service.GetAllScheduledMessages();

        // Assert
        Assert.NotNull(allMessages);
        Assert.Equal(2, allMessages.Count());
        _mockLogger.Object.LogInformation(
            "GetAllScheduledMessages_ReturnsAllMessages completed with {MessageCount}",
            allMessages.Count());
    }

    [Fact]
    public void GetScheduledMessagesForChat_ReturnsChatMessages()
    {
        // Arrange
        var chatId1 = 12345L;
        var chatId2 = 67890L;
        var text1 = "Message 1";
        var text2 = "Message 2";
        var text3 = "Message 3";
        var sendAt = DateTimeOffset.UtcNow.AddMinutes(1);

        _mockLogger.Object.LogInformation(
            "GetScheduledMessagesForChat_ReturnsChatMessages called with {FirstChatId}, {SecondChatId}, and {SendAt}",
            chatId1,
            chatId2,
            sendAt);

        var messageId1 = _service.ScheduleMessageAsync(chatId1, text1, sendAt).Result;
        var messageId2 = _service.ScheduleMessageAsync(chatId2, text2, sendAt).Result;
        var messageId3 = _service.ScheduleMessageAsync(chatId1, text3, sendAt).Result;

        // Act
        var chat1Messages = _service.GetScheduledMessagesForChat(chatId1).ToList();
        var chat2Messages = _service.GetScheduledMessagesForChat(chatId2).ToList();

        // Assert
        Assert.Equal(2, chat1Messages.Count);
        Assert.Equal(1, chat2Messages.Count);
        Assert.All(chat1Messages, m => Assert.Equal(chatId1, m.ChatId));
        Assert.All(chat2Messages, m => Assert.Equal(chatId2, m.ChatId));
        _mockLogger.Object.LogInformation(
            "GetScheduledMessagesForChat_ReturnsChatMessages completed with {FirstChatMessageCount} and {SecondChatMessageCount}",
            chat1Messages.Count,
            chat2Messages.Count);
    }

    [Fact]
    public async Task SendScheduledMessageAsync_SuccessfulSend_MarksAsSent()
    {
        // Arrange
        var chatId = 12345L;
        var text = "Test message";
        var sendAt = DateTimeOffset.UtcNow.AddSeconds(1);

        _mockLogger.Object.LogInformation(
            "SendScheduledMessageAsync_SuccessfulSend_MarksAsSent called with {ChatId} and {SendAt}",
            chatId,
            sendAt);

        _mockTelegramApiClient
            .Setup(x => x.SendMessageAsync(chatId, text))
            .ReturnsAsync(true);

        var messageId = await _service.ScheduleMessageAsync(chatId, text, sendAt, _cts.Token);

        // Wait for message to be sent
        await Task.Delay(TimeSpan.FromSeconds(2), _cts.Token);

        // Assert
        var scheduledMessage = _service.GetScheduledMessage(messageId);
        Assert.NotNull(scheduledMessage);
        Assert.True(scheduledMessage.IsSent);
        Assert.NotNull(scheduledMessage.SentAt);
        Assert.Null(scheduledMessage.ErrorMessage);
        _mockLogger.Object.LogInformation(
            "SendScheduledMessageAsync_SuccessfulSend_MarksAsSent completed with {MessageId}",
            messageId);
    }

    [Fact]
    public async Task SendScheduledMessageAsync_FailedSend_RetriesAndEventuallyFails()
    {
        // Arrange
        var chatId = 12345L;
        var text = "Test message";
        var sendAt = DateTimeOffset.UtcNow.AddSeconds(1);

        _mockLogger.Object.LogInformation(
            "SendScheduledMessageAsync_FailedSend_RetriesAndEventuallyFails called with {ChatId} and {SendAt}",
            chatId,
            sendAt);
        _mockLogger.Object.LogWarning("Scheduled message for {ChatId} will exercise the retry path", chatId);

        // First two attempts fail, third succeeds
        _mockTelegramApiClient
            .SetupSequence(x => x.SendMessageAsync(chatId, text))
            .ReturnsAsync(false)
            .ReturnsAsync(false)
            .ReturnsAsync(true);

        var messageId = await _service.ScheduleMessageAsync(chatId, text, sendAt, _cts.Token);

        // Wait for retries to complete
        await Task.Delay(TimeSpan.FromSeconds(5), _cts.Token);

        // Assert
        var scheduledMessage = _service.GetScheduledMessage(messageId);
        Assert.NotNull(scheduledMessage);
        Assert.True(scheduledMessage.IsSent);
        Assert.Equal(3, scheduledMessage.AttemptCount);
        Assert.Null(scheduledMessage.ErrorMessage);
        _mockLogger.Object.LogInformation(
            "SendScheduledMessageAsync_FailedSend_RetriesAndEventuallyFails completed with {MessageId} after {AttemptCount} attempts",
            messageId,
            scheduledMessage.AttemptCount);
    }

    [Fact]
    public async Task SendScheduledMessageAsync_PersistentFailure_MarksAsFailed()
    {
        // Arrange
        var chatId = 12345L;
        var text = "Test message";
        var sendAt = DateTimeOffset.UtcNow.AddSeconds(1);

        _mockLogger.Object.LogInformation(
            "SendScheduledMessageAsync_PersistentFailure_MarksAsFailed called with {ChatId} and {SendAt}",
            chatId,
            sendAt);
        _mockLogger.Object.LogWarning("Scheduled message for {ChatId} will exercise the persistent failure path", chatId);

        // All attempts fail
        _mockTelegramApiClient
            .Setup(x => x.SendMessageAsync(chatId, text))
            .ReturnsAsync(false);

        var messageId = await _service.ScheduleMessageAsync(chatId, text, sendAt, _cts.Token);

        // Wait for max retries to complete
        await Task.Delay(TimeSpan.FromSeconds(10), _cts.Token);

        // Assert
        var scheduledMessage = _service.GetScheduledMessage(messageId);
        Assert.NotNull(scheduledMessage);
        Assert.True(scheduledMessage.AttemptCount >= 3);
        Assert.False(scheduledMessage.IsSent);
        Assert.NotNull(scheduledMessage.ErrorMessage);
        _mockLogger.Object.LogInformation(
            "SendScheduledMessageAsync_PersistentFailure_MarksAsFailed completed with {MessageId} after {AttemptCount} attempts",
            messageId,
            scheduledMessage.AttemptCount);
    }

    [Fact]
    public void Dispose_CleansUpResources()
    {
        // Arrange
        var chatId = 12345L;
        var text = "Test message";
        var sendAt = DateTimeOffset.UtcNow.AddMinutes(1);

        _mockLogger.Object.LogInformation(
            "Dispose_CleansUpResources called with {ChatId} and {SendAt}",
            chatId,
            sendAt);

        var messageId = _service.ScheduleMessageAsync(chatId, text, sendAt).Result;
        Assert.Single(_service.GetAllScheduledMessages());

        // Act
        _service.Dispose();

        // Assert
        Assert.Empty(_service.GetAllScheduledMessages());
        _mockLogger.Object.LogInformation(
            "Dispose_CleansUpResources completed with {MessageId}",
            messageId);
    }
}
