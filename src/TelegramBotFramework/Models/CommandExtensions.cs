#nullable enable

namespace TelegramBotFramework.Models;

/// <summary>
/// Provides extension methods for the <see cref="Command"/> class to enhance command functionality.
/// </summary>
public static class CommandExtensions
{
    /// <summary>
    /// Determines whether the command has any parameters defined.
    /// </summary>
    /// <param name="command">The command instance.</param>
    /// <returns>True if the command has parameters; otherwise, false.</returns>
    public static bool HasParameters(this Command command)
    {
        return command.Parameters is not null && command.Parameters.Count > 0;
    }

    /// <summary>
    /// Gets the primary command pattern (the name) for display purposes.
    /// </summary>
    /// <param name="command">The command instance.</param>
    /// <returns>The primary command pattern.</returns>
    public static string GetPrimaryPattern(this Command command)
    {
        return command.Name;
    }

    /// <summary>
    /// Determines whether the command is a standard slash command.
    /// </summary>
    /// <param name="command">The command instance.</param>
    /// <returns>True if the command is a standard slash command; otherwise, false.</returns>
    public static bool IsStandardCommand(this Command command)
    {
        return command.Type == CommandType.Standard;
    }

    /// <summary>
    /// Gets a formatted string representation of the command for logging and display.
    /// </summary>
    /// <param name="command">The command instance.</param>
    /// <returns>A formatted string containing command details.</returns>
    public static string GetFormattedString(this Command command)
    {
        var patterns = command.GetCommandPatterns().ToList();
        var patternText = patterns.Count > 0
            ? string.Join(", ", patterns.Select(p => $"'{p}'"))
            : "no patterns";

        var parametersText = command.HasParameters()
            ? $"with {command.Parameters!.Count} parameter(s)"
            : "without parameters";

        var rateLimitText = command.RateLimitPerMinute.HasValue
            ? $"rate limited to {command.RateLimitPerMinute} per minute"
            : "no rate limit";

        return $"Command '{command.Name}' ({command.Type}) - {command.Description} " +
               $"[{patternText}, {parametersText}, {rateLimitText}] " +
               $"[Created: {command.CreatedAt:yyyy-MM-dd}, Executions: {command.ExecutionCount}]";
    }
}