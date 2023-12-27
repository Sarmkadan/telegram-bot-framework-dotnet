# SessionService
The `SessionService` class is designed to manage user sessions in a telegram bot application. It provides methods for creating, retrieving, updating, and deleting sessions, as well as navigating to menus and recording session activity. This service is a crucial component in managing the state of user interactions with the bot.

## API
### Constructors
* `public SessionService()`: Initializes a new instance of the `SessionService` class.
* `public SessionService(MenuService menuService)`: Initializes a new instance of the `SessionService` class with the specified `MenuService`.

### Session Management
* `public async Task<Models.UserSession> CreateSessionAsync()`: Creates a new user session. Returns the created `UserSession` object.
* `public async Task<Models.UserSession?> GetActiveSessionAsync()`: Retrieves the active user session. Returns the active `UserSession` object, or `null` if no active session exists.
* `public Task<Models.UserSession?> GetSessionByIdAsync()`: Retrieves a user session by its ID. Returns the `UserSession` object, or `null` if no session with the specified ID exists.
* `public async Task<IList<Models.UserSession>> GetAllActiveSessionsAsync()`: Retrieves all active user sessions. Returns a list of active `UserSession` objects.
* `public async Task<IList<Models.UserSession>> GetSessionsByUserIdAsync()`: Retrieves all user sessions for a specific user. Returns a list of `UserSession` objects.
* `public async Task<bool> DeleteSessionAsync()`: Deletes a user session. Returns `true` if the session was deleted successfully, `false` otherwise.
* `public async Task<int> ExpireInactiveSessionsAsync()`: Expires all inactive user sessions. Returns the number of expired sessions.
* `public async Task<Models.UserSession?> GetSessionAsync()`: Retrieves a user session. Returns the `UserSession` object, or `null` if no session exists.
* `public async Task<bool> UpdateSessionContextAsync()`: Updates the context of a user session. Returns `true` if the context was updated successfully, `false` otherwise.
* `public async Task<string?> GetSessionContextAsync()`: Retrieves the context of a user session. Returns the session context, or `null` if no context exists.
* `public async Task<bool> CloseSessionAsync()`: Closes a user session. Returns `true` if the session was closed successfully, `false` otherwise.
* `public async Task<int> CloseExpiredSessionsAsync()`: Closes all expired user sessions. Returns the number of closed sessions.

### Menu Navigation
* `public async Task<Models.UserSession> NavigateToMenuAsync()`: Navigates to a menu in a user session. Returns the updated `UserSession` object.
* `public async Task RecordSessionActivityAsync()`: Records activity in a user session.

### Menu Service
* `public MenuService MenuService { get; }`: Gets the `MenuService` instance associated with this `SessionService`.
* `public async Task<Models.Menu?> GetMenuAsync()`: Retrieves a menu. Returns the `Menu` object, or `null` if no menu exists.
* `public async Task<Models.Menu> CreateMenuAsync()`: Creates a new menu. Returns the created `Menu` object.

## Usage
```csharp
// Create a new session
var sessionService = new SessionService();
var userSession = await sessionService.CreateSessionAsync();

// Navigate to a menu
await sessionService.NavigateToMenuAsync();
```

```csharp
// Get all active sessions
var sessionService = new SessionService();
var activeSessions = await sessionService.GetAllActiveSessionsAsync();
foreach (var session in activeSessions)
{
    Console.WriteLine($"Session ID: {session.Id}, User ID: {session.UserId}");
}
```

## Notes
* The `SessionService` class is designed to be thread-safe, allowing multiple threads to access and modify sessions concurrently.
* When creating a new session, the `CreateSessionAsync` method will throw an exception if the session cannot be created due to an underlying storage error.
* The `GetActiveSessionAsync` method will return `null` if no active session exists, and the `GetSessionByIdAsync` method will return `null` if no session with the specified ID exists.
* The `ExpireInactiveSessionsAsync` and `CloseExpiredSessionsAsync` methods will expire and close sessions based on their last activity timestamp, and will return the number of affected sessions.
* The `UpdateSessionContextAsync` method will throw an exception if the session context cannot be updated due to an underlying storage error.
* The `GetSessionContextAsync` method will return `null` if no session context exists.
* The `CloseSessionAsync` method will throw an exception if the session cannot be closed due to an underlying storage error.
