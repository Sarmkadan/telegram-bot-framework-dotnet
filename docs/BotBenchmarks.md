# BotBenchmarks
The `BotBenchmarks` type is designed to provide a set of methods for benchmarking the performance of a Telegram bot. It allows developers to measure the execution time of various bot operations, such as processing messages and managing user sessions. This can be useful for identifying performance bottlenecks and optimizing the bot's code.

## API
The `BotBenchmarks` type has the following public members:
* `public BotBenchmarks`: The constructor for the `BotBenchmarks` type.
* `public void Setup`: Sets up the benchmarking environment. This method does not take any parameters and does not return a value. It may throw exceptions if the setup process fails.
* `public async Task<TelegramBotFramework.Models.ExecutionContext> ProcessMessageBenchmark`: Benchmarks the processing of a message. This method does not take any parameters and returns a `TelegramBotFramework.Models.ExecutionContext` object. It may throw exceptions if the benchmarking process fails.
* `public async Task<TelegramBotFramework.Models.UserSession> GetUserSessionBenchmark`: Benchmarks the retrieval of a user session. This method does not take any parameters and returns a `TelegramBotFramework.Models.UserSession` object. It may throw exceptions if the benchmarking process fails.
* `public async Task<bool> EndUserSessionBenchmark`: Benchmarks the ending of a user session. This method does not take any parameters and returns a boolean value indicating whether the operation was successful. It may throw exceptions if the benchmarking process fails.
* `public static void Main`: The main entry point for the `BotBenchmarks` type. This method does not take any parameters and does not return a value.

## Usage
Here are two examples of how to use the `BotBenchmarks` type:
```csharp
// Example 1: Benchmarking message processing
var benchmarks = new BotBenchmarks();
benchmarks.Setup();
var executionContext = await benchmarks.ProcessMessageBenchmark();
Console.WriteLine($"Message processing took {executionContext.ExecutionTime}ms");

// Example 2: Benchmarking user session management
var benchmarks = new BotBenchmarks();
benchmarks.Setup();
var userSession = await benchmarks.GetUserSessionBenchmark();
Console.WriteLine($"User session retrieval took {userSession.RetrievalTime}ms");
await benchmarks.EndUserSessionBenchmark();
Console.WriteLine("User session ended successfully");
```

## Notes
The `BotBenchmarks` type is designed to be used in a single-threaded environment. Using it in a multi-threaded environment may lead to inconsistent results or exceptions. Additionally, the `Setup` method should be called before any benchmarking methods to ensure that the environment is properly set up. The `ProcessMessageBenchmark`, `GetUserSessionBenchmark`, and `EndUserSessionBenchmark` methods may throw exceptions if the benchmarking process fails, and should be handled accordingly. The `Main` method is the entry point for the `BotBenchmarks` type and should be used to start the benchmarking process.
