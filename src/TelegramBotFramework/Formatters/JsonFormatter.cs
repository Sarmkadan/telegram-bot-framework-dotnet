#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Formatters;

using System.Text.Json;
using System.Text.Json.Serialization;
using TelegramBotFramework.Models;

/// <summary>
/// Formats data as JSON output for API responses and exports.
/// Supports both single objects and collections with customizable serialization.
/// </summary>
public sealed class JsonFormatter : IOutputFormatter
{
    private readonly JsonSerializerOptions _options;

    public JsonFormatter(bool pretty = false)
    {
        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = pretty,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };
    }

    public string Format<T>(T data)
    {
        System.ArgumentNullException.ThrowIfNull(data);
        return JsonSerializer.Serialize(data, _options);
    }

    public string Format<T>(IEnumerable<T> data)
    {
        ArgumentNullException.ThrowIfNull(data);
        var wrapper = new { items = data.ToList(), count = data.Count() };
        return JsonSerializer.Serialize(wrapper, _options);
    }

    public string FormatError(string errorCode, string message, string? details = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(errorCode);
        ArgumentException.ThrowIfNullOrEmpty(message);
        var errorResponse = new
        {
            error = errorCode,
            message = message,
            details = details,
            timestamp = DateTime.UtcNow
        };

        return JsonSerializer.Serialize(errorResponse, _options);
    }

    public string FormatMessage(Message message)
    {
        ArgumentNullException.ThrowIfNull(message);
        var formatted = new
        {
            id = message.MessageId,
            content = message.Content,
            userId = message.UserId,
            chatId = message.ChatId,
            createdAt = message.CreatedAt,
            isEdited = message.IsEdited,
            type = message.Type.ToString()
        };

        return JsonSerializer.Serialize(formatted, _options);
    }

    public string FormatMessages(IEnumerable<Message> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);
        var formattedMessages = messages.Select(m => new
        {
            id = m.MessageId,
            content = m.Content,
            userId = m.UserId,
            chatId = m.ChatId,
            createdAt = m.CreatedAt,
            isEdited = m.IsEdited,
            type = m.Type.ToString()
        }).ToList();

        var wrapper = new { messages = formattedMessages, count = formattedMessages.Count };
        return JsonSerializer.Serialize(wrapper, _options);
    }
}

/// <summary>
/// Interface for output formatters (JSON, CSV, XML, etc).
/// </summary>
public interface IOutputFormatter
{
    string Format<T>(T data);
    string Format<T>(IEnumerable<T> data);
    string FormatError(string errorCode, string message, string? details = null);
    string FormatMessage(Message message);
    string FormatMessages(IEnumerable<Message> messages);
}