# BotControllerBuilder

`BotControllerBuilder` is a fluent builder for `ProcessMessageRequest` objects. It lives in the `TelegramBotFramework.Controllers` namespace, next to `BotController`, and is intended for code that needs to construct a message-processing request without populating the request model by hand, such as tests, replay tooling, or internal dispatch code that forwards a message to `BotController.ProcessMessage`.

The builder collects values through chained `With*` calls, validates the required fields in `Build()`, and returns a new `ProcessMessageRequest` on each call. The class is `sealed` and has a public parameterless constructor; there are no constructor arguments.

## API

### Constructors

#### `public BotControllerBuilder()`
Creates an empty builder. The defaults are:

| Field         | Default            |
|---------------|--------------------|
| `UserId`      | `0` (not set)      |
| `ChatId`      | `0` (not set)      |
| `FirstName`   | `string.Empty`     |
| `LastName`    | `null`             |
| `Content`     | `string.Empty`     |
| `MessageType` | `MessageType.Text` |

`UserId`, `ChatId` and `Content` must be set before `Build()` succeeds.

### Methods

All `With*` methods return the same builder instance so calls can be chained.

#### `public BotControllerBuilder WithUserId(long userId)`
Sets the Telegram user identifier.
*   **Parameters**: `userId` - the user identifier. Must be greater than zero.
*   **Return Value**: The builder instance.
*   **Throws**: `ArgumentException` immediately if `userId` is less than or equal to zero. The builder state is not changed in that case.

#### `public BotControllerBuilder WithChatId(long chatId)`
Sets the Telegram chat identifier.
*   **Parameters**: `chatId` - the chat identifier. Must be greater than zero.
*   **Return Value**: The builder instance.
*   **Throws**: `ArgumentException` immediately if `chatId` is less than or equal to zero.

#### `public BotControllerBuilder WithFirstName(string? firstName)`
Sets the user's first name.
*   **Parameters**: `firstName` - the first name. `null` is stored as `string.Empty`.
*   **Return Value**: The builder instance.
*   **Throws**: Nothing.

#### `public BotControllerBuilder WithLastName(string? lastName)`
Sets the user's last name.
*   **Parameters**: `lastName` - the last name. `null` is kept as `null`, which matches the optional nature of the property on `ProcessMessageRequest`.
*   **Return Value**: The builder instance.
*   **Throws**: Nothing.

#### `public BotControllerBuilder WithContent(string content)`
Sets the message content.
*   **Parameters**: `content` - the message text or payload. Must not be empty.
*   **Return Value**: The builder instance.
*   **Throws**: `ArgumentNullException` if `content` is `null`; `ArgumentException` if it is an empty string. These come from `ArgumentException.ThrowIfNullOrEmpty`. The XML comment on the method describes both cases as `ArgumentException`, but the `null` case actually throws `ArgumentNullException`, which derives from `ArgumentException`.

#### `public BotControllerBuilder WithMessageType(MessageType messageType)`
Sets the kind of message being simulated. See `MessageType` in `Models/Message.cs` for the available values (`Text`, `Photo`, `Video`, `Audio`, `Document`, `Sticker`, `Location`, `Contact`, `Poll`).
*   **Parameters**: `messageType` - the message type. Not validated beyond the enum's own range.
*   **Return Value**: The builder instance.
*   **Throws**: Nothing.

#### `public static BotControllerBuilder From(ProcessMessageRequest template)`
Creates a new builder pre-filled with the values of an existing request. The template is copied, not referenced, so later changes to the template do not affect the builder.
*   **Parameters**: `template` - the request to copy values from.
*   **Return Value**: A new `BotControllerBuilder` instance.
*   **Throws**: `ArgumentNullException` if `template` is `null`.
*   **Note**: `From` copies values without validating them. An invalid template (for example, `UserId = 0`) is accepted here and rejected later by `Build()`.

#### `public ProcessMessageRequest Build()`
Validates the collected values and creates a `ProcessMessageRequest`.
*   **Parameters**: None.
*   **Return Value**: A new `ProcessMessageRequest` populated from the builder's current state. Each call returns a new instance.
*   **Throws**: `ArgumentException` in these cases, checked in this order:
    1.  `UserId` is not set or is less than or equal to zero.
    2.  `ChatId` is not set or is less than or equal to zero.
    3.  `Content` is `null` or empty.

    `FirstName`, `LastName` and `MessageType` are not validated by `Build()`.

## Behavior notes

*   The builder is mutable. Calling `With*` methods on the same instance changes its state, so create a separate builder when you need two requests that differ in only one field.
*   Validation is split between the setters and `Build()`. `WithUserId`, `WithChatId` and `WithContent` reject bad values as soon as they are called. `From` skips validation, so its result is only checked when `Build()` runs.
*   `ProcessMessageRequest` is a plain model with public setters. Nothing in the builder prevents you from modifying the request after `Build()`.

## Usage example

Build a plain text request and pass it to a controller:

```csharp
using TelegramBotFramework.Controllers;
using TelegramBotFramework.Models;

ProcessMessageRequest request = new BotControllerBuilder()
    .WithUserId(123456789)
    .WithChatId(123456789)
    .WithFirstName("Ada")
    .WithLastName("Lovelace")
    .WithContent("/start")
    .WithMessageType(MessageType.Text)
    .Build();

IActionResult result = await botController.ProcessMessage(request, cancellationToken);
```

Start from an existing request and change one field. `From` copies the template, so the original is left unchanged:

```csharp
ProcessMessageRequest original = new()
{
    UserId = 123456789,
    ChatId = 123456789,
    FirstName = "Ada",
    Content = "/help"
};

ProcessMessageRequest edited = BotControllerBuilder.From(original)
    .WithContent("/menu")
    .Build();
```

Handle validation failures from `Build()`:

```csharp
try
{
    ProcessMessageRequest request = new BotControllerBuilder()
        .WithUserId(123456789)
        .WithChatId(123456789)
        .Build(); // Content was never set
}
catch (ArgumentException ex)
{
    // ex.ParamName is "_content"
    logger.LogWarning(ex, "Invalid message request");
}
```

Note that `ParamName` reports the private field name (`_userId`, `_chatId`, `_content`) for `Build()` failures, not the public method parameter name. Callers that match on `ParamName` should expect this.
