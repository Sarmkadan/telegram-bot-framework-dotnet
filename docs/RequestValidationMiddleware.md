# RequestValidationMiddleware

`RequestValidationMiddleware` is an ASP.NET Core middleware component that performs cheap, early checks on the bodies of incoming `POST`, `PUT`, and `PATCH` requests before they reach a controller. It rejects requests that have no body, use an unsupported `Content-Type`, exceed a 5 MB size limit, or contain malformed JSON, and answers them with a `4xx` status code without invoking the rest of the pipeline.

The middleware validates only the transport-level shape of the body: presence, media type, size, and JSON syntax. It does not check the body against a schema, bind it to a model, or inspect individual fields. Schema-level validation remains the responsibility of the controller or handler.

Namespace: `TelegramBotFramework.Middleware`
Source: `src/TelegramBotFramework/Middleware/RequestValidationMiddleware.cs`

## API

### `public RequestValidationMiddleware(RequestDelegate next, ILogger<RequestValidationMiddleware> logger)`

Creates the middleware instance. ASP.NET Core calls this constructor when the middleware is added to the pipeline.

- **Parameters**
  - `next`: The next delegate in the ASP.NET Core pipeline. Called for every request that passes validation, and for every request that is not `POST`, `PUT`, or `PATCH`.
  - `logger`: Logger used to emit a debug message for each request that passes validation.
- **Exceptions**
  - None thrown by the constructor itself. The constructor assigns its arguments without null checks, so pass the pipeline-provided values.

### `public async Task InvokeAsync(HttpContext context)`

Runs the validation for the current request. ASP.NET Core invokes this method once per request.

- **Parameters**
  - `context`: The `HttpContext` for the current request. The method reads `context.Request` and, on failure, writes to `context.Response`.
- **Return Value**
  A `Task` that completes when the request has either been passed to the next middleware or answered with an error status code.
- **Exceptions**
  - Exceptions raised while reading the request body, or by downstream middleware, propagate to the caller. The middleware does not catch them.

## Behavior

### Methods that are not validated

Only `POST`, `PUT`, and `PATCH` are checked. The comparison is an exact string match against the upper-case method names, so a lower-case method such as `post` is not validated. Every other method, including `GET`, `DELETE`, and `HEAD`, is passed straight to the next middleware.

### Validation steps

For a `POST`, `PUT`, or `PATCH` request the checks run in this order. The first failing check ends the request.

| Step | Condition | Response | Message body |
|------|-----------|----------|--------------|
| 1. Body presence | `Content-Length` is missing or `0` | `400 Bad Request` | `Request body is required` |
| 2. Media type | `Content-Type` does not contain `application/json` or `application/x-www-form-urlencoded` | `415 Unsupported Media Type` | `Content-Type must be application/json or application/x-www-form-urlencoded` |
| 3. Size | `Content-Length` is greater than 5 MB (5 × 1024 × 1024 bytes) | `413 Payload Too Large` | `Request body exceeds maximum allowed size` |
| 4. JSON syntax | Body is not empty or whitespace and the media type is `application/json`, but the body does not parse as JSON | `400 Bad Request` | `Invalid JSON in request body` |

Notes on each step:

- **Body presence.** The check relies on the `Content-Length` header. A request sent with `Transfer-Encoding: chunked` and no `Content-Length` is rejected by step 1 even if it carries a body. A `POST` that legitimately has no body, such as an action endpoint that takes everything from the route, is also rejected; send an explicit body or exclude the route from the pipeline.
- **Media type.** The header value is lower-cased and then matched with `Contains`, so parameters such as `; charset=utf-8` are accepted. Because the match is a substring match, a type such as `application/json-patch+json` also passes. `multipart/form-data` is rejected.
- **Size.** Only the declared `Content-Length` is checked. The middleware does not count bytes as they are read, so a client that under-declares its length is not caught by this step.
- **JSON syntax.** The body is parsed with `System.Text.Json.JsonDocument.Parse`, which accepts a single JSON value. Form-encoded bodies are not parsed. A body that consists only of whitespace is accepted for `application/json` requests.

### Body buffering

When the request passes step 1 and step 2, the middleware calls `HttpRequest.EnableBuffering()` and reads the body into a string to check it. After validation it sets the body stream position back to `0`, so the controller or model binder can read the body from the beginning. Bodies larger than the in-memory buffer threshold are spilled to a temporary file by ASP.NET Core; this is handled by the framework.

### Accepted requests

When all checks pass, the middleware writes a debug log entry with the message `Request validation passed for {Method} {Path}` and calls the next middleware. The response status is whatever the downstream pipeline produces.

### Rejected requests

When a check fails, the middleware sets the response status code, writes the message from the table above as plain text, and returns without calling the next middleware. It does not log a warning or error.

## Usage

### Registering in the pipeline

The framework does not register this middleware in `AddTelegramBotFramework`, and the sample host in `src/TelegramBotFramework/Program.cs` does not use it. Add it yourself with `UseMiddleware`. Place it before the middleware and endpoints that consume request bodies, and after any middleware that must see every request, such as exception handling.

```csharp
using TelegramBotFramework.Configuration;
using TelegramBotFramework.Middleware;
using TelegramBotFramework.Models;

var builder = WebApplication.CreateBuilder(args);

BotConfiguration botConfig = ConfigurationLoader.LoadFromEnvironment();
builder.Services.AddTelegramBotFramework(botConfig);
builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();

// Reject malformed or unsupported request bodies before they reach controllers.
app.UseMiddleware<RequestValidationMiddleware>();

app.MapControllers();
app.Run();
```

### Calling a protected endpoint

```bash
# Valid: JSON body with the correct Content-Type
curl -X POST https://localhost:5001/api/v1/your-route \
     -H "Content-Type: application/json" \
     -d '{"chatId": 42, "text": "hello"}'

# Rejected with 415: wrong media type
curl -X POST https://localhost:5001/api/v1/your-route \
     -H "Content-Type: text/plain" \
     -d 'hello'

# Rejected with 400: malformed JSON
curl -X POST https://localhost:5001/api/v1/your-route \
     -H "Content-Type: application/json" \
     -d '{"chatId": 42,'
```

Responses for the rejected requests:

```text
HTTP/1.1 415 Unsupported Media Type

Content-Type must be application/json or application/x-www-form-urlencoded
```

```text
HTTP/1.1 400 Bad Request

Invalid JSON in request body
```

## Notes

- **Plain-text error bodies.** Errors are written as plain text rather than ProblemDetails JSON. Clients that expect structured errors should map the status codes instead of parsing the body.
- **Ordering with other middleware.** Middleware registered before this one runs first, so place `AuthenticationMiddleware` ahead of it when unauthenticated callers should receive `401` rather than a validation error.
- **Thread safety.** The middleware holds no per-request state. Its only instance fields are the next delegate and the logger, so one instance is shared safely across concurrent requests.
- **Scope.** The middleware checks HTTP requests only. Telegram updates delivered through the polling or webhook services do not pass through it.
