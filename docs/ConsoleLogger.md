# ConsoleLogger

`ConsoleLogger<T>` is an internal implementation of `Microsoft.Extensions.Logging.ILogger<T>` that writes every log entry to the console with `Console.WriteLine`. It is a fallback for components that accept an optional `ILogger<T>` and are constructed without one, so the framework still produces output when no logging infrastructure is configured. The class is declared in `src/TelegramBotFramework/Events/ConsoleLogger.cs` together with the internal helper `NullDisposable`.

> **Note:** `ConsoleLogger<T>` is `internal sealed`. It is not part of the public API of the NuGet package. Code outside the assembly cannot construct it. Use `Microsoft.Extensions.Logging` providers (for example, `AddConsole()`) when you need logging in an application.

## Purpose

Several framework types use the following pattern in their constructors:

```csharp
_logger = logger ?? new ConsoleLogger<EventBus>();
```

The types that follow this pattern include `EventBus`, `InMemoryEventBus`, `EventPublisher`, `EventHandlerBase<TEvent>`, `AuthorizationMiddleware`, `MetricsMiddleware`, `WebhookHandler`, `PollingStrategy`, `ExternalApiIntegration`, `TelegramCircuitBreaker`, `BackgroundTaskWorker`, `ScheduledTaskManager`, `ScheduledMessageService`, `BroadcastService`, and `DistributedCacheProvider`. Each of them accepts a logger but works without one.

The generic parameter `T` identifies the owning type. The class stores no state from `T`, so it only affects the static type of the instance.

## API

### Constructor

```csharp
// Implicit default constructor. No parameters.
new ConsoleLogger<T>()
```

Creates a logger. It holds no resources and does not throw.

### IsEnabled

```csharp
public bool IsEnabled(LogLevel logLevel)
```

Always returns `true`. Every level is enabled, including `Trace` and `Debug`, so the logger writes all messages regardless of severity. Callers that check `IsEnabled` before building an expensive message will always build it.

### Log

```csharp
public void Log<TState>(
    LogLevel logLevel,
    EventId eventId,
    TState state,
    Exception? exception,
    Func<TState, Exception?, string> formatter)
```

Writes one line to standard output in this format:

```text
[{logLevel}] {formatter(state, exception)}
```

For example, `[Warning] Retry 3 of 5`. The following behavior follows from the implementation:

- `logLevel` is printed using the `LogLevel` enum name (`Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`, `None`).
- `eventId` is ignored and does not appear in the output.
- `exception` is passed only to `formatter`. The logger does not print the exception on its own. If the formatter does not include it, the message and stack trace are lost. The default formatter used by the `LogError(exception, message, ...)` extension methods ignores the exception, so the stack trace is not written.
- The output goes to `Console.Out`. Errors and warnings are not sent to `Console.Error`.

### BeginScope (explicit interface implementation)

```csharp
IDisposable? ILogger.BeginScope<TState>(TState state)
```

Implemented explicitly on `ILogger`, so it is reachable through an `ILogger<T>` reference. It always returns a `NullDisposable`, so scope data is discarded and nothing is written for the scope.

### NullDisposable

```csharp
internal sealed class NullDisposable : IDisposable
{
    public void Dispose() { }
}
```

A no-op `IDisposable` returned by `BeginScope`. It is internal to the `TelegramBotFramework.Events` namespace.

## Behavior summary

| Aspect | Behavior |
|--------|----------|
| Visibility | `internal sealed` |
| Levels | All enabled (`IsEnabled` returns `true`) |
| Output | `Console.WriteLine`, one line per entry, prefixed with `[LogLevel]` |
| Event ID | Not written |
| Exception | Not written separately; only what `formatter` returns is printed |
| Scopes | Not supported; `BeginScope` returns a no-op disposable |
| Thread safety | Each call makes one `Console.WriteLine` call; the class has no mutable state |

## Related types

- `TelegramApiClient.cs` (namespace `TelegramBotFramework.Integration`) contains a separate `internal sealed class ConsoleLogger<T>` with the same behavior. It is a different type in a different namespace. Code in `TelegramBotFramework.Events` resolves to the class described here, and code in `TelegramBotFramework.Integration` resolves to the copy in `TelegramApiClient.cs`.
- Tests that exercise the "no logger supplied" constructor path are in `tests/telegram-bot-framework-dotnet.Tests/` (for example, `EventPublisherTests.cs` and `PollingStrategyTests.cs`).

## Usage

`ConsoleLogger<T>` is internal, so the examples apply inside the `TelegramBotFramework` assembly. Test projects can use it through `InternalsVisibleTo`.

Direct use in framework code:

```csharp
using Microsoft.Extensions.Logging;
using TelegramBotFramework.Events;

ILogger<EventBus> logger = new ConsoleLogger<EventBus>();

logger.LogInformation("Subscribed to {EventType}", typeof(OrderPlaced).Name);
// Output: [Information] Subscribed to OrderPlaced

logger.LogWarning("Retry {Attempt} of {MaxAttempts}", 3, 5);
// Output: [Warning] Retry 3 of 5
```

Fallback in a constructor, the same pattern the framework uses:

```csharp
internal sealed class ExampleService
{
    private readonly ILogger<ExampleService> _logger;

    public ExampleService(ILogger<ExampleService>? logger = null)
    {
        _logger = logger ?? new ConsoleLogger<ExampleService>();
    }
}
```

When an exception must appear in the output, include it in the formatter. The `LogError` extension methods do not do this, so write the formatter explicitly through `Log`:

```csharp
logger.Log(
    LogLevel.Error,
    new EventId(0),
    "payment failed",
    exception,
    (state, ex) => $"{state}: {ex}");
// Output: [Error] payment failed: System.InvalidOperationException: ...
```
