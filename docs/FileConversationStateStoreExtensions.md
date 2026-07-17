# FileConversationStateStoreExtensions

Provides file‑system based helper methods for persisting and retrieving `UserFlowState` objects used by the conversation flow infrastructure. The extension methods operate on a designated folder where each flow state is stored as an individual JSON file, enabling simple, durable storage without external dependencies.

## API

### `ExistsAsync`

```csharp
public static async Task<bool> ExistsAsync(
    string flowId,
    string userId,
    CancellationToken cancellationToken = default)
```

**Purpose** – Determines whether a state file exists for the given flow and user.  
**Parameters**  
- `flowId`: Identifier of the conversation flow. Must not be `null` or whitespace.  
- `userId`: Identifier of the user whose state is being checked. Must not be `null` or whitespace.  
- `cancellationToken`: Optional token to cancel the operation.  

**Return Value** – `true` if a state file matching `flowId` and `userId` exists; otherwise `false`.  
**Exceptions**  
- `ArgumentNullException` – If `flowId` or `userId` is `null`.  
- `ArgumentException` – If `flowId` or `userId` consists only of whitespace.  
- `IOException` – If an I/O error occurs while accessing the storage directory.  
- `UnauthorizedAccessException` – If the process lacks permission to read the directory or file.  
- `OperationCanceledException` – If `cancellationToken` is triggered.

### `TryLoadStateAsync`

```csharp
public static async Task<UserFlowState?> TryLoadStateAsync(
    string flowId,
    string userId,
    CancellationToken cancellationToken = default)
```

**Purpose** – Attempts to deserialize and return the state stored for the specified flow and user.  
**Parameters** – Same as `ExistsAsync`.  

**Return Value** – The deserialized `UserFlowState` instance if the file exists and contains valid JSON; otherwise `null`.  
**Exceptions**  
- `ArgumentNullException` – If `flowId` or `userId` is `null`.  
- `ArgumentException` – If `flowId` or `userId` consists only of whitespace.  
- `IOException` – If an I/O error occurs while reading the file.  
- `UnauthorizedAccessException` – If access to the file is denied.  
- `JsonException` – If the file contents cannot be parsed as a `UserFlowState`.  
- `OperationCanceledException` – If `cancellationToken` is triggered.

### `TryDeleteStateAsync`

```csharp
public static async Task<bool> TryDeleteStateAsync(
    string flowId,
    string userId,
    CancellationToken cancellationToken = default)
```

**Purpose** – Deletes the state file for the given flow and user, if it exists.  
**Parameters** – Same as `ExistsAsync`.  

**Return Value** – `true` if a file was found and deleted; `false` if no matching file existed.  
**Exceptions**  
- `ArgumentNullException` – If `flowId` or `userId` is `null`.  
- `ArgumentException` – If `flowId` or `userId` consists only of whitespace.  
- `IOException` – If an I/O error occurs while deleting the file.  
- `UnauthorizedAccessException` – If the process lacks permission to delete the file.  
- `OperationCanceledException` – If `cancellationToken` is triggered.

### `LoadStatesByStatusAsync`

```csharp
public static async Task<IReadOnlyList<UserFlowState>> LoadStatesByStatusAsync(
    FlowStatus status,
    CancellationToken cancellationToken = default)
```

**Purpose** – Retrieves all state files whose `Status` property matches the supplied `FlowStatus`.  
**Parameters**  
- `status`: The flow status to filter by.  
- `cancellationToken`: Optional token to cancel the operation.  

**Return Value** – A read‑only list of `UserFlowState` objects matching the status. The list may be empty if no states match.  
**Exceptions**  
- `ArgumentNullException` – If `status` is `null`.  
- `IOException` – If an I/O error occurs while enumerating files.  
- `UnauthorizedAccessException` – If access to the storage directory is denied.  
- `JsonException` – If any encountered file cannot be deserialized.  
- `OperationCanceledException` – If `cancellationToken` is triggered.

### `LoadStatesByFlowAsync`

```csharp
public static async Task<IReadOnlyList<UserFlowState>> LoadStatesByFlowAsync(
    string flowId,
    CancellationToken cancellationToken = default)
```

**Purpose** – Retrieves all state files associated with the specified flow identifier, regardless of user or status.  
**Parameters**  
- `flowId`: Identifier of the conversation flow. Must not be `null` or whitespace.  
- `cancellationToken`: Optional token to cancel the operation.  

**Return Value** – A read‑only list of `UserFlowState` objects for the given flow.  
**Exceptions**  
- `ArgumentNullException` – If `flowId` is `null`.  
- `ArgumentException` – If `flowId` consists only of whitespace.  
- `IOException` – If an I/O error occurs while enumerating files.  
- `UnauthorizedAccessException` – If access to the storage directory is denied.  
- `JsonException` – If any file fails to deserialize.  
- `OperationCanceledException` – If `cancellationToken` is triggered.

### `LoadStatesByFlowAndStatusAsync`

```csharp
public static async Task<IReadOnlyList<UserFlowState>> LoadStatesByFlowAndStatusAsync(
    string flowId,
    FlowStatus status,
    CancellationToken cancellationToken = default)
```

**Purpose** – Retrieves all state files for a given flow that also have the specified status.  
**Parameters**  
- `flowId`: Identifier of the conversation flow. Must not be `null` or whitespace.  
- `status`: The flow status to filter by. Must not be `null`.  
- `cancellationToken`: Optional token to cancel the operation.  

**Return Value** – A read‑only list of matching `UserFlowState` objects.  
**Exceptions**  
- `ArgumentNullException` – If `flowId` or `status` is `null`.  
- `ArgumentException` – If `flowId` consists only of whitespace.  
- `IOException` – If an I/O error occurs while enumerating files.  
- `UnauthorizedAccessException` – If access to the storage directory is denied.  
- `JsonException` – If any file fails to deserialize.  
- `OperationCanceledException` – If `cancellationToken` is triggered.

### `GetStateFilePath`

```csharp
public static string GetStateFilePath(string flowId, string userId)
```

**Purpose** – Computes the full file system path where a state for the given flow and user would be stored.  
**Parameters**  
- `flowId`: Identifier of the conversation flow. Must not be `null` or whitespace.  
- `userId`: Identifier of the user. Must not be `null` or whitespace.  

**Return Value** – The absolute path to the state file. No file system access is performed; the method only builds the path based on the configured storage root.  
**Exceptions**  
- `ArgumentNullException` – If `flowId` or `userId` is `null`.  
- `ArgumentException` – If `flowId` or `userId` consists only of whitespace.

### `LoadInactiveStatesAsync`

```csharp
public static async Task<IReadOnlyList<UserFlowState>> LoadInactiveStatesAsync(
    CancellationToken cancellationToken = default)
```

**Purpose** – Retrieves all state files whose `Status` is `Inactive`. Useful for cleanup or maintenance tasks.  
**Parameters**  
- `cancellationToken`: Optional token to cancel the operation.  

**Return Value** – A read‑only list of `UserFlowState` objects with `Status == FlowStatus.Inactive`.  
**Exceptions**  
- `IOException` – If an I/O error occurs while enumerating files.  
- `UnauthorizedAccessException` – If access to the storage directory is denied.  
- `JsonException` – If any file fails to deserialize.  
- `OperationCanceledException` – If `cancellationToken` is triggered.

### `LoadOldCompletedStatesAsync`

```csharp
public static async Task<IReadOnlyList<UserFlowState>> LoadOldCompletedStatesAsync(
    TimeSpan olderThan,
    CancellationToken cancellationToken = default)
```

**Purpose** – Retrieves all completed state files that were completed earlier than the supplied `TimeSpan` relative to the current UTC time.  
**Parameters**  
- `olderThan`: The minimum age a completed state must exceed to be included. Must be non‑negative.  
- `cancellationToken`: Optional token to cancel the operation.  

**Return Value** – A read‑only list of `UserFlowState` objects where `Status == FlowStatus.Completed` and `CompletedUtc < DateTime.UtcNow - olderThan`.  
**Exceptions**  
- `ArgumentOutOfRangeException` – If `olderThan` is negative.  
- `IOException` – If an I/O error occurs while enumerating files.  
- `UnauthorizedAccessException` – If access to the storage directory is denied.  
- `JsonException` – If any file fails to deserialize.  
- `OperationCanceledException` – If `cancellationToken` is triggered.

## Usage

### Example 1: Checking and loading a user’s flow state

```csharp
string flowId = "order-processing";
string userId = "42";

// Ensure a state exists before attempting to load it.
bool exists = await FileConversationStateStoreExtensions.ExistsAsync(
    flowId, userId);

if (exists)
{
    UserFlowState? state = await FileConversationStateStoreExtensions.TryLoadStateAsync(
        flowId, userId);

    if (state != null)
    {
        // Process the loaded state (e.g., resume the flow).
        Console.WriteLine($"Loaded state for user {userId}: {state.Status}");
    }
}
else
{
    Console.WriteLine($"No state found for user {userId} in flow {flowId}.");
}
```

### Example 2: Performing maintenance – deleting old completed states

```csharp
// Remove completed states older than 30 days.
TimeSpan maxAge = TimeSpan.FromDays(30);
IReadOnlyList<UserFlowState> oldCompleted =
    await FileConversationStateStoreExtensions.LoadOldCompletedStatesAsync(maxAge);

foreach (UserFlowState state in oldCompleted)
{
    bool deleted = await FileConversationStateStoreExtensions.TryDeleteStateAsync(
        state.FlowId, state.UserId);

    if (deleted)
    {
        Console.WriteLine($"Deleted old completed state for flow {state.FlowId}, user {state.UserId}.");
    }
    else
    {
        Console.WriteLine($"Failed to delete state for flow {state.FlowId}, user {state.UserId}.");
    }
}
```

## Notes

- All extension methods are **static** and do not retain any mutable state; therefore they are inherently thread‑safe with respect to the class itself.  
- The underlying file system operations are **not** atomic across multiple calls. Concurrent invocations that target the same file (e.g., one thread deleting while another reads) may result in `IOException` or unexpected behavior. Callers should coordinate access to individual state files if concurrent modifications are possible.  
- Methods that enumerate files (`LoadStatesByStatusAsync`, `LoadStatesByFlowAsync`, `LoadStatesByFlowAndStatusAsync`, `LoadInactiveStatesAsync`, `LoadOldCompletedStatesAsync`) may throw `IOException` if the storage directory becomes unavailable during enumeration.  
- JSON deserialization errors surface as `JsonException`; malformed files are skipped only in the sense that the operation fails entirely—partial results are not returned.  
- The `GetStateFilePath` method performs **no** I/O; it merely combines the configured base directory with the supplied identifiers using a platform‑appropriate separator. It is safe to call from any thread.  
- Cancellation is respected wherever a `CancellationToken` is supplied; if cancellation is triggered, the method will throw `OperationCanceledException` and no partial state will be returned.  
- The implementation assumes that the storage root directory has been configured elsewhere in the application (e.g., via dependency injection). If the directory does not exist, the first write operation will create it; read‑only operations will throw `DirectoryNotFoundException` wrapped in an `IOException`.
