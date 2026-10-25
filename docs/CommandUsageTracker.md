# CommandUsageTracker

Thread-safe, in-memory service that counts how often each bot command is invoked and when it was last and first used.

Namespace: `TelegramBotFramework.Services`
Visibility: `public sealed class`
Implements: `ICommandUsageTracker`
Source: `src/TelegramBotFramework/Services/CommandUsageTracker.cs`

## Purpose

`CommandService` calls `RecordCommandInvocation` each time it dispatches a command, after the cooldown check and before the command runs. The tracker keeps the counts so the bot can report which commands are used most and which have gone unused.

Use the tracker when you want to:

- Show the most-used commands, for example in an admin report or a `/stats` command.
- Check when a specific command was last used.
- Read every command's counters, for example to export them.

Everything is kept in memory. Restarting the process clears all counters.

## Registration

`DependencyInjectionSetup.AddTelegramBotFramework` registers the tracker as a singleton:

```csharp
services.AddSingleton<ICommandUsageTracker, CommandUsageTracker>();
```

Inject `ICommandUsageTracker`, not the concrete class, so tests can substitute a mock.

## Public members

### Constructor

`CommandUsageTracker()` takes no arguments. The default constructor is used, and the class has no other constructors.

### `RecordCommandInvocation(string commandName)`

Adds one invocation to the command's counter.

- Returns without doing anything if `commandName` is `null`, empty, or whitespace.
- Adds a leading `/` if the name does not already start with one. So `"start"` and `"/start"` count toward the same entry. The name is otherwise case-sensitive.
- Creates the entry on first use with `TotalInvocations = 1`, and sets `FirstUsedAt` and `LastUsedAt` to the current UTC time. Later calls increment `TotalInvocations` and update `LastUsedAt`.
- Runs the cleanup described under [Cleanup](#cleanup) first, if an hour has passed since the last cleanup.

### `GetTopCommands(int count)`

Returns up to `count` entries as `IList<(string CommandName, int InvocationCount)>`, sorted by `InvocationCount` from highest to lowest.

- Returns an empty list if `count` is `0` or negative.
- `CommandName` includes the leading `/`, for example `"/start"`.
- Commands with equal counts can appear in any order.

### `GetLastUsedTimestamp(string commandName)`

Returns the UTC time of the most recent invocation of `commandName`, or `null` if the command has no entry.

- Returns `null` if `commandName` is `null`, empty, or whitespace.
- Normalizes the name the same way as `RecordCommandInvocation`, so `"start"` and `"/start"` give the same result.

### `GetAllCommandStats()`

Returns an `IDictionary<string, CommandUsageStats>` keyed by command name, including the leading `/`.

- The dictionary is a new copy, so adding or removing keys does not affect the tracker.
- The `CommandUsageStats` values are the tracker's own objects, not copies. Changing a value's properties changes the tracker's data. Treat the values as read-only.

## CommandUsageStats

Defined in `ICommandUsageTracker.cs` in the same namespace.

| Property | Type | Meaning |
|----------|------|---------|
| `TotalInvocations` | `int` | Number of recorded invocations. |
| `FirstUsedAt` | `DateTime?` | UTC time of the first recorded invocation. |
| `LastUsedAt` | `DateTime?` | UTC time of the most recent invocation. |

## Cleanup

Entries that have not been used for more than 30 days are removed. Cleanup runs inside `RecordCommandInvocation`, at most once per hour, so no background timer is needed. The first cleanup happens an hour after the tracker is created. Its job is to stop the dictionary from growing without limit when many command names are seen.

Because the 30-day check looks at `LastUsedAt`, a command that is still being used keeps its entry and its counter. Removing an entry also resets its counter. A command that is used again after removal starts again from `1`.

## Thread safety

- Counters are stored in a `ConcurrentDictionary`. Each increment goes through `AddOrUpdate`, so concurrent calls to `RecordCommandInvocation` do not lose counts.
- The cleanup step takes a lock. The check that decides whether cleanup is due does not, so two threads can both start a cleanup at the same time. That is harmless: removing an entry twice has no effect.
- `GetTopCommands`, `GetLastUsedTimestamp`, and `GetAllCommandStats` read the live dictionary without a lock. They always return consistent entries, but a result may not include an invocation that is being recorded at that moment.

## Usage example

Recording invocations and reading the results:

```csharp
using TelegramBotFramework.Services;

ICommandUsageTracker tracker = new CommandUsageTracker();

// Record invocations. The leading "/" is optional.
tracker.RecordCommandInvocation("/start");
tracker.RecordCommandInvocation("help");
tracker.RecordCommandInvocation("/start");

// Top two commands: [("/start", 2), ("/help", 1)]
var top = tracker.GetTopCommands(2);
foreach (var (name, count) in top)
{
    Console.WriteLine($"{name}: {count}");
}

// Last use of "/start" (same result for "start").
DateTime? lastUsed = tracker.GetLastUsedTimestamp("start");

// Full statistics for every command seen so far.
foreach (var (name, stats) in tracker.GetAllCommandStats())
{
    Console.WriteLine($"{name}: {stats.TotalInvocations} uses, first {stats.FirstUsedAt:u}, last {stats.LastUsedAt:u}");
}
```

Injecting the tracker into a service, for example an admin command handler:

```csharp
public sealed class StatsReporter
{
    private readonly ICommandUsageTracker _usageTracker;

    public StatsReporter(ICommandUsageTracker usageTracker)
    {
        _usageTracker = usageTracker;
    }

    public string BuildTopCommandsReport(int count)
    {
        var lines = _usageTracker
            .GetTopCommands(count)
            .Select(entry => $"{entry.CommandName}: {entry.InvocationCount}");

        return string.Join(Environment.NewLine, lines);
    }
}
```

## Related

- `ICommandUsageTracker` - the interface, in `ICommandUsageTracker.cs`.
- `CommandService` - the caller that records each dispatched command.
