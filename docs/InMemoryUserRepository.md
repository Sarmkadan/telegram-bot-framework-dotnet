# InMemoryUserRepository

The `InMemoryUserRepository` class provides a transient, non-persistent implementation of data access logic for managing `BotUser` and `Command` entities within the `telegram-bot-framework-dotnet` ecosystem. Designed primarily for testing, prototyping, or single-instance ephemeral deployments, this repository stores all state in memory using concurrent-safe collections, offering asynchronous CRUD operations, filtering, pagination, and existence checks without requiring an external database connection.

## API

The repository exposes asynchronous methods for managing two distinct entity types: `Models.BotUser` and `Models.Command`. Note that method names are overloaded between these two entity types; in a compiled context, these are distinguished by their return types and parameter signatures.

### BotUser Operations

*   **`Task<Models.BotUser?> GetByIdAsync(Guid id)`**
    Retrieves a specific user by their unique internal identifier. Returns the `BotUser` instance if found; otherwise, returns `null`. Does not throw for missing entities.

*   **`Task<IList<Models.BotUser>> GetAllAsync()`**
    Retrieves a list containing all users currently stored in the repository. Returns an empty list if no users exist.

*   **`Task<Models.BotUser> CreateAsync(Models.BotUser user)`**
    Adds a new user to the repository. Assigns a new unique identifier if not already set. Returns the created entity. Throws an exception if a user with the same ID already exists.

*   **`Task<Models.BotUser> UpdateAsync(Models.BotUser user)`**
    Updates an existing user record with the provided data. Returns the updated entity. Throws an exception if the user ID does not exist in the repository.

*   **`Task<bool> DeleteAsync(Guid id)`**
    Removes a user by their unique identifier. Returns `true` if the user was found and deleted; returns `false` if the ID was not found.

*   **`Task<bool> ExistsAsync(Guid id)`**
    Checks whether a user with the specified identifier exists. Returns `true` if found, `false` otherwise.

*   **`Task<int> CountAsync()`**
    Returns the total number of users currently stored in the repository.

*   **`Task<Models.BotUser?> GetByTelegramIdAsync(long telegramId)`**
    Retrieves a user by their specific Telegram platform ID. Returns the `BotUser` if found; otherwise, returns `null`.

*   **`Task<Models.BotUser?> GetByUsernameAsync(string username)`**
    Retrieves a user by their username. Returns the `BotUser` if found; otherwise, returns `null`. Comparison is typically case-insensitive depending on internal implementation.

*   **`Task<IList<Models.BotUser>> GetByStatusAsync(UserStatus status)`**
    Retrieves a list of users matching the specified status enumeration. Returns an empty list if no matches are found.

*   **`Task<IList<Models.BotUser>> GetByRoleAsync(UserRole role)`**
    Retrieves a list of users matching the specified role enumeration. Returns an empty list if no matches are found.

*   **`Task<IList<Models.BotUser>> SearchAsync(string query)`**
    Performs a text-based search across user properties (typically username or display name). Returns a list of matching users.

*   **`Task<IList<Models.BotUser>> GetPaginatedAsync(int pageNumber, int pageSize)`**
    Retrieves a subset of users based on pagination parameters. `pageNumber` is zero-indexed or one-indexed based on framework convention (typically zero), and `pageSize` defines the count per page. Returns a list of users for the requested page.

### Command Operations

*   **`Task<Models.Command?> GetByIdAsync(Guid id)`**
    Retrieves a specific command definition by its unique identifier. Returns the `Command` instance if found; otherwise, returns `null`.

*   **`Task<IList<Models.Command>> GetAllAsync()`**
    Retrieves a list of all registered command definitions.

*   **`Task<Models.Command> CreateAsync(Models.Command command)`**
    Registers a new command definition. Returns the created entity. Throws an exception if a command with the same ID already exists.

*   **`Task<Models.Command> UpdateAsync(Models.Command command)`**
    Updates an existing command definition. Returns the updated entity. Throws an exception if the command ID does not exist.

*   **`Task<bool> DeleteAsync(Guid id)`**
    Removes a command definition by its unique identifier. Returns `true` if successful, `false` if the ID was not found.

*   **`Task<bool> ExistsAsync(Guid id)`**
    Checks for the existence of a command definition by ID. Returns `true` if found, `false` otherwise.

*   **`Task<int> CountAsync()`**
    Returns the total number of command definitions stored.

## Usage

### Example 1: Managing Bot Users
This example demonstrates creating a user, retrieving them by Telegram ID, updating their status, and handling pagination.

```csharp
var repository = new InMemoryUserRepository();

// Create a new user
var newUser = new Models.BotUser 
{ 
    TelegramId = 123456789, 
    Username = "johndoe", 
    Status = UserStatus.Active,
    Role = UserRole.Member
};

var createdUser = await repository.CreateAsync(newUser);

// Retrieve by Telegram ID
var fetchedUser = await repository.GetByTelegramIdAsync(123456789);
if (fetchedUser != null)
{
    // Update the user's status
    fetchedUser.Status = UserStatus.Banned;
    await repository.UpdateAsync(fetchedUser);
}

// Get paginated list of all users
var allUsersPage = await repository.GetPaginatedAsync(0, 10);
Console.WriteLine($"Retrieved {allUsersPage.Count} users on page 1.");
```

### Example 2: Managing Commands
This example illustrates registering a command, verifying its existence, and deleting it.

```csharp
var repository = new InMemoryUserRepository();

// Create a new command
var helpCommand = new Models.Command 
{ 
    Name = "/help", 
    Description = "Shows help information" 
};

var createdCommand = await repository.CreateAsync(helpCommand);

// Check existence
bool exists = await repository.ExistsAsync(createdCommand.Id);
if (exists)
{
    // Retrieve and verify
    var retrievedCommand = await repository.GetByIdAsync(createdCommand.Id);
    Console.WriteLine($"Command found: {retrievedCommand?.Name}");
    
    // Delete the command
    bool deleted = await repository.DeleteAsync(createdCommand.Id);
    Console.WriteLine($"Deletion successful: {deleted}");
}
```

## Notes

*   **Data Persistence**: As an in-memory implementation, all data is lost when the application process terminates or the `InMemoryUserRepository` instance is garbage collected. This class is not suitable for production environments requiring durable storage.
*   **Thread Safety**: The implementation utilizes concurrent collections internally to ensure thread safety for read and write operations across multiple asynchronous tasks. However, complex multi-step transactions (e.g., check-then-act sequences like `ExistsAsync` followed by `CreateAsync`) are not atomic and may require external locking if strict consistency is required under high concurrency.
*   **Overloaded Signatures**: The API contains method name overloads for `BotUser` and `Command` entities (e.g., `CreateAsync`, `DeleteAsync`). In C#, these are resolved at compile time based on the argument types passed (`Models.BotUser` vs `Models.Command`) and the expected return type. Care must be taken when using dynamic invocation or reflection to distinguish between these methods.
*   **Null Handling**: Retrieval methods ending in `Async` that return a single entity (e.g., `GetByIdAsync`, `GetByUsernameAsync`) return `null` if the entity is not found, rather than throwing an exception. Methods returning lists (e.g., `GetAllAsync`, `SearchAsync`) return an empty list rather than `null` when no results are found.
*   **Exception Behavior**: `CreateAsync` and `UpdateAsync` will throw an exception if integrity constraints are violated (e.g., creating a duplicate ID or updating a non-existent record). `DeleteAsync` handles missing records gracefully by returning `false`.
