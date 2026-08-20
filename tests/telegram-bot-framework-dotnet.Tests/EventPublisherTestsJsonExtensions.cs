using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TelegramBotFramework.Tests
{
    public static class EventPublisherTestsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public static string ToJson(this EventPublisherTests value, bool indented = false)
        {
            return JsonSerializer.Serialize(value, _jsonSerializerOptions);
        }

        public static EventPublisherTests? FromJson(string json)
        {
            return JsonSerializer.Deserialize<EventPublisherTests>(json, _jsonSerializerOptions);
        }

        public static bool TryFromJson(string json, out EventPublisherTests? value)
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