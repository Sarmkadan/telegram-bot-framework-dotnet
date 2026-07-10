# TelegramApiClient

The `TelegramApiClient` class provides a structured interface for interacting with the Telegram Bot API. It encapsulates the necessary networking logic to perform common bot operations such as sending, editing, and deleting messages, managing webhooks, and responding to callback queries, abstracting the underlying HTTP transport layer.

## API

### Constructors

#### `public TelegramApiClient()`
Initializes a new instance of the `TelegramApiClient` class.

### Methods

#### `public async Task<bool> SendMessageAsync(long chatId, string text)`
Sends a text message to a specific chat.
*   **Parameters**: `chatId` (Unique identifier for the target chat), `text` (Text of the message).
*   **Returns**: `true` if the message was sent successfully, `false` otherwise.
*   **Exceptions**: Throws `HttpRequestException` if the network request fails or the API returns a non-success status code.

#### `public async Task<bool> SendMessageWithButtonsAsync(long chatId, string text, IEnumerable<IEnumerable<InlineKeyboardButton>> buttons)`
Sends a text message with an inline keyboard attached.
*   **Parameters**: `chatId` (Target chat identifier), `text` (Message text), `buttons` (A collection of rows, each containing a collection of buttons).
*   **Returns**: `true` if the message was sent successfully, `false` otherwise.
*   **Exceptions**: Throws `HttpRequestException` if the network request fails.

#### `public async Task<bool> EditMessageAsync(long chatId, int messageId, string newText)`
Edits the text of an existing message.
*   **Parameters**: `chatId` (Target chat identifier), `messageId` (Identifier of the message to edit), `newText` (The updated text content).
*   **Returns**: `true` if the message was edited successfully, `false` otherwise.
*   **Exceptions**: Throws `HttpRequestException` if the request fails, for example, if the message cannot be found or edited.

#### `public async Task<bool> DeleteMessageAsync(long chatId, int messageId)`
Deletes a message.
*   **Parameters**: `chatId` (Target chat identifier), `messageId` (Identifier of the message to delete).
*   **Returns**: `true` if the message was deleted successfully, `false` otherwise.
*   **Exceptions**: Throws `HttpRequestException` if the request fails.

#### `public async Task<string?> GetMeAsync()`
Retrieves basic information about the bot.
*   **Returns**: A JSON string containing bot details if successful, or `null` if the request fails.
*   **Exceptions**: Throws `HttpRequestException` on network failure.

#### `public async Task<bool> AnswerCallbackQueryAsync(string callbackQueryId, string text)`
Responds to a callback query initiated by a button click.
*   **Parameters**: `callbackQueryId` (Unique identifier for the query), `text` (Optional text to display to the user).
*   **Returns**: `true` if the answer was sent successfully, `false` otherwise.
*   **Exceptions**: Throws `HttpRequestException` if the request fails.

#### `public async Task<bool> SetWebhookAsync(string url)`
Configures the bot to use a webhook for receiving updates.
*   **Parameters**: `url` (The HTTPS URL for the webhook).
*   **Returns**: `true` if the webhook was set successfully, `false` otherwise.
*   **Exceptions**: Throws `HttpRequestException` on network failure.

#### `public async Task<bool> RemoveWebhookAsync()`
Removes the current webhook configuration, allowing the bot to use long polling.
*   **Returns**: `true` if the webhook was removed successfully, `false` otherwise.
*   **Exceptions**: Throws `HttpRequestException` on network failure.

### Properties

#### `public bool IsEnabled`
Indicates whether the client is currently active and processing requests.

### Methods (Logging)

#### `public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)`
Logs a message to the configured logging infrastructure.
*   **Parameters**: Standard parameters as defined by the `ILogger` interface.

## Usage

### Sending a Simple Message
```csharp
var client = new TelegramApiClient();
if (client.IsEnabled)
{
    bool success = await client.SendMessageAsync(123456789, "Hello, Telegram!");
}
```

### Sending a Message with Inline Buttons
```csharp
var client = new TelegramApiClient();
var buttons = new List<List<InlineKeyboardButton>>
{
    new List<InlineKeyboardButton> { new InlineKeyboardButton("Click Me", "callback_data") }
};
bool success = await client.SendMessageWithButtonsAsync(123456789, "Choose an option:", buttons);
```

## Notes

*   **Thread Safety**: While the `TelegramApiClient` is designed for use in asynchronous contexts, instances should generally be managed as singleton-scoped or transient-scoped dependencies within a dependency injection container.
*   **Exception Handling**: All asynchronous API methods throw `HttpRequestException` on communication failure. Implement robust `try-catch` blocks to handle network timeouts, rate limiting, and invalid API responses.
*   **Webhook Conflicts**: Do not attempt to use long polling (`GetUpdates`) while a webhook is actively set via `SetWebhookAsync`. Always ensure webhooks are properly removed via `RemoveWebhookAsync` if switching polling modes.
