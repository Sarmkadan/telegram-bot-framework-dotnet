# HttpErrorHandlingMiddleware

Middleware component that intercepts HTTP pipeline exceptions, normalizes them into a structured error response, and ensures consistent error reporting across the ASP.NET Core application.

## API

### `public HttpErrorHandlingMiddleware`

Constructor that initializes the middleware instance. No parameters are required; the middleware uses the default dependency injection pipeline to access `ILogger<HttpErrorHandlingMiddleware>` and `IWebHostEnvironment`.

### `public async Task InvokeAsync(HttpContext context, RequestDelegate next)`

Invokes the middleware as part of the ASP.NET Core pipeline.

- **Parameters**
  - `context`: The `HttpContext` for the current HTTP request.
  - `next`: The delegate representing the next middleware in the pipeline.

- **Return Value**
  Returns a `Task` that completes when the middleware has finished processing the request or exception.

- **Exceptions**
  Throws no exceptions directly; any exceptions thrown during processing are caught and handled internally.

### `public string ErrorCode`

Gets the standardized error code associated with the current error response.

- **Remarks**
  The value is set when an exception is intercepted and mapped to a known error category (e.g., `INVALID_INPUT`, `AUTH_FAILED`, `INTERNAL_ERROR`). Returns `null` if no error has been captured.

### `public string Message`

Gets the human-readable error message associated with the current error response.

- **Remarks**
  The message is derived from the intercepted exception or mapped to a user-friendly string based on the error code. Returns `null` if no error has been captured.

### `public DateTime Timestamp`

Gets the UTC timestamp when the error was captured.

- **Remarks**
  The value is set at the moment the exception is intercepted. Returns `default(DateTime)` if no error has been captured.

### `public string Path`

Gets the request path where the error occurred.

- **Remarks**
  The value reflects the raw request path (e.g., `/api/messages/send`) at the time of the exception. Returns `null` if no error has been captured.

### `public string TraceId`

Gets the correlation identifier for tracing the request across services.

- **Remarks**
  The value is derived from `HttpContext.TraceIdentifier`. Returns `null` if no error has been captured.

## Usage

### Example 1: Basic Integration in Startup
