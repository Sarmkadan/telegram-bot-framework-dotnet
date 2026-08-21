using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TelegramBotFramework.Tests
{
    public static class UserSessionValidationTestsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public static string ToJson(this UserSessionValidationTests value, bool indented = false)
        {
            return JsonSerializer.Serialize(value, _jsonSerializerOptions);
        }

        public static UserSessionValidationTests? FromJson(string json)
        {
            return JsonSerializer.Deserialize<UserSessionValidationTests>(json, _jsonSerializerOptions);
        }

        public static bool TryFromJson(string json, out UserSessionValidationTests? value)
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