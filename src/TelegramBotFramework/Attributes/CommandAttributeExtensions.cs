#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace TelegramBotFramework.Attributes;

/// <summary>
/// Provides extension methods for <see cref="CommandAttribute"/> to enhance command attribute functionality.
/// </summary>
public static class CommandAttributeExtensions
{
    /// <summary>
    /// Determines whether the command name matches the specified input, considering both the primary name and aliases.
    /// </summary>
    /// <param name="attribute">The command attribute.</param>
    /// <param name="input">The input string to compare against.</param>
    /// <returns>True if the input matches the command name or any of its aliases; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="attribute"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
    public static bool Matches(this CommandAttribute attribute, string input)
    {
        ArgumentNullException.ThrowIfNull(attribute);
        ArgumentNullException.ThrowIfNull(input);

        if (string.IsNullOrWhiteSpace(input))
            return false;

        var normalizedInput = input.TrimStart('/');

        if (string.Equals(attribute.Name, normalizedInput, StringComparison.OrdinalIgnoreCase))
            return true;

        return attribute.Aliases.Contains(normalizedInput, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets all possible command names for this attribute, including the primary name and aliases.
    /// </summary>
    /// <param name="attribute">The command attribute.</param>
    /// <returns>An enumerable of all command names.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="attribute"/> is null.</exception>
    public static IEnumerable<string> GetAllNames(this CommandAttribute attribute)
    {
        ArgumentNullException.ThrowIfNull(attribute);

        yield return attribute.Name;

        foreach (var alias in attribute.Aliases)
        {
            yield return alias;
        }
    }

    /// <summary>
    /// Determines whether the command has any aliases defined.
    /// </summary>
    /// <param name="attribute">The command attribute.</param>
    /// <returns>True if the command has one or more aliases; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="attribute"/> is null.</exception>
    public static bool HasAliases(this CommandAttribute attribute)
    {
        ArgumentNullException.ThrowIfNull(attribute);
        return attribute.Aliases.Length > 0;
    }

    /// <summary>
    /// Creates a formatted command string representation suitable for display in help text.
    /// </summary>
    /// <param name="attribute">The command attribute.</param>
    /// <param name="includeDescription">Whether to include the description in the output.</param>
    /// <returns>A formatted string representing the command.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="attribute"/> is null.</exception>
    public static string ToDisplayString(this CommandAttribute attribute, bool includeDescription = true)
    {
        ArgumentNullException.ThrowIfNull(attribute);

        var commandPrefix = "/" + attribute.Name;

        if (!includeDescription || string.IsNullOrWhiteSpace(attribute.Description))
            return commandPrefix;

        return $"{commandPrefix} - {attribute.Description}";
    }
}