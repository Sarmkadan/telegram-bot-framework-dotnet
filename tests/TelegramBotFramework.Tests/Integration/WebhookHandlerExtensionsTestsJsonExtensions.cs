using System.Text.Json;
using System.Text.Json.Serialization;

namespace TelegramBotFramework.Tests.Integration;

/// <summary>
/// Contains JSON serialization helpers for the <see cref="WebhookHandlerExtensionsTests"/> class.
/// </summary>
public static class WebhookHandlerExtensionsTestsJsonExtensions
{
    /// <summary>
    /// Cached <see cref="JsonSerializerOptions"/> with camelCase property naming policy.
    /// </summary>
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Converts the current <see cref="WebhookHandlerExtensionsTests"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="WebhookHandlerExtensionsTests"/> instance to convert.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this WebhookHandlerExtensionsTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        var options = indented ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true } : _jsonOptions;
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Converts a JSON string to a <see cref="WebhookHandlerExtensionsTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="WebhookHandlerExtensionsTests"/> instance, or <see langword="null"/> if the JSON is empty or contains only whitespace.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/> or empty.</exception>
    public static WebhookHandlerExtensionsTests? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<WebhookHandlerExtensionsTests>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to convert a JSON string to a <see cref="WebhookHandlerExtensionsTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">When this method returns, contains the deserialized <see cref="WebhookHandlerExtensionsTests"/> instance if the conversion succeeded, or <see langword="null"/> if it failed. This parameter is treated as an output parameter.</param>
    /// <returns><see langword="true"/> if <paramref name="json"/> was successfully converted; otherwise, <see langword="false"/>.</returns>
    public static bool TryFromJson(string json, out WebhookHandlerExtensionsTests? value)
    {
        if (string.IsNullOrEmpty(json))
        {
            value = null;
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<WebhookHandlerExtensionsTests>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}