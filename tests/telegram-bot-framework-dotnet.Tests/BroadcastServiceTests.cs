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

/// <summary>
/// Test class for BroadcastService functionality.
/// Contains unit tests for broadcasting messages to multiple chats.
/// </summary>
public class BroadcastServiceTests : IBroadcastServiceTests
{
    private readonly Mock<ITelegramApiClient> _mockApiClient;
    private readonly BroadcastService _broadcastService;

    /// <summary>
    /// Initializes a new instance of the BroadcastServiceTests class.
    /// Sets up mock dependencies for testing.
    /// </summary>
    public BroadcastServiceTests()
    {
        _mockApiClient = new Mock<ITelegramApiClient>();
        _broadcastService = new BroadcastService(_mockApiClient.Object);
    }

    /// <summary>
    /// Tests that broadcasting to an empty chat ID list returns success with zero chats processed.
    /// </summary>
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

    /// <summary>
    /// Tests that broadcasting to valid chat IDs sends messages to all chats successfully.
    /// </summary>
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

    /// <summary>
    /// Tests that broadcasting with failed messages properly collects failures when ContinueOnError is true.
    /// </summary>
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

    /// <summary>
    /// Tests that broadcasting with ContinueOnError false throws an exception on the first failed message.
    /// </summary>
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

    /// <summary>
    /// Tests that broadcasting respects the MessagesPerSecond rate limit setting.
    /// </summary>
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

    /// <summary>
    /// Tests that broadcasting with a progress callback invokes the callback during execution.
    /// </summary>
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

    /// <summary>
    /// Tests that broadcasting respects cancellation tokens and stops operation when a cancellation token is triggered.
    /// </summary>
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

    /// <summary>
/// Tests that broadcasting to users correctly converts BotUser objects to chat IDs and sends messages.
/// </summary>
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

    /// <summary>
    /// Tests that GetRateLimitStats returns current rate limiting statistics.
    /// </summary>
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

    /// <summary>
    /// Tests that broadcasting with a message formatter applies the formatter to the message text.
    /// </summary>
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

    /// <summary>
    /// Tests that the Dispose method properly disposes of resources without throwing exceptions.
    /// </summary>
    [Fact]
    public void Dispose_DisposesResources()
    {
        // Act
        _broadcastService.Dispose();

        // Assert - no exception thrown
        Assert.True(true);
    }
}
