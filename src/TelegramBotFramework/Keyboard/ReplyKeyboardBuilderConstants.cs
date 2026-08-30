#nullable enable

namespace TelegramBotFramework.Keyboard;

/// <summary>
/// Constants for <see cref="ReplyKeyboardBuilder"/>.
/// </summary>
internal static class ReplyKeyboardBuilderConstants
{
    /// <summary>
    /// Default maximum number of buttons per row.
    /// </summary>
    public const int DefaultMaxButtonsPerRow = 2;

    /// <summary>
    /// Error message for empty button text.
    /// </summary>
    public const string ButtonTextCannotBeEmpty = "Button text cannot be empty.";
}