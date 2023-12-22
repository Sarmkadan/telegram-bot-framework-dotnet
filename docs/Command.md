# Command

The `Command` class serves as the central definition and orchestration structure for bot commands within the `telegram-bot-framework-dotnet`. It encapsulates the metadata, configuration, and state required for the framework to identify, validate, and execute user-triggered commands, managing aspects such as administrative permissions, rate limiting, and command aliases.

## API

### Properties

*   **Name** (`string`)
    The unique identifier for the command.
*   **Description** (`string`)
    A brief explanation of the command's functionality, typically used for help menus.
*   **HandlerType** (`string`)
    The fully qualified type name of the class responsible for handling the command logic.
*   **Type** (`CommandType`)
    The categorized type of the command, defining how the framework processes it.
*   **RequiresAdmin** (`bool`)
    Indicates whether the command requires administrative privileges to execute.
*   **IsEnabled** (`bool`)
    Determines if the command is currently active and available for execution.
*   **ExecutionCount** (`int`)
    The total number of times this command has been successfully executed.
*   **Parameters** (`List<CommandParameter>?`)
    A list of parameters required or supported by the command.
*   **CreatedAt** (`DateTime`)
    The timestamp indicating when the command definition was initialized.
*   **UpdatedAt** (`DateTime`)
    The timestamp indicating when the command definition or state was last modified.
*   **Alias** (`string?`)
    An alternative name or shorthand that can trigger this command.
*   **RateLimitPerMinute** (`int?`)
    The maximum number of times this command can be executed per minute; `null` if no limit is applied.
*   **Validate** (`bool`)
    Indicates whether input validation should be performed on the command's parameters before execution.
*   **GetCommandPatterns** (`IEnumerable<string>`)
    Retrieves the collection of pattern strings (e.g., regexes or exact matches) that trigger this command.

### Methods

*   **CanExecuteBy(...)** (`bool`)
    Determines if a specific user or context has the necessary permissions to execute this command.
*   **RecordExecution()** (`void`)
    Updates the internal state to reflect a command execution, incrementing `ExecutionCount` and updating `UpdatedAt`.
*   **IsRateLimited()** (`bool`)
    Evaluates whether the command is currently subject to rate limiting restrictions based on its configuration and current execution history.

### CommandParameter Members
The `Parameters` property uses the `CommandParameter` type, which includes the following members:
*   **Name** (`string`): The identifier for the parameter.
*   **Type** (`string`): The data type of the parameter.
*   **IsRequired** (`bool`): Indicates if the parameter must be provided for the command to execute.

## Usage

### Example 1: Accessing Command Metadata
```csharp
var command = GetCommand("help");

if (command.IsEnabled)
{
    Console.WriteLine($"Command: {command.Name}");
    Console.WriteLine($"Description: {command.Description}");
    
    if (command.RequiresAdmin)
    {
        Console.WriteLine("Note: Admin access required.");
    }
}
```

### Example 2: Checking Rate Limits Before Execution
```csharp
var command = GetCommand("report");

if (command.IsRateLimited())
{
    Console.WriteLine("Command is currently rate-limited. Try again later.");
}
else
{
    command.RecordExecution();
    ExecuteCommand(command);
}
```

## Notes

*   **Thread Safety**: The `Command` class is not inherently thread-safe. When updating `ExecutionCount` via `RecordExecution` or modifying command state in a multi-threaded environment, external synchronization mechanisms (such as `lock` statements) should be employed.
*   **State Persistence**: Properties such as `ExecutionCount` and `UpdatedAt` represent the in-memory state of the command. If the application restarts, these values will be reset unless persisted to an external data store by the implementing application.
*   **Validation**: When `Validate` is set to `true`, ensure that all `CommandParameter` objects in the `Parameters` list are correctly defined to avoid unexpected validation failures during command invocation.
