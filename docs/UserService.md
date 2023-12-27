# UserService

The `UserService` class provides the primary interface for managing user data within the `telegram-bot-framework-dotnet` ecosystem. It handles the lifecycle of `BotUser` entities, including creation, retrieval, updates, and deletion, while offering specialized methods for administrative tasks such as banning, promoting, and tracking user activity. This service abstracts direct database interactions, ensuring consistent data access patterns and maintaining referential integrity between Telegram user IDs and internal bot user records.

## API

### Constructors

#### `public UserService`
Initializes a new instance of the `UserService` class. This constructor typically injects required dependencies such as database contexts or repository interfaces to facilitate data operations.

### User Retrieval and Creation

#### `public async Task<Models.BotUser> GetOrCreateUserAsync`
Retrieves an existing user based on provided criteria or creates a new `BotUser` record if one does not exist.
*   **Parameters**: Varies by overload (typically includes Telegram ID or user initialization data).
*   **Return Value**: Returns a `Models.BotUser` instance. If the user was newly created, it is returned with generated properties populated.
*   **Exceptions**: May throw exceptions related to database connectivity or constraint violations during insertion.

#### `public async Task<Models.BotUser?> GetUserByIdAsync`
Retrieves a user by their internal system identifier.
*   **Parameters**: `id` (The internal unique identifier of the user).
*   **Return Value**: Returns the `Models.BotUser` if found; otherwise, returns `null`.
*   **Exceptions**: Throws if the provided ID format is invalid.

#### `public async Task<Models.BotUser?> GetUserByTelegramIdAsync`
Retrieves a user by their native Telegram user ID.
*   **Parameters**: `telegramId` (The unique identifier assigned by Telegram).
*   **Return Value**: Returns the `Models.BotUser` if found; otherwise, returns `null`.
*   **Exceptions**: Throws if the provided Telegram ID is invalid.

### User Updates and Deletion

#### `public async Task<Models.BotUser> UpdateUserAsync`
Updates an existing user record with new information.
*   **Parameters**: Varies by overload (typically the `BotUser` entity or specific fields to update).
*   **Return Value**: Returns the updated `Models.BotUser` instance reflecting the persisted changes.
*   **Exceptions**: Throws if the user does not exist or if concurrency conflicts occur.

#### `public async Task<bool> DeleteUserAsync`
Permanently removes a user record from the system.
*   **Parameters**: Typically accepts a user ID or `BotUser` instance.
*   **Return Value**: Returns `true` if the deletion was successful; `false` if the user did not exist.
*   **Exceptions**: May throw if foreign key constraints prevent deletion.

### Search and Filtering

#### `public async Task<IList<Models.BotUser>> SearchUsersAsync`
Searches for users based on text criteria or specific filters.
*   **Parameters**: Search query string or filter object.
*   **Return Value**: A list of `Models.BotUser` matching the search criteria. Returns an empty list if no matches are found.
*   **Exceptions**: Throws if the search query is malformed.

#### `public async Task<IList<Models.BotUser>> GetUsersByStatusAsync`
Retrieves a list of users filtered by their current status (e.g., Active, Banned, Inactive).
*   **Parameters**: `status` (The user status enum or string).
*   **Return Value**: A list of `Models.BotUser` having the specified status.
*   **Exceptions**: Throws if the provided status is invalid.

### Administrative Actions

#### `public async Task<bool> BanUserAsync`
Marks a user as banned, restricting their ability to interact with the bot.
*   **Parameters**: User identifier or `BotUser` instance.
*   **Return Value**: Returns `true` if the ban was applied successfully; `false` otherwise.
*   **Exceptions**: Throws if the user does not exist.

#### `public async Task<bool> UnbanUserAsync`
Removes the ban status from a user, restoring their access.
*   **Parameters**: User identifier or `BotUser` instance.
*   **Return Value**: Returns `true` if the unban was successful; `false` if the user was not banned or does not exist.
*   **Exceptions**: Throws if the user does not exist.

#### `public async Task<IList<Models.BotUser>> GetAdministratorsAsync`
Retrieves a list of all users currently holding administrator privileges.
*   **Parameters**: None.
*   **Return Value**: A list of `Models.BotUser` flagged as administrators.
*   **Exceptions**: None typical, subject to database availability.

#### `public async Task<bool> PromoteToAdminAsync`
Grants administrator privileges to a specific user.
*   **Parameters**: User identifier or `BotUser` instance.
*   **Return Value**: Returns `true` if promotion was successful; `false` if the user was already an admin or does not exist.
*   **Exceptions**: Throws if the user does not exist.

#### `public async Task<bool> DemoteAdminAsync`
Revokes administrator privileges from a specific user.
*   **Parameters**: User identifier or `BotUser` instance.
*   **Return Value**: Returns `true` if demotion was successful; `false` if the user was not an admin or does not exist.
*   **Exceptions**: Throws if the user does not exist.

### Statistics and Activity

#### `public async Task<int> GetTotalUsersCountAsync`
Calculates the total number of users registered in the system.
*   **Parameters**: None.
*   **Return Value**: An integer representing the total count.
*   **Exceptions**: None typical.

#### `public async Task<int> GetActiveUsersCountAsync`
Calculates the number of users currently in an active state.
*   **Parameters**: None.
*   **Return Value**: An integer representing the count of active users.
*   **Exceptions**: None typical.

#### `public async Task RecordUserActivityAsync`
Logs a timestamped activity event for a specific user, updating their last seen information.
*   **Parameters**: User identifier or `BotUser` instance.
*   **Return Value**: Returns `void` (wrapped in Task).
*   **Exceptions**: Throws if the user does not exist.

## Usage

### Example 1: User Onboarding and Activity Tracking
This example demonstrates retrieving or creating a user upon receiving a message and recording their activity.

```csharp
public async Task HandleMessageAsync(long telegramId, string username)
{
    // Initialize the service (assuming dependency injection or manual instantiation)
    var userService = new UserService();

    // Get existing user or create a new one
    var user = await userService.GetOrCreateUserAsync(telegramId, username);

    if (user != null)
    {
        // Record that the user is currently active
        await userService.RecordUserActivityAsync(user.Id);

        if (user.IsBanned)
        {
            Console.WriteLine($"User {user.Id} is banned and cannot proceed.");
            return;
        }

        Console.WriteLine($"Welcome back, {user.Username}!");
    }
}
```

### Example 2: Administrative Moderation
This example illustrates promoting a user to admin and retrieving the updated list of administrators.

```csharp
public async Task PromoteUserAsync(long targetTelegramId, long executorTelegramId)
{
    var userService = new UserService();

    // Verify executor is an admin (simplified check)
    var executor = await userService.GetUserByTelegramIdAsync(executorTelegramId);
    if (executor == null || !executor.IsAdmin)
    {
        throw new UnauthorizedAccessException("Only administrators can promote users.");
    }

    var targetUser = await userService.GetUserByTelegramIdAsync(targetTelegramId);
    if (targetUser == null)
    {
        Console.WriteLine("Target user not found.");
        return;
    }

    // Promote the user
    bool success = await userService.PromoteToAdminAsync(targetUser.Id);

    if (success)
    {
        // Fetch updated list of administrators
        var admins = await userService.GetAdministratorsAsync();
        Console.WriteLine($"Promotion successful. Total admins: {admins.Count}");
    }
    else
    {
        Console.WriteLine("Failed to promote user.");
    }
}
```

## Notes

*   **Null Handling**: Methods returning singular entities (`GetUserByIdAsync`, `GetUserByTelegramIdAsync`) return `null` if no record is found rather than throwing an exception. Callers must handle null checks appropriately.
*   **Idempotency**: Actions such as `BanUserAsync`, `UnbanUserAsync`, `PromoteToAdminAsync`, and `DemoteAdminAsync` are designed to be idempotent. Attempting to ban an already banned user will return `false` but will not throw an error, allowing for safe retry logic.
*   **Concurrency**: As an asynchronous service interacting with a database, `UserService` is generally stateless regarding individual requests. However, concurrent updates to the same user entity (e.g., simultaneous `UpdateUserAsync` calls) may result in concurrency conflicts depending on the underlying database provider's isolation levels. Implementations should be prepared to handle database concurrency exceptions.
*   **Data Consistency**: `GetOrCreateUserAsync` ensures that a user record exists before returning. However, in high-concurrency scenarios, race conditions during the "create" phase might occur if the underlying database does not enforce unique constraints on Telegram IDs properly.
*   **Activity Recording**: `RecordUserActivityAsync` is intended to be called frequently. Implementations should ensure this method is optimized for write-heavy loads to prevent bottlenecks during high-traffic periods.
