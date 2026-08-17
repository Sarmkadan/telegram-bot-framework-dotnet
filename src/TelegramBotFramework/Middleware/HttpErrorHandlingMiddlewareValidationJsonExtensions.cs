using System.Text.Json;
using System.Text.Json.Serialization;

namespace TelegramBotFramework.Middleware;

/// <summary>
/// Provides JSON extension methods for <see cref="HttpErrorResponse"/>.
/// </summary>
public static class HttpErrorHandlingMiddlewareValidationJsonExtensions
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes the <see cref="HttpErrorResponse"/> to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="HttpErrorResponse"/> instance.</param>
    /// <param name="indented">Whether to format the JSON string with indentation.</param>
    /// <returns>A JSON string representation of the <see cref="HttpErrorResponse"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static string ToJson(this HttpErrorResponse value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        
        var options = indented ? new JsonSerializerOptions(_options) { WriteIndented = true } : _options;
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="HttpErrorResponse"/>.
    /// </summary>
    /// <param name="json">The JSON string.</param>
    /// <returns>The deserialized <see cref="HttpErrorResponse"/>, or null if deserialization fails.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null or empty.</exception>
    public static HttpErrorResponse? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        
        return JsonSerializer.Deserialize<HttpErrorResponse>(json, _options);
    }

    /// <summary>
    /// Tries to deserialize a JSON string to a <see cref="HttpErrorResponse"/>.
    /// </summary>
    /// <param name="json">The JSON string.</param>
    /// <param name="value">The deserialized <see cref="HttpErrorResponse"/>, or null if deserialization fails.</param>
    /// <returns>True if deserialization is successful; otherwise, false.</returns>
    public static bool TryFromJson(string json, out HttpErrorResponse? value)
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
