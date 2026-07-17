# InMemoryConversationStateStoreExtensions

Provides extension methods for managing conversation state stored in an `InMemoryConversationStateStore`. These methods simplify common operations such as loading, updating, querying, and cleaning up user flow states without exposing the store’s internal collections directly.

## API

### `public static async Task<UserFlowState?> TryLoadStateAsync(this InMemoryConversationStateStore store, string userId, string flowId)`
Attempts to retrieve the state associated with the specified user and flow.  
- **Parameters**  
  - `store`: The `InMemoryConversationStateStore` instance to query.  
  - `userId`: Identifier of the user whose state is sought.  
  - `flowId`: Identifier of the conversation flow.  
- **Return value**  
  - A `Task` that resolves to the `UserFlowState` if found; otherwise `null`.  
- **Exceptions**  
  - `ArgumentNullException` if `store`, `userId`, or `flowId` is `null`.  
  - `ObjectDisposedException` if the store has been disposed.

### `public static async Task<bool> HasStateAsync(this InMemoryConversationStateStore store, string userId, string flowId)`
Checks whether a state exists for the given user and flow.  
- **Parameters**  
  - `store`: The store to inspect.  
  - `userId`: User identifier.  
  - `flowId`: Flow identifier.  
- **Return value**  
  - `true` if a state is present; otherwise `false`.  
- **Exceptions**  
  - `ArgumentNullException` for any `null` argument.  
  - `ObjectDisposedException` if the store is disposed.

### `public static async Task<UserFlowState?> UpdateStateStatusAsync(this InMemoryConversationStateStore store, string stateId, UserFlowStateStatus newStatus)`
Updates the status of an existing state.  
- **Parameters**  
  - `store`: The store containing the state.  
  - `stateId`: Unique identifier of the state to update.  
  - `newStatus`: The new `UserFlowStateStatus` value to apply.  
- **Return value**  
  - The updated `UserFlowState` if the state exists; otherwise `null`.  
- **Exceptions**  
  - `ArgumentNullException` if `store` or `stateId` is `null`.  
  - `InvalidOperationException` if no state with `stateId` is found.  
  - `ObjectDisposedException` if the store is disposed.

### `public static async Task<IReadOnlyList<UserFlowState>> GetActiveStatesAsync(this InMemoryConversationStateStore store)`
Retrieves all states that are not in a terminal status.  
- **Parameters**  
  - `store`: The store to query.  
- **Return value**  
  - A read‑only list of `UserFlowState` instances representing active states.  
- **Exceptions**  
  - `ObjectDisposedException` if the store has been disposed.

### `public static async Task<int> RemoveTerminalStatesAsync(this InMemoryConversationStateStore store)`
Removes all states whose status is terminal (e.g., completed or cancelled).  
- **Parameters**  
  - `store`: The store to clean up.  
- **Return value**  
  - The number of states removed.  
- **Exceptions**  
  - `ObjectDisposedException` if the store is disposed.

### `public static async Task<bool> TouchStateAsync(this InMemoryConversationStateStore store, string stateId)`
Updates the last‑accessed timestamp of a state to the current time.  
- **Parameters**  
  - `store`: The store containing the state.  
  - `stateId`: Identifier of the state to touch.  
- **Return value**  
  - `true` if the state was found and touched; otherwise `false`.  
- **Exceptions**  
  - `ArgumentNullException` if `store` or `stateId` is `null`.  
  - `ObjectDisposedException` if the store is disposed.

### `public static int GetStateCount(this InMemoryConversationStateStore store)`
Gets the current number of states stored in the instance.  
- **Parameters**  
  - `store`: The store to query.  
- **Return value**  
  - An `int` representing the total state count.  
- **Exceptions**  
  - `ObjectDisposedException` if the store has been disposed.

### `public static async Task<UserFlowState?> FindStateByIdAsync(this InMemoryConversationStateStore store, string stateId)`
Locates a state by its unique identifier.  
- **Parameters**  
  - `store`: The store to search.  
  - `stateId`: The identifier of the desired state.  
- **Return value**  
  - The matching `UserFlowState` if found; otherwise `null`.  
- **Exceptions**  
  - `ArgumentNullException` if `store` or `stateId` is `null`.  
  - `ObjectDisposedException` if the store is disposed.

### `public static async Task<int> RemoveStaleStatesAsync(this InMemoryConversationStateStore store, TimeSpan maxAge)`
Removes states that have not been accessed for longer than `maxAge`.  
- **Parameters**  
  - `store`: The store to clean.  
  - `maxAge`: The maximum allowable age for a state; states older than this are considered stale.  
- **Return value**  
  - The number of states removed.  
- **Exceptions**  
  - `ArgumentNullException` if `store` is `null`.  
  - `ArgumentOutOfRangeException` if `maxAge` is negative.  
  - `ObjectDisposedException` if the store is disposed.

## Usage

### Example 1: Loading and updating a user flow state
```csharp
var store = new InMemoryConversationStateStore();

// Assume a state has already been created elsewhere.
string userId = "12345";
string flowId = "order-flow";

// Try to load the existing state.
UserFlowState? state = await store.TryLoadStateAsync(userId, flowId);
if (state == null)
{
    // Handle missing state (e.g., create a new one).
    return;
}

// Touch the state to refresh its last‑accessed time.
bool touched = await store.TouchStateAsync(state.Id);
if (!touched)
{
    // This should not happen unless the state was removed concurrently.
    throw new InvalidOperationException("State disappeared after load.");
}

// Update the state status to indicate progress.
UserFlowState? updated = await store.UpdateStateStatusAsync(state.Id, UserFlowStateStatus.InProgress);
if (updated == null)
{
    throw new InvalidOperationException("Failed to update state status.");
}
```

### Example 2: Periodic cleanup of terminal and stale states
```csharp
var store = new InMemoryConversationStateStore();

// Remove states that have reached a terminal condition.
int removedTerminal = await store.RemoveTerminalStatesAsync();
logger.Info($"Removed {removedTerminal} terminal states.");

// Remove states that have not been touched in the last 24 hours.
TimeSpan staleThreshold = TimeSpan.FromHours(24);
int removedStale = await store.RemoveStaleStatesAsync(store, staleThreshold);
logger.Info($"Removed {removedStale} stale states older than {staleThreshold}.");
```

## Notes

- All extension methods operate on the supplied `InMemoryConversationStateStore` instance. The store itself is **not** thread‑safe; concurrent access from multiple threads requires external synchronization (e.g., a `lock` around calls) unless the store’s internal implementation guarantees safety for a particular operation.
- Passing `null` for any reference‑type argument results in an `ArgumentNullException`.  
- If the store has been disposed via its `Dispose` method, any subsequent call throws an `ObjectDisposedException`.  
- Methods that search for a state (`TryLoadStateAsync`, `FindStateByIdAsync`, `UpdateStateStatusAsync`, `TouchStateAsync`) return `null` or `false` when the state cannot be located; they do not throw for missing states except where explicitly noted (e.g., `UpdateStateStatusAsync` throws when the state is absent).  
- `RemoveTerminalStatesAsync` and `RemoveStaleStatesAsync` return the count of removed items; a return value of `0` indicates that no matching states were found at the time of invocation.  
- The `GetStateCount` property reflects the number of states currently held in memory and may change rapidly in a high‑throughput scenario; callers should treat the value as a snapshot.  
- No method modifies the store’s disposal state; disposal must be performed explicitly by the owner of the `InMemoryConversationStateStore`.
