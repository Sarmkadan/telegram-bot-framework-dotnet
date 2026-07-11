# CommandServiceAdditionalTests

`CommandServiceAdditionalTests` is a test class that validates the behavior of the command service infrastructure in the Telegram Bot Framework for .NET. It focuses on verifying command availability based on user roles, command execution lifecycle, permission checks, execution counting, and rate-limiting logic. The tests ensure that the service correctly enforces role-based access, handles disabled commands, tracks execution metrics, and respects per-user rate limits.

## API

### public CommandServiceAdditionalTests

Default constructor. Initializes a new instance of the test class. No parameters, no return value, does not throw.

---

### public async Task GetAvailableCommandsAsync_WithAdminRole_ReturnsAdminCommands

**Purpose:** Verifies that when a user with the `Admin` role requests available commands, all commands (including admin-only commands) are returned.

**Parameters:** None (test method).

**Return value:** `Task` – completes when the assertion passes or fails.

**Throws:** Assertion exceptions if the returned command set does not match the expected admin-visible commands.

---

### public async Task GetAvailableCommandsAsync_WithUserRole_ReturnsOnlyNonAdminCommands

**Purpose:** Verifies that a standard `User` role receives only commands not restricted to higher privilege levels.

**Parameters:** None (test method).

**Return value:** `Task` – completes when the assertion passes or fails.

**Throws:** Assertion exceptions if admin-restricted commands appear in the result.

---

### public async Task GetAvailableCommandsAsync_WithModeratorRole_ReturnsCommandsForModeratorAndAbove

**Purpose:** Verifies that a `Moderator` receives commands available to their role and any commands inherited from lower roles, but not commands exclusive to `Admin`.

**Parameters:** None (test method).

**Return value:** `Task` – completes when the assertion passes or fails.

**Throws:** Assertion exceptions if the command set does not align with moderator-level permissions.

---

### public async Task ExecuteCommandAsync_WithValidContext_ExecutesSuccessfully

**Purpose:** Confirms that a command with a valid execution context (correct permissions, active command, active user) runs to completion without errors.

**Parameters:** None (test method).

**Return value:** `Task` – completes when the assertion passes or fails.

**Throws:** Assertion exceptions if the execution result indicates failure or errors are added to the context.

---

### public async Task ExecuteCommandAsync_WithDisabledCommand_AddsErrorToContext

**Purpose:** Ensures that attempting to execute a disabled command does not proceed and instead populates the context with an appropriate error entry.

**Parameters:** None (test method).

**Return value:** `Task` – completes when the assertion passes or fails.

**Throws:** Assertion exceptions if no error is recorded in the context.

---

### public async Task ExecuteCommandAsync_WithInsufficientPermissions_AddsErrorToContext

**Purpose:** Validates that when a user lacks the required role for a command, execution is blocked and an error is written to the context.

**Parameters:** None (test method).

**Return value:** `Task` – completes when the assertion passes or fails.

**Throws:** Assertion exceptions if the context remains error-free despite insufficient permissions.

---

### public async Task CanUserExecuteCommandAsync_WithInactiveUser_ReturnsFalse

**Purpose:** Checks that the permission evaluation returns `false` when the requesting user is marked as inactive.

**Parameters:** None (test method).

**Return value:** `Task` – completes when the assertion passes or fails.

**Throws:** Assertion exceptions if the result is `true`.

---

### public async Task CanUserExecuteCommandAsync_WithNonExistentCommand_ReturnsFalse

**Purpose:** Verifies that querying execution capability for a command name that does not exist yields `false`.

**Parameters:** None (test method).

**Return value:** `Task` – completes when the assertion passes or fails.

**Throws:** Assertion exceptions if the result is `true`.

---

### public async Task CanUserExecuteCommandAsync_WithDisabledCommand_ReturnsFalse

**Purpose:** Confirms that a disabled command is not considered executable, even for an otherwise eligible user.

**Parameters:** None (test method).

**Return value:** `Task` – completes when the assertion passes or fails.

**Throws:** Assertion exceptions if the result is `true`.

---

### public async Task RecordCommandExecutionAsync_WithValidCommand_IncrementsExecutionCount

**Purpose:** Tests that recording an execution for a known, active command increases its tracked execution count.

**Parameters:** None (test method).

**Return value:** `Task` – completes when the assertion passes or fails.

**Throws:** Assertion exceptions if the count does not increment.

---

### public async Task RecordCommandExecutionAsync_WithNonExistentCommand_DoesNotThrow

**Purpose:** Ensures that attempting to record an execution for a command name not present in the registry completes without throwing an exception (graceful no-op).

**Parameters:** None (test method).

**Return value:** `Task` – completes when the assertion passes or fails.

**Throws:** Assertion exceptions if the method throws any exception.

---

### public async Task GetCommandExecutionCountAsync_WithExistingCommand_ReturnsCount

**Purpose:** Validates that retrieving the execution count for a command that has recorded executions returns the correct accumulated value.

**Parameters:** None (test method).

**Return value:** `Task` – completes when the assertion passes or fails.

**Throws:** Assertion exceptions if the returned count does not match the expected value.

---

### public async Task GetCommandExecutionCountAsync_WithNonExistentCommand_ReturnsZero

**Purpose:** Verifies that querying the execution count for an unknown command name returns zero rather than throwing or returning a non-zero default.

**Parameters:** None (test method).

**Return value:** `Task` – completes when the assertion passes or fails.

**Throws:** Assertion exceptions if the result is not zero.

---

### public async Task IsCommandRateLimitedAsync_WithNoRateLimitConfigured_ReturnsFalse

**Purpose:** Confirms that when no rate limit is set for a command, the rate-limit check returns `false` (not rate-limited).

**Parameters:** None (test method).

**Return value:** `Task` – completes when the assertion passes or fails.

**Throws:** Assertion exceptions if the result is `true`.

---

### public async Task IsCommandRateLimitedAsync_WithMultipleUsers_ResetsRateLimitPerUser

**Purpose:** Ensures that rate limiting is scoped per user. A command under rate limit for one user does not affect another user’s ability to execute it, and each user’s window resets independently.

**Parameters:** None (test method).

**Return value:** `Task` – completes when the assertion passes or fails.

**Throws:** Assertion exceptions if the rate-limit state leaks across users.

## Usage

### Example 1: Role-Based Command Availability

```csharp
[TestMethod]
public async Task AdminSeesAllCommands_UserSeesSubset()
{
    var tests = new CommandServiceAdditionalTests();

    // Admin role should receive all registered commands
    await tests.GetAvailableCommandsAsync_WithAdminRole_ReturnsAdminCommands();

    // Standard user role should receive only non-admin commands
    await tests.GetAvailableCommandsAsync_WithUserRole_ReturnsOnlyNonAdminCommands();
}
```

### Example 2: Execution Counting and Rate Limiting

```csharp
[TestMethod]
public async Task TrackExecutionsAndCheckRateLimit()
{
    var tests = new CommandServiceAdditionalTests();

    // Record an execution and verify the count increments
    await tests.RecordCommandExecutionAsync_WithValidCommand_IncrementsExecutionCount();
    await tests.GetCommandExecutionCountAsync_WithExistingCommand_ReturnsCount();

    // Verify that without a configured rate limit, the command is not throttled
    await tests.IsCommandRateLimitedAsync_WithNoRateLimitConfigured_ReturnsFalse();

    // Ensure per-user rate-limit isolation
    await tests.IsCommandRateLimitedAsync_WithMultipleUsers_ResetsRateLimitPerUser();
}
```

## Notes

- All test methods are asynchronous and return `Task`. They are designed to be executed within a test runner that supports `async` test methods (e.g., MSTest, xUnit, NUnit).
- The tests assume a pre-configured command registry with commands assigned to specific roles (`User`, `Moderator`, `Admin`) and a known disabled command. Test setup is handled internally by the class or its base infrastructure.
- `RecordCommandExecutionAsync_WithNonExistentCommand_DoesNotThrow` explicitly validates defensive coding: the service must not throw for unknown command names during execution recording.
- `GetCommandExecutionCountAsync_WithNonExistentCommand_ReturnsZero` ensures that callers can safely query counts without first checking command existence.
- Rate-limiting tests confirm per-user isolation. The implementation must track rate-limit windows keyed by user identity, not globally. Concurrent calls for different users should not interfere.
- No thread-safety guarantees are implied by the signatures alone, but the rate-limit test with multiple users suggests the underlying service handles concurrent per-user tracking correctly. Tests should be run sequentially unless the test framework explicitly supports parallel execution with isolation.
