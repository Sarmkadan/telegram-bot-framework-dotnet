# SessionServiceTests

Unit test suite for the `SessionService` class within the `telegram-bot-framework-dotnet` project. This class verifies the correctness of session lifecycle management, including creation, retrieval, activity recording, navigation, closure, expiration, and deletion. All tests are asynchronous and target the service’s public contract under various conditions—existing sessions, missing sessions, and edge cases such as already-closed or inactive sessions.

## API

### `public SessionServiceTests`

Constructor. Initializes the test class. No parameters, no return value, does not throw.

---

### `public async Task GetActiveSessionAsync_WithExistingActiveSession_ReturnsSession`

**Purpose:** Verifies that `GetActiveSessionAsync` returns the session object when an active session exists for the given criteria.  
**Parameters:** None (self-contained test).  
**Return value:** `Task` (test method).  
**Throws:** Test assertion failures if the returned session is null or does not match the expected active session.

---

### `public async Task GetActiveSessionAsync_WithNoActiveSession_ReturnsNull`

**Purpose:** Confirms that `GetActiveSessionAsync` returns `null` when no active session is present.  
**Parameters:** None.  
**Return value:** `Task`.  
**Throws:** Test assertion failures if the result is not `null`.

---

### `public async Task CreateSessionAsync_CreatesNewSession`

**Purpose:** Ensures that `CreateSessionAsync` successfully creates a new session and that the session can be retrieved afterwards.  
**Parameters:** None.  
**Return value:** `Task`.  
**Throws:** Test assertion failures if the session is not created or cannot be found.

---

### `public async Task CreateSessionAsync_WithCustomTimeout_CreatesSessionWithCorrectExpiration`

**Purpose:** Validates that when a custom timeout is supplied to `CreateSessionAsync`, the resulting session’s expiration is set correctly relative to the creation time.  
**Parameters:** None.  
**Return value:** `Task`.  
**Throws:** Test assertion failures if the expiration does not match the expected value based on the custom timeout.

---

### `public async Task RecordSessionActivityAsync_UpdatesLastActivityAndIncrementsInteractionCount`

**Purpose:** Checks that calling `RecordSessionActivityAsync` updates the session’s last activity timestamp and increments the interaction counter.  
**Parameters:** None.  
**Return value:** `Task`.  
**Throws:** Test assertion failures if the last activity time or interaction count is not updated as expected.

---

### `public async Task RecordSessionActivityAsync_WithNonExistingSession_DoesNotThrow`

**Purpose:** Verifies that `RecordSessionActivityAsync` handles a non-existent session gracefully without throwing an exception.  
**Parameters:** None.  
**Return value:** `Task`.  
**Throws:** Test fails if the method throws any exception.

---

### `public async Task CloseSessionAsync_WithActiveSession_ClosesSessionAndReturnsTrue`

**Purpose:** Confirms that closing an active session succeeds, marks the session as closed, and returns `true`.  
**Parameters:** None.  
**Return value:** `Task`.  
**Throws:** Test assertion failures if the return value is not `true` or the session remains open.

---

### `public async Task CloseSessionAsync_WithAlreadyClosedSession_ReturnsFalse`

**Purpose:** Ensures that attempting to close a session that is already closed returns `false`.  
**Parameters:** None.  
**Return value:** `Task`.  
**Throws:** Test assertion failures if the result is not `false`.

---

### `public async Task CloseSessionAsync_WithNonExistingSession_ReturnsFalse`

**Purpose:** Verifies that closing a session ID that does not exist returns `false` and does not throw.  
**Parameters:** None.  
**Return value:** `Task`.  
**Throws:** Test assertion failures if the result is not `false` or an exception is thrown.

---

### `public async Task NavigateToMenuAsync_UpdatesCurrentMenuId`

**Purpose:** Tests that `NavigateToMenuAsync` correctly updates the `CurrentMenuId` property of the target session.  
**Parameters:** None.  
**Return value:** `Task`.  
**Throws:** Test assertion failures if the menu ID is not updated to the expected value.

---

### `public async Task GetSessionByIdAsync_WithExistingSession_ReturnsSession`

**Purpose:** Validates that `GetSessionByIdAsync` returns the correct session when the ID exists.  
**Parameters:** None.  
**Return value:** `Task`.  
**Throws:** Test assertion failures if the returned session is null or does not match.

---

### `public async Task GetSessionByIdAsync_WithNonExistingSession_ReturnsNull`

**Purpose:** Confirms that `GetSessionByIdAsync` returns `null` for an unknown session ID.  
**Parameters:** None.  
**Return value:** `Task`.  
**Throws:** Test assertion failures if the result is not `null`.

---

### `public async Task GetAllActiveSessionsAsync_ReturnsActiveSessions`

**Purpose:** Ensures that `GetAllActiveSessionsAsync` returns only sessions that are currently active, excluding closed or expired ones.  
**Parameters:** None.  
**Return value:** `Task`.  
**Throws:** Test assertion failures if the collection includes inactive sessions or misses active ones.

---

### `public async Task GetSessionsByUserIdAsync_ReturnsUserSessions`

**Purpose:** Verifies that `GetSessionsByUserIdAsync` returns all sessions belonging to a specific user.  
**Parameters:** None.  
**Return value:** `Task`.  
**Throws:** Test assertion failures if the returned set does not match the expected user sessions.

---

### `public async Task DeleteSessionAsync_WithExistingSession_DeletesAndReturnsTrue`

**Purpose:** Checks that deleting an existing session removes it from the store and returns `true`.  
**Parameters:** None.  
**Return value:** `Task`.  
**Throws:** Test assertion failures if the session still exists afterward or the return value is not `true`.

---

### `public async Task DeleteSessionAsync_WithNonExistingSession_ReturnsFalse`

**Purpose:** Confirms that attempting to delete a non-existent session returns `false` without side effects.  
**Parameters:** None.  
**Return value:** `Task`.  
**Throws:** Test assertion failures if the result is not `false` or an exception is thrown.

---

### `public async Task ExpireInactiveSessionsAsync_WithInactiveSessions_ClosesThem`

**Purpose:** Validates that `ExpireInactiveSessionsAsync` identifies sessions that have exceeded the inactivity threshold and closes them.  
**Parameters:** None.  
**Return value:** `Task`.  
**Throws:** Test assertion failures if inactive sessions remain open or active sessions are incorrectly closed.

## Usage

### Example 1: Running a subset of session lifecycle tests in a CI pipeline

```csharp
using Xunit;

public class SessionServiceRegressionTests
{
    private readonly SessionServiceTests _tests = new SessionServiceTests();

    [Fact]
    public async Task Lifecycle_Create_Record_Close_ShouldSucceed()
    {
        await _tests.CreateSessionAsync_CreatesNewSession();
        await _tests.RecordSessionActivityAsync_UpdatesLastActivityAndIncrementsInteractionCount();
        await _tests.CloseSessionAsync_WithActiveSession_ClosesSessionAndReturnsTrue();
    }

    [Fact]
    public async Task Retrieval_MissingSession_ShouldReturnNull()
    {
        await _tests.GetActiveSessionAsync_WithNoActiveSession_ReturnsNull();
        await _tests.GetSessionByIdAsync_WithNonExistingSession_ReturnsNull();
    }
}
```

### Example 2: Validating expiration and deletion logic in a test harness

```csharp
using Xunit;

public class SessionMaintenanceTests
{
    private readonly SessionServiceTests _tests = new SessionServiceTests();

    [Fact]
    public async Task Maintenance_ExpireAndDelete_ShouldCleanUpCorrectly()
    {
        await _tests.CreateSessionAsync_WithCustomTimeout_CreatesSessionWithCorrectExpiration();
        await _tests.ExpireInactiveSessionsAsync_WithInactiveSessions_ClosesThem();
        await _tests.DeleteSessionAsync_WithExistingSession_DeletesAndReturnsTrue();
        await _tests.DeleteSessionAsync_WithNonExistingSession_ReturnsFalse();
    }
}
```

## Notes

- **Idempotency and safety:** Methods like `RecordSessionActivityAsync` and `CloseSessionAsync` are designed to handle non-existent or already-closed sessions without throwing. Tests explicitly assert this behaviour (`ReturnsFalse`, `DoesNotThrow`), confirming the service does not leak exceptions for invalid input.
- **Expiration precision:** `CreateSessionAsync_WithCustomTimeout_CreatesSessionWithCorrectExpiration` implies that expiration is computed from the creation time plus the timeout. Tests should account for small clock skews if using `DateTime.UtcNow` comparisons.
- **State isolation:** Each test method is self-contained and expects a clean session store state. In practice, the test harness likely resets or isolates the underlying storage between tests to prevent cross-test contamination.
- **Thread safety:** The signatures do not expose any concurrent access patterns directly. However, the presence of `GetAllActiveSessionsAsync` and `ExpireInactiveSessionsAsync` suggests that production code may enumerate sessions while other operations mutate the store. Implementations should use appropriate synchronisation (e.g., locks, concurrent collections) to avoid enumeration exceptions or lost updates; the test suite implicitly validates correctness under sequential execution.
- **Return value conventions:** Boolean-returning methods (`CloseSessionAsync`, `DeleteSessionAsync`) use `true` to indicate a state change occurred and `false` when the target was already in the desired state or did not exist. This convention is consistently verified across the test methods.
