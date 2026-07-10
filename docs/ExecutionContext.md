# ExecutionContext

The `ExecutionContext` class serves as the central container for data associated with the processing of a specific message or command within the `telegram-bot-framework-dotnet` ecosystem. It encapsulates the identity of the user and chat, the current command being executed, associated parameters and state, and tracks the lifecycle of the request, allowing bot handlers to maintain context across asynchronous operations and control flow execution.

## API

### Properties

*   `ContextId` (`string`): A unique identifier for the current execution context.
*   `UserId` (`long`): The unique identifier of the user who initiated the request.
*   `ChatId` (`long`): The unique identifier of the chat in which the request occurred.
*   `User` (`BotUser?`): The user entity associated with the request, if available.
*   `Session` (`UserSession?`): The active user session, if one exists.
*   `Command` (`Command?`): The command currently being processed, if applicable.
*   `Message` (`Message?`): The original message triggering the execution.
*   `Parameters` (`Dictionary<string, object>?`): A collection of parameters parsed from the message.
*   `CreatedAt` (`DateTime`): The timestamp indicating when the context was initialized.
*   `States` (`Dictionary<string, object>`): A mutable dictionary for storing arbitrary state data during the command's lifecycle.
*   `Errors` (`List<string>?`): A list of errors encountered during processing.
*   `IsValid` (`bool`): Indicates whether the current context is considered valid for further processing.
*   `PendingResponse` (`string?`): The text or content of a pending response to be sent to the user.
*   `IsStopped` (`bool`): Indicates if the execution flow has been terminated.

### Methods

*   `T? GetParameter<T>()`: Retrieves a parameter value by key, cast to the specified type `T`. Returns default if not found or cannot be cast.
*   `void SetParameter(string key, object value)`: Adds or updates a parameter in the context.
*   `T? GetState<T>(string key)`: Retrieves a state value by key, cast to the specified type `T`. Returns default if not found or cannot be cast.
*   `void SetState(string key, object value)`: Adds or updates a state value in the context.
*   `void RespondAndStop(string message)`: Sets the `PendingResponse`, terminates the command flow, and marks the context as stopped.
*   `void StopProcessing()`: Immediately marks the context as stopped, preventing subsequent handlers from executing.

## Usage

### Retrieving and Utilizing Contextual Data

```csharp
public async Task HandleAsync(ExecutionContext context)
{
    // Retrieve a typed parameter parsed from the message
    var itemId = context.GetParameter<int>("itemId");
    
    // Access state for multi-step command logic
    var retryCount = context.GetState<int>("retryCount");
    
    if (itemId <= 0)
    {
        context.RespondAndStop("Invalid item ID provided.");
        return;
    }
    
    // Proceed with logic...
}
```

### Managing Execution Flow

```csharp
public async Task ProcessCommand(ExecutionContext context)
{
    try
    {
        // Perform logic
        await _service.ExecuteAsync(context.UserId);
        
        context.SetState("status", "processed");
    }
    catch (Exception ex)
    {
        context.Errors ??= new List<string>();
        context.Errors.Add(ex.Message);
        
        // Terminate execution flow due to error
        context.StopProcessing();
    }
}
```

## Notes

*   **Thread Safety:** The `ExecutionContext` is not thread-safe. It is designed to be scoped to a single request lifecycle. Access to properties like `States` or `Parameters` should not be performed concurrently from multiple threads.
*   **State Persistence:** The `States` dictionary is ephemeral and exists only for the lifetime of the `ExecutionContext`. It is not automatically persisted to long-term storage unless explicitly handled by the application logic.
*   **Validation:** If `IsValid` is set to `false` by a middleware or pre-processor, subsequent commands should respect this and skip execution.
*   **`RespondAndStop`:** This method is intended to short-circuit the pipeline immediately. Any code executed after calling this method in the same handler will run, but subsequent handlers in the pipeline will be skipped.
