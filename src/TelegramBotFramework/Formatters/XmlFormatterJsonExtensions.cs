#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace TelegramBotFramework.Formatters;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="XmlFormatter"/>.
/// </summary>
public static class XmlFormatterJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = CreateJsonSerializerOptions();

    private static JsonSerializerOptions CreateJsonSerializerOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.Strict
        };
        return options;
    }

    /// <summary>
    /// Serializes the <see cref="XmlFormatter"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The formatter to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the formatter's configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this XmlFormatter value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = new JsonSerializerOptions(_jsonOptions)
        {
            WriteIndented = indented
        };

        return JsonSerializer.Serialize(new XmlFormatterConfiguration(value), options);
    }

    /// <summary>
    /// Deserializes a JSON string to an <see cref="XmlFormatter"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="XmlFormatter"/> instance, or null if the JSON is invalid.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid.</exception>
    public static XmlFormatter? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            var config = JsonSerializer.Deserialize<XmlFormatterConfiguration>(json, _jsonOptions);
            return config is null ? null : new XmlFormatter(config.Pretty);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to an <see cref="XmlFormatter"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized formatter, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out XmlFormatter? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            var config = JsonSerializer.Deserialize<XmlFormatterConfiguration>(json, _jsonOptions);
            value = config is null ? null : new XmlFormatter(config.Pretty);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Represents the serializable configuration of an <see cref="XmlFormatter"/>.
    /// </summary>
    private sealed class XmlFormatterConfiguration
    {
        public bool Pretty { get; set; }

        [JsonConstructor]
        public XmlFormatterConfiguration(bool pretty)
        {
            Pretty = pretty;
        }

        public XmlFormatterConfiguration(XmlFormatter formatter)
        {
            Pretty = formatter.GetPretty();
        }
    }
}
