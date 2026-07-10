# InMemoryMessageRepository

The `InMemoryMessageRepository` class provides a transient, in-memory implementation for storing and retrieving Telegram bot messages and user sessions. Designed primarily for development, testing, or lightweight single-instance deployments, it implements standard repository patterns without relying on external database systems. This repository manages two distinct entity types: `Models.Message` for tracking bot communication history and `Models.UserSession` for maintaining stateful user interactions, offering asynchronous CRUD operations and various filtering capabilities.

## API

The repository exposes asynchronous methods for managing both `Message` and `UserSession` entities. Note that method names are overloaded based on the entity context implied by their return types and specific parameter signatures.

### Message Operations

*   **`Task<Models.Message?> GetByIdAsync`**
    Retrieves a specific message by its unique identifier. Returns the `Message` object if found; otherwise, returns `null`. Does not throw if the ID is missing.

*   **`Task<IList<Models.Message>> GetAllAsync`**
    Retrieves all stored messages. Returns an empty list if no messages exist.

*   **`Task<Models.Message> CreateAsync`**
    Persists a new `Message` instance to the store. Returns the created entity, typically with an assigned ID. Throws an exception if the entity is null or if a duplicate ID constraint is violated.

*   **`Task<Models.Message> UpdateAsync`**
    Updates an existing `Message` instance. Returns the updated entity. Throws an exception if the entity does not exist or is null.

*   **`Task<bool> DeleteAsync`**
    Removes a message by its identifier. Returns `true` if the deletion was successful; `false` if the identifier was not found.

*   **`Task<bool> ExistsAsync`**
    Checks for the existence of a message by its identifier. Returns `true` if found, `false` otherwise.

*   **`Task<int> CountAsync`**
    Returns the total number of messages currently stored.

*   **`Task<IList<Models.Message>> GetByUserIdAsync`**
    Retrieves all messages associated with a specific user ID. Returns an empty list if no matches are found.

*   **`Task<IList<Models.Message>> GetByChatIdAsync`**
    Retrieves all messages originating from or sent to a specific chat ID. Returns an empty list if no matches are found.

*   **`Task<IList<Models.Message>> GetByStatusAsync`**
    Filters and retrieves messages matching a specific status enum value. Returns an empty list if no matches are found.

*   **`Task<IList<Models.Message>> GetByCommandAsync`**
    Retrieves messages containing or matching a specific bot command. Returns an empty list if no matches are found.

*   **`Task<IList<Models.Message>> GetByDateRangeAsync`**
    Retrieves messages created within a specified start and end `DateTime` range. Returns an empty list if no matches are found.

*   **`Task<IList<Models.Message>> GetPaginatedAsync`**
    Retrieves a subset of messages based on pagination parameters (skip and take). Returns a list containing the requested page of data.

### UserSession Operations

*   **`Task<Models.UserSession?> GetByIdAsync`**
    Retrieves a specific user session by its unique identifier. Returns the `UserSession` object if found; otherwise, returns `null`.

*   **`Task<IList<Models.UserSession>> GetAllAsync`**
    Retrieves all stored user sessions. Returns an empty list if no sessions exist.

*   **`Task<Models.UserSession> CreateAsync`**
    Persists a new `UserSession` instance. Returns the created session object. Throws an exception if the input is null or constraints are violated.

*   **`Task<Models.UserSession> UpdateAsync`**
    Updates an existing `UserSession`. Returns the updated session. Throws an exception if the session does not exist.

*   **`Task<bool> DeleteAsync`**
    Removes a user session by its identifier. Returns `true` if successful; `false` if the ID was not found.

*   **`Task<bool> ExistsAsync`**
    Verifies the existence of a user session by ID. Returns `true` if present, `false` otherwise.

*   **`Task<int> CountAsync`**
    Returns the total count of stored user sessions.

## Usage

### Example 1: Managing Message Lifecycle
This example demonstrates creating a message, verifying its existence, retrieving it by user ID, and subsequently deleting it.

```csharp
using TelegramBotFramework.Repositories;
using TelegramBotFramework.Models;

// Initialize the repository
var repository = new InMemoryMessageRepository();

// Create a new message
var newMessage = new Message 
{ 
    Id = "msg_101", 
    UserId = 12345, 
    ChatId = 67890, 
    Content = "Hello Bot", 
    Status = MessageStatus.Received 
};

var created = await repository.CreateAsync(newMessage);

// Verify existence
bool exists = await repository.ExistsAsync(created.Id); // Returns true

// Retrieve messages by User ID
var userMessages = await repository.GetByUserIdAsync(12345);

// Update the message status
created.Status = MessageStatus.Processed;
await repository.UpdateAsync(created);

// Delete the message
bool deleted = await repository.DeleteAsync(created.Id);
```

### Example 2: Session Management and Pagination
This example illustrates handling user sessions and retrieving a paginated list of messages.

```csharp
using TelegramBotFramework.Repositories;
using TelegramBotFramework.Models;

var repository = new InMemoryMessageRepository();

// Create a user session
var session = new UserSession 
{ 
    Id = "sess_abc", 
    UserId = 12345, 
    State = "WaitingForInput" 
};

await repository.CreateAsync(session);

// Retrieve the session
var retrievedSession = await repository.GetByIdAsync("sess_abc");
if (retrievedSession != null)
{
    retrievedSession.State = "Completed";
    await repository.UpdateAsync(retrievedSession);
}

// Get paginated messages (Skip 0, Take 10)
var page = await repository.GetPaginatedAsync(0, 10);

// Clean up session
await repository.DeleteAsync(session.Id);
```

## Notes

*   **Data Volatility**: As an in-memory implementation, all data is stored in the application's runtime memory. Data is lost immediately upon application restart, process termination, or domain recycle. This makes the class unsuitable for production environments requiring data persistence.
*   **Thread Safety**: The underlying storage mechanism typically relies on standard .NET collections (e.g., `List<T>` or `Dictionary<TKey, TValue>`). While individual method calls are asynchronous, the class does not inherently guarantee thread-safe concurrent modifications without external locking mechanisms. In high-concurrency scenarios, race conditions may occur during simultaneous read/write operations on the same entity.
*   **Null Handling**: Methods ending in `?` (e.g., `GetByIdAsync`) explicitly return `null` to indicate a missing entity rather than throwing an exception. Conversely, `UpdateAsync` and `CreateAsync` generally expect valid objects and may throw `ArgumentNullException` or custom exceptions if preconditions are not met.
*   **Filtering Logic**: Filtering methods (e.g., `GetByCommandAsync`, `GetByDateRangeAsync`) perform in-memory linear scans or lookups. Performance will degrade as the collection size grows, making this implementation inefficient for large datasets compared to database-backed repositories.
