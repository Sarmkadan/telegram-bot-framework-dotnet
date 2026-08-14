using System;
using TelegramBotFramework.Services;
using Xunit;

namespace TelegramBotFramework.Tests;

public class RateLimitStatsTests
{
    [Fact]
    public void Constructor_ShouldInitializeAllPropertiesCorrectly()
    {
        // Arrange
        var messagesPerSecond = 10;
        var maxConcurrency = 5;
        var totalSent = 1234L;
        var totalFailed = 12L;
        var avgMps = 8.5;
        var currentConcurrency = 2;

        // Act
        var stats = new RateLimitStats(
            messagesPerSecond,
            maxConcurrency,
            totalSent,
            totalFailed,
            avgMps,
            currentConcurrency);

        // Assert
        Assert.Equal(messagesPerSecond, stats.MessagesPerSecond);
        Assert.Equal(maxConcurrency, stats.MaxConcurrency);
        Assert.Equal(totalSent, stats.TotalMessagesSent);
        Assert.Equal(totalFailed, stats.TotalMessagesFailed);
        Assert.Equal(avgMps, stats.AverageMessagesPerSecond);
        Assert.Equal(currentConcurrency, stats.CurrentConcurrency);
        Assert.True((DateTime.UtcNow - stats.Timestamp).TotalSeconds < 1,
            "Timestamp should be set to the creation time (within 1 second).");
    }

    [Fact]
    public void Constructor_ShouldAllowZeroAndNegativeValues()
    {
        // Arrange & Act
        var stats = new RateLimitStats(
            messagesPerSecond: 0,
            maxConcurrency: -1,
            totalMessagesSent: 0,
            totalMessagesFailed: -5,
            averageMessagesPerSecond: 0.0,
            currentConcurrency: -2);

        // Assert – the class does not validate inputs, so values are stored as‑is.
        Assert.Equal(0, stats.MessagesPerSecond);
        Assert.Equal(-1, stats.MaxConcurrency);
        Assert.Equal(0L, stats.TotalMessagesSent);
        Assert.Equal(-5L, stats.TotalMessagesFailed);
        Assert.Equal(0.0, stats.AverageMessagesPerSecond);
        Assert.Equal(-2, stats.CurrentConcurrency);
    }

    [Fact]
    public void Constructor_ShouldHandleLargeValuesWithoutOverflow()
    {
        // Arrange
        var maxInt = int.MaxValue;
        var maxLong = long.MaxValue;
        var maxDouble = double.MaxValue;

        // Act
        var stats = new RateLimitStats(
            messagesPerSecond: maxInt,
            maxConcurrency: maxInt,
            totalMessagesSent: maxLong,
            totalMessagesFailed: maxLong,
            averageMessagesPerSecond: maxDouble,
            currentConcurrency: maxInt);

        // Assert
        Assert.Equal(maxInt, stats.MessagesPerSecond);
        Assert.Equal(maxInt, stats.MaxConcurrency);
        Assert.Equal(maxLong, stats.TotalMessagesSent);
        Assert.Equal(maxLong, stats.TotalMessagesFailed);
        Assert.Equal(maxDouble, stats.AverageMessagesPerSecond);
        Assert.Equal(maxInt, stats.CurrentConcurrency);
    }

    [Fact]
    public void Timestamp_ShouldBeUtcAndRecent()
    {
        // Act
        var stats = new RateLimitStats(1, 1, 0, 0, 0.0, 0);

        // Assert
        Assert.Equal(DateTimeKind.Utc, stats.Timestamp.Kind);
        var now = DateTime.UtcNow;
        var diff = now - stats.Timestamp;
        Assert.InRange(diff.TotalSeconds, 0, 2); // allow a small margin for test execution time
    }
}
