# WebhookOptions

`WebhookOptions` defines the configuration required to register and manage an HTTPS webhook for a Telegram bot within the `telegram-bot-framework-dotnet`. It encapsulates the connection details, security parameters, and filtering options necessary for the framework to correctly receive and process updates delivered by the Telegram Bot API.

## API

*   **`string Url`**
    The public HTTPS URL where Telegram should send bot updates. This must be a valid, reachable URL accessible by Telegram servers.

*   **`string? SecretToken`**
    An optional, secret string that Telegram will include in the `X-Telegram-Bot-Api-Secret-Token` header. Use this to verify that incoming requests originate from Telegram rather than unauthorized sources.

*   **`int MaxConnections`**
    The maximum allowed number of simultaneous HTTPS connections to the webhook URL. Valid range is 1-100.

*   **`string[]? AllowedUpdates`**
    An optional list of update types that the bot should receive (e.g., `["message", "edited_channel_post"]`). If `null`, the bot receives all update types.

*   **`string ListenPath`**
    The relative path on the local server application where the framework listens for incoming webhook requests from Telegram (e.g., `"/webhook/my-bot"`).

*   **`bool DropPendingUpdates`**
    If set to `true`, instructs Telegram to discard any updates that were accumulated while the bot was offline or not processing requests.

*   **`void Validate()`**
    Performs validation on the current configuration. Throws an `ArgumentException` if required fields (such as `Url` or `ListenPath`) are null or improperly formatted, or if numeric values fall outside of allowed ranges.

## Usage

### Minimal Configuration
```csharp
var options = new WebhookOptions
{
    Url = "https://mybot.example.com/api/updates",
    ListenPath = "/api/updates"
};

options.Validate();
```

### Detailed Configuration with Security
```csharp
var options = new WebhookOptions
{
    Url = "https://mybot.example.com/bot/handle",
    SecretToken = "super-secret-token-value",
    MaxConnections = 40,
    AllowedUpdates = new[] { "message", "callback_query" },
    ListenPath = "/bot/handle",
    DropPendingUpdates = true
};

options.Validate();
```

## Notes

*   **Validation:** Always invoke `Validate()` before passing the `WebhookOptions` object to the framework's initialization methods to ensure configuration integrity and prevent runtime failures during webhook registration.
*   **Security:** When using `SecretToken`, ensure that the secret is stored securely (e.g., using user secrets or environment variables) and is not hardcoded in source control.
*   **Thread Safety:** This class is designed primarily as a Data Transfer Object (DTO) for configuration. It is not inherently thread-safe for concurrent writes. It should be fully populated and validated on the main thread during the application startup sequence before being passed to framework services for read-only access.
*   **HTTPS Requirement:** Telegram requires the `Url` to be an HTTPS endpoint. HTTP URLs will be rejected by the Telegram API.
