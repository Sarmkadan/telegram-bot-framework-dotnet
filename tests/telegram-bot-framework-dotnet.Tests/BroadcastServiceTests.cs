#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using Moq;
using TelegramBotFramework.Integration;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;
using Xunit;

namespace TelegramBotFramework.Tests;

public class BroadcastServiceTests
{
    private readonly Mock<ITelegramApiClient> _mockApiClient;
    private readonly BroadcastService _broadcastService;

    public BroadcastServiceTests()
    {
        _mockApiClient = new Mock<ITelegramApiClient>();
        _broadcastService = new BroadcastService(_mockApiClient.Object);
    }

    [Fact]
    public async Task BroadcastAsync_WithEmptyChatIds_ReturnsSuccessWithNoChats()
    {
        var result = await _broadcastService.BroadcastAsync(
            chatIds: Array.Empty<long>(),
            messageText: "test");

        Assert.Equal(0, result.TotalChats);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(0, result.FailedCount);
        Assert.True(result.AllSuccessful);
    }

    [Fact]
    public async Task BroadcastAsync_WithValidChats_SendsMessagesToAllChats()
    {
        // Arrange
        var chatIds = new long[] { 123L, 456L, 789L };
        _mockApiClient.Setup(x => x.SendMessageAsync(123L, "test message"))
            .ReturnsAsync(true);
        _mockApiClient.Setup(x => x.SendMessageAsync(456L, "test message"))
            .ReturnsAsync(true);
        _mockApiClient.Setup(x => x.SendMessageAsync(789L, "test message"))
            .ReturnsAsync(true);

        // Act
        var result = await _broadcastService.BroadcastAsync(
            chatIds: chatIds,
            messageText: "test message");

        // Assert
        Assert.Equal(3, result.TotalChats);
        Assert.Equal(3, result.SuccessCount);
        Assert.Equal(0, result.FailedCount);
        Assert.True(result.AllSuccessful);
        Assert.Equal(3, result.SuccessfulChatIds.Count);
        Assert.Empty(result.Failures);
    }

    [Fact]
    public async Task BroadcastAsync_WithFailedMessages_CollectsFailures()
    {
        // Arrange
        var chatIds = new long[] { 123L, 456L, 789L };
        _mockApiClient.Setup(x => x.SendMessageAsync(123L, "test message"))
            .ReturnsAsync(true);
        _mockApiClient.Setup(x => x.SendMessageAsync(456L, "test message"))
            .ReturnsAsync(false); // Failure
        _mockApiClient.Setup(x => x.SendMessageAsync(789L, "test message"))
            .ThrowsAsync(new Exception("API error")); // Exception

        // Act
        var result = await _broadcastService.BroadcastAsync(
            chatIds: chatIds,
            messageText: "test message",
            options: new BroadcastOptions { ContinueOnError = true });

        // Assert
        Assert.Equal(3, result.TotalChats);
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal(2, result.FailedCount);
        Assert.False(result.AllSuccessful);
        Assert.Single(result.SuccessfulChatIds);
        Assert.Equal(2, result.Failures.Count);
    }

    [Fact]
    public async Task BroadcastAsync_WithContinueOnErrorFalse_ThrowsOnFirstError()
    {
        // Arrange
        var chatIds = new long[] { 123L, 456L };
        _mockApiClient.Setup(x => x.SendMessageAsync(123L, "test message"))
            .ReturnsAsync(true);
        _mockApiClient.Setup(x => x.SendMessageAsync(456L, "test message"))
            .ThrowsAsync(new Exception("API error"));

        // Act & Assert
        await Assert.ThrowsAsync<BroadcastException>(() =>
            _broadcastService.BroadcastAsync(
                chatIds: chatIds,
                messageText: "test message",
                options: new BroadcastOptions { ContinueOnError = false }));
    }

    [Fact]
    public async Task BroadcastAsync_WithRateLimit_RespectsMessagesPerSecond()
    {
        // Arrange
        var chatIds = new long[] { 1L, 2L, 3L, 4L, 5L };
        _mockApiClient.Setup(x => x.SendMessageAsync(It.IsAny<long>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        var options = new BroadcastOptions { MessagesPerSecond = 2 };
        var startTime = DateTime.UtcNow;

        // Act
        var result = await _broadcastService.BroadcastAsync(
            chatIds: chatIds,
            messageText: "test",
            options: options);

        // Assert - should take at least 2 seconds for 5 messages at 2 msg/s
        var elapsed = DateTime.UtcNow - startTime;
        Assert.True(elapsed.TotalSeconds >= 2, $"Expected at least 2 seconds, took {elapsed.TotalSeconds}s");
        Assert.Equal(5, result.SuccessCount);
    }

    [Fact]
    public async Task BroadcastAsync_WithProgressCallback_CallsCallback()
    {
        // Arrange
        var chatIds = new long[] { 1L, 2L };
        _mockApiClient.Setup(x => x.SendMessageAsync(It.IsAny<long>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        var callbackInvoked = false;
        Task Callback(BroadcastProgress progress)
        {
            callbackInvoked = true;
            return Task.CompletedTask;
        }

        // Act
        var result = await _broadcastService.BroadcastAsync(
            chatIds: chatIds,
            messageText: "test",
            progressCallback: Callback);

        // Assert
        Assert.True(callbackInvoked);
    }

    [Fact]
    public async Task BroadcastAsync_WithCancellation_CancelsOperation()
    {
        // Arrange
        var chatIds = new long[] { 1L, 2L, 3L, 4L, 5L };
        _mockApiClient.Setup(x => x.SendMessageAsync(It.IsAny<long>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        var cts = new CancellationTokenSource();
        var callbackInvokedCount = 0;

        Task Callback(BroadcastProgress progress)
        {
            callbackInvokedCount++;
            if (callbackInvokedCount >= 2)
            {
                cts.Cancel();
            }
            return Task.CompletedTask;
        }

        // Act
        var result = await _broadcastService.BroadcastAsync(
            chatIds: chatIds,
            messageText: "test",
            progressCallback: Callback,
            cancellationToken: cts.Token);

        // Assert - should have processed at least 2 chats before cancellation
        Assert.True(result.ProcessedCount >= 2);
        Assert.True(result.SuccessCount <= 2);
    }

    [Fact]
    public async Task BroadcastToUsersAsync_ConvertsUsersToChatIds()
    {
        // Arrange
        var users = new BotUser[]
        {
            new BotUser { TelegramId = 111L, FirstName = "User1" },
            new BotUser { TelegramId = 222L, FirstName = "User2" }
        };
        _mockApiClient.Setup(x => x.SendMessageAsync(It.IsAny<long>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _broadcastService.BroadcastToUsersAsync(
            users: users,
            messageText: "test");

        // Assert
        Assert.Equal(2, result.TotalChats);
        Assert.Equal(2, result.SuccessCount);
        Assert.Equal(2, _mockApiClient.Invocations.Count);
    }

    [Fact]
    public void GetRateLimitStats_ReturnsStatistics()
    {
        // Arrange
        var stats = _broadcastService.GetRateLimitStats();

        // Assert
        Assert.Equal(1, stats.MessagesPerSecond); // Default rate limiter allows 1
        Assert.Equal(1, stats.MaxConcurrency);   // Default concurrency limiter allows 1
        Assert.Equal(0, stats.TotalMessagesSent);
        Assert.Equal(0, stats.TotalMessagesFailed);
        Assert.InRange(stats.AverageMessagesPerSecond, 0, 1);
        Assert.Equal(0, stats.CurrentConcurrency);
    }

    [Fact]
    public async Task BroadcastAsync_WithMessageFormatter_AppliesFormatter()
    {
        // Arrange
        var chatIds = new long[] { 123L };
        _mockApiClient.Setup(x => x.SendMessageAsync(123L, "[123] custom message"))
            .ReturnsAsync(true);

        // Act
        var result = await _broadcastService.BroadcastAsync(
            chatIds: chatIds,
            messageText: "custom message",
            options: new BroadcastOptions
            {
                MessageFormatter = (text, chatId) => $"[{chatId}] {text}"
            });

        // Assert
        Assert.Equal(1, result.SuccessCount);
        _mockApiClient.Verify(x => x.SendMessageAsync(123L, "[123] custom message"), Times.Once);
    }

    [Fact]
    public void Dispose_DisposesResources()
    {
        // Act
        _broadcastService.Dispose();

        // Assert - no exception thrown
        Assert.True(true);
    }
}
