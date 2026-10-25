# BotPipelineMiddleware

`src/TelegramBotFramework/Middleware/BotPipelineMiddleware.cs` holds the pieces that compose the bot middleware pipeline:

- `MiddlewarePipelineBuilder`: a fluent builder that collects middleware types. It is used by the DI setup to register the default pipeline.
- `BotErrorHandlingMiddleware`: converts `BotFrameworkException` into an error on the execution context.
- `BotLoggingMiddleware`: logs the start and end of each request.

The file does not define a class named `BotPipelineMiddleware`. The name refers to the file as a whole. `BotLoggingMiddleware` is also documented in [BotLoggingMiddleware.md](BotLoggingMiddleware.md). `ErrorHandlingMiddleware.cs` is a different, HTTP-level component (`HttpErrorHandlingMiddleware`) and is not part of this pipeline.

## Purpose

`BotOrchestrator` runs every registered `IBotMiddleware` over an `ExecutionContext`. Each middleware receives the context and a `next` delegate. It can do work before calling `next`, after it, or both, and it can stop the chain by not calling `next`.

`MiddlewarePipelineBuilder` gives the registration step a single, readable list of middleware types. It does not construct or run anything. It returns a `Type[]`, and the caller decides how to register the types.

## Public API

### MiddlewarePipelineBuilder

```csharp
public sealed class MiddlewarePipelineBuilder
```

#### Use<TMiddleware>

```csharp
public MiddlewarePipelineBuilder Use<TMiddleware>() where TMiddleware : IBotMiddleware
```

Appends `typeof(TMiddleware)` to the internal list and returns the same builder, so calls can be chained. The type is not resolved or instantiated here.

#### Build

```csharp
public Type[] Build()
```

Returns a copy of the collected types, in the order `Use` was called. Calling `Build` does not clear the builder. Later `Use` calls are not reflected in an array that was already returned.

### BotErrorHandlingMiddleware

```csharp
public sealed class BotErrorHandlingMiddleware : IBotMiddleware
public BotErrorHandlingMiddleware(ILogger<BotErrorHandlingMiddleware> logger)
public int Priority => 10;
```

`ProcessAsync` calls `next` inside a `try` block. If `next` throws `BotFrameworkException`, the middleware:

1. calls `context.AddError(ex.Message)`,
2. logs the exception at `Error` level with the user ID, context ID, and `ex.ErrorCode` (or `BOT_FRAMEWORK_ERROR` when the code is null),
3. returns `context` as a normal result, so the caller does not see the exception.

Any other exception type is not caught and propagates to the caller.

### BotLoggingMiddleware

```csharp
public sealed class BotLoggingMiddleware : IBotMiddleware, IBotLoggingMiddleware
public BotLoggingMiddleware(ILogger<BotLoggingMiddleware> logger)
public int Priority => 100;
```

`ProcessAsync` logs an `Information` entry with the user ID, command name, and context ID, then calls `next`. After `next` returns, it logs a second `Information` entry with the result's user ID, the same command name, context ID, and `IsValid`. The command name is taken from `context.Command?.Name`, then `context.Message?.CommandName`, and falls back to `<none>`. The middleware does not catch exceptions, so if `next` throws, the second entry is not written.

The constructor throws `ArgumentNullException` when `logger` is null.

## Execution order

`BotOrchestrator` sorts middleware by `Priority` in descending order when it is constructed, using `OrderByDescending`. Higher values run earlier. The sort is stable, so middleware with equal priority keep their registration order.

**The order of `Use` calls does not determine execution order.** It determines only the registration order in the container. For the two classes in this file:

| Middleware | Priority | Position in chain |
|---|---|---|
| `BotLoggingMiddleware` | 100 | Runs first (outermost) |
| `BotErrorHandlingMiddleware` | 10 | Runs inside logging |

Because logging is outside error handling, the error handler converts the exception and returns normally. Logging then records the completed request with `IsValid` set to `false` when an error was added.

## Usage

### Composing the pipeline

Register the middleware types with the builder, then register each type as `IBotMiddleware`. This is what `DependencyInjectionSetup` does for the default pipeline:

```csharp
using TelegramBotFramework.Middleware;

var pipeline = new MiddlewarePipelineBuilder()
    .Use<BotErrorHandlingMiddleware>()
    .Use<BotLoggingMiddleware>()
    .Use<AuthorizationMiddleware>()
    .Use<RateLimitingMiddleware>()
    .Build();

foreach (var middlewareType in pipeline)
{
    services.AddTransient(typeof(IBotMiddleware), middlewareType);
}
```

The default setup registers the types as transient, so each `IBotMiddleware` instance is created by the container. `BotOrchestrator` then receives the full set and orders it by `Priority`. The registration order in the code above is not the runtime order.

### Adding a custom middleware

Implement `IBotMiddleware`, then add it to the builder. The priority determines where it runs.

```csharp
public sealed class TimingMiddleware : IBotMiddleware
{
    public int Priority => 50;

    public async Task<Models.ExecutionContext> ProcessAsync(
        Models.ExecutionContext context,
        Func<Models.ExecutionContext, Task<Models.ExecutionContext>> next,
        CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        var result = await next(context).ConfigureAwait(false);
        Console.WriteLine($"Handled in {DateTimeOffset.UtcNow - started}");
        return result;
    }
}

var pipeline = new MiddlewarePipelineBuilder()
    .Use<BotErrorHandlingMiddleware>()
    .Use<BotLoggingMiddleware>()
    .Use<TimingMiddleware>()
    .Build();
```

With a priority of 50, `TimingMiddleware` runs after `BotLoggingMiddleware` (100) and before `BotErrorHandlingMiddleware` (10). Its timing code therefore runs inside the logging entry and outside the error handler.

## Notes

- `MiddlewarePipelineBuilder` is a plain collector. Errors in `Use` (for example, a type that does not implement `IBotMiddleware`) are rejected at compile time by the generic constraint. `Build` does not validate that the types can be constructed.
- Both middleware classes take `ILogger<T>` in the constructor. They are registered through the container, so the container must supply the logger.
- `RateLimitingMiddleware` and `AuthorizationMiddleware` are documented in their own files.
