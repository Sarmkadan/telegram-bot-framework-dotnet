using System;
using System.Text.Json;

namespace TelegramBotFramework.ConversationFlow
{
    /// <summary>
    /// System.Text.Json helpers for <see cref="ConversationFlowOptions"/>.
    /// </summary>
    public static class ConversationFlowOptionsExtensionsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Converts the <see cref="ConversationFlowOptions"/> to a JSON string.
        /// </summary>
        /// <param name="value">The <see cref="ConversationFlowOptions"/> instance to convert.</param>
        /// <param name="indented">Whether to format the JSON with indentation.</param>
        /// <returns>A JSON string representing the <see cref="ConversationFlowOptions"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public static string ToJson(this ConversationFlowOptions value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);
            return JsonSerializer.Serialize(value, _jsonOptions);
        }

        /// <summary>
        /// Converts a JSON string to a <see cref="ConversationFlowOptions"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to convert.</param>
        /// <returns>A <see cref="ConversationFlowOptions"/> instance, or <c>null</c> if the input is <c>null</c> or empty.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty.</exception>
        /// <exception cref="JsonException">Thrown when the JSON is invalid.</exception>
        public static ConversationFlowOptions? FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<ConversationFlowOptions>(json, _jsonOptions);
        }

        /// <summary>
        /// Attempts to convert a JSON string to a <see cref="ConversationFlowOptions"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to convert.</param>
        /// <param name="value">When this method returns, contains the <see cref="ConversationFlowOptions"/> instance if the conversion succeeded, or <c>null</c> if it failed.</param>
        /// <returns><c>true</c> if the conversion succeeded; otherwise, <c>false</c>.</returns>
        public static bool TryFromJson(string json, out ConversationFlowOptions? value)
        {
            if (string.IsNullOrEmpty(json))
            {
                value = null;
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<ConversationFlowOptions>(json, _jsonOptions);
                return value is not null;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }
    }
}