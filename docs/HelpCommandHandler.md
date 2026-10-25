# HelpCommandHandler

`src/TelegramBotFramework/Commands/HelpCommandHandler.cs` implements the built-in `/help` command. It lists every registered command handler that carries a `[Command]` attribute, using each attribute's name and description.

```csharp
public sealed class HelpCommandHandler : ICommandHandler, IHelpCommandHandler
```

It is annotated with `[Command("help", Description = "Show available commands")]`, so it also appears in its own listing.

## Purpose

The handler builds a plain-text command list for the user. It does not send a message itself. `HandleAsync` stores the text in the execution context under the state key `help_text`, and the caller decides how to deliver it.

The command list is built from the handlers that the DI container supplies as `IEnumerable<ICommandHandler>`. A handler appears in the list only if its class has a `CommandAttribute`. Handlers without the attribute are skipped.

## Constructor

```csharp
public HelpCommandHandler(
    IEnumerable<ICommandHandler> handlers,
    ILogger<HelpCommandHandler> logger)
```

- `handlers` is the set of command handlers to inspect. Throws `ArgumentNullException` if null.
- `logger` receives the informational log entries. Throws `ArgumentNullException` if null.

The constructor writes an `Information` entry, `HelpCommandHandler instantiated`.

## Public methods

### HandleAsync

```csharp
public Task<Models.ExecutionContext> HandleAsync(
    Models.ExecutionContext context,
    CancellationToken cancellationToken = default)
```

Calls `BuildHelpText()`, stores the result with `context.SetState("help_text", helpText)`, and returns a completed task holding the same `context`.

The `cancellationToken` parameter is accepted to match the interface, but it is not used. The method does no asynchronous work and never throws because of cancellation.

The handler reads `context.UserId` only for logging.

### BuildHelpText

```csharp
public string BuildHelpText()
```

Returns the help message. The output has this form:

```
Available commands:

/help — Show available commands
/start — Start the bot
/ping
```

Details of the format:

- The first line is `Available commands:`, followed by one blank line.
- Commands are sorted by their attribute name with LINQ `OrderBy`, which uses the default string comparer.
- Each line is `/{Name}`. When `Description` is not null, empty, or white space, the line becomes `/{Name} — {Description}`, with an em dash (U+2014) as the separator.
- The `Name` value is the one stored on the attribute. `CommandAttribute` strips any leading slash, so `[Command("/help")]` produces `/help`.
- `Aliases` are not listed.
- Trailing whitespace is removed from the final string.

When no handler has a `CommandAttribute`, the result is only the header line.

`BuildHelpText` logs `Building help text` and, on completion, `Built help text with {Length} characters`.

## Usage

### Registration

The DI setup in `Configuration/DependencyInjectionSetup.cs` registers the handler as a transient `ICommandHandler`:

```csharp
services.AddTransient<Commands.ICommandHandler, Commands.HelpCommandHandler>();
```

Command handlers that should appear in `/help` need a `[Command]` attribute:

```csharp
[Command("start", Description = "Start the bot")]
public sealed class StartCommandHandler : ICommandHandler
{
    public Task<Models.ExecutionContext> HandleAsync(
        Models.ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        // ...
        return Task.FromResult(context);
    }
}
```

The registration above adds the handler only as `ICommandHandler`. `IHelpCommandHandler` is not registered, so `GetRequiredService<IHelpCommandHandler>()` throws unless your application registers it. To use the handler directly, construct it yourself or register the interface in your own setup.

### Reading the result

Code that runs after the handler can read the text from the context:

```csharp
var handler = new HelpCommandHandler(handlers, logger);
var context = await handler.HandleAsync(executionContext, cancellationToken).ConfigureAwait(false);
var helpText = context.GetState<string>("help_text");
```

To get the text without an execution context, call `BuildHelpText()` directly:

```csharp
string text = handler.BuildHelpText();
```

## Notes

- `BuildHelpText()` reads the attributes by reflection on every call. The cost grows with the number of handlers.
- The method writes its own log entries on every call, so calling it often produces log noise at `Information` level.
