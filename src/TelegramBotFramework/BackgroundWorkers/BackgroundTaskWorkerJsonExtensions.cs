#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;

namespace TelegramBotFramework.BackgroundWorkers;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for <see cref="BackgroundTaskWorker"/>.
/// </summary>
public static class BackgroundTaskWorkerJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes the <see cref="BackgroundTaskWorker"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The background task worker to serialize. Cannot be <see langword="null"/>.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the background task worker.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this BackgroundTaskWorker value, bool indented = false)
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
    /// Deserializes a JSON string to a <see cref="BackgroundTaskWorker"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. If <see langword="null"/>, empty, or whitespace, returns <see langword="null"/>.</param>
    /// <returns>A deserialized <see cref="BackgroundTaskWorker"/> instance, or <see langword="null"/> if the JSON is empty or whitespace.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static BackgroundTaskWorker? FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<BackgroundTaskWorker>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="BackgroundTaskWorker"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Cannot be <see langword="null"/>, empty, or whitespace.</param>
    /// <param name="value">Receives the deserialized <see cref="BackgroundTaskWorker"/> instance if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out BackgroundTaskWorker? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<BackgroundTaskWorker>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}