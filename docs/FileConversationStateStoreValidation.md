# FileConversationStateStoreValidation

This static class provides validation helpers for `FileConversationStateStore` instances. It ensures that the store’s configuration is correct before it is used by the framework.

## API

### `public static IReadOnlyList<string> Validate(this FileConversationStateStore store)`

- **Purpose**: Returns a list of validation error messages for the given store. An empty list indicates the store is valid.  
- **Parameters**:  
  - `store`: The `FileConversationStateStore` instance to validate.  
- **Return value**: `IReadOnlyList<string>` containing error messages. If the list is empty, the store passes validation.  
- **Throws**: None. Validation errors are returned as strings.

### `public static bool IsValid(this FileConversationStateStore store)`

- **Purpose**: Convenience method that returns `true` if the store has no validation errors.  
- **Parameters**:  
  - `store`: The `FileConversationStateStore` instance to check.  
- **Return value**: `true` if `Validate(store)` returns an empty list; otherwise `false`.  
- **Throws**: None.

### `public static void EnsureValid(this FileConversationStateStore store)`

- **Purpose**: Throws an `InvalidOperationException` if the store is not valid.  
- **Parameters**:  
  - `store`: The `FileConversationStateStore` instance to validate.  
- **Return value**: None.  
- **Throws**: `InvalidOperationException` containing the concatenated validation error messages.

## Usage

