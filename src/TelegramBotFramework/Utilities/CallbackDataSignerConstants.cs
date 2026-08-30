#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Utilities;

/// <summary>
/// Constants for CallbackDataSigner.
/// </summary>
internal static class CallbackDataSignerConstants
{
    /// <summary>
    /// Separator character used between data and signature.
    /// </summary>
    public const char Separator = '|';

    /// <summary>
    /// Maximum total bytes allowed for signed callback data (Telegram limit).
    /// </summary>
    public const int MaxTotalBytes = 64;

    /// <summary>
    /// Maximum bytes for original data (leaves room for separator + signature).
    /// </summary>
    public const int MaxDataBytes = 56;

    /// <summary>
    /// Length of HMAC signature in bytes (truncated to fit within limits).
    /// </summary>
    public const int SignatureBytes = 8;
}