# BotConfigurationAdditionalTests

Unit tests for additional validation and configuration scenarios of `BotConfiguration` in the Telegram Bot Framework. These tests cover edge cases around admin IDs, custom settings, session timeouts, and validation rules for bot credentials and limits.

## API

### `BotConfiguration_WithNullAdminIds_ListIsInitialized`
Ensures that when `AdminIds` is set to `null`, the internal list is initialized to an empty collection rather than remaining `null`.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: None

---

### `BotConfiguration_WithNullCustomSettings_DictionaryIsInitialized`
Verifies that when `CustomSettings` is set to `null`, the internal dictionary is initialized to an empty dictionary rather than remaining `null`.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: None

---

### `BotConfiguration_WithEmptyCustomSettings_DictionaryIsInitialized`
Confirms that when `CustomSettings` is set to an empty dictionary, the internal state remains consistent and no exceptions are thrown.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: None

---
### `IsAdmin_WithEmptyAdminIds_ReturnsFalse`
Tests that calling `IsAdmin` with an empty collection of admin IDs returns `false` when no owner ID is set.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: None

---
### `IsAdmin_WithEmptyAdminIdsAndOwnerId_ReturnsTrueForOwner`
Ensures that when `AdminIds` is empty but `OwnerId` is set, `IsAdmin` returns `true` only for the owner ID.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: None

---
### `GetSessionTimeout_WithDefaultValue_Returns30Minutes`
Validates that the default session timeout is 30 minutes when no custom value is provided.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: None

---
### `GetSessionTimeout_WithCustomValue_ReturnsCorrectTimeSpan`
Checks that a custom session timeout value is correctly returned by `GetSessionTimeout`.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: None

---
### `Validate_WithWhitespaceBotUsername_ThrowsException`
Ensures that `Validate` throws an exception when the bot username contains only whitespace.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: `ArgumentException` if the username is whitespace

---
### `Validate_WithWhitespaceBotToken_ThrowsException`
Confirms that `Validate` throws an exception when the bot token contains only whitespace.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: `ArgumentException` if the token is whitespace

---
### `Validate_WithSingleCharacterBotToken_ThrowsException`
Validates that `Validate` throws an exception when the bot token is a single character.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: `ArgumentException` if the token length is less than 2 characters

---
### `Validate_WithMaxConcurrentRequests_ReturnsTrue`
Tests that `Validate` returns `true` when `MaxConcurrentRequests` is set to a valid maximum value.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: None

---
### `Validate_WithOneMaxConcurrentRequests_ReturnsTrue`
Ensures that `Validate` returns `true` when `MaxConcurrentRequests` is set to 1.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: None

---
### `Validate_WithOneSessionTimeout_ReturnsTrue`
Checks that `Validate` returns `true` when `SessionTimeout` is set to 1 minute.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: None

---
### `SetCustomSetting_OverwritesExistingValue`
Verifies that calling `SetCustomSetting` with an existing key overwrites the previous value.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: None

---
### `RemoveAdmin_WithEmptyAdminIds_ReturnsFalse`
Ensures that calling `RemoveAdmin` on an empty admin list returns `false`.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: None

---
### `RemoveAdmin_RemovesOnlySpecifiedAdmin`
Confirms that `RemoveAdmin` removes only the specified admin ID and leaves others intact.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: None

---

## Usage

### Example 1: Validating Bot Configuration with Edge Cases
