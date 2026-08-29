#nullable enable

namespace TelegramBotFramework.Keyboard;

/// <summary>
/// Contains constant values used by InlineKeyboardBuilderExtensions.
/// </summary>
internal static class InlineKeyboardBuilderExtensionsConstants
{
    /// <summary>
    /// Default callback data for confirmation button.
    /// </summary>
    public const string ConfirmCallbackData = "confirm";

    /// <summary>
    /// Default callback data for cancel button.
    /// </summary>
    public const string CancelCallbackData = "cancel";

    /// <summary>
    /// Default base callback data for pagination.
    /// </summary>
    public const string PaginationBaseCallbackData = "page";

    /// <summary>
    /// Text for the confirm button.
    /// </summary>
    public const string ConfirmButtonText = "✅ Confirm";

    /// <summary>
    /// Text for the cancel button.
    /// </summary>
    public const string CancelButtonText = "❌ Cancel";

    /// <summary>
    /// Text for the previous page button.
    /// </summary>
    public const string PreviousPageButtonText = "⬅️ Previous";

    /// <summary>
    /// Format string for the current page button.
    /// </summary>
    public const string CurrentPageButtonTextFormat = "📄 Page {0}";

    /// <summary>
    /// Text for the next page button.
    /// </summary>
    public const string NextPageButtonText = "Next ➡️";

    /// <summary>
    /// Default callback data for no-operation buttons.
    /// </summary>
    public const string NoOpCallbackData = "noop";

    /// <summary>
    /// Suffix for current page indicator in pagination.
    /// </summary>
    public const string CurrentPageSuffix = "_current";
}