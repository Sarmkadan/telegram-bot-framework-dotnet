# InMemoryConversationStateStore

The `InMemoryConversationStateStore` provides a volatile, in-memory implementation of state management for Telegram bot conversations. It is designed for development environments, lightweight testing, or applications where persistence across process restarts is not required. By storing conversation states in memory, it provides fast read and write access, but all stored state data is lost whenever the application process terminates.

## API

### SaveStateAsync
Persists the provided `UserFlowState` to the in-memory store, overwriting any existing state associated with the same conversation identifier.

- **Parameters:**
  - `state`: The `UserFlowState` object to be saved.
- **Returns:** A `Task` representing the asynchronous operation.
- **Exceptions:** Throws an exception if the provided state or its identifier is null or invalid.

### LoadStateAsync
Retrieves the `UserFlowState` for a specific conversation identifier from the memory store.

- **Parameters:**
  - `chatId`: The identifier of the conversation to load.
- **Returns:** A `Task<UserFlowState?>` containing the state if found, otherwise `null`.

### DeleteStateAsync
Removes the `UserFlowState` associated with a specific conversation identifier from the memory store.

- **Parameters:**
  - `chatId`: The identifier of the conversation whose state should be removed.
- **Returns:** A `Task` representing the asynchronous operation.

### LoadAllActiveStatesAsync
Retrieves a read-only list of all conversation states currently maintained in the memory store.

- **Returns:** A `Task<IReadOnlyList<UserFlowState>>` containing all currently stored `UserFlowState` objects.

## Usage

### Saving and Loading State
```csharp
var store = new InMemoryConversationStateStore();
var state = new UserFlowState(chatId: 12345, currentStep: "Welcome");

// Save the state
await store.SaveStateAsync(state);

// Load the state later
var loadedState = await store.LoadStateAsync(12345);
if (loadedState != null)
{
    Console.WriteLine($"Resuming conversation at: {loadedState.CurrentStep}");
}
```

### Retrieving All Active Conversations
```csharp
var store = new InMemoryConversationStateStore();
// ... assume states have been populated ...

// Retrieve all active states for administrative tasks
IReadOnlyList<UserFlowState> allStates = await store.LoadAllActiveStatesAsync();
foreach (var s in allStates)
{
    Console.WriteLine($"Chat ID: {s.ChatId}, Current Step: {s.CurrentStep}");
}
```

## Notes

- **Data Volatility**: As this store operates entirely in memory, all conversation states are volatile and will be lost immediately upon application restart or process termination. It is not suitable for production scenarios requiring persistent session data.
- **Thread Safety**: The `InMemoryConversationStateStore` implementation is designed to be thread-safe, ensuring that concurrent read and write operations from multiple incoming bot requests do not cause data corruption or race conditions.
- **Memory Usage**: In high-traffic scenarios, storing a large number of active conversation states in memory may lead to increased memory consumption. For systems with a large number of concurrent users, a persistent storage implementation is recommended.
