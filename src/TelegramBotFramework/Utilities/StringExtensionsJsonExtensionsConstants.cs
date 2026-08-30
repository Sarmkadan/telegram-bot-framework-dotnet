#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TelegramBotFramework.Utilities;

/// <summary>
/// Constants for StringExtensionsJsonExtensions.
/// </summary>
internal static class StringExtensionsJsonExtensionsConstants
{
    public static readonly JsonNamingPolicy NamingPolicy = JsonNamingPolicy.CamelCase;
    public static readonly bool DefaultWriteIndented = false;
    public static readonly JsonIgnoreCondition DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    public static readonly ReferenceHandler DefaultReferenceHandler = ReferenceHandler.IgnoreCycles;
}