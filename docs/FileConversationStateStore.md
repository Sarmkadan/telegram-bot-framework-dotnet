# FileConversationStateStore

The `FileConversationStateStore` provides a mechanism for persisting and retrieving user conversation states using the local file system. It is intended for scenarios where lightweight, file-based storage is sufficient for maintaining the lifecycle of conversation flows, ensuring that state information persists across bot restarts.

## API

### SaveStateAsync
Persists the provided `UserFlowState` instance to the configured storage location.
- **Parameters:** `UserFlowState state`
- **Throws:** Throws `IOException` if the file system is inaccessible or `UnauthorizedAccessException` if write permissions are insufficient.

### LoadStateAsync
Retrieves the `UserFlowState` associated with a specific conversation identifier.
- **Parameters:** `string conversationId`
- **Returns:** A `UserFlowState` object if found; otherwise, `null`.
- **Throws:** Throws `IOException` if the storage file cannot be read.

### DeleteStateAsync
Removes the persisted `UserFlowState` associated with the specified conversation identifier from the file system.
- **Parameters:** `string conversationId`
- **Throws:** Throws `FileNotFoundException` if no state exists for the given ID, or `IOException` during file deletion operations.

### LoadAllActiveStatesAsync
Retrieves a collection of all `UserFlowState` objects currently persisted in the storage directory.
- **Returns:** An `IReadOnlyList<UserFlowState>` containing all active states.
- **Throws:** Throws `IOException` if the storage directory cannot be accessed or enumerated.

## Usage

### Saving a Conversation State
```csharp
var store = new FileConversationStateStore(storagePath);
var state = new UserFlowState { ConversationId = "123", Step = "start" };

await store.SaveStateAsync(state);
```

### Retrieving and Deleting a State
```csharp
var store = new FileConversationStateStore(storagePath);
var state = await store.LoadStateAsync("123");

if (state != null)
{
    // Process state...
    await store.DeleteStateAsync("123");
}
```

## Notes

- **Thread Safety:** This class does not implement internal locking. Simultaneous read and write operations on the same conversation identifier from different threads or processes may result in file access conflicts or corrupted data. Ensure external synchronization if concurrent access to the same storage files is required.
- **File System Performance:** Performance is dependent on the underlying disk I/O. For high-throughput bots, this implementation may become a bottleneck compared to in-memory or database-backed stores.
- **Consistency:** Operations are not atomic at the file system level. A failure during a write operation may result in a partially written or empty state file.
