#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Attributes;

/// <summary>
/// Marks a class as a handler for a specific bot command.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class CommandAttribute : Attribute
{
    /// <summary>
    /// The command name (e.g. "start", "help"). Leading slash is optional.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Optional human-readable description shown in the /help listing.
    /// </summary>
    public string? Description { get; set; }

    public CommandAttribute(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Command name must not be empty.", nameof(name));

        Name = name.TrimStart('/');
    }
}
