#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Attributes;

/// <summary>
/// Interface for command attribute.
/// </summary>
public interface ICommandAttribute
{
    /// <summary>
    /// The command name (e.g. "start", "help"). Leading slash is optional.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Optional human-readable description shown in the /help listing.
    /// </summary>
    string? Description { get; set; }

    /// <summary>
    /// Optional list of aliases for this command (e.g. ["start", "begin"]).
    /// </summary>
    string[] Aliases { get; set; }
}