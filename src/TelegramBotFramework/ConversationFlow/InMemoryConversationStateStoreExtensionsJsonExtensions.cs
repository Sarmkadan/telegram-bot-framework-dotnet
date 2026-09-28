#nullable enable

using System.Text.Json;

namespace TelegramBotFramework.ConversationFlow;

/// <summary>
/// Provides System.Text.Json extension methods for <see cref="InMemoryConversationStateStore"/>.
/// </summary>
public static class InMemoryConversationStateStoreExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Serializes the current instance to a JSON string.
    /// </summary>
    /// <param name="value">The instance to serialize.</param>
    /// <param name="indented">Whether to produce indented JSON.</param>
    /// <returns>A JSON string representing the instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this InMemoryConversationStateStore value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into an <see cref="InMemoryConversationStateStore"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>An <see cref="InMemoryConversationStateStore"/> instance, or <c>null</c> if <paramref name="json"/> is <c>null</c> or empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <c>null</c>.</exception>
    /// <exception cref="JsonException">The JSON is invalid.</exception>
    public static InMemoryConversationStateStore? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (json.Length == 0)
        {
            return null;
        }

        return JsonSerializer.Deserialize<InMemoryConversationStateStore>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into an <see cref="InMemoryConversationStateStore"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">When this method returns, contains the deserialized value if the input JSON was valid; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if <paramref name="json"/> was valid JSON; otherwise, <c>false</c>.</returns>
    public static bool TryFromJson(string json, out InMemoryConversationStateStore? value)
    {
        if (json is null)
        {
            value = null;
            return false;
        }

        if (json.Length == 0)
        {
            value = null;
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<InMemoryConversationStateStore>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}