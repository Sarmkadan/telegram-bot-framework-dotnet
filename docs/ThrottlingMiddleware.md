# ThrottlingMiddleware

`ThrottlingMiddleware` is a bot pipeline middleware (`IBotMiddleware`) that limits how many messages each Telegram user can send within a sliding time window. When a user goes over the limit, the middleware does not call the rest of the pipeline; it sets a response message on the execution context and stops processing.

It keeps its state in memory, per user ID, so limits apply only within a single process.

Namespace: `TelegramBotFramework.Middleware`
Source: `src/TelegramBotFramework/Middleware/ThrottlingMiddleware.cs`

## Purpose

Use this middleware to protect the bot from a single user flooding it with messages. It is independent of `RateLimitingMiddleware` and of the `IRateLimitingStrategy` services; it has its own counters and its own limit settings.

The middleware is **not** part of the default pipeline built by `AddTelegramBotFramework`. You must register it yourself (see [Registering](#registering)).

## API

### `public ThrottlingMiddleware(ILogger<ThrottlingMiddleware> logger, int maxMessages = 20, int windowSizeSeconds = 60)`

Creates the middleware.

- **Parameters**
  - `logger`: Logger used to record throttled users and invalid user IDs. Must not be `null`.
  - `maxMessages`: Maximum number of messages a user may send in one window. Values less than or equal to `0` are replaced with `20`. Default: `20`.
  - `windowSizeSeconds`: Length of the sliding window in seconds. Values less than or equal to `0` are replaced with `60`. Default: `60`.
- **Exceptions**
  - `ArgumentNullException`: `logger` is `null`.

Invalid numeric arguments are not rejected. They silently fall back to the defaults, so a configuration typo such as `0` produces a 20-message, 60-second limit.

### `public int Priority { get; }`

Always returns `30`. The bot orchestrator runs middleware in descending `Priority` order, so `ThrottlingMiddleware` runs after any middleware with a higher value and before those with a lower value. The source comment describes this slot as coming after rate limiting.

### `public async Task<Models.ExecutionContext> ProcessAsync(Models.ExecutionContext context, Func<Models.ExecutionContext, Task<Models.ExecutionContext>> next, CancellationToken cancellationToken = default)`

Checks the sender's message count and either forwards the context to `next` or stops the pipeline.

- **Parameters**
  - `context`: The execution context for the incoming update. `UserId` identifies the sender.
  - `next`: The delegate that runs the remaining middleware.
  - `cancellationToken`: Accepted for the `IBotMiddleware` contract. The middleware does not use it.
- **Return Value**
  A `Task<Models.ExecutionContext>` that resolves to the context after processing.
- **Exceptions**
  - None are thrown by the middleware itself for normal input. Exceptions from `next` propagate to the caller.

## Behavior

`ProcessAsync` evaluates the following rules in order:

1. **Already stopped or invalid context.** If `context.IsValid` is `false` or `context.IsStopped` is `true`, the context is returned immediately. `next` is not called, and no count is recorded.
2. **Invalid user ID.** If `context.UserId` is `0` or negative, the middleware adds the error `UserId must be positive for throttling.`, logs a warning, and **still calls `next`**. No throttling is applied to that update. Because `AddError` sets `IsValid` to `false`, downstream middleware sees an invalid context.
3. **Limit exceeded.** If the user already has `maxMessages` timestamps inside the window, the middleware:
   - adds the error `Throttling limit exceeded for user {UserId}. Max {maxMessages} messages per {windowSeconds} seconds.`;
   - calls `RespondAndStop` with the same text, so the presentation layer sends it to the user;
   - logs a warning;
   - returns without calling `next`.
   The rejected message is **not** recorded, so it does not extend the user's window.
4. **Within limit.** The current UTC time is recorded and `next` is called.

### Sliding window

For each user, the middleware keeps a list of UTC timestamps of accepted messages. On each check it first removes every timestamp older than `now - windowSize`, then compares the remaining count with `maxMessages`. A user is allowed again as soon as enough old timestamps have aged out of the window. There is no fixed reset time.

### First message from a user

The first message a user sends in the lifetime of the process is allowed, but its timestamp is **not** added to that user's list. Subsequent messages are counted normally. In practice a user can get `maxMessages + 1` messages through in their first window. Subsequent windows behave as documented above.

### Thread safety

The middleware is safe to call from concurrent updates. Access to the per-user lists is guarded by a `ReaderWriterLockSlim` taken in upgradeable-read mode, so checks are serialized. Counters live in each instance, so every instance enforces its own limit. Share one instance (see [Registering](#registering)) if you need a single limit for the whole bot.

## Registering

The middleware has no `Add*` extension method. Register it with the DI container alongside the default pipeline. Its `Priority` determines where it runs.

Default limits (20 messages per 60 seconds):

```csharp
using TelegramBotFramework.Middleware;

builder.Services.AddTransient<IBotMiddleware, ThrottlingMiddleware>();
```

Custom limits (5 messages per 10 seconds) need a factory, because the constructor takes plain `int` parameters:

```csharp
using Microsoft.Extensions.Logging;
using TelegramBotFramework.Middleware;

builder.Services.AddSingleton<IBotMiddleware>(sp => new ThrottlingMiddleware(
    sp.GetRequiredService<ILogger<ThrottlingMiddleware>>(),
    maxMessages: 5,
    windowSizeSeconds: 10));
```

Registering a single shared instance (`AddSingleton`) keeps one set of counters for the whole bot. Use `AddTransient` only if you accept that each middleware instance counts separately.

## Usage

The following example shows a flood-protection setup with a custom limit and the middleware's response when a user exceeds it:

```csharp
using Microsoft.Extensions.Logging;
using TelegramBotFramework.Middleware;

// Allow at most 10 messages per user every 30 seconds.
builder.Services.AddSingleton<IBotMiddleware>(sp => new ThrottlingMiddleware(
    sp.GetRequiredService<ILogger<ThrottlingMiddleware>>(),
    maxMessages: 10,
    windowSizeSeconds: 30));
```

When user `42` sends an eleventh message within 30 seconds, the middleware responds with:

```text
Throttling limit exceeded for user 42. Max 10 messages per 30 seconds.
```

and the command handlers and later middleware do not run for that update. Because the user's first message is not recorded (see [First message from a user](#first-message-from-a-user)), a user who sends eleven messages in a row gets eleven through and the twelfth is the first one blocked.

To call the middleware directly, for example in a unit test:

```csharp
var middleware = new ThrottlingMiddleware(logger, maxMessages: 2, windowSizeSeconds: 60);
var context = new Models.ExecutionContext { UserId = 1 };

Task<Models.ExecutionContext> Next(Models.ExecutionContext ctx) => Task.FromResult(ctx);

await middleware.ProcessAsync(context, Next);   // allowed (not recorded)
await middleware.ProcessAsync(context, Next);   // allowed (recorded)
await middleware.ProcessAsync(context, Next);   // allowed (recorded, count reaches 2)
var blocked = await middleware.ProcessAsync(context, Next); // blocked: IsStopped == true
```

## Notes

- **Not in the default pipeline.** `DependencyInjectionSetup` registers only `BotErrorHandlingMiddleware`, `BotLoggingMiddleware`, `AuthorizationMiddleware`, and `RateLimitingMiddleware`. Nothing else applies throttling until you register this middleware.
- **In-memory only.** Counters are lost on restart and are not shared between instances of the bot. A multi-instance deployment gets a separate limit per instance.
- **No eviction.** Each user ID that has sent a message keeps an entry in the dictionary for the life of the process. Old timestamps are trimmed on each check, but the entries themselves are never removed. For bots with a very large number of distinct users, memory grows with that count.
- **Cancellation is not observed.** The `cancellationToken` argument is neither checked nor passed to `next`, so this middleware never cancels work on its own.
- **Unused lock lifetime.** The `ReaderWriterLockSlim` is never disposed. The class is not `IDisposable`, so it lives as long as the middleware instance.
- **Invalid user IDs are not throttled.** Updates whose `UserId` is `0` or negative bypass the limit, but they are still logged and marked invalid.
