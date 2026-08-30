#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace TelegramBotFramework.Keyboard;

/// <summary>
/// Constants for InlineQueryResultBuilder validation limits and formatting.
/// </summary>
internal static class InlineQueryResultBuilderConstants
{
    /// <summary>
    /// Maximum number of inline query results allowed (Telegram limit).
    /// </summary>
    public const int MaxResultsLimit = 50;

    /// <summary>
    /// Maximum length of result ID in bytes (Telegram limit).
    /// </summary>
    public const int MaxIdLengthBytes = 64;

    /// <summary>
    /// Maximum length of title in characters (Telegram limit).
    /// </summary>
    public const int MaxTitleLength = 64;

    /// <summary>
    /// Maximum length of content in characters (Telegram limit).
    /// </summary>
    public const int MaxContentLength = 1024;

    /// <summary>
    /// Separator used in location coordinate strings (latitude,longitude).
    /// </summary>
    public const string LocationCoordinateSeparator = ",";
}