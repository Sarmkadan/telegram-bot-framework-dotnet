# ConversationFlowOptions

`ConversationFlowOptions` provides the configuration settings required to manage multi-step conversational flows within the `telegram-bot-framework-dotnet` library. It allows developers to define flow lifetimes, resource limits per user, automated message responses for flow termination, and custom eviction logic, ensuring consistent and resource-efficient state management for complex interactive bot sessions.

## API

### DefaultFlowTimeout
*   **Purpose**: Specifies the `TimeSpan` of inactivity permitted before an active conversation flow is automatically considered expired.
*   **Default**: `TimeSpan.FromMinutes(5)`

### MaxActiveFlowsPerUser
*   **Purpose**: Limits the number of concurrent, active conversation flows allowed for a single user.
*   **Default**: `1`

### AutoResumeOnSessionRestore
*   **Purpose**: A `bool` value indicating whether active flows should automatically resume when a user session is restored.
*   **Default**: `true`

### MaxHistoryPerUser
*   **Purpose**: Defines the maximum number of historical flow steps to be stored for a user, enabling context tracking.
*   **Default**: `10`

### FlowAbandonedMessage
*   **Purpose**: The text message sent to the user when a flow is explicitly abandoned by the bot or external triggers.
*   **Default**: `"The conversation flow has been abandoned."`

### FlowTimeoutMessage
*   **Purpose**: The text message sent to the user when a flow expires due to the inactivity threshold defined in `DefaultFlowTimeout`.
*   **Default**: `"The conversation flow has timed out due to inactivity."`

### EnableFlowEvents
*   **Purpose**: A `bool` flag to toggle the publication of flow-related events (e.g., started, ended, evicted).
*   **Default**: `false`

### CleanupIntervalMinutes
*   **Purpose**: The interval in minutes at which the background process scans for and cleans up expired conversation flows.
*   **Default**: `1`

### AbortKeyword
*   **Purpose**: An optional string keyword that, when sent by the user, immediately aborts the current active flow.
*   **Return Value**: Returns `string?`.
*   **Note**: If set to `null`, no abort keyword functionality is enabled.

### AbortAcknowledgementMessage
*   **Purpose**: The text message returned to the user when they successfully trigger an abort via the `AbortKeyword`.
*   **Default**: `"The conversation flow has been aborted."`

### TimeoutEvictionPolicy
*   **Purpose**: Specifies the `FlowEvictionPolicy` applied when a flow is terminated due to a timeout.
*   **Exceptions**: Throws `ArgumentException` if an undefined `FlowEvictionPolicy` value is assigned.

### OnEviction
*   **Purpose**: An optional delegate invoked when a flow is evicted from the active state, allowing for custom cleanup or logging.
*   **Parameters**: `UserFlowState` (the state of the evicted flow), `CancellationToken` (for managing task lifecycle).
*   **Return Value**: `Task` representing the asynchronous operation.
*   **Exceptions**: Throws an exception if the assigned delegate throws an unhandled exception during execution.

## Usage

### Configuring Options via Dependency Injection
```csharp
builder.Services.AddTelegramBotFlows(options =>
{
    options.DefaultFlowTimeout = TimeSpan.FromMinutes(15);
    options.MaxActiveFlowsPerUser = 2;
    options.AbortKeyword = "/cancel";
    options.AbortAcknowledgementMessage = "Flow cancelled. Let me know if you need anything else.";
    options.EnableFlowEvents = true;
});
```

### Implementing Custom Eviction Logic
```csharp
options.OnEviction = async (state, cancellationToken) =>
{
    // Log the eviction of a flow for diagnostic purposes
    await logger.LogInformationAsync($"Flow {state.FlowId} evicted for user {state.UserId}.", cancellationToken);
    
    // Perform cleanup of external resources if necessary
    await database.ClearTemporaryStateAsync(state.UserId, cancellationToken);
};
```

## Notes

*   **Thread-Safety**: `ConversationFlowOptions` instances are intended to be configured once during service registration. Once the framework initializes, these options should be treated as immutable to ensure thread-safe access during flow processing.
*   **AbortKeyword Conflict**: Care should be taken when selecting the `AbortKeyword` to ensure it does not conflict with existing bot commands or common conversational phrases, which could lead to unintentional flow termination.
*   **CleanupIntervalMinutes**: A shorter `CleanupIntervalMinutes` improves accuracy in flow expiration but increases the frequency of background processing tasks; this value should be tuned based on the anticipated volume of concurrent users.
*   **Delegates**: Ensure that the `OnEviction` delegate is efficient and handles its own exceptions to prevent blocking the background flow-cleanup process.
