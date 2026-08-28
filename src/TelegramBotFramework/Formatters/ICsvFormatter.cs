namespace TelegramBotFramework.Formatters;

using System.Collections.Generic;
using TelegramBotFramework.Models;

/// <summary>
/// Defines methods for formatting data as CSV output.
/// </summary>
public interface ICsvFormatter
{
    string Format<T>(T data);
    string Format<T>(IEnumerable<T> data);
    string FormatError(string errorCode, string message, string? details = null);
    string FormatMessage(Message message);
    string FormatMessages(IEnumerable<Message> messages);
}