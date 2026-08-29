#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================
using FluentAssertions;
using TelegramBotFramework.Services;
using Xunit;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Tests for the CommandUsageTracker class.
/// </summary>
public sealed class CommandUsageTrackerTests : ICommandUsageTrackerTests
{
    private readonly CommandUsageTracker _tracker = new();

    /// <summary>
    /// Tests that RecordCommandInvocation correctly records a command invocation.
    /// </summary>
    [Fact]
    public void RecordCommandInvocation_RecordsInvocation()
    {
        // Act
        _tracker.RecordCommandInvocation("/test");
        _tracker.RecordCommandInvocation("/test");
        _tracker.RecordCommandInvocation("/other");

        // Assert
        var topCommands = _tracker.GetTopCommands(10);
        topCommands.Should().HaveCount(2);
        topCommands[0].CommandName.Should().Be("/test");
        topCommands[0].InvocationCount.Should().Be(2);
        topCommands[1].CommandName.Should().Be("/other");
        topCommands[1].InvocationCount.Should().Be(1);
    }

    /// <summary>
    /// Tests that command names are normalized to start with a slash.
    /// </summary>
    [Fact]
    public void RecordCommandInvocation_NormalizesCommandName()
    {
        // Act
        _tracker.RecordCommandInvocation("test"); // No leading slash
        _tracker.RecordCommandInvocation("/test2"); // With leading slash

        // Assert
        var topCommands = _tracker.GetTopCommands(10);
        topCommands.Should().HaveCount(2);
        topCommands[0].CommandName.Should().Be("/test");
        topCommands[1].CommandName.Should().Be("/test2");
    }

    /// <summary>
    /// Tests that GetTopCommands returns commands sorted by invocation count in descending order.
    /// </summary>
    [Fact]
    public void GetTopCommands_ReturnsCommandsSortedByCountDescending()
    {
        // Arrange
        _tracker.RecordCommandInvocation("/least");
        _tracker.RecordCommandInvocation("/most");
        _tracker.RecordCommandInvocation("/most");
        _tracker.RecordCommandInvocation("/most");
        _tracker.RecordCommandInvocation("/middle");
        _tracker.RecordCommandInvocation("/middle");

        // Act
        var topCommands = _tracker.GetTopCommands(2);

        // Assert
        topCommands.Should().HaveCount(2);
        topCommands[0].CommandName.Should().Be("/most");
        topCommands[0].InvocationCount.Should().Be(3);
        topCommands[1].CommandName.Should().Be("/middle");
        topCommands[1].InvocationCount.Should().Be(2);
    }

    /// <summary>
    /// Tests that GetTopCommands returns an empty list when count is zero or negative.
    /// </summary>
    [Fact]
    public void GetTopCommands_WithZeroOrNegativeCount_ReturnsEmptyList()
    {
        // Arrange
        _tracker.RecordCommandInvocation("/test");

        // Act
        var empty1 = _tracker.GetTopCommands(0);
        var empty2 = _tracker.GetTopCommands(-1);

        // Assert
        empty1.Should().BeEmpty();
        empty2.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that GetLastUsedTimestamp returns the correct timestamp for a command.
    /// </summary>
    [Fact]
    public void GetLastUsedTimestamp_ReturnsCorrectTimestamp()
    {
        // Arrange
        var before = DateTime.UtcNow.AddMilliseconds(-10);
        _tracker.RecordCommandInvocation("/test");
        var after = DateTime.UtcNow.AddMilliseconds(10);

        // Act
        var lastUsed = _tracker.GetLastUsedTimestamp("/test");

        // Assert
        lastUsed.Should().NotBeNull();
        lastUsed.Should().BeOnOrAfter(before);
        lastUsed.Should().BeOnOrBefore(after);
    }

    /// <summary>
    /// Tests that GetLastUsedTimestamp returns null for a command that was never invoked.
    /// </summary>
    [Fact]
    public void GetLastUsedTimestamp_ForNeverUsedCommand_ReturnsNull()
    {
        // Act
        var lastUsed = _tracker.GetLastUsedTimestamp("/nonexistent");

        // Assert
        lastUsed.Should().BeNull();
    }

    /// <summary>
    /// Tests that GetAllCommandStats returns all recorded statistics.
    /// </summary>
    [Fact]
    public void GetAllCommandStats_ReturnsAllStatistics()
    {
        // Arrange
        _tracker.RecordCommandInvocation("/test1");
        _tracker.RecordCommandInvocation("/test2");
        _tracker.RecordCommandInvocation("/test1");

        // Act
        var allStats = _tracker.GetAllCommandStats();

        // Assert
        allStats.Should().HaveCount(2);
        allStats.Should().ContainKey("/test1");
        allStats.Should().ContainKey("/test2");
        allStats["/test1"].TotalInvocations.Should().Be(2);
        allStats["/test2"].TotalInvocations.Should().Be(1);
    }

    /// <summary>
    /// Tests that RecordCommandInvocation with null or empty command name doesn't throw.
    /// </summary>
    [Fact]
    public void RecordCommandInvocation_WithNullOrEmptyCommandName_DoesNotThrow()
    {
        // Act & Assert
        _tracker.Invoking(t => t.RecordCommandInvocation(null)).Should().NotThrow();
        _tracker.Invoking(t => t.RecordCommandInvocation("")).Should().NotThrow();
        _tracker.Invoking(t => t.RecordCommandInvocation("   ")).Should().NotThrow();
    }

    /// <summary>
    /// Tests that GetLastUsedTimestamp with null or empty command name returns null.
    /// </summary>
    [Fact]
    public void GetLastUsedTimestamp_WithNullOrEmptyCommandName_ReturnsNull()
    {
        // Act
        var result1 = _tracker.GetLastUsedTimestamp(null);
        var result2 = _tracker.GetLastUsedTimestamp("");
        var result3 = _tracker.GetLastUsedTimestamp("   ");

        // Assert
        result1.Should().BeNull();
        result2.Should().BeNull();
        result3.Should().BeNull();
    }

    /// <summary>
    /// Tests that the tracker correctly tracks first and last used timestamps.
    /// </summary>
    [Fact]
    public void RecordCommandInvocation_TracksFirstAndLastUsedTimestamps()
    {
        // Arrange
        var firstCallTime = DateTime.UtcNow;
        _tracker.RecordCommandInvocation("/test");
        var afterFirstCall = DateTime.UtcNow;

        System.Threading.Thread.Sleep(10); // Ensure time passes

        var secondCallTime = DateTime.UtcNow;
        _tracker.RecordCommandInvocation("/test");
        var afterSecondCall = DateTime.UtcNow;

        // Act
        var stats = _tracker.GetAllCommandStats()["/test"];

        // Assert
        stats.FirstUsedAt.Should().BeOnOrAfter(firstCallTime).And.BeOnOrBefore(afterFirstCall);
        stats.LastUsedAt.Should().BeOnOrAfter(secondCallTime).And.BeOnOrBefore(afterSecondCall);
        stats.TotalInvocations.Should().Be(2);
    }
}