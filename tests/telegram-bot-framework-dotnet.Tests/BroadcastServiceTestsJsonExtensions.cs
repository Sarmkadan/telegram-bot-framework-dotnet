using System;
using System.Text.Json;
using TelegramBotFramework.Models;

namespace TelegramBotFramework.Tests
{
    public static class BroadcastServiceTestsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public static string ToJson(this BroadcastServiceTests value, bool indented = false)
        {
            return JsonSerializer.Serialize(value, _jsonSerializerOptions);
        }

        public static BroadcastServiceTests? FromJson(string json)
        {
            return JsonSerializer.Deserialize<BroadcastServiceTests>(json, _jsonSerializerOptions);
        }

        public static bool TryFromJson(string json, out BroadcastServiceTests? value)
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