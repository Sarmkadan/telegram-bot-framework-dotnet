# CommandMenuSyncService

`CommandMenuSyncService` builds the bot's command menu from the framework's command declarations and publishes it to Telegram. It scans the loaded assemblies for classes decorated with `[Command]`, turns each one into a `BotCommand`, and calls `ITelegramApiClient.SetMyCommandsAsync` so the commands appear in the Telegram client's menu. The class is declared in `src/TelegramBotFramework/Services/CommandMenuSyncService.cs`.

> **Note:** `CommandMenuSyncService` is `public sealed`. The framework's DI setup does not register it, so create it yourself and pass it an `ITelegramApiClient`.

## Purpose

Telegram shows the `/` command menu from a list that the bot uploads with the `setMyCommands` method. Without this service, that list has to be written by hand and kept in step with the command classes. `CommandMenuSyncService` derives the list from the `[Command]` attributes, so the menu matches the commands the bot declares.

Each synchronization replaces the menu on Telegram's side with the list collected at that moment.

## Declaring commands

The service reads only the `[Command]` attribute on classes:

```csharp
[Command("start", Description = "Start the bot")]
public sealed class StartCommand
{
    // ...
}
```

The attribute is declared as follows (`src/TelegramBotFramework/Attributes/CommandAttribute.cs`):

- `AttributeTargets.Class`, `Inherited = false`, `AllowMultiple = false`. Only classes that carry the attribute directly are found. A subclass does not inherit the attribute from its base class.
- `Name` is set by the constructor, which strips leading `/` characters.
- `Description` is an optional property. If it is `null`, the menu entry gets an empty description.
- `Aliases` are not included in the menu. Only `Name` is published.

## API

### Constructor

```csharp
public CommandMenuSyncService(ITelegramApiClient apiClient)
```

Stores the client that will receive the command list. The constructor does not call the API and does not validate the argument.

### SyncAsync

```csharp
public Task<bool> SyncAsync(CancellationToken cancellationToken = default)
```

Collects all `[Command]` declarations and sends them to Telegram.

- **Parameters:** `cancellationToken` is checked once, before collection starts. If it is already cancelled, the method throws `OperationCanceledException`. The token is also passed to `SetMyCommandsAsync`.
- **Returns:** `true` if Telegram accepted the list, meaning `ApiResult<bool>.Success` is `true`. `false` means the API call returned an error. The error details are not returned. Check the `ApiResult` through the client if you need them.
- **Exceptions:** Exceptions from `SetMyCommandsAsync` (for example, network failures) propagate to the caller. The method does not catch them.

## How the command list is built

The collection step works as follows:

1. It enumerates `AppDomain.CurrentDomain.GetAssemblies()` and calls `GetTypes()` on each assembly.
2. It keeps the types that have a `[Command]` attribute.
3. For each type, it trims the `Name` and `Description`. A `null` description becomes an empty string.
4. It adds a `BotCommand(Name, Description)` only when `Name` is not empty after trimming. Entries with an empty name are skipped.

The method does not remove duplicates. If two classes declare the same name, Telegram receives both entries. The order follows the assembly and type enumeration order, which is not guaranteed.

Because the whole loaded domain is scanned, a `[Command]` class in any loaded assembly is included, including ones from referenced libraries.

> **Warning:** If no class has a `[Command]` attribute, `SyncAsync` sends an empty list. Telegram treats that as a request to clear the menu. `GetTypes()` can also throw `ReflectionTypeLoadException` if an assembly has a missing dependency. The service does not catch that exception.

## Usage example

A typical call, after the bot's command classes are loaded:

```csharp
using TelegramBotFramework.Integration;
using TelegramBotFramework.Services;

public sealed class BotStartup
{
    private readonly ITelegramApiClient _apiClient;

    public BotStartup(ITelegramApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task PublishCommandMenuAsync(CancellationToken cancellationToken)
    {
        var menuSync = new CommandMenuSyncService(_apiClient);

        bool accepted = await menuSync.SyncAsync(cancellationToken).ConfigureAwait(false);

        if (!accepted)
        {
            // Telegram rejected the list. Log it or retry later.
        }
    }
}
```

Call `SyncAsync` once at startup, or again after command definitions change. It is not called automatically by the framework.

## Related types

- `ITelegramApiClient.SetMyCommandsAsync(IReadOnlyList<BotCommand>, CancellationToken)`: the API call the service makes. It returns `ApiResult<bool>`.
- `BotCommand` (`src/TelegramBotFramework/Integration/ITelegramApiClient.cs`): `record BotCommand(string Command, string Description)`.
- `CommandAttribute` (`src/TelegramBotFramework/Attributes/CommandAttribute.cs`): the declaration the service reads.
