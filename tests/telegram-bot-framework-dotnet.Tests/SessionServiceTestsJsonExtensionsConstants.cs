#nullable enable

using System.Text.Json;

namespace TelegramBotFramework.Tests
{
    /// <summary>
    /// Provides shared values used by <see cref="SessionServiceTestsJsonExtensions"/>.
    /// </summary>
    internal static class SessionServiceTestsJsonExtensionsConstants
    {
        public static readonly JsonSerializerOptions BaseJsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }
}
