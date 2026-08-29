#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Models;

/// <summary>
/// Represents a bot command that can be executed by users.
/// </summary>
public sealed class Command : ICommand, IEquatable<Command>
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string HandlerType { get; set; } = string.Empty;

    public CommandType Type { get; set; } = CommandType.Standard;

    public bool RequiresAdmin { get; set; }

    public bool IsEnabled { get; set; } = true;

    public int ExecutionCount { get; set; }

    public List<CommandParameter>? Parameters { get; set; }

    public List<string> Aliases { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

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
    /// Gets all command patterns including primary name and aliases.
    /// </summary>
    public IEnumerable<string> GetCommandPatterns()
    {
        yield return Name;
        foreach (var alias in Aliases)
        {
            if (!string.IsNullOrWhiteSpace(alias))
                yield return alias;
        }
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

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>true if the current object is equal to the <paramref name="other">parameter</paramref>; otherwise, false.</returns>
    public bool Equals(Command? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name
               && Description == other.Description
               && HandlerType == other.HandlerType
               && Type == other.Type
               && RequiresAdmin == other.RequiresAdmin
               && IsEnabled == other.IsEnabled
               && ExecutionCount == other.ExecutionCount
               && Equals(Parameters, other.Parameters);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Command)obj);
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Description, HandlerType, Type, RequiresAdmin, IsEnabled, ExecutionCount, Parameters);
    }

    /// <summary>
    /// Returns a value that indicates whether the values of two <see cref="Command"/> objects are equal.
    /// </summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns>true if the <paramref name="left"/> and <paramref name="right"/> parameters have the same value; otherwise, false.</returns>
    public static bool operator ==(Command? left, Command? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (ReferenceEquals(null, left)) return false;
        if (ReferenceEquals(null, right)) return false;
        return left.Equals(right);
    }

    /// <summary>
    /// Returns a value that indicates whether the values of two <see cref="Command"/> objects are not equal.
    /// </summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns>true if <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, false.</returns>
    public static bool operator !=(Command? left, Command? right)
    {
        return !(left == right);
    }
}

public enum CommandType
{
    Standard = 0,
    Menu = 1,
    Inline = 2,
    Callback = 3
}

public sealed class CommandParameter
{
    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = "string";

    public bool IsRequired { get; set; } = true;

    public string? DefaultValue { get; set; }

    public string? Description { get; set; }

    public string? Pattern { get; set; }
}