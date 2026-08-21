using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;
using TelegramBotFramework.Strategies;
using ExecutionContext = TelegramBotFramework.Models.ExecutionContext;
using Xunit;

namespace TelegramBotFramework.Middleware.Tests
{
    /// <summary>
    /// JSON serialization helpers for <see cref="RateLimitingMiddlewareTests"/>.
    /// </summary>
    public static class RateLimitingMiddlewareTestsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Serializes the specified value to JSON.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation.</param>
        /// <returns>JSON string representation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
        public static string ToJson(this RateLimitingMiddlewareTests value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);
            _jsonOptions.WriteIndented = indented;
            return System.Text.Json.JsonSerializer.Serialize(value, value.GetType(), _jsonOptions);
        }

        /// <summary>
        /// Deserializes a JSON string into a <see cref="RateLimitingMiddlewareTests"/>.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>Deserialized object or null if json is null.</returns>
        /// <exception cref="ArgumentException">Thrown when json is empty.</exception>
        /// <exception cref="JsonException">Thrown when deserialization fails.</exception>
        public static RateLimitingMiddlewareTests? FromJson(string? json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);
            return System.Text.Json.JsonSerializer.Deserialize<RateLimitingMiddlewareTests>(json, _jsonOptions);
        }

        /// <summary>
        /// Tries to deserialize a JSON string into a <see cref="RateLimitingMiddlewareTests"/>.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized object if successful.</param>
        /// <returns>True if deserialization succeeded, false otherwise.</returns>
        public static bool TryFromJson(string? json, out RateLimitingMiddlewareTests? value)
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
