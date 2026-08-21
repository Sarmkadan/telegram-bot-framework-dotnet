using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using TelegramBotFramework.Tests;

namespace TelegramBotFramework.Tests
{
    /// <summary>
    /// Provides System.Text.Json serialization helpers for EventBusExtensionsTests.
    /// </summary>
    public static class EventBusExtensionsTestsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        /// <summary>
        /// Serializes the given EventBusExtensionsTests instance to JSON.
        /// </summary>
        /// <param name="value">The object to serialize</param>
        /// <param name="indented">When true, includes indentation for readability</param>
        /// <returns>JSON string representation</returns>
        /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
        public static string ToJson(this EventBusExtensionsTests? value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);
            _jsonOptions.WriteIndented = indented;
            return JsonSerializer.Serialize(value, _jsonOptions);
        }

        /// <summary>
        /// Deserializes a JSON string into an EventBusExtensionsTests instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize</param>
        /// <returns>Deserialized object or null if deserialization fails</returns>
        /// <exception cref="ArgumentException">Thrown when input is empty</exception>
        public static EventBusExtensionsTests? FromJson(string? json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);
            try
            {
                return JsonSerializer.Deserialize<EventBusExtensionsTests>(json, _jsonOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into an EventBusExtensionsTests instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize</param>
        /// <param name="value">Output parameter containing the deserialized object</param>
        /// <returns>True if deserialization succeeded, false otherwise</returns>
        public static bool TryFromJson(string? json, out EventBusExtensionsTests? value)
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
