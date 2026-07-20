#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================
using System.Collections.Concurrent;

namespace TelegramBotFramework.Services;

/// <summary>
/// Thread-safe service for tracking command usage statistics.
/// </summary>
public sealed class CommandUsageTracker : ICommandUsageTracker
{
    private readonly ConcurrentDictionary<string, CommandUsageStats> _commandStats = new();
    private readonly object _statsLock = new();
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(1);
    private DateTime _lastCleanup = DateTime.UtcNow;

    /// <summary>
    /// Records a command invocation.
    /// </summary>
    /// <param name="commandName">Name of the command that was invoked</param>
    public void RecordCommandInvocation(string commandName)
    {
        if (string.IsNullOrWhiteSpace(commandName))
        {
            return;
        }

        // Normalize command name (ensure it starts with /)
        var normalizedName = commandName.StartsWith("/")
            ? commandName
            : $"/{commandName}";

        var now = DateTime.UtcNow;

        // Periodically clean up unused entries to prevent unbounded growth
        CleanupUnusedCommands(now);

        _commandStats.AddOrUpdate(
            normalizedName,
            key => new CommandUsageStats
            {
                TotalInvocations = 1,
                FirstUsedAt = now,
                LastUsedAt = now
            },
            (key, existingStats) =>
            {
                existingStats.TotalInvocations++;
                existingStats.LastUsedAt = now;
                return existingStats;
            }
        );
    }

    /// <summary>
    /// Gets the top N most frequently used commands.
    /// </summary>
    /// <param name="count">Number of top commands to return</param>
    /// <returns>List of command names and invocation counts, sorted by count descending</returns>
    public IList<(string CommandName, int InvocationCount)> GetTopCommands(int count)
    {
        if (count <= 0)
        {
            return new List<(string, int)>();
        }

        return _commandStats
            .OrderByDescending(kvp => kvp.Value.TotalInvocations)
            .Take(count)
            .Select(kvp => (kvp.Key, kvp.Value.TotalInvocations))
            .ToList();
    }

    /// <summary>
    /// Gets the last used timestamp for a specific command.
    /// </summary>
    /// <param name="commandName">Name of the command</param>
    /// <returns>Last used timestamp or null if never used</returns>
    public DateTime? GetLastUsedTimestamp(string commandName)
    {
        if (string.IsNullOrWhiteSpace(commandName))
        {
            return null;
        }

        var normalizedName = commandName.StartsWith("/")
            ? commandName
            : $"/{commandName}";

        if (_commandStats.TryGetValue(normalizedName, out var stats))
        {
            return stats.LastUsedAt;
        }

        return null;
    }

    /// <summary>
    /// Gets all command usage statistics.
    /// </summary>
    /// <returns>Dictionary mapping command names to their usage statistics</returns>
    public IDictionary<string, CommandUsageStats> GetAllCommandStats()
    {
        return new Dictionary<string, CommandUsageStats>(_commandStats);
    }

    /// <summary>
    /// Cleans up commands that haven't been used in a long time.
    /// </summary>
    private void CleanupUnusedCommands(DateTime now)
    {
        // Only clean up periodically to avoid performance overhead
        if (now - _lastCleanup < CleanupInterval)
        {
            return;
        }

        lock (_statsLock)
        {
            var commandsToRemove = _commandStats
                .Where(kvp => kvp.Value.LastUsedAt.HasValue)
                .Where(kvp => now - kvp.Value.LastUsedAt!.Value > TimeSpan.FromDays(30))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var commandName in commandsToRemove)
            {
                _commandStats.TryRemove(commandName, out _);
            }

            _lastCleanup = now;
        }
    }
}
