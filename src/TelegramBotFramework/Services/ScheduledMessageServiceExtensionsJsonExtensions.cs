#nullable enable

using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace TelegramBotFramework.Services;

/// <summary>
/// Provides System.Text.Json serialization helpers for <see cref="ScheduledMessage"/>.
/// </summary>
public static class ScheduledMessageServiceExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Serializes a scheduled message to a JSON string using camelCase property names.
    /// </summary>
    /// <param name="value">The scheduled message to serialize.</param>
    /// <param name="indented">True to write indented JSON; otherwise, compact JSON.</param>
    /// <returns>The JSON representation of the scheduled message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException">The scheduled message contains a type that cannot be serialized.</exception>
    public static string ToJson(this ScheduledMessage value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = indented }))
        {
            JsonSerializer.Serialize(writer, value, _jsonOptions);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// Deserializes a scheduled message from a JSON string using camelCase property names.
    /// </summary>
    /// <param name="json">The JSON text to deserialize.</param>
    /// <returns>The deserialized scheduled message, or <see langword="null"/> if the JSON literal is <c>null</c>.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="JsonException">The JSON text is malformed or does not match <see cref="ScheduledMessage"/>.</exception>
    public static ScheduledMessage? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<ScheduledMessage>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a scheduled message from a JSON string without throwing on malformed input.
    /// </summary>
    /// <param name="json">The JSON text to deserialize.</param>
    /// <param name="value">
    /// When this method returns <see langword="true"/>, the deserialized scheduled message; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the JSON was parsed into a non-null scheduled message; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out ScheduledMessage? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<ScheduledMessage>(json, _jsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
