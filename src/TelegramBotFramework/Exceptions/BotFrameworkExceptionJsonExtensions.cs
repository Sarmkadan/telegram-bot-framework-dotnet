#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TelegramBotFramework.Exceptions;

/// <summary>
/// Provides JSON serialization and deserialization extensions for BotFrameworkException.
/// </summary>
public static class BotFrameworkExceptionJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes the BotFrameworkException to a JSON string.
    /// </summary>
    /// <param name="value">The exception to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the exception.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this BotFrameworkException value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions)
            {
                WriteIndented = true
            }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a BotFrameworkException from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized BotFrameworkException, or null if the JSON is null or empty.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is malformed and cannot be deserialized.</exception>
    public static BotFrameworkException? FromJson(string? json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<BotFrameworkException>(json, _jsonSerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a BotFrameworkException from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized BotFrameworkException, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out BotFrameworkException? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<BotFrameworkException>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}