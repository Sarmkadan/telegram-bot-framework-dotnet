# UserSession

The `UserSession` class manages the state and contextual information for an individual user's interaction within the `telegram-bot-framework-dotnet`. It tracks conversational progress, stores temporary user data, maintains command history, and handles session lifecycle management, including activity tracking and expiration.

## API

### Properties

*   **`string SessionId`**: A unique identifier for the session.
*   **`long UserId`**: The unique identifier of the user associated with this session.
*   **`long ChatId`**: The unique identifier of the chat where this session is active.
*   **`SessionState State`**: The current state of the conversation (e.g., awaiting input, menu selection).
*   **`string CurrentContext`**: The active conversational context for the session.
*   **`string? CurrentMenuId`**: The identifier of the current menu being displayed, if any.
*   **`DateTime CreatedAt`**: The timestamp when the session was initialized.
*   **`DateTime? LastActivityAt`**: The timestamp of the most recent user interaction.
*   **`DateTime? ExpiresAt`**: The timestamp indicating when the session becomes invalid. If `null`, the session does not have an expiration.
*   **`Dictionary<string, string>? ContextData`**: A collection of key-value pairs storing temporary session-specific data.
*   **`List<string>? CommandHistory`**: A list of commands previously executed within this session.
*   **`int InteractionCount`**: The total number of interactions recorded in this session.
*   **`string? UserInput`**: The raw text or last payload received from the user.
*   **`bool IsExpired`**: Indicates whether the session has passed its expiration time.

### Methods

*   **`void UpdateActivity()`**: Refreshes the `LastActivityAt` timestamp to the current time.
*   **`TimeSpan GetDuration()`**: Returns the total duration of the session from `CreatedAt` to the current time.
*   **`void SetContextData(string key, string value)`**: Adds or updates a key-value pair in `ContextData`. Initializes the dictionary if it is currently `null`.
*   **`string? GetContextData(string key)`**: Retrieves the value associated with the specified key in `ContextData`. Returns `null` if the key does not exist or if `ContextData` is `null`.
*   **`bool RemoveContextData(string key)`**: Removes the specified key from `ContextData`. Returns `true` if the key was found and removed, otherwise `false`.
*   **`void ClearContextData()`**: Removes all entries from `ContextData`.

## Usage

### Example 1: Updating and Retrieving Context Data

```csharp
// Updating context data during an interaction
userSession.SetContextData("preferred_language", "en-US");
userSession.UpdateActivity();

// Retrieving context data
var lang = userSession.GetContextData("preferred_language");
if (lang != null)
{
    Console.WriteLine($"User preference: {lang}");
}
```

### Example 2: Checking Session Expiration

```csharp
if (userSession.IsExpired)
{
    Console.WriteLine("Session has expired. Cleaning up...");
    // Logic to reset or terminate the user session
}
else
{
    Console.WriteLine($"Session active for: {userSession.GetDuration().TotalMinutes} minutes.");
}
```

## Notes

*   **Thread Safety**: The `UserSession` class is not inherently thread-safe. If multiple threads access or modify `ContextData` or `CommandHistory` concurrently, external synchronization mechanisms (such as `lock` statements) must be implemented.
*   **Nullability**: Members marked with `?` can be `null`. Proper null-checking is required before accessing `ContextData`, `CommandHistory`, or using properties that may not be initialized.
*   **Expiration**: The `IsExpired` property relies on the system clock. If `ExpiresAt` is not set, `IsExpired` will consistently return `false`.
