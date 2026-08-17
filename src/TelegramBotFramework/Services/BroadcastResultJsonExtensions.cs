using System.Text.Json;

namespace TelegramBotFramework.Services;

/// <summary>
/// System.Text.Json helpers for serializing and deserializing <see cref="BroadcastResult"/>.
/// </summary>
public static class BroadcastResultJsonExtensions
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    /// <summary>
    /// Serializes this <see cref="BroadcastResult"/> to a JSON string.
    /// </summary>
    /// <param name="value">The broadcast result to serialize.</param>
    /// <param name="indented">Whether to pretty-print the JSON output.</param>
    /// <returns>The JSON representation of <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this BroadcastResult value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? jsonSerializerOptions
            : new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a <see cref="BroadcastResult"/> from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="BroadcastResult"/>, or <c>null</c> if <paramref name="json"/> is <c>null</c>.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is <c>null</c> or empty.</exception>
    /// <exception cref="JsonException"><paramref name="json"/> is not valid JSON or does not match <see cref="BroadcastResult"/>.</exception>
    public static BroadcastResult? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<BroadcastResult>(json, jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a <see cref="BroadcastResult"/> from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized <see cref="BroadcastResult"/> on success, or <c>null</c> on failure.</param>
    /// <returns><c>true</c> if deserialization succeeded; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is <c>null</c> or empty.</exception>
    public static bool TryFromJson(string json, out BroadcastResult? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = FromJson(json);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}