using System;
using Xunit;
using TelegramBotFramework.ConversationFlow;

namespace TelegramBotFramework.Tests;

public class ConversationFlowOptionsTests
{
    [Fact]
    public void DefaultValues_ShouldMatchExpected()
    {
        var options = new ConversationFlowOptions();

        Assert.Equal(TimeSpan.FromMinutes(30), options.DefaultFlowTimeout);
        Assert.Equal(1, options.MaxActiveFlowsPerUser);
        Assert.True(options.AutoResumeOnSessionRestore);
        Assert.Equal(50, options.MaxHistoryPerUser);
        Assert.Equal(
            "Your conversation was interrupted. You can start over at any time.",
            options.FlowAbandonedMessage);
        Assert.Equal(
            "Your session has timed out due to inactivity. Please start the conversation again.",
            options.FlowTimeoutMessage);
        Assert.True(options.EnableFlowEvents);
        Assert.Equal(60, options.CleanupIntervalMinutes);
        Assert.Equal("/cancel", options.AbortKeyword);
        Assert.Equal(
            "Conversation cancelled. Use the menu to start again.",
            options.AbortAcknowledgementMessage);
        Assert.Equal(FlowEvictionPolicy.SilentDiscard, options.TimeoutEvictionPolicy);
        Assert.Null(options.OnEviction);
    }

    [Fact]
    public void PropertyAssignments_ShouldPersistValues()
    {
        var options = new ConversationFlowOptions
        {
            DefaultFlowTimeout = TimeSpan.FromHours(1),
            MaxActiveFlowsPerUser = 5,
            AutoResumeOnSessionRestore = false,
            MaxHistoryPerUser = 10,
            FlowAbandonedMessage = "Abandoned",
            FlowTimeoutMessage = "Timeout",
            EnableFlowEvents = false,
            CleanupIntervalMinutes = 5,
            AbortKeyword = "stop",
            AbortAcknowledgementMessage = "Cancelled",
            TimeoutEvictionPolicy = FlowEvictionPolicy.NotifyUser
        };

        Assert.Equal(TimeSpan.FromHours(1), options.DefaultFlowTimeout);
        Assert.Equal(5, options.MaxActiveFlowsPerUser);
        Assert.False(options.AutoResumeOnSessionRestore);
        Assert.Equal(10, options.MaxHistoryPerUser);
        Assert.Equal("Abandoned", options.FlowAbandonedMessage);
        Assert.Equal("Timeout", options.FlowTimeoutMessage);
        Assert.False(options.EnableFlowEvents);
        Assert.Equal(5, options.CleanupIntervalMinutes);
        Assert.Equal("stop", options.AbortKeyword);
        Assert.Equal("Cancelled", options.AbortAcknowledgementMessage);
        Assert.Equal(FlowEvictionPolicy.NotifyUser, options.TimeoutEvictionPolicy);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void AbortKeyword_CanBeNullOrEmpty(string? keyword)
    {
        var options = new ConversationFlowOptions { AbortKeyword = keyword };
        Assert.Equal(keyword, options.AbortKeyword);
    }

    [Fact]
    public void MaxActiveFlowsPerUser_Zero_IsAllowed()
    {
        var options = new ConversationFlowOptions { MaxActiveFlowsPerUser = 0 };
        Assert.Equal(0, options.MaxActiveFlowsPerUser);
    }
}
