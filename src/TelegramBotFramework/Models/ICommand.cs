#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Models;

/// <summary>
/// Represents a bot command that can be executed by users.
/// </summary>
public interface ICommand
{
    string Name { get; set; }

    string Description { get; set; }

    string HandlerType { get; set; }

    CommandType Type { get; set; }

    bool RequiresAdmin { get; set; }

    bool IsEnabled { get; set; }

    int ExecutionCount { get; set; }

    List<CommandParameter>? Parameters { get; set; }

    List<string> Aliases { get; set; }

    DateTime CreatedAt { get; set; }

    DateTime UpdatedAt { get; set; }

    int? RateLimitPerMinute { get; set; }

    /// <summary>
    /// Validates the command definition.
    /// </summary>
    bool Validate();

    /// <summary>
    /// Gets all command patterns including primary name and aliases.
    /// </summary>
    IEnumerable<string> GetCommandPatterns();

    /// <summary>
    /// Checks if a user can execute this command based on role.
    /// </summary>
    bool CanExecuteBy(UserRole role);

    /// <summary>
    /// Increments execution count.
    /// </summary>
    void RecordExecution();

    /// <summary>
    /// Checks if command is rate limited.
    /// </summary>
    bool IsRateLimited(int executionsInLastMinute);
}