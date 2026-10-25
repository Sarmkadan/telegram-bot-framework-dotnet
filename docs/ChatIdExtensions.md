# ChatIdExtensions

Extension methods on `long` for classifying and formatting Telegram chat IDs. Telegram encodes the chat type in the sign and prefix of the ID: private chats are positive, basic groups are negative, and supergroups and channels are negative with a `-100` prefix. These helpers let handlers branch on chat type without repeating that logic.

Namespace: `TelegramBotFramework.Extensions`

## API

### `public static bool IsGroup(this long chatId)`
Returns `true` if the chat ID belongs to a basic group (negative, and not starting with `-100`).

- **Parameters**
  - `chatId`: The Telegram chat ID.
- **Return value**
  - `true` for a basic group; otherwise, `false`.
- **Remarks**
  - Supergroups and channels (`-100...`) return `false`. Use `IsChannel` for those.

---

### `public static bool IsChannel(this long chatId)`
Returns `true` if the chat ID belongs to a channel or supergroup (negative, starting with `-100`).

- **Parameters**
  - `chatId`: The Telegram chat ID.
- **Return value**
  - `true` for a channel or supergroup; otherwise, `false`.
- **Remarks**
  - Telegram uses the same `-100` prefix for both channels and supergroups, so they cannot be told apart from the ID alone.

---

### `public static bool IsPrivate(this long chatId)`
Returns `true` if the chat ID belongs to a private (one-to-one) chat, which is any positive ID.

- **Parameters**
  - `chatId`: The Telegram chat ID.
- **Return value**
  - `true` for a private chat; otherwise, `false`.
- **Remarks**
  - `0` is not a valid chat ID and returns `false`.

---

### `public static string ToTelegramString(this long chatId)`
Returns the string form of the chat ID as Telegram uses it, for example in API payloads, logs, or callback data.

- **Parameters**
  - `chatId`: The Telegram chat ID.
- **Return value**
  - The decimal string representation of `chatId`, including the minus sign for negative IDs (for example `"-100123456"`).
- **Remarks**
  - Equivalent to `chatId.ToString()`. No culture-specific formatting is applied to the digits.

## Classification summary

| Chat ID              | `IsPrivate` | `IsGroup` | `IsChannel` |
|----------------------|-------------|-----------|-------------|
| `123456`             | `true`      | `false`   | `false`     |
| `-123456`            | `false`     | `true`    | `false`     |
| `-100123456`         | `false`     | `false`   | `true`      |
| `-100`               | `false`     | `false`   | `true`      |
| `0`                  | `false`     | `false`   | `false`     |

## Usage

```csharp
using TelegramBotFramework.Extensions;

long chatId = update.Message!.Chat.Id;

if (chatId.IsPrivate())
{
    // Reply directly to the user.
}
else if (chatId.IsGroup())
{
    // Basic group: may need to address the sender by name.
}
else if (chatId.IsChannel())
{
    // Channel or supergroup: check admin rights before posting.
}

// Logging or building a string key for a chat.
string key = $"chat:{chatId.ToTelegramString()}";
```
