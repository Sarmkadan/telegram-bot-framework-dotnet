#nullable enable
using System;
using TelegramBotFramework.ConversationFlow;
using Xunit;

namespace TelegramBotFramework.Tests;

public sealed class ConversationFlowOptionsExtensionsTests
{
    [Fact]
    public void FluentConfigurationMethods_SetValuesAndReturnSameInstance()
    {
        var options = new ConversationFlowOptions();
        var timeout = TimeSpan.FromTicks(1);

        var timeoutResult = options.WithDefaultFlowTimeout(timeout);
        var maximumResult = options.WithMaxActiveFlowsPerUser(1);
        var abortResult = options.EnableAbortKeyword("stop", "Stopped.");

        Assert.Same(options, timeoutResult);
        Assert.Same(options, maximumResult);
        Assert.Same(options, abortResult);
        Assert.Equal(timeout, options.DefaultFlowTimeout);
        Assert.Equal(1, options.MaxActiveFlowsPerUser);
        Assert.Equal("stop", options.AbortKeyword);
        Assert.Equal("Stopped.", options.AbortAcknowledgementMessage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void WithDefaultFlowTimeout_NonPositiveTimeout_Throws(long ticks)
    {
        var options = new ConversationFlowOptions();

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => options.WithDefaultFlowTimeout(TimeSpan.FromTicks(ticks)));

        Assert.Equal("timeout", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void WithMaxActiveFlowsPerUser_ValueBelowOne_Throws(int maximum)
    {
        var options = new ConversationFlowOptions();

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => options.WithMaxActiveFlowsPerUser(maximum));

        Assert.Equal("maxActiveFlows", exception.ParamName);
    }

    [Theory]
    [InlineData(null, "Stopped.", "abortKeyword")]
    [InlineData("", "Stopped.", "abortKeyword")]
    [InlineData("stop", null, "acknowledgementMessage")]
    [InlineData("stop", "", "acknowledgementMessage")]
    public void EnableAbortKeyword_NullOrEmptyValue_Throws(
        string? keyword,
        string? acknowledgement,
        string expectedParameter)
    {
        var options = new ConversationFlowOptions();

        var exception = Assert.Throws<ArgumentException>(
            () => options.EnableAbortKeyword(keyword!, acknowledgement!));

        Assert.Equal(expectedParameter, exception.ParamName);
    }

    [Fact]
    public void PublicMethods_NullOptions_ThrowArgumentNullException()
    {
        ConversationFlowOptions? options = null;

        Assert.Throws<ArgumentNullException>(
            () => options!.WithDefaultFlowTimeout(TimeSpan.FromMinutes(1)));
        Assert.Throws<ArgumentNullException>(
            () => options!.WithMaxActiveFlowsPerUser(1));
        Assert.Throws<ArgumentNullException>(
            () => options!.EnableAbortKeyword("stop", "Stopped."));
        Assert.Throws<ArgumentNullException>(() => options!.Validate());
    }

    [Fact]
    public void Validate_ValidBoundaryValuesAndDisabledAbortKeyword_DoesNotThrow()
    {
        var options = new ConversationFlowOptions
        {
            DefaultFlowTimeout = TimeSpan.FromTicks(1),
            MaxActiveFlowsPerUser = 1,
            MaxHistoryPerUser = 0,
            CleanupIntervalMinutes = 1,
            AbortKeyword = string.Empty
        };

        options.Validate();
    }

    [Theory]
    [InlineData("timeout")]
    [InlineData("maximum")]
    [InlineData("history")]
    [InlineData("cleanup")]
    [InlineData("keyword")]
    [InlineData("acknowledgement")]
    public void Validate_InvalidSetting_Throws(string invalidSetting)
    {
        var options = new ConversationFlowOptions();
        switch (invalidSetting)
        {
            case "timeout": options.DefaultFlowTimeout = TimeSpan.Zero; break;
            case "maximum": options.MaxActiveFlowsPerUser = 0; break;
            case "history": options.MaxHistoryPerUser = -1; break;
            case "cleanup": options.CleanupIntervalMinutes = 0; break;
            case "keyword": options.AbortKeyword = " "; break;
            case "acknowledgement": options.AbortAcknowledgementMessage = " "; break;
        }

        Assert.ThrowsAny<ArgumentException>(() => options.Validate());
    }
}
