#nullable enable

using System;
using System.Text.Json;

namespace TelegramBotFramework.Tests.Integration;

/// <summary>
/// System.Text.Json serialization helpers for <see cref="PollingStrategyTests"/>.
/// </summary>
public static class PollingStrategyTestsJsonExtensions
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Serializes this <see cref="PollingStrategyTests"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The instance to serialize.</param>
    /// <param name="indented">When <c>true</c>, the output is pretty-printed with indentation.</param>
    /// <returns>The JSON representation of <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this PollingStrategyTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        return JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(Options) { WriteIndented = true } : Options);
    }

    /// <summary>
    /// Deserializes a <see cref="PollingStrategyTests"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized instance, or <c>null</c> when <paramref name="json"/> is <c>null</c>.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or whitespace.</exception>
    /// <exception cref="JsonException"><paramref name="json"/> is not valid JSON or does not match the target type.</exception>
    public static PollingStrategyTests? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        return JsonSerializer.Deserialize<PollingStrategyTests>(json, Options);
    }

    /// <summary>
    /// Attempts to deserialize a <see cref="PollingStrategyTests"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">When this method returns <c>true</c>, contains the deserialized instance; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> when deserialization succeeded; otherwise, <c>false</c>.</returns>
    public static bool TryFromJson(string json, out PollingStrategyTests? value)
    {
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