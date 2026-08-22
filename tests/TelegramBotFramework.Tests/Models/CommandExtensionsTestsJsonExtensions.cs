using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Linq.Expressions;

namespace TelegramBotFramework.Tests.Models
{
    /// <summary>
    /// Provides System.Text.Json serialization helpers for <see cref="CommandExtensionsTests"/>.
    /// </summary>
    public static class CommandExtensionsTestsJsonExtensions
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Serializes the specified <see cref="CommandExtensionsTests"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="indented">If true, includes indentation for readability.</param>
        /// <returns>A JSON string representation of the object.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static string ToJson(this CommandExtensionsTests value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);
            var json = JsonSerializer.Serialize(value, _options);
            return indented ? JsonDocument.Parse(json).ToString() : json;
        }

        /// <summary>
        /// Deserializes a JSON string into a <see cref="CommandExtensionsTests"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>Deserialized object or null if input is empty.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is empty.</exception>
        /// <exception cref="JsonException">Thrown if JSON is invalid.</exception>
        public static CommandExtensionsTests? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);
            try
            {
                return JsonSerializer.Deserialize<CommandExtensionsTests>(json, _options);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Tries to deserialize a JSON string into a <see cref="CommandExtensionsTests"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized object on success.</param>
        /// <returns>True if deserialization succeeded; false otherwise.</returns>
        public static bool TryFromJson(string json, out CommandExtensionsTests? value)
        {
            try
            {
                value = FromJson(json);
                return value != null;
            }
            catch
            {
                value = null;
                return false;
            }
        }
    }
}
