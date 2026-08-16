using System;
using System.Text.Json;

namespace TelegramBotFramework.Integration
{
    /// <summary>
    /// Provides JSON serialization helpers for <see cref="MediaGroupItem"/>.
    /// </summary>
    public static class MediaGroupItemJsonExtensions
    {
        // Cached options with camel‑case naming; WriteIndented is set per call.
        private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Serializes the <see cref="MediaGroupItem"/> to a JSON string.
        /// </summary>
        /// <param name="value">The <see cref="MediaGroupItem"/> instance to serialize.</param>
        /// <param name="indented">If <c>true</c>, the output JSON will be formatted with indentation.</param>
        /// <returns>A JSON representation of <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public static string ToJson(this MediaGroupItem value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);
            var options = indented ? new JsonSerializerOptions(_options) { WriteIndented = true } : _options;
            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string into a <see cref="MediaGroupItem"/>.
        /// </summary>
        /// <param name="json">The JSON string representing a <see cref="MediaGroupItem"/>.</param>
        /// <returns>The deserialized <see cref="MediaGroupItem"/>, or <c>null</c> if the JSON is empty.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <c>null</c> or empty.</exception>
        /// <exception cref="JsonException">Thrown when the JSON cannot be deserialized into a <see cref="MediaGroupItem"/>.</exception>
        public static MediaGroupItem? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);
            return JsonSerializer.Deserialize<MediaGroupItem>(json, _options);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into a <see cref="MediaGroupItem"/>.
        /// </summary>
        /// <param name="json">The JSON string representing a <see cref="MediaGroupItem"/>.</param>
        /// <param name="value">
        /// When this method returns, contains the deserialized <see cref="MediaGroupItem"/> if the operation succeeded; otherwise, <c>null</c>.
        /// </param>
        /// <returns><c>true</c> if deserialization succeeded; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <c>null</c> or empty.</exception>
        public static bool TryFromJson(string json, out MediaGroupItem? value)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);
            try
            {
                value = JsonSerializer.Deserialize<MediaGroupItem>(json, _options);
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
