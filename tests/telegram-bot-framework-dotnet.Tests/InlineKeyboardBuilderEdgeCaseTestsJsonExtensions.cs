using System.Text.Json;
using TelegramBotFramework.Keyboard;
using Xunit;

namespace TelegramBotFramework.Tests
{
    public static class InlineKeyboardBuilderEdgeCaseTestsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };

        public static string ToJson(this InlineKeyboardBuilderEdgeCaseTests value, bool indented = false)
        {
            if (indented)
            {
                return JsonSerializer.Serialize(value, _jsonSerializerOptions);
            }
            else
            {
                return JsonSerializer.Serialize(value);
            }
        }

        public static InlineKeyboardBuilderEdgeCaseTests? FromJson(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<InlineKeyboardBuilderEdgeCaseTests>(json);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public static bool TryFromJson(string json, out InlineKeyboardBuilderEdgeCaseTests? value)
        {
            try
            {
                value = JsonSerializer.Deserialize<InlineKeyboardBuilderEdgeCaseTests>(json);
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