# InlineQueryBuilder

Fluent builder for `InlineQuery`. Each `With*` method sets one property and returns the same builder, and `Build()` checks the required fields (query ID and user ID) before returning a new `InlineQuery`.

Namespace: `TelegramBotFramework.Models`
Visibility: `public sealed class`

## Purpose

`InlineQuery` is a plain class with settable properties. It can be created with an object initializer, but nothing enforces that `QueryId` and `UserId` are set. `InlineQueryBuilder` sets the same values through a chain of `With*` calls, and `Build()` rejects a query with a missing query ID or a non-positive user ID before it reaches the rest of the pipeline.

Use the builder when you want to:

- Create an inline query in one expression, for example in tests or when simulating an incoming inline update.
- Copy an existing query and change a few values with `From(...)`.
- Get an early failure from `Build()` when the query ID or user ID is missing.

## Creating a builder

The constructor is public and takes no arguments. A new builder starts with these values:

| Field | Initial value |
|-------|---------------|
| `_queryId`, `_query`, `_offset` | `string.Empty` (`_queryId` is invalid until set) |
| `_userId` | `0` (invalid until set) |
| `_status` | `InlineQueryStatus.Pending` |
| `_receivedAt` | `DateTime.UtcNow` at construction |
| `_answeredAt`, `_metadata` | `null` |

Start from an existing query with the static method `From(InlineQuery template)`. It throws `ArgumentNullException` if `template` is `null`. The template is not modified. `From` passes `QueryId` through `WithQueryId`, so it also throws if the template's `QueryId` is `null` or empty. It does not check `UserId` at that point; that check runs in `Build()`.

## Builder methods

Every `With*` method returns the same builder instance, so calls chain.

| Method | Parameter | Stored as | Checked when |
|--------|-----------|-----------|--------------|
| `WithQueryId(string value)` | Telegram query identifier | `QueryId` | Immediately: throws `ArgumentNullException` if `null`, `ArgumentException` if empty. |
| `WithUserId(long value)` | Telegram user ID | `UserId` | Not checked here. `Build()` requires a positive value. |
| `WithQuery(string value)` | Query text | `Query` | Not checked. |
| `WithOffset(string value)` | Pagination offset | `Offset` | Not checked. |
| `WithStatus(InlineQueryStatus value)` | Processing status | `Status` | Not checked. |
| `WithReceivedAt(DateTime value)` | Received timestamp (UTC) | `ReceivedAt` | Not checked. |
| `WithAnsweredAt(DateTime? value)` | Answered timestamp (UTC), or `null` | `AnsweredAt` | Not checked. |
| `WithMetadata(Dictionary<string, object>? value)` | Metadata dictionary, or `null` | `Metadata` | Not checked. |

`WithQueryId` is the only setter that validates its input immediately. Its check rejects `null` and empty strings but not whitespace-only strings. `Build()` uses a stricter check and rejects whitespace too.

`WithMetadata` stores the dictionary reference. Changes made to the dictionary after the call are visible to the builder and to any query it builds.

## Static method

```csharp
public static InlineQueryBuilder From(InlineQuery template)
```

Copies `QueryId`, `UserId`, `Query`, `Offset`, `Status`, `ReceivedAt`, `AnsweredAt`, and `Metadata` from `template`.

`Metadata` is copied by reference, so the new builder and the template share the same dictionary.

## Build

```csharp
public InlineQuery Build()
```

`Build()` runs these checks in order and throws `ArgumentException` on the first failure:

1. `_queryId` must not be `null`, empty, or whitespace. The parameter name is `_queryId`.
2. `_userId` must be positive. The parameter name is `_userId`.

If both checks pass, `Build()` returns a new `InlineQuery` with the builder's values. `Metadata` is the same dictionary reference the builder holds, not a copy.

### What Build does not check

- `Query` and `Offset` are not checked. An empty query text is valid, as the `InlineQuery` documentation describes for open-ended queries.
- `ReceivedAt` and `AnsweredAt` are not checked for consistency. A query can be built with an `AnsweredAt` value that is earlier than `ReceivedAt`.
- `Status` is not checked against `AnsweredAt`. A query can be built as `Pending` with an answered timestamp set.

## Usage example

```csharp
using TelegramBotFramework.Models;

// Build a new inline query, for example in a test
var query = new InlineQueryBuilder()
    .WithQueryId("123456789")
    .WithUserId(42)
    .WithQuery("cats")
    .WithOffset(string.Empty)
    .WithStatus(InlineQueryStatus.Processing)
    .Build();

// Copy an existing query and mark it as answered
var answered = InlineQueryBuilder.From(query)
    .WithStatus(InlineQueryStatus.Answered)
    .WithAnsweredAt(DateTime.UtcNow)
    .Build();

// Missing QueryId or non-positive UserId fails in Build()
try
{
    var invalid = new InlineQueryBuilder()
        .WithQuery("cats")
        .Build();
}
catch (ArgumentException ex)
{
    // ex.ParamName == "_queryId"
}
```

## Related types

- `InlineQuery`: the model this builder creates. Defined in `src/TelegramBotFramework/Models/InlineQuery.cs`.
- `InlineQueryStatus`: the values for `WithStatus`: `Pending`, `Processing`, `Answered`, `Failed`, `Cached`.
- `InlineQueryResultBuilder`: builds the results returned for an inline query.
