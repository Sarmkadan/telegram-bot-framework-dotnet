#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Formatters;

using System.Collections.Generic;
using TelegramBotFramework.Models;

/// <summary>
/// Abstraction over message formatting for display and logging.
/// Supports markdown, plain text, and HTML output formats.
/// </summary>
public interface IMessageFormatter
{
    /// <summary>
    /// Formats a message as plain text suitable for logging.
    /// </summary>
    string FormatAsPlainText(Message message);

    /// <summary>
    /// Formats a message as Telegram-compatible markdown.
    /// </summary>
    string FormatAsMarkdown(Message message);

    /// <summary>
    /// Formats a message as HTML.
    /// </summary>
    string FormatAsHtml(Message message);

    /// <summary>
    /// Formats multiple messages as a conversation thread.
    /// </summary>
    string FormatAsConversation(IEnumerable<Message> messages, bool markdown = true);

    /// <summary>
    /// Truncates a message text for display in previews.
    /// </summary>
    string TruncateForPreview(Message message, int maxLength = 100);

    /// <summary>
    /// Formats a message with metadata for debugging.
    /// </summary>
    string FormatForDebug(Message message);
}
