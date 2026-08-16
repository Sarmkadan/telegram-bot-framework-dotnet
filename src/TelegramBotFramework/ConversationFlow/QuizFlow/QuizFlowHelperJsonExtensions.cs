using System;
using System.Text.Json;

namespace TelegramBotFramework.ConversationFlow.QuizFlow
{
    /// <summary>
    /// Provides JSON serialization extensions for <see cref="QuizFlowHelper"/>.
    /// </summary>
    public static class QuizFlowHelperJsonExtensions
    {
        private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Serializes the <see cref="QuizFlowHelper"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The instance to serialize.</param>
        /// <param name="indented">If <c>true</c>, the output JSON will be indented.</param>
        /// <returns>A JSON representation of <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public static string ToJson(this QuizFlowHelper value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(_options) { WriteIndented = true }
                : _options;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string to a <see cref="QuizFlowHelper"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized <see cref="QuizFlowHelper"/> instance, or <c>null</c> if the JSON does not represent a valid object.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
        /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized to <see cref="QuizFlowHelper"/>.</exception>
        public static QuizFlowHelper? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);
            return JsonSerializer.Deserialize<QuizFlowHelper>(json, _options);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a <see cref="QuizFlowHelper"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">
        /// When this method returns, contains the deserialized <see cref="QuizFlowHelper"/> if the operation succeeded; otherwise, <c>null</c>.
        /// </param>
        /// <returns><c>true</c> if deserialization succeeded; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
        public static bool TryFromJson(string json, out QuizFlowHelper? value)
        {
            ArgumentNullException.ThrowIfNull(json);
            try
            {
                value = JsonSerializer.Deserialize<QuizFlowHelper>(json, _options);
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
