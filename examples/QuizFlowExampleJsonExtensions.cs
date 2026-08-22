using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TelegramBotFramework.Examples
{
    /// <summary>
    /// Provides System.Text.Json serialization helpers for <see cref="QuizFlowExample"/>.
    /// </summary>
    public static class QuizFlowExampleJsonExtensions
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
            .WithIndent(true);

        /// <summary>
        /// Serializes the specified <see cref="QuizFlowExample"/> to a JSON string.
        /// </summary>
        /// <param name="value">The instance to serialize. Throws if null.</param>
        /// <param name="indented">If true, uses indented formatting. Defaults to false.</param>
        /// <returns>JSON string representation.</returns>
        /// <exception cref="ArgumentNullException">value is null.</exception>
        public static string ToJson(this QuizFlowExample value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);
            var options = indented ? _options : new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string into a <see cref="QuizFlowExample"/>.
        /// </summary>
        /// <param name="json">JSON string to deserialize. Throws if null or empty.</param>
        /// <returns>Deserialized instance.</returns>
        /// <exception cref="ArgumentException">json is empty.</exception>
        /// <exception cref="JsonException">Deserialization fails.</exception>
        public static QuizFlowExample FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);
            return JsonSerializer.Deserialize<QuizFlowExample>(json, _options) ?? throw new JsonException("Deserialized to null");
        }

        /// <summary>
        /// Tries to deserialize a JSON string into a <see cref="QuizFlowExample"/>.
        /// </summary>
        /// <param name="json">JSON string to deserialize. Can be null.</param>
        /// <param name="value">Output parameter set to deserialized instance on success.</param>
        /// <returns>true if deserialization succeeded; false otherwise.</returns>
        public static bool TryFromJson(string json, out QuizFlowExample? value)
        {
            value = null;
            if (string.IsNullOrWhiteSpace(json))
                return false;

            try
            {
                value = JsonSerializer.Deserialize<QuizFlowExample>(json, _options);
                return value != null;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
