using System.Text.Json;

namespace TelegramBotFramework.Services
{
    /// <summary>
    /// JSON serialization helpers for <see cref="SessionService"/>.
    /// </summary>
    public static class SessionServiceJsonExtensions
    {
        // Cached serializer options – camelCase property names.
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Serializes the <see cref="SessionService"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The instance to serialize.</param>
        /// <param name="indented">If <c>true</c>, the output will be formatted with indentation.</param>
        /// <returns>A JSON representation of the instance.</returns>
        public static string ToJson(this SessionService value, bool indented = false)
        {
            // Create a copy of the cached options with the desired indentation setting.
            var options = new JsonSerializerOptions(_options)
            {
                WriteIndented = indented
            };

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string into a <see cref="SessionService"/> instance.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <returns>The deserialized <see cref="SessionService"/>, or <c>null</c> if the JSON is empty.</returns>
        public static SessionService? FromJson(string json)
        {
            return JsonSerializer.Deserialize<SessionService>(json, _options);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into a <see cref="SessionService"/> instance.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <param name="value">When this method returns, contains the deserialized value if the operation succeeded; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if deserialization succeeded; otherwise, <c>false</c>.</returns>
        public static bool TryFromJson(string json, out SessionService? value)
        {
            try
            {
                value = JsonSerializer.Deserialize<SessionService>(json, _options);
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
