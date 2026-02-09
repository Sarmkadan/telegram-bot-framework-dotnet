#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Formatters;

using System.Text;
using TelegramBotFramework.Models;

/// <summary>
/// Formats messages for display and logging with support for different output formats.
/// Handles markdown, plain text, and HTML formatting.
/// </summary>
public sealed class MessageFormatter
{
    /// <summary>
    /// Formats a message as plain text suitable for logging.
    /// </summary>
    public static string FormatAsPlainText(Message message)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"[{message.Timestamp:yyyy-MM-dd HH:mm:ss}] {message.SenderId}:");
        sb.AppendLine(message.Text);

        if (message.EditedTimestamp.HasValue)
            sb.AppendLine($"(Edited: {message.EditedTimestamp:yyyy-MM-dd HH:mm:ss})");

        return sb.ToString();
    }

    /// <summary>
    /// Formats a message as Telegram-compatible markdown.
    /// </summary>
    public static string FormatAsMarkdown(Message message)
    {
        var sb = new StringBuilder();
        sb.Append($"**[{message.Timestamp:HH:mm}]** ");
        sb.Append($"_{EscapeMarkdown(message.SenderId)}_: ");
        sb.Append(EscapeMarkdown(message.Text));

        if (message.EditedTimestamp.HasValue)
            sb.Append($" _(edited)_");

        return sb.ToString();
    }

    /// <summary>
    /// Formats a message as HTML.
    /// </summary>
    public static string FormatAsHtml(Message message)
    {
        var sb = new StringBuilder();
        sb.Append("<div class='message'>");
        sb.Append($"<span class='timestamp'>[{message.Timestamp:HH:mm}]</span> ");
        sb.Append($"<strong>{EscapeHtml(message.SenderId)}</strong>: ");
        sb.Append($"<span class='text'>{EscapeHtml(message.Text)}</span>");

        if (message.EditedTimestamp.HasValue)
            sb.Append("<span class='edited'>(edited)</span>");

        sb.Append("</div>");
        return sb.ToString();
    }

    /// <summary>
    /// Formats multiple messages as a conversation thread.
    /// </summary>
    public static string FormatAsConversation(IEnumerable<Message> messages, bool markdown = true)
    {
        var sb = new StringBuilder();

        foreach (var message in messages)
        {
            var formatted = markdown ? FormatAsMarkdown(message) : FormatAsPlainText(message);
            sb.AppendLine(formatted);
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Truncates a message text for display in previews.
    /// </summary>
    public static string TruncateForPreview(Message message, int maxLength = 100)
    {
        var text = message.Text;

        // Remove newlines for preview
        text = text.Replace("\r\n", " ").Replace("\n", " ");

        if (text.Length > maxLength)
            text = text[..maxLength] + "…";

        return text;
    }

    /// <summary>
    /// Formats a message with metadata for debugging.
    /// </summary>
    public static string FormatForDebug(Message message)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== Message Debug Info ===");
        sb.AppendLine($"ID: {message.Id}");
        sb.AppendLine($"Type: {message.MessageType}");
        sb.AppendLine($"Sender ID: {message.SenderId}");
        sb.AppendLine($"Chat ID: {message.ChatId}");
        sb.AppendLine($"Timestamp: {message.Timestamp:O}");
        sb.AppendLine($"Edited: {(message.EditedTimestamp?.ToString("O") ?? "No")}");
        sb.AppendLine($"Length: {message.Text?.Length ?? 0} chars");
        sb.AppendLine($"Content: {message.Text}");
        return sb.ToString();
    }

    /// <summary>
    /// Escapes special characters for Markdown format.
    /// </summary>
    private static string EscapeMarkdown(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // Escape markdown special characters
        var specialChars = new[] { '_', '*', '[', ']', '(', ')', '~', '`', '\\' };
        var result = text;

        foreach (var ch in specialChars)
        {
            result = result.Replace(ch.ToString(), $"\\{ch}");
        }

        return result;
    }

    /// <summary>
    /// Escapes special characters for HTML format.
    /// </summary>
    private static string EscapeHtml(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }
}