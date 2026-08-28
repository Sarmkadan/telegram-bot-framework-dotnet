#nullable enable

namespace TelegramBotFramework.Keyboard;

/// <summary>
/// Defines the contract for a reply keyboard button.
/// </summary>
public interface IReplyKeyboardButton
{
    /// <summary>Gets the label displayed to the user.</summary>
    string Text { get; }

    /// <summary>
    /// Gets or sets the request contact value indicating if pressing the button will prompt the user to send their contact (optional).
    /// </summary>
    bool RequestContact { get; set; }

    /// <summary>
    /// Gets or sets the request location value indicating if pressing the button will prompt the user to send their location (optional).
    /// </summary>
    bool RequestLocation { get; set; }
}