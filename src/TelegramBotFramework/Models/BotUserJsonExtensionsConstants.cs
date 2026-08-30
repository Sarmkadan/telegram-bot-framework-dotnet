#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TelegramBotFramework.Models;

/// <summary>
/// Constants for BotUserJsonExtensions.
/// </summary>
internal static class BotUserJsonExtensionsConstants
{
    /// <summary>
    /// Default JSON serializer options for BotUser serialization.
    /// </summary>
    public static readonly JsonSerializerOptions BotUserJsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
}