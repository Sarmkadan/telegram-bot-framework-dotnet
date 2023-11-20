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
public class JsonFormatter : IOutputFormatter
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
        return JsonSerializer.Serialize(data, _options);
    }

    public string Format<T>(IEnumerable<T> data)
    {
        var wrapper = new { items = data.ToList(), count = data.Count() };
        return JsonSerializer.Serialize(wrapper, _options);
    }

    public string FormatError(string errorCode, string message, string? details = null)
    {
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
        var formatted = new
        {
            id = message.Id,
            text = message.Text,
            senderId = message.SenderId,
            chatId = message.ChatId,
            timestamp = message.Timestamp,
            editedTimestamp = message.EditedTimestamp,
            type = message.MessageType.ToString()
        };

        return JsonSerializer.Serialize(formatted, _options);
    }

    public string FormatMessages(IEnumerable<Message> messages)
    {
        var formattedMessages = messages.Select(m => new
        {
            id = m.Id,
            text = m.Text,
            senderId = m.SenderId,
            chatId = m.ChatId,
            timestamp = m.Timestamp,
            type = m.MessageType.ToString()
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
