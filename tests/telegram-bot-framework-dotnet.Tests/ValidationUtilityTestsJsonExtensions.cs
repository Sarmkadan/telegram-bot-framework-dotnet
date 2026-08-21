/********************************************************************************************************************************
*
* Copyright (c) Sarmkadan. All rights reserved.
* Licensed under the MIT License. See LICENSE in the project root.
*
********************************************************************************************************************************/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TelegramBotFramework.Tests
{
    /// <summary>
    /// Provides System.Text.Json serialization helpers for ValidationUtilityTests.
    /// </summary>
    public static class ValidationUtilityTestsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        /// <summary>
        /// Serializes the specified ValidationUtilityTests instance to a JSON string.
        /// </summary>
        /// <param name="value">The ValidationUtilityTests instance to serialize.</param>
        /// <param name="indented">When true, includes indentation for readability.</param>
        /// <returns>A JSON string representation of the instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this ValidationUtilityTests? value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);
            _jsonOptions.WriteIndented = indented;
            return JsonSerializer.Serialize(value, _jsonOptions);
        }

        /// <summary>
        /// Deserializes a JSON string into a ValidationUtilityTests instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>A ValidationUtilityTests instance or null if deserialization fails.</returns>
        public static ValidationUtilityTests? FromJson(string? json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            try
            {
                return JsonSerializer.Deserialize<ValidationUtilityTests>(json, _jsonOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into a ValidationUtilityTests instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized instance on success.</param>
        /// <returns>True if deserialization succeeded, false otherwise.</returns>
        public static bool TryFromJson(string? json, out ValidationUtilityTests? value)
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
