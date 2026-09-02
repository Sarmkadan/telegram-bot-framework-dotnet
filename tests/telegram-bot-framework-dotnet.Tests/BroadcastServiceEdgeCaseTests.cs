#nullable enable

using Moq;
using TelegramBotFramework.Integration;
using TelegramBotFramework.Services;
using Xunit;

namespace TelegramBotFramework.Tests;

public class BroadcastServiceEdgeCaseTests
{
    private readonly Mock<ITelegramApiClient> _mockApiClient = new();

    [Fact]
    public async Task BroadcastAsync_WithEmptyChatList_ReturnsSuccessWithZeroSent()
    {
        var service = new BroadcastService(_mockApiClient.Object);

        var result = await service.BroadcastAsync(Array.Empty<long>(), "message");

        Assert.Equal(0, result.TotalChats);
        Assert.Equal(0, result.ProcessedCount);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(0, result.FailedCount);
        Assert.True(result.AllSuccessful);
        _mockApiClient.Verify(
            client => client.SendMessageAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task BroadcastAsync_WithNullOrWhitespaceMessage_ThrowsArgumentException(string? message)
    {
        var service = new BroadcastService(_mockApiClient.Object);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.BroadcastAsync(new[] { 1L }, message!));

        Assert.Equal("messageText", exception.ParamName);
        _mockApiClient.Verify(
            client => client.SendMessageAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task BroadcastAsync_WhenCancelledMidBroadcast_StopsSendingAndReturnsPartialResult()
    {
        var service = new BroadcastService(_mockApiClient.Object);
        using var cancellationSource = new CancellationTokenSource();
        _mockApiClient
            .Setup(client => client.SendMessageAsync(It.IsAny<long>(), "message", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Task CancelAfterFirstChat(BroadcastProgress progress)
        {
            cancellationSource.Cancel();
            return Task.CompletedTask;
        }

        var result = await service.BroadcastAsync(
            new[] { 1L, 2L, 3L },
            "message",
            OneChatPerBatchOptions(),
            CancelAfterFirstChat,
            cancellationSource.Token);

        Assert.Equal(3, result.TotalChats);
        Assert.Equal(1, result.ProcessedCount);
        Assert.Equal(1, result.SuccessCount);
        Assert.Contains("cancelled", result.Summary!, StringComparison.OrdinalIgnoreCase);
        _mockApiClient.Verify(
            client => client.SendMessageAsync(It.IsAny<long>(), "message", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task BroadcastAsync_WhenApiCallsFail_CollectsFailuresAndContinuesSending()
    {
        var service = new BroadcastService(_mockApiClient.Object);
        _mockApiClient.Setup(client => client.SendMessageAsync(1L, "message", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mockApiClient.Setup(client => client.SendMessageAsync(2L, "message", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("API unavailable"));
        _mockApiClient.Setup(client => client.SendMessageAsync(3L, "message", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await service.BroadcastAsync(
            new[] { 1L, 2L, 3L },
            "message",
            new BroadcastOptions { ContinueOnError = true, MessagesPerSecond = 0 });

        Assert.Equal(3, result.ProcessedCount);
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal(2, result.FailedCount);
        Assert.Contains(result.Failures, failure => failure.ChatId == 1L);
        Assert.Contains(result.Failures, failure =>
            failure.ChatId == 2L && failure.ErrorMessage == "API unavailable");
        _mockApiClient.Verify(
            client => client.SendMessageAsync(It.IsAny<long>(), "message", It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task BroadcastAsync_WithProgressCallback_ReportsMonotonicallyIncreasingProcessedCounts()
    {
        var service = new BroadcastService(_mockApiClient.Object);
        var processedCounts = new List<int>();
        _mockApiClient
            .Setup(client => client.SendMessageAsync(It.IsAny<long>(), "message", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Task RecordProgress(BroadcastProgress progress)
        {
            processedCounts.Add(progress.ProcessedCount);
            return Task.CompletedTask;
        }

        await service.BroadcastAsync(
            new[] { 1L, 2L, 3L, 4L },
            "message",
            OneChatPerBatchOptions(),
            RecordProgress);

        Assert.Equal(new[] { 1, 2, 3, 4 }, processedCounts);
        Assert.All(
            processedCounts.Zip(processedCounts.Skip(1)),
            pair => Assert.True(pair.First < pair.Second));
    }

    private static BroadcastOptions OneChatPerBatchOptions() => new()
    {
        MessagesPerSecond = 1,
        BatchDelay = TimeSpan.Zero
    };
}
