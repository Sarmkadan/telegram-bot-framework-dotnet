# BotConfigurationBuilder

Fluent builder for `BotConfiguration`. It sets the core bot settings with chainable `With*` methods, checks the bot token in `Build()`, and returns a `BotConfiguration` instance.

Namespace: `TelegramBotFramework.Models`
Visibility: `public sealed class`

## Purpose

`BotConfiguration` is a plain settable class with defaults for most properties. Setting its properties one by one works, but nothing checks the values together. `BotConfigurationBuilder` gives the core settings as a chain of `With*` calls, so a configuration can be assembled in one expression, for example from environment variables or `appsettings.json`.

Use the builder when you want to:

- Assemble the core bot settings in one expression.
- Copy an existing `BotConfiguration` and change a few values with `From(...)`.
- Get an early failure from `Build()` when the bot token is missing.

The builder does not register anything with the DI container. Pass the built `BotConfiguration` to the framework the same way you would pass any other `BotConfiguration` instance.

## Creating a builder

The class has no explicit constructor, so `new BotConfigurationBuilder()` is available. The builder starts from a new `BotConfiguration` with its defaults.

| Method | Returns | Description |
|--------|---------|-------------|
| `new BotConfigurationBuilder()` | `BotConfigurationBuilder` | Starts from the `BotConfiguration` defaults. `BotToken` is empty, so you must call `WithBotToken` before `Build()`. |
| `From(BotConfiguration template)` | `BotConfigurationBuilder` | Copies the template's values into a new internal `BotConfiguration`. Throws `ArgumentNullException` if `template` is `null`. |

`From` copies these ten properties only:

- `BotToken`, `BotUsername`, `OwnerId`, `DatabaseConnectionString`
- `SessionTimeoutMinutes`, `MessageProcessingTimeoutSeconds`
- `EnableLogging`, `LogLevel`, `MaxConcurrentRequests`, `EnableWebhook`

The other `BotConfiguration` properties (`ApiKey`, `WebhookUrl`, `WebhookSecret`, `CustomSettings`, `AdminIds`, `EnableRateLimiting`, `RateLimitPerMinute`, `LocalizationLanguage`) are not copied, and the builder has no method for them. Set them on the result of `Build()` instead. The template itself is not modified.

## Builder methods

Every `With*` method returns the same builder instance, so calls chain.

| Method | Parameter | Stored as | Checked when |
|--------|-----------|-----------|--------------|
| `WithBotToken(string token)` | Bot token | `BotToken` | Immediately: `ArgumentNullException` if `null`, `ArgumentException` if empty. |
| `WithBotUsername(string? username)` | Bot username | `BotUsername` | Immediately: `ArgumentNullException` if `null`, `ArgumentException` if empty. |
| `WithOwnerId(long? ownerId)` | Owner Telegram ID, or `null` | `OwnerId` | Not checked. |
| `WithDatabaseConnectionString(string connectionString)` | Connection string | `DatabaseConnectionString` | Immediately: `ArgumentNullException` if `null`, `ArgumentException` if empty. |
| `WithSessionTimeoutMinutes(int minutes)` | Timeout in minutes | `SessionTimeoutMinutes` | Not checked. |
| `WithMessageProcessingTimeoutSeconds(int seconds)` | Timeout in seconds | `MessageProcessingTimeoutSeconds` | Not checked. |
| `WithEnableLogging(bool enable = true)` | `true` or `false` | `EnableLogging` | Not checked. |
| `WithLogLevel(LogLevel level)` | A `LogLevel` value | `LogLevel` | Not checked. |
| `WithMaxConcurrentRequests(int max)` | Maximum concurrent requests | `MaxConcurrentRequests` | Not checked. |
| `WithEnableWebhook(bool enable = true)` | `true` or `false` | `EnableWebhook` | Not checked. |

Only the four string methods throw when you call them. They use `ArgumentException.ThrowIfNullOrEmpty`, which throws `ArgumentNullException` for `null` input and `ArgumentException` for an empty string. The other `With*` methods store the value as given.

`LogLevel` is the enum in the same namespace (`TelegramBotFramework.Models.LogLevel`): `Debug`, `Info` (default), `Warning`, `Error`, `Critical`.

## Build

```csharp
public BotConfiguration Build()
```

`Build()` calls `ArgumentException.ThrowIfNullOrEmpty` on `BotToken` and throws `ArgumentException` (with parameter name `BotToken`) if the token is empty. It returns the `BotConfiguration` instance the builder holds, not a copy.

Calling a `With*` method after `Build()` changes that same object. Call `Build()` again if you want the current state, and be aware that the earlier result changes too.

### What Build does not check

- `Build()` does not call `BotConfiguration.Validate()`. `Validate()` also requires `BotUsername`, `SessionTimeoutMinutes >= 1`, and `MaxConcurrentRequests >= 1`. A configuration returned by `Build()` can pass the builder and still fail `Validate()`.
- A whitespace-only `BotToken` passes `Build()`. `BotConfiguration.Validate()` rejects it.
- `BotUsername`, numeric values, and `LogLevel` are not range-checked by the builder.

If you need the full set of checks, call `Validate()` on the result:

```csharp
var config = builder.Build();
config.Validate();
```

## Defaults

The builder starts from the `BotConfiguration` defaults:

| Property | Default |
|----------|---------|
| `BotToken` | `""` (empty; `Build()` fails until set) |
| `BotUsername` | `""` |
| `OwnerId` | `null` |
| `DatabaseConnectionString` | `""` |
| `SessionTimeoutMinutes` | `30` |
| `MessageProcessingTimeoutSeconds` | `10` |
| `EnableLogging` | `true` |
| `LogLevel` | `LogLevel.Info` |
| `MaxConcurrentRequests` | `10` |
| `EnableWebhook` | `false` |

See [BotConfiguration](BotConfiguration.md) for the full list of properties.

## Usage

Build a configuration from values you read at startup:

```csharp
using TelegramBotFramework.Models;

var botToken = Environment.GetEnvironmentVariable("BOT_TOKEN")
    ?? throw new InvalidOperationException("BOT_TOKEN is not set.");

BotConfiguration config = new BotConfigurationBuilder()
    .WithBotToken(botToken)
    .WithBotUsername("my_example_bot")
    .WithOwnerId(123456789L)
    .WithDatabaseConnectionString("Data Source=bot.db")
    .WithSessionTimeoutMinutes(60)
    .WithLogLevel(LogLevel.Debug)
    .WithMaxConcurrentRequests(20)
    .WithEnableWebhook(false)
    .Build();

// Properties the builder does not cover are set on the built instance.
config.ApiKey = Environment.GetEnvironmentVariable("API_KEY");
config.RateLimitPerMinute = 60;

config.Validate();
```

Copy an existing configuration and change one value:

```csharp
BotConfiguration debugConfig = BotConfigurationBuilder
    .From(config)
    .WithLogLevel(LogLevel.Debug)
    .Build();
```

Note that `From` copies only the ten properties listed above, so `ApiKey`, `RateLimitPerMinute`, and the other uncovered properties are not carried over into `debugConfig`.

The token shown above is read from the environment. Do not hardcode real tokens or connection secrets in source files.
