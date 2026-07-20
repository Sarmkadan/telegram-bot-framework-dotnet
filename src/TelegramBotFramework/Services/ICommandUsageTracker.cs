#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================
using System.Collections.Concurrent;

namespace TelegramBotFramework.Services;

/// <summary>
/// Service for tracking command usage statistics.
/// </summary>
public interface ICommandUsageTracker
{
    /// <summary>
    /// Records a command invocation.
    /// </summary>
    /// <param name="commandName">Name of the command that was invoked</param>
    void RecordCommandInvocation(string commandName);

    /// <summary>
    /// Gets the top N most frequently used commands.
    /// </summary>
    /// <param name="count">Number of top commands to return</param>
    /// <returns>List of command names and invocation counts, sorted by count descending</returns>
    IList<(string CommandName, int InvocationCount)> GetTopCommands(int count);

    /// <summary>
    /// Gets the last used timestamp for a specific command.
    /// </summary>
    /// <param name="commandName">Name of the command</param>
    /// <returns>Last used timestamp or null if never used</returns>
    DateTime? GetLastUsedTimestamp(string commandName);

    /// <summary>
    /// Gets all command usage statistics.
    /// </summary>
    /// <returns>Dictionary mapping command names to their usage statistics</returns>
    IDictionary<string, CommandUsageStats> GetAllCommandStats();
}

/// <summary>
/// Represents command usage statistics.
/// </summary>
public sealed class CommandUsageStats
{
    /// <summary>
    /// Total number of invocations.
    /// </summary>
    public int TotalInvocations { get; set; }

    /// <summary>
    /// Last timestamp when the command was invoked.
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// First timestamp when the command was invoked.
    /// </summary>
    public DateTime? FirstUsedAt { get; set; }
}
