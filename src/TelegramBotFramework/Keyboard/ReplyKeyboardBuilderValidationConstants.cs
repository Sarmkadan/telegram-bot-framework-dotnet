#nullable enable

namespace TelegramBotFramework.Keyboard;

/// <summary>
/// Contains constant values used in <see cref="ReplyKeyboardBuilderValidation"/>.
/// </summary>
internal static class ReplyKeyboardBuilderValidationConstants
{
    /// <summary>
    /// Error message when the validator is used with a non-ReplyKeyboardBuilder instance.
    /// </summary>
    public const string ValidatorOnlyWorksWithReplyKeyboardBuilderInstances = "Validator only works with ReplyKeyboardBuilder instances";

    /// <summary>
    /// Format string for the header of validation failed exceptions.
    /// </summary>
    public const string ValidationFailedHeader = "ReplyKeyboardBuilder validation failed with {0} error(s):";

    /// <summary>
    /// Maximum allowed length for a button's text.
    /// </summary>
    public const int MaxButtonTextLength = 64;

    /// <summary>
    /// Format string for error when a button has empty or whitespace text.
    /// </summary>
    public const string ButtonEmptyTextFormat = "Button at row {0}, position {1} has empty or whitespace text.";

    /// <summary>
    /// Format string for error when a button's text exceeds the maximum allowed length.
    /// </summary>
    public const string ButtonTextTooLongFormat = "Button at row {0}, position {1} has text longer than {2} characters (length: {3}).";
}