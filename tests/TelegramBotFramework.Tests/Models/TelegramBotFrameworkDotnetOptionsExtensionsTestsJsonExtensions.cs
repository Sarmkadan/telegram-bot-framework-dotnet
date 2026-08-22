using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TelegramBotFramework.Tests.Models
{
    public static class TelegramBotFrameworkDotnetOptionsExtensionsTestsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, WriteIndented = false };

        /// <summary>
        /// Serializes the specified value to a JSON string.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="indented">If true, the JSON string will be indented.</param>
        /// <returns>A JSON string representing the object.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this TelegramBotFrameworkDotnetOptionsExtensionsTests value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);
            _jsonOptions.WriteIndented = indented;
            return JsonSerializer.Serialize(value, _jsonOptions);
        }

        /// <summary>
        /// Deserializes a JSON string into an instance of <see cref="TelegramBotFrameworkDotnetOptionsExtensionsTests"/>.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>Deserialized object or null if input is empty.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty.</exception>
        /// <exception cref="JsonException">Thrown if JSON is invalid.</exception>
        public static TelegramBotFrameworkDotnetOptionsExtensionsTests? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);
            try
            {
                return JsonSerializer.Deserialize<TelegramBotFrameworkDotnetOptionsExtensionsTests>(json, _jsonOptions);
            }
            catch (JsonException ex)
            {
                throw new JsonException("Failed to deserialize JSON: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into an instance of <see cref="TelegramBotFrameworkDotnetOptionsExtensionsTests"/>.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized object if successful.</param>
        /// <returns>True if deserialization succeeded; false otherwise.</returns>
        public static bool TryFromJson(string json, out TelegramBotFrameworkDotnetOptionsExtensionsTests? value)
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
