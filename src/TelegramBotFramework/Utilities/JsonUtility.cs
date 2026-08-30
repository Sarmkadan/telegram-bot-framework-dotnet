#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Utilities;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Utility class for JSON serialization and deserialization operations.
/// Provides consistent JSON handling throughout the framework with custom settings.
/// </summary>
public static class JsonUtility
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    private static readonly JsonSerializerOptions PrettyOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    /// <summary>
    /// Serializes an object to JSON string.
    /// </summary>
    public static string Serialize<T>(T? obj, bool pretty = false)
    {
        ArgumentNullException.ThrowIfNull(obj);

        var options = pretty ? PrettyOptions : DefaultOptions;
        return JsonSerializer.Serialize(obj, options);
    }

    /// <summary>
    /// Deserializes a JSON string to an object.
    /// </summary>
    public static T? Deserialize<T>(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<T>(json, DefaultOptions);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    /// <summary>
    /// Attempts to deserialize JSON and returns success status.
    /// </summary>
    public static bool TryDeserialize<T>(string json, out T? result)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            result = JsonSerializer.Deserialize<T>(json, DefaultOptions);
            return result  is not null;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <summary>
    /// Validates if a string is valid JSON.
    /// </summary>
    public static bool IsValidJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            using var doc = JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Parses a JSON string into a JsonElement for flexible access.
    /// </summary>
    public static JsonElement? ParseJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.Clone();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Merges two JSON objects (second overrides first).
    /// </summary>
    public static string MergeJson(string json1, string json2)
    {
        ArgumentException.ThrowIfNullOrEmpty(json1);
        ArgumentException.ThrowIfNullOrEmpty(json2);

        var obj1 = JsonSerializer.Deserialize<Dictionary<string, object>>(json1, DefaultOptions) ?? new();
        var obj2 = JsonSerializer.Deserialize<Dictionary<string, object>>(json2, DefaultOptions) ?? new();

        foreach (var kvp in obj2)
            obj1[kvp.Key] = kvp.Value;

        return JsonSerializer.Serialize(obj1, DefaultOptions);
    }

    /// <summary>
    /// Gets a nested property value from a JSON string using dot notation.
    /// Example: GetPropertyValue(json, "user.profile.name")
    /// </summary>
    public static string? GetPropertyValue(string json, string propertyPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        ArgumentException.ThrowIfNullOrEmpty(propertyPath);

        try
        {
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement;

            foreach (var property in propertyPath.Split('.'))
            {
                if (element.TryGetProperty(property, out var nestedElement))
                    element = nestedElement;
                else
                    return null;
            }

            return element.GetRawText();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Converts a JSON string to pretty-printed format.
    /// </summary>
    public static string PrettyPrint(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            using var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc.RootElement, PrettyOptions);
        }
        catch
        {
            return json;
        }
    }

    /// <summary>
    /// Minifies a JSON string by removing unnecessary whitespace.
    /// </summary>
    public static string Minify(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            using var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc.RootElement, DefaultOptions);
        }
        catch
        {
            return json;
        }
    }
}
