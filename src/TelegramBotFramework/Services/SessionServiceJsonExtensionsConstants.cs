using System.Text.Json;

namespace TelegramBotFramework.Services;

/// <summary>
/// Contains constants for SessionServiceJsonExtensions.
/// </summary>
internal static class SessionServiceJsonExtensionsConstants
{
    public static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}