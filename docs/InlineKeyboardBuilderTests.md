# InlineKeyboardBuilderTests

Unit tests for the `InlineKeyboardBuilder` class, verifying correct construction of Telegram Bot inline keyboard markup and state persistence behavior. Tests cover button addition, row management, serialization, and integration with the conversation state store.

## API

### `Build_WithSingleCallbackButton_CreatesOneRowOneButton()`
Verifies that adding a single callback button results in a keyboard with exactly one row containing one button. No exceptions are thrown.

### `Build_WithUrlButton_SetsTypeAndUrl()`
Ensures that a button with a URL is built with the correct `url` field and `type` set to `"url"`. Does not validate Telegram API compliance.

### `Build_WithSwitchInlineButton_SetsTypeAndQuery()`
Validates that a switch inline query button is constructed with `type` set to `"switch_inline_query"` and the provided query value.

### `Build_AutoWrapsButtonsAtMaxPerRow()`
Confirms that when the number of buttons exceeds the maximum allowed per row (currently 8), the builder automatically creates new rows to maintain compliance with Telegram’s inline keyboard limits.

### `NewRow_ForcesRowBreakBeforeMaxReached()`
Checks that calling `NewRow()` manually inserts a row break regardless of whether the current row has reached the maximum button count.

### `ToButtonLabels_ReturnsTwoDimensionalLabelArray()`
Returns the keyboard markup as a two-dimensional array of strings representing button labels. Useful for testing without invoking the Telegram Bot API.

### `ToMenu_ConvertsMarkupToMenuModel()`
Converts the constructed inline keyboard into a `MenuModel` instance. Useful for framework-level integration testing.

### `Build_WithNoButtons_ThrowsInvalidOperationException()`
Ensures that attempting to build a keyboard with no buttons throws an `InvalidOperationException`.

### `AddButton_WithCallbackDataExceeding64Bytes_ThrowsArgumentException()`
Validates that adding a callback button with callback data exceeding 64 bytes throws an `ArgumentException`.

### `AddButton_WithEmptyText_ThrowsArgumentException()`
Ensures that adding a button with empty text throws an `ArgumentException`.

### `SaveAndLoad_RoundTrip_ReturnsPersistedState()`
Asynchronously tests that saving and loading an inline keyboard state via the in-memory conversation state store returns the original state without modification.

### `LoadStateAsync_WhenNoState_ReturnsNull()`
Asynchronously verifies that loading a non-existent state returns `null`.

### `DeleteStateAsync_RemovesPersistedState()`
Asynchronously confirms that deleting a persisted state removes it from the store.

### `LoadAllActiveStatesAsync_ReturnsOnlyActiveAndWaiting()`
Asynchronously checks that loading all active states returns only those with status `"active"` or `"waiting"`.

### `SaveStateAsync_Overwrites_ExistingEntry()`
Asynchronously ensures that saving a state with an existing key overwrites the previous entry.

### `DeleteStateAsync_OnNonExistentUser_DoesNotThrow()`
Asynchronously validates that deleting a state for a non-existent user does not throw an exception.

### `FileConversationStateStoreTests`
Test fixture for file-based conversation state persistence. Contains tests for saving, loading, and deleting states stored in files.

### `Dispose()`
Releases resources used by the test context. Implements `IDisposable`.

### `SaveAndLoad_RoundTrip_PersistsToFile()`
Asynchronously tests that saving and loading a state via the file-based conversation state store correctly persists and retrieves the state from disk.

### `DeleteStateAsync_RemovesFile()`
Asynchronously verifies that deleting a state removes the corresponding file from the file system.

## Usage

### Example 1: Building a Simple Inline Keyboard
