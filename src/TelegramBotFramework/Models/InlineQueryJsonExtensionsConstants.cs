#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TelegramBotFramework.Models;

/// <summary>
/// Constants for InlineQueryJsonExtensions.
/// </summary>
internal static class InlineQueryJsonExtensionsConstants
{
    /// <summary>
    /// Default JSON serializer options for InlineQuery serialization.
    /// </summary>
    public static readonly JsonSerializerOptions InlineQueryJsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
}