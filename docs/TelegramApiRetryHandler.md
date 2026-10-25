# TelegramApiRetryHandler

Internal helper that wraps every HTTP call the framework makes to the Telegram Bot API and retries transient failures: rate limits (429), timeouts (408), server errors (500, 502, 503, 504, 505), and network or cancellation exceptions. It honors Telegram's `Retry-After` value for rate limits, uses exponential backoff with jitter for everything else, and logs each attempt with the bot token redacted.

You do not construct this class yourself. `TelegramApiClient` creates one per client instance and passes it the `TelegramApiRetryOptions` you supply.

Namespace: `TelegramBotFramework.Integration`
Visibility: `internal sealed class`

## Purpose

Telegram returns 429 when a bot exceeds its rate limits and may return 5xx or 408 during transient outages. Without retries, a single transient response fails the whole call. This handler centralizes the retry policy so that every `TelegramApiClient` method gets the same behavior.

## Retry behavior

`MaxRetryAttempts` is the number of retries after the first attempt, so a call makes at most `MaxRetryAttempts + 1` attempts.

### Rate limiting (HTTP 429)

- Retried when `RetryOnRateLimited` is `true`.
- The wait is taken from the server's `Retry-After` value, looked up in this order:
  1. The `Retry-After` response header, as a delta in seconds or as an HTTP date.
  2. The `parameters.retry_after` field in the JSON error body.
  3. A default of 5 seconds.
- If `RespectRetryAfter` is `false`, the server value is ignored and the wait is 5 seconds.
- The wait is the server value multiplied by a jitter factor (see [Delay and jitter](#delay-and-jitter)). It is not capped by `MaxDelayMilliseconds`.
- When retries are exhausted, the handler throws `TelegramRateLimitedException` with the last `Retry-After` value.
- When `RetryOnRateLimited` is `false`, the 429 response is returned to the caller unchanged.

### Server errors (HTTP 500, 502, 503, 504, 505)

- Retried when `RetryOnServerErrors` is `true`, using exponential backoff.
- When retries are exhausted, the handler throws `TelegramServerException` carrying the status code.
- When `RetryOnServerErrors` is `false`, the response is returned to the caller unchanged.

### Request timeout (HTTP 408)

- Retried when `RetryOnTimeout` is `true`, using exponential backoff.
- When retries are exhausted, the 408 response is returned to the caller. No exception is thrown.

### Network and cancellation errors

- `HttpRequestException` and `TaskCanceledException` (which includes `HttpClient` timeouts) are retried when `RetryOnNetworkErrors` is `true`.
- When retries are exhausted, the original exception is rethrown.
- Any other exception is retried while attempts remain, regardless of `RetryOnNetworkErrors`. The last one propagates when attempts run out.

### Other responses

All other status codes, including 2xx, 3xx, and other 4xx codes, are returned to the caller on the first attempt.

### Non-idempotent methods

`ExecuteWithRetryAsync` accepts an `isIdempotent` flag, which `TelegramApiClient` sets from the method name. In the current implementation the flag does not change which status codes are retried: 408, 429, and 5xx are retried for both idempotent and non-idempotent methods, and network errors are retried for both as well. The flag only affects the early-return branch for other status codes. Treat the flag as informational until that logic is revisited.

### GET requests

`ExecuteGetWithRetryAsync` retries 429, 5xx, and network or cancellation errors. It does not retry 408, so a 408 response is returned immediately.

### Delay and jitter

For 5xx, 408, and network errors, the base wait is `BaseDelayMilliseconds` on the first retry. After each retry the base doubles, up to `MaxDelayMilliseconds`. The actual wait is the base multiplied by a random factor between 0.5 and 1.5.

For 429, the wait comes from the `Retry-After` value instead of the exponential base. The doubling state still advances, so it applies to any later non-429 retries in the same call.

With the defaults (`MaxRetryAttempts = 3`, `BaseDelayMilliseconds = 1000`, `MaxDelayMilliseconds = 30000`), the nominal waits between attempts are about 1 s, 2 s, and 4 s, each scaled by the jitter factor.

### Logging

- Each attempt logs at `Debug`, with the attempt number, maximum attempts, and method name.
- Each retryable failure logs at `Warning`, with the method name and a URL whose bot token is replaced by `TokenRedaction.RedactTokenFromUrl`.
- Each wait logs at `Debug` with the delay and jitter factor.

## Public methods

The class is internal, so these members are documented for maintainers.

### `public TelegramApiRetryHandler(TelegramApiRetryOptions options, ILogger<TelegramApiClient> logger)`
Creates the handler and validates the options.

- **Parameters**
  - `options`: Retry configuration. Must not be `null`.
  - `logger`: Logger used for attempt and failure messages. Must not be `null`.
- **Exceptions**
  - `ArgumentNullException`: `options` or `logger` is `null`.
  - `ArgumentException`: `options.Validate()` rejects the values (see [Options](#options)).

---

### `public Task<HttpResponseMessage> ExecuteWithRetryAsync(HttpClient httpClient, string url, HttpContent? content, string methodName, bool isIdempotent, CancellationToken cancellationToken = default)`
Sends a `POST` with the given content and applies the retry policy above.

- **Parameters**
  - `httpClient`: The client used to send the request.
  - `url`: Full request URL, including the bot token path segment. The token is redacted in logs.
  - `content`: Request body, or `null` for an empty body.
  - `methodName`: Telegram method name, used only for logging and exception messages.
  - `isIdempotent`: Whether the method is safe to repeat. See [Non-idempotent methods](#non-idempotent-methods).
  - `cancellationToken`: Cancels the request and any pending wait.
- **Return value**
  - The response from the final attempt. It may be a non-success response that was not retried, or a 408 after retries ran out.
- **Exceptions**
  - `TelegramRateLimitedException`: 429 persisted after all retries.
  - `TelegramServerException`: A 5xx status persisted after all retries.
  - `HttpRequestException` or `TaskCanceledException`: Network failure persisted after all retries, or cancellation.

---

### `public Task<HttpResponseMessage> ExecuteGetWithRetryAsync(HttpClient httpClient, string url, string methodName, CancellationToken cancellationToken = default)`
Sends a `GET` and applies the retry policy, except for 408 handling (see [GET requests](#get-requests)).

- **Parameters**
  - `httpClient`: The client used to send the request.
  - `url`: Full request URL. The token is redacted in logs.
  - `methodName`: Telegram method name, used only for logging and exception messages.
  - `cancellationToken`: Cancels the request and any pending wait.
- **Return value**
  - The response from the final attempt.
- **Exceptions**
  - The same as `ExecuteWithRetryAsync`, minus the 408 behavior.

## Options

Retry behavior is configured with `TelegramApiRetryOptions`, a public class in the same folder. Its defaults are as follows.

| Property | Default | Effect |
|---|---|---|
| `MaxRetryAttempts` | `3` | Retries after the first attempt. Must be `>= 0`. |
| `BaseDelayMilliseconds` | `1000` | Starting backoff delay. Must be `> 0` and `<= MaxDelayMilliseconds`. |
| `MaxDelayMilliseconds` | `30000` | Upper bound for the exponential backoff. Must be `> 0`. |
| `RespectRetryAfter` | `true` | Use the server's `Retry-After` value for 429. When `false`, a 5-second wait is used. |
| `RetryOnRateLimited` | `true` | Retry 429 responses. |
| `RetryOnServerErrors` | `true` | Retry 5xx responses. |
| `RetryOnTimeout` | `true` | Retry 408 responses. Only applies to `ExecuteWithRetryAsync`. |
| `RetryOnNetworkErrors` | `true` | Retry `HttpRequestException` and `TaskCanceledException`. |
| `RetryOnServiceUnavailable` | `true` | Declared, but not read by the handler. |
| `RetryOnBadGateway` | `true` | Declared, but not read by the handler. |
| `RetryOnGatewayTimeout` | `true` | Declared, but not read by the handler. |
| `RetryOnInternalServerError` | `true` | Declared, but not read by the handler. |

`Validate()` runs in the handler constructor and throws `ArgumentException` if any rule above is violated.

The four per-status flags (`RetryOnServiceUnavailable`, `RetryOnBadGateway`, `RetryOnGatewayTimeout`, `RetryOnInternalServerError`) do not take effect. All 5xx codes are governed by `RetryOnServerErrors`.

## Usage example

Because the handler is internal, configure it through `TelegramApiClient`:

```csharp
using TelegramBotFramework.Integration;

var retryOptions = new TelegramApiRetryOptions
{
    MaxRetryAttempts = 5,
    BaseDelayMilliseconds = 500,
    MaxDelayMilliseconds = 10000,
    RespectRetryAfter = true
};

var client = new TelegramApiClient(botToken, retryOptions: retryOptions);

var result = await client.SendMessageAsync(chatId, "Deployment finished");

if (!result.Success)
{
    // Retries were exhausted or the error was not retryable.
    // TelegramApiClient converts TelegramRateLimitedException and
    // TelegramServerException into a failed ApiResult, so check the result
    // instead of catching them here.
    Console.WriteLine($"Send failed ({result.ErrorCode}): {result.Description}");
}
```

`TelegramApiClient` converts the exceptions the handler throws into a failed `ApiResult<bool>`, so callers normally check `Success` rather than catching `TelegramRateLimitedException` or `TelegramServerException`.

To disable retries entirely, set `MaxRetryAttempts = 0`. The first failure is then returned or thrown as usual.

## Related

- `TelegramApiClient`: creates the handler and calls it for every API request.
- `TelegramApiRetryOptions`: retry configuration, in the same file as the handler's dependencies.
- `TelegramRateLimitedException`, `TelegramServerException`: exceptions thrown after retries are exhausted.
- `TokenRedaction` (`Utilities/`): removes the bot token from URLs and messages before logging.
