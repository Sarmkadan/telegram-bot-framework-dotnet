#nullable enable

using System.Text.Json;

namespace TelegramBotFramework.Keyboard;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="ReplyKeyboardButton"/>.
/// </summary>
public static class ReplyKeyboardButtonJsonExtensions
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Serializes the <see cref="ReplyKeyboardButton"/> to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="ReplyKeyboardButton"/> to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representing the <see cref="ReplyKeyboardButton"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this ReplyKeyboardButton value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        return JsonSerializer.Serialize(value, _options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="ReplyKeyboardButton"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="ReplyKeyboardButton"/> instance, or <see langword="null"/> if the input is <see langword="null"/> or empty.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid for <see cref="ReplyKeyboardButton"/>.</exception>
    public static ReplyKeyboardButton? FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            ArgumentException.ThrowIfNullOrEmpty(json);
            return null;
        }

        return JsonSerializer.Deserialize<ReplyKeyboardButton>(json, _options);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="ReplyKeyboardButton"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">When this method returns, contains the deserialized <see cref="ReplyKeyboardButton"/> if the deserialization succeeded, or <see langword="null"/> if it failed.</param>
    /// <returns><see langword="true"/> if the deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    public static bool TryFromJson(string json, out ReplyKeyboardButton? value)
    {
        if (string.IsNullOrEmpty(json))
        {
            value = null;
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<ReplyKeyboardButton>(json, _options);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}