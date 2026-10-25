# TelegramApiRetryOptions

Configuration object for the retry policy that `TelegramApiClient` applies to every call it makes to the Telegram Bot API. It controls how many times a failed call is retried, how long the backoff waits between attempts, and which kinds of failures are retried at all.

Namespace: `TelegramBotFramework.Integration`
Visibility: `public sealed class`

## Purpose

Telegram returns 429 when a bot exceeds its rate limits, and may return 408 or 5xx responses during transient outages. Network errors and timeouts also happen. `TelegramApiRetryOptions` lets you tune the retry policy for these cases instead of relying on the built-in defaults.

You pass an instance to the `TelegramApiClient` constructor. If you pass `null` (or omit the argument), the client uses `new TelegramApiRetryOptions()`, which carries the defaults listed below.

The behavior each flag produces is implemented in `TelegramApiRetryHandler`. See [TelegramApiRetryHandler](TelegramApiRetryHandler.md) for the full retry sequence, jitter, and logging.

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `MaxRetryAttempts` | `int` | `3` | Number of retries after the first attempt. A call makes at most `MaxRetryAttempts + 1` attempts. Must be `>= 0`. |
| `BaseDelayMilliseconds` | `int` | `1000` | Initial backoff delay in milliseconds for 408, 5xx, and network retries. Doubles after each retry, up to `MaxDelayMilliseconds`. Must be `> 0`. |
| `MaxDelayMilliseconds` | `int` | `30000` | Upper bound, in milliseconds, for the exponential backoff delay. Must be `> 0` and `>= BaseDelayMilliseconds`. |
| `RespectRetryAfter` | `bool` | `true` | When `true`, the wait for a 429 response comes from Telegram's `Retry-After` value. When `false`, that value is ignored and a fixed 5-second wait is used. |
| `RetryOnRateLimited` | `bool` | `true` | Retry on HTTP 429 Too Many Requests. When `false`, the 429 response is returned to the caller unchanged. |
| `RetryOnServerErrors` | `bool` | `true` | Retry on 5xx server errors. When `false`, the 5xx response is returned to the caller unchanged. This flag is the one the retry handler reads for all 5xx codes. |
| `RetryOnNetworkErrors` | `bool` | `true` | Retry on `HttpRequestException` and `TaskCanceledException`, which includes `HttpClient` timeouts. |
| `RetryOnTimeout` | `bool` | `true` | Retry on HTTP 408 Request Timeout. |
| `RetryOnServiceUnavailable` | `bool` | `true` | Declared for 503 Service Unavailable. See [Flags that are not yet read](#flags-that-are-not-yet-read). |
| `RetryOnBadGateway` | `bool` | `true` | Declared for 502 Bad Gateway. See [Flags that are not yet read](#flags-that-are-not-yet-read). |
| `RetryOnGatewayTimeout` | `bool` | `true` | Declared for 504 Gateway Timeout. See [Flags that are not yet read](#flags-that-are-not-yet-read). |
| `RetryOnInternalServerError` | `bool` | `true` | Declared for 500 Internal Server Error. See [Flags that are not yet read](#flags-that-are-not-yet-read). |

### Flags that are not yet read

`RetryOnServiceUnavailable`, `RetryOnBadGateway`, `RetryOnGatewayTimeout`, and `RetryOnInternalServerError` are public settable properties, but the current retry handler does not read them. All 5xx retries are controlled by `RetryOnServerErrors`. Setting one of these four flags to `false` has no effect today. Use `RetryOnServerErrors` to turn off retries for all server errors.

## Validation

`Validate()` checks the values and throws `ArgumentException` on the first problem it finds:

- `MaxRetryAttempts` must be zero or greater.
- `BaseDelayMilliseconds` must be positive.
- `MaxDelayMilliseconds` must be positive.
- `BaseDelayMilliseconds` must be less than or equal to `MaxDelayMilliseconds`.

`TelegramApiClient` calls `Validate()` in its constructor, so invalid values fail when the client is created, not on the first API call. `TelegramApiRetryHandler` also calls it.

Validation does not check the boolean flags.

## Usage

Use the defaults:

```csharp
var client = new TelegramApiClient(botToken);
```

Retry more often and wait longer between attempts, but do not trust the server's `Retry-After` value:

```csharp
using TelegramBotFramework.Integration;

var retryOptions = new TelegramApiRetryOptions
{
    MaxRetryAttempts = 5,
    BaseDelayMilliseconds = 500,
    MaxDelayMilliseconds = 60000,
    RespectRetryAfter = false
};

var client = new TelegramApiClient(botToken, retryOptions: retryOptions);
```

Disable retries completely, so every failure reaches the caller on the first attempt:

```csharp
var retryOptions = new TelegramApiRetryOptions
{
    MaxRetryAttempts = 0
};

var client = new TelegramApiClient(botToken, retryOptions: retryOptions);
```

Retry only rate-limit responses and network errors, and leave other failures to the caller:

```csharp
var retryOptions = new TelegramApiRetryOptions
{
    RetryOnServerErrors = false,
    RetryOnTimeout = false
};

var client = new TelegramApiClient(botToken, retryOptions: retryOptions);
```

## Exceptions

Retry options themselves throw only `ArgumentException`, from `Validate()`. The exceptions below come from the retry handler when retries run out:

- `TelegramRateLimitedException` is thrown when 429 retries are exhausted. It carries the last `Retry-After` value in seconds.
- `TelegramServerException` is thrown when 5xx retries are exhausted. It carries the HTTP status code.

When retries for 408 or network errors run out, the handler returns the 408 response or rethrows the network exception. It does not wrap it.

## Related types

- `TelegramApiClient`: takes an optional `TelegramApiRetryOptions` in its constructor.
- `TelegramApiRetryHandler`: applies the policy described by this class. See [TelegramApiRetryHandler](TelegramApiRetryHandler.md).
- `TelegramRateLimitedException` and `TelegramServerException`: thrown when retries are exhausted.
