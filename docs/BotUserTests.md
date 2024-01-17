# BotUserTests

`BotUserTests` is the unit test suite for the `BotUser` domain entity in the Telegram Bot Framework. It validates user display name formatting, input validation rules, activity tracking, metadata storage, command authorization logic, execution counting, rate limiting behavior, and command pattern generation. The class ensures that `BotUser` behaves correctly across all its public contracts and edge cases.

## API

### GetDisplayName_WithFirstAndLastName_ReturnsFullName
Verifies that when both `FirstName` and `LastName` are set, `GetDisplayName()` returns them concatenated with a space separator.

### GetDisplayName_WithoutLastName_ReturnsFirstNameOnly
Verifies that when `LastName` is null or empty, `GetDisplayName()` returns only the `FirstName` without trailing whitespace.

### Validate_WithNonPositiveTelegramId_ThrowsInvalidOperationException
Ensures that calling `Validate()` on a `BotUser` whose `TelegramId` is zero or negative throws an `InvalidOperationException`.

### Validate_WithEmptyFirstName_ThrowsInvalidOperationException
Ensures that calling `Validate()` on a `BotUser` whose `FirstName` is null, empty, or whitespace throws an `InvalidOperationException`.

### UpdateActivity_IncrementsMessagesCount
Confirms that invoking `UpdateActivity()` increases the user's tracked message count by exactly one.

### SetMetadata_AndGetMetadata_RoundTripsValue
Tests that a value stored via `SetMetadata(key, value)` can be retrieved unchanged by `GetMetadata(key)`.

### GetMetadata_WhenKeyNotPresent_ReturnsNull
Verifies that `GetMetadata(key)` returns null when the specified key has never been set.

### SetMetadata_OverwritesExistingKey
Ensures that calling `SetMetadata(key, newValue)` on an already-existing key replaces the old value with the new one.

### CanExecuteBy_AdminCommandAndUserRole_ReturnsFalse
Asserts that a command configured with `Admin` required role returns `false` from `CanExecuteBy` when the user has the `User` role.

### CanExecuteBy_AdminCommandAndModeratorRole_ReturnsFalse
Asserts that a command configured with `Admin` required role returns `false` from `CanExecuteBy` when the user has the `Moderator` role.

### CanExecuteBy_AdminCommandAndAdminRole_ReturnsTrue
Asserts that a command configured with `Admin` required role returns `true` from `CanExecuteBy` when the user has the `Admin` role.

### CanExecuteBy_WhenCommandIsDisabled_ReturnsFalseForAnyRole
Verifies that a disabled command returns `false` from `CanExecuteBy` regardless of the user's role, even if the role would otherwise satisfy the command's requirements.

### RecordExecution_IncrementsExecutionCount
Confirms that calling `RecordExecution(commandName)` increases the execution counter for that command by one.

### RecordExecution_CalledMultipleTimes_AccumulatesCount
Ensures that multiple calls to `RecordExecution(commandName)` accumulate the count correctly rather than resetting it.

### IsRateLimited_WhenExecutionsAtLimit_ReturnsTrue
Tests that `IsRateLimited(commandName, limit, window)` returns `true` when the number of recorded executions within the time window equals or exceeds the limit.

### IsRateLimited_WhenExecutionsBelowLimit_ReturnsFalse
Tests that `IsRateLimited(commandName, limit, window)` returns `false` when the number of recorded executions within the time window is below the limit.

### IsRateLimited_WhenNoLimitConfigured_ReturnsFalseRegardlessOfCount
Verifies that when no rate limit is configured (limit is zero or negative), `IsRateLimited` returns `false` even after many executions.

### GetCommandPatterns_WithAlias_ReturnsBothNameAndAlias
Confirms that `GetCommandPatterns()` for a command that has an alias returns a collection containing both the primary command name and the alias.

### GetCommandPatterns_WithoutAlias_ReturnsOnlyName
Confirms that `GetCommandPatterns()` for a command without an alias returns a collection containing only the primary command name.

### Validate_StandardCommandMissingLeadingSlash_ThrowsInvalidOperationException
Ensures that validating a standard command definition whose name does not start with `/` throws an `InvalidOperationException`.

## Usage

### Testing user display name formatting
```csharp
[TestMethod]
public void GetDisplayName_FormatsCorrectly()
{
    var userWithFullName = new BotUser
    {
        FirstName = "John",
        LastName = "Doe"
    };
    Assert.AreEqual("John Doe", userWithFullName.GetDisplayName());

    var userWithoutLastName = new BotUser
    {
        FirstName = "Alice",
        LastName = null
    };
    Assert.AreEqual("Alice", userWithoutLastName.GetDisplayName());
}
```

### Testing command authorization and rate limiting together
```csharp
[TestMethod]
public void CommandAuthorization_AndRateLimiting_IntegrationScenario()
{
    var adminUser = new BotUser { Role = UserRole.Admin };
    var command = new BotCommand { RequiredRole = UserRole.Admin };

    // Authorization
    Assert.IsTrue(adminUser.CanExecuteBy(command));

    // Record executions up to the limit
    for (int i = 0; i < 5; i++)
        adminUser.RecordExecution(command.Name);

    Assert.IsTrue(adminUser.IsRateLimited(command.Name, limit: 5, window: TimeSpan.FromMinutes(1)));

    // Disabled command should block even admins
    command.IsDisabled = true;
    Assert.IsFalse(adminUser.CanExecuteBy(command));
}
```

## Notes

- **Validation order**: `Validate()` checks multiple conditions; the exact order in which violations are detected (TelegramId vs. FirstName vs. command format) is not guaranteed by the test signatures and should not be relied upon.
- **Metadata thread safety**: The `SetMetadata`/`GetMetadata` tests imply a dictionary-backed store. If `BotUser` is accessed concurrently, external synchronization is required unless the implementation uses a `ConcurrentDictionary`.
- **Rate limiting window**: `IsRateLimited` operates on a sliding time window. Tests that call `RecordExecution` and immediately check `IsRateLimited` assume the window is large enough to contain all recorded timestamps. In production, timestamps outside the window are excluded automatically.
- **Command patterns casing**: `GetCommandPatterns` returns patterns suitable for matching; consumers should not assume case-insensitivity unless the framework explicitly normalizes them.
- **Disabled commands**: A disabled command returns `false` from `CanExecuteBy` for all roles, including `Admin`. This is a hard override that takes precedence over role checks.
