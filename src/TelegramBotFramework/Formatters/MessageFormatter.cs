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
public sealed class MessageFormatter : IMessageFormatter
{
    /// <summary>
    /// Formats a message as plain text suitable for logging.
    /// </summary>
    public static string FormatAsPlainText(Message message)
    {
        ArgumentNullException.ThrowIfNull(message);
        var sb = new StringBuilder();
        sb.AppendLine($"[{message.CreatedAt:yyyy-MM-dd HH:mm:ss}] {message.UserId}:");
        sb.AppendLine(message.Content);

        if (message.IsEdited)
            sb.AppendLine("(Edited)");

        return sb.ToString();
    }

    /// <summary>
    /// Formats a message as Telegram-compatible markdown.
    /// </summary>
    public static string FormatAsMarkdown(Message message)
    {
        ArgumentNullException.ThrowIfNull(message);
        var sb = new StringBuilder();
        sb.Append($"**[{message.CreatedAt:HH:mm}]** ");
        sb.Append($"_{EscapeMarkdown(message.UserId.ToString())}_: ");
        sb.Append(EscapeMarkdown(message.Content));

        if (message.IsEdited)
            sb.Append(" _(edited)_");

        return sb.ToString();
    }

    /// <summary>
    /// Formats a message as HTML.
    /// </summary>
    public static string FormatAsHtml(Message message)
    {
        ArgumentNullException.ThrowIfNull(message);
        var sb = new StringBuilder();
        sb.Append("<div class='message'>");
        sb.Append($"<span class='timestamp'>[{message.CreatedAt:HH:mm}]</span> ");
        sb.Append($"<strong>{EscapeHtml(message.UserId.ToString())}</strong>: ");
        sb.Append($"<span class='text'>{EscapeHtml(message.Content)}</span>");

        if (message.IsEdited)
            sb.Append("<span class='edited'>(edited)</span>");

        sb.Append("</div>");
        return sb.ToString();
    }

    /// <summary>
    /// Formats multiple messages as a conversation thread.
    /// </summary>
    public static string FormatAsConversation(IEnumerable<Message> messages, bool markdown = true)
    {
        ArgumentNullException.ThrowIfNull(messages);
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
        ArgumentNullException.ThrowIfNull(message);
        var text = message.Content.Replace("\r\n", " ").Replace("\n", " ");

        if (text.Length > maxLength)
            text = text[..maxLength] + "…";

        return text;
    }

    /// <summary>
    /// Formats a message with metadata for debugging.
    /// </summary>
    public static string FormatForDebug(Message message)
    {
        ArgumentNullException.ThrowIfNull(message);
        var sb = new StringBuilder();
        sb.AppendLine("=== Message Debug Info ===");
        sb.AppendLine($"ID: {message.MessageId}");
        sb.AppendLine($"Type: {message.Type}");
        sb.AppendLine($"Sender ID: {message.UserId}");
        sb.AppendLine($"Chat ID: {message.ChatId}");
        sb.AppendLine($"Timestamp: {message.CreatedAt:O}");
        sb.AppendLine($"Edited: {(message.IsEdited ? "Yes" : "No")}");
        sb.AppendLine($"Length: {message.Content.Length} chars");
        sb.AppendLine($"Content: {message.Content}");
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

    // Explicit interface implementations delegate to the static helpers,
    // preserving the existing static API while enabling DI-based consumption.
    string IMessageFormatter.FormatAsPlainText(Message message) => FormatAsPlainText(message);

    string IMessageFormatter.FormatAsMarkdown(Message message) => FormatAsMarkdown(message);

    string IMessageFormatter.FormatAsHtml(Message message) => FormatAsHtml(message);

    string IMessageFormatter.FormatAsConversation(IEnumerable<Message> messages, bool markdown)
        => FormatAsConversation(messages, markdown);

    string IMessageFormatter.TruncateForPreview(Message message, int maxLength)
        => TruncateForPreview(message, maxLength);

    string IMessageFormatter.FormatForDebug(Message message) => FormatForDebug(message);
}
