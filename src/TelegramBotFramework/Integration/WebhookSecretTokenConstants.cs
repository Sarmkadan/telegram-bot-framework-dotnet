#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Integration;

/// <summary>
/// Constants for <see cref="WebhookSecretToken"/>.
/// </summary>
internal static class WebhookSecretTokenConstants
{
    /// <summary>Maximum length of a secret token accepted by Telegram's <c>setWebhook</c>.</summary>
    public const int MaxLength = 256;

    /// <summary>Length of a generated secret token when no length is requested.</summary>
    public const int DefaultLength = 64;

    /// <summary>Characters Telegram accepts in a secret token: <c>A-Z</c>, <c>a-z</c>, <c>0-9</c>, <c>_</c> and <c>-</c>.</summary>
    public const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_-";

    /// <summary>Message used when a secret token contains characters or a length Telegram does not accept.</summary>
    public const string InvalidSecretTokenMessage =
        "Secret token must be 1-256 characters from A-Z, a-z, 0-9, '_' and '-'.";
}
