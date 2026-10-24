#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Buffers;
using System.Security.Cryptography;

namespace TelegramBotFramework.Integration;

/// <summary>
/// Generates and validates secret tokens for the <c>secret_token</c> parameter of Telegram's <c>setWebhook</c>.
/// Telegram echoes the token back in the <c>X-Telegram-Bot-Api-Secret-Token</c> header of every update.
/// </summary>
public static class WebhookSecretToken
{
    private static readonly SearchValues<char> AllowedCharacters =
        SearchValues.Create(WebhookSecretTokenConstants.Alphabet);

    /// <summary>Gets the maximum length of a secret token accepted by Telegram.</summary>
    public const int MaxLength = WebhookSecretTokenConstants.MaxLength;

    /// <summary>
    /// Generates a cryptographically random secret token from the characters Telegram accepts.
    /// </summary>
    /// <param name="length">The number of characters to generate, between 1 and <see cref="MaxLength"/>.</param>
    /// <returns>A new random secret token.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="length"/> is less than 1 or greater than <see cref="MaxLength"/>.</exception>
    public static string Generate(int length = WebhookSecretTokenConstants.DefaultLength)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(length, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, MaxLength);

        return RandomNumberGenerator.GetString(WebhookSecretTokenConstants.Alphabet, length);
    }

    /// <summary>
    /// Determines whether a value is a secret token Telegram accepts: 1 to <see cref="MaxLength"/>
    /// characters, all from <c>A-Z</c>, <c>a-z</c>, <c>0-9</c>, <c>_</c> and <c>-</c>.
    /// </summary>
    /// <param name="token">The candidate secret token.</param>
    /// <returns><see langword="true"/> if the token is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(string? token) =>
        !string.IsNullOrEmpty(token)
        && token.Length <= MaxLength
        && token.AsSpan().IndexOfAnyExcept(AllowedCharacters) < 0;
}
