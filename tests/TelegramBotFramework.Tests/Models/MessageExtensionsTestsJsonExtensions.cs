using System.Text.Json;

namespace TelegramBotFramework.Tests.Models
{
    public static class MessageExtensionsTestsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCase = JsonPropertyNameCase.CamelCase
        };

        public static string ToJson(this MessageExtensionsTests value, bool indented = false)
        {
            return JsonSerializer.Serialize(value, _jsonSerializerOptions);
        }

        public static MessageExtensionsTests? FromJson(string json)
        {
            return JsonSerializer.Deserialize<MessageExtensionsTests>(json, _jsonSerializerOptions);
        }

        public static bool TryFromJson(string json, out MessageExtensionsTests? value)
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