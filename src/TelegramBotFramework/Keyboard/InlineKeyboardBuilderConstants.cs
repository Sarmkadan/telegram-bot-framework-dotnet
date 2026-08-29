#nullable enable

namespace TelegramBotFramework.Keyboard;

/// <summary>
/// Constants for InlineKeyboardBuilder to avoid magic values.
/// </summary>
internal static class InlineKeyboardBuilderConstants
{
    /// <summary>
    /// Default maximum number of buttons per row.
    /// </summary>
    public const int DefaultMaxButtonsPerRow = 3;

    /// <summary>
    /// Error message for empty button text.
    /// </summary>
    public const string ButtonTextCannotBeEmpty = "Button text cannot be empty.";

    /// <summary>
    /// Error message for empty URL.
    /// </summary>
    public const string UrlCannotBeEmpty = "URL cannot be empty.";

    /// <summary>
    /// Error message for empty menu ID.
    /// </summary>
    public const string MenuIdCannotBeEmpty = "Menu ID cannot be empty.";

    /// <summary>
    /// Error message for empty title.
    /// </summary>
    public const string TitleCannotBeEmpty = "Title cannot be empty.";

    /// <summary>
    /// Error message for empty keyboard build.
    /// </summary>
    public const string CannotBuildEmptyKeyboard = "Cannot build an empty keyboard — add at least one button.";

    /// <summary>
    /// Error message for invalid max buttons per row value.
    /// </summary>
    public const string MaxButtonsPerRowMustBeAtLeastOne = "Must be at least 1.";
}