# UserSessionBuilder

Fluent builder for `UserSession`. Each `With*` method sets one property and returns the same builder, and `Build()` checks the session ID, user ID and chat ID and returns a new `UserSession`.

Namespace: `TelegramBotFramework.Models`
Visibility: `public sealed class`

## Purpose

`UserSession` is a plain settable class. Creating one with an object initializer works, but nothing checks that `SessionId`, `UserId` and `ChatId` were set. `UserSessionBuilder` gives the same settings as a chain of `With*` calls, and `Build()` rejects a missing session ID and non-positive user and chat IDs before the session is stored or used.

Use the builder when you want to:

- Create a session in one expression, for example in tests or when a new conversation starts.
- Copy an existing session and change a few values with `From(...)`, for example to move it to another state.
- Get an early failure from `Build()` when the session ID, user ID or chat ID is missing.

## Creating a builder

The constructor is the implicit public parameterless constructor. A new builder starts with these values:

| Field | Initial value |
|-------|---------------|
| `sessionId` | `string.Empty` (invalid until set) |
| `userId`, `chatId` | `0` (invalid until set) |
| `state` | `SessionState.Active` |
| `currentContext` | `"menu"` |
| `currentMenuId` | `null` |
| `createdAt` | `DateTime.UtcNow` at construction |
| `lastActivityAt` | `DateTime.UtcNow` at construction |
| `expiresAt` | `null` |
| `contextData` | `null` |

Start from an existing session with the static method `From(UserSession template)`. It copies the template's values into a new builder and throws `ArgumentNullException` if `template` is `null`. The template is not modified.

## Builder methods

Every `With*` method returns the same builder instance, so calls chain.

| Method | Parameter | Stored as | Checked when |
|--------|-----------|-----------|--------------|
| `WithSessionId(string sessionId)` | Session ID | `SessionId` | Immediately: throws `ArgumentException` if `null` or empty. |
| `WithUserId(long userId)` | Telegram user ID | `UserId` | `Build()`: must be greater than `0`. |
| `WithChatId(long chatId)` | Telegram chat ID | `ChatId` | `Build()`: must be greater than `0`. |
| `WithState(SessionState state)` | Session state | `State` | Not checked. |
| `WithCurrentContext(string currentContext)` | Current context name | `CurrentContext` | Immediately: throws `ArgumentException` if `null` or empty. |
| `WithCurrentMenuId(string? currentMenuId)` | Current menu ID, or `null` | `CurrentMenuId` | Not checked. |
| `WithCreatedAt(DateTime createdAt)` | Creation timestamp | `CreatedAt` | Not checked. |
| `WithLastActivityAt(DateTime? lastActivityAt)` | Last activity timestamp, or `null` | `LastActivityAt` | Not checked. |
| `WithExpiresAt(DateTime? expiresAt)` | Expiration timestamp, or `null` | `ExpiresAt` | Not checked. |
| `WithContextData(Dictionary<string, string>? contextData)` | Context data, or `null` | `ContextData` | Not checked. |

Only `WithSessionId` and `WithCurrentContext` check their input when you call them, and they reject `null` and empty strings only. Whitespace passes these two methods. `Build()` rejects a whitespace-only session ID, so a whitespace value fails at `Build()`, not at the `With*` call.

The user and chat ID checks run only in `Build()`, so an invalid ID is reported when you build, not when you set it.

### What the builder does not set

`UserSession` has properties that the builder does not expose. `Build()` leaves them at the defaults from the `UserSession` class:

- `InteractionCount`: `0`.
- `CommandHistory`: `null`.
- `UserInput`: `null`.

`From(template)` does not copy these three properties. A session created by `From(...).Build()` starts with a cleared command history, interaction count and user input.

## Build

```csharp
public UserSession Build()
```

`Build()` runs these checks in order and throws `ArgumentException` on the first failure:

1. `SessionId` must not be `null`, empty or whitespace. The exception's parameter name is `sessionId`.
2. `UserId` must be greater than `0`. The exception's parameter name is `userId`.
3. `ChatId` must be greater than `0`. The exception's parameter name is `chatId`.

If all checks pass, `Build()` returns a new `UserSession` with `State`, `CurrentContext`, timestamps and `ContextData` set from the builder.

### Shared references

`Build()` does not copy the context data dictionary. The returned session uses the same `ContextData` instance as the builder, and `From(template)` shares the template's dictionary too. Changes made through one object are visible through the other. Pass a new dictionary if the sessions must stay independent.

## Usage example

```csharp
using TelegramBotFramework.Models;

// Create a new session for a user in a chat.
var session = new UserSessionBuilder()
    .WithSessionId("session-123")
    .WithUserId(42)
    .WithChatId(1001)
    .WithCurrentContext("menu")
    .WithContextData(new Dictionary<string, string> { ["language"] = "en" })
    .Build();

// Copy the session and mark it idle. The copy shares the ContextData dictionary with the original.
var idle = UserSessionBuilder.From(session)
    .WithState(SessionState.Idle)
    .WithLastActivityAt(DateTime.UtcNow)
    .Build();

// Missing user ID: Build() throws ArgumentException (parameter name "userId").
// new UserSessionBuilder().WithSessionId("s").WithChatId(1).Build();
```
