# WebhookController

`WebhookController` is the ASP.NET Core MVC controller that exposes the Telegram webhook endpoint. Telegram sends every update as an HTTP `POST` to this endpoint. The controller validates the request, hands the parsed update to `WebhookService` for background processing, and answers Telegram right away. It also exposes a `GET` endpoint that reports the current webhook state.

The class is declared in `src/TelegramBotFramework/Controllers/WebhookController.cs` in the `TelegramBotFramework.Controllers` namespace:

```csharp
[ApiController]
[Route("api/webhook")]
public sealed class WebhookController : ControllerBase, IWebhookController
```

Route strings and response messages come from the internal `WebhookControllerConstants` class.

## Purpose

- Accept Telegram updates over HTTP and reject requests that are malformed or fail secret-token validation.
- Keep the response fast. The update is queued on a bounded channel, so slow handlers never delay the HTTP response and Telegram does not retry because of them.
- Apply back-pressure. When the queue is full under the `Reject` policy, the controller answers `429 Too Many Requests` with `Retry-After`, so Telegram redelivers the update later.
- Report status for health checks and diagnostics through the info endpoint.

## Constructor

### `public WebhookController(WebhookService webhookService, ILogger<WebhookController> logger)`

Both arguments are required. A `null` value throws `ArgumentNullException`. The controller is resolved by ASP.NET Core dependency injection. Its dependencies are not created by the controller itself.

## Endpoints

| Method | Route | Action | Purpose |
|--------|-------|--------|---------|
| `POST` | `/api/webhook/telegram` | `ReceiveUpdate` | Receives a Telegram update and queues it for processing. |
| `GET` | `/api/webhook/info` | `GetInfo` | Returns the current `WebhookInfo` snapshot as JSON. |

### `POST /api/webhook/telegram` — `ReceiveUpdate`

```csharp
public async Task<IActionResult> ReceiveUpdate(CancellationToken cancellationToken = default)
```

The action runs these steps in order:

1. **Content type.** The action accepts only `Content-Type: application/json`, as set by `[Consumes]`. ASP.NET Core rejects other media types before the action runs.
2. **Size check.** If the `Content-Length` header is present and exceeds `WebhookOptions.MaxRequestBodySize` (default 1 MB), the controller returns `413 Payload Too Large`. The check uses only the declared length. Requests without `Content-Length` are read in full.
3. **Read body.** The body is read as text. An empty or whitespace-only body returns `400 Bad Request`.
4. **Secret token.** The value of the `X-Telegram-Bot-Api-Secret-Token` header is passed to `WebhookService.ParseAndValidateAsync`, which compares it with `WebhookOptions.SecretToken` and parses the JSON body. If either step fails, the controller returns `401 Unauthorized`.
5. **Queue.** The parsed update is passed to `WebhookService.EnqueueUpdate`. The result decides the response, as shown below.

| Status | When | Notes |
|--------|------|-------|
| `200 OK` | The update was queued (`WebhookEnqueueResult.Accepted`). | Empty body. Telegram treats this as delivered. |
| `400 Bad Request` | The body is empty or whitespace only. | Body: `Request body is required.` |
| `401 Unauthorized` | The secret token does not match, or the body cannot be parsed as an update. | No body. |
| `413 Payload Too Large` | `Content-Length` is greater than `MaxRequestBodySize`. | Body: `Request body too large.` |
| `429 Too Many Requests` | The queue is full and the policy is `WebhookQueueFullPolicy.Reject` (`WebhookEnqueueResult.Rejected`). | Sends `Retry-After: 1`. Body: `Update queue is full, retry later.` |
| `503 Service Unavailable` | The service is stopping and the queue is closed (`WebhookEnqueueResult.Closed`). | Body: `Bot is shutting down.` |

Processing errors inside handlers never change this response, because the update is answered before any handler runs.

### `GET /api/webhook/info` — `GetInfo`

```csharp
public IActionResult GetInfo()
```

Returns `200 OK` with the `WebhookInfo` object from `WebhookService.GetInfo()`:

| Property | Type | Meaning |
|----------|------|---------|
| `IsRegistered` | `bool` | Whether the webhook is registered with Telegram. |
| `Url` | `string?` | The registered URL, or `null` if not registered. |
| `RegisteredAt` | `DateTime?` | When the webhook was registered. |
| `UpdatesDispatched` | `long` | Number of updates processed by the background workers. |
| `QueueDepth` | `int` | Number of updates currently waiting in the queue. |
| `QueueCapacity` | `int` | Maximum number of queued updates. |
| `UpdatesDropped` | `long` | Number of queued updates evicted under `WebhookQueueFullPolicy.DropOldest`. |

Do not expose this endpoint publicly without protection. It reveals the registered URL and queue state.

## Configuration

The controller reads its settings from `WebhookOptions` through `WebhookService`:

| Option | Default | Effect on this controller |
|--------|---------|---------------------------|
| `SecretToken` | `null` | When set, requests without a matching `X-Telegram-Bot-Api-Secret-Token` header get `401`. |
| `MaxRequestBodySize` | `1_048_576` (1 MB) | Upper limit for the declared `Content-Length`; larger requests get `413`. |
| `QueueCapacity` | `1000` | Maximum number of updates waiting in the queue. |
| `QueueFullPolicy` | `Reject` | `Reject` returns `429` when the queue is full. `DropOldest` evicts the oldest queued update instead, so the response is always `200`. |
| `ListenPath` | `/api/webhook/telegram` | See the note below. |

> **Note on `ListenPath`:** the XML documentation on the class says the endpoint follows `WebhookOptions.ListenPath`. In the current code, the route is fixed by the `[Route]` and `[HttpPost]` attributes, which combine `WebhookControllerConstants.Route` and `TelegramRoute` into `api/webhook/telegram`. `ListenPath` is not read by this controller. Set the route you want Telegram to call through the URL you register with `WebhookOptions.Url`, and do not rely on `ListenPath` to change the route.

Use `WebhookOptionsBuilder` to build the options:

```csharp
var options = WebhookOptionsBuilder.Create()
    .WithUrl("https://bot.example.com/api/webhook/telegram")
    .WithSecretToken(secretToken)          // 1-256 chars: A-Z a-z 0-9 _ -
    .WithQueueCapacity(1000)
    .WithQueueFullPolicy(WebhookQueueFullPolicy.Reject)
    .Build();
```

`AddWebhookService` calls `WebhookOptions.Validate()`, which rejects a `ListenPath` that does not start with `/`, a non-positive `MaxRequestBodySize`, and an invalid secret token.

## Usage

### Hosting the endpoint

The controller is an ordinary MVC controller. Its assembly must be part of the application, and the services it depends on must be registered. In the framework's self-contained host, `WebhookServer` adds the controller assembly with `AddApplicationPart` and maps controllers with `MapControllers`.

For a custom ASP.NET Core host, register the webhook service and map controllers:

```csharp
builder.Services.AddControllers()
    .AddApplicationPart(typeof(WebhookController).Assembly);

var app = builder.Build();
app.MapControllers();
await app.RunAsync();
```

> **Note on DI:** the constructor takes the concrete `WebhookService` type. `AddWebhookService` registers the service as `IWebhookService` and the hosted service, but in the current code it does not register `WebhookService` itself. Before you rely on the controller, confirm that `WebhookService` can be resolved from the container in your host. If it cannot, resolution of `WebhookController` will fail.

### Calling the endpoint

Telegram calls the endpoint once you register the webhook URL. The same request can be sent by hand for testing. Replace the placeholder values:

```bash
curl -i -X POST "https://bot.example.com/api/webhook/telegram" \
  -H "Content-Type: application/json" \
  -H "X-Telegram-Bot-Api-Secret-Token: $WEBHOOK_SECRET" \
  --data '{"update_id":1,"message":{"message_id":1,"date":1700000000,"chat":{"id":42,"type":"private"},"text":"/start"}}'
```

Expected results: `200 OK` when the update is queued, `401` when the header is missing or wrong, `429` with `Retry-After: 1` when the queue is full.

Read the queue state:

```bash
curl -s "https://bot.example.com/api/webhook/info"
```

```json
{
  "isRegistered": true,
  "url": "https://bot.example.com/api/webhook/telegram",
  "registeredAt": "2026-10-20T08:15:00Z",
  "updatesDispatched": 1532,
  "queueDepth": 0,
  "queueCapacity": 1000,
  "updatesDropped": 0
}
```

### Handling `429` from a sender

Telegram retries updates that the endpoint did not acknowledge with `2xx`. When you send updates from your own tooling, honour the `Retry-After` header on `429` responses and retry after that many seconds.

## Notes

- The response is sent before any handler runs. Handler errors are logged by the background workers and do not change the HTTP status.
- The secret token is compared as part of `WebhookService.ParseAndValidateAsync`. The controller logs a warning on failure but does not log the token.
- `ReceiveUpdate` and `GetInfo` are the only public actions. The controller implements `IWebhookController`, which declares the same two members.
- Set `SecretToken` in production. Without it, anyone who knows the URL can post fake updates.
