# BroadcastService
The `BroadcastService` class is designed to handle broadcasting messages to multiple users or chats in a Telegram bot. It provides methods for sending messages to a list of users or chats, as well as retrieving rate limit statistics. This service is useful for bots that need to send notifications or updates to a large number of users.

## API
* `public BroadcastService`: The constructor for the `BroadcastService` class.
* `public async Task<BroadcastResult> BroadcastAsync`: Sends a broadcast message to a list of chats or users. This method returns a `BroadcastResult` object, which contains information about the success or failure of the broadcast. It throws a `BroadcastException` if an error occurs during the broadcast.
* `public async Task<BroadcastResult> BroadcastToUsersAsync`: Sends a broadcast message to a list of users. This method returns a `BroadcastResult` object, which contains information about the success or failure of the broadcast. It throws a `BroadcastException` if an error occurs during the broadcast.
* `public RateLimitStats GetRateLimitStats`: Retrieves the current rate limit statistics for the bot. This method returns a `RateLimitStats` object, which contains information about the number of requests that can be made within a certain time period.
* `public void Dispose`: Disposes of the `BroadcastService` object and releases any unmanaged resources.
* `public BroadcastException`: An exception that is thrown when an error occurs during a broadcast.

## Usage
Here are two examples of using the `BroadcastService` class:
```csharp
// Example 1: Broadcasting a message to a list of chats
var broadcastService = new BroadcastService();
var chats = new List<long> { 123456789, 987654321 };
var message = "Hello, world!";
var result = await broadcastService.BroadcastAsync(chats, message);
Console.WriteLine($"Broadcast result: {result.SuccessfulChats} successful chats, {result.FailedChats} failed chats");

// Example 2: Broadcasting a message to a list of users
var broadcastService = new BroadcastService();
var users = new List<long> { 123456789, 987654321 };
var message = "Hello, world!";
var result = await broadcastService.BroadcastToUsersAsync(users, message);
Console.WriteLine($"Broadcast result: {result.SuccessfulUsers} successful users, {result.FailedUsers} failed users");
```

## Notes
* The `BroadcastService` class is not thread-safe, and should not be used concurrently from multiple threads.
* The `BroadcastAsync` and `BroadcastToUsersAsync` methods may throw a `BroadcastException` if an error occurs during the broadcast. This exception can be caught and handled by the calling code.
* The `GetRateLimitStats` method returns a `RateLimitStats` object, which contains information about the number of requests that can be made within a certain time period. This information can be used to avoid exceeding the rate limit and causing errors.
* The `Dispose` method should be called when the `BroadcastService` object is no longer needed, to release any unmanaged resources.
