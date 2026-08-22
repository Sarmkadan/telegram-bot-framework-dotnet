using System.Text.Json;
using System.Collections.Generic;
using TelegramBotFramework.Services;
using TelegramBotFramework.Models;

namespace TelegramBotFramework.Tests.Services
{
    public static class InlineQueryServiceTestsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public static string ToJson(this InlineQueryServiceTests value, bool indented = false)
        {
            return JsonSerializer.Serialize(value, _jsonSerializerOptions);
        }

        public static InlineQueryServiceTests? FromJson(string json)
        {
            return JsonSerializer.Deserialize<InlineQueryServiceTests?>(json, _jsonSerializerOptions);
        }

        public static bool TryFromJson(string json, out InlineQueryServiceTests? value)
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