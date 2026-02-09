#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Utilities;

using System.Text.RegularExpressions;

/// <summary>
/// Utility class for common validation patterns and checks.
/// Centralizes validation logic to ensure consistency across the application.
/// </summary>
public static class ValidationUtility
{
    /// <summary>
    /// Validates a Telegram user ID (must be positive integer).
    /// </summary>
    public static bool IsValidTelegramUserId(long userId)
    {
        return userId > 0;
    }

    /// <summary>
    /// Validates a Telegram chat ID (can be positive or negative).
    /// </summary>
    public static bool IsValidTelegramChatId(long chatId)
    {
        return chatId != 0;
    }

    /// <summary>
    /// Validates a Telegram token format (typically 10 digits:abc...).
    /// </summary>
    public static bool IsValidTelegramToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        return Regex.IsMatch(token, @"^\d+:[A-Za-z0-9_-]{27}$");
    }

    /// <summary>
    /// Validates a URL format using a simple regex pattern.
    /// </summary>
    public static bool IsValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        try
        {
            new Uri(url);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates IPv4 address format.
    /// </summary>
    public static bool IsValidIPv4(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return false;

        var pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
        return Regex.IsMatch(ipAddress, pattern);
    }

    /// <summary>
    /// Validates a mobile phone number (basic validation for common formats).
    /// </summary>
    public static bool IsValidPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        var cleanNumber = Regex.Replace(phoneNumber, @"\D", "");
        return cleanNumber.Length >= 10 && cleanNumber.Length <= 15;
    }

    /// <summary>
    /// Validates command name format (must start with / and contain only alphanumeric).
    /// </summary>
    public static bool IsValidCommandName(string? commandName)
    {
        if (string.IsNullOrWhiteSpace(commandName))
            return false;

        return Regex.IsMatch(commandName, @"^/[a-z0-9_]+$", RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Validates that a string contains only safe characters for use in filenames.
    /// </summary>
    public static bool IsValidFilename(string? filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            return false;

        var invalidChars = new[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };
        return !filename.Any(ch => invalidChars.Contains(ch));
    }

    /// <summary>
    /// Validates password strength (at least 8 chars, uppercase, lowercase, digit, special char).
    /// </summary>
    public static bool IsStrongPassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return false;

        return password.Any(char.IsUpper) &&
               password.Any(char.IsLower) &&
               password.Any(char.IsDigit) &&
               password.Any(ch => !char.IsLetterOrDigit(ch));
    }

    /// <summary>
    /// Validates a string matches a specific length range.
    /// </summary>
    public static bool IsValidLength(string? value, int minLength, int maxLength)
    {
        if (value  is null)
            return minLength == 0;

        return value.Length >= minLength && value.Length <= maxLength;
    }

    /// <summary>
    /// Validates a numeric string (can be negative or contain decimal point).
    /// </summary>
    public static bool IsNumeric(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && decimal.TryParse(value, out _);
    }

    /// <summary>
    /// Validates a GUID format.
    /// </summary>
    public static bool IsValidGuid(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && Guid.TryParse(value, out _);
    }
}