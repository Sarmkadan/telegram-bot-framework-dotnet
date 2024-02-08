#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace TelegramBotFramework.Utilities;

using System;
using System.Globalization;
using System.Text;

/// <summary>
/// Extension methods for string manipulation and validation.
/// Provides common string operations like truncation, slug generation, and validation.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Truncates a string to a maximum length and appends ellipsis if truncated.
    /// </summary>
    /// <param name="value">The string to truncate.</param>
    /// <param name="maxLength">The maximum length of the resulting string.</param>
    /// <param name="suffix">The suffix to append when truncating. Defaults to "…".</param>
    /// <returns>The truncated string, or the original string if it's shorter than maxLength.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="suffix"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxLength"/> is negative.</exception>
    public static string Truncate(this string value, int maxLength, string suffix = "…")
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);
        ArgumentNullException.ThrowIfNull(suffix);

        if (string.IsNullOrEmpty(value))
            return value;

        return value.Length <= maxLength
            ? value
            : value[..Math.Max(0, maxLength - suffix.Length)] + suffix;
    }

    /// <summary>
    /// Converts a string to a URL-friendly slug format.
    /// Example: "Hello World!" => "hello-world"
    /// </summary>
    /// <param name="value">The string to convert to a slug.</param>
    /// <returns>The URL-friendly slug.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static string ToSlug(this string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var slug = value.ToLowerInvariant();
        // Remove diacritics using Unicode normalization
        slug = RemoveDiacritics(slug);

        // Remove invalid characters (keep only letters, numbers, spaces, and dashes)
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");

        // Replace multiple spaces with single dash
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");

        // Remove multiple consecutive dashes
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");

        // Trim dashes
        return slug.Trim('-');
    }

    /// <summary>
    /// Removes diacritical marks from characters in the string.
    /// </summary>
    /// <param name="text">The text containing diacritics.</param>
    /// <returns>The text with diacritics removed.</returns>
    private static string RemoveDiacritics(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>
    /// Determines if a string is a valid email address.
    /// Uses simplified validation - for strict validation use System.ComponentModel.DataAnnotations
    /// </summary>
    /// <param name="value">The email address to validate.</param>
    /// <returns>True if the email appears valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static bool IsValidEmail(this string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return System.Text.RegularExpressions.Regex.IsMatch(value, pattern);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Determines if a string contains only alphanumeric characters.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>True if the string contains only alphanumeric characters; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static bool IsAlphanumeric(this string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value.All(char.IsLetterOrDigit);
    }

    /// <summary>
    /// Repeats a string a specified number of times.
    /// </summary>
    /// <param name="value">The string to repeat.</param>
    /// <param name="count">The number of times to repeat the string.</param>
    /// <returns>The repeated string, or empty string if count is zero or value is empty.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static string Repeat(this string value, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentNullException.ThrowIfNull(value);

        if (count == 0 || string.IsNullOrEmpty(value))
            return string.Empty;

        return string.Create(value.Length * count, value, (span, state) =>
        {
            state.AsSpan().CopyTo(span);
            for (var i = 1; i < count; i++)
            {
                state.AsSpan().CopyTo(span.Slice(i * state.Length));
            }
        });
    }

    /// <summary>
    /// Reverses a string.
    /// </summary>
    /// <param name="value">The string to reverse.</param>
    /// <returns>The reversed string, or the original string if it's null or empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static string Reverse(this string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.Length == 0)
            return value;

        var chars = value.ToCharArray();
        Array.Reverse(chars);
        return new string(chars);
    }

    /// <summary>
    /// Extracts numbers from a string.
    /// </summary>
    /// <param name="value">The string to extract numbers from.</param>
    /// <returns>A string containing only the digits from the input, or empty string if input is null or empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static string ExtractNumbers(this string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new string(value.Where(char.IsDigit).ToArray());
    }

    /// <summary>
    /// Ensures a string starts with a specified prefix.
    /// </summary>
    /// <param name="value">The string to check and potentially prefix.</param>
    /// <param name="prefix">The prefix to ensure is present.</param>
    /// <returns>The string with prefix prepended if it didn't start with it; otherwise, the original string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="prefix"/> is null.</exception>
    public static string EnsureStartsWith(this string? value, string prefix)
    {
        ArgumentNullException.ThrowIfNull(prefix);

        return value is null
            ? prefix
            : value.StartsWith(prefix)
                ? value
                : prefix + value;
    }

    /// <summary>
    /// Ensures a string ends with a specified suffix.
    /// </summary>
    /// <param name="value">The string to check and potentially suffix.</param>
    /// <param name="suffix">The suffix to ensure is present.</param>
    /// <returns>The string with suffix appended if it didn't end with it; otherwise, the original string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="suffix"/> is null.</exception>
    public static string EnsureEndsWith(this string? value, string suffix)
    {
        ArgumentNullException.ThrowIfNull(suffix);

        return value is null
            ? suffix
            : value.EndsWith(suffix)
                ? value
                : value + suffix;
    }

    /// <summary>
    /// Capitalizes the first character of a string.
    /// </summary>
    /// <param name="value">The string to capitalize.</param>
    /// <returns>The string with the first character capitalized, or the original string if it's null or empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static string Capitalize(this string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.Length == 0)
            return value;

        return char.ToUpperInvariant(value[0]) + value[1..];
    }
}