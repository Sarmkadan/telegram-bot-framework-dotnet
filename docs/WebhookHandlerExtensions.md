# WebhookHandlerExtensions

Provides a set of static extension methods for the `WebhookHandler` class that simplify access to common properties of the incoming Telegram update. These methods allow handlers to retrieve the message text, check for callback data, and obtain the chat or user identifier without directly inspecting the underlying update object.

## API

### `GetMessageText`

```csharp
public static string? GetMessageText(this WebhookHandler handler)
```

Returns the text content of the message associated with the current update, or `null` if no message is present or the message does not contain text (e.g., it is a photo, sticker, or callback query).

- **Parameters**  
  `handler` – The `WebhookHandler` instance that is currently processing an update.

- **Returns**  
  `string?` – The message text, or `null` if the update does not contain a text message.

- **Throws**  
  `InvalidOperationException` – If `handler` is not currently processing any update (i.e., `handler.Update` is `null`).

### `HasCallbackData`

```csharp
public static bool HasCallbackData(this WebhookHandler handler)
```

Indicates whether the current update contains callback query data (i.e., originates from an inline keyboard button press).

- **Parameters**  
  `handler` – The `WebhookHandler` instance that is currently processing an update.

- **Returns**  
  `bool` – `true` if the update is a `CallbackQuery` update; otherwise `false`.

- **Throws**  
  `InvalidOperationException` – If `handler` is not currently processing any update.

### `GetChatId`

```csharp
public static long GetChatId(this WebhookHandler handler)
```

Retrieves the unique identifier of the chat from which the current update originated.

- **Parameters**  
  `handler` – The `WebhookHandler` instance that is currently processing an update.

- **Returns**  
  `long` – The chat ID.

- **Throws**  
  `InvalidOperationException` – If `handler` is not currently processing any update, or if the update does not contain a chat (e.g., a `CallbackQuery` without a message).

### `GetUserId`

```csharp
public static long GetUserId(this WebhookHandler handler)
```

Retrieves the unique identifier of the user who sent the current update.

- **Parameters**  
  `handler` – The `WebhookHandler` instance that is currently processing an update.

- **Returns**  
  `long` – The user ID.

- **Throws**  
  `InvalidOperationException` – If `handler` is not currently processing any update, or if the update does not contain a user (e.g., a channel post without a sender).

## Usage

### Example 1: Responding to a text message

```csharp
public class MyHandler : WebhookHandler
{
    protected override async Task HandleUpdateAsync(CancellationToken cancellationToken)
    {
        string? text = this.GetMessageText();
        if (text != null)
        {
            long chatId = this.GetChatId();
            await BotClient.SendTextMessageAsync(chatId, $"You said: {text}");
        }
    }
}
```

### Example 2: Handling a callback query

```csharp
public class MyHandler : WebhookHandler
{
    protected override async Task HandleUpdateAsync(CancellationToken cancellationToken)
    {
        if (this.HasCallbackData())
        {
            long userId = this.GetUserId();
            long chatId = this.GetChatId();
            await BotClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, $"User {userId} pressed a button.");
        }
    }
}
```

## Notes

- All methods assume that the `WebhookHandler` is currently processing a valid update. Calling any of them outside of the `HandleUpdateAsync` method (or equivalent lifecycle) will throw an `InvalidOperationException`.
- `GetMessageText` returns `null` for updates that are not text messages (e.g., photos, stickers, voice messages, or callback queries). It does not throw in these cases; only when no update is set.
- `GetChatId` and `GetUserId` may throw if the update type does not contain the respective identifier. For example, a `CallbackQuery` without an associated message will have no chat, and a channel post without a sender will have no user.
- These extension methods are thread-safe when used within a single `WebhookHandler` instance, as the underlying update reference is immutable during the handling of a single request. Concurrent access to the same handler from multiple threads is not supported.
