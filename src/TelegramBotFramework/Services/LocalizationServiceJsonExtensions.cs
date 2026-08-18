using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TelegramBotFramework.Services
{
    public static class LocalizationServiceJsonExtensions
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
;

        /// <summary>
        /// Serializes the given <see cref="LocalizationService"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The instance to serialize.</param>
        /// <param name="indented">If true, the JSON string will be formatted with indentation.</param>
        /// <returns>A JSON string representation of the instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this LocalizationService value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);
            _options.WriteIndented = indented;
            return JsonSerializer.Serialize(value, _options);
        }

        /// <summary>
        /// Deserializes a JSON string into a <see cref="LocalizationService"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>Deserialized instance or null if input is null.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
        /// <exception cref="JsonException">Thrown when JSON is malformed.</exception>
        public static LocalizationService? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);
            return JsonSerializer.Deserialize<LocalizationService>(json, _options);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into a <see cref="LocalizationService"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized instance if successful.</param>
        /// <returns>True if deserialization succeeded; false otherwise.</returns>
        public static bool TryFromJson(string json, out LocalizationService? value)
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
