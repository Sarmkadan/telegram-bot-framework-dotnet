using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TelegramBotFramework.Keyboard
{
    public static class ReplyKeyboardBuilderJsonExtensions
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Serializes the <see cref="ReplyKeyboardBuilder"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The <see cref="ReplyKeyboardBuilder"/> to serialize.</param>
        /// <param name="indented">If true, includes indentation for readability.</param>
        /// <returns>A JSON string representation of the builder.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this ReplyKeyboardBuilder value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);
            _options.WriteIndented = indented;
            return JsonSerializer.Serialize(value, _options);
        }

        /// <summary>
        /// Deserializes a JSON string into a <see cref="ReplyKeyboardBuilder"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>A deserialized <see cref="ReplyKeyboardBuilder"/> instance or null if input is invalid.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or null.</exception>
        public static ReplyKeyboardBuilder? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);
            try
            {
                return JsonSerializer.Deserialize<ReplyKeyboardBuilder>(json, _options);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into a <see cref="ReplyKeyboardBuilder"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized <see cref="ReplyKeyboardBuilder"/> instance if successful.</param>
        /// <returns>True if deserialization succeeded; false otherwise.</returns>
        public static bool TryFromJson(string json, out ReplyKeyboardBuilder? value)
        {
            try
            {
                value = FromJson(json);
                return value != null;
            }
            catch (ArgumentException)
            {
                value = null;
                return false;
            }
        }
    }
}
