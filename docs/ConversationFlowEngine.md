# ConversationFlowEngine

The `ConversationFlowEngine` is the primary orchestrator for managing stateful, multi-step conversational flows within the telegram-bot-framework-dotnet. It provides a structured mechanism to define, persist, and execute conversational logic, allowing developers to maintain complex user interactions over multiple message exchanges while handling state transitions, flow history, and expired session cleanup.

## API

### Constructors
- `ConversationFlowEngine()`: Initializes a new instance of the engine with default configuration.
- `ConversationFlowEngine(IConversationRepository repository)`: Initializes a new instance of the engine using a custom repository for persistent storage of flow states.

### Methods
- `Task RegisterFlowAsync(FlowDefinition flow)`: Registers a new `FlowDefinition` with the engine. Throws an exception if a flow with the same identifier is already registered.
- `Task UnregisterFlowAsync(string flowId)`: Removes a registered `FlowDefinition` from the engine by its identifier.
- `Task<FlowDefinition?> GetFlowAsync(string flowId)`: Retrieves a `FlowDefinition` by its identifier. Returns `null` if the flow is not registered.
- `Task<IReadOnlyList<FlowDefinition>> GetAllFlowsAsync()`: Returns a read-only list of all currently registered flow definitions.
- `async Task<UserFlowState> StartFlowAsync(long chatId, string flowId)`: Initiates a new conversation flow for a specific user. Returns the initial `UserFlowState`.
- `async Task<FlowStepResult> ProcessInputAsync(long chatId, Message message)`: Processes an incoming message for a user currently in an active flow. Returns a `FlowStepResult` indicating the outcome of the step.
- `Task<UserFlowState?> GetActiveFlowStateAsync(long chatId)`: Retrieves the current active `UserFlowState` for the specified user. Returns `null` if no flow is active.
- `async Task AbortFlowAsync(long chatId)`: Terminates any active conversation flow for the specified user and removes the current state.
- `async Task<UserFlowState?> ResumeFlowAsync(long chatId)`: Attempts to resume a suspended conversation flow for the specified user. Returns the resumed `UserFlowState`, or `null` if no flow could be resumed.
- `Task<IReadOnlyList<UserFlowState>> GetFlowHistoryAsync(long chatId)`: Retrieves the historical records of completed or aborted flows for a user.
- `Task<bool> IsUserInFlowAsync(long chatId)`: Returns `true` if the specified user is currently participating in an active flow, otherwise `false`.
- `async Task<int> CleanupExpiredFlowStatesAsync()`: Removes flow states that have exceeded their configured expiration time. Returns the count of cleaned-up records.

## Usage

### Registering and Starting a Flow
```csharp
// Assuming engine is injected
var flowDefinition = new FlowDefinition("registration-flow", steps);
await engine.RegisterFlowAsync(flowDefinition);

// Start the flow when the user sends a command
if (message.Text == "/register")
{
    await engine.StartFlowAsync(message.Chat.Id, "registration-flow");
    await botClient.SendTextMessageAsync(message.Chat.Id, "Starting registration...");
}
```

### Processing Incoming Messages
```csharp
// Check if the user is in a flow before processing
if (await engine.IsUserInFlowAsync(message.Chat.Id))
{
    var result = await engine.ProcessInputAsync(message.Chat.Id, message);
    
    if (result.Status == FlowStatus.Completed)
    {
        await botClient.SendTextMessageAsync(message.Chat.Id, "Flow completed successfully!");
    }
}
```

## Notes

- **Thread Safety**: The `ConversationFlowEngine` is designed to be thread-safe when accessing its internal registry of flows, provided the underlying `IConversationRepository` implementation ensures transactional consistency for state persistence.
- **State Persistence**: If initialized without a repository, state is held in memory. In production environments, it is highly recommended to inject a persistent `IConversationRepository` to ensure flow states survive application restarts.
- **Async Operations**: All methods are asynchronous. Ensure that the calling code properly awaits these tasks to avoid race conditions or unhandled exceptions within the conversational state machine.
- **Expired Flows**: It is recommended to schedule `CleanupExpiredFlowStatesAsync` periodically (e.g., using a background task) to manage memory and storage growth from abandoned user sessions.
