# MarkdownV2Extensions

Static helpers for building Telegram [MarkdownV2](https://core.telegram.org/bots/api#markdownv2-style) text. MarkdownV2 treats 18 punctuation characters as syntax, so any user-supplied text that contains them breaks the message unless each one is escaped with a backslash. The helpers escape the content for you and wrap it in the matching formatting markers, so handlers can build formatted replies without hand-written escaping.

Namespace: `TelegramBotFramework.Extensions`

## API

### `public static string EscapeMarkdownV2(string text)`
Escapes every MarkdownV2 reserved character in `text` by prefixing it with a backslash.

- **Parameters**
  - `text`: The raw text to escape.
- **Return value**
  - The escaped string. A `null` or empty input is returned unchanged.
- **Remarks**
  - Reserved characters: `_ * [ ] ( ) ~ ` > # + - = | { } . !`
  - Characters that are not reserved, such as letters, digits, spaces, commas, and `:`, are left as they are.
  - The backslash itself is not escaped. See [Limitations](#limitations).

---

### `public static string Bold(string text)`
Wraps the escaped `text` in MarkdownV2 bold markers (`*...*`).

- **Parameters**
  - `text`: The raw text to display in bold.
- **Return value**
  - The bold-formatted, escaped string.
- **Remarks**
  - The content is passed through `EscapeMarkdownV2`, so it cannot close the bold span early.

---

### `public static string Italic(string text)`
Wraps the escaped `text` in MarkdownV2 italic markers (`_..._`).

- **Parameters**
  - `text`: The raw text to display in italics.
- **Return value**
  - The italic-formatted, escaped string.
- **Remarks**
  - The content is passed through `EscapeMarkdownV2`.

---

### `public static string InlineCode(string text)`
Wraps the escaped `text` in MarkdownV2 inline code markers (`` `...` ``).

- **Parameters**
  - `text`: The raw text to display as inline code.
- **Return value**
  - The inline-code-formatted, escaped string.
- **Remarks**
  - Inside a code span, Telegram treats escapes the same way as elsewhere, so the output is safe to send with `parse_mode` set to `MarkdownV2`.

---

### `public static string Link(string text, string url)`
Creates a MarkdownV2 inline link (`[text](url)`).

- **Parameters**
  - `text`: The link text shown to the user.
  - `url`: The URL the link points to.
- **Return value**
  - The link, with both `text` and `url` escaped.
- **Remarks**
  - `url` is escaped with the same routine as the text, so every reserved character in it, including `.`, `-`, `_`, and `=`, gets a backslash. Telegram treats a backslash before a reserved character as an escape, so the link still resolves to the intended address, but the raw string differs from the URL. See [Limitations](#limitations).

## Escaping reference

| Input                   | Method                    | Output                              |
|-------------------------|---------------------------|-------------------------------------|
| `Hello, world!`         | `EscapeMarkdownV2`        | `Hello, world\!`                    |
| `v1.0`                  | `Bold`                    | `*v1\.0*`                           |
| `note_1`                | `Italic`                  | `_note\_1_`                         |
| `a+b`                   | `InlineCode`              | `` `a\+b` ``                        |
| `docs` / `https://example.com/a_b` | `Link`         | `[docs](https://example\.com/a\_b)` |
| `""`                    | `EscapeMarkdownV2`        | `""`                                |

## Limitations

- **Backslash is not escaped.** Telegram requires a literal `\` to be written as `\\`. `EscapeMarkdownV2` does not do this, so input that ends with a backslash, or has one before a reserved character, can change the meaning of the message. Escape backslashes yourself before calling the helpers if the input may contain them.
- **`null` in formatting helpers.** `Bold`, `Italic`, and `InlineCode` do not check for `null`. `Bold(null)` returns `**`, because the null interpolates as an empty string. Check input before calling them.
- **Links.** Telegram only requires `)` and `\` to be escaped inside the URL part of a link. Escaping the whole URL, as `Link` does, is accepted by Telegram but makes the raw output harder to read.
- **Sending.** These helpers only produce strings. The message must be sent with `parse_mode` set to `MarkdownV2`. The framework's `ITelegramApiClient.SendMessageAsync` sends plain text, so use a sending path that accepts a parse mode.

## Usage

```csharp
using TelegramBotFramework.Extensions;

string user = "alice_01";
string repo = "telegram-bot-framework-dotnet";
int stars = 42;

// Escape raw text that may contain reserved characters.
string safe = MarkdownV2Extensions.EscapeMarkdownV2($"Score: {stars}/100!");
// -> "Score: 42/100\!"

// Build a formatted reply from parts. Each part is escaped separately.
string reply = string.Join(
    " ",
    MarkdownV2Extensions.Bold(repo),
    "by",
    MarkdownV2Extensions.Italic(user),
    MarkdownV2Extensions.InlineCode($"v{stars}.0"),
    MarkdownV2Extensions.Link("Repository", $"https://github.com/Sarmkadan/{repo}"));
// -> "*telegram\-bot\-framework\-dotnet* by _alice\_01_ `v42\.0` [Repository](https://github\.com/Sarmkadan/telegram\-bot\-framework\-dotnet)"

// Send `reply` with parse_mode = MarkdownV2 through the bot's send path.
```
