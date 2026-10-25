# BotUserBuilder

Fluent builder for `BotUser`. Each `With*` method sets one property and returns the same builder, and `Build()` checks the Telegram ID and first name and returns a new `BotUser`.

Namespace: `TelegramBotFramework.Models`
Visibility: `public sealed class`

## Purpose

`BotUser` is a plain settable class. Creating one with an object initializer works, but nothing checks that `TelegramId` and `FirstName` were set. `BotUserBuilder` gives the same settings as a chain of `With*` calls, and `Build()` rejects a non-positive Telegram ID and a missing first name before the user is stored or used.

Use the builder when you want to:

- Create a user in one expression, for example in tests or when a new user first talks to the bot.
- Copy an existing user and change a few values with `From(...)`, for example to ban the user or promote them to a moderator.
- Get an early failure from `Build()` when the Telegram ID or first name is missing.

## Creating a builder

The constructor is the explicit public parameterless constructor. A new builder starts with these values:

| Field | Initial value |
|-------|---------------|
| `telegramId` | `0` (invalid until set) |
| `firstName` | `null` (invalid until set) |
| `lastName`, `username`, `phoneNumber` | `null` |
| `status` | `UserStatus.Active` |
| `role` | `UserRole.User` |
| `createdAt`, `updatedAt` | `DateTime.UtcNow` at construction |
| `lastActivityAt` | `null` |

Start from an existing user with the static method `From(BotUser template)`. It copies the template's values into a new builder and throws `ArgumentNullException` if `template` is `null`. The template is not modified.

## Builder methods

Every `With*` method returns the same builder instance, so calls chain.

| Method | Parameter | Stored as | Checked when |
|--------|-----------|-----------|--------------|
| `WithTelegramId(long value)` | Telegram user ID | `TelegramId` | `Build()`: must be greater than `0`. |
| `WithFirstName(string value)` | First name | `FirstName` | Immediately: throws `ArgumentException` if `null` or empty. `Build()` also rejects whitespace. |
| `WithLastName(string? value)` | Last name, or `null` | `LastName` | Not checked. |
| `WithUsername(string? value)` | Username, or `null` | `Username` | Not checked. |
| `WithPhoneNumber(string? value)` | Phone number, or `null` | `PhoneNumber` | Not checked. |
| `WithStatus(UserStatus value)` | User status | `Status` | Not checked. |
| `WithRole(UserRole value)` | User role | `Role` | Not checked. |
| `WithCreatedAt(DateTime value)` | Creation timestamp | `CreatedAt` | Not checked. |
| `WithUpdatedAt(DateTime value)` | Update timestamp | `UpdatedAt` | Not checked. |
| `WithLastActivityAt(DateTime? value)` | Last activity timestamp, or `null` | `LastActivityAt` | Not checked. |

`WithFirstName` rejects `null` and empty strings when you call it. Whitespace passes that check, so a whitespace-only name fails at `Build()`, not at the `With*` call.

The Telegram ID check runs only in `Build()`, so an invalid ID is reported when you build, not when you set it.

### What the builder does not set

`BotUser` has properties that the builder does not expose. `Build()` leaves them at the defaults from the `BotUser` class:

- `IsBot`: `false`.
- `IsPremium`: `false`.
- `CommandsExecuted`: `0`.
- `MessagesCount`: `0`.
- `Metadata`: `null`.

`From(template)` does not copy these five properties. A user created by `From(...).Build()` starts with cleared counters, no premium or bot flag, and no metadata. Use `BotUser.SetMetadata` or set the flags on the result if you need them.

## Build

```csharp
public BotUser Build()
```

`Build()` runs these checks in order and throws `ArgumentException` on the first failure:

1. `TelegramId` must be greater than `0`. The exception's parameter name is `_telegramId`.
2. `FirstName` must not be `null`, empty or whitespace. The exception's parameter name is `_firstName`.

If both checks pass, `Build()` returns a new `BotUser` with the stored values. `UpdatedAt` is taken from the builder as set; the builder does not refresh it. Call `WithUpdatedAt(DateTime.UtcNow)` if you need the current time.

## From and missing first names

`From(template)` passes the template's `FirstName` through `WithFirstName`, and a `null` first name is replaced with `string.Empty` first. Because `WithFirstName` rejects empty strings, `From(template)` throws `ArgumentException` when `template.FirstName` is `null` or empty. The exception is thrown by `From`, before `Build()` is called.

If a template may have no first name, set one before calling `From`, or build the copy from scratch with `new BotUserBuilder()`.

## Usage example

```csharp
using TelegramBotFramework.Models;

// Create a new user the first time they write to the bot.
var user = new BotUserBuilder()
    .WithTelegramId(42)
    .WithFirstName("Ada")
    .WithLastName("Lovelace")
    .WithUsername("ada")
    .WithStatus(UserStatus.Active)
    .WithRole(UserRole.User)
    .Build();

// Copy the user and promote them to moderator. The copy keeps the original timestamps.
var moderator = BotUserBuilder.From(user)
    .WithRole(UserRole.Moderator)
    .WithUpdatedAt(DateTime.UtcNow)
    .Build();

// Missing first name: Build() throws ArgumentException (parameter name "_firstName").
// new BotUserBuilder().WithTelegramId(1).Build();
```
