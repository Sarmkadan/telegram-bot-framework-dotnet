# WebhookHandler

The `WebhookHandler` class serves as the primary component for processing incoming Telegram webhook requests within the `telegram-bot-framework-dotnet` library. It facilitates the ingestion, validation, and parsing of serialized JSON payloads from Telegram's servers into structured objects, exposing the essential data extracted from the update for subsequent application-level handling.

## API

### Constructors

*   **`public WebhookHandler()`**
    Initializes a new, empty instance of the `WebhookHandler` class.

### Methods

*   **`public async Task<TelegramUpdate?> ProcessUpdateAsync()`**
    Asynchronously processes the internal request state. Returns a populated `TelegramUpdate` object if the update is successfully parsed; otherwise, returns `null`.

### Properties

*   **`public bool ValidateWebhookRequest`**
    Indicates whether the current webhook request has passed validation checks, such as origin verification.
*   **`public long UpdateId`**
    The unique identifier assigned by Telegram to the update.
*   **`public UpdateType MessageType`**
    The type of the incoming update (e.g., Message, CallbackQuery).
*   **`public DateTime Timestamp`**
    The date and time when the update was received or generated.
*   **`public TelegramMessage? Message`**
    Contains the `TelegramMessage` object if the update type is a standard message; otherwise, `null`.
*   **`public string? CallbackData`**
    The payload associated with a callback query, if applicable.
*   **`public string? CallbackQueryId`**
    The unique identifier for the callback query, used for responding to the query.
*   **`public string? InlineQuery`**
    The query string if the update is an inline query.
*   **`public long MessageId`**
    The unique identifier of the message, if applicable.
*   **`public long ChatId`**
    The unique identifier of the chat where the message originated.
*   **`public long UserId`**
    The unique identifier of the user who triggered the update.
*   **`public string? Text`**
    The text content of the message, if applicable.
*   **`public DateTime? EditedTimestamp`**
    The date and time when the message was last edited, if applicable; otherwise, `null`.

## Usage

### Example 1: Basic Webhook Processing

```csharp
public async Task HandleTelegramWebhook(HttpContext context)
{
    var handler = new WebhookHandler();
    // Assuming framework handles request body binding
    var update = await handler.ProcessUpdateAsync();

    if (handler.ValidateWebhookRequest && update != null)
    {
        Console.WriteLine($"Received update {handler.UpdateId} from chat {handler.ChatId}");
    }
}
```

### Example 2: Inspecting Message Content

```csharp
public void ProcessTextMessage(WebhookHandler handler)
{
    if (handler.MessageType == UpdateType.Message && handler.Text != null)
    {
        Console.WriteLine($"User {handler.UserId} sent: {handler.Text}");
        
        if (handler.EditedTimestamp.HasValue)
        {
            Console.WriteLine($"Message edited at: {handler.EditedTimestamp}");
        }
    }
}
```

## Notes

*   **Thread Safety:** The `WebhookHandler` class maintains state regarding the processed request. It is not intended to be shared across threads simultaneously; a new instance should be created for each incoming request.
*   **Nullability:** Properties representing optional data (e.g., `Message`, `CallbackData`, `Text`) may return `null` depending on the type of update received. Always verify the `MessageType` before accessing specific property groups.
*   **Duplicate Timestamps:** The `Timestamp` property is exposed via multiple access points in the underlying implementation; usage of either provides the same value.
