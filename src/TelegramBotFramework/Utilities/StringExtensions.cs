#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Utilities;

using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Extension methods for string manipulation and validation.
/// Provides common string operations like truncation, slug generation, and validation.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Truncates a string to a maximum length and appends ellipsis if truncated.
    /// </summary>
    public static string Truncate(this string value, int maxLength, string suffix = "…")
    {
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
    public static string ToSlug(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var slug = value.ToLowerInvariant();
        // Remove accents
        var bytes = Encoding.GetEncoding("Cyrillic").GetBytes(slug);
        slug = Encoding.ASCII.GetString(bytes);
        // Remove invalid characters
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        // Replace multiple spaces with single dash
        slug = Regex.Replace(slug, @"\s+", "-");
        // Remove multiple consecutive dashes
        slug = Regex.Replace(slug, @"-+", "-");
        // Trim dashes
        return slug.Trim('-');
    }

    /// <summary>
    /// Determines if a string is a valid email address.
    /// Uses simplified validation - for strict validation use System.ComponentModel.DataAnnotations
    /// </summary>
    public static bool IsValidEmail(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(value, pattern);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Determines if a string contains only alphanumeric characters.
    /// </summary>
    public static bool IsAlphanumeric(this string value)
    {
        return !string.IsNullOrEmpty(value) && value.All(char.IsLetterOrDigit);
    }

    /// <summary>
    /// Repeats a string a specified number of times.
    /// </summary>
    public static string Repeat(this string value, int count)
    {
        if (count <= 0 || string.IsNullOrEmpty(value))
            return string.Empty;

        var sb = new StringBuilder(value.Length * count);
        for (int i = 0; i < count; i++)
            sb.Append(value);

        return sb.ToString();
    }

    /// <summary>
    /// Reverses a string.
    /// </summary>
    public static string Reverse(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return new string(value.Reverse().ToArray());
    }

    /// <summary>
    /// Extracts numbers from a string.
    /// </summary>
    public static string ExtractNumbers(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return new string(value.Where(char.IsDigit).ToArray());
    }

    /// <summary>
    /// Ensures a string starts with a specified prefix.
    /// </summary>
    public static string EnsureStartsWith(this string value, string prefix)
    {
        if (string.IsNullOrEmpty(value))
            return prefix;

        return value.StartsWith(prefix) ? value : prefix + value;
    }

    /// <summary>
    /// Ensures a string ends with a specified suffix.
    /// </summary>
    public static string EnsureEndsWith(this string value, string suffix)
    {
        if (string.IsNullOrEmpty(value))
            return suffix;

        return value.EndsWith(suffix) ? value : value + suffix;
    }

    /// <summary>
    /// Capitalizes the first character of a string.
    /// </summary>
    public static string Capitalize(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return char.ToUpper(value[0]) + value[1..];
    }
}