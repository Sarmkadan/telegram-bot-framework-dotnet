# MessageBuilder

Fluent builder for `Message`. Each `With*` method sets one property and returns the same builder, and `Build()` checks the required fields (user ID, chat ID, content) before returning a new `Message`.

Namespace: `TelegramBotFramework.Models`
Visibility: `public sealed class`

## Purpose

`Message` is a plain class with settable properties. It can be created with an object initializer, but nothing enforces that `UserId`, `ChatId`, or `Content` are set. `MessageBuilder` sets the same values through a chain of `With*` calls, and `Build()` rejects a message with missing required fields before it reaches the rest of the pipeline.

Use the builder when you want to:

- Create a message in one expression, for example in tests or when simulating an incoming update.
- Copy an existing message and change a few values with `From(...)`.
- Get an early failure from `Build()` when the user ID, chat ID, or content is missing.

## Creating a builder

The constructor is public and takes no arguments. A new builder starts with these values:

| Field | Initial value |
|-------|---------------|
| `_messageId`, `_userId`, `_chatId` | `0` (invalid for user and chat until set) |
| `_content` | `string.Empty` (invalid until set) |
| `_type` | `MessageType.Text` |
| `_status` | `MessageStatus.Received` |
| `_createdAt` | `DateTime.UtcNow` at construction |
| `_processedAt`, `_commandName`, `_metadata` | `null` |

Start from an existing message with the static method `From(Message template)`. It throws `ArgumentNullException` if `template` is `null`. The template is not modified. Because `From` passes each value through the `With*` methods, it also throws `ArgumentException` if the template has a non-positive `UserId` or `ChatId`, or whitespace-only `Content`. Those are the same checks as the individual setters, and they run at `From` time, not at `Build()` time.

## Builder methods

Every `With*` method returns the same builder instance, so calls chain.

| Method | Parameter | Stored as | Checked when |
|--------|-----------|-----------|--------------|
| `WithMessageId(long messageId)` | Message ID | `MessageId` | Not checked. |
| `WithUserId(long userId)` | User ID | `UserId` | Immediately: throws `ArgumentException` if `userId <= 0`. |
| `WithChatId(long chatId)` | Chat ID | `ChatId` | Immediately: throws `ArgumentException` if `chatId <= 0`. |
| `WithContent(string content)` | Message text | `Content` | Immediately: throws `ArgumentException` if `null`, empty, or whitespace. |
| `WithType(MessageType type)` | Message type | `Type` | Not checked. |
| `WithStatus(MessageStatus status)` | Message status | `Status` | Not checked. |
| `WithCreatedAt(DateTime createdAt)` | Creation timestamp | `CreatedAt` | Not checked. |
| `WithProcessedAt(DateTime? processedAt)` | Processing timestamp, or `null` | `ProcessedAt` | Not checked. |
| `WithCommandName(string? commandName)` | Command name, or `null` | `CommandName` | Not checked. |
| `WithMetadata(Dictionary<string, object>? metadata)` | Metadata dictionary, or `null` | `Metadata` | Not checked. |

The three required values (`UserId`, `ChatId`, `Content`) are checked when you call their setters and again in `Build()`. `MessageId` has no check at all.

`WithMetadata` stores the dictionary reference. Changes made to the dictionary after the call are visible to the builder and to any message it builds.

## Static method

```csharp
public static MessageBuilder From(Message template)
```

Copies `MessageId`, `UserId`, `ChatId`, `Content`, `Type`, `Status`, `CreatedAt`, `ProcessedAt`, `CommandName`, and `Metadata` from `template`.

`From` does not copy `AttachmentUrls`, `IsEdited`, `ReplyToMessageId`, or `ForwardedFromUserId`, because the builder has no methods for them. A message created with `From(...).Build()` therefore has the default values for those four properties.

`Metadata` is copied by reference, so the new builder and the template share the same dictionary.

## Build

```csharp
public Message Build()
```

`Build()` runs these checks in order and throws `ArgumentException` on the first failure:

1. `_userId` must be positive. The parameter name is `_userId`.
2. `_chatId` must be positive. The parameter name is `_chatId`.
3. `_content` must not be `null`, empty, or whitespace. The parameter name is `_content`.

If all checks pass, `Build()` returns a new `Message` with the builder's values. `Metadata` is the same dictionary reference the builder holds, not a copy.

### What Build does not check

`Build()` does not call `Message.Validate()`. Because `Validate()` performs the same three checks, calling it after `Build()` is redundant for the required fields. It is not called by `Build()`, so a message created through the builder is not validated again. `Validate()` throws `InvalidOperationException`, not `ArgumentException`, which is why the two error types differ.

`Build()` also does not set `AttachmentUrls`, `IsEdited`, `ReplyToMessageId`, or `ForwardedFromUserId`. Those keep their default values on the built message. To set them, assign them on the returned `Message` directly.

## Usage example

```csharp
using TelegramBotFramework.Models;

var incoming = new MessageBuilder()
    .WithMessageId(42)
    .WithUserId(1001)
    .WithChatId(1001)
    .WithContent("/start")
    .WithType(MessageType.Text)
    .WithCommandName("start")
    .WithMetadata(new Dictionary<string, object> { ["source"] = "test" })
    .Build();

incoming.Validate();

// Copy the message and mark it as processed.
var processed = MessageBuilder.From(incoming)
    .WithStatus(MessageStatus.Processed)
    .WithProcessedAt(DateTime.UtcNow)
    .Build();

// Set a field the builder does not cover.
processed.ReplyToMessageId = 41;
```

`Build()` throws `ArgumentException` if a required field is missing:

```csharp
// Throws ArgumentException: "Message content cannot be empty." (parameter name: _content)
var empty = new MessageBuilder()
    .WithUserId(1001)
    .WithChatId(1001)
    .Build();
```
