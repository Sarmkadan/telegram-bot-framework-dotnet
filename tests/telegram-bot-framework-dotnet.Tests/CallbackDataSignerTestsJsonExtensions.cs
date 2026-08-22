/************************************************************************************************
* Copyright (c) 2026 TelegramBotFramework. All rights reserved.
* Licensed under the MIT License. See LICENSE in the project root.
************************************************************************************************/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TelegramBotFramework.Tests
{
    /// <summary>
    /// JSON serialization helpers for <see cref="CallbackDataSignerTests"/>.
    /// </summary>
    public static class CallbackDataSignerTestsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Serializes the given <see cref="CallbackDataSignerTests"/> instance to JSON.
        /// </summary>
        /// <param name="value">The instance to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation.</param>
        /// <returns>JSON string representation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this CallbackDataSignerTests value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            _jsonOptions.WriteIndented = indented;
            return JsonSerializer.Serialize(value, _jsonOptions);
        }

        /// <summary>
        /// Deserializes a JSON string into a <see cref="CallbackDataSignerTests"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>Deserialized instance or null if deserialization fails.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        /// <exception cref="JsonException">Thrown when JSON is invalid.</exception>
        public static CallbackDataSignerTests? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);

            try
            {
                return JsonSerializer.Deserialize<CallbackDataSignerTests>(json, _jsonOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Tries to deserialize a JSON string into a <see cref="CallbackDataSignerTests"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized instance if successful.</param>
        /// <returns>True if deserialization succeeded, false otherwise.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty.</exception>
        public static bool TryFromJson(string json, out CallbackDataSignerTests? value)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            value = null;
            try
            {
                value = JsonSerializer.Deserialize<CallbackDataSignerTests>(json, _jsonOptions);
                return value != null;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
