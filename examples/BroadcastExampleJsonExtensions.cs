using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TelegramBotFramework.Examples
{
    public static class BroadcastExampleJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public static string ToJson(this BroadcastExample value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);
            return JsonSerializer.Serialize(value, _jsonSerializerOptions);
        }

        public static BroadcastExample? FromJson(string json)
        {
            return JsonSerializer.Deserialize<BroadcastExample>(json, _jsonSerializerOptions);
        }

        public static bool TryFromJson(string json, out BroadcastExample? value)
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