# BotConfigurationTests

The `BotConfigurationTests` class contains unit tests for the `BotConfiguration` type, which holds settings for a Telegram bot (token, username, admin list, session timeout, and concurrency limits). Each test verifies a specific behavior of `BotConfiguration`—default values, validation rules, admin management, and property access—ensuring the configuration object behaves correctly under normal, boundary, and invalid conditions.

## API

All methods are public, return `void`, and take no parameters. They are designed to be executed by a test framework (e.g., xUnit, NUnit). A test passes if all assertions within it succeed; otherwise it fails with a descriptive message.

- **`BotConfiguration_DefaultValues_AreCorrect`**  
  Verifies that a newly created `BotConfiguration` instance has expected default values (e.g., `BotToken` is `null`, `BotUsername` is `null`, `SessionTimeout` is a positive default, `MaxConcurrentRequests` is a positive default, `AdminIds` is `null`).

- **`Validate_WithValidConfiguration_ReturnsTrue`**  
  Creates a `BotConfiguration` with valid, non‑empty `BotToken` and `BotUsername`, then calls `Validate()` and asserts it returns `true`.

- **`Validate_WithEmptyBotToken_ThrowsException`**  
  Sets `BotToken` to an empty string and calls `Validate()`. Asserts that an `ArgumentException` (or a derived exception) is thrown.

- **`Validate_WithEmptyBotUsername_ThrowsException`**  
  Sets `BotUsername` to an empty string and calls `Validate()`. Asserts that an `ArgumentException` is thrown.

- **`Validate_WithWhitespaceBotToken_ThrowsException`**  
  Sets `BotToken` to a whitespace‑only string and calls `Validate()`. Asserts that an `ArgumentException` is thrown.

- **`Validate_WithZeroSessionTimeout_ThrowsException`**  
  Sets `SessionTimeout` to `TimeSpan.Zero` and calls `Validate()`. Asserts that an `ArgumentException` is thrown.

- **`Validate_WithNegativeSessionTimeout_ThrowsException`**  
  Sets `SessionTimeout` to a negative `TimeSpan` and calls `Validate()`. Asserts that an `ArgumentException` is thrown.

- **`Validate_WithZeroMaxConcurrentRequests_ThrowsException`**  
  Sets `MaxConcurrentRequests` to `0` and calls `Validate()`. Asserts that an `ArgumentException` is thrown.

- **`Validate_WithNegativeMaxConcurrentRequests_ThrowsException`**  
  Sets `MaxConcurrentRequests` to a negative integer and calls `Validate()`. Asserts that an `ArgumentException` is thrown.

- **`IsAdmin_WithOwnerId_ReturnsTrue`**  
  Sets `OwnerId` to a specific value, does not add any admin IDs, then calls `IsAdmin()` with that same value. Asserts the method returns `true`.

- **`IsAdmin_WithAdminId_ReturnsTrue`**  
  Adds a user ID to the `AdminIds` list, then calls `IsAdmin()` with that ID. Asserts the method returns `true`.

- **`IsAdmin_WithNonAdminId_ReturnsFalse`**  
  Sets `OwnerId` to one value and `AdminIds` to a list containing a different value, then calls `IsAdmin()` with a third, unrelated ID. Asserts the method returns `false`.

- **`IsAdmin_WithNullAdminIds_ReturnsFalse`**  
  Leaves `AdminIds` as `null`, sets `OwnerId` to a value, then calls `IsAdmin()` with a different ID. Asserts the method returns `false`.

- **`AddAdmin_WithNewAdmin_AddsToList`**  
  Calls `AddAdmin()` with a user ID when `AdminIds` is `null`. Verifies that `AdminIds` is initialized and contains the added ID.

- **`AddAdmin_WithExistingAdmin_DoesNotAddDuplicate`**  
  Adds a user ID to `AdminIds`, then calls `AddAdmin()` again with the same ID. Verifies that the list still contains only one occurrence of that ID.

- **`AddAdmin_WithNullAdminIds_InitializesList`**  
  Calls `AddAdmin()` with a user ID when `AdminIds` is `null`. Verifies that `AdminIds` is no longer `null` and contains the ID.

- **`RemoveAdmin_WithExistingAdmin_ReturnsTrueAndRemoves`**  
  Adds a user ID to `AdminIds`, then calls `RemoveAdmin()` with that ID. Asserts the method returns `true` and that the ID is no longer in the list.

- **`RemoveAdmin_WithNonExistingAdmin_ReturnsFalse`**  
  Calls `RemoveAdmin()` with a user ID that is not in `AdminIds`. Asserts the method returns `false`.

- **`RemoveAdmin_WithNullAdminIds_ReturnsFalse`**  
  Calls `RemoveAdmin()` with any user ID when `AdminIds` is `null`. Asserts the method returns `false`.

- **`GetSessionTimeout_ReturnsCorrectTimeSpan`**  
  Sets `SessionTimeout` to a specific `TimeSpan`, then calls `GetSessionTimeout()` and asserts the returned value equals the set value.

## Usage

The following examples demonstrate typical usage of the `BotConfiguration` class that these tests validate.

**Example 1 – Creating and validating a configuration**

```csharp
var config = new BotConfiguration
{
    BotToken = "123456:ABC-DEF1234ghIkl-zyx57W2v1u123ew11",
    BotUsername = "MyBot",
    SessionTimeout = TimeSpan.FromMinutes(5),
    MaxConcurrentRequests = 10
};

bool isValid = config.Validate(); // returns true

config.BotToken = "";
try
{
    config.Validate();
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}
```

**Example 2 – Managing admin users**

```csharp
var config = new BotConfiguration
{
    OwnerId = 100L
};

// Add an admin
config.AddAdmin(200L);
config.AddAdmin(300L);

// Check membership
bool isOwnerAdmin = config.IsAdmin(100L);   // true (owner is always admin)
bool isAdmin = config.IsAdmin(200L);        // true
bool isNotAdmin = config.IsAdmin(999L);     // false

// Remove an admin
bool removed = config.RemoveAdmin(200L);    // true
bool removedAgain = config.RemoveAdmin(200L); // false (already removed)
```

## Notes

- **Edge cases** – The tests cover empty and whitespace‑only tokens, zero and negative timeouts, zero and negative concurrency limits, null admin lists, duplicate admin additions, and removal of non‑existing admins. These are the primary boundary conditions for `BotConfiguration`.
- **Thread safety** – The `BotConfiguration` class is not inherently thread‑safe. Mutations to `AdminIds` (via `AddAdmin` and `RemoveAdmin`) are not synchronized. If the same instance is accessed concurrently from multiple threads, external locking should be used.
- **Owner vs. admin** – The `IsAdmin` method treats the owner ID as an implicit admin even if it is not present in the `AdminIds` list. The tests confirm this behavior.
- **Validation order** – The `Validate` method checks all properties and throws on the first invalid value encountered. The tests do not specify the exact order, but each invalid case is tested independently.
