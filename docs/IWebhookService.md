# IWebhookService

The `IWebhookService` interface provides a standardized abstraction for managing and monitoring the Telegram webhook configuration for a bot. It allows consumers to inspect the current state of the webhook registration, including the active status, the configured endpoint URL, the registration timestamp, and the cumulative count of updates dispatched through the webhook.

## API

### `bool IsRegistered`
Indicates whether a webhook is currently registered with the Telegram Bot API. Returns `true` if a webhook is active, otherwise `false`.

### `string? Url`
The current URL configured for the webhook endpoint. Returns `null` if no webhook is currently registered.

### `DateTime? RegisteredAt`
The timestamp indicating when the current webhook was successfully registered. Returns `null` if no webhook is currently registered.

### `long UpdatesDispatched`
The total number of updates dispatched via the webhook since the service was initialized or the webhook was last registered. This value is used for monitoring throughput.

## Usage

### Checking Webhook Status
```csharp
if (webhookService.IsRegistered)
{
    Console.WriteLine($"Webhook is currently active at: {webhookService.Url}");
}
else
{
    Console.WriteLine("No webhook is currently registered.");
}
```

### Monitoring Webhook Metrics
```csharp
long totalUpdates = webhookService.UpdatesDispatched;
DateTime? registrationTime = webhookService.RegisteredAt;

if (webhookService.IsRegistered)
{
    Console.WriteLine($"Active since: {registrationTime?.ToString("O")}");
    Console.WriteLine($"Total updates dispatched: {totalUpdates}");
}
```

## Notes

- **Nullability**: `Url` and `RegisteredAt` are nullable types and will return `null` when `IsRegistered` is `false`. Implementations must ensure these properties are updated atomically or synchronized correctly when the webhook state changes.
- **Thread Safety**: Implementations of `IWebhookService` should be thread-safe for property access. However, because these properties reflect the state of an external network registration, values may be updated asynchronously by background tasks; consumers should treat the values as a snapshot of the current state.
- **Counter Reset**: The `UpdatesDispatched` property value is generally intended to persist for the lifetime of the `IWebhookService` instance and may reset to zero upon service re-initialization or a successful re-registration of the webhook.
