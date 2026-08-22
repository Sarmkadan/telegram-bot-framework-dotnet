using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Specialized;
using System.Runtime;

namespace TelegramBotFramework.Tests.Keyboard
{
    public static class ReplyKeyboardBuilderTestsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamePolicy = JsonNamingPolicy.CamelCase,
        };

        public static string ToJson(this ReplyKeyboardBuilderTests value, bool indented = false)
        {
            return JsonSerializer.Serialize(value, _jsonSerializerOptions);
        }

        public static ReplyKeyboardBuilderTests? FromJson(string json)
        {
            return JsonSerializer.Deserialize<ReplyKeyboardBuilderTests>(json, _jsonSerializerOptions);
        }

        public static bool TryFromJson(string json, out ReplyKeyboardBuilderTests? value)
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