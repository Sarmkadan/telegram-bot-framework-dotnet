#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

namespace TelegramBotFramework.Utilities;

/// <summary>
/// Provides utilities for redacting bot tokens from strings to prevent credential leaks.
/// </summary>
public static class TokenRedaction
{
    private const string TokenPlaceholder = "[BOT_TOKEN_REDACTED]";

    /// <summary>
    /// Redacts bot token from a URL string.
    /// Replaces patterns like "bot123456:ABC-DEF1234ghIkl-zyx57W2v1u123ew11" with a placeholder.
    /// </summary>
    /// <param name="url">The URL string that may contain a bot token.</param>
    /// <returns>The redacted URL string, or the original if no token was found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="url"/> is null.</exception>
    public static string RedactTokenFromUrl(string url)
    {
        ArgumentNullException.ThrowIfNull(url);

        // Match Telegram bot token pattern in URLs: "bot" followed by token characters
        // Telegram bot tokens have the format: 123456:ABC-DEF1234ghIkl-zyx57W2v1u123ew11
        // Pattern: bot<digits>:<alphanumeric with dashes>
        var tokenPattern = @"bot\d+:[a-zA-Z0-9_-]+(?::[a-zA-Z0-9_-]+)*";

        return System.Text.RegularExpressions.Regex.Replace(
            url,
            tokenPattern,
            TokenPlaceholder,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
    }

    /// <summary>
    /// Redacts bot token from an exception message.
    /// </summary>
    /// <param name="message">The exception message that may contain a bot token.</param>
    /// <returns>The redacted message, or the original if no token was found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
    public static string RedactTokenFromMessage(string message)
    {
        ArgumentNullException.ThrowIfNull(message);

        // Match Telegram bot token pattern in messages
        var tokenPattern = @"bot\d+:[a-zA-Z0-9_-]+(?::[a-zA-Z0-9_-]+)*";

        return System.Text.RegularExpressions.Regex.Replace(
            message,
            tokenPattern,
            TokenPlaceholder,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
    }

    /// <summary>
    /// Redacts bot token from an exception object's message and any inner exceptions.
    /// </summary>
    /// <param name="exception">The exception to redact tokens from.</param>
    /// <returns>The redacted exception message.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static string RedactToken(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var message = exception.Message;
        var redactedMessage = RedactTokenFromMessage(message);

        // Also check inner exceptions
        if (exception.InnerException != null)
        {
            var innerMessage = RedactToken(exception.InnerException);
            if (!string.Equals(redactedMessage, message, StringComparison.Ordinal))
            {
                // If we redacted something in the outer exception, include inner details
                return $"{redactedMessage} (Inner exception: {innerMessage})";
            }
        }

        return redactedMessage;
    }
}