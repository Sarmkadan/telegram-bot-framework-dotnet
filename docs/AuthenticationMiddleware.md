# AuthenticationMiddleware

`AuthenticationMiddleware` is an ASP.NET Core middleware component that protects the HTTP surface of a bot host with a single shared API key. Every request whose path is not on the public allow-list must present the configured key, either as a Bearer token, in the `X-API-Key` header, or in the `api_key` query parameter. Requests that fail the check receive `401 Unauthorized` and never reach the rest of the pipeline.

The middleware only answers "is this caller holding the configured key?". It does not create a user identity: on success it records a timestamp in `HttpContext.Items` and does not populate `HttpContext.User`. Authorization decisions that depend on claims therefore need a different mechanism.

Namespace: `TelegramBotFramework.Middleware`
Source: `src/TelegramBotFramework/Middleware/AuthenticationMiddleware.cs`

## API

### `public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)`

Creates the middleware and initializes the public endpoint allow-list.

- **Parameters**
  - `next`: The next delegate in the ASP.NET Core pipeline. Must not be `null`.
  - `logger`: Logger used to record rejected attempts. Must not be `null`.
- **Exceptions**
  - `ArgumentNullException`: `next` or `logger` is `null`.

### `public async Task InvokeAsync(HttpContext context, Models.BotConfiguration config)`

Runs the authentication check for the current request. ASP.NET Core resolves `config` from dependency injection, so it must be registered (the framework's `AddTelegramBotFramework` call does this).

- **Parameters**
  - `context`: The `HttpContext` for the current request. Must not be `null`.
  - `config`: The bot configuration. Its `ApiKey` property holds the expected key.
- **Return Value**
  A `Task` that completes when the request has either been passed to the next middleware or rejected.
- **Exceptions**
  - `ArgumentNullException`: `context` or `config` is `null`.

## Behavior

### Public endpoints

Requests are not checked when the request path **starts with** one of the following values (case-insensitive prefix match):

| Prefix | Purpose |
|--------|---------|
| `/health` | Health probe |
| `/api/webhook` | Telegram webhook endpoint |
| `/swagger` | API documentation |
| `/api/v1/bot/update` | Bot update endpoint |

Because the match is a prefix match, paths such as `/healthz` or `/swagger-internal` are also treated as public. Keep this in mind when naming new routes.

### Credential sources

For non-public paths the middleware looks for the key in this order:

1. **`Authorization` header with the `Bearer ` scheme** (scheme matched case-insensitively). If this header is present with a Bearer scheme, its token is the only credential that is checked. A wrong Bearer token is rejected even if a valid `X-API-Key` header is also sent; the other sources are not consulted.
2. **`X-API-Key` header**. Checked only when no Bearer credential was presented. A non-Bearer `Authorization` header (for example `Basic ...`) is ignored and this source is consulted instead.
3. **`api_key` query parameter**. Checked only when neither header was presented. This is the least secure option because query strings tend to end up in access logs and proxy logs; prefer the header.

If none of these is present, the request is rejected.

### Key comparison

The presented value is compared to `config.ApiKey` with ordinal, case-sensitive string equality. Whitespace is not trimmed.

If `config.ApiKey` is `null` or empty, every non-public request is rejected. The middleware fails closed rather than allowing unauthenticated access.

### Rejected requests

When the check fails, the middleware:

- logs a warning using the message `Unauthorized access attempt from {IP} to {Path}` with the remote IP address and request path;
- sets the response status code to `401`;
- writes the body `Unauthorized`;
- does **not** call the next middleware.

### Accepted requests

When the check succeeds, the middleware stores the UTC time of the check in `context.Items["AuthenticatedAt"]` (a `DateTime`) and calls the next middleware.

## Usage

### Registering in the pipeline

Place the middleware after HTTPS redirection and before the middleware and endpoints that must be protected. The source comment in the file notes that authentication has to run before authorization.

```csharp
using TelegramBotFramework.Configuration;
using TelegramBotFramework.Middleware;
using TelegramBotFramework.Models;

var builder = WebApplication.CreateBuilder(args);

BotConfiguration botConfig = ConfigurationLoader.LoadFromEnvironment();
botConfig.ApiKey = Environment.GetEnvironmentVariable("BOT_API_KEY")
    ?? throw new InvalidOperationException("BOT_API_KEY is not set.");
builder.Services.AddTelegramBotFramework(botConfig);
builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();

// 1. Authenticate: rejects requests without the configured API key (401),
//    except for the public endpoints listed above.
app.UseMiddleware<AuthenticationMiddleware>();

// 2. Authorize: runs only for requests that passed authentication.
app.UseAuthorization();

app.MapControllers();
app.Run();
```

The sample host in `src/TelegramBotFramework/Program.cs` does not register this middleware today; add the `UseMiddleware<AuthenticationMiddleware>()` line yourself when you want the protection.

### Configuring the key

The middleware reads `BotConfiguration.ApiKey`. Neither `ConfigurationLoader.LoadFromEnvironment` nor `ConfigurationLoader.LoadFromJsonFile` populates this property, so assign it in code from a secret source that is kept out of source control (for example an environment variable read by your host). If it is left unset, every non-public request is rejected.

```csharp
botConfig.ApiKey = Environment.GetEnvironmentVariable("BOT_API_KEY")
    ?? throw new InvalidOperationException("BOT_API_KEY is not set.");
```

### Calling a protected endpoint

```bash
# Preferred: Bearer token
curl -H "Authorization: Bearer $BOT_API_KEY" https://localhost:5001/api/v1/your-protected-route

# Alternative: header
curl -H "X-API-Key: $BOT_API_KEY" https://localhost:5001/api/v1/your-protected-route

# Last resort: query string (may be logged by proxies)
curl "https://localhost:5001/api/v1/your-protected-route?api_key=$BOT_API_KEY"
```

A missing or incorrect key returns `401 Unauthorized`:

```text
HTTP/1.1 401 Unauthorized

Unauthorized
```

## Notes

- **Ordering with authorization.** This middleware does not set `HttpContext.User`, so `UseAuthorization()` and `[Authorize]` attributes have no authenticated principal to evaluate. Use them only together with a scheme that populates `User`.
- **Comparison timing.** The comparison uses ordinal string equality, which is not constant-time. Treat the key as a shared secret delivered over HTTPS only.
- **Thread safety.** The middleware holds no per-request state. Its only instance fields are the logger, the next delegate, and an immutable allow-list set built in the constructor, so a single instance is safely shared across concurrent requests.
- **Scope.** The middleware protects HTTP endpoints only. Telegram updates delivered through the bot's polling or webhook service are not authenticated by it.
