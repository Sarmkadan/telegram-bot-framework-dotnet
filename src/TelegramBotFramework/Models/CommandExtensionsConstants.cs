#nullable enable

namespace TelegramBotFramework.Models;

/// <summary>
/// Constants used in <see cref="CommandExtensions"/>.
/// </summary>
internal static class CommandExtensionsConstants
{
    /// <summary>
    /// The text to display when a command has no patterns.
    /// </summary>
    public const string NoPatterns = "no patterns";

    /// <summary>
    /// The text to display when a command has no parameters.
    /// </summary>
    public const string WithoutParameters = "without parameters";

    /// <summary>
    /// The text to display when a command has no rate limit.
    /// </summary>
    public const string NoRateLimit = "no rate limit";

    /// <summary>
    /// The date format string for displaying command creation time.
    /// </summary>
    public const string DateFormat = "yyyy-MM-dd";
}