#nullable enable
using System.Text.Json;

namespace TelegramBotFramework.Strategies.Tests;

/// <summary>
/// JSON serialization helpers for <see cref="InMemoryRateLimitingStrategyTests"/>.
/// </summary>
public static class InMemoryRateLimitingStrategyTestsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Converts the <see cref="InMemoryRateLimitingStrategyTests"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The instance to convert.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representing the instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this InMemoryRateLimitingStrategyTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        var options = indented ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true } : _jsonOptions;
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Converts a JSON string to an <see cref="InMemoryRateLimitingStrategyTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to convert.</param>
    /// <returns>An instance of <see cref="InMemoryRateLimitingStrategyTests"/> or null if the input is null or empty.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static InMemoryRateLimitingStrategyTests? FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            throw new ArgumentException("Input JSON string is null or empty.", nameof(json));
        }

        return JsonSerializer.Deserialize<InMemoryRateLimitingStrategyTests>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to convert a JSON string to an <see cref="InMemoryRateLimitingStrategyTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to convert.</param>
    /// <param name="value">When this method returns, contains the deserialized value if the conversion succeeded, or null if it failed.</param>
    /// <returns>true if the conversion succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out InMemoryRateLimitingStrategyTests? value)
    {
        if (string.IsNullOrEmpty(json))
        {
            value = null;
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<InMemoryRateLimitingStrategyTests>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}