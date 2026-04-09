// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Models;

/// <summary>
/// Represents a bot command that can be executed by users.
/// </summary>
public class Command
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string HandlerType { get; set; } = string.Empty;

    public CommandType Type { get; set; } = CommandType.Standard;

    public bool RequiresAdmin { get; set; }

    public bool IsEnabled { get; set; } = true;

    public int ExecutionCount { get; set; }

    public List<CommandParameter>? Parameters { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string? Alias { get; set; }

    public int? RateLimitPerMinute { get; set; }

    /// <summary>
    /// Validates the command definition.
    /// </summary>
    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new InvalidOperationException("Command name is required");

        if (!Name.StartsWith("/") && Type == CommandType.Standard)
            throw new InvalidOperationException("Standard commands must start with /");

        if (string.IsNullOrWhiteSpace(HandlerType))
            throw new InvalidOperationException("HandlerType is required");

        return true;
    }

    /// <summary>
    /// Gets the full command pattern including alias.
    /// </summary>
    public IEnumerable<string> GetCommandPatterns()
    {
        yield return Name;
        if (!string.IsNullOrWhiteSpace(Alias))
            yield return Alias;
    }

    /// <summary>
    /// Checks if a user can execute this command based on role.
    /// </summary>
    public bool CanExecuteBy(UserRole role)
    {
        if (!IsEnabled)
            return false;

        if (RequiresAdmin && role < UserRole.Administrator)
            return false;

        return true;
    }

    /// <summary>
    /// Increments execution count.
    /// </summary>
    public void RecordExecution()
    {
        ExecutionCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if command is rate limited.
    /// </summary>
    public bool IsRateLimited(int executionsInLastMinute) =>
        RateLimitPerMinute.HasValue && executionsInLastMinute >= RateLimitPerMinute.Value;
}

public enum CommandType
{
    Standard = 0,
    Menu = 1,
    Inline = 2,
    Callback = 3
}

public class CommandParameter
{
    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = "string";

    public bool IsRequired { get; set; } = true;

    public string? DefaultValue { get; set; }

    public string? Description { get; set; }

    public string? Pattern { get; set; }
}
