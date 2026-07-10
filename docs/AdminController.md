# AdminController

`AdminController` is an ASP.NET Core MVC controller that exposes administrative endpoints for managing a Telegram bot framework instance. It provides operations for inspecting runtime configuration, retrieving usage statistics, managing bot administrators and banned users, registering and deleting custom bot commands, inspecting menu structures, and closing expired user sessions. All endpoints require appropriate authorization and are designed to be consumed by a secure administrative dashboard or API client.

## API

### `public AdminController`

Constructor. Initializes a new instance of the controller with the required bot framework services. Dependencies are injected via standard ASP.NET Core dependency injection.

- **Parameters:** *(constructor dependencies are not part of the public documented signature)*
- **Returns:** A new `AdminController` instance.
- **Throws:** `ArgumentNullException` if any required dependency is `null`.

---

### `public IActionResult GetConfiguration`

Retrieves the current runtime configuration of the bot framework, including settings such as the bot token prefix, webhook URL, and feature flags.

- **Parameters:** None.
- **Returns:** `IActionResult` containing a JSON-serialized representation of the configuration object.
- **Throws:** No documented exceptions. Returns an appropriate error status code if configuration retrieval fails internally.

---

### `public async Task<IActionResult> GetStatistics`

Returns aggregated usage statistics for the bot, such as total active users, command invocation counts, and session metrics over a configurable time window.

- **Parameters:** None.
- **Returns:** `Task<IActionResult>` that resolves to a JSON payload with statistics data.
- **Throws:** `InvalidOperationException` if the statistics service is unavailable or data collection has not been initialized.

---

### `public async Task<IActionResult> GetAdministrators`

Lists all users currently granted administrator privileges within the bot framework.

- **Parameters:** None.
- **Returns:** `Task<IActionResult>` containing a JSON array of administrator user objects (typically including Telegram user ID, username, and promotion date).
- **Throws:** No documented exceptions. Returns an empty array if no administrators exist.

---

### `public async Task<IActionResult> PromoteToAdmin`

Promotes a specified user to administrator status, granting access to administrative commands and the admin dashboard.

- **Parameters:** *(bound from request body or query)* A user identifier (Telegram user ID or username).
- **Returns:** `Task<IActionResult>` with a success confirmation or an error message if the user is already an administrator or does not exist.
- **Throws:** `ArgumentException` if the provided user identifier is invalid or empty. `InvalidOperationException` if the user is already an administrator.

---

### `public async Task<IActionResult> DemoteAdmin`

Revokes administrator privileges from a specified user.

- **Parameters:** *(bound from request body or query)* A user identifier (Telegram user ID or username).
- **Returns:** `Task<IActionResult>` with a success confirmation or an error message if the user is not currently an administrator.
- **Throws:** `ArgumentException` if the provided user identifier is invalid or empty. `InvalidOperationException` if the user is not an administrator.

---

### `public async Task<IActionResult> BanUser`

Bans a user from interacting with the bot. Banned users receive no responses and are blocked from all bot functionality.

- **Parameters:** *(bound from request body or query)* A user identifier (Telegram user ID or username) and optionally a reason string.
- **Returns:** `Task<IActionResult>` with a success confirmation or an error if the user is already banned.
- **Throws:** `ArgumentException` if the user identifier is invalid. `InvalidOperationException` if the user is already banned.

---

### `public async Task<IActionResult> UnbanUser`

Lifts a previously applied ban, restoring the user's ability to interact with the bot.

- **Parameters:** *(bound from request body or query)* A user identifier (Telegram user ID or username).
- **Returns:** `Task<IActionResult>` with a success confirmation or an error if the user is not currently banned.
- **Throws:** `ArgumentException` if the user identifier is invalid. `InvalidOperationException` if the user is not banned.

---

### `public async Task<IActionResult> RegisterCommand`

Registers a new custom bot command with its associated handler and metadata. The command becomes immediately available to users with appropriate permissions.

- **Parameters:** *(bound from request body)* A command definition object containing the command name, description, handler type information, and optional permission requirements.
- **Returns:** `Task<IActionResult>` with the registered command details or an error if the command name conflicts with an existing command.
- **Throws:** `ArgumentException` if the command name is null, empty, or contains invalid characters. `InvalidOperationException` if a command with the same name already exists.

---

### `public async Task<IActionResult> GetCommand`

Retrieves the definition and metadata for a specific registered command by its name.

- **Parameters:** *(bound from query)* The command name as a string.
- **Returns:** `Task<IActionResult>` containing the command definition JSON or a 404 status if not found.
- **Throws:** `ArgumentException` if the command name is null or empty.

---

### `public async Task<IActionResult> DeleteCommand`

Removes a previously registered custom command from the bot. Built-in framework commands cannot be deleted.

- **Parameters:** *(bound from query)* The command name as a string.
- **Returns:** `Task<IActionResult>` with a success confirmation or an error if the command does not exist or is a protected built-in command.
- **Throws:** `ArgumentException` if the command name is null or empty. `InvalidOperationException` if attempting to delete a non-removable built-in command.

---

### `public async Task<IActionResult> GetMenus`

Returns the current menu structure configuration, including inline keyboard layouts and callback mappings for all defined menus.

- **Parameters:** None.
- **Returns:** `Task<IActionResult>` containing a JSON representation of all configured menus.
- **Throws:** No documented exceptions.

---

### `public async Task<IActionResult> CloseExpiredSessions`

Forcefully terminates all user sessions that have exceeded their configured time-to-live. This is an administrative housekeeping operation that frees resources and logs out inactive users.

- **Parameters:** None.
- **Returns:** `Task<IActionResult>` with a summary of the number of sessions closed.
- **Throws:** `InvalidOperationException` if the session management service is unavailable.

---

## Usage

### Example 1: Promoting a User and Registering a Command

```csharp
// Assume _adminController is an injected or resolved AdminController instance
// Promote user 123456789 to administrator
var promoteResult = await _adminController.PromoteToAdmin(userId: 123456789);
if (promoteResult is OkObjectResult okPromote)
{
    Console.WriteLine($"User promoted: {okPromote.Value}");
}

// Register a new custom command for the newly promoted admin
var commandDef = new CommandDefinition
{
    Name = "stats",
    Description = "Show personal statistics",
    HandlerType = typeof(PersonalStatsCommandHandler).AssemblyQualifiedName,
    RequiredRole = "User"
};
var registerResult = await _adminController.RegisterCommand(commandDef);
if (registerResult is OkObjectResult okRegister)
{
    Console.WriteLine($"Command registered: {okRegister.Value}");
}
```

### Example 2: Banning a User and Closing Expired Sessions

```csharp
// Ban a problematic user with a reason
var banResult = await _adminController.BanUser(userId: 987654321, reason: "Spam activity detected");
if (banResult is OkObjectResult)
{
    Console.WriteLine("User banned successfully.");
}

// Perform routine session cleanup
var cleanupResult = await _adminController.CloseExpiredSessions();
if (cleanupResult is OkObjectResult okCleanup)
{
    Console.WriteLine($"Closed {okCleanup.Value} expired sessions.");
}
```

---

## Notes

- **Authorization:** All endpoints assume the caller has been authenticated and authorized as a bot administrator. The controller itself does not enforce this — an external authorization filter or middleware is expected to gate access.
- **Thread Safety:** The controller is not inherently thread-safe. It relies on the thread safety guarantees of the underlying services (command registry, session store, user repository). Concurrent calls to `PromoteToAdmin` and `DemoteAdmin` for the same user, or to `BanUser` and `UnbanUser`, may race; the underlying store should use optimistic concurrency or locking to maintain consistency.
- **Idempotency:** `PromoteToAdmin` and `BanUser` throw `InvalidOperationException` when the target user is already in the desired state, making them non-idempotent by design. Callers should check current state via `GetAdministrators` or an equivalent user status endpoint before invoking these methods if idempotent behavior is required.
- **Command Registration Conflicts:** `RegisterCommand` validates uniqueness at call time. In a multi-instance deployment sharing a persistent command store, a race condition could still cause a duplicate key violation at the storage layer. The caller should handle 409-style conflict responses gracefully.
- **Session Cleanup:** `CloseExpiredSessions` is designed for manual administrative invocation. In production, a background job or scheduled task should normally handle expired session eviction; this endpoint serves as an override for immediate cleanup needs.
- **Return Types:** All methods return `IActionResult`, which at runtime will be concrete types such as `OkObjectResult`, `BadRequestObjectResult`, `NotFoundResult`, or `StatusCodeResult`. Callers should inspect the status code and body to determine success or failure.
