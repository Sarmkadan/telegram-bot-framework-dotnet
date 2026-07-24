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
    /// Serializes the BotFrameworkException to a JSON string using strict allow-list serialization.
    /// Only explicitly permitted properties are included to prevent accidental leakage of sensitive data
    /// such as bot tokens, connection strings, or other credentials that might be present in the
    /// Exception.Data dictionary or other properties.
    /// </summary>
    /// <param name="value">The exception to serialize. Must not be null.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the exception with sensitive data redacted.</returns>
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

        // Use custom serialization that filters sensitive data from Exception.Data
        var optionsWithConverter = new JsonSerializerOptions(options)
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            Converters = { new BotFrameworkExceptionSafeSerializationConverter() }
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

    /// <summary>
    /// Custom JSON converter that ensures safe serialization of BotFrameworkException.
    /// Filters out sensitive data from the Data dictionary and redacts tokens from all text properties.
    /// </summary>
    private sealed class BotFrameworkExceptionSafeSerializationConverter : JsonConverter<BotFrameworkException>
    {
        public override BotFrameworkException Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Use default deserialization for reading
            return JsonSerializer.Deserialize<BotFrameworkException>(ref reader, options) ?? throw new JsonException("Failed to deserialize BotFrameworkException");
        }

        public override void Write(Utf8JsonWriter writer, BotFrameworkException value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            // Create a safe JSON object manually to avoid recursion issues
            writer.WriteStartObject();

            // Message - always redact tokens
            var message = TokenRedaction.RedactTokenFromMessage(value.Message ?? string.Empty);
            if (message != null)
            {
                writer.WriteString("message", message);
            }

            // ErrorCode - always include for BotFrameworkException
            if (value is BotFrameworkException botEx && !string.IsNullOrEmpty(botEx.ErrorCode))
            {
                writer.WriteString("errorCode", botEx.ErrorCode);
            }

            // StackTrace - redact tokens
            if (!string.IsNullOrEmpty(value.StackTrace))
            {
                var redactedStackTrace = TokenRedaction.RedactTokenFromMessage(value.StackTrace);
                writer.WriteString("stackTrace", redactedStackTrace);
            }

            // Source - redact tokens
            if (!string.IsNullOrEmpty(value.Source))
            {
                var redactedSource = TokenRedaction.RedactTokenFromMessage(value.Source);
                writer.WriteString("source", redactedSource);
            }

            // HelpLink - redact tokens
            if (!string.IsNullOrEmpty(value.HelpLink))
            {
                var redactedHelpLink = TokenRedaction.RedactTokenFromMessage(value.HelpLink);
                writer.WriteString("helpLink", redactedHelpLink);
            }

            // Data dictionary - filter to safe properties only
            FilterSafeDataProperties(writer, value.Data);

            // InnerException - handle recursively
            if (value.InnerException != null)
            {
                writer.WritePropertyName("innerException");
                Write(writer, value.InnerException as BotFrameworkException ?? new BotFrameworkException(value.InnerException.Message, "INNER_EXCEPTION"), options);
            }

            writer.WriteEndObject();
        }

        /// <summary>
        /// Filters the Exception.Data dictionary to only include safe, non-sensitive properties.
        /// This prevents accidental leakage of credentials, connection strings, or other sensitive data.
        /// </summary>
        private static void FilterSafeDataProperties(Utf8JsonWriter writer, System.Collections.IDictionary data)
        {
            if (data.Count == 0)
            {
                return;
            }

            // Only include properties that are explicitly allow-listed
            // This is a strict allow-list to prevent any accidental leakage
            foreach (var keyObj in data.Keys)
            {
                if (keyObj is string key && IsSafeDataProperty(key))
                {
                    var value = data[key];
                    if (value == null || value is string || value is int || value is long || value is bool)
                    {
                        writer.WritePropertyName(key);
                        JsonSerializer.Serialize(writer, value, _jsonSerializerOptions);
                    }
                }
            }
        }

        /// <summary>
        /// Validates that a Data dictionary property name is safe to include.
        /// This is a strict allow-list to prevent credential leakage.
        /// </summary>
        private static bool IsSafeDataProperty(string propertyName)
        {
            // Only allow specific known-safe property names that are part of the framework's exception types
            return propertyName.Equals("commandName", StringComparison.OrdinalIgnoreCase) ||
                   propertyName.Equals("userId", StringComparison.OrdinalIgnoreCase) ||
                   propertyName.Equals("sessionId", StringComparison.OrdinalIgnoreCase) ||
                   propertyName.Equals("requiredPermission", StringComparison.OrdinalIgnoreCase) ||
                   propertyName.Equals("retryAfterSeconds", StringComparison.OrdinalIgnoreCase) ||
                   propertyName.Equals("updateId", StringComparison.OrdinalIgnoreCase);
        }
    }
}