#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;

namespace TelegramBotFramework.Services;

/// <summary>
/// System.Text.Json serialization helpers for <see cref="ScheduledMessageService"/>.
/// </summary>
public static class ScheduledMessageServiceJsonExtensions
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Serializes the scheduled message service to a JSON string.
    /// </summary>
    /// <param name="value">The scheduled message service instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON string representation of the service</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c></exception>
    public static string ToJson(this ScheduledMessageService value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(Options) { WriteIndented = true }
            : Options;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="ScheduledMessageService"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>The deserialized service, or <c>null</c> if <paramref name="json"/> is <c>null</c></returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is <c>null</c> or empty</exception>
    /// <exception cref="JsonException"><paramref name="json"/> is not valid JSON</exception>
    public static ScheduledMessageService? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<ScheduledMessageService>(json, Options);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="ScheduledMessageService"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">The deserialized service, or <c>null</c> if deserialization failed</param>
    /// <returns><c>true</c> if deserialization succeeded, <c>false</c> otherwise</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is <c>null</c> or empty</exception>
    public static bool TryFromJson(string json, out ScheduledMessageService? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<ScheduledMessageService>(json, Options);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}