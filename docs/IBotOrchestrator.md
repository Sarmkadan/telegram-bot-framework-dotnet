# IBotOrchestrator

Central interface for orchestrating bot interactions, managing user sessions, processing messages and commands, and handling menu navigation within the Telegram Bot Framework for .NET.

## API

### `BotOrchestrator`
The default implementation of `IBotOrchestrator` provided by the framework. This class coordinates the lifecycle of user sessions, message processing, command execution, and menu interactions. It is designed to be used as a singleton or scoped service within dependency injection containers.

### `ProcessUserMessageAsync`
Processes an incoming user message, determines the appropriate action (e.g., command execution, session update, or menu navigation), and returns an updated execution context.

- **Parameters**
  - `userId` (long): Unique identifier of the user.
  - `messageText` (string): Raw text of the user's message.
  - `cancellationToken` (CancellationToken): Token to monitor for cancellation requests.

- **Returns**
  - `Task<Models.ExecutionContext>`: Updated execution context reflecting the result of processing (e.g., command output, menu state, or session update).

- **Exceptions**
  - Throws `ArgumentNullException` if `messageText` is null.
  - Throws `ArgumentException` if `userId` is non-positive.

### `ExecuteUserCommandAsync`
Executes a registered command for the specified user, updating the session and returning the resulting execution context.

- **Parameters**
  - `userId` (long): Unique identifier of the user.
  - `commandName` (string): Name of the command to execute.
  - `args` (IReadOnlyList<string>): Arguments passed to the command.
  - `cancellationToken` (CancellationToken): Token to monitor for cancellation requests.

- **Returns**
  - `Task<Models.ExecutionContext>`: Execution context containing command output, updated session state, and any resulting menu.

- **Exceptions**
  - Throws `ArgumentNullException` if `commandName` or `args` is null.
  - Throws `ArgumentException` if `userId` is non-positive or `commandName` is empty.

### `DisplayMenuAsync`
Generates and returns the current menu for the user based on their session state.

- **Parameters**
  - `userId` (long): Unique identifier of the user.
  - `cancellationToken` (CancellationToken): Token to monitor for cancellation requests.

- **Returns**
  - `Task<Models.Menu>`: The menu to display, or `null` if no menu is applicable.

- **Exceptions**
  - Throws `ArgumentException` if `userId` is non-positive.

### `HandleMenuButtonAsync`
Processes a button interaction within a menu for the specified user.

- **Parameters**
  - `userId` (long): Unique identifier of the user.
  - `buttonId` (string): Identifier of the pressed button.
  - `cancellationToken` (CancellationToken): Token to monitor for cancellation requests.

- **Returns**
  - `Task<bool>`: `true` if the button was handled successfully; otherwise, `false`.

- **Exceptions**
  - Throws `ArgumentNullException` if `buttonId` is null.
  - Throws `ArgumentException` if `userId` is non-positive.

### `GetUserSessionAsync`
Retrieves the current user session associated with the specified user.

- **Parameters**
  - `userId` (long): Unique identifier of the user.
  - `cancellationToken` (CancellationToken): Token to monitor for cancellation requests.

- **Returns**
  - `Task<Models.UserSession>`: The user's session, or `null` if no session exists.

- **Exceptions**
  - Throws `ArgumentException` if `userId` is non-positive.

### `EndUserSessionAsync`
Terminates the user session, releasing any associated resources and clearing state.

- **Parameters**
  - `userId` (long): Unique identifier of the user.
  - `cancellationToken` (CancellationToken): Token to monitor for cancellation requests.

- **Returns**
  - `Task<bool>`: `true` if the session was successfully ended; otherwise, `false`.

- **Exceptions**
  - Throws `ArgumentException` if `userId` is non-positive.

## Usage

### Example 1: Processing a user message
