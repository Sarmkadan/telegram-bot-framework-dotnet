#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Utilities;

/// <summary>
/// Utility class for working with enumerations.
/// Provides methods for parsing, converting, and describing enum values.
/// </summary>
public static class EnumHelper
{
    /// <summary>
    /// Gets all values of an enumeration type.
    /// </summary>
    public static IEnumerable<T> GetAllValues<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T)).Cast<T>();
    }

    /// <summary>
    /// Safely parses a string to an enum value with a default fallback.
    /// </summary>
    public static T TryParse<T>(string? value, T defaultValue) where T : Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;

        try
        {
            return (T)Enum.Parse(typeof(T), value, ignoreCase: true);
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Gets the description of an enum value from DescriptionAttribute if present.
    /// </summary>
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false)
            .FirstOrDefault() as System.ComponentModel.DescriptionAttribute;

        return attribute?.Description ?? value.ToString();
    }

    /// <summary>
    /// Converts an enum to a dictionary of name-value pairs.
    /// Useful for creating dropdown lists or lookup tables.
    /// </summary>
    public static Dictionary<string, T> EnumToDictionary<T>() where T : Enum
    {
        var dict = new Dictionary<string, T>();
        foreach (var value in GetAllValues<T>())
            dict[value.ToString()] = value;

        return dict;
    }

    /// <summary>
    /// Checks if an enum value has a specific flag (for flags enums).
    /// </summary>
    public static bool HasFlag<T>(this T value, T flag) where T : Enum
    {
        return value.HasFlag(flag);
    }

    /// <summary>
    /// Gets the numeric value of an enum member.
    /// </summary>
    public static object GetNumericValue(this Enum value)
    {
        return Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()));
    }

    /// <summary>
    /// Gets all attributes of a specific type on an enum value.
    /// </summary>
    public static IEnumerable<T> GetAttributes<T>(this Enum value) where T : Attribute
    {
        var field = value.GetType().GetField(value.ToString());
        return field?.GetCustomAttributes(typeof(T), false).Cast<T>() ?? Enumerable.Empty<T>();
    }

    /// <summary>
    /// Creates a dictionary of enum values with their descriptions for UI display.
    /// </summary>
    public static Dictionary<T, string> EnumToDisplayDictionary<T>() where T : Enum
    {
        var dict = new Dictionary<T, string>();
        foreach (var value in GetAllValues<T>())
            dict[value] = value.GetDescription();

        return dict;
    }

    /// <summary>
    /// Determines if a string value is a valid member of an enum type.
    /// </summary>
    public static bool IsValid<T>(string? value) where T : Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            Enum.Parse(typeof(T), value, ignoreCase: true);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the name of an enum value as it appears in the source code.
    /// </summary>
    public static string GetName<T>(T value) where T : Enum
    {
        return Enum.GetName(typeof(T), value) ?? string.Empty;
    }
}