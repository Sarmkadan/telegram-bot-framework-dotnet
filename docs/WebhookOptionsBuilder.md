# WebhookOptionsBuilder

Fluent builder for `WebhookOptions`. It sets each webhook property with a chainable method, checks the combined result in `Build()`, and returns a `WebhookOptions` instance.

Namespace: `TelegramBotFramework.Integration`
Visibility: `public sealed class`

## Purpose

`WebhookOptions` is a plain settable class. Setting its properties one by one works, but nothing checks the combination of values until `Validate()` runs. `WebhookOptionsBuilder` gives the same settings as a chain of `With*` calls, and `Build()` checks the range and format rules for the values you set before handing the object back.

Use the builder when you want to:

- Assemble webhook settings in one expression, for example from configuration.
- Copy an existing `WebhookOptions` and change a few values with `From(...)`.
- Get early failures from `Build()` instead of at startup.

The builder does not register anything with the DI container. `AddWebhookMode` takes an `Action<WebhookOptions>` and calls `Validate()` on the instance it creates. See [Integration with AddWebhookMode](#integration-with-addwebhookmode).

## Creating a builder

The constructor is private. Start from one of these two static methods:

| Method | Returns | Description |
|--------|---------|-------------|
| `Create()` | `WebhookOptionsBuilder` | Starts from a new `WebhookOptions` with its defaults. `Url` is empty, so you must call `WithUrl` before `Build()`. |
| `From(WebhookOptions template)` | `WebhookOptionsBuilder` | Copies every property of `template` into a new internal `WebhookOptions`. Throws `ArgumentNullException` if `template` is `null`. |

`From` copies `AllowedUpdates` as a clone, so changing the array you passed in later does not change the builder's copy. The template itself is not modified.

## Builder methods

Every `With*` method returns the same builder instance, so calls chain.

| Method | Parameter | Stored as | Checked when |
|--------|-----------|-----------|--------------|
| `WithUrl(string url)` | Absolute `https` or `http` URL | `Url` | Immediately: throws `ArgumentException` if null or empty. The URL format is checked in `Build()`. |
| `WithSecretToken(string? secretToken)` | Secret token, or `null` to disable | `SecretToken` | Not checked by `Build()`. |
| `WithMaxConnections(int maxConnections)` | `1`–`100` | `MaxConnections` | `Build()` |
| `WithAllowedUpdates(string[]? allowedUpdates)` | Update type names, or `null` for all types | `AllowedUpdates` (cloned) | Not checked. |
| `WithListenPath(string listenPath)` | Path starting with `/`, or empty | `ListenPath` | `Build()`, only when not empty. |
| `WithDropPendingUpdates(bool dropPendingUpdates)` | `true` or `false` | `DropPendingUpdates` | Not checked. |
| `WithQueueCapacity(int queueCapacity)` | `1`–`100000` | `QueueCapacity` | `Build()` |
| `WithQueueFullPolicy(WebhookQueueFullPolicy queueFullPolicy)` | A defined enum value | `QueueFullPolicy` | `Build()` |
| `WithQueueWorkerCount(int queueWorkerCount)` | `1`–`64` | `QueueWorkerCount` | `Build()` |

Only `WithUrl` throws when you call it. The other `With*` methods store the value as given, and `Build()` reports any invalid value. `WithUrl` also checks for null or empty input right away.

`WithAllowedUpdates` clones the array, like `From` does. An array you pass in is not kept by reference.

`WithQueueFullPolicy` takes a `WebhookQueueFullPolicy` value (`Integration/WebhookQueueFullPolicy.cs`):

- `Reject` (default): rejects the incoming update with `429 Too Many Requests`, so Telegram redelivers it.
- `DropOldest`: evicts the oldest queued update and answers `200 OK`.

## Build

```csharp
public WebhookOptions Build()
```

`Build()` runs these checks in order and throws `ArgumentException` on the first failure:

1. `Url` must not be null, empty, or whitespace.
2. `Url` must be an absolute `http` or `https` URI.
3. `MaxConnections` must be between 1 and 100.
4. `ListenPath`, if not empty, must start with `/`.
5. `UpdateDeduplicationWindow` must be positive.
6. `MaxRequestBodySize` must be positive.
7. `QueueCapacity` must be between 1 and 100000.
8. `QueueWorkerCount` must be between 1 and 64.
9. `QueueFullPolicy` must be a defined enum value.

`Build()` returns the `WebhookOptions` instance the builder holds, not a copy. Calling a `With*` method after `Build()` changes that same object. Call `Build()` again if you want a new instance from the current state, and be aware that the first result changes too.

### What Build does not check

- `SecretToken` is not checked. `WebhookOptions.Validate()` checks it against `WebhookSecretToken.IsValid`, and `AddWebhookMode` calls `Validate()`. A `WebhookOptions` returned by `Build()` with an invalid secret token will therefore fail later, when it goes through `AddWebhookMode`.
- `AllowedUpdates` and `DropPendingUpdates` are not checked.

### Exception types

`Build()` throws `ArgumentException`. `WebhookOptions.Validate()` throws `InvalidOperationException` for the same range rules. If you catch errors from both, handle both types.

## Defaults

`Create()` starts from `new WebhookOptions()`, so the defaults are:

| Property | Default |
|----------|---------|
| `Url` | `""` (must be set) |
| `SecretToken` | `null` |
| `MaxConnections` | `40` |
| `AllowedUpdates` | `null` (all types) |
| `ListenPath` | `/api/webhook/telegram` |
| `DropPendingUpdates` | `false` |
| `UpdateDeduplicationWindow` | 5 minutes |
| `MaxRequestBodySize` | 1,048,576 bytes (1 MB) |
| `QueueCapacity` | `1000` |
| `QueueFullPolicy` | `Reject` |
| `QueueWorkerCount` | `4` |

## Usage

Build a configuration with the required URL and a few overrides:

```csharp
using TelegramBotFramework.Integration;

var options = WebhookOptionsBuilder.Create()
    .WithUrl("https://mybot.example.com/api/webhook/telegram")
    .WithSecretToken(secret)
    .WithMaxConnections(20)
    .WithAllowedUpdates(new[] { "message", "callback_query" })
    .WithQueueCapacity(5000)
    .WithQueueWorkerCount(8)
    .WithQueueFullPolicy(WebhookQueueFullPolicy.DropOldest)
    .Build();
```

Start from an existing options object and change only the queue settings:

```csharp
var tuned = WebhookOptionsBuilder.From(options)
    .WithQueueWorkerCount(16)
    .Build();
```

`From` copies `options`, so `options` itself is unchanged. The built `tuned` is a separate instance.

Handle an invalid configuration:

```csharp
try
{
    var invalid = WebhookOptionsBuilder.Create()
        .WithUrl("https://mybot.example.com/api/webhook/telegram")
        .WithQueueWorkerCount(100)
        .Build();
}
catch (ArgumentException ex)
{
    // "QueueWorkerCount must be between 1 and 64."
    logger.LogError(ex, "Invalid webhook configuration");
}
```

## Integration with AddWebhookMode

`AddWebhookMode` is defined in `Configuration/DependencyInjectionSetup.cs`. It has one overload:

```csharp
public static IServiceCollection AddWebhookMode(
    this IServiceCollection services,
    Action<WebhookOptions> configure)
```

It creates a `WebhookOptions`, passes it to `configure`, then calls `Validate()`. It has no overload that accepts an existing `WebhookOptions` instance, so a built instance cannot be passed in directly. To use a built instance, copy its values inside the delegate:

```csharp
services.AddWebhookMode(opts =>
{
    opts.Url = options.Url;
    opts.SecretToken = options.SecretToken;
    opts.MaxConnections = options.MaxConnections;
    opts.AllowedUpdates = options.AllowedUpdates;
    opts.ListenPath = options.ListenPath;
    opts.DropPendingUpdates = options.DropPendingUpdates;
    opts.UpdateDeduplicationWindow = options.UpdateDeduplicationWindow;
    opts.MaxRequestBodySize = options.MaxRequestBodySize;
    opts.QueueCapacity = options.QueueCapacity;
    opts.QueueFullPolicy = options.QueueFullPolicy;
    opts.QueueWorkerCount = options.QueueWorkerCount;
});
```

Copying every property keeps the values in sync if `WebhookOptions` gains new properties later, as long as this list is updated too.

## Related types

- [`WebhookOptions`](WebhookOptions.md): the object the builder creates.
- `WebhookQueueFullPolicy` (`Integration/WebhookQueueFullPolicy.cs`): the enum used by `WithQueueFullPolicy`.
- `WebhookSecretToken`: checks the secret token during `WebhookOptions.Validate()`.
- `AddWebhookMode` (`Configuration/DependencyInjectionSetup.cs`): registers webhook options with the DI container.
