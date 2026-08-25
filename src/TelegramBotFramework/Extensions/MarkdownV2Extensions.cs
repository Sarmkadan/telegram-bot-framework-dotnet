using System;
using System.Text.RegularExpressions;

namespace TelegramBotFramework.Extensions
{
    /// <summary>
    /// Extension methods for formatting text using Telegram MarkdownV2.
    /// See https://core.telegram.org/bots/api#formatting-options for more details.
    /// </summary>
    public static class MarkdownV2Extensions
    {
        /// <summary>
        /// Escapes all reserved characters in the input string for use in Telegram MarkdownV2.
        /// Reserved characters: _ * [ ] ( ) ~ ` > # + - = | { } . !
        /// </summary>
        /// <param name="text">The input string to escape.</param>
        /// <returns>The escaped string safe for use in Telegram MarkdownV2.</returns>
        public static string EscapeMarkdownV2(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // List of reserved characters that must be escaped with a backslash in MarkdownV2
            var reservedChars = new[] { '_', '*', '[', ']', '(', ')', '~', '`', '>', '#', '+', '-', '=', '|', '{', '}', '.', '!' };

            foreach (var c in reservedChars)
            {
                // Replace each reserved character with a backslash followed by the character
                text = text.Replace(c.ToString(), $@"\{c}");
            }

            return text;
        }

        /// <summary>
        /// Wraps the input string in bold formatting using Telegram MarkdownV2.
        /// The content is escaped to prevent interference with formatting.
        /// </summary>
        /// <param name="text">The input string to format as bold.</param>
        /// <returns>The bold-formatted string.</returns>
        public static string Bold(string text)
        {
            return $"*{EscapeMarkdownV2(text)}*";
        }

        /// <summary>
        /// Wraps the input string in italic formatting using Telegram MarkdownV2.
        /// The content is escaped to prevent interference with formatting.
        /// </summary>
        /// <param name="text">The input string to format as italic.</param>
        /// <returns>The italic-formatted string.</returns>
        public static string Italic(string text)
        {
            return $"_{EscapeMarkdownV2(text)}_";
        }

        /// <summary>
        /// Wraps the input string in inline code formatting using Telegram MarkdownV2.
        /// The content is escaped to prevent interference with formatting.
        /// </summary>
        /// <param name="text">The input string to format as inline code.</param>
        /// <returns>The inline code-formatted string.</returns>
        public static string InlineCode(string text)
        {
            return $"`{EscapeMarkdownV2(text)}`";
        }

        /// <summary>
        /// Creates a link using Telegram MarkdownV2 formatting.
        /// Both the link text and URL are escaped to prevent interference with formatting.
        /// </summary>
        /// <param name="text">The link text to be displayed.</param>
        /// <param name="url">The URL the link points to.</param>
        /// <returns>The MarkdownV2 formatted link.</returns>
        public static string Link(string text, string url)
        {
            return $"[{EscapeMarkdownV2(text)}]({EscapeMarkdownV2(url)})";
        }
    }
}