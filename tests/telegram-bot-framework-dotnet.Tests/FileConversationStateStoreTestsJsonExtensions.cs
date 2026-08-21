using System.Text.Json;
using System.Text.Json.Serialization;
using TelegramBotFramework.ConversationFlow;

namespace TelegramBotFramework.Tests
{
    /// <summary>
    /// System.Text.Json serialization helpers for <see cref="FileConversationStateStoreTests"/>.
    /// </summary>
    public static class FileConversationStateStoreTestsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
        };

        /// <summary>
        /// Serializes the value to a JSON string using camelCase naming.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <param name="indented">Whether to pretty-print the JSON output.</param>
        /// <returns>The JSON representation of <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        public static string ToJson(this FileConversationStateStoreTests value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
                : _jsonSerializerOptions;
            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string into a <see cref="FileConversationStateStoreTests"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized value, or <c>null</c> if <paramref name="json"/> is empty or whitespace.</returns>
        /// <exception cref="JsonException"><paramref name="json"/> is not valid JSON.</exception>
        public static FileConversationStateStoreTests? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            return JsonSerializer.Deserialize<FileConversationStateStoreTests>(json, _jsonSerializerOptions);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into a <see cref="FileConversationStateStoreTests"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized value, or <c>null</c> when deserialization fails.</param>
        /// <returns><c>true</c> if deserialization succeeded; otherwise, <c>false</c>.</returns>
        public static bool TryFromJson(string json, out FileConversationStateStoreTests? value)
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
}