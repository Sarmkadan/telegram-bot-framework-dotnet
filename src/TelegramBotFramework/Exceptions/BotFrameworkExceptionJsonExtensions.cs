#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using TelegramBotFramework.Utilities;

namespace TelegramBotFramework.Exceptions;

/// <summary>
/// Provides JSON serialization and deserialization extensions for BotFrameworkException.
/// </summary>
public static class BotFrameworkExceptionJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes the BotFrameworkException to a JSON string.
    /// </summary>
    /// <param name="value">The exception to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the exception.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this BotFrameworkException value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions)
            {
                WriteIndented = true
            }
            : _jsonSerializerOptions;

        // Add custom converter to redact tokens
        var optionsWithConverter = new JsonSerializerOptions(options)
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            Converters = { new BotFrameworkExceptionJsonConverter() }
        };

        return JsonSerializer.Serialize(value, optionsWithConverter);
    }

    /// <summary>
    /// Deserializes a BotFrameworkException from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized BotFrameworkException, or null if the JSON is null or empty.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is malformed and cannot be deserialized.</exception>
    public static BotFrameworkException? FromJson(string? json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<BotFrameworkException>(json, _jsonSerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a BotFrameworkException from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized BotFrameworkException, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out BotFrameworkException? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<BotFrameworkException>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
        catch
        {
            value = null;
            return false;
        }
    }
}

/// <summary>
/// Custom JSON converter for BotFrameworkException that redacts bot tokens from messages.
/// </summary>
internal sealed class BotFrameworkExceptionJsonConverter : JsonConverter<BotFrameworkException>
{
    public override BotFrameworkException Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<BotFrameworkException>(ref reader, options) ?? throw new JsonException("Failed to deserialize BotFrameworkException");
    }

    public override void Write(Utf8JsonWriter writer, BotFrameworkException value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        // Create a redacted version of the exception for serialization
        var redactedException = new RedactedBotFrameworkExceptionWrapper(value);

        var customOptions = new JsonSerializerOptions(options)
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            WriteIndented = options.WriteIndented
        };

        JsonSerializer.Serialize(writer, redactedException, customOptions);
    }

    /// <summary>
    /// Wrapper class that holds redacted exception data for serialization.
    /// </summary>
    private sealed class RedactedBotFrameworkExceptionWrapper
    {
        public string? Message { get; }
        public string? ErrorCode { get; }
        public string? StackTrace { get; }
        public RedactedExceptionWrapper? InnerException { get; }
        public string? Source { get; }
        public string? HelpLink { get; }

        public RedactedBotFrameworkExceptionWrapper(Exception exception)
        {
            Message = TokenRedaction.RedactTokenFromMessage(exception.Message);
            ErrorCode = exception is BotFrameworkException botEx ? botEx.ErrorCode : null;
            StackTrace = exception.StackTrace != null ? TokenRedaction.RedactTokenFromMessage(exception.StackTrace) : null;
            Source = TokenRedaction.RedactTokenFromMessage(exception.Source ?? string.Empty);
            HelpLink = exception.HelpLink != null ? TokenRedaction.RedactTokenFromMessage(exception.HelpLink) : null;

            if (exception.InnerException != null)
            {
                InnerException = new RedactedExceptionWrapper(exception.InnerException);
            }
        }
    }

    /// <summary>
    /// Wrapper class for any redacted exception (not just BotFrameworkException).
    /// </summary>
    private sealed class RedactedExceptionWrapper
    {
        public string? Message { get; }
        public string? ErrorCode { get; }
        public string? StackTrace { get; }
        public RedactedExceptionWrapper? InnerException { get; }
        public string? Source { get; }
        public string? HelpLink { get; }

        public RedactedExceptionWrapper(Exception exception)
        {
            Message = TokenRedaction.RedactTokenFromMessage(exception.Message);
            ErrorCode = exception is BotFrameworkException botEx ? botEx.ErrorCode : null;
            StackTrace = exception.StackTrace != null ? TokenRedaction.RedactTokenFromMessage(exception.StackTrace) : null;
            Source = TokenRedaction.RedactTokenFromMessage(exception.Source ?? string.Empty);
            HelpLink = exception.HelpLink != null ? TokenRedaction.RedactTokenFromMessage(exception.HelpLink) : null;

            if (exception.InnerException != null)
            {
                InnerException = new RedactedExceptionWrapper(exception.InnerException);
            }
        }
    }
}