#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Keyboard;

/// <summary>
/// Represents a single button within a <see cref="ReplyKeyboardMarkup"/>.
/// </summary>
public sealed class ReplyKeyboardButton
{
    /// <summary>Gets the label displayed to the user.</summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the request contact value indicating if pressing the button will prompt the user to send their contact (optional).
    /// </summary>
    public bool RequestContact { get; set; }

    /// <summary>
    /// Gets or sets the request location value indicating if pressing the button will prompt the user to send their location (optional).
    /// </summary>
    public bool RequestLocation { get; set; }

}
