# PollingStrategy

The `PollingStrategy` class provides a mechanism for automating the retrieval and processing of updates from the Telegram Bot API. It manages the lifecycle of the polling loop, tracks the status of the connection, and maintains state information regarding the most recently processed updates to ensure sequential and reliable handling of incoming data.

## API

### PollingStrategy()
Initializes a new instance of the `PollingStrategy` class.

### Start()
`public void Start()`
Initiates the polling process. If the polling loop is already active, this method performs no action.

### StopAsync()
`public async Task StopAsync()`
Asynchronously terminates the polling process. Returns a `Task` representing the asynchronous operation. It is recommended to await this method to ensure the polling loop has completely stopped before proceeding.

### GetStatus()
`public PollingStatus GetStatus()`
Returns the current `PollingStatus` of the strategy, indicating whether it is running, stopped, or in an error state.

### LastPollTime
`public DateTime? LastPollTime { get; }`
Gets the timestamp of the last successful polling request. Returns `null` if no polling request has been made yet.

### ProcessUpdateAsync()
`public async Task ProcessUpdateAsync()`
Asynchronously triggers the processing of the next available update from the Telegram API. Returns a `Task` representing the asynchronous operation.

### IsRunning
`public bool IsRunning { get; }`
Gets a value indicating whether the polling loop is currently active.

### LastUpdateId
`public long LastUpdateId { get; }`
Gets the identifier of the last update that was successfully processed by the strategy.

## Usage

### Basic Lifecycle Management
```csharp
var pollingStrategy = new PollingStrategy();

// Start the polling process
pollingStrategy.Start();

// Perform other application tasks...

// Gracefully shut down polling
await pollingStrategy.StopAsync();
```

### Monitoring Status
```csharp
var pollingStrategy = new PollingStrategy();
pollingStrategy.Start();

// Check strategy health
if (pollingStrategy.IsRunning)
{
    var status = pollingStrategy.GetStatus();
    Console.WriteLine($"Polling is active. Status: {status}, Last Update ID: {pollingStrategy.LastUpdateId}");
}
```

## Notes

- **Thread Safety**: The `PollingStrategy` class is not inherently thread-safe. Accessing or modifying its state from multiple threads simultaneously may lead to unpredictable behavior. It is recommended to manage the lifecycle of the strategy from a single control thread.
- **Async Operations**: Methods suffixed with `Async` perform I/O-bound operations against the Telegram API. These methods should always be awaited to prevent potential deadlocks and ensure that the polling state remains consistent.
- **`StopAsync` Behavior**: Calling `StopAsync` while the strategy is already stopped is a safe operation and will not throw an exception.
- **`Start` Behavior**: If `Start` is called while the strategy is already running, the request is ignored, ensuring that multiple concurrent polling loops are not inadvertently created.
