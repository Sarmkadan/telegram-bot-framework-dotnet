# ErrorHandlingMiddleware

`HttpErrorHandlingMiddleware` is an ASP.NET Core middleware component that catches exceptions thrown by the rest of the HTTP pipeline and converts them into a consistent JSON error response. Each exception type is mapped to an HTTP status code and a stable error code, so API clients get the same response shape regardless of where the failure happened.

The middleware lives in `src/TelegramBotFramework/Middleware/ErrorHandlingMiddleware.cs` (the file name differs from the class name; the public type is `HttpErrorHandlingMiddleware`). It is an HTTP-level component: it wraps `HttpContext` and is not part of the Telegram update pipeline (`IBotMiddleware`).

Namespace: `TelegramBotFramework.Middleware`
Source: `src/TelegramBotFramework/Middleware/ErrorHandlingMiddleware.cs`

## Purpose

- Catch unhandled exceptions from downstream middleware and controllers so they never reach the ASP.NET Core default exception page.
- Return a `application/json` body of type `HttpErrorResponse` with `ErrorCode`, `Message`, `Timestamp`, `Path`, and `TraceId`.
- Log the request lifecycle (start, finish, failure) with the request path and trace identifier.

The middleware does not catch `OperationCanceledException`. Cancellations are rethrown so that cancelled requests behave as the host expects.

## API

### `public sealed class HttpErrorHandlingMiddleware : IHttpErrorHandlingMiddleware`

The middleware class. It is `sealed` and implements `IHttpErrorHandlingMiddleware`.

### `public HttpErrorHandlingMiddleware(RequestDelegate next, ILogger<HttpErrorHandlingMiddleware> logger)`

Creates the middleware.

- **Parameters**
  - `next`: The next delegate in the ASP.NET Core pipeline. Supplied by the framework.
  - `logger`: Logger used for request logging and failure reporting. Resolved from dependency injection.
- **Exceptions**
  - None thrown by the constructor itself. Passing `null` is not guarded and will fail later, on the first request.

### `public async Task InvokeAsync(HttpContext context)`

Runs the middleware for the current request. It calls the next delegate and, if an exception escapes it, writes the error response.

- **Parameters**
  - `context`: The `HttpContext` for the current request.
- **Return Value**
  A `Task` that completes when the request has either finished normally or the error response has been written.
- **Behavior**
  - On success, logs that processing finished and returns. The response is untouched.
  - On an exception other than `OperationCanceledException`, logs the failure, calls the private error path, and logs the final status code. The exception is not rethrown.
  - `OperationCanceledException` is not caught and propagates to the caller.

### Public properties

`HttpErrorHandlingMiddleware` also exposes `ErrorCode`, `Message`, `Timestamp`, `Path`, and `TraceId` as get/set properties (required by `IHttpErrorHandlingMiddleware`). They default to `string.Empty` and `default(DateTime)`. **`InvokeAsync` does not write to them.** The values sent to the client are built in a local `HttpErrorResponse`, so these properties always keep their default values during normal operation. Do not read them to get the current error.

`ToString()` returns a summary of those properties in the form `HttpErrorHandlingMiddleware { ... }`.

### `public sealed class HttpErrorResponse`

The JSON body written for failed requests.

| Property | Type | Value |
|----------|------|-------|
| `ErrorCode` | `string` | Stable error code (see the mapping table below). |
| `Message` | `string` | Human-readable message. |
| `Timestamp` | `DateTime` | UTC time when the error response was created. |
| `Path` | `string` | Request path (`HttpContext.Request.Path`). |
| `TraceId` | `string` | `HttpContext.TraceIdentifier`. |

## Behavior

### Exception mapping

The mapping is evaluated in the order listed. `ArgumentNullException` is matched before its base type `ArgumentException`.

| Exception | HTTP status | `ErrorCode` | `Message` |
|-----------|-------------|-------------|-----------|
| `ArgumentNullException` | 400 | `INVALID_ARGUMENT` | `Required argument is null` |
| `ArgumentException` | 400 | `INVALID_ARGUMENT` | The exception's `Message` |
| `InvalidOperationException` | 409 | `INVALID_STATE` | The exception's `Message` |
| `TimeoutException` | 408 | `REQUEST_TIMEOUT` | `Request processing timed out` |
| `NotImplementedException` | 501 | `NOT_IMPLEMENTED` | `This feature is not yet implemented` |
| `BotFrameworkException` | 500 | The exception's `ErrorCode`, or `BOT_FRAMEWORK_ERROR` if it is `null` | The exception's `Message` |
| Any other exception | 500 | `INTERNAL_ERROR` | `An unexpected error occurred. Please try again later.` |

Exception messages are returned to the client for the `ArgumentException`, `InvalidOperationException`, and `BotFrameworkException` cases. Avoid putting secrets or internal details into those messages.

### Response format

For a handled exception the middleware:

1. Sets `Content-Type` to `application/json`.
2. Sets the response status code from the mapping table.
3. Writes an `HttpErrorResponse` with `WriteAsJsonAsync`.

Example body for a `TimeoutException` on `/api/v1/orders` with trace `0HN7...`:

```json
{
  "errorCode": "REQUEST_TIMEOUT",
  "message": "Request processing timed out",
  "timestamp": "2026-10-25T10:15:30.1234567Z",
  "path": "/api/v1/orders",
  "traceId": "0HN7..."
}
```

(JSON property names follow the serializer's default camelCase convention.)

### Logging

The middleware logs through the injected `ILogger<HttpErrorHandlingMiddleware>`:

- An information entry when processing starts and when it finishes successfully.
- A warning and an error entry, including the exception, when an exception is caught.
- An information entry with the final status code after the error response is written.

Logs include the request path and `TraceIdentifier` as structured properties.

## Usage

### Registering in the pipeline

The middleware is not registered automatically by `AddTelegramBotFramework`. Add it to the ASP.NET Core pipeline yourself, early in the request pipeline, so that it wraps the endpoints and controllers:

```csharp
using TelegramBotFramework.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

app.UseMiddleware<HttpErrorHandlingMiddleware>();

app.MapControllers();

app.Run();
```

Place it before any middleware whose exceptions you want converted to JSON. Middleware that runs before it (for example, a component that is outside its `try` block) will not have its exceptions handled by this component.

### Example: handling a failure from a controller

With the registration above, an exception thrown in an action is converted to a response instead of a 500 error page:

```csharp
[ApiController]
[Route("api/v1/orders")]
public sealed class OrdersController : ControllerBase
{
    [HttpPost]
    public IActionResult Create(CreateOrderRequest request)
    {
        if (request.Quantity <= 0)
        {
            // Mapped to 400 INVALID_ARGUMENT with this message
            throw new ArgumentException("Quantity must be greater than zero.");
        }

        return Ok();
    }
}
```

A request with a non-positive quantity receives `400 Bad Request` with `errorCode` `INVALID_ARGUMENT` and `message` `Quantity must be greater than zero.`

### Example: throwing a framework exception

`BotFrameworkException` subclasses carry an optional error code that is passed through to the client:

```csharp
throw new BotFrameworkException("Session has expired.", "SESSION_EXPIRED");
```

Such an exception results in a `500` response with `errorCode` `SESSION_EXPIRED`. If the error code is omitted, `BOT_FRAMEWORK_ERROR` is used.

## Related

- `docs/HttpErrorHandlingMiddleware.md` - reference page for the same type, which may not match the current source.
- `docs/BotFrameworkException.md` - the base exception type used by the `BotFrameworkException` mapping.
- `docs/AuthenticationMiddleware.md` and `docs/AuthorizationMiddleware.md` - other HTTP middleware in the same namespace.
