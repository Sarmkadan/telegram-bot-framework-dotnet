# BotController

The `BotController` serves as the primary HTTP entry point for handling incoming Telegram Bot API updates within the `telegram-bot-framework-dotnet` ecosystem. It exposes a set of standardized endpoints designed to process messages, retrieve user and session context, and manage bot commands and menus, while simultaneously exposing intrinsic properties of the current request context such as user identity and message content.

## API

### Constructors

#### `public BotController()`
Initializes a new instance of the `BotController` class. This constructor sets up the necessary internal state to handle the current HTTP request context associated with a Telegram update.

### Properties

#### `public long UserId`
Gets the unique identifier of the Telegram user associated with the current request. This value is extracted from the incoming update payload.

#### `public long ChatId`
Gets the unique identifier of the chat where the message originated. In private chats, this often correlates with the `UserId`, but differs in group or channel contexts.

#### `public string FirstName`
Gets the first name of the user who triggered the current update. This property is populated based on the sender's profile data provided by Telegram.

#### `public string? LastName`
Gets the last name of the user who triggered the current update. This property may be `null` if the user has not set a last name in their Telegram profile.

#### `public string Content`
Gets the textual content or payload of the incoming message. The format of this string depends on the `MessageType`.

#### `public MessageType MessageType`
Gets the enumeration value indicating the type of the incoming message (e.g., Text, Command, Callback). This determines how the `Content` property should be interpreted.

### Actions

#### `public IActionResult Health()`
Provides a lightweight endpoint for monitoring the operational status of the bot service.
*   **Purpose**: Returns an immediate response to verify the controller is reachable and the application is running.
*   **Parameters**: None.
*   **Return Value**: An `IActionResult` indicating success (typically HTTP 200 OK).
*   **Throws**: Does not throw under normal operating conditions; may throw standard ASP.NET Core infrastructure exceptions if the host environment is unstable.

#### `public async Task<IActionResult> ProcessMessage()`
Handles the core logic for processing an incoming message update from Telegram.
*   **Purpose**: Parses the incoming request, updates the controller's context properties (`UserId`, `Content`, etc.), and dispatches the message to the appropriate handler logic.
*   **Parameters**: None (reads from the current HTTP request body).
*   **Return Value**: A `Task<IActionResult>` representing the asynchronous operation. Returns an HTTP status code indicating whether the message was processed successfully or if an error occurred.
*   **Throws**: May throw exceptions related to JSON deserialization if the payload is malformed, or propagation of unhandled exceptions from downstream message handlers.

#### `public async Task<IActionResult> GetUser()`
Retrieves detailed information about the user associated with the current context.
*   **Purpose**: Fetches user data, potentially enriching the basic `FirstName`/`LastName` properties with stored database records or additional Telegram API calls.
*   **Parameters**: None.
*   **Return Value**: A `Task<IActionResult>` containing the user data serialized as JSON or an error status if the user cannot be found.
*   **Throws**: May throw if the `UserId` context is invalid or if the underlying data store is unavailable.

#### `public async Task<IActionResult> GetSession()`
Retrieves the current session state for the specific user and chat combination.
*   **Purpose**: Returns persistent or temporary state data associated with the user's interaction flow.
*   **Parameters**: None.
*   **Return Value**: A `Task<IActionResult>` containing the session data. Returns an empty or default result if no session exists.
*   **Throws**: May throw if the session provider is unreachable or serialization fails.

#### `public async Task<IActionResult> GetCommands()`
Returns a list of available commands supported by the bot in the current context.
*   **Purpose**: Dynamically generates or retrieves the command list based on user permissions or current state.
*   **Parameters**: None.
*   **Return Value**: A `Task<IActionResult>` containing a collection of command definitions.
*   **Throws**: Generally does not throw unless command registration services are misconfigured.

#### `public async Task<IActionResult> GetMenu()`
Retrieves the current interactive menu structure applicable to the user.
*   **Purpose**: Provides the definition for inline keyboards or reply keyboards required for the current step in the conversation flow.
*   **Parameters**: None.
*   **Return Value**: A `Task<IActionResult>` containing the menu configuration.
*   **Throws**: May throw if the menu definition for the current state is missing or malformed.

## Usage

### Example 1: Manual Context Inspection
This example demonstrates how to access the intrinsic properties of the `BotController` within a derived implementation or during debugging to inspect the current message context before processing.

```csharp
public class CustomBotController : BotController
{
    public async Task<IActionResult> AnalyzeIncoming()
    {
        // Access intrinsic properties populated by the framework
        var userIdentifier = $"{FirstName} {LastName ?? ""} ({UserId})";
        var logMessage = $"User {userIdentifier} sent [{MessageType}]: {Content} in Chat {ChatId}";

        // Perform custom logging or validation
        Console.WriteLine(logMessage);

        if (MessageType == MessageType.Command)
        {
            return await ProcessMessage();
        }

        return Ok(new { Status = "Ignored non-command message" });
    }
}
```

### Example 2: Orchestrating Session and Menu Retrieval
This example illustrates a workflow where the controller retrieves the current session state to determine which menu to present to the user.

```csharp
public async Task<IActionResult> RenderCurrentInterface()
{
    // Retrieve the current session to determine user state
    var sessionResult = await GetSession();
    
    if (sessionResult is OkObjectResult sessionData)
    {
        // Logic to determine menu based on session content would go here
        // For demonstration, we simply return the available menu
        return await GetMenu();
    }

    // If no session exists, initialize a default flow
    return await ProcessMessage();
}
```

## Notes

*   **Context Dependency**: The properties `UserId`, `ChatId`, `FirstName`, `LastName`, `Content`, and `MessageType` are stateful relative to the current HTTP request. They are only valid after the framework has parsed the incoming Telegram update, typically during or after the execution of `ProcessMessage`. Accessing these properties in a standalone request without an associated update payload may result in default values (e.g., `0` for IDs, `null` for strings).
*   **Thread Safety**: As an ASP.NET Core controller, `BotController` is instantiated per request. Therefore, instance members are inherently thread-safe within the scope of a single request. However, static modifications to the class or shared resources accessed by the async methods (`GetSession`, `ProcessMessage`) must be synchronized externally if they involve mutable shared state.
*   **Nullability**: The `LastName` property is explicitly nullable (`string?`). Consumers must perform null checks before concatenating or displaying this value to avoid runtime null reference exceptions.
*   **Asynchronous Execution**: All data retrieval methods (`GetUser`, `GetSession`, `GetCommands`, `GetMenu`, `ProcessMessage`) are asynchronous. Calling these methods without `await` will result in unobserved tasks and likely return incomplete `Task` objects rather than the intended `IActionResult`.
