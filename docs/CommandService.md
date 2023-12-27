# CommandService

The `CommandService` is the central orchestration component within the `telegram-bot-framework-dotnet` responsible for the lifecycle management, validation, and execution of bot commands. It handles the registration of command definitions, enforces access control and rate-limiting policies prior to execution, and maintains persistent records of command usage statistics. This service acts as the gateway between incoming Telegram updates and the underlying command logic, ensuring that only authorized and compliant requests are processed while providing asynchronous interfaces for all operations to support high-concurrency environments.

## API

### `CommandService`
The public constructor used to instantiate the service. It typically initializes internal repositories, caching mechanisms, and dependency injections required for command resolution and execution tracking.

### `GetCommandAsync`
Retrieves a specific command definition by its identifier or name.
*   **Parameters**: Implicitly accepts a command identifier (string or int) based on framework configuration.
*   **Return Value**: `Task<Models.Command?>`. Returns the command model if found; otherwise, returns `null`.
*   **Throws**: May throw exceptions if the underlying data store is unavailable or if the query format is invalid.

### `RegisterCommandAsync`
Registers a new command definition with the service, making it available for discovery and execution.
*   **Parameters**: Accepts a `Models.Command` object containing the command metadata, triggers, and configuration.
*   **Return Value**: `Task<Models.Command>`. Returns the registered command instance, potentially with updated system-generated IDs or timestamps.
*   **Throws**: Throws an exception if a command with the same unique identifier already exists or if the provided model is invalid.

### `UnregisterCommandAsync`
Removes a previously registered command from the service, preventing further execution.
*   **Parameters**: Accepts the unique identifier of the command to remove.
*   **Return Value**: `Task<bool>`. Returns `true` if the command was successfully found and removed; `false` if the command did not exist.
*   **Throws**: Generally does not throw for non-existent commands (returns `false`), but may throw if the storage backend fails during the deletion process.

### `GetAvailableCommandsAsync`
Retrieves a list of all currently registered and active commands available to the bot.
*   **Parameters**: None (may implicitly filter based on scope if configured).
*   **Return Value**: `Task<IList<Models.Command>>`. Returns a list of command models. The list may be empty if no commands are registered.
*   **Throws**: May throw if the retrieval process encounters a critical infrastructure error.

### `ExecuteCommandAsync`
Validates permissions and rate limits, then executes the logic associated with a specific command.
*   **Parameters**: Accepts an execution context or request object containing user details, command arguments, and chat information.
*   **Return Value**: `Task<Models.ExecutionContext>`. Returns the updated execution context containing the result of the operation, output data, or error states.
*   **Throws**: Throws specific exceptions if the user lacks permission (`UnauthorizedException`), if the command is rate-limited (`RateLimitException`), or if the command logic itself fails.

### `CanUserExecuteCommandAsync`
Evaluates whether a specific user meets the criteria (roles, permissions, blacklist status) to execute a given command.
*   **Parameters**: Accepts user identifiers and the target command identifier.
*   **Return Value**: `Task<bool>`. Returns `true` if the user is authorized; `false` otherwise.
*   **Throws**: Unlikely to throw under normal conditions; returns `false` for invalid inputs unless the permission backend is unreachable.

### `IsCommandRateLimitedAsync`
Checks if a specific command invocation exceeds the configured frequency limits for the given user or chat.
*   **Parameters**: Accepts user/chat identifiers and the command identifier.
*   **Return Value**: `Task<bool>`. Returns `true` if the action is currently rate-limited; `false` if allowed.
*   **Throws**: May throw if the rate-limiting store (e.g., Redis or memory cache) is inaccessible.

### `RecordCommandExecutionAsync`
Persists a record of a command execution event for auditing and statistical purposes.
*   **Parameters**: Accepts details of the execution, including timestamp, user ID, command ID, and success/failure status.
*   **Return Value**: `Task`. Completes when the record is successfully persisted.
*   **Throws**: Throws if the logging or storage mechanism fails to write the record.

### `GetCommandExecutionCountAsync`
Retrieves the total number of times a specific command has been executed.
*   **Parameters**: Accepts the command identifier and optional time-range filters.
*   **Return Value**: `Task<int>`. Returns the aggregate count of executions.
*   **Throws**: May throw if the analytics backend is unavailable.

## Usage

### Example 1: Registering and Listing Commands
This example demonstrates initializing the service, registering a new command, and retrieving the full list of available commands.

```csharp
using TelegramBotFramework;
using TelegramBotFramework.Models;

public async Task InitializeCommandsAsync(CommandService commandService)
{
    // Define a new command
    var helpCommand = new Command
    {
        Name = "help",
        Description = "Displays available commands",
        Trigger = "/help",
        IsPublic = true
    };

    // Register the command
    var registered = await commandService.RegisterCommandAsync(helpCommand);
    Console.WriteLine($"Registered command: {registered.Name} with ID {registered.Id}");

    // Retrieve all available commands
    var availableCommands = await commandService.GetAvailableCommandsAsync();
    
    foreach (var cmd in availableCommands)
    {
        Console.WriteLine($"- {cmd.Name}: {cmd.Description}");
    }
}
```

### Example 2: Validation and Execution Flow
This example illustrates the manual validation flow before executing a command, checking permissions and rate limits explicitly.

```csharp
using TelegramBotFramework;
using TelegramBotFramework.Models;

public async Task HandleUserRequestAsync(CommandService commandService, long userId, string commandName)
{
    // Check if the user is authorized
    bool isAuthorized = await commandService.CanUserExecuteCommandAsync(userId, commandName);
    if (!isAuthorized)
    {
        Console.WriteLine("Access denied.");
        return;
    }

    // Check rate limiting
    bool isLimited = await commandService.IsCommandRateLimitedAsync(userId, commandName);
    if (isLimited)
    {
        Console.WriteLine("Rate limit exceeded. Please try again later.");
        return;
    }

    // Prepare execution context
    var context = new ExecutionContext
    {
        UserId = userId,
        CommandName = commandName,
        Timestamp = DateTime.UtcNow
    };

    try
    {
        // Execute the command
        var result = await commandService.ExecuteCommandAsync(context);
        
        // Record the successful execution
        await commandService.RecordCommandExecutionAsync(result);
        
        Console.WriteLine($"Command executed successfully. Status: {result.Status}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Execution failed: {ex.Message}");
        // Optionally record failure
    }
}
```

## Notes

*   **Thread Safety**: All public methods are asynchronous and designed to be thread-safe. The service relies on underlying concurrent collections or external distributed stores (e.g., databases, caches) to manage state safely across multiple concurrent requests.
*   **Null Handling**: `GetCommandAsync` explicitly returns `null` if a command is not found, rather than throwing an exception. Callers must handle null checks appropriately.
*   **Race Conditions**: While individual methods are atomic, sequences of operations (e.g., checking `IsCommandRateLimitedAsync` followed by `ExecuteCommandAsync`) are not inherently transactional. In high-throughput scenarios, a race condition could theoretically occur where a limit is exceeded between the check and the execution. The `ExecuteCommandAsync` method performs its own internal validation to mitigate this risk.
*   **Persistence**: Methods like `RecordCommandExecutionAsync` and `RegisterCommandAsync` involve I/O operations. Transient network failures or database locks may cause these methods to throw. Implementations should include retry logic or global exception handling for these specific calls.
*   **Return Values**: `UnregisterCommandAsync` returns a boolean indicating success rather than throwing on missing entities, allowing for idempotent cleanup operations without requiring prior existence checks.
