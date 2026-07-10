# MessageFormatter

Utility class for formatting text content according to Telegram Bot API message formatting requirements and common presentation needs. Provides methods to transform raw text into valid plain text, Markdown, HTML, conversation-style text, and debug-friendly representations, along with utilities for truncating and previewing content.

## API

### `FormatAsPlainText(string text)`
Converts the input text into a plain text representation safe for use in Telegram Bot API messages where formatting is not supported. Removes or escapes characters that could interfere with plain text display, such as control characters and excessive whitespace.

- **Parameters**
  - `text` (string): The raw input text to format.

- **Returns**
  - `string`: A plain text version of the input, with formatting characters removed or escaped.

- **Throws**
  - `ArgumentNullException`: If `text` is `null`.

---

### `FormatAsMarkdown(string text)`
Transforms the input text into a Markdown-formatted string suitable for Telegram Bot API messages using MarkdownV2 syntax. Escapes special Markdown characters and ensures compatibility with the Telegram parser.

- **Parameters**
  - `text` (string): The raw input text to format.

- **Returns**
  - `string`: A Markdown-formatted version of the input, with special characters escaped.

- **Throws**
  - `ArgumentNullException`: If `text` is `null`.

---
### `FormatAsHtml(string text)`
Converts the input text into an HTML-formatted string safe for use in Telegram Bot API messages. Escapes HTML entities and ensures compatibility with the Telegram HTML parser.

- **Parameters**
  - `text` (string): The raw input text to format.

- **Returns**
  - `string`: An HTML-formatted version of the input, with HTML entities escaped.

- **Throws**
  - `ArgumentNullException`: If `text` is `null`.

---
### `FormatAsConversation(string text)`
Formats the input text as a conversation-style message, typically used for bot replies in a chat context. May include prefixes, line breaks, or other cues to simulate natural conversation flow.

- **Parameters**
  - `text` (string): The raw input text to format.

- **Returns**
  - `string`: A conversation-style formatted version of the input.

- **Throws**
  - `ArgumentNullException`: If `text` is `null`.

---
### `TruncateForPreview(string text, int maxLength)`
Truncates the input text to a specified maximum length for preview purposes, appending an ellipsis if truncation occurs. Useful for displaying shortened previews of longer messages.

- **Parameters**
  - `text` (string): The input text to truncate.
  - `maxLength` (int): The maximum allowed length of the output string, including the ellipsis.

- **Returns**
  - `string`: The truncated text, or the original text if it is shorter than `maxLength`. If truncated, the last three characters are replaced with an ellipsis (`…`).

- **Throws**
  - `ArgumentNullException`: If `text` is `null`.
  - `ArgumentOutOfRangeException`: If `maxLength` is less than 3.

---
### `FormatForDebug(string text)`
Formats the input text for debug output, including visible representations of whitespace and control characters. Useful for logging and debugging scenarios where raw content visibility is important.

- **Parameters**
  - `text` (string): The raw input text to format.

- **Returns**
  - `string`: A debug-formatted version of the input, with whitespace and control characters made visible (e.g., `\n` becomes `[LF]`, spaces become `[SP]`).

- **Throws**
  - `ArgumentNullException`: If `text` is `null`.

## Usage
