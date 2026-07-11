# MessageExtensions

The `MessageExtensions` class provides a set of static extension methods designed to simplify common inspection patterns for `Message` objects within the Telegram Bot Framework. By encapsulating logic for detecting commands, attachments, reply contexts, and type identification, this utility class reduces boilerplate code in message handlers and promotes cleaner, more readable bot logic.

## API

### IsCommand
Determines whether the specified message contains a bot command.
*   **Purpose**: Checks if the message text starts with a forward slash (`/`) and conforms to standard command formatting.
*   **Parameters**: `this Message message` – The message instance to evaluate.
*   **Return Value**: `bool` – Returns `true` if the message is a command; otherwise, `false`.
*   **Exceptions**: Throws `ArgumentNullException` if the `message` argument is null.

### HasAttachments
Checks if the message contains any media or file attachments.
*   **Purpose**: Inspects the message properties to identify the presence of photos, documents, audio, video, or other file types.
*   **Parameters**: `this Message message` – The message instance to evaluate.
*   **Return Value**: `bool` – Returns `true` if one or more attachments are present; otherwise, `false`.
*   **Exceptions**: Throws `ArgumentNullException` if the `message` argument is null.

### GetTypeString
Retrieves a string representation of the specific message type.
*   **Purpose**: Returns a descriptive string indicating the category of the message (e.g., "Text", "Photo", "Command") based on its content structure.
*   **Parameters**: `this Message message` – The message instance to evaluate.
*   **Return Value**: `string` – A string describing the message type. Returns "Unknown" if the type cannot be determined.
*   **Exceptions**: Throws `ArgumentNullException` if the `message` argument is null.

### IsReply
Determines if the message is sent as a reply to another message.
*   **Purpose**: Verifies whether the `ReplyToMessage` property of the message is populated.
*   **Parameters**: `this Message message` – The message instance to evaluate.
*   **Return Value**: `bool` – Returns `true` if the message is a reply; otherwise, `false`.
*   **Exceptions**: Throws `ArgumentNullException` if the `message` argument is null.

## Usage

The following example demonstrates how to use `IsCommand` and `GetTypeString` to route incoming messages within a handler:

```csharp
public async Task HandleMessageAsync(Message message)
{
    if (message.IsCommand())
    {
        var commandType = message.GetTypeString();
        await ProcessCommandAsync(message, commandType);
        return;
    }

    if (message.HasAttachments())
    {
        await SaveAttachmentAsync(message);
    }
}
```

The next example illustrates checking for reply contexts to maintain conversation threads:

```csharp
public async Task RespondToUserAsync(Message incomingMessage)
{
    if (incomingMessage.IsReply())
    {
        var originalMessage = incomingMessage.ReplyToMessage;
        await ReplyAsync($"Re: {originalMessage.Text}", incomingMessage.Chat.Id);
    }
    else
    {
        await SendAsync("Please reply to a specific message for context.", incomingMessage.Chat.Id);
    }
}
```

## Notes

*   **Null Safety**: All extension methods in this class perform a null check on the `message` parameter. Passing a null reference will result in an `ArgumentNullException`. Callers should ensure the message object is valid before invoking these methods.
*   **Thread Safety**: As this class consists entirely of stateless static methods that operate solely on the provided input parameters, it is fully thread-safe and can be used concurrently across multiple request handling threads without synchronization.
*   **Command Detection Logic**: The `IsCommand` method relies on the standard Telegram convention where commands begin with `/`. It does not validate whether the command string matches a registered handler, only that the format implies a command.
*   **Attachment Definition**: `HasAttachments` returns `true` for any non-text content type supported by the Telegram API, including photos, voice notes, documents, and contact shares.
