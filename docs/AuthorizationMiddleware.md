# AuthorizationMiddleware

`AuthorizationMiddleware` is a bot pipeline component (`IBotMiddleware`) that decides whether the user who sent an update may run the command in that update. It checks the command's `RequiresAdmin` flag against the user's `UserRole`. If the user is below `Admin`, the middleware marks the `ExecutionContext` invalid and stops the pipeline before the command runs.

The middleware does not authenticate anyone. It assumes the `ExecutionContext.User` has already been loaded, which is why it must run after the user-loading step. It also does not handle HTTP requests; for the ASP.NET Core API key check, see [AuthenticationMiddleware](AuthenticationMiddleware.md).

Namespace: `TelegramBotFramework.Middleware`
Source: `src/TelegramBotFramework/Middleware/AuthorizationMiddleware.cs`

> **Name clash.** `src/TelegramBotFramework/Events/AuthorizationMiddleware.cs` defines a different class with the same name (`TelegramBotFramework.Events.AuthorizationMiddleware`, an `EventMiddlewareBase<MessageReceivedEvent>`). This document covers only the `Middleware` namespace class.

## API

### `public AuthorizationMiddleware(IUserService userService, ICommandService commandService, ILogger<AuthorizationMiddleware> logger)`

Creates the middleware. All three parameters are resolved from dependency injection.

- **Parameters**
  - `userService`: Used by the framework's user services. Not called directly by the current implementation; the user is read from `ExecutionContext.User`.
  - `commandService`: Looks up the `Command` definition for `ExecutionContext.Command.Name` through `GetCommandAsync`.
  - `logger`: Records denied and misconfigured requests.
- **Exceptions**
  - `ArgumentNullException`: any argument is `null`.

### `public int Priority { get; }`

Always `30`. The bot orchestrator sorts middleware by `Priority` in descending order, so higher values run earlier. In the default pipeline (`DependencyInjectionSetup`), this places authorization after `BotErrorHandlingMiddleware` and `BotLoggingMiddleware`, and before `RateLimitingMiddleware` (`Priority` 20).

### `public async Task<Models.ExecutionContext> ProcessAsync(Models.ExecutionContext context, Func<Models.ExecutionContext, Task<Models.ExecutionContext>> next, CancellationToken cancellationToken)`

Runs the authorization check and either passes the context to `next` or returns it without calling `next`.

- **Parameters**
  - `context`: The execution context for the current update.
  - `next`: The next middleware in the pipeline. Called only when the request is allowed.
  - `cancellationToken`: Passed to `ICommandService.GetCommandAsync`.
- **Return Value**
  The context returned by `next`, or the original context when the request is rejected.
- **Exceptions**
  - Exceptions from `ICommandService.GetCommandAsync` are not caught here and propagate to the caller.

## Behavior

The checks run in this order:

1. **Invalid context.** If `context.IsValid` is `false`, the context is returned unchanged and `next` is not called. Earlier middleware has already rejected this update.
2. **Missing user.** If `context.User` is `null`, the middleware calls `AddError("User not found in context for authorization.")`, logs a warning with `context.UserId`, and returns without calling `next`.
3. **Command check.** If `context.Command` is `null` (the update is not a command), the middleware calls `next`. Otherwise it looks up the command by `context.Command.Name`:
   - If the command exists, has `RequiresAdmin = true`, and `context.User.Role` is below `UserRole.Admin`, the middleware calls `AddError`, logs a warning, and returns without calling `next`.
   - If the command is not registered (`GetCommandAsync` returns `null`), the middleware does not reject it. It calls `next` and leaves the unknown-command handling to later middleware and the orchestrator.
4. **Allowed.** In every other case, the middleware calls `next(context)` and returns its result.

### Role rules

Only `RequiresAdmin` is enforced. The check compares `UserRole` values numerically:

| `UserRole` | Value | Runs `RequiresAdmin` commands |
|------------|-------|-------------------------------|
| `User` | 0 | No |
| `Moderator` | 1 | No |
| `Admin` (`Administrator`) | 2 | Yes |
| `Owner` | 3 | Yes |

Commands without `RequiresAdmin` are allowed for every role, including users who have no role assigned.

### Rejected requests

When the middleware rejects a request, it:

- calls `context.AddError(...)`, which sets `IsValid = false`;
- logs a warning with the user's `TelegramId` and the command name (or the `UserId` for a missing user);
- does **not** call `next`.

The orchestrator only executes a command when the final context is valid, so a rejected command is not run.

## Usage

### Pipeline registration

`AddTelegramBotFramework` registers the middleware in the default pipeline; you do not need to add it yourself. The default order is defined in `Configuration/DependencyInjectionSetup.cs`:

```csharp
var middlewarePipeline = new MiddlewarePipelineBuilder()
    .Use<Middleware.BotErrorHandlingMiddleware>()
    .Use<Middleware.BotLoggingMiddleware>()
    .Use<Middleware.AuthorizationMiddleware>()
    .Use<Middleware.RateLimitingMiddleware>()
    .Build();
```

### Marking a command as admin-only

Set `RequiresAdmin = true` when you register the command. The middleware reads the flag from the registered `Command`, not from the handler.

```csharp
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;

// Inside a startup routine that has access to ICommandService.
var broadcastCommand = new Command
{
    Name = "/broadcast",
    Description = "Send a message to all users",
    HandlerType = "BroadcastCommandHandler",
    Type = CommandType.Standard,
    IsEnabled = true,
    RequiresAdmin = true
};

await commandService.RegisterCommandAsync(broadcastCommand);
```

### Granting a role

Roles are stored on `BotUser.Role`. Raise a user's role with `IUserService.UpdateUserAsync`, which takes the updated `BotUser`:

```csharp
var user = await userService.GetUserByTelegramIdAsync(adminTelegramId);
if (user is not null)
{
    user.Role = UserRole.Admin;
    await userService.UpdateUserAsync(user);
}
```

Use a trusted source for this (for example an owner-only command or a configuration step). Any code that can call `UpdateUserAsync` can change a user's role.

### Handling the rejection

The middleware does not send a reply to the user. It only sets an error on the context. To tell the user, handle the error in your own code, or check `context.Errors` after the pipeline finishes. Logging through `BotLoggingMiddleware` also records the request.

## Notes

- **Ordering.** Keep authorization after anything that loads `context.User` and after error handling, and before any middleware that performs work on behalf of the command. Because `Priority` is 30, it runs before `RateLimitingMiddleware` (20), so rejected admin commands do not consume rate-limit budget.
- **Scope.** Only `RequiresAdmin` is checked. There are no per-command role lists or custom permission claims in this class.
- **Thread safety.** The middleware holds only readonly dependencies and keeps no per-request state, so one instance can process concurrent updates.
