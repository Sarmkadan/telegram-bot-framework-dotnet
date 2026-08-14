#nullable enable
using System;
using System.Collections.Generic;
using TelegramBotFramework.Services;
using Xunit;

namespace TelegramBotFramework.Tests;

public class BroadcastProgressTests
{
    [Fact]
    public void Constructor_SetsAllProperties_Correctly()
    {
        // Arrange
        var failures = new List<FailedChat>
        {
            new FailedChat(12345, "network error", 2),
            new FailedChat(67890, "timeout", 1)
        };
        var elapsed = TimeSpan.FromSeconds(30);
        var estimated = TimeSpan.FromSeconds(70);
        const double mps = 5.5;

        // Act
        var progress = new BroadcastProgress(
            totalChats: 100,
            processedCount: 30,
            successCount: 28,
            failedCount: 2,
            failures: failures,
            elapsedTime: elapsed,
            estimatedTimeRemaining: estimated,
            currentMessagesPerSecond: mps);

        // Assert
        Assert.Equal(100, progress.TotalChats);
        Assert.Equal(30, progress.ProcessedCount);
        Assert.Equal(28, progress.SuccessCount);
        Assert.Equal(2, progress.FailedCount);
        Assert.Same(failures, progress.Failures);
        Assert.Equal(elapsed, progress.ElapsedTime);
        Assert.Equal(estimated, progress.EstimatedTimeRemaining);
        Assert.Equal(mps, progress.CurrentMessagesPerSecond);
    }

    [Fact]
    public void ProgressPercentage_CalculatesCorrectly()
    {
        var progress = new BroadcastProgress(
            totalChats: 8,
            processedCount: 2,
            successCount: 2,
            failedCount: 0,
            failures: new List<FailedChat>(),
            elapsedTime: TimeSpan.Zero,
            estimatedTimeRemaining: null,
            currentMessagesPerSecond: 0);

        Assert.Equal(25, progress.ProgressPercentage);
    }

    [Fact]
    public void ProgressPercentage_ReturnsZero_WhenTotalChatsIsZero()
    {
        var progress = new BroadcastProgress(
            totalChats: 0,
            processedCount: 0,
            successCount: 0,
            failedCount: 0,
            failures: new List<FailedChat>(),
            elapsedTime: TimeSpan.Zero,
            estimatedTimeRemaining: null,
            currentMessagesPerSecond: 0);

        Assert.Equal(0, progress.ProgressPercentage);
    }

    [Fact]
    public void IsComplete_ReturnsTrue_WhenProcessedEqualsTotal()
    {
        var progress = new BroadcastProgress(
            totalChats: 5,
            processedCount: 5,
            successCount: 5,
            failedCount: 0,
            failures: new List<FailedChat>(),
            elapsedTime: TimeSpan.Zero,
            estimatedTimeRemaining: null,
            currentMessagesPerSecond: 0);

        Assert.True(progress.IsComplete);
    }

    [Fact]
    public void IsComplete_ReturnsTrue_WhenProcessedExceedsTotal()
    {
        var progress = new BroadcastProgress(
            totalChats: 3,
            processedCount: 5,
            successCount: 3,
            failedCount: 2,
            failures: new List<FailedChat>(),
            elapsedTime: TimeSpan.Zero,
            estimatedTimeRemaining: null,
            currentMessagesPerSecond: 0);

        Assert.True(progress.IsComplete);
    }

    [Fact]
    public void FailedChat_Properties_AreSetCorrectly()
    {
        var failedChat = new FailedChat(chatId: 987654321, errorMessage: "boom", retryAttempts: 3);

        Assert.Equal(987654321, failedChat.ChatId);
        Assert.Equal("boom", failedChat.ErrorMessage);
        Assert.Equal(3, failedChat.RetryAttempts);
    }

    [Fact]
    public void Constructor_AllowsEmptyFailuresCollection()
    {
        var progress = new BroadcastProgress(
            totalChats: 10,
            processedCount: 1,
            successCount: 1,
            failedCount: 0,
            failures: new List<FailedChat>(),
            elapsedTime: TimeSpan.FromSeconds(1),
            estimatedTimeRemaining: null,
            currentMessagesPerSecond: 1.0);

        Assert.Empty(progress.Failures);
    }
}
