// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Formatters;

using System.Reflection;
using System.Text;
using TelegramBotFramework.Models;

/// <summary>
/// Formats data as CSV output for exports and data interchange.
/// Handles escaping, quoted fields, and supports generic collections.
/// </summary>
public class CsvFormatter : IOutputFormatter
{
    private const string FieldSeparator = ",";
    private const string LineEnding = "\r\n";
    private const char QuoteChar = '"';

    public string Format<T>(T data)
    {
        var items = new[] { data };
        return Format((IEnumerable<T>)items);
    }

    public string Format<T>(IEnumerable<T> data)
    {
        var list = data.ToList();
        if (list.Count == 0)
            return string.Empty;

        var type = typeof(T);
        var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToList();

        if (properties.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();

        // Write headers
        var headers = properties.Select(p => EscapeField(p.Name));
        sb.AppendLine(string.Join(FieldSeparator, headers));

        // Write data rows
        foreach (var item in list)
        {
            var values = properties.Select(p =>
            {
                var value = p.GetValue(item);
                var stringValue = value?.ToString() ?? string.Empty;
                return EscapeField(stringValue);
            });

            sb.AppendLine(string.Join(FieldSeparator, values));
        }

        return sb.ToString();
    }

    public string FormatError(string errorCode, string message, string? details = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine("ErrorCode,Message,Details,Timestamp");

        var detailsValue = details ?? string.Empty;
        var escapedErrorCode = EscapeField(errorCode);
        var escapedMessage = EscapeField(message);
        var escapedDetails = EscapeField(detailsValue);

        sb.AppendLine($"{escapedErrorCode},{escapedMessage},{escapedDetails},{EscapeField(DateTime.UtcNow.ToString("O"))}");

        return sb.ToString();
    }

    public string FormatMessage(Message message)
    {
        return Format(new[] { message });
    }

    public string FormatMessages(IEnumerable<Message> messages)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Id,Text,SenderId,ChatId,Timestamp,Type");

        foreach (var msg in messages)
        {
            var fields = new[]
            {
                EscapeField(msg.Id),
                EscapeField(msg.Text),
                EscapeField(msg.SenderId),
                EscapeField(msg.ChatId),
                EscapeField(msg.Timestamp.ToString("O")),
                EscapeField(msg.MessageType.ToString())
            };

            sb.AppendLine(string.Join(FieldSeparator, fields));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Escapes a field value for CSV format (quotes and escapes quotes).
    /// </summary>
    private static string EscapeField(string? field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;

        // If field contains special characters, wrap in quotes and escape inner quotes
        if (field.Contains(FieldSeparator) || field.Contains(LineEnding) || field.Contains(QuoteChar.ToString()))
        {
            return QuoteChar + field.Replace(QuoteChar.ToString(), QuoteChar.ToString() + QuoteChar) + QuoteChar;
        }

        return field;
    }
}
