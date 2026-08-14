using System;
using System.Collections.Generic;
using TelegramBotFramework.Models;
using Xunit;

namespace TelegramBotFramework.Tests;

public class InlineQueryTests
{
    [Fact]
    public void Validate_ReturnsTrue_WhenAllRequiredFieldsAreValid()
    {
        var query = new InlineQuery
        {
            QueryId = "abc123",
            UserId = 42
        };

        var result = query.Validate();

        Assert.True(result);
    }

    [Fact]
    public void Validate_Throws_WhenQueryIdIsEmpty()
    {
        var query = new InlineQuery
        {
            QueryId = string.Empty,
            UserId = 1
        };

        var ex = Assert.Throws<InvalidOperationException>(() => query.Validate());
        Assert.Equal("QueryId is required", ex.Message);
    }

    [Fact]
    public void Validate_Throws_WhenUserIdIsNotPositive()
    {
        var query = new InlineQuery
        {
            QueryId = "valid",
            UserId = 0
        };

        var ex = Assert.Throws<InvalidOperationException>(() => query.Validate());
        Assert.Equal("UserId must be positive", ex.Message);
    }

    [Fact]
    public void SetMetadata_InitializesDictionary_And_GetMetadata_ReturnsValue()
    {
        var query = new InlineQuery
        {
            QueryId = "id",
            UserId = 1
        };

        query.SetMetadata("key", 123);
        var value = query.GetMetadata("key");

        Assert.NotNull(query.Metadata);
        Assert.Equal(123, value);
    }

    [Fact]
    public void GetMetadata_ReturnsNull_WhenKeyDoesNotExist()
    {
        var query = new InlineQuery
        {
            QueryId = "id",
            UserId = 1
        };

        var value = query.GetMetadata("nonexistent");

        Assert.Null(value);
    }

    [Fact]
    public void GetProcessingDurationMs_ReturnsMinusOne_WhenNotAnswered()
    {
        var query = new InlineQuery
        {
            QueryId = "id",
            UserId = 1,
            ReceivedAt = DateTime.UtcNow,
            AnsweredAt = null
        };

        var duration = query.GetProcessingDurationMs();

        Assert.Equal(-1, duration);
    }

    [Fact]
    public void GetProcessingDurationMs_ReturnsCorrectMilliseconds_WhenAnswered()
    {
        var received = DateTime.UtcNow;
        var answered = received.AddMilliseconds(250);
        var query = new InlineQuery
        {
            QueryId = "id",
            UserId = 1,
            ReceivedAt = received,
            AnsweredAt = answered
        };

        var duration = query.GetProcessingDurationMs();

        Assert.InRange(duration, 240, 260); // allow small timing variance
    }

    [Fact]
    public void DefaultValues_AreSetCorrectly()
    {
        var query = new InlineQuery();

        Assert.Equal(string.Empty, query.Query);
        Assert.Equal(string.Empty, query.Offset);
        Assert.Equal(InlineQueryStatus.Pending, query.Status);
        Assert.NotNull(query.ReceivedAt);
        Assert.Null(query.AnsweredAt);
        Assert.Null(query.Metadata);
    }
}
