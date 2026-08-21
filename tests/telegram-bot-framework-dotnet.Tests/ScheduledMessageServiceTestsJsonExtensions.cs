#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Extension methods for JSON serialization of <see cref="ScheduledMessageServiceTests"/>.
/// </summary>
public static class ScheduledMessageServiceTestsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private static readonly JsonSerializerOptions _indentedJsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };

    /// <summary>
    /// Converts the <see cref="ScheduledMessageServiceTests"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The instance to convert.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representing the instance.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this ScheduledMessageServiceTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        return JsonSerializer.Serialize(value, indented ? _indentedJsonOptions : _jsonOptions);
    }

    /// <summary>
    /// Converts a JSON string to a <see cref="ScheduledMessageServiceTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to convert.</param>
    /// <returns>A <see cref="ScheduledMessageServiceTests"/> instance, or <see langword="null"/> if the input is invalid.</returns>
    /// <exception cref="ArgumentException">If <paramref name="json"/> is <see langword="null"/> or empty.</exception>
    public static ScheduledMessageServiceTests? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<ScheduledMessageServiceTests>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to convert a JSON string to a <see cref="ScheduledMessageServiceTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to convert.</param>
    /// <param name="value">When this method returns, contains the <see cref="ScheduledMessageServiceTests"/> instance if the conversion succeeded, or <see langword="null"/> if it failed.</param>
    /// <returns><see langword="true"/> if <paramref name="json"/> was successfully converted; otherwise, <see langword="false"/>.</returns>
    public static bool TryFromJson(string json, out ScheduledMessageServiceTests? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        try
        {
            value = JsonSerializer.Deserialize<ScheduledMessageServiceTests>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = default;
            return false;
        }
    }
}