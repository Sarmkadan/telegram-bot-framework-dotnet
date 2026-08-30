using System.Text.Json;
using System.Text.Json.Serialization;

namespace TelegramBotFramework.Integration;

/// <summary>
/// Constants for HttpClientFactory json extensions.
/// </summary>
internal static class HttpClientFactoryJsonExtensionsConstants
{
    public const JsonSerializerDefaults SerializerDefaults = JsonSerializerDefaults.Web;
    public static readonly JsonNamingPolicy PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    public const bool DefaultWriteIndented = false;
    public const bool IndentedWriteIndented = true;
    public const JsonIgnoreCondition DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    public static readonly JsonNamingPolicy EnumNamingPolicy = JsonNamingPolicy.CamelCase;
}
