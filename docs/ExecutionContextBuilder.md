# ExecutionContextBuilder

Fluent builder for `ExecutionContext`. Each `With*` method sets one property and returns the same builder, and `Build()` checks the user and chat IDs and returns a new `ExecutionContext`.

Namespace: `TelegramBotFramework.Models`
Visibility: `public sealed class`

## Purpose

`ExecutionContext` is a plain settable class. Creating one with an object initializer works, but nothing checks that `UserId` and `ChatId` were set. `ExecutionContextBuilder` gives the same settings as a chain of `With*` calls, and `Build()` rejects non-positive user and chat IDs before the context reaches the pipeline.

Use the builder when you want to:

- Create a context in one expression, for example in tests or when a Telegram update is converted into a context.
- Copy an existing context and change a few values with `From(...)`.
- Get an early failure from `Build()` when the user or chat ID is missing.

## Creating a builder

The constructor is public and takes no arguments. A new builder starts with these values:

| Property | Initial value |
|----------|---------------|
| `_contextId` | A new GUID string |
| `_createdAt` | `DateTime.UtcNow` at construction |
| `_states` | An empty dictionary |
| `_userId`, `_chatId` | `0` (invalid until set) |
| All others | `null` |

Start from an existing context with the static method `From(ExecutionContext template)`. It copies the template's values into a new builder and throws `ArgumentNullException` if `template` is `null`. The template is not modified.

## Builder methods

Every `With*` method returns the same builder instance, so calls chain.

| Method | Parameter | Stored as | Checked when |
|--------|-----------|-----------|--------------|
| `WithContextId(string value)` | Context ID | `ContextId` | Immediately: throws `ArgumentException` if `null` or empty. |
| `WithUserId(long value)` | Telegram user ID | `UserId` | `Build()`: must be greater than `0`. |
| `WithChatId(long value)` | Telegram chat ID | `ChatId` | `Build()`: must be greater than `0`. |
| `WithUser(BotUser? value)` | Bot user, or `null` | `User` | Not checked. |
| `WithSession(UserSession? value)` | Session, or `null` | `Session` | Not checked. |
| `WithCommand(Command? value)` | Command, or `null` | `Command` | Not checked. |
| `WithMessage(Message? value)` | Telegram message, or `null` | `Message` | Not checked. |
| `WithParameters(Dictionary<string, object>? value)` | Parameters, or `null` | `Parameters` | Not checked. |
| `WithCreatedAt(DateTime value)` | Creation timestamp | `CreatedAt` | Not checked. |
| `WithStates(Dictionary<string, object> value)` | State dictionary | `States` | Immediately: throws `ArgumentNullException` if `null`. |

Only `WithContextId` and `WithStates` check their input when you call them. The ID checks in `Build()` run later, so an invalid user or chat ID is reported only at `Build()`.

`WithContextId` rejects an empty string but accepts whitespace.

### What the builder does not set

`ExecutionContext` has more properties than the builder. `Build()` leaves these at the defaults from the `ExecutionContext` class:

- `Errors`: a new empty list.
- `IsValid`: `true`.
- `PendingResponse`, `IsStopped`: their defaults. These are read-only from outside the class.

`From(template)` does not copy `Errors`, `IsValid`, `PendingResponse`, or `IsStopped`. A context created by `From(...).Build()` therefore starts with clean error and validity state.

## Build

```csharp
public ExecutionContext Build()
```

`Build()` runs these checks in order and throws `ArgumentException` on the first failure:

1. `UserId` must be greater than `0`.
2. `ChatId` must be greater than `0`.

The exception's parameter name is `_userId` or `_chatId`, which are the builder's internal field names. Use the message text to tell the two apart.

If both checks pass, `Build()` returns a new `ExecutionContext`. `ContextId` falls back to a new GUID if it is `null`, but `WithContextId` already rejects `null`, so in practice the value you set is used.

### Shared references

`Build()` does not copy the collections. The returned context uses the same `Parameters` and `States` dictionaries as the builder, and `From(template)` shares the template's `States` dictionary too. Changes made through one object are visible through the other.

Calling `Build()` more than once returns separate `ExecutionContext` instances that share the same dictionaries. Each call uses the builder's current values at that moment.

## Defaults

If you call only `Build()` on a new builder, it fails, because `UserId` and `ChatId` default to `0`. Set both IDs first.

Once they are set, these values come from the builder's defaults:

| Property | Default |
|----------|---------|
| `ContextId` | New GUID string |
| `CreatedAt` | `DateTime.UtcNow` at builder creation |
| `States` | Empty dictionary |
| `User`, `Session`, `Command`, `Message`, `Parameters` | `null` |

## Usage

Create a context for a user and chat:

```csharp
using TelegramBotFramework.Models;

var context = new ExecutionContextBuilder()
    .WithUserId(123456789)
    .WithChatId(123456789)
    .WithCommand(command)
    .WithParameters(new Dictionary<string, object> { ["lang"] = "en" })
    .Build();
```

Copy an existing context and change only the parameters. `From` keeps the template's `States` dictionary, as described above:

```csharp
var updated = ExecutionContextBuilder.From(context)
    .WithParameters(new Dictionary<string, object> { ["lang"] = "uk" })
    .Build();
```

Handle a missing chat ID:

```csharp
try
{
    var invalid = new ExecutionContextBuilder()
        .WithUserId(123456789)
        .Build();
}
catch (ArgumentException ex)
{
    // "ChatId must be positive"
    logger.LogError(ex, "Invalid execution context");
}
```

## Related types

- [`ExecutionContext`](ExecutionContext.md): the object the builder creates.
- `BotUser`, `UserSession`, `Command`, `Message`: the types for the optional `With*` values.
