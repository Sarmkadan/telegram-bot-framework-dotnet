using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TelegramBotFramework.Services
{
    /// <summary>
    /// Provides JSON serialization helpers for the <see cref="CommandUsageTracker"/> type.
    /// </summary>
    public static class CommandUsageTrackerJsonExtensions
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        /// <summary>
        /// Serializes the given <see cref="CommandUsageTracker"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="indented">If true, the JSON string will be indented for readability.</param>
        /// <returns>A JSON string representing the object.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static string ToJson(this CommandUsageTracker value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);
            _options.WriteIndented = indented;
            return JsonSerializer.Serialize(value, _options);
        }

        /// <summary>
        /// Deserializes a JSON string into a <see cref="CommandUsageTracker"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized object or null if the input is empty.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is empty.</exception>
        /// <exception cref="JsonException">Thrown if the JSON is invalid.</exception>
        public static CommandUsageTracker? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);
            try
            {
                return JsonSerializer.Deserialize<CommandUsageTracker>(json, _options);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into a <see cref="CommandUsageTracker"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized object if successful.</param>
        /// <returns>True if deserialization succeeded; false otherwise.</returns>
        public static bool TryFromJson(string json, out CommandUsageTracker? value)
        {
            try
            {
                value = FromJson(json);
                return value != null;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }
    }
}
