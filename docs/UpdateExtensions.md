# UpdateExtensions

Extension methods on `Telegram.Bot.Types.Update` that pull out common data without repeating the per-update-type null checks. They cover the chat ID, user ID, text payload, command parsing, and update-type classification. Each lookup checks the update types that can carry the value, in a fixed order, and returns the first one it finds.

Namespace: `TelegramBotFramework.Extensions`

## API

### Chat, user, and text extraction

### `public static long? GetChatId(this Update update)`
Returns the chat ID associated with the update, or `null` if the update has no chat.

- **Parameters**
  - `update`: The Telegram update. Must not be `null`.
- **Return value**
  - The chat ID, or `null` when the update does not contain a chat.
- **Remarks**
  - Checks, in order: `Message`, `EditedMessage`, `ChannelPost`, `EditedChannelPost`, then the message attached to `CallbackQuery`.
  - Inline queries and chosen inline results have no chat and return `null`.
  - Throws `ArgumentNullException` if `update` is `null`.

---

### `public static long? GetUserId(this Update update)`
Returns the ID of the user who triggered the update, or `null` if none is available.

- **Parameters**
  - `update`: The Telegram update. Must not be `null`.
- **Return value**
  - The user ID, or `null` when the update does not contain a user.
- **Remarks**
  - Checks, in order: `Message`, `EditedMessage`, `ChannelPost`, `EditedChannelPost`, `CallbackQuery`, `InlineQuery`, `ChosenInlineResult`, then `PollAnswer.User`.
  - Channel posts can have no sender, in which case the lookup continues to the next source.
  - Throws `ArgumentNullException` if `update` is `null`.

---

### `public static string? GetText(this Update update)`
Returns the textual payload of the update, or `null` if there is none.

- **Parameters**
  - `update`: The Telegram update. Must not be `null`.
- **Return value**
  - The text, or `null` when the update has no textual data.
- **Remarks**
  - Checks, in order: message text (`Message`, `EditedMessage`, `ChannelPost`, `EditedChannelPost`), then `CallbackQuery.Data`, `InlineQuery.Query`, and `ChosenInlineResult.Query`.
  - Empty strings are skipped, so a later source is used when an earlier one is empty.
  - Messages without text (photos, stickers, and so on) fall through to the later sources and usually return `null`.
  - Throws `ArgumentNullException` if `update` is `null`.

---

### `public static bool IsCommand(this Update update, out string command, out string args)`
Determines whether the update's text is a bot command (starts with `/`) and splits it into the command name and its arguments.

- **Parameters**
  - `update`: The Telegram update.
  - `command`: When the method returns `true`, the command name without the leading `/`. Set to `string.Empty` otherwise.
  - `args`: When the method returns `true`, the trimmed remainder of the text after the command. Set to `string.Empty` if there are no arguments.
- **Return value**
  - `true` if the text starts with `/`; otherwise, `false`.
- **Remarks**
  - Both out parameters are always assigned, even when the method returns `false`.
  - The text is split on the first space character only. Other whitespace, such as a newline, is not a separator.
  - The command name is not normalized. A group-style mention such as `/start@MyBot` keeps the `@MyBot` suffix in `command`.
  - Text that is only a slash (`/` or `/   `) currently throws `IndexOutOfRangeException`. Check the text with `GetText()` before calling if such input is possible.
  - Calls `GetText()`, so a `null` update throws `ArgumentNullException`.

---

### Update-type classification

Each method returns `true` when `update.Type` matches the named `UpdateType`. A `null` update returns `false` and does not throw.

| Method | Matches `UpdateType` |
|--------|----------------------|
| `IsMessage` | `Message` |
| `IsEditedMessage` | `EditedMessage` |
| `IsChannelPost` | `ChannelPost` |
| `IsEditedChannelPost` | `EditedChannelPost` |
| `IsInlineQuery` | `InlineQuery` |
| `IsChosenInlineResult` | `ChosenInlineResult` |
| `IsCallbackQuery` | `CallbackQuery` |
| `IsPollAnswer` | `PollAnswer` |
| `IsPreCheckoutQuery` | `PreCheckoutQuery` |
| `IsShippingQuery` | `ShippingQuery` |

All of them have the signature `public static bool IsXxx(this Update update)`.

## Usage

```csharp
using TelegramBotFramework.Extensions;
using Telegram.Bot.Types;

public static class UpdateRouter
{
    public static void Route(Update update)
    {
        // Identify the sender and chat, if the update has them.
        long? chatId = update.GetChatId();
        long? userId = update.GetUserId();

        if (update.IsCommand(out string command, out string args))
        {
            // "/search cats and dogs" -> command = "search", args = "cats and dogs"
            HandleCommand(chatId, userId, command, args);
            return;
        }

        if (update.IsCallbackQuery())
        {
            // Callback data is also returned by GetText().
            HandleCallback(update.GetText());
            return;
        }

        string? text = update.GetText();
        if (!string.IsNullOrEmpty(text))
        {
            HandleText(chatId, text);
        }
    }

    private static void HandleCommand(long? chatId, long? userId, string command, string args) { }
    private static void HandleCallback(string? data) { }
    private static void HandleText(long? chatId, string text) { }
}
```
