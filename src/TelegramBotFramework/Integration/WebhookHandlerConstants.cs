#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

namespace TelegramBotFramework.Integration;

/// <summary>
/// Constants for the WebhookHandler class.
/// </summary>
internal static class WebhookHandlerConstants
{
    /// <summary>
    /// The name of the header used for Telegram webhook secret token validation.
    /// </summary>
    public const string TelegramSecretTokenHeaderName = "X-Telegram-Bot-Api-Secret-Token";

    /// <summary>
    /// Maximum allowed length for message text to prevent DoS attacks.
    /// </summary>
    public const int MaxMessageTextLength = 10_000; // 10KB - Telegram's typical limit is 4096 chars

    /// <summary>
    /// Maximum allowed length for caption to prevent DoS attacks.
    /// </summary>
    public const int MaxCaptionLength = 10_000; // 10KB - same as message text

    /// <summary>
    /// Maximum number of message entities (mentions, links, etc.).
    /// </summary>
    public const int MaxEntityCount = 100;

    /// <summary>
    /// Maximum number of rows in inline keyboard.
    /// </summary>
    public const int MaxInlineKeyboardRows = 100;

    /// <summary>
    /// Maximum number of buttons per row in inline keyboard.
    /// </summary>
    public const int MaxInlineKeyboardColumns = 10;

    /// <summary>
    /// Overall message size limit to prevent DoS attacks.
    /// </summary>
    public const int MaxMessageLength = 20_000; // 20KB - overall message size limit

    /// <summary>
    /// Callback data limit.
    /// </summary>
    public const int MaxCallbackDataLength = 1_000; // 1KB - callback data limit
}