using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using TelegramBotFramework.Exceptions;

namespace TelegramBotFramework.Tests
{
    public static class BotFrameworkExceptionJsonExtensionsTestsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };

        public static string ToJson(this BotFrameworkExceptionJsonExtensionsTests value, bool indented = false)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var jsonSerializerOptions = indented ? _jsonSerializerOptions : new JsonSerializerOptions();
            return JsonSerializer.Serialize(value, jsonSerializerOptions);
        }

        public static BotFrameworkExceptionJsonExtensionsTests? FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<BotFrameworkExceptionJsonExtensionsTests>(json, _jsonSerializerOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public static bool TryFromJson(string json, out BotFrameworkExceptionJsonExtensionsTests? value)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                value = null;
                return false;
            }

            try
            {
                value = JsonSerializer.Deserialize<BotFrameworkExceptionJsonExtensionsTests>(json, _jsonSerializerOptions);
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
