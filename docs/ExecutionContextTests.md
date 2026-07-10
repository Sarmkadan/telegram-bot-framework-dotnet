# ExecutionContextTests

Unit tests for the `ExecutionContext` class, verifying initialization, state management, error handling, validation, and processing control in a Telegram bot framework context.

## API

### `Constructor_WithDefaultValues_InitializesCorrectly`
Ensures that a new `ExecutionContext` instance is initialized with default values: empty errors list, empty states dictionary, zero duration, and `IsStopped` set to `false`.

### `Constructor_WithUserAndSession_StoresReferences`
Verifies that the constructor correctly stores the provided `IUser` and `ISession` references in the context.

### `AddError_AddsErrorToErrorsList`
Confirms that calling `AddError` appends the given error to the internal errors list.

### `AddError_WithNullError_DoesNotAdd`
Ensures that passing a `null` error to `AddError` does not modify the errors list.

### `AddError_WithEmptyError_DoesNotAdd`
Ensures that passing an empty or whitespace-only error string to `AddError` does not modify the errors list.

### `SetState_AddsStateToStatesDictionary`
Validates that `SetState` adds a new key-value pair to the internal states dictionary.

### `SetState_OverwritesExistingState`
Ensures that `SetState` overwrites an existing state value for a given key.

### `SetState_WithNullKey_DoesNotAdd`
Confirms that calling `SetState` with a `null` key does not modify the states dictionary.

### `SetState_WithEmptyKey_DoesNotAdd`
Confirms that calling `SetState` with an empty or whitespace-only key does not modify the states dictionary.

### `GetState_WithExistingKey_ReturnsValue`
Verifies that `GetState` returns the correct value when the key exists in the states dictionary.

### `GetState_WithNonExistingKey_ReturnsDefault`
Ensures that `GetState` returns the default value (`null`) when the key does not exist.

### `GetState_WithWrongType_ReturnsDefault`
Validates that `GetState` returns the default value when the stored value is of an incompatible type.

### `Validate_WithValidContext_ReturnsTrue`
Confirms that `Validate` returns `true` when the context contains valid user, session, and message data.

### `Validate_WithNullUser_AddsErrorAndReturnsFalse`
Ensures that `Validate` adds an appropriate error and returns `false` when the user is `null`.

### `Validate_WithNullSession_AddsErrorAndReturnsFalse`
Ensures that `Validate` adds an appropriate error and returns `false` when the session is `null`.

### `Validate_WithNullMessage_AddsErrorAndReturnsFalse`
Ensures that `Validate` adds an appropriate error and returns `false` when the message is `null`.

### `Validate_WithZeroUserId_AddsErrorAndReturnsFalse`
Validates that `Validate` adds an error and returns `false` when the user ID is zero.

### `Validate_WithZeroChatId_AddsErrorAndReturnsFalse`
Validates that `Validate` adds an error and returns `false` when the chat ID is zero.

### `StopProcessing_SetsIsStoppedToTrue`
Confirms that calling `StopProcessing` sets the `IsStopped` flag to `true`.

### `GetDuration_ReturnsTimeSpanSinceCreation`
Verifies that `GetDuration` returns a `TimeSpan` representing the elapsed time since the context was created.

## Usage

### Example 1: Validating and processing a Telegram update
