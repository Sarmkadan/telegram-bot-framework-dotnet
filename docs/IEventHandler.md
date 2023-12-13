# IEventHandler

The `IEventHandler` interface defines a contract for processing Telegram bot events within the `telegram-bot-framework-dotnet` project. It provides access to message context, command execution results, and bot state transitions, enabling developers to implement custom logic for handling incoming updates, commands, and state changes.

## API

### `Task HandleAsync`
Executes the event handling logic asynchronously.
- **Purpose**: Processes the incoming event (e.g., message, command, or state change) and performs any required actions.
- **Returns**: A `Task` representing the asynchronous operation.
- **Throws**: May throw exceptions if event processing fails, depending on the implementation.

### `string GetHandlerName()`
Retrieves the name of the event handler.
- **Purpose**: Provides a human-readable identifier for the handler, useful for logging or debugging.
- **Returns**: A `string` representing the handler's name.
- **Remarks**: The default implementation returns the type name of the handler. Override this method to provide a custom name.

### Message Context Properties
Properties exposing metadata about the received message or event:
- **`long ChatId`**: The unique identifier of the chat where the message was received.
- **`long UserId`**: The unique identifier of the user who sent the message.
- **`string? MessageText`**: The text content of the message, if available. `null` if the message lacks text (e.g., media messages).
- **`DateTime MessageTimestamp`**: The timestamp when the message was received.
- **`MessageReceivedEvent`**: The raw event object containing the full message payload, if applicable.

### Command Execution Properties
Properties related to command processing:
- **`string CommandName`**: The name of the executed command (e.g., `/start`).
- **`string? Arguments`**: The arguments passed to the command, if any. `null` if no arguments were provided.
- **`bool Success`**: Indicates whether the command execution succeeded (`true`) or failed (`false`).
- **`string? ErrorMessage`**: Contains the error message if `Success` is `false`. `null` if the command executed successfully.
- **`CommandExecutedEvent`**: The raw event object containing the full command execution details, if applicable.

### Bot State Transition Properties
Properties related to bot state changes:
- **`string PreviousState`**: The state of the bot before the transition occurred.
- **`string NewState`**: The state of the bot after the transition.
- **`string? Reason`**: An optional description of why the state change occurred. `null` if no reason was provided.
- **`BotStateChangedEvent`**: The raw event object containing the full state transition details, if applicable.

## Usage

### Example 1: Handling a Message
