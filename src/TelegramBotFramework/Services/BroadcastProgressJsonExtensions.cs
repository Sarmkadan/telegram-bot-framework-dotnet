using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TelegramBotFramework.Services
{
    public static class BroadcastProgressJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        /// <summary>
        /// Converts a <see cref="BroadcastProgress"/> object to a JSON string.
        /// </summary>
        /// <param name="value">The <see cref="BroadcastProgress"/> object to convert.</param>
        /// <param name="indented">Whether to format the JSON with indentation.</param>
        /// <returns>A JSON string representing the <see cref="BroadcastProgress"/> object.</returns>
        public static string ToJson(this BroadcastProgress value, bool indented = false)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return indented ? JsonSerializer.Serialize(value, _jsonSerializerOptions) : JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = false });
        }

        /// <summary>
        /// Deserializes a JSON string to a <see cref="BroadcastProgress"/> object.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>A <see cref="BroadcastProgress"/> object deserialized from the JSON string.</returns>
        public static BroadcastProgress? FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentException("JSON string is null, empty, or whitespace-only.", nameof(json));
            }

            try
            {
                return JsonSerializer.Deserialize<BroadcastProgress>(json, _jsonSerializerOptions);
            }
            catch (JsonException ex)
            {
                throw new ArgumentException("Invalid JSON string.", nameof(json), ex);
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a <see cref="BroadcastProgress"/> object.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized <see cref="BroadcastProgress"/> object, or null if deserialization fails.</param>
        /// <returns>True if deserialization succeeds; otherwise, false.</returns>
        public static bool TryFromJson(string json, out BroadcastProgress? value)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                value = null;
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<BroadcastProgress>(json, _jsonSerializerOptions);
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