# MessageService

`MessageService` is a core component of the `telegram-bot-framework-dotnet` project responsible for managing the lifecycle of Telegram messages within the system. It provides methods for processing incoming messages, retrieving message histories, marking messages as processed or failed, and performing maintenance tasks such as archiving old messages. The service acts as an intermediary between raw message data and higher-level bot logic, ensuring reliable message tracking and state management.

## API

### `MessageService`
The constructor initializes the service. No public constructor parameters are exposed; dependencies are injected internally.

---

### `Task<Models.Message> ProcessIncomingMessageAsync`
Processes an incoming Telegram message, storing it in the database and preparing it for further handling.

**Returns:**
- A `Task<Models.Message>` representing the processed message with updated metadata (e.g., timestamps, status).

**Throws:**
- `ArgumentNullException`: If the incoming message data is invalid or missing required fields.
- `InvalidOperationException`: If the message cannot be persisted due to database constraints.

---

### `Task<Models.Message?> GetMessageAsync`
Retrieves a single message by its unique identifier.

**Returns:**
- A `Task<Models.Message?>` representing the message if found, or `null` if no message exists with the specified ID.

**Throws:**
- `ArgumentException`: If the provided message ID is invalid (e.g., empty or malformed).

---

### `Task<IList<Models.Message>> GetUserMessagesAsync`
Fetches all messages associated with a specific user, ordered by timestamp (newest first).

**Returns:**
- A `Task<IList<Models.Message>>` containing the user's messages. Returns an empty list if no messages exist for the user.

**Throws:**
- `ArgumentException`: If the user identifier is invalid.

---

### `Task<IList<Models.Message>> GetFailedMessagesAsync`
Retrieves all messages marked as failed, typically for retry or diagnostic purposes.

**Returns:**
- A `Task<IList<Models.Message>>` containing failed messages. Returns an empty list if no failed messages exist.

**Throws:**
- None.

---

### `Task<bool> MarkAsProcessedAsync`
Updates a message's status to "processed" in the database.

**Returns:**
- A `Task<bool>` indicating `true` if the operation succeeded, `false` if the message was not found or the update failed.

**Throws:**
- `ArgumentException`: If the message ID is invalid.

---

### `Task<bool> MarkAsFailedAsync`
Updates a message's status to "failed" in the database, typically after an error during processing.

**Returns:**
- A `Task<bool>` indicating `true` if the operation succeeded, `false` if the message was not found or the update failed.

**Throws:**
- `ArgumentException`: If the message ID is invalid.

---

### `Task<int> GetUnprocessedMessageCountAsync`
Returns the number of messages awaiting processing, useful for monitoring backlog or throttling.

**Returns:**
- A `Task<int>` representing the count of unprocessed messages.

**Throws:**
- None.

---

### `Task ArchiveOldMessagesAsync`
Archives messages older than a configurable threshold (e.g., 30 days), removing them from the active message store. The threshold is defined in the application configuration.

**Returns:**
- A `Task` representing the completion of the operation.

**Throws:**
- `InvalidOperationException`: If the archive operation fails due to database errors.
