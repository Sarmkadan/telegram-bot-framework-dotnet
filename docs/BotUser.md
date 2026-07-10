# BotUser

The `BotUser` class represents a user entity within the `telegram-bot-framework-dotnet` ecosystem, encapsulating essential Telegram profile information, interaction statistics, role-based authorization status, and extensible metadata for application-specific state tracking and persistence.

## API

### Properties
*   **`TelegramId`** (`long`): The unique identifier assigned to the user by Telegram.
*   **`FirstName`** (`string?`): The user's first name, as provided by Telegram.
*   **`LastName`** (`string?`): The user's last name, as provided by Telegram.
*   **`Username`** (`string?`): The user's Telegram handle.
*   **`PhoneNumber`** (`string?`): The user's registered phone number, if available and shared.
*   **`Status`** (`UserStatus`): The current operational status of the user within the bot framework.
*   **`Role`** (`UserRole`): The authorization role assigned to the user.
*   **`CreatedAt`** (`DateTime`): The timestamp when the user record was first created in the framework.
*   **`UpdatedAt`** (`DateTime`): The timestamp when the user record was last modified.
*   **`LastActivityAt`** (`DateTime?`): The timestamp indicating the most recent interaction by the user.
*   **`IsBot`** (`bool`): Indicates whether the user is a bot account.
*   **`IsPremium`** (`bool`): Indicates whether the user has a Telegram Premium subscription.
*   **`CommandsExecuted`** (`int`): A running tally of the total commands executed by the user.
*   **`MessagesCount`** (`int`): A running tally of the total messages sent by the user.
*   **`Metadata`** (`Dictionary<string, string>?`): A dictionary for storing arbitrary key-value pairs associated with the user.
*   **`Validate`** (`bool`): A flag indicating the current validation status of the user object.

### Methods
*   **`GetDisplayName()`** (`string`): Returns a string suitable for displaying the user, typically prioritized as: Username, then Full Name, or a fallback string if no names are set.
*   **`UpdateActivity()`** (`void`): Updates the `LastActivityAt` property to the current UTC time.
*   **`SetMetadata(string key, string value)`** (`void`): Adds or updates a key-value pair within the `Metadata` dictionary.
*   **`GetMetadata(string key)`** (`string?`): Retrieves the value associated with the specified key in `Metadata`, or returns `null` if the key does not exist.

## Usage

### Example 1: Updating User Activity and Display Name
```csharp
// Retrieve the user from your repository
BotUser user = userRepository.GetById(telegramId);

// Update activity timestamp upon interaction
user.UpdateActivity();

// Log the user's display name
Console.WriteLine($"Processing request from: {user.GetDisplayName()}");
```

### Example 2: Managing Custom Metadata
```csharp
// Assign custom data to the user
user.SetMetadata("preferred_language", "en-US");
user.SetMetadata("subscription_tier", "pro");

// Retrieve and use metadata
string? lang = user.GetMetadata("preferred_language");
if (lang == "en-US")
{
    // Apply English-specific logic
}
```

## Notes

*   **Thread-Safety**: The `BotUser` class is not inherently thread-safe. Concurrent access to the `Metadata` dictionary or simultaneous updates to properties from different threads should be guarded by external locking mechanisms to prevent data corruption or race conditions.
*   **Metadata Initialization**: The `Metadata` property is nullable. Ensure the `Metadata` dictionary is initialized before invoking `SetMetadata` or `GetMetadata` to avoid `NullReferenceException` if the underlying implementation does not handle lazy initialization.
*   **Timestamp Precision**: `CreatedAt`, `UpdatedAt`, and `LastActivityAt` utilize `DateTime` values, which are typically treated as UTC for persistence. Standardize all time-based operations to UTC to ensure consistent behavior across different time zones.
