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
        _tracker.RecordCommandInvocation(CommandUsageTrackerTestsConstants.TestCommand);
        _tracker.RecordCommandInvocation(CommandUsageTrackerTestsConstants.TestCommand);
        _tracker.RecordCommandInvocation(CommandUsageTrackerTestsConstants.OtherCommand);

        // Assert
        var topCommands = _tracker.GetTopCommands(CommandUsageTrackerTestsConstants.TopCommandsCount);
        topCommands.Should().HaveCount(2);
        topCommands[0].CommandName.Should().Be(CommandUsageTrackerTestsConstants.TestCommand);
        topCommands[0].InvocationCount.Should().Be(2);
        topCommands[1].CommandName.Should().Be(CommandUsageTrackerTestsConstants.OtherCommand);
        topCommands[1].InvocationCount.Should().Be(1);
    }

    /// <summary>
    /// Tests that command names are normalized to start with a slash.
    /// </summary>
    [Fact]
    public void RecordCommandInvocation_NormalizesCommandName()
    {
        // Act
        _tracker.RecordCommandInvocation(CommandUsageTrackerTestsConstants.TestInputWithoutSlash); // No leading slash
        _tracker.RecordCommandInvocation(CommandUsageTrackerTestsConstants.Test2Command); // With leading slash

        // Assert
        var topCommands = _tracker.GetTopCommands(CommandUsageTrackerTestsConstants.TopCommandsCount);
        topCommands.Should().HaveCount(2);
        topCommands[0].CommandName.Should().Be(CommandUsageTrackerTestsConstants.TestCommand);
        topCommands[1].CommandName.Should().Be(CommandUsageTrackerTestsConstants.Test2Command);
    }

    /// <summary>
    /// Tests that GetTopCommands returns commands sorted by invocation count in descending order.
    /// </summary>
    [Fact]
    public void GetTopCommands_ReturnsCommandsSortedByCountDescending()
    {
        // Arrange
        _tracker.RecordCommandInvocation(CommandUsageTrackerTestsConstants.LeastCommand);
        _tracker.RecordCommandInvocation(CommandUsageTrackerTestsConstants.MostCommand);
        _tracker.RecordCommandInvocation(CommandUsageTrackerTestsConstants.MostCommand);
        _tracker.RecordCommandInvocation(CommandUsageTrackerTestsConstants.MostCommand);
        _tracker.RecordCommandInvocation(CommandUsageTrackerTestsConstants.MiddleCommand);
        _tracker.RecordCommandInvocation(CommandUsageTrackerTestsConstants.MiddleCommand);

        // Act
        var topCommands = _tracker.GetTopCommands(2);

        // Assert
        topCommands.Should().HaveCount(2);
        topCommands[0].CommandName.Should().Be(CommandUsageTrackerTestsConstants.MostCommand);
        topCommands[0].InvocationCount.Should().Be(3);
        topCommands[1].CommandName.Should().Be(CommandUsageTrackerTestsConstants.MiddleCommand);
        topCommands[1].InvocationCount.Should().Be(2);
    }

    /// <summary>
    /// Tests that GetTopCommands returns an empty list when count is zero or negative.
    /// </summary>
    [Fact]
    public void GetTopCommands_WithZeroOrNegativeCount_ReturnsEmptyList()
    {
        // Arrange
        _tracker.RecordCommandInvocation(CommandUsageTrackerTestsConstants.TestCommand);

        // Act
        var empty1 = _tracker.GetTopCommands(CommandUsageTrackerTestsConstants.ZeroCount);
        var empty2 = _tracker.GetTopCommands(CommandUsageTrackerTestsConstants.NegativeOneCount);

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
        var before = DateTime.UtcNow.AddMilliseconds(-CommandUsageTrackerTestsConstants.TimestampDeltaMilliseconds);
        _tracker.RecordCommandInvocation(CommandUsageTrackerTestsConstants.TestCommand);
        var after = DateTime.UtcNow.AddMilliseconds(10);

        // Act
        var lastUsed = _tracker.GetLastUsedTimestamp(CommandUsageTrackerTestsConstants.TestCommand);

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
        var lastUsed = _tracker.GetLastUsedTimestamp(CommandUsageTrackerTestsConstants.NonexistentCommand);

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
        _tracker.RecordCommandInvocation(CommandUsageTrackerTestsConstants.Test1Command);
        _tracker.RecordCommandInvocation(CommandUsageTrackerTestsConstants.Test2Command);
        _tracker.RecordCommandInvocation(CommandUsageTrackerTestsConstants.Test1Command);

        // Act
        var allStats = _tracker.GetAllCommandStats();

        // Assert
        allStats.Should().HaveCount(2);
        allStats.Should().ContainKey(CommandUsageTrackerTestsConstants.Test1Command);
        allStats.Should().ContainKey(CommandUsageTrackerTestsConstants.Test2Command);
        allStats[CommandUsageTrackerTestsConstants.Test1Command].TotalInvocations.Should().Be(2);
        allStats[CommandUsageTrackerTestsConstants.Test2Command].TotalInvocations.Should().Be(1);
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
        _tracker.RecordCommandInvocation(CommandUsageTrackerTestsConstants.TestCommand);
        var afterFirstCall = DateTime.UtcNow;

        System.Threading.Thread.Sleep(CommandUsageTrackerTestsConstants.ShortSleepMilliseconds); // Ensure time passes

        var secondCallTime = DateTime.UtcNow;
        _tracker.RecordCommandInvocation(CommandUsageTrackerTestsConstants.TestCommand);
        var afterSecondCall = DateTime.UtcNow;

        // Act
        var stats = _tracker.GetAllCommandStats()[CommandUsageTrackerTestsConstants.TestCommand];

        // Assert
        stats.FirstUsedAt.Should().BeOnOrAfter(firstCallTime).And.BeOnOrBefore(afterFirstCall);
        stats.LastUsedAt.Should().BeOnOrAfter(secondCallTime).And.BeOnOrBefore(afterSecondCall);
        stats.TotalInvocations.Should().Be(2);
    }
}