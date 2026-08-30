#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TelegramBotFramework.Utilities;

/// <summary>
/// Constants for DateTimeExtensionsJsonExtensions.
/// </summary>
internal static class DateTimeExtensionsJsonExtensionsConstants
{
    public static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };
}