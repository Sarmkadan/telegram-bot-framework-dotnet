#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="SessionServiceTests"/>.
/// </summary>
public static class SessionServiceTestsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = SessionServiceTestsJsonExtensionsConstants.BaseJsonSerializerOptions;

    /// <summary>
    /// Serializes the <see cref="SessionServiceTests"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this SessionServiceTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        return JsonSerializer.Serialize(value, indented
            ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
            : _jsonSerializerOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="SessionServiceTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="SessionServiceTests"/> instance, or <see langword="null"/> if the JSON is empty or whitespace.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or whitespace.</exception>
    /// <exception cref="JsonException">The JSON is invalid or cannot be deserialized.</exception>
    public static SessionServiceTests? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<SessionServiceTests>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="SessionServiceTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized instance if successful.</param>
    /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or whitespace.</exception>
    public static bool TryFromJson(string json, out SessionServiceTests? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<SessionServiceTests>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}