#nullable enable

using System.Text.Json;

namespace TelegramBotFramework.BackgroundWorkers;

/// <summary>
/// Constants for BackgroundTaskWorkerJsonExtensions.
/// </summary>
internal static class BackgroundTaskWorkerJsonExtensionsConstants
{
    /// <summary>
    /// The default JSON serializer options used for BackgroundTaskWorker serialization.
    /// </summary>
    public static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Indicates whether JSON should be written with indentation for readability.
    /// </summary>
    public const bool JsonWriteIndented = true;
}