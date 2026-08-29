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
        _cts.Cancel();
        _cts.Dispose();
        _service.Dispose();
    }

    [Fact]
    public async Task ScheduleMessageAsync_WithFutureTime_SchedulesSuccessfully()
    {
        // Arrange
        var chatId = 12345L;
        var text = "Hello, World!";
        var sendAt = DateTimeOffset.UtcNow.AddSeconds(2);

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
    }

    [Fact]
    public async Task ScheduleMessageAsync_WithDelay_SchedulesSuccessfully()
    {
        // Arrange
        var chatId = 12345L;
        var text = "Delayed message";
        var delay = TimeSpan.FromSeconds(1);

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
    }

    [Fact]
    public async Task ScheduleMessageAsync_InvalidChatId_ThrowsArgumentException()
    {
        // Arrange
        var chatId = -1L;
        var text = "Test message";
        var sendAt = DateTimeOffset.UtcNow.AddMinutes(1);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ScheduleMessageAsync(chatId, text, sendAt, _cts.Token));
    }

    [Fact]
    public async Task ScheduleMessageAsync_EmptyText_ThrowsArgumentException()
    {
        // Arrange
        var chatId = 12345L;
        var text = string.Empty;
        var sendAt = DateTimeOffset.UtcNow.AddMinutes(1);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ScheduleMessageAsync(chatId, text, sendAt, _cts.Token));
    }

    [Fact]
    public async Task ScheduleMessageAsync_PastTime_ThrowsArgumentException()
    {
        // Arrange
        var chatId = 12345L;
        var text = "Test message";
        var sendAt = DateTimeOffset.UtcNow.AddMinutes(-1);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.ScheduleMessageAsync(chatId, text, sendAt, _cts.Token));
    }

    [Fact]
    public void CancelScheduledMessage_CancelsSuccessfully()
    {
        // Arrange
        var chatId = 12345L;
        var text = "Message to cancel";
        var sendAt = DateTimeOffset.UtcNow.AddSeconds(5);

        var messageId = _service.ScheduleMessageAsync(chatId, text, sendAt).Result;
        Assert.NotNull(messageId);

        // Act
        var cancelled = _service.CancelScheduledMessage(messageId);

        // Assert
        Assert.True(cancelled);

        var scheduledMessage = _service.GetScheduledMessage(messageId);
        Assert.NotNull(scheduledMessage);
        Assert.True(scheduledMessage.IsCancelled);
    }

    [Fact]
    public void CancelScheduledMessage_InvalidId_ReturnsFalse()
    {
        // Act
        var cancelled = _service.CancelScheduledMessage("invalid-id");

        // Assert
        Assert.False(cancelled);
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

        var messageId1 = _service.ScheduleMessageAsync(chatId1, text1, sendAt).Result;
        var messageId2 = _service.ScheduleMessageAsync(chatId2, text2, sendAt).Result;

        // Act
        var allMessages = _service.GetAllScheduledMessages();

        // Assert
        Assert.NotNull(allMessages);
        Assert.Equal(2, allMessages.Count());
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
    }

    [Fact]
    public async Task SendScheduledMessageAsync_SuccessfulSend_MarksAsSent()
    {
        // Arrange
        var chatId = 12345L;
        var text = "Test message";
        var sendAt = DateTimeOffset.UtcNow.AddSeconds(1);

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
    }

    [Fact]
    public async Task SendScheduledMessageAsync_FailedSend_RetriesAndEventuallyFails()
    {
        // Arrange
        var chatId = 12345L;
        var text = "Test message";
        var sendAt = DateTimeOffset.UtcNow.AddSeconds(1);

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
    }

    [Fact]
    public async Task SendScheduledMessageAsync_PersistentFailure_MarksAsFailed()
    {
        // Arrange
        var chatId = 12345L;
        var text = "Test message";
        var sendAt = DateTimeOffset.UtcNow.AddSeconds(1);

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
    }

    [Fact]
    public void Dispose_CleansUpResources()
    {
        // Arrange
        var chatId = 12345L;
        var text = "Test message";
        var sendAt = DateTimeOffset.UtcNow.AddMinutes(1);

        var messageId = _service.ScheduleMessageAsync(chatId, text, sendAt).Result;
        Assert.Single(_service.GetAllScheduledMessages());

        // Act
        _service.Dispose();

        // Assert
        Assert.Empty(_service.GetAllScheduledMessages());
    }
}