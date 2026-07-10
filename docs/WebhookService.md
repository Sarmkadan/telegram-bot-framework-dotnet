# WebhookService

The `WebhookService` is the primary component within the `telegram-bot-framework-dotnet` responsible for managing Telegram webhook integration. It encapsulates the lifecycle of a bot's webhook, including registration and unregistration with the Telegram API, parsing and validating incoming HTTP requests, and dispatching valid updates to the configured bot handlers.

## API

### Constructor
`public WebhookService(...)`
Initializes a new instance of the `WebhookService` with required dependencies such as `HttpClient`, `IConfiguration`, and `ILogger`.

### StartAsync
`public async Task StartAsync(CancellationToken cancellationToken = default)`
Starts the webhook service and its underlying listener components. Throws an `InvalidOperationException` if the service is already running.

### StopAsync
`public async Task StopAsync(CancellationToken cancellationToken = default)`
Stops the webhook service and cleans up resources.

### RegisterAsync
`public async Task RegisterAsync(string url, CancellationToken cancellationToken = default)`
Registers the specified URL as the webhook endpoint with the Telegram API. Throws `HttpRequestException` on network failure or `TelegramApiException` if registration is rejected.

### UnregisterAsync
`public async Task UnregisterAsync(CancellationToken cancellationToken = default)`
Removes the webhook registration, effectively stopping Telegram from sending updates to the configured URL.

### DispatchUpdateAsync
`public async Task DispatchUpdateAsync(TelegramUpdate update, CancellationToken cancellationToken = default)`
Dispatches a validated `TelegramUpdate` to the registered update handler.

### GetInfo
`public WebhookInfo GetInfo()`
Retrieves the current status and configuration of the webhook from the Telegram API. Note: This operation may block briefly if cached info is stale and a network fetch is required.

### ParseAndValidateAsync
`public async Task<TelegramUpdate?> ParseAndValidateAsync(string requestBody, string secretToken, CancellationToken cancellationToken = default)`
Parses and validates an incoming raw JSON request body from Telegram, verifying the `secretToken` if provided. Returns a `TelegramUpdate` object if valid, otherwise returns `null`.

## Usage

### Registering a Webhook
```csharp
public async Task SetupWebhook(WebhookService webhookService, string baseUrl)
{
    var webhookUrl = $"{baseUrl}/api/telegram/webhook";
    await webhookService.RegisterAsync(webhookUrl);
    Console.WriteLine("Webhook registered successfully.");
}
```

### Handling an Incoming Webhook Request
```csharp
[HttpPost("api/telegram/webhook")]
public async Task<IActionResult> HandleUpdate([FromBody] string body, [FromHeader(Name = "X-Telegram-Bot-Api-Secret-Token")] string secretToken)
{
    var update = await _webhookService.ParseAndValidateAsync(body, secretToken);
    if (update != null)
    {
        await _webhookService.DispatchUpdateAsync(update);
        return Ok();
    }
    return BadRequest();
}
```

## Notes

*   **Thread Safety:** The `WebhookService` is designed to be thread-safe for its operations. Concurrent calls to `RegisterAsync` or `UnregisterAsync` should be avoided to prevent race conditions during the API state change.
*   **Async/Await:** All operations interacting with external network resources are performed asynchronously. Ensure proper management of `CancellationToken` to handle application shutdowns gracefully.
*   **Error Handling:** Implement robust error handling around `ParseAndValidateAsync`, as it deals with untrusted input from the Telegram server.
