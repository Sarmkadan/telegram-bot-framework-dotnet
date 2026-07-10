# CommandServiceTests

`CommandServiceTests` is a unit test class that validates the behavior of `CommandService` within the telegram-bot-framework-dotnet library. It contains asynchronous test methods that cover command retrieval, execution with disabled or permission‑restricted commands, rate‑limiting checks, and registration of invalid commands. Each test method follows the Arrange‑Act‑Assert pattern and is designed to run under a test framework such as xUnit or NUnit.

## API

### `public CommandServiceTests`

The default constructor. No parameters. Initializes a new instance of the test class. Typically used by the test runner to instantiate the class before each test method.

### `public async Task GetCommandAsync_WhenExists_ReturnsCommand`

Tests that `CommandService.GetCommandAsync` returns the expected command instance when a command with the given identifier exists in the service’s registry.

- **Parameters**: None.
- **Returns**: `Task` – the test passes if the command is returned; otherwise it fails.
- **Throws**: No exceptions are expected; the test itself may throw if an assertion fails.

### `public async Task GetCommandAsync_WhenDoesNotExist_ReturnsNull`

Tests that `CommandService.GetCommandAsync` returns `null` when no command matches the provided identifier.

- **Parameters**: None.
- **Returns**: `Task` – the test passes if `null` is returned; otherwise it fails.
- **Throws**: No exceptions are expected.

### `public async Task ExecuteCommandAsync_WhenCommandIsDisabled_AddsErrorToContext`

Tests that executing a disabled command via `CommandService.ExecuteCommandAsync` results in an error being added to the execution context (e.g., a `CommandContext` object). The test verifies that the command is not actually invoked.

- **Parameters**: None.
- **Returns**: `Task` – the test passes if the context contains an appropriate error; otherwise it fails.
- **Throws**: No exceptions are expected.

### `public async Task ExecuteCommandAsync_WithInsufficientPermissions_AddsErrorToContext`

Tests that executing a command for which the caller lacks sufficient permissions causes an error to be added to the execution context. The command itself is not executed.

- **Parameters**: None.
- **Returns**: `Task` – the test passes if the context contains a permission‑related error; otherwise it fails.
- **Throws**: No exceptions are expected.

### `public async Task IsCommandRateLimitedAsync_WhenExceedsLimit_ReturnsTrue`

Tests that `CommandService.IsCommandRateLimitedAsync` returns `true` when the command has been invoked more times than the configured rate limit allows within the sliding window.

- **Parameters**: None.
- **Returns**: `Task` – the test passes if the method returns `true`; otherwise it fails.
- **Throws**: No exceptions are expected.

### `public async Task IsCommandRateLimitedAsync_WhenWithinLimit_ReturnsFalse`

Tests that `CommandService.IsCommandRateLimitedAsync` returns `false` when the command has not yet exceeded the rate limit.

- **Parameters**: None.
- **Returns**: `Task` – the test passes if the method returns `false`; otherwise it fails.
- **Throws**: No exceptions are expected.

### `public async Task RegisterCommandAsync_WithInvalidCommand_ThrowsException`

Tests that `CommandService.RegisterCommandAsync` throws an exception when provided with a command object that does not meet the service’s validation criteria (e.g., missing required attributes, null properties, or duplicate identifiers).

- **Parameters**: None.
- **Returns**: `Task` – the test passes if the expected exception is thrown; otherwise it fails.
- **Throws**: The test itself does not throw; it expects the method under test to throw.

## Usage

The following examples demonstrate how `CommandServiceTests` is typically used within a test project. Both examples assume the test framework is xUnit and that the necessary dependencies (e.g., `CommandService`, mock `IChatService`, etc.) are set up in a constructor or a test initializer.

**Example 1: Testing command retrieval and rate limiting**

```csharp
[Fact]
public async Task GetCommandAndRateLimit_Integration()
{
    // Arrange
    var service = new CommandService(/* dependencies */);
    var command = new MyCommand();
    await service.RegisterCommandAsync(command);

    // Act & Assert – command exists
    var retrieved = await service.GetCommandAsync("mycommand");
    Assert.NotNull(retrieved);

    // Simulate rate limit exceedance
    for (int i = 0; i < 5; i++)
    {
        await service.ExecuteCommandAsync(/* context */);
    }
    bool isLimited = await service.IsCommandRateLimitedAsync("mycommand");
    Assert.True(isLimited);
}
```

**Example 2: Testing error handling for disabled and permission‑restricted commands**

```csharp
[Fact]
public async Task DisabledAndPermissionErrors_AreAddedToContext()
{
    // Arrange
    var service = new CommandService(/* dependencies */);
    var disabledCmd = new DisabledCommand();
    var restrictedCmd = new AdminOnlyCommand();
    await service.RegisterCommandAsync(disabledCmd);
    await service.RegisterCommandAsync(restrictedCmd);

    var context = new CommandContext(/* user, chat, etc. */);

    // Act – execute disabled command
    await service.ExecuteCommandAsync(disabledCmd, context);
    Assert.Contains(context.Errors, e => e.Code == ErrorCode.CommandDisabled);

    // Act – execute permission‑restricted command as a regular user
    context.Errors.Clear();
    await service.ExecuteCommandAsync(restrictedCmd, context);
    Assert.Contains(context.Errors, e => e.Code == ErrorCode.InsufficientPermissions);
}
```

## Notes

- **Thread safety**: The test methods themselves are not inherently thread‑safe because they rely on shared state (e.g., a single `CommandService` instance). When running tests in parallel, each test should create its own service instance to avoid interference. The `CommandService` implementation under test may or may not be thread‑safe; these tests do not validate concurrent access.
- **Edge cases**:  
  - `GetCommandAsync_WhenDoesNotExist_ReturnsNull` covers the case where the command identifier is null, empty, or whitespace – the test should ensure the service handles these gracefully.  
  - `RegisterCommandAsync_WithInvalidCommand_ThrowsException` should test multiple invalid states: null command, missing `CommandAttribute`, duplicate registration, and commands with invalid rate‑limit configurations.  
  - Rate‑limit tests (`IsCommandRateLimitedAsync_*`) depend on the time window; they may need to use a mocked time provider to avoid flakiness.  
- **Test isolation**: Each test method is expected to set up its own fixtures (e.g., mock dependencies, fresh `CommandService` instance) to avoid side effects. The class does not expose any shared setup or teardown methods.
