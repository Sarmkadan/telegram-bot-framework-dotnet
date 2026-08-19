using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Text.Json.JsonSerializer;

namespace TelegramBotFramework.Tests
{
    public static class MessageFormatterTestsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Serializes the given <see cref="MessageFormatterTests"/> instance to JSON.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="indented">If true, includes indentation for readability.</param>
        /// <returns>JSON string representation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this MessageFormatterTests value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);
            var options = indented ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true } : _jsonOptions;
            return Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string into a <see cref="MessageFormatterTests"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to parse.</param>
        /// <returns>Deserialized object or null if input is empty.</returns>
        /// <exception cref="ArgumentException">Thrown for invalid JSON.</exception>
        public static MessageFormatterTests? FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;

            ArgumentException.ThrowIfNullOrEmpty(json);
            try
            {
                return Deserialize<MessageFormatterTests>(json, _jsonOptions);
            }
            catch (JsonException ex)
            {
                throw new ArgumentException("Invalid JSON format.", ex);
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into a <see cref="MessageFormatterTests"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to parse.</param>
        /// <param name="value">The deserialized object on success.</param>
        /// <returns>True if deserialization succeeded; false otherwise.</returns>
        public static bool TryFromJson(string json, out MessageFormatterTests? value)
        {
            try
            {
                value = FromJson(json);
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }
    }
}
