# HealthController

Minimal health-check endpoint that reports whether the bot process is up and what the webhook pipeline is doing.

Namespace: `TelegramBotFramework.Controllers`
Visibility: `public sealed class`
Base class: `ControllerBase` (attributes: `[ApiController]`, `[Route("api/[controller]")]`)
Source: `src/TelegramBotFramework/Controllers/HealthController.cs`

## Purpose

`HealthController` answers one question for monitoring tools, load balancers, and uptime checks: is the bot process running, and how is its webhook intake doing?

Use it when you want to:

- Point a liveness or uptime probe (Docker `HEALTHCHECK`, Kubernetes probe, external monitor) at the bot.
- See whether the Telegram webhook is registered and which URL it points to.
- Watch the webhook queue: how many updates are waiting, how many were dispatched, and how many were dropped.

The controller holds no state of its own. Every call reads the current values from `IWebhookService`, so the numbers are always live.

## Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/health` | Returns the health report as JSON. |

The route comes from `[Route("api/[controller]")]`, so the controller name `Health` gives `api/health`.

### `GET /api/health`

Action: `Get()` returns `IActionResult`.

On success (HTTP `200 OK`) the body has this shape:

| Field | Type | Meaning |
|-------|------|---------|
| `status` | string | Always `"healthy"` on success. |
| `uptime` | string | Time since the controller was created, in the `c` (constant) format, `"d.hh:mm:ss.fffffff"` for long uptimes. |
| `uptimeSeconds` | number (long) | The same uptime as whole seconds. |
| `applicationStartTime` | string (UTC date-time) | When the controller instance was created. |
| `webhook.isRegistered` | boolean | Whether the Telegram webhook is currently registered. |
| `webhook.url` | string or null | The registered webhook URL. `null` when not registered. |
| `webhook.registeredAt` | string (UTC date-time) or null | When the last successful registration happened. |
| `webhook.updatesDispatched` | number (long) | Updates dispatched since the last registration. |
| `webhook.queueDepth` | number (int) | Updates currently waiting in the queue for a worker. |
| `webhook.queueCapacity` | number (int) | Maximum size of the queue. |
| `webhook.updatesDropped` | number (long) | Queued updates evicted under the `DropOldest` queue policy. |
| `timestamp` | string (UTC date-time) | When the report was generated. |

If reading the webhook state throws, the action logs the error and returns HTTP `500 Internal Server Error` with:

```json
{
  "status": "unhealthy",
  "error": "Health check failed",
  "timestamp": "2026-10-25T12:00:00.0000000Z"
}
```

The endpoint returns `200` whenever the process can answer. It does not check Telegram API connectivity, so a `200` means "the process and webhook service respond", not "Telegram is reachable".

## Dependencies

Constructor parameters (both required, `ArgumentNullException` if `null`):

- `IWebhookService webhookService` - source of the `WebhookInfo` snapshot (see `Integration/IWebhookService.cs`).
- `ILogger<HealthController> logger` - logs an information entry on each successful check and an error entry on failure.

ASP.NET Core creates a controller instance per request, and `_applicationStartTime` is captured in the constructor. As a result, `uptime` measures time since the current request's controller was created, not true process uptime. Use the `webhook.*` fields for state, and treat `uptime` as informational. If you need real process uptime, record the start time once at process level and report from there.

## Registration

`Program.cs` calls `MapControllers()`, so the controller is discovered automatically. `DependencyInjectionSetup.AddTelegramBotFramework` registers `IWebhookService` as a singleton, which is the only dependency the controller needs. No extra registration is required.

The controller has no `[Authorize]` attribute, so the endpoint is anonymous unless you add an authorization policy or a global filter.

## Usage example

### Check the endpoint

```bash
curl -s http://localhost:<port>/api/health
```

Replace `<port>` with the port the host listens on (see `launchSettings.json` or your `ASPNETCORE_URLS`).

Example response (values are illustrative):

```json
{
  "status": "healthy",
  "uptime": "1.02:15:30.4521000",
  "uptimeSeconds": 94530,
  "applicationStartTime": "2026-10-24T09:45:00.1234567Z",
  "webhook": {
    "isRegistered": true,
    "url": "https://bot.example.com/api/bot",
    "registeredAt": "2026-10-24T09:45:02.0000000Z",
    "updatesDispatched": 1482,
    "queueDepth": 0,
    "queueCapacity": 100,
    "updatesDropped": 0
  },
  "timestamp": "2026-10-25T11:59:45.6789012Z"
}
```

### Use it as a Docker health check

```dockerfile
HEALTHCHECK --interval=30s --timeout=5s --retries=3 \
  CMD curl -fsS http://localhost:8080/api/health || exit 1
```

`curl -f` exits non-zero on HTTP `500`, which marks the container unhealthy.

### Alert on a full queue

A `queueDepth` close to `queueCapacity`, or a growing `updatesDropped`, means updates are backing up. A monitor can parse the JSON and alert when either happens:

```bash
curl -s http://localhost:<port>/api/health \
  | jq -e '.webhook.queueDepth < (.webhook.queueCapacity * 0.8) and .webhook.updatesDropped == 0' > /dev/null \
  || echo "webhook queue under pressure"
```

## Related

- [WebhookService](WebhookService.md) - produces the `WebhookInfo` snapshot this controller reports.
- [BotController](BotController.md) - the webhook endpoint Telegram posts updates to.
- `Controllers/WebhookController.cs` - exposes webhook info through its own `GetInfo` action (no reference doc yet).
