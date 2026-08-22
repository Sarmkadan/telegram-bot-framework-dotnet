using System;
using System.Text.Json;

namespace HmacCallbackExample
{
    public static class HmacCallbackExampleJsonExtensions
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };

        public static string ToJson(this HmacCallbackExample value, bool indented = false)
        {
            return JsonSerializer.Serialize(value, jsonSerializerOptions);
        }

        public static HmacCallbackExample? FromJson(string json)
        {
            return JsonSerializer.Deserialize<HmacCallbackExample>(json, jsonSerializerOptions);
        }

        public static bool TryFromJson(string json, out HmacCallbackExample? value)
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