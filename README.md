# Telegram Bot Framework for .NET

A modern, opinionated framework for building scalable Telegram bots with .NET 10. Handles commands, menus, session state, middleware pipelines, and both webhook and polling integration with built-in rate limiting, caching, and conversation flows.

![Build](https://github.com/sarmkadan/telegram-bot-framework-dotnet/actions/workflows/build.yml/badge.svg) ![License](https://img.shields.io/github/license/sarmkadan/telegram-bot-framework-dotnet) ![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)

## Features

- Command routing with role-based access control
- Inline keyboard builder (fluent API)
- User sessions with context storage and expiry
- Conversation flow engine with durable state (in-memory or file-backed)
- Middleware pipeline: error handling, authorization, rate limiting
- Three rate limiting strategies: token bucket, sliding window, fixed window
- Event bus (pub/sub) for decoupled components
- Background task queue and scheduled task manager
- Webhook mode with secret-token validation and auto-registration
- In-memory cache provider with TTL support

## IRateLimitingStrategy

The `IRateLimitingStrategy` interface defines the contract for implementing rate limiting algorithms in the Telegram Bot Framework. It provides a consistent API for checking if requests are allowed and tracking remaining request capacity. The framework includes three built-in implementations: `TokenBucketStrategy`, `SlidingWindowStrategy`, and `FixedWindowStrategy`, each suitable for different traffic patterns and consistency requirements.

**Example usage:**

```csharp
using TelegramBotFramework.Strategies;

// Create a token bucket strategy with capacity of 100 tokens and refill rate of 10 tokens per second
var tokenBucketStrategy = new TokenBucketStrategy(bucketCapacity: 100, tokensPerSecond: 10);

// Check if a request from a specific user is allowed
string userIdentifier = "user_12345";
bool isAllowed = tokenBucketStrategy.IsRequestAllowed(userIdentifier);

if (isAllowed)
{
    Console.WriteLine("Request allowed - processing user request");
    // Process the request...
}
else
{
    Console.WriteLine("Rate limit exceeded - request denied");
}

// Get remaining requests for this user
int remainingRequests = tokenBucketStrategy.GetRemainingRequests(userIdentifier);
Console.WriteLine($"Remaining requests: {remainingRequests}");

// Create a sliding window strategy with 30 requests per 1-minute window
var slidingWindowStrategy = new SlidingWindowStrategy(
    requestsPerWindow: 30,
    windowDuration: TimeSpan.FromMinutes(1)
);

// Check if request is allowed
bool isSlidingAllowed = slidingWindowStrategy.IsRequestAllowed(userIdentifier);

// Create a fixed window strategy with 60 requests per minute
var fixedWindowStrategy = new FixedWindowStrategy(
    requestsPerWindow: 60,
    windowDuration: TimeSpan.FromMinutes(1)
);

// Check if request is allowed
bool isFixedAllowed = fixedWindowStrategy.IsRequestAllowed(userIdentifier);
```

## Architecture

The framework is a single assembly built around one idea: every update becomes an `ExecutionContext` that flows through a priority-ordered middleware pipeline into domain services backed by swappable repositories. Webhook and polling modes feed the same pipeline. Layers, design decisions with their trade-offs, data flow and extension points are documented in [docs/architecture.md](docs/architecture.md).

## Configuration

All configuration is done via `appsettings.json`. Here are the available settings:

| Key                          | Description                                      | Example Value              |
|------------------------------|--------------------------------------------------|----------------------------|
| `botToken`                   | Telegram bot token (required)                    | `123456789:ABCDEF...`      |
| `botUsername`                | Bot username (required)                          | `my_bot_username`          |
| `databaseConnectionString`   | Database connection string                       | `Server=localhost;...`     |
| `sessionTimeoutMinutes`     | Session timeout in minutes                       | `30`                       |
| `messageProcessingTimeoutSeconds` | Message processing timeout in seconds        | `10`                       |
| `maxConcurrentRequests`     | Maximum number of concurrent requests          | `10`                       |
| `enableLogging`              | Enable logging                                   | `true`                     |
| `enableRateLimiting`         | Enable rate limiting                             | `true`                     |
| `rateLimitPerMinute`         | Max requests per minute                          | `30`                       |

> ⚠️ Never commit actual secrets like `botToken` to version control.

## TelegramBotFrameworkDotnetOptions

The `TelegramBotFrameworkDotnetOptions` class provides a strongly-typed configuration object for initializing and managing the Telegram Bot Framework. It encapsulates essential settings such as bot credentials, database connections, and performance tuning parameters, allowing for cleaner configuration in code compared to JSON-only setups.

**Example usage**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Models;

// Configure the framework options in code
services.Configure<TelegramBotFrameworkDotnetOptions>(options =>
{
    options.BotToken = "123456789:ABCDEF";
    options.BotUsername = "my_bot_username";
    options.DatabaseConnectionString = "Server=localhost;Database=BotDb;";
    options.SessionTimeoutMinutes = 30;
    options.MessageProcessingTimeoutSeconds = 15;
    options.MaxConcurrentRequests = 20;
    options.EnableLogging = true;
    options.EnableRateLimiting = true;
    options.RateLimitPerMinute = 60;
});
```

## WebhookServiceExtensions

Provides extension methods for configuring and managing webhook services in Telegram bot applications. Includes methods for registering webhook services, checking registration status, and accessing webhook‑related options and statistics.

**Example usage**

```csharp
// Example: Configure webhook service (see WebhookOptions for details)
services.AddWebhookService(options => {
    options.Url = "https://mybot.com/webhook";
});
```

## WebhookService

The `WebhookService` is a production-ready hosted service that automatically registers and unregisters Telegram webhooks, and dispatches validated updates to subscribed handlers. It implements `IHostedService` for seamless integration with ASP.NET Core applications and provides methods for manual webhook management.

**Key features:**
- Automatic webhook registration on application startup
- Automatic webhook removal on application shutdown
- Secret token validation for secure webhook endpoints
- Update dispatching to event handlers
- Runtime statistics via `GetInfo()`

**Example usage**

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TelegramBotFramework.Integration;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
    BotToken = "your-bot-token",
    BotUsername = "your-bot-username"
});

// Configure webhook options
services.Configure<WebhookOptions>(options =>
{
    options.Url = "https://yourdomain.com/api/webhook";
    options.SecretToken = "your-secret-token";
    options.MaxConnections = 40;
});

// Register WebhookService as hosted service
services.AddHostedService<WebhookService>();

var serviceProvider = services.BuildServiceProvider();

// Resolve WebhookService
var webhookService = serviceProvider.GetRequiredService<WebhookService>();

// Subscribe to update events
webhookService.OnUpdateReceived += async update =>
{
    Console.WriteLine($"Received update {update.UpdateId} of type {update.MessageType}");
    // Handle the update here
};

// Get webhook information
var info = webhookService.GetInfo();
Console.WriteLine($"Webhook registered: {info.IsRegistered}");
Console.WriteLine($"Updates dispatched: {info.UpdatesDispatched}");

// Manually register/unregister (typically handled automatically by IHostedService)
await webhookService.RegisterAsync();
await webhookService.UnregisterAsync();

// Parse and validate incoming webhook payload
var update = await webhookService.ParseAndValidateAsync(
    jsonBody: requestBody,
    secretTokenHeader: request.Headers["X-Telegram-Bot-Api-Secret-Token"]
);

if (update != null)
{
    await webhookService.DispatchUpdateAsync(update);
}
```

## ScheduledTaskManagerExtensions

Provides fluent extension methods for managing scheduled tasks, allowing for flexible scheduling, querying, and monitoring of background operations. It simplifies the interaction with `ScheduledTaskManager`, enabling developers to easily schedule tasks, check for failures or overdue operations, and retrieve task statistics.

**Example usage**

```csharp
// Example: Scheduling and querying tasks
var manager = serviceProvider.GetRequiredService<IScheduledTaskManager>();

// Schedule a daily task
manager.ScheduleDailyAt("Cleanup", "03:00", () => Console.WriteLine("Cleanup started"));

// Check for overdue tasks
var overdue = manager.GetOverdueTasks();

// Retrieve task statistics
var stats = manager.GetStatistics();
Console.WriteLine($"Total Tasks: {stats.TotalTasks}, Running: {stats.RunningTasks}");

// Wait for a specific task completion
await manager.WaitForCompletionAsync("Cleanup");
```

## ScheduledTaskManager

The `ScheduledTaskManager` class manages scheduled and recurring background tasks using timers. It provides functionality for scheduling one-time tasks that execute after a delay, as well as recurring tasks that run at regular intervals. The manager tracks task execution, maintains statistics, and handles errors gracefully.

**Key features:**
- Schedule one-time tasks with `ScheduleOnce()` for delayed execution
- Schedule recurring tasks with `ScheduleRecurring()` for periodic execution
- Cancel tasks with `CancelTask()`
- Query task status with `GetAllTasks()` and `GetTask()`
- Monitor task execution with properties like `LastExecutedAt`, `LastSuccessAt`, `LastErrorAt`, and `ExecutionCount`
- Graceful shutdown with `StopAll()` and `Dispose()`

**Example usage**

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TelegramBotFramework.BackgroundWorkers;

// Setup your services
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());

var serviceProvider = services.BuildServiceProvider();
var taskManager = serviceProvider.GetRequiredService<ScheduledTaskManager>();

// Schedule a one-time task to run after 5 minutes
var oneTimeTaskId = taskManager.ScheduleOnce(
    async () => 
    {
        Console.WriteLine("One-time task executed!");
        await Task.CompletedTask;
    },
    TimeSpan.FromMinutes(5),
    "OneTimeCleanup"
);

// Schedule a recurring task to run every 30 minutes
var recurringTaskId = taskManager.ScheduleRecurring(
    async () => 
    {
        Console.WriteLine("Recurring cleanup task executed at: {0}", DateTime.UtcNow);
        await Task.CompletedTask;
    },
    TimeSpan.FromMinutes(30),
    "RecurringCleanup"
);

// Get all scheduled tasks
var allTasks = taskManager.GetAllTasks();
foreach (var task in allTasks)
{
    Console.WriteLine($"Task: {task.Name}, ID: {task.Id}");
    Console.WriteLine($"  - IsRecurring: {task.IsRecurring}");
    Console.WriteLine($"  - Interval: {task.Interval}");
    Console.WriteLine($"  - Created: {task.CreatedAt}");
    Console.WriteLine($"  - LastExecuted: {task.LastExecutedAt}");
    Console.WriteLine($"  - ExecutionCount: {task.ExecutionCount}");
}

// Get a specific task
var specificTask = taskManager.GetTask(recurringTaskId);
if (specificTask != null)
{
    Console.WriteLine($"Found task: {specificTask.Name}");
}

// Cancel a task
bool cancelled = taskManager.CancelTask(oneTimeTaskId);
Console.WriteLine($"Task cancelled: {cancelled}");

// Stop all tasks when shutting down
// taskManager.StopAll();
// taskManager.Dispose();
```

## InlineKeyboardBuilder

The `InlineKeyboardBuilder` class provides a fluent API for constructing inline keyboards with buttons in Telegram. It enables programmatic creation of complex keyboard layouts with callback buttons, URL buttons, and switch inline query buttons, supporting dynamic row management and easy conversion to Telegram's `InlineKeyboardMarkup` format.

**Key features:**
- Fluent API for building inline keyboards with method chaining
- Support for callback buttons, URL buttons, and switch inline query buttons
- Row-based layout management with `NewRow()` for organizing buttons into rows
- Conversion methods to generate Telegram-compatible `InlineKeyboardMarkup`
- Access to button properties for customization

**Example usage**

```csharp
using TelegramBotFramework.Keyboard;

// Create a new inline keyboard builder
var builder = InlineKeyboardBuilder.Create()
    .AddButton("📊 Dashboard", "dashboard")
    .AddButton("🔧 Settings", "settings")
    .NewRow()
    .AddUrlButton("🌐 Visit Website", "https://example.com")
    .AddButton("📈 Analytics", "analytics")
    .NewRow()
    .AddButton("✅ Confirm", "confirm_yes", "yes")
    .AddButton("❌ Cancel", "confirm_no", "no");

// Build the inline keyboard markup
var keyboardMarkup = builder.Build();

// Use with Telegram Bot API
// await botClient.SendTextMessageAsync(
//     chatId: message.Chat.Id,
//     text: "Please choose an option:",
//     replyMarkup: keyboardMarkup
// );
```

## InlineKeyboardBuilderExtensions

Provides fluent, helper extension methods for building complex Telegram inline keyboards using `InlineKeyboardBuilder`. These methods simplify row management, grid layout creation, and common button patterns like confirmation and pagination.

**Example usage**

```csharp
// Assuming you have an instance of InlineKeyboardBuilder
var builder = new InlineKeyboardBuilder();

builder
    .AddButtonRow(
        ("Button 1", "callback_1"),
        ("Button 2", "callback_2")
    )
    .AddUrlButtonRow(
        ("GitHub", "https://github.com")
    )
    .AddPaginationRow(hasPrevious: false, hasNext: true, pageNumber: 1)
    .AddConfirmationRow("yes", "no");
```

## ICacheProvider

The `ICacheProvider` interface defines the contract for cache providers within the Telegram Bot Framework. It provides an abstraction layer for various caching implementations (in-memory, distributed, etc.) and exposes statistics tracking for monitoring cache performance. The interface supports basic CRUD operations with optional time-to-live (TTL) expiration and provides atomic get-or-create operations for efficient data retrieval.

**Key features:**
- Generic type support for type-safe caching operations
- TTL-based expiration with `TimeSpan?` parameter
- Atomic get-or-create pattern via `GetOrCreateAsync`
- Statistics tracking (hits, misses, sets, removals, memory usage)
- Bulk operations via concrete implementations
- Thread-safe operations for concurrent access

**Example usage**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Caching;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
    BotToken = "your-bot-token",
    BotUsername = "your-bot-username"
});

// Add cache provider (typically configured in Startup.cs)
services.AddSingleton<ICacheProvider, LocalCacheProvider>();

var serviceProvider = services.BuildServiceProvider();

// Resolve the cache provider
var cacheProvider = serviceProvider.GetRequiredService<ICacheProvider>();

// Example 1: Basic get/set operations
await cacheProvider.SetAsync("user:123:profile", new { Name = "John Doe", Email = "john@example.com" }, TimeSpan.FromMinutes(30));

var cachedProfile = await cacheProvider.GetAsync<object>("user:123:profile");
if (cachedProfile != null)
{
    Console.WriteLine("Profile retrieved from cache!");
}

// Example 2: Atomic get-or-create pattern
var userData = await cacheProvider.GetOrCreateAsync(
    "user:456:data",
    async () => 
    {
        // This factory is only called if the key doesn't exist
        await Task.Delay(100); // Simulate expensive operation
        return new { LastAccessed = DateTime.UtcNow, Count = 1 };
    },
    TimeSpan.FromHours(1)
);

Console.WriteLine($"User data: {userData.LastAccessed}");

// Example 3: Check if key exists
bool exists = await cacheProvider.ExistsAsync("user:123:profile");
Console.WriteLine($"Key exists: {exists}");

// Example 4: Remove a cached item
await cacheProvider.RemoveAsync("user:123:profile");

// Example 5: Get cache statistics for monitoring
var stats = await cacheProvider.GetStatisticsAsync();
Console.WriteLine($"Cache stats - Hits: {stats.HitCount}, Misses: {stats.MissCount}, Items: {stats.ItemCount}, Memory: {stats.MemoryBytes} bytes");
Console.WriteLine($"Cache hit rate: {stats.HitRate:F2}%");

// Example 6: Clear all cache entries (use with caution in production)
// await cacheProvider.FlushAsync();
```

## DistributedCacheProvider

The `DistributedCacheProvider` class is an abstract base class for implementing distributed cache providers (Redis, Memcached, etc.) within the Telegram Bot Framework. It provides serialization/deserialization, common cache operations, and serves as the foundation for creating distributed cache implementations. Subclass this to create specific distributed cache providers for your caching backend.

**Key features:**
- Abstract base class for distributed cache implementations
- JSON serialization/deserialization for type-safe caching
- TTL-based expiration support
- Atomic get-or-create operations via `GetOrCreateAsync`
- Statistics tracking via `GetStatisticsAsync`
- Thread-safe operations with built-in error handling
- Fallback to `NoOpCacheProvider` when distributed cache is unavailable

**Example usage:**

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TelegramBotFramework.Caching;

// Example: Implementing a RedisCacheProvider
public class RedisCacheProvider : DistributedCacheProvider
{
    private readonly IDatabase _redis;
    
    public RedisCacheProvider(IDatabase redis, ILogger<RedisCacheProvider> logger) 
        : base(logger)
    {
        _redis = redis;
    }
    
    protected override async Task<string?> GetValueAsync(string key)
    {
        return await _redis.StringGetAsync(key);
    }
    
    protected override async Task SetValueAsync(string key, string value, TimeSpan? expiration)
    {
        var options = expiration.HasValue 
            ? new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration }
            : null;
        await _redis.StringSetAsync(key, value, expiration);
    }
    
    protected override async Task RemoveValueAsync(string key)
    {
        await _redis.KeyDeleteAsync(key);
    }
    
    protected override async Task<bool> KeyExistsAsync(string key)
    {
        return await _redis.KeyExistsAsync(key);
    }
    
    protected override async Task FlushAllAsync()
    {
        // Redis-specific flush implementation
        await _redis.ExecuteAsync("FLUSHALL");
    }
    
    protected override async Task<CacheStatistics> GetStatsAsync()
    {
        // Implement Redis-specific statistics
        var info = await _redis.ExecuteAsync("INFO");
        // Parse Redis info and return CacheStatistics
        return new CacheStatistics();
    }
}

// Setup your services with distributed cache
var services = new ServiceCollection();
services.AddLogging();

// Register your Redis implementation
services.AddSingleton<ICacheProvider, RedisCacheProvider>();

var serviceProvider = services.BuildServiceProvider();

// Resolve the cache provider
var cacheProvider = serviceProvider.GetRequiredService<ICacheProvider>() as RedisCacheProvider;

// Example 1: Basic get/set operations with distributed cache
await cacheProvider.SetAsync("user:123:profile", 
    new { Name = "John Doe", Email = "john@example.com" }, 
    TimeSpan.FromMinutes(30));

var cachedProfile = await cacheProvider.GetAsync<object>("user:123:profile");
if (cachedProfile != null)
{
    Console.WriteLine("Profile retrieved from distributed cache!");
}

// Example 2: Atomic get-or-create pattern
var userData = await cacheProvider.GetOrCreateAsync(
    "user:456:data",
    async () => 
    {
        // This factory is only called if the key doesn't exist
        await Task.Delay(100); // Simulate expensive operation
        return new { LastAccessed = DateTime.UtcNow, Count = 1 };
    },
    TimeSpan.FromHours(1)
);

Console.WriteLine($"User data: {userData.LastAccessed}");

// Example 3: Check if key exists in distributed cache
bool exists = await cacheProvider.ExistsAsync("user:123:profile");
Console.WriteLine($"Key exists: {exists}");

// Example 4: Remove a cached item from distributed cache
await cacheProvider.RemoveAsync("user:123:profile");

// Example 5: Get cache statistics from distributed cache
var stats = await cacheProvider.GetStatisticsAsync();
Console.WriteLine($"Cache stats - Hits: {stats.HitCount}, Misses: {stats.MissCount}, Items: {stats.ItemCount}");

// Example 6: Clear all cache entries (use with caution in production)
// await cacheProvider.FlushAsync();
```

## LocalCacheProvider

The `LocalCacheProvider` class provides an in-memory, thread-safe cache implementation that stores data locally within the application process. It implements the `ICacheProvider` interface and is ideal for scenarios where distributed caching isn't required or when you need a lightweight, fast cache with minimal overhead. The provider supports time-to-live (TTL) expiration, atomic operations, and comprehensive statistics tracking.

**Key features:**
- Thread-safe in-memory storage with O(1) average complexity for most operations
- TTL-based expiration with configurable timeouts
- Atomic get-or-create operations via `GetOrCreateAsync`
- Statistics tracking (hits, misses, sets, removals, memory usage)
- Bulk operations for efficient batch processing
- Simple integration with dependency injection

**Example usage**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Caching;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
    BotToken = "your-bot-token",
    BotUsername = "your-bot-username"
});

// Register LocalCacheProvider as the cache implementation
services.AddSingleton<ICacheProvider, LocalCacheProvider>();

var serviceProvider = services.BuildServiceProvider();

// Resolve the cache provider
var cacheProvider = serviceProvider.GetRequiredService<ICacheProvider>() as LocalCacheProvider;

// Example 1: Basic get/set operations with TTL
await cacheProvider.SetAsync("user:123:profile", 
    new { Name = "John Doe", Email = "john@example.com" }, 
    TimeSpan.FromMinutes(30));

var cachedProfile = await cacheProvider.GetAsync<object>("user:123:profile");
if (cachedProfile != null)
{
    Console.WriteLine("Profile retrieved from cache!");
}

// Example 2: Atomic get-or-create pattern
var userData = await cacheProvider.GetOrCreateAsync(
    "user:456:data",
    async () =>
    {
        // This factory is only called if the key doesn't exist
        await Task.Delay(100); // Simulate expensive operation
        return new { LastAccessed = DateTime.UtcNow, Count = 1 };
    },
    TimeSpan.FromHours(1)
);

Console.WriteLine($"User data: {userData.LastAccessed}");

// Example 3: Check if key exists
bool exists = await cacheProvider.ExistsAsync("user:123:profile");
Console.WriteLine($"Key exists: {exists}");

// Example 4: Remove a cached item
await cacheProvider.RemoveAsync("user:123:profile");

// Example 5: Get cache statistics for monitoring
var stats = await cacheProvider.GetStatisticsAsync();
Console.WriteLine($"Cache stats - Hits: {stats.HitCount}, Misses: {stats.MissCount}, Items: {stats.ItemCount}, Memory: {stats.MemoryBytes} bytes");
Console.WriteLine($"Cache hit rate: {stats.HitRate:F2}%");

// Example 6: Clear all cache entries (use with caution in production)
// await cacheProvider.FlushAsync();
```

## LocalCacheProviderTests

The `LocalCacheProviderTests` class contains unit tests for the `LocalCacheProvider` class, which provides in-memory caching functionality for Telegram bot framework components. The test suite covers basic CRUD operations, expiration behavior, cache statistics tracking, and thread-safe operations on the cache provider.

**Example usage**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Caching;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
    BotToken = "your-bot-token",
    BotUsername = "your-bot-username"
});

// Register LocalCacheProvider as the cache implementation
services.AddSingleton<ICacheProvider, LocalCacheProvider>();

var serviceProvider = services.BuildServiceProvider();

// Resolve the cache provider
var cacheProvider = serviceProvider.GetRequiredService<ICacheProvider>() as LocalCacheProvider;

// Test 1: Basic get/set operations with TTL
await cacheProvider.SetAsync("user:123:profile", new { Name = "John Doe", Email = "john@example.com" }, TimeSpan.FromMinutes(30));
var cachedProfile = await cacheProvider.GetAsync<object>("user:123:profile");
if (cachedProfile != null)
{
    Console.WriteLine("Profile retrieved from cache!");
}

// Test 2: Check if key exists
bool exists = await cacheProvider.ExistsAsync("user:123:profile");
Console.WriteLine($"Key exists: {exists}");

// Test 3: Remove a cached item
await cacheProvider.RemoveAsync("user:123:profile");

// Test 4: GetOrCreateAsync - factory is only called when key doesn't exist
var userData = await cacheProvider.GetOrCreateAsync(
    "user:456:data",
    async () => 
    {
        // This factory is only called if the key doesn't exist
        await Task.Delay(100); // Simulate expensive operation
        return new { LastAccessed = DateTime.UtcNow, Count = 1 };
    },
    TimeSpan.FromHours(1)
);

// Test 5: Get cache statistics for monitoring
var stats = await cacheProvider.GetStatisticsAsync();
Console.WriteLine($"Cache stats - Hits: {stats.HitCount}, Misses: {stats.MissCount}, Items: {stats.ItemCount}");

// Test 6: Flush all cache entries (use with caution in production)
// await cacheProvider.FlushAsync();
```

## LocalCacheProviderExtensions

Provides additional utility methods for `LocalCacheProvider` to simplify common caching operations such as conditional retrieval, batch management, and atomic get-or-create patterns. These extensions improve code readability and efficiency when working with cached data in your bot services.

**Example usage**

```csharp
// Assuming you have an instance of LocalCacheProvider
var provider = serviceProvider.GetRequiredService<LocalCacheProvider>();

// Get or create a value atomically
var user = await provider.GetOrCreateAsync("user:123", () => new User("John Doe"), TimeSpan.FromMinutes(10));

// Try to get a value
var (success, cachedValue) = await provider.TryGetAsync<User>("user:123");

// Get or set multiple values in batch
var keys = new List<string> { "key1", "key2" };
var results = await provider.GetManyAsync<string>(keys);

await provider.SetManyAsync(new Dictionary<string, string> { { "key3", "val3" } });

// Remove multiple items
await provider.RemoveManyAsync(new List<string> { "key1", "key2" });
```

## EventPublisherExtensions

Provides extension methods for the `EventPublisher` class that simplify common event publishing scenarios with strongly-typed APIs and automatic null handling. These extensions handle correlation ID management, batch publishing, and provide convenience overloads for frequently used event types like message received, command executed, and bot state changes.

**Example usage**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Events;
using TelegramBotFramework.Models;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
    BotToken = "your-bot-token",
    BotUsername = "your-bot-username"
});

var serviceProvider = services.BuildServiceProvider();

// Resolve the event publisher
var eventPublisher = serviceProvider.GetRequiredService<EventPublisher>();

// Example 1: Publish a message received event
await eventPublisher.PublishMessageReceivedAsync(chatId: 123456789L, userId: 987654321L, messageText: "Hello, bot!");

// Example 2: Publish a command executed event
await eventPublisher.PublishCommandExecutedAsync(
    commandName: "start",
    userId: 987654321L,
    arguments: "--help",
    success: true
);

// Example 3: Publish a bot state changed event
await eventPublisher.PublishBotStateChangedAsync(
    newState: "Processing",
    previousState: "Idle",
    reason: "User initiated action"
);

// Example 4: Publish an event with correlation ID tracking
var userRegisteredEvent = new UserRegisteredEvent
{
    UserId = 987654321L,
    Username = "johndoe",
    Timestamp = DateTime.UtcNow
};

await eventPublisher.PublishWithCorrelationAsync(
    @event: userRegisteredEvent,
    correlationId: "user-registration-123"
);

// Example 5: Publish a collection of events with correlation tracking
var events = new List<IEvent>
{
    new MessageReceivedEvent { ChatId = 123, UserId = 456, Text = "First message" },
    new MessageReceivedEvent { ChatId = 123, UserId = 456, Text = "Second message" },
    new CommandExecutedEvent { CommandName = "help", UserId = 456, Success = true }
};

await eventPublisher.PublishCollectionAsync(
    events: events,
    correlationId: "user-session-456"
);
```

## ConversationFlowExtensions

Provides extension methods for registering conversation flow services in the dependency-injection container and for building `FlowDefinition` instances using a fluent API. Includes methods for both in-memory and file-based state persistence, enabling durable multi-step conversations that survive process restarts.

**Example usage**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.ConversationFlow;
using TelegramBotFramework.Integration;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
    BotToken = "your-bot-token",
    BotUsername = "your-bot-username"
});

// Register conversation flow engine with in-memory state store
services.AddConversationFlows(opts =>
{
    opts.DefaultFlowTimeout = TimeSpan.FromMinutes(15);
    opts.EnableFlowEvents = true;
    opts.AbortKeyword = "/stop";
});

// OR register with file-based state store for persistence across restarts
services.AddConversationFlowsWithFileStore(
    stateDirectory: "/var/lib/telegram-bot/conversation-states",
    configure: opts => opts.DefaultFlowTimeout = TimeSpan.FromMinutes(10)
);

var serviceProvider = services.BuildServiceProvider();

// Resolve conversation flow engine
var flowEngine = serviceProvider.GetRequiredService<ConversationFlowEngine>();

// Create a conversation flow using the fluent builder API
var onboardingFlow = ConversationFlowExtensions
    .CreateFlow("user_onboarding", "User Onboarding Flow")
    .WithDescription("Guides new users through the initial setup process")
    .WithTimeout(TimeSpan.FromMinutes(5))
    .AddStep(new FlowStep
    {
        StepId = "welcome",
        Prompt = "Welcome! Let's get started. What's your name?",
        InputType = FlowInputType.Text,
        VariableName = "user_name",
        DefaultNextStepId = "ask_email"
    })
    .AddStep(new FlowStep
    {
        StepId = "ask_email",
        Prompt = "Great! What's your email address?",
        InputType = FlowInputType.Email,
        VariableName = "user_email",
        IsTerminal = false,
        DefaultNextStepId = "confirm_details"
    })
    .AddStep(new FlowStep
    {
        StepId = "confirm_details",
        Prompt = "Please confirm your details:\n\nName: {{user_name}}\nEmail: {{user_email}}\n\nIs this correct?",
        InputType = FlowInputType.Confirmation,
        IsTerminal = true,
        Transitions = new List<FlowTransition>
        {
            new FlowTransition { From = "yes", To = "complete" },
            new FlowTransition { From = "no", To = "welcome" }
        }
    })
    .AddStep(new FlowStep
    {
        StepId = "complete",
        Prompt = "Onboarding complete! Thank you for signing up.",
        InputType = FlowInputType.None,
        IsTerminal = true
    })
    .OnCompletionNavigateTo("main_menu")
    .AllowResume(true)
    .WithMetadata("category", "onboarding")
    .WithMetadata("priority", "high")
    .Build();

// Register the flow with the engine
await flowEngine.RegisterFlowAsync(onboardingFlow);

// Later, when a user starts the flow
var userId = 123456789L;
var chatId = 987654321L;

// Start the flow for this user
await flowEngine.StartFlowAsync(userId, chatId, "user_onboarding");

// Process user input through the flow
await flowEngine.ProcessInputAsync(userId, chatId, "John Doe");
await flowEngine.ProcessInputAsync(userId, chatId, "john@example.com");
await flowEngine.ProcessInputAsync(userId, chatId, "yes");
```

## InMemoryConversationStateStoreExtensions

Provides extension methods for `InMemoryConversationStateStore` that simplify common state management operations beyond basic CRUD operations. These extensions offer convenient methods for checking state existence, updating state status, managing active states, cleaning up terminal states, and tracking state activity timestamps.

**Example usage:**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.ConversationFlow;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
  BotToken = "your-bot-token",
  BotUsername = "your-bot-username"
});

var serviceProvider = services.BuildServiceProvider();

// Resolve the in-memory conversation state store
var stateStore = serviceProvider.GetRequiredService<InMemoryConversationStateStore>();

// Example 1: Check if a user has an active state
var userId = 123456789L;
bool hasState = await stateStore.HasStateAsync(userId);
Console.WriteLine($"User has active state: {hasState}");

// Example 2: Try to load a state (returns null if not found)
var state = await stateStore.TryLoadStateAsync(userId);
if (state != null)
{
  Console.WriteLine($"State loaded: {state.StateId}");
}

// Example 3: Update the state status and persist changes
var updatedState = await stateStore.UpdateStateStatusAsync(userId, FlowStateStatus.Completed);
if (updatedState != null)
{
  Console.WriteLine($"State status updated to: {updatedState.Status}");
}

// Example 4: Get all active states (Active or WaitingForInput)
var activeStates = await stateStore.GetActiveStatesAsync();
Console.WriteLine($"Found {activeStates.Count} active states");

// Example 5: Remove all terminal states (Completed, Aborted, TimedOut, Failed)
int removedTerminalCount = await stateStore.RemoveTerminalStatesAsync();
Console.WriteLine($"Removed {removedTerminalCount} terminal states");

// Example 6: Update the last activity timestamp (touch state)
bool touched = await stateStore.TouchStateAsync(userId);
Console.WriteLine($"State was updated: {touched}");

// Example 7: Get the total count of stored states
totalStateCount = stateStore.GetStateCount();
Console.WriteLine($"Total states in store: {totalStateCount}");

// Example 8: Find a state by its unique StateId
var foundState = await stateStore.FindStateByIdAsync(state.StateId);
if (foundState != null)
{
  Console.WriteLine($"Found state by ID: {foundState.StateId}");
}

// Example 9: Remove stale states (not updated since cutoff time)
var cutoffTime = DateTime.UtcNow.AddHours(-1);
int removedStaleCount = await stateStore.RemoveStaleStatesAsync(cutoffTime);
Console.WriteLine($"Removed {removedStaleCount} stale states");
```

## FileConversationStateStoreValidation

Provides validation helpers for `FileConversationStateStore` instances. Validates the configuration and runtime state of file-based conversation state storage to ensure the configured directory exists and is accessible before attempting to persist conversation state.

**Example usage:**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.ConversationFlow;

// Setup your services with file-based state store
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
  BotToken = "your-bot-token",
  BotUsername = "your-bot-username"
});

// Configure file-based conversation state store
services.AddConversationFlowsWithFileStore(
  stateDirectory: "/var/lib/telegram-bot/conversation-states",
  configure: opts => opts.DefaultFlowTimeout = TimeSpan.FromMinutes(10)
);

var serviceProvider = services.BuildServiceProvider();

// Resolve the file-based conversation state store
var stateStore = serviceProvider.GetRequiredService<FileConversationStateStore>();

// Validate the store configuration and runtime state
var validationErrors = stateStore.Validate();

if (validationErrors.Count > 0)
{
  Console.WriteLine("FileConversationStateStore validation failed:");
  foreach (var error in validationErrors)
  {
    Console.WriteLine($"- {error}");
  }
}
else
{
  Console.WriteLine("FileConversationStateStore is valid and ready to use");
}

// Quick validation check
if (stateStore.IsValid())
{
  Console.WriteLine("Store configuration is valid");
}

// Ensure validation throws if invalid (useful for startup validation)
try
{
  stateStore.EnsureValid();
  Console.WriteLine("Store passed validation successfully");
}
catch (ArgumentException ex)
{
  Console.WriteLine($"Store validation failed: {ex.Message}");
}
```

## FileConversationStateStoreExtensions


Provides extension methods for `FileConversationStateStore` that simplify common file-based state management operations. These extensions offer convenient methods for checking state existence, loading states, deleting states, and filtering states by status, flow, or age. The file-based store persists conversation states to disk, enabling durable state across application restarts.

**Example usage:**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.ConversationFlow;

// Setup your services with file-based state store
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
  BotToken = "your-bot-token",
  BotUsername = "your-bot-username"
});

// Configure file-based conversation state store
services.AddConversationFlowsWithFileStore(
  stateDirectory: "/var/lib/telegram-bot/conversation-states",
  configure: opts => opts.DefaultFlowTimeout = TimeSpan.FromMinutes(10)
);

var serviceProvider = services.BuildServiceProvider();

// Resolve the file-based conversation state store
var stateStore = serviceProvider.GetRequiredService<FileConversationStateStore>();

// Example 1: Check if a user has an existing state file
var userId = 123456789L;
bool stateExists = await stateStore.ExistsAsync(userId);
Console.WriteLine($"State file exists: {stateExists}");

// Example 2: Try to load a state (returns null if file doesn't exist)
var state = await stateStore.TryLoadStateAsync(userId);
if (state != null)
{
  Console.WriteLine($"State loaded from file: {state.StateId}");
}

// Example 3: Get the file path for a user's state
string stateFilePath = stateStore.GetStateFilePath(userId);
Console.WriteLine($"State file path: {stateFilePath}");

// Example 4: Try to delete a state file (returns false if it doesn't exist)
bool deleted = await stateStore.TryDeleteStateAsync(userId);
Console.WriteLine($"State file deleted: {deleted}");

// Example 5: Load all states with a specific status
var activeStates = await stateStore.LoadStatesByStatusAsync(FlowStateStatus.Active);
Console.WriteLine($"Found {activeStates.Count} active states");

// Example 6: Load all states for a specific flow
var flowStates = await stateStore.LoadStatesByFlowAsync("user_onboarding");
Console.WriteLine($"Found {flowStates.Count} states for user_onboarding flow");

// Example 7: Load all states for a specific flow with a specific status
var completedOnboardingStates = await stateStore.LoadStatesByFlowAndStatusAsync(
  "user_onboarding",
  FlowStateStatus.Completed
);
Console.WriteLine($"Found {completedOnboardingStates.Count} completed onboarding states");

// Example 8: Load states that have been inactive for more than 24 hours
var inactiveStates = await stateStore.LoadInactiveStatesAsync(TimeSpan.FromHours(24));
Console.WriteLine($"Found {inactiveStates.Count} inactive states");

// Example 9: Load completed states older than 7 days
var oldCompletedStates = await stateStore.LoadOldCompletedStatesAsync(TimeSpan.FromDays(7));
Console.WriteLine($"Found {oldCompletedStates.Count} old completed states");
```

## TelegramBotFrameworkDotnetOptionsExtensionsTests

The `TelegramBotFrameworkDotnetOptionsExtensionsTests` class contains unit tests for the extension methods of `TelegramBotFrameworkDotnetOptions` that provide validation and timeout utilities for framework configuration. These tests verify that configuration validation works correctly, timeout values are properly calculated, and database configuration detection functions as expected.

**Tested methods:**
- `Validate` - Validates configuration options and throws for invalid settings
- `GetSessionTimeout` - Returns configured session timeout as TimeSpan
- `GetMessageProcessingTimeout` - Returns configured message processing timeout as TimeSpan
- `HasDatabaseConfigured` - Detects whether a database connection string is configured

**Example usage:**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Models;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
    BotToken = "your-bot-token",
    BotUsername = "your-bot-username",
    SessionTimeoutMinutes = 30,
    MessageProcessingTimeoutSeconds = 15,
    DatabaseConnectionString = "Server=localhost;Database=BotDb;"
});

var serviceProvider = services.BuildServiceProvider();

// Resolve options (typically via IOptions pattern)
var options = serviceProvider.GetRequiredService<IOptions<TelegramBotFrameworkDotnetOptions>>().Value;

// Test 1: Validate configuration
try
{
    options.Validate();
    Console.WriteLine("Configuration is valid");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Configuration error: {ex.Message}");
}

// Test 2: Get session timeout
var sessionTimeout = options.GetSessionTimeout();
Console.WriteLine($"Session timeout: {sessionTimeout.TotalMinutes} minutes");

// Test 3: Get message processing timeout
var messageTimeout = options.GetMessageProcessingTimeout();
Console.WriteLine($"Message processing timeout: {messageTimeout.TotalSeconds} seconds");

// Test 4: Check if database is configured
bool hasDatabase = options.HasDatabaseConfigured();
Console.WriteLine($"Database configured: {hasDatabase}");
```

## WebhookHandlerExtensionsTests

The `WebhookHandlerExtensionsTests` class contains unit tests for the extension methods of `WebhookHandler` that simplify common webhook update processing scenarios. These tests verify proper handling of null updates, callback data matching, and ID extraction from Telegram updates, ensuring robust error handling and correct behavior for webhook integration scenarios.

**Tested methods:**
- `GetMessageText` - Returns null when message is null, throws when update is null
- `HasCallbackData` - Returns true when callback data matches expected value
- `GetChatId` - Returns 0 when message is null
- `GetUserId` - Returns 0 when message is null

**Example usage:**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Integration;
using TelegramBotFramework.Models;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
    BotToken = "your-bot-token",
    BotUsername = "your-bot-username"
});

var serviceProvider = services.BuildServiceProvider();

// Resolve WebhookHandler
var webhookHandler = new WebhookHandler();

// Test 1: GetMessageText with null message
var nullUpdate = new TelegramUpdate { Message = null };
string? messageText = webhookHandler.GetMessageText(nullUpdate);
Console.WriteLine(messageText); // null

// Test 2: GetMessageText with null update (throws)
try
{
    webhookHandler.GetMessageText(null);
}
catch (ArgumentNullException ex)
{
    Console.WriteLine("Caught ArgumentNullException as expected");
}

// Test 3: HasCallbackData with matching callback
var callbackUpdate = new TelegramUpdate { CallbackData = "confirm_yes" };
bool hasCallback = webhookHandler.HasCallbackData(callbackUpdate, "confirm_yes");
Console.WriteLine(hasCallback); // true

// Test 4: HasCallbackData with non-matching callback
bool noMatch = webhookHandler.HasCallbackData(callbackUpdate, "cancel");
Console.WriteLine(noMatch); // false

// Test 5: GetChatId with null message
var chatIdUpdate = new TelegramUpdate { Message = null };
long chatId = webhookHandler.GetChatId(chatIdUpdate);
Console.WriteLine(chatId); // 0

// Test 6: GetUserId with null message
long userId = webhookHandler.GetUserId(chatIdUpdate);
Console.WriteLine(userId); // 0
```

## CommandExtensionsTests

The `CommandExtensionsTests` class contains unit tests for the extension methods of `Command` that provide utility functionality for command objects. These tests verify that command extension methods correctly identify command properties, extract patterns, determine command types, and generate formatted string representations for logging and display purposes.

**Tested methods:**
- `HasParameters` - Returns true when command has parameters, false when it doesn't
- `GetPrimaryPattern` - Returns the command name as the primary pattern
- `IsStandardCommand` - Returns true when command type is Standard
- `GetFormattedString` - Generates a formatted string representation of the command

**Example usage:**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Models;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
    BotToken = "your-bot-token",
    BotUsername = "your-bot-username"
});

var serviceProvider = services.BuildServiceProvider();

// Create a command with parameters
var commandWithParams = new Command
{
    Name = "/announce",
    Type = CommandType.Standard,
    Description = "Send an announcement to all users",
    Parameters = new List<CommandParameter>
    {
        new CommandParameter { Name = "message", Type = "string", IsRequired = true },
        new CommandParameter { Name = "priority", Type = "int", IsRequired = false, DefaultValue = "1" }
    },
    ExecutionCount = 42,
    CreatedAt = DateTime.UtcNow.AddDays(-7)
};

// Test 1: Check if command has parameters
bool hasParameters = commandWithParams.HasParameters(); // true
Console.WriteLine($"Command has parameters: {hasParameters}");

// Test 2: Get primary pattern
string primaryPattern = commandWithParams.GetPrimaryPattern(); // "/announce"
Console.WriteLine($"Primary pattern: {primaryPattern}");

// Test 3: Check if command is standard
bool isStandard = commandWithParams.IsStandardCommand(); // true
Console.WriteLine($"Is standard command: {isStandard}");

// Test 4: Get formatted string representation
string formatted = commandWithParams.GetFormattedString();
Console.WriteLine(formatted);
/* Output example:
Command '/announce' (Standard) - Send an announcement to all users ['/announce'] with 2 parameter(s), no rate limit [Created: 2024-07-12, Executions: 42]
*/

// Test 5: Command without parameters
var simpleCommand = new Command
{
    Name = "/help",
    Type = CommandType.Standard,
    Description = "Display help information"
};

bool simpleHasParams = simpleCommand.HasParameters(); // false
Console.WriteLine($"Simple command has parameters: {simpleHasParams}");

// Test 6: Non-standard command
var menuCommand = new Command
{
    Name = "main_menu",
    Type = CommandType.Menu,
    Description = "Main menu command"
};

bool isMenuStandard = menuCommand.IsStandardCommand(); // false
Console.WriteLine($"Menu command is standard: {isMenuStandard}");
```

## MessageExtensionsTests

The `MessageExtensionsTests` class contains unit tests for extension methods that provide utility functionality for `Message` objects in the Telegram Bot Framework. These tests verify that message extension methods correctly identify message properties, extract type information, and determine reply status, enabling robust message processing and routing in bot applications.

**Tested methods:**
- `IsCommand()` - Returns true when the message has a command name starting with '/', false otherwise
- `HasAttachments()` - Returns true when the message has one or more attachment URLs
- `GetTypeString()` - Returns the string representation of the message type (e.g., "text", "photo")
- `IsReply()` - Returns true when the message has a ReplyToMessageId set

**Example usage:**

```csharp
using TelegramBotFramework.Models;

// Create a message with a command
var commandMessage = new Message { CommandName = "/start" };

// Test 1: Check if message is a command
bool isCommand = commandMessage.IsCommand(); // true
Console.WriteLine($"Message is command: {isCommand}");

// Test 2: Check if message is NOT a command
var textMessage = new Message { CommandName = "hello" };
bool notCommand = textMessage.IsCommand(); // false
Console.WriteLine($"Text message is command: {notCommand}");

// Test 3: Check if message has attachments
var photoMessage = new Message { AttachmentUrls = new[] { "https://example.com/photo.jpg" } };
bool hasAttachments = photoMessage.HasAttachments(); // true
Console.WriteLine($"Photo message has attachments: {hasAttachments}");

// Test 4: Get message type string
var textMsg = new Message { Type = MessageType.Text };
string typeString = textMsg.GetTypeString(); // "text"
Console.WriteLine($"Message type: {typeString}");

// Test 5: Check if message is a reply
var replyMessage = new Message { ReplyToMessageId = 123 };
bool isReply = replyMessage.IsReply(); // true
Console.WriteLine($"Message is reply: {isReply}");

// Test 6: Check if regular message is NOT a reply
var regularMessage = new Message { ReplyToMessageId = null };
bool notReply = regularMessage.IsReply(); // false
Console.WriteLine($"Regular message is reply: {notReply}");
```

## UserService

The `UserService` class provides centralized user management for Telegram bot applications. It handles user registration, retrieval, updating, and deletion, enabling features like user sessions, role-based access control, and personalized bot experiences.

### Key Features:
- User registration with automatic ID generation and configurable timeouts
- Active user retrieval and management
- Context data storage and retrieval for custom user properties
- User status management (Active, Inactive, Banned, Suspended)
- User role management (User, Moderator, Admin, Owner)
- Metadata storage for custom user preferences
- Validation and display name generation

### Example Usage:
```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;

// Setup your services
var services = new ServiceCollection();
// ... add other services ...
services.AddTelegramBotFramework(new BotConfiguration
{
    BotToken = "your-bot-token",
    BotUsername = "your-bot-username"
});

var serviceProvider = services.BuildServiceProvider();
var userService = serviceProvider.GetRequiredService<IUserService>();

// Create a new user
var newUser = new BotUser
{
    TelegramId = 123456789,
    FirstName = "John",
    LastName = "Doe",
    Username = "johndoe",
    PhoneNumber = "+1234567890",
    Status = UserStatus.Active,
    Role = UserRole.User,
    IsPremium = true,
    IsBot = false,
    Metadata = new Dictionary<string, string>
    {
        { "preferred_language", "en" },
        { "timezone", "UTC-5" }
    }
};

// Validate user data
newUser.Validate();

// Register the user
var createdUser = await userService.UpdateUserAsync(newUser);
Console.WriteLine($"User created: {createdUser.TelegramId}");
```

## SessionServiceTests

The `SessionServiceTests` class contains unit tests for the `SessionService` class.

### Example usage

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;
using TelegramBotFramework.Tests;

public class SessionServiceTests
{
    private readonly SessionService _sessionService;

    public SessionServiceTests()
    {
        var services = new ServiceCollection();
        services.AddTelegramBotFramework(new BotConfiguration
        {
            BotToken = "test_token",
            BotUsername = "test_bot",
            SessionTimeoutMinutes = 30
        });

        var serviceProvider = services.BuildServiceProvider();
        _sessionService = serviceProvider.GetRequiredService<SessionService>();
    }

    public async Task GetActiveSessionAsync_WithExistingActiveSession_ReturnsSession()
    {
        // Arrange
        var userId = 123456789L;
        var chatId = 987654321L;
        var session = await _sessionService.CreateSessionAsync(userId, chatId);

        // Act
        var activeSession = await _sessionService.GetActiveSessionAsync(userId);

        // Assert
        Assert.NotNull(activeSession);
        Assert.Equal(session.SessionId, activeSession.SessionId);
    }

    public async Task GetActiveSessionAsync_WithNoActiveSession_ReturnsNull()
    {
        // Arrange
        var userId = 123456789L;

        // Act
        var activeSession = await _sessionService.GetActiveSessionAsync(userId);

        // Assert
        Assert.Null(activeSession);
    }

    public async Task CreateSessionAsync_CreatesNewSession()
    {
        // Arrange
        var userId = 123456789L;
        var chatId = 987654321L;

        // Act
        var session = await _sessionService.CreateSessionAsync(userId, chatId);

        // Assert
        Assert.NotNull(session);
        Assert.Equal(userId, session.UserId);
        Assert.Equal(chatId, session.ChatId);
        Assert.Equal(SessionState.Active, session.State);
    }

    public async Task CreateSessionAsync_WithCustomTimeout_CreatesSessionWithCorrectExpiration()
    {
        // Arrange
        var userId = 123456789L;
        var chatId = 987654321L;
        var timeout = TimeSpan.FromMinutes(15);

        // Act
        var session = await _sessionService.CreateSessionAsync(userId, chatId, timeout);

        // Assert
        Assert.NotNull(session);
        Assert.Equal(timeout, session.ExpiresAt - session.CreatedAt);
    }

    public async Task RecordSessionActivityAsync_UpdatesLastActivityAndIncrementsInteractionCount()
    {
        // Arrange
        var userId = 123456789L;
        var chatId = 987654321L;
        var session = await _sessionService.CreateSessionAsync(userId, chatId);
        var initialLastActivity = session.LastActivityAt;
        var initialCount = session.InteractionCount;

        // Act
        await _sessionService.RecordSessionActivityAsync(session.SessionId);
        var updatedSession = await _sessionService.GetSessionByIdAsync(session.SessionId);

        // Assert
        Assert.NotNull(updatedSession);
        Assert.True(updatedSession.LastActivityAt > initialLastActivity);
        Assert.Equal(initialCount + 1, updatedSession.InteractionCount);
    }

    public async Task RecordSessionActivityAsync_WithNonExistingSession_DoesNotThrow()
    {
        // Arrange
        var nonExistingSessionId = Guid.NewGuid();

        // Act & Assert
        await _sessionService.RecordSessionActivityAsync(nonExistingSessionId);
        // Should not throw
    }

    public async Task CloseSessionAsync_WithActiveSession_ClosesSessionAndReturnsTrue()
    {
        // Arrange
        var userId = 123456789L;
        var chatId = 987654321L;
        var session = await _sessionService.CreateSessionAsync(userId, chatId);

        // Act
        var result = await _sessionService.CloseSessionAsync(session.SessionId);

        // Assert
        Assert.True(result);
        var closedSession = await _sessionService.GetSessionByIdAsync(session.SessionId);
        Assert.Equal(SessionState.Closed, closedSession.State);
    }

    public async Task CloseSessionAsync_WithAlreadyClosedSession_ReturnsFalse()
    {
        // Arrange
        var userId = 123456789L;
        var chatId = 987654321L;
        var session = await _sessionService.CreateSessionAsync(userId, chatId);
        await _sessionService.CloseSessionAsync(session.SessionId);

        // Act
        var result = await _sessionService.CloseSessionAsync(session.SessionId);

        // Assert
        Assert.False(result);
    }

    public async Task CloseSessionAsync_WithNonExistingSession_ReturnsFalse()
    {
        // Arrange
        var nonExistingSessionId = Guid.NewGuid();

        // Act
        var result = await _sessionService.CloseSessionAsync(nonExistingSessionId);

        // Assert
        Assert.False(result);
    }

    public async Task NavigateToMenuAsync_UpdatesCurrentMenuId()
    {
        // Arrange
        var userId = 123456789L;
        var chatId = 987654321L;
        var session = await _sessionService.CreateSessionAsync(userId, chatId);

        // Act
        var updatedSession = await _sessionService.NavigateToMenuAsync(session.SessionId, "main_menu");

        // Assert
        Assert.Equal("main_menu", updatedSession.CurrentMenuId);
    }

    public async Task GetSessionByIdAsync_WithExistingSession_ReturnsSession()
    {
        // Arrange
        var userId = 123456789L;
        var chatId = 987654321L;
        var session = await _sessionService.CreateSessionAsync(userId, chatId);

        // Act
        var retrievedSession = await _sessionService.GetSessionByIdAsync(session.SessionId);

        // Assert
        Assert.NotNull(retrievedSession);
        Assert.Equal(session.SessionId, retrievedSession.SessionId);
    }

    public async Task GetSessionByIdAsync_WithNonExistingSession_ReturnsNull()
    {
        // Arrange
        var nonExistingSessionId = Guid.NewGuid();

        // Act
        var session = await _sessionService.GetSessionByIdAsync(nonExistingSessionId);

        // Assert
        Assert.Null(session);
    }

    public async Task GetAllActiveSessionsAsync_ReturnsActiveSessions()
    {
        // Arrange
        var userId1 = 123456789L;
        var userId2 = 987654321L;
        var chatId = 111111111L;
        await _sessionService.CreateSessionAsync(userId1, chatId);
        await _sessionService.CreateSessionAsync(userId2, chatId);

        // Act
        var activeSessions = await _sessionService.GetAllActiveSessionsAsync();

        // Assert
        Assert.NotEmpty(activeSessions);
        Assert.Equal(2, activeSessions.Count);
    }

    public async Task GetSessionsByUserIdAsync_ReturnsUserSessions()
    {
        // Arrange
        var userId = 123456789L;
        var chatId1 = 111111111L;
        var chatId2 = 222222222L;
        await _sessionService.CreateSessionAsync(userId, chatId1);
        await _sessionService.CreateSessionAsync(userId, chatId2);

        // Act
        var userSessions = await _sessionService.GetSessionsByUserIdAsync(userId);

        // Assert
        Assert.NotEmpty(userSessions);
        Assert.Equal(2, userSessions.Count);
        Assert.All(userSessions, s => Assert.Equal(userId, s.UserId));
    }

    public async Task DeleteSessionAsync_WithExistingSession_DeletesAndReturnsTrue()
    {
        // Arrange
        var userId = 123456789L;
        var chatId = 987654321L;
        var session = await _sessionService.CreateSessionAsync(userId, chatId);

        // Act
        var result = await _sessionService.DeleteSessionAsync(session.SessionId);

        // Assert
        Assert.True(result);
        var deletedSession = await _sessionService.GetSessionByIdAsync(session.SessionId);
        Assert.Null(deletedSession);
    }

    public async Task DeleteSessionAsync_WithNonExistingSession_ReturnsFalse()
    {
        // Arrange
        var nonExistingSessionId = Guid.NewGuid();

        // Act
        var result = await _sessionService.DeleteSessionAsync(nonExistingSessionId);

        // Assert
        Assert.False(result);
    }

    public async Task ExpireInactiveSessionsAsync_WithInactiveSessions_ClosesThem()
    {
        // Arrange
        var userId1 = 123456789L;
        var userId2 = 987654321L;
        var chatId = 111111111L;
        var session1 = await _sessionService.CreateSessionAsync(userId1, chatId);
        var session2 = await _sessionService.CreateSessionAsync(userId2, chatId);

        // Act
        var expiredCount = await _sessionService.ExpireInactiveSessionsAsync(TimeSpan.Zero);

        // Assert
        Assert.Equal(2, expiredCount);
        var expiredSession1 = await _sessionService.GetSessionByIdAsync(session1.SessionId);
        var expiredSession2 = await _sessionService.GetSessionByIdAsync(session2.SessionId);
        Assert.Equal(SessionState.Expired, expiredSession1.State);
        Assert.Equal(SessionState.Expired, expiredSession2.State);
    }
}
```

## BotOrchestratorTests

The `BotOrchestratorTests` class contains unit tests for the `BotOrchestrator` class. It verifies various scenarios including message processing, command execution, menu handling, session management, and constructor validation. The test suite covers constructor parameter validation, message processing with different content types, command extraction and execution, menu navigation and button handling, and session retrieval operations.

**Example usage:**

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;
using TelegramBotFramework.Tests;
using Xunit;

// Setup test services
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());

// Create orchestrator with mocked dependencies
var mockUserService = new Mock<IUserService>();
var mockCommandService = new Mock<ICommandService>();
var mockSessionService = new Mock<ISessionService>();
var mockMessageService = new Mock<IMessageService>();
var mockMenuService = new Mock<IMenuService>();
var mockLogger = new Mock<ILogger<BotOrchestrator>>();
var middlewares = new List<Middleware.IBotMiddleware>();

var configuration = new BotConfiguration
{
    BotToken = "test-token",
    BotUsername = "TestBot"
};

var orchestrator = new BotOrchestrator(
    mockUserService.Object,
    mockCommandService.Object,
    mockSessionService.Object,
    mockMessageService.Object,
    mockMenuService.Object,
    middlewares,
    configuration,
    mockLogger.Object
);

// Test 1: Constructor validation
Assert.Throws<ArgumentNullException>(() => new BotOrchestrator(
    null!, mockCommandService.Object, mockSessionService.Object, 
    mockMessageService.Object, mockMenuService.Object, middlewares, 
    configuration, mockLogger.Object
));

// Test 2: Process user message
var user = new BotUser { UserId = 123, FirstName = "John", LastName = "Doe", Role = UserRole.User };
var session = new UserSession { SessionId = "session-123", UserId = 123, ChatId = 456, IsActive = true };
var message = new Message { MessageId = 1, UserId = 123, ChatId = 456, Content = "Hello", Type = MessageType.Text };

mockUserService.Setup(s => s.GetOrCreateUserAsync(123, "John", "Doe", It.IsAny<CancellationToken>()))
    .ReturnsAsync(user);
mockSessionService.Setup(s => s.GetActiveSessionAsync(123, It.IsAny<CancellationToken>()))
    .ReturnsAsync(session);
mockSessionService.Setup(s => s.CreateSessionAsync(123, 456, It.IsAny<CancellationToken>()))
    .ReturnsAsync(session);
mockMessageService.Setup(s => s.ProcessIncomingMessageAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(message);

var result = await orchestrator.ProcessUserMessageAsync(123, 456, "Hello", "John", "Doe");
Assert.NotNull(result);
Assert.Equal(123, result.UserId);
Assert.Equal(456, result.ChatId);

// Test 3: Execute user command
mockCommandService.Setup(s => s.GetCommandAsync("test", It.IsAny<CancellationToken>()))
    .ReturnsAsync(new Command { Name = "/test", IsEnabled = true });
mockCommandService.Setup(s => s.RecordCommandExecutionAsync("test", It.IsAny<CancellationToken>()))
    .Returns(Task.CompletedTask);

var commandResult = await orchestrator.ExecuteUserCommandAsync(123, 456, "test");
Assert.NotNull(commandResult);
Assert.Equal("/test", commandResult.Command?.Name);

// Test 4: Display menu
mockMenuService.Setup(s => s.GetMenuAsync("main", It.IsAny<CancellationToken>()))
    .ReturnsAsync(new Menu { MenuId = "main", Title = "Main Menu", Buttons = new List<MenuButton>() });
mockSessionService.Setup(s => s.GetActiveSessionAsync(123, It.IsAny<CancellationToken>()))
    .ReturnsAsync(session);
mockSessionService.Setup(s => s.NavigateToMenuAsync("session-123", "main", It.IsAny<CancellationToken>()))
    .ReturnsAsync(session);

var menuResult = await orchestrator.DisplayMenuAsync(123, "main");
Assert.NotNull(menuResult);
Assert.Equal("main", menuResult.MenuId);

// Test 5: Handle menu button
var button = new MenuButton { CallbackData = "/start", Action = ButtonAction.ExecuteCommand };
mockMenuService.Setup(s => s.GetButtonAsync("main", "/start", It.IsAny<CancellationToken>()))
    .ReturnsAsync(button);
mockSessionService.Setup(s => s.GetActiveSessionAsync(123, It.IsAny<CancellationToken>()))
    .ReturnsAsync(session);
mockCommandService.Setup(s => s.GetCommandAsync("start", It.IsAny<CancellationToken>()))
    .ReturnsAsync(new Command { Name = "/start", IsEnabled = true });
mockCommandService.Setup(s => s.RecordCommandExecutionAsync("start", It.IsAny<CancellationToken>()))
    .Returns(Task.CompletedTask);

var buttonResult = await orchestrator.HandleMenuButtonAsync(123, "main", "/start");
Assert.True(buttonResult);

// Test 6: Get user session
mockSessionService.Setup(s => s.GetActiveSessionAsync(123, It.IsAny<CancellationToken>()))
    .ReturnsAsync(session);

var sessionResult = await orchestrator.GetUserSessionAsync(123);
Assert.NotNull(sessionResult);
Assert.Equal("session-123", sessionResult.SessionId);
```

## StateManagementExample

The `StateManagementExample` class demonstrates how to handle complex user flows with form data, multi-step processes, and conversation state tracking using the framework's session management capabilities. This example shows how to maintain state across multiple interactions, collect user input through sequential steps, and persist data between messages.

**Key features:**
- Multi-step form processing with sequential state transitions
- Context data storage and retrieval for maintaining form state
- Integration with session service for durable state persistence
- Example of both registration and survey workflows

**Example usage:**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Examples;
using TelegramBotFramework.Models;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
    BotToken = "your-bot-token",
    BotUsername = "your-bot-username",
    SessionTimeoutMinutes = 30
});

var serviceProvider = services.BuildServiceProvider();

// Resolve the state management example
var stateExample = new StateManagementExample(serviceProvider);

// Run the state management example
await stateExample.RunAsync();

// The example demonstrates:
// 1. Creating a registration form flow with multiple steps
// 2. Collecting user data (FirstName, Email, PhoneNumber)
// 3. Creating a feedback survey flow
// 4. Collecting survey responses (SatisfactionLevel, ImprovementSuggestions, WouldRecommend)
// 5. Maintaining state across all interactions
```

## BotOrchestratorAdditionalTests

The `BotOrchestratorAdditionalTests` class contains additional unit tests for the `BotOrchestrator` class, extending the basic test coverage with edge cases, boundary conditions, and specific scenarios not covered in the main test suite. This test suite verifies proper handling of empty messages, null values, very long content, command parameters, non-existent commands, menu operations with null sessions, various button actions, session management edge cases, and command name extraction with different whitespace characters.

**Key features tested:**
- Empty message content handling and error reporting
- Null user properties (e.g., last name)
- Maximum message length boundaries (4000 characters)
- Command parameter storage and retrieval
- Non-existent command error handling
- Menu operations with null sessions
- Button action handling (OpenUrl, SwitchInline)
- Session exception scenarios
- Command name extraction with various whitespace characters

**Example usage:**

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;
using TelegramBotFramework.Tests;
using Xunit;

// Setup test services
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());

// Create orchestrator with mocked dependencies
var mockUserService = new Mock<IUserService>();
var mockCommandService = new Mock<ICommandService>();
var mockSessionService = new Mock<ISessionService>();
var mockMessageService = new Mock<IMessageService>();
var mockMenuService = new Mock<IMenuService>();
var mockLogger = new Mock<ILogger<BotOrchestrator>>();
var middlewares = new List<Middleware.IBotMiddleware>();

var configuration = new BotConfiguration
{
  BotToken = "test-token",
  BotUsername = "TestBot"
};

var orchestrator = new BotOrchestrator(
  mockUserService.Object,
  mockCommandService.Object,
  mockSessionService.Object,
  mockMessageService.Object,
  mockMenuService.Object,
  middlewares,
  configuration,
  mockLogger.Object
);

// Test 1: Process message with empty content
var result1 = await orchestrator.ProcessUserMessageAsync(123, 456, "", "John", "Doe");
Assert.False(result1.IsValid);
Assert.Contains("empty", result1.Errors.First());

// Test 2: Process message with null last name
var result2 = await orchestrator.ProcessUserMessageAsync(123, 456, "Hello", "John");
Assert.True(result2.IsValid);
Assert.Equal(123, result2.UserId);

// Test 3: Process very long message (4000 characters)
var longMessage = new string('x', 4000);
var result3 = await orchestrator.ProcessUserMessageAsync(123, 456, longMessage, "John", "Doe");
Assert.True(result3.IsValid);

// Test 4: Execute command with parameters
var parameters = new Dictionary<string, object> { { "param1", "value1" }, { "param2", 123 } };
var result4 = await orchestrator.ExecuteUserCommandAsync(123, 456, "test", parameters);
Assert.NotNull(result4.Parameters);
Assert.Equal(2, result4.Parameters.Count);

// Test 5: Execute non-existent command
var result5 = await orchestrator.ExecuteUserCommandAsync(123, 456, "nonexistent");
Assert.False(result5.IsValid);
Assert.Contains("not found", result5.Errors.First());

// Test 6: Display menu with null session
var menuResult = await orchestrator.DisplayMenuAsync(123, "main");
Assert.NotNull(menuResult);
Assert.Equal("main", menuResult.MenuId);

// Test 7: Handle menu button with OpenUrl action
var button1 = new MenuButton { CallbackData = "https://example.com", Action = ButtonAction.OpenUrl };
mockMenuService.Setup(s => s.GetButtonAsync("main", "https://example.com", It.IsAny<CancellationToken>()))
  .ReturnsAsync(button1);
var result7 = await orchestrator.HandleMenuButtonAsync(123, "main", "https://example.com");
Assert.True(result7);

// Test 8: Handle menu button with SwitchInline action
var button2 = new MenuButton { CallbackData = "inline_query", Action = ButtonAction.SwitchInline };
mockMenuService.Setup(s => s.GetButtonAsync("main", "inline_query", It.IsAny<CancellationToken>()))
  .ReturnsAsync(button2);
var result8 = await orchestrator.HandleMenuButtonAsync(123, "main", "inline_query");
Assert.True(result8);

// Test 9: Get user session with no active session (throws exception)
await Assert.ThrowsAsync<Exceptions.SessionException>(
  () => orchestrator.GetUserSessionAsync(123)
);

// Test 10: End user session with no active session
var result10 = await orchestrator.EndUserSessionAsync(123);
Assert.False(result10);

// Test 11: Extract command name with multiple spaces
var commandName1 = BotOrchestrator.ExtractCommandName("/start param1 param2");
Assert.Equal("start", commandName1);

// Test 12: Extract command name with leading/trailing spaces
var commandName2 = BotOrchestrator.ExtractCommandName(" /start ");
Assert.Equal("start", commandName2);

// Test 13: Extract command name with tab characters
var commandName3 = BotOrchestrator.ExtractCommandName("/start\tparam1");
Assert.Equal("start", commandName3);
```

## UserServiceTests

The `UserServiceTests` class contains unit tests for the `UserService` class. It verifies user management functionality including creation, retrieval, updating, deletion, and activity tracking through comprehensive test cases that cover various scenarios like existing vs. new users, null values, partial updates, and status filtering.

**Example usage:**

```csharp
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TelegramBotFramework.Models;
using TelegramBotFramework.Repositories;
using TelegramBotFramework.Services;
using Xunit;

public class UserServiceTestsExample
{
    [Fact]
    public async Task GetOrCreateUserAsync_WithExistingUser_ReturnsExistingUser()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var mockLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(mockRepository.Object, mockLogger.Object);
        
        var existingUser = new BotUser { UserId = 123, FirstName = "John", LastName = "Doe" };
        mockRepository
            .Setup(r => r.GetByIdAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await userService.GetOrCreateUserAsync(123, "John", "Doe", "johndoe");

        // Assert
        result.Should().Be(existingUser);
        mockRepository.Verify(r => r.CreateAsync(It.IsAny<BotUser>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetOrCreateUserAsync_WithNonExistingUser_CreatesAndReturnsNewUser()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var mockLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(mockRepository.Object, mockLogger.Object);
        
        mockRepository
            .Setup(r => r.GetByIdAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BotUser?)null);
        mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<BotUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BotUser user, CancellationToken _) => user);

        // Act
        var result = await userService.GetOrCreateUserAsync(123, "John", "Doe", "johndoe");

        // Assert
        result.Should().NotBeNull();
        result.TelegramId.Should().Be(123);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Username.Should().Be("johndoe");
        result.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithExistingUser_ReturnsUser()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var mockLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(mockRepository.Object, mockLogger.Object);
        
        var user = new BotUser { UserId = 123, FirstName = "John", LastName = "Doe" };
        mockRepository
            .Setup(r => r.GetByIdAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await userService.GetUserByIdAsync(123);

        // Assert
        result.Should().Be(user);
    }

    [Fact]
    public async Task UpdateUserAsync_UpdatesUserProperties()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var mockLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(mockRepository.Object, mockLogger.Object);
        
        var user = new BotUser { UserId = 123, FirstName = "John", LastName = "Doe", Username = "johndoe" };
        mockRepository
            .Setup(r => r.GetByIdAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<BotUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await userService.UpdateUserAsync(123, "John", "Smith", "johnsmith");

        // Assert
        result.Should().NotBeNull();
        result.LastName.Should().Be("Smith");
        result.Username.Should().Be("johnsmith");
    }

    [Fact]
    public async Task DeleteUserAsync_WithExistingUser_DeletesAndReturnsTrue()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var mockLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(mockRepository.Object, mockLogger.Object);
        
        mockRepository
            .Setup(r => r.DeleteAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await userService.DeleteUserAsync(123);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SearchUsersAsync_FiltersByFirstName()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var mockLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(mockRepository.Object, mockLogger.Object);
        
        var users = new List<BotUser>
        {
            new BotUser { UserId = 1, FirstName = "John", LastName = "Doe" },
            new BotUser { UserId = 2, FirstName = "Jane", LastName = "Smith" }
        };
        mockRepository
            .Setup(r => r.SearchAsync("John", It.IsAny<CancellationToken>()))
            .ReturnsAsync(users.Where(u => u.FirstName.Contains("John", StringComparison.OrdinalIgnoreCase)).ToList());

        // Act
        var result = await userService.SearchUsersAsync("John");

        // Assert
        result.Should().HaveCount(1);
        result[0].FirstName.Should().Be("John");
    }

    [Fact]
    public async Task GetUsersByStatusAsync_ReturnsFilteredUsers()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var mockLogger = new Mock<ILogger<UserService>>();
        var userService = new UserService(mockRepository.Object, mockLogger.Object);
        
        var activeUsers = new List<BotUser>
        {
            new BotUser { UserId = 1, Status = UserStatus.Active },
            new BotUser { UserId = 2, Status = UserStatus.Active }
        };
        mockRepository
            .Setup(r => r.GetByStatusAsync(UserStatus.Active, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeUsers);

        // Act
        var result = await userService.GetUsersByStatusAsync(UserStatus.Active);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(u => u.Status.Should().Be(UserStatus.Active));
    }
}
```

## XmlFormatterJsonExtensions

Provides JSON serialization and deserialization extensions for `XmlFormatter`, enabling conversion between XML formatter configuration and JSON representations. This is useful for persisting formatter settings, transmitting configurations between services, or storing formatter preferences in databases.

**Key features:**
- Serialize `XmlFormatter` to JSON with optional pretty-printing
- Deserialize JSON back to `XmlFormatter` instances with error handling
- Try-based deserialization for safe JSON parsing
- Preserves formatter configuration including pretty-printing preference

**Example usage:**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Formatters;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
    BotToken = "your-bot-token",
    BotUsername = "your-bot-username"
});

var serviceProvider = services.BuildServiceProvider();

// Resolve XmlFormatter
var xmlFormatter = new XmlFormatter(pretty: true);

// Example 1: Serialize formatter to JSON
string json = xmlFormatter.ToJson();
Console.WriteLine(json);
// Output: {"pretty":true}

// Example 2: Serialize with pretty printing
string prettyJson = xmlFormatter.ToJson(indented: true);
Console.WriteLine(prettyJson);
/* Output:
{
  "pretty": true
}
*/

// Example 3: Deserialize JSON back to formatter
string formatterJson = "{\"pretty\":false}";
var formatter = XmlFormatterJsonExtensions.FromJson(formatterJson);
if (formatter != null)
{
    Console.WriteLine($"Formatter pretty: {formatter.GetPretty()}");
}

// Example 4: Try-based deserialization (safe parsing)
string invalidJson = "{invalid}";
bool success = XmlFormatterJsonExtensions.TryFromJson(invalidJson, out var result);
Console.WriteLine(success); // false
Console.WriteLine(result); // null

// Example 5: Create formatter from configuration
var configJson = "{\"pretty\":true}";
var configuredFormatter = XmlFormatterJsonExtensions.FromJson(configJson);
if (configuredFormatter != null)
{
    Console.WriteLine($"Configured formatter pretty: {configuredFormatter.GetPretty()}");
}
```

## BotUserTests

The `BotUserTests` class contains unit tests for the `BotUser` and `Command` classes, focusing on user metadata management, validation, and command execution tracking.


It verifies display name generation, metadata storage/retrieval, validation rules, activity tracking, and command execution statistics including rate limiting functionality.

### Example usage

```csharp
using FluentAssertions;
using TelegramBotFramework.Models;

// Create a new user with first and last name
var user = new BotUser { TelegramId = 123456789, FirstName = "John", LastName = "Doe" };

// Test display name generation
string fullName = user.GetDisplayName(); // "John Doe"

// Test metadata management
user.SetMetadata("subscription_tier", "premium");
user.SetMetadata("last_active", DateTime.UtcNow.ToString());

string tier = user.GetMetadata("subscription_tier"); // "premium"
string missing = user.GetMetadata("non_existent_key"); // null

// Test activity tracking
user.UpdateActivity(); // Increments MessagesCount
user.UpdateActivity();

int messageCount = user.MessagesCount; // 2

// Test command execution tracking
var command = new Command { Name = "/announce", HandlerType = "AnnouncementHandler", RequiresAdmin = true };

// Test command validation and execution
command.RecordExecution(); // Increments ExecutionCount
command.RecordExecution();

int executionCount = command.ExecutionCount; // 2

// Test rate limiting
bool isRateLimited = command.IsRateLimited(10); // false (below limit of 10)
bool atLimit = command.IsRateLimited(10); // true (at limit)

// Test command permission checks
bool canExecute = command.CanExecuteBy(UserRole.Administrator); // true
bool cannotExecute = command.CanExecuteBy(UserRole.User); // false

// Test command patterns (handles aliases)
var patterns = command.GetCommandPatterns(); // ["/announce"]

var aliasedCommand = new Command { Name = "/start", HandlerType = "Handler", Alias = "/go" };
var aliasedPatterns = aliasedCommand.GetCommandPatterns(); // ["/start", "/go"]
```

## SessionService

The `SessionService` class provides centralized session management for Telegram bot applications, handling creation, retrieval, updating, and cleanup of user sessions. Sessions track conversation state, context data, and interaction history, enabling stateful conversations, menu navigation, and persistent context between messages across multiple interactions.

**Key features:**
- Session creation with automatic ID generation and configurable timeouts
- Active session retrieval and management
- Context data storage and retrieval for custom session properties
- Session state management (Active, Expired, Closed)
- Session expiration and cleanup operations
- Menu navigation tracking within sessions
- Activity recording for session management

**Example usage**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
  BotToken = "your-bot-token",
  BotUsername = "your-bot-username",
  SessionTimeoutMinutes = 30
});

var serviceProvider = services.BuildServiceProvider();

// Resolve the session service
var sessionService = serviceProvider.GetRequiredService<SessionService>();

// Create a new session for a user
var userId = 123456789L;
var chatId = 987654321L;

var session = await sessionService.CreateSessionAsync(userId, chatId);
Console.WriteLine($"Session created: {session.SessionId} for user {userId}");

// Create a session with custom timeout
var customTimeoutSession = await sessionService.CreateSessionAsync(
    userId, 
    chatId, 
    TimeSpan.FromMinutes(15)
);
Console.WriteLine($"Custom timeout session created: {customTimeoutSession.SessionId}");

// Get active session for a user
var activeSession = await sessionService.GetActiveSessionAsync(userId);
if (activeSession != null)
{
    Console.WriteLine($"Active session found: {activeSession.SessionId}");
}

// Get session by ID
var retrievedSession = await sessionService.GetSessionByIdAsync(session.SessionId);
if (retrievedSession != null)
{
    Console.WriteLine($"Retrieved session: {retrievedSession.SessionId}");
}

// Get all active sessions
var allActiveSessions = await sessionService.GetAllActiveSessionsAsync();
Console.WriteLine($"Found {allActiveSessions.Count} active sessions");

// Get sessions by user ID
var userSessions = await sessionService.GetSessionsByUserIdAsync(userId);
Console.WriteLine($"User {userId} has {userSessions.Count} sessions");

// Update session context data
bool contextUpdated = await sessionService.UpdateSessionContextAsync(
    session.SessionId, 
    "user_preferences.theme", 
    "dark"
);
Console.WriteLine($"Context updated: {contextUpdated}");

// Retrieve session context data
string? themePreference = await sessionService.GetSessionContextAsync(
    session.SessionId, 
    "user_preferences.theme"
);
Console.WriteLine($"Theme preference: {themePreference}"); // "dark"

// Navigate to a specific menu within the session
var updatedSession = await sessionService.NavigateToMenuAsync(
    session.SessionId, 
    "main_menu"
);
Console.WriteLine($"Navigated to menu: {updatedSession.CurrentMenuId}");

// Record session activity (updates LastActivityAt)
await sessionService.RecordSessionActivityAsync(session.SessionId);

// Close a session when done
bool sessionClosed = await sessionService.CloseSessionAsync(session.SessionId);
Console.WriteLine($"Session closed: {sessionClosed}");

// Delete a session
bool sessionDeleted = await sessionService.DeleteSessionAsync(session.SessionId);
Console.WriteLine($"Session deleted: {sessionDeleted}");

// Expire inactive sessions (older than 24 hours)
int expiredCount = await sessionService.ExpireInactiveSessionsAsync(
    TimeSpan.FromHours(24)
);
Console.WriteLine($"Expired {expiredCount} inactive sessions");

// Close all expired sessions
int closedExpiredCount = await sessionService.CloseExpiredSessionsAsync();
Console.WriteLine($"Closed {closedExpiredCount} expired sessions");
```

## JsonUtility

The `JsonUtility` class provides a comprehensive set of static methods for JSON serialization, deserialization, validation, and manipulation. It uses consistent `System.Text.Json` settings throughout the framework with camelCase property naming, case-insensitive matching, and null value handling, ensuring predictable and maintainable JSON processing across all components.

**Example usage**

```csharp
using TelegramBotFramework.Utilities;

// Example 1: Serialize an object to JSON
var user = new { Name = "John Doe", Email = "john@example.com", Role = "Admin" };
string json = JsonUtility.Serialize(user);
Console.WriteLine(json);
// Output: {"name":"John Doe","email":"john@example.com","role":"Admin"}

// Example 2: Serialize with pretty printing
string prettyJson = JsonUtility.Serialize(user, pretty: true);
Console.WriteLine(prettyJson);
/* Output:
{
  "name": "John Doe",
  "email": "john@example.com",
  "role": "Admin"
}
*/

// Example 3: Deserialize JSON back to object
string jsonData = "{\"name\":\"Jane Smith\",\"email\":\"jane@example.com\",\"role\":\"User\"}";
var deserializedUser = JsonUtility.Deserialize<dynamic>(jsonData);
Console.WriteLine(deserializedUser?.name); // "Jane Smith"

// Example 4: TryDeserialize with error handling
string invalidJson = "{invalid}";
bool success = JsonUtility.TryDeserialize<dynamic>(invalidJson, out var result);
Console.WriteLine(success); // false

// Example 5: Validate JSON
string testJson = "{\"valid\":true}";
bool isValid = JsonUtility.IsValidJson(testJson);
Console.WriteLine(isValid); // true

// Example 6: Parse JSON for flexible access
var parsed = JsonUtility.ParseJson("{\"user\":{\"profile\":{\"name\":\"Alice\"}}}");
if (parsed.HasValue)
{
    Console.WriteLine(parsed.Value.GetProperty("user").GetProperty("profile").GetProperty("name").GetString());
}

// Example 7: Merge two JSON objects
string json1 = "{\"name\":\"John\",\"age\":30}";
string json2 = "{\"age\":31,\"city\":\"NYC\"}";
string merged = JsonUtility.MergeJson(json1, json2);
Console.WriteLine(merged); // {"name":"John","age":31,"city":"NYC"}

// Example 8: Get nested property value
string userJson = "{\"user\":{\"profile\":{\"name\":\"Bob\",\"email\":\"bob@example.com\"}}}";
string email = JsonUtility.GetPropertyValue(userJson, "user.profile.email");
Console.WriteLine(email); // "\"bob@example.com\""

// Example 9: Pretty print existing JSON
string minified = "{\"data\":{\"items\":[1,2,3]}}";
string pretty = JsonUtility.PrettyPrint(minified);
Console.WriteLine(pretty);
/* Output:
{
  "data": {
    "items": [
      1,
      2,
      3
    ]
  }
}
*/

// Example 10: Minify JSON for storage/transmission
string largeJson = """
{
  "name": "Charlie",
  "email": "charlie@example.com",
  "preferences": {
    "theme": "dark",
    "language": "en"
  }
}
""";
string minifiedJson = JsonUtility.Minify(largeJson);
Console.WriteLine(minifiedJson.Length < largeJson.Length); // true
```

## StringExtensions

Provides extension methods for string manipulation and validation. Includes utilities for truncating strings, generating URL-friendly slugs, validating email addresses, checking alphanumeric content, repeating strings, reversing text, extracting numbers, ensuring string prefixes/suffixes, and capitalizing text.

**Example usage**

```csharp
using TelegramBotFramework.Utilities;

// Example 1: Truncate a long string
string longText = "This is a very long text that needs to be shortened for display purposes";
string truncated = longText.Truncate(20); // "This is a very long…"
Console.WriteLine(truncated);

// Example 2: Convert text to URL-friendly slug
string title = "Hello World! This is a Test";
string slug = title.ToSlug(); // "hello-world-this-is-a-test"
Console.WriteLine(slug);

// Example 3: Validate email address
string email = "user@example.com";
bool isValid = email.IsValidEmail(); // true
Console.WriteLine(isValid);

// Example 4: Check if string is alphanumeric
string alphanumeric = "abc123";
bool isAlphanumeric = alphanumeric.IsAlphanumeric(); // true
Console.WriteLine(isAlphanumeric);

// Example 5: Repeat a string multiple times
string repeated = "abc".Repeat(3); // "abcabcabc"
Console.WriteLine(repeated);

// Example 6: Reverse a string
string original = "Hello";
string reversed = original.Reverse(); // "olleH"
Console.WriteLine(reversed);

// Example 7: Extract numbers from text
string textWithNumbers = "Order #12345 shipped on 2024-07-16";
string numbers = textWithNumbers.ExtractNumbers(); // "1234520240716"
Console.WriteLine(numbers);

// Example 8: Ensure string starts with prefix
string path = "config/settings.json";
string ensuredPath = path.EnsureStartsWith("./"); // "./config/settings.json"
Console.WriteLine(ensuredPath);

// Example 9: Ensure string ends with suffix
string url = "https://example.com"
string fullUrl = url.EnsureEndsWith("/"); // "https://example.com/"
Console.WriteLine(fullUrl);

// Example 10: Capitalize first character
string name = "john doe";
string capitalized = name.Capitalize(); // "John doe"
Console.WriteLine(capitalized);
```

## BotConfigurationTests

The `BotConfigurationTests` class contains unit tests for the `BotConfiguration` class. It verifies configuration validation, default values, and various helper methods including admin management, custom settings, and session timeout calculations.

**Example usage**

```csharp
using FluentAssertions;
using TelegramBotFramework.Models;
using Xunit;

public class BotConfigurationTestsExample
{
    [Fact]
    public void ValidateConfiguration()
    {
        // Create a valid configuration
        var config = new BotConfiguration
        {
            BotToken = "123456789:ABCDEFghijklmnopqrstuvwxyz",
            BotUsername = "MyTestBot",
            SessionTimeoutMinutes = 30,
            MaxConcurrentRequests = 20,
            OwnerId = 123456789
        };
        
        // Validate configuration
        bool isValid = config.Validate();
        Assert.True(isValid);
        
        // Check if user is admin
        bool isAdmin = config.IsAdmin(123456789);
        Assert.True(isAdmin);
        
        // Get session timeout
        TimeSpan timeout = config.GetSessionTimeout();
        Assert.Equal(TimeSpan.FromMinutes(30), timeout);
        
        // Add admin
        config.AddAdmin(987654321);
        Assert.Contains(987654321, config.AdminIds);
        
        // Remove admin
        bool removed = config.RemoveAdmin(987654321);
        Assert.True(removed);
        
        // Set custom settings
        config.SetCustomSetting("api_endpoint", "https://api.example.com");
        string? endpoint = config.GetCustomSetting("api_endpoint");
        Assert.Equal("https://api.example.com", endpoint);
    }
}
```

## BotConfigurationAdditionalTests

The `BotConfigurationAdditionalTests` class contains additional unit tests for the `BotConfiguration` class, focusing on edge cases, null handling, and validation scenarios not covered in the main test classes. This test suite verifies proper initialization of collections, validation behavior, and configuration management under various conditions.

**Key features tested:**
- Null collection initialization (AdminIds, CustomSettings)
- Empty collection handling
- Validation edge cases (whitespace, minimum values)
- Configuration property management
- Admin management operations

**Example usage:**

```csharp
using FluentAssertions;
using TelegramBotFramework.Models;
using Xunit;

// Setup a BotConfiguration instance
var config = new BotConfiguration
{
    BotToken = "123456789:ABCDEFghijklmnopqrstuvwxyz",
    BotUsername = "MyBot",
    OwnerId = 123456789
};

// Test 1: Add admin to configuration with null AdminIds list
config.AddAdmin(987654321);
Assert.Contains(987654321, config.AdminIds);

// Test 2: Set custom settings with null CustomSettings dictionary
config.SetCustomSetting("api_endpoint", "https://api.example.com");
Assert.Equal("https://api.example.com", config.GetCustomSetting("api_endpoint"));

// Test 3: Check if user is admin (returns false for non-admin)
bool isAdmin = config.IsAdmin(999999999);
Assert.False(isAdmin);

// Test 4: Check if owner is admin (returns true even with empty AdminIds)
var configWithOwner = new BotConfiguration
{
    BotToken = "test-token",
    BotUsername = "TestBot",
    OwnerId = 123456789
};
Assert.True(configWithOwner.IsAdmin(123456789));

// Test 5: Get session timeout with default value
TimeSpan defaultTimeout = config.GetSessionTimeout();
Assert.Equal(TimeSpan.FromMinutes(30), defaultTimeout);

// Test 6: Get session timeout with custom value
config.SessionTimeoutMinutes = 60;
TimeSpan customTimeout = config.GetSessionTimeout();
Assert.Equal(TimeSpan.FromMinutes(60), customTimeout);

// Test 7: Validate configuration with valid values
bool isValid = config.Validate();
Assert.True(isValid);

// Test 8: Remove admin from configuration
bool removed = config.RemoveAdmin(987654321);
Assert.True(removed);
Assert.DoesNotContain(987654321, config.AdminIds);
```

## StringExtensionTests

Contains unit tests for the `StringExtensions` class, verifying the functionality of various string manipulation and validation extension methods. The test suite covers truncation, email validation, alphanumeric checks, string repetition, text reversal, number extraction, prefix/suffix operations, and capitalization.

**Example usage**

```csharp
using FluentAssertions;
using TelegramBotFramework.Utilities;
using Xunit;

public class StringExtensionTestsExample
{
    [Fact]
    public void Truncate_Example()
    {
        // Truncate a long string to 15 characters
        string longText = "This is a very long text that needs truncation";
        string result = longText.Truncate(15);
        
        result.Should().Be("This is a very…");
    }
    
    [Fact]
    public void IsValidEmail_Example()
    {
        // Validate email addresses
        "user@example.com".IsValidEmail().Should().BeTrue();
        "invalid-email".IsValidEmail().Should().BeFalse();
    }
    
    [Fact]
    public void IsAlphanumeric_Example()
    {
        // Check if string contains only alphanumeric characters
        "abc123".IsAlphanumeric().Should().BeTrue();
        "abc-123".IsAlphanumeric().Should().BeFalse();
    }
    
    [Fact]
    public void Repeat_Example()
    {
        // Repeat a string multiple times
        "ab".Repeat(3).Should().Be("ababab");
        "x".Repeat(5).Should().Be("xxxxx");
    }
    
    [Fact]
    public void ExtractNumbers_Example()
    {
        // Extract only digits from a mixed string
        "Order #12345 shipped on 2024-07-16".ExtractNumbers()
            .Should().Be("1234520240716");
        "No numbers here".ExtractNumbers().Should().BeEmpty();
    }
    
    [Fact]
    public void EnsureStartsWith_Example()
    {
        // Ensure string starts with specified prefix
        "config/settings.json".EnsureStartsWith("./")
            .Should().Be("./config/settings.json");
        "https://example.com".EnsureStartsWith("https://")
            .Should().Be("https://example.com");
    }
    
    [Fact]
    public void EnsureEndsWith_Example()
    {
        // Ensure string ends with specified suffix
        "hello".EnsureEndsWith("!").Should().Be("hello!");
        "https://example.com/".EnsureEndsWith("/")
            .Should().Be("https://example.com/");
    }
    
    [Fact]
    public void Capitalize_Example()
    {
        // Capitalize first character of string
        "john doe".Capitalize().Should().Be("John doe");
        "Hello World".Capitalize().Should().Be("Hello World");
    }
}
```

## CommandServiceAdditionalTests

The `CommandServiceAdditionalTests` class contains advanced unit tests for the `CommandService` class, extending the basic test coverage with scenarios for role-based command filtering, command execution tracking, rate limiting, and permission validation. This test suite verifies that commands are properly filtered based on user roles, disabled commands are handled correctly, execution counts are tracked accurately, and rate limiting works per-user rather than globally.

**Key features tested:**
- Role-based command access control (Admin, Moderator, User)
- Command execution tracking and statistics
- Disabled command handling and error reporting
- Rate limiting per user identifier
- Permission validation and error handling
- User status validation (active vs inactive users)

**Example usage:**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;
using TelegramBotFramework.Tests;
using Xunit;

// Setup test services
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());

// Create command service with mocked dependencies
var mockCommandRepository = new Mock<ICommandRepository>();
var mockUserService = new Mock<IUserService>();
var mockLogger = new Mock<ILogger<CommandService>>();

var commandService = new CommandService(
    mockCommandRepository.Object,
    mockUserService.Object,
    mockLogger.Object
);

// Test 1: Check available commands for different user roles
var adminCommands = await commandService.GetAvailableCommandsAsync(UserRole.Administrator);
var userCommands = await commandService.GetAvailableCommandsAsync(UserRole.User);
var moderatorCommands = await commandService.GetAvailableCommandsAsync(UserRole.Moderator);

// Test 2: Check if user can execute a command
bool canExecute = await commandService.CanUserExecuteCommandAsync(123, "help");

// Test 3: Execute a command and verify execution count is incremented
var executionResult = await commandService.ExecuteCommandAsync(context);
if (executionResult.IsValid)
{
    int executionCount = await commandService.GetCommandExecutionCountAsync("help");
    Console.WriteLine($"Command executed {executionCount} times");
}

// Test 4: Check rate limiting for a command
bool isRateLimited = await commandService.IsCommandRateLimitedAsync(123, "announce");
if (!isRateLimited)
{
    // User can execute the command
}

// Test 5: Record command execution for analytics
await commandService.RecordCommandExecutionAsync("help");
```

## DateTimeExtensions

Provides extension methods for common DateTime operations including Unix timestamp conversions, temporal comparisons, and calendar calculations. Useful for working with timestamps, scheduling, and date-based calculations in bot applications.

**Example usage**

```csharp
using TelegramBotFramework.Utilities;

// Example 1: Convert DateTime to Unix timestamp
var now = DateTime.UtcNow;
long unixTimestamp = now.ToUnixTimestamp();
Console.WriteLine($"Unix timestamp: {unixTimestamp}");

// Example 2: Convert Unix timestamp back to DateTime
var dateFromTimestamp = DateTimeExtensions.FromUnixTimestamp(unixTimestamp);
Console.WriteLine($"Date from timestamp: {dateFromTimestamp}");

// Example 3: Check if date is in the past or future
var pastDate = DateTime.UtcNow.AddDays(-1);
var futureDate = DateTime.UtcNow.AddDays(1);

Console.WriteLine($"Past date is past: {pastDate.IsPast()}"); // true
Console.WriteLine($"Future date is future: {futureDate.IsFuture()}"); // true

// Example 4: Get start and end of day
var today = DateTime.Today;
var startOfDay = today.StartOfDay();
var endOfDay = today.EndOfDay();

Console.WriteLine($"Start of day: {startOfDay}");
Console.WriteLine($"End of day: {endOfDay}");

// Example 5: Get start and end of week (Monday to Sunday)
var thisWeekStart = DateTime.UtcNow.StartOfWeek();
var thisWeekEnd = DateTime.UtcNow.EndOfWeek();

Console.WriteLine($"Week starts: {thisWeekStart:yyyy-MM-dd}");
Console.WriteLine($"Week ends: {thisWeekEnd:yyyy-MM-dd}");

// Example 6: Get start and end of month
var thisMonthStart = DateTime.UtcNow.StartOfMonth();
var thisMonthEnd = DateTime.UtcNow.EndOfMonth();

Console.WriteLine($"Month starts: {thisMonthStart:yyyy-MM-dd}");
Console.WriteLine($"Month ends: {thisMonthEnd:yyyy-MM-dd}");

// Example 7: Convert to human-readable relative time
var oneHourAgo = DateTime.UtcNow.AddHours(-1);
var twoDaysAgo = DateTime.UtcNow.AddDays(-2);

Console.WriteLine($"One hour ago: {oneHourAgo.ToRelativeTimeString()}"); // "1h ago"
Console.WriteLine($"Two days ago: {twoDaysAgo.ToRelativeTimeString()}"); // "2d ago"

// Example 8: Check if date is between two dates
var startDate = DateTime.UtcNow.AddDays(-7);
var endDate = DateTime.UtcNow.AddDays(7);
var testDate = DateTime.UtcNow.AddDays(-3);

Console.WriteLine($"Is test date between: {testDate.IsBetween(startDate, endDate)}"); // true

// Example 9: Add business days (skips weekends)
var monday = new DateTime(2024, 7, 15); // Monday
var nextMonday = monday.AddBusinessDays(5); // 5 business days later

Console.WriteLine($"Monday + 5 business days: {nextMonday:yyyy-MM-dd (dddd)}"); // "2024-07-22 (Monday)"

// Example 10: Calculate age from birth date
var birthDate = new DateTime(1990, 5, 15);
int age = birthDate.GetAge();

Console.WriteLine($"Age: {age} years");
```

## ValidationUtility

The `ValidationUtility` class provides a comprehensive set of static validation methods for common data formats and patterns used throughout the Telegram Bot Framework. It centralizes validation logic to ensure consistency and type safety when processing user input, configuration values, and API responses, helping prevent injection attacks and malformed data from entering your application.

**Key features:**
- Telegram-specific validation (user IDs, chat IDs, bot tokens)
- URL and IP address validation for webhook configurations
- Phone number and filename validation for user data handling
- Password strength validation for secure authentication
- GUID and numeric string validation for database operations
- Length validation for text fields and constraints

**Example usage**

```csharp
using TelegramBotFramework.Utilities;

// Example 1: Validate Telegram user and chat IDs before processing updates
var userId = 123456789L;
var chatId = 987654321L;

if (!ValidationUtility.IsValidTelegramUserId(userId))
{
    Console.WriteLine("Invalid user ID");
    return;
}

if (!ValidationUtility.IsValidTelegramChatId(chatId))
{
    Console.WriteLine("Invalid chat ID");
    return;
}

// Example 2: Validate bot token format before configuration
string botToken = "123456789:ABCDEFghijklmnopqrstuvwxyz";
if (!ValidationUtility.IsValidTelegramToken(botToken))
{
    Console.WriteLine("Invalid bot token format");
    return;
}

// Example 3: Validate webhook URL and IP address for deployment
string webhookUrl = "https://mybot.example.com/webhook";
string serverIp = "192.168.1.100";

if (!ValidationUtility.IsValidUrl(webhookUrl))
{
    Console.WriteLine("Invalid webhook URL");
    return;
}

if (!ValidationUtility.IsValidIPv4(serverIp))
{
    Console.WriteLine("Invalid server IP address");
    return;
}

// Example 4: Validate user-provided phone numbers and filenames
string phoneNumber = "+1 (555) 123-4567";
string filename = "user_document.pdf";

if (!ValidationUtility.IsValidPhoneNumber(phoneNumber))
{
    Console.WriteLine("Invalid phone number format");
    return;
}

if (!ValidationUtility.IsValidFilename(filename))
{
    Console.WriteLine("Invalid filename - contains illegal characters");
    return;
}

// Example 5: Validate password strength for user registration
string password = "SecureP@ssw0rd123";
if (!ValidationUtility.IsStrongPassword(password))
{
    Console.WriteLine("Password does not meet strength requirements");
    return;
}

// Example 6: Validate text field lengths and numeric inputs
string username = "johndoe";
if (!ValidationUtility.IsValidLength(username, 3, 20))
{
    Console.WriteLine("Username must be between 3 and 20 characters");
    return;
}

string ageInput = "25";
if (!ValidationUtility.IsNumeric(ageInput))
{
    Console.WriteLine("Age must be a numeric value");
    return;
}

// Example 7: Validate GUIDs for database operations
string productId = "550e8400-e29b-41d4-a716-446655440000";
if (!ValidationUtility.IsValidGuid(productId))
{
    Console.WriteLine("Invalid GUID format");
    return;
}

// Example 8: Validate command names for bot commands
string commandName = "/start";
if (!ValidationUtility.IsValidCommandName(commandName))
{
    Console.WriteLine("Invalid command name format");
    return;
}
```

## CryptoUtility

The `CryptoUtility` class provides cryptographic operations including hashing, password management, and encoding utilities. It offers secure methods for generating hashes, creating and verifying password hashes, generating random tokens, and encoding/decoding Base64 strings. These utilities are essential for implementing secure authentication, data integrity checks, and token-based systems in your bot applications.

**Key features:**
- Secure password hashing with PBKDF2 and salt
- SHA256 and MD5 hashing algorithms
- HMAC-SHA256 for message authentication
- Cryptographically secure random string and token generation
- Base64 encoding and decoding
- Password verification with hash comparison

**Example usage**

```csharp
using TelegramBotFramework.Utilities;

// Example 1: Generate SHA256 hash
string input = "Hello World!";
string sha256Hash = CryptoUtility.HashSHA256(input);
Console.WriteLine($"SHA256 hash: {sha256Hash}");

// Example 2: Generate MD5 hash (for non-security purposes only)
string md5Hash = CryptoUtility.HashMD5(input);
Console.WriteLine($"MD5 hash: {md5Hash}");

// Example 3: Hash and verify a password
string password = "SecurePassword123!";
string hashedPassword = CryptoUtility.HashPassword(password);
Console.WriteLine($"Hashed password: {hashedPassword}");

// Verify the password
bool isValid = CryptoUtility.VerifyPassword(password, hashedPassword);
Console.WriteLine($"Password valid: {isValid}"); // true

// Verify wrong password
bool isInvalid = CryptoUtility.VerifyPassword("WrongPassword", hashedPassword);
Console.WriteLine($"Wrong password valid: {isInvalid}"); // false

// Example 4: Generate a random string
string randomString = CryptoUtility.GenerateRandomString(16);
Console.WriteLine($"Random string: {randomString}"); // e.g., "xK9pLm2qR4sT7vW"

// Example 5: Generate a random token
string token = CryptoUtility.GenerateRandomToken(32);
Console.WriteLine($"Random token: {token}"); // e.g., "a3f7c9b2e1d8f0e4a9b6c3d2e5f8a7b"

// Example 6: Compute HMAC-SHA256 for message authentication
string secretKey = "my-secret-key";
string hmacHash = CryptoUtility.ComputeHmacSHA256(input, secretKey);
Console.WriteLine($"HMAC-SHA256: {hmacHash}");

// Example 7: Encode and decode Base64
string originalText = "Hello Telegram Bot!";
string encoded = CryptoUtility.EncodeBase64(originalText);
Console.WriteLine($"Base64 encoded: {encoded}"); // "SGVsbG8gVGVsZWdyYW0gQm90IQ=="

string? decoded = CryptoUtility.DecodeBase64(encoded);
Console.WriteLine($"Base64 decoded: {decoded}"); // "Hello Telegram Bot!"

// Example 8: Safe Base64 decoding with error handling
string? invalidBase64 = CryptoUtility.DecodeBase64("invalid==base64");
if (invalidBase64 == null)
{
    Console.WriteLine("Failed to decode invalid Base64");
}
```

## ReflectionHelper

The `ReflectionHelper` class provides utility methods for reflection operations, enabling dynamic type inspection, instantiation, and property manipulation at runtime. It simplifies common reflection patterns used throughout the framework for plugin architectures, dependency injection, and metadata-driven development.

**Key features:**
- Type discovery with `GetTypesImplementing<T>` and `GetTypesWithAttribute<T>`
- Dynamic object creation via `CreateInstance<T>` overloads
- Property inspection and manipulation with `GetProperties<T>`, `GetPropertyValue`, and `SetPropertyValue`
- Type hierarchy analysis with `IsSubclassOfGeneric`
- Display name generation for complex types including generics and nullable types
- Method inspection via `GetPublicMethods`
- Constant enumeration with `GetConstants`

**Example usage**

```csharp
using TelegramBotFramework.Utilities;

// Example 1: Discover all types implementing an interface
var commandHandlers = ReflectionHelper.GetTypesImplementing<ICommandHandler>()
    .Where(t => !t.IsAbstract);
foreach (var handlerType in commandHandlers)
{
    Console.WriteLine($"Found command handler: {handlerType.Name}");
}

// Example 2: Find types with a specific attribute
var commandTypes = ReflectionHelper.GetTypesWithAttribute<CommandAttribute>();
foreach (var commandType in commandTypes)
{
    Console.WriteLine($"Found command type: {commandType.Name}");
}

// Example 3: Create an instance dynamically
var botConfigType = typeof(TelegramBotFrameworkDotnetOptions);
var botConfig = ReflectionHelper.CreateInstance<TelegramBotFrameworkDotnetOptions>(botConfigType);
if (botConfig != null)
{
    Console.WriteLine("Configuration created successfully");
}

// Example 4: Get properties with a specific attribute
var properties = ReflectionHelper.GetProperties<InjectAttribute>(typeof(LocalCacheProvider));
foreach (var prop in properties)
{
    Console.WriteLine($"Injected property: {prop.Name}");
}

// Example 5: Get and set property values dynamically
var settings = new TelegramBotFrameworkDotnetOptions();
ReflectionHelper.SetPropertyValue(settings, "BotToken", "test_token_12345");
var token = ReflectionHelper.GetPropertyValue(settings, "BotToken") as string;
Console.WriteLine($"Token set to: {token}");

// Example 6: Check if type is a subclass of a generic type
bool isGenericList = ReflectionHelper.IsSubclassOfGeneric(typeof(CustomList<>), typeof(List<>));
Console.WriteLine($"Is CustomList<T> a List<T>: {isGenericList}");

// Example 7: Get display name for complex types
var displayName = ReflectionHelper.GetDisplayName(typeof(List<string>));
Console.WriteLine($"Display name: {displayName}"); // "List<String>"

// Example 8: Get all public methods of a type
var methods = ReflectionHelper.GetPublicMethods(typeof(StringBuilder));
Console.WriteLine($"StringBuilder has {methods.Count()} public methods");

// Example 9: Get all constants from a type
var constants = ReflectionHelper.GetConstants(typeof(HttpStatusCode));
foreach (var constant in constants)
{
    Console.WriteLine($"Constant: {constant.Name} = {constant.GetValue(null)}");
}

// Example 2: Check if collection is null or empty
List<string>? nullList = null;
IEnumerable<int> emptyList = Enumerable.Empty<int>();

bool isNullOrEmpty1 = nullList.IsNullOrEmpty(); // true
bool isNullOrEmpty2 = emptyList.IsNullOrEmpty(); // true
bool isNullOrEmpty3 = users.IsNullOrEmpty(); // false

// Example 3: Check if collection has items
bool hasItems1 = nullList.HasItems(); // false
bool hasItems2 = emptyList.HasItems(); // false
bool hasItems3 = users.HasItems(); // true

// Example 4: Shuffle a collection randomly
var shuffledNumbers = Enumerable.Range(1, 10).Shuffle();
Console.WriteLine("Shuffled: " + string.Join(", ", shuffledNumbers));

// Example 5: Add multiple items to a collection at once
var tags = new HashSet<string>();
tags.AddRange(new[] { "csharp", "dotnet", "telegram", "bot" });
Console.WriteLine("Tags count: " + tags.Count); // 4

// Example 6: Convert to dictionary safely (keeps first occurrence on duplicates)
var people = new[] {
    new { Id = 1, Name = "Alice" },
    new { Id = 2, Name = "Bob" },
    new { Id = 1, Name = "Alice Duplicate" } // Duplicate key - first is kept
};

var peopleDict = people.ToDictionarySafe(p => p.Id, p => p.Name);
Console.WriteLine("Person with ID 1: " + peopleDict[1]); // "Alice"

// Example 7: Execute action for each item in collection (useful in LINQ chains)
var numbers = new List<int> { 1, 2, 3, 4, 5 };
var processedNumbers = numbers
    .ForEach(n => Console.WriteLine($"Processing {n}"))
    .Select(n => n * 2)
    .ToList();

// Output: Processing 1, Processing 2, Processing 3, Processing 4, Processing 5
// Result: [2, 4, 6, 8, 10]
```

## EnumHelper

The `EnumHelper` class provides utility methods for working with enumerations in .NET. It offers functionality for parsing enum values, converting enums to dictionaries, retrieving descriptions from enum members, checking flag values, and working with enum attributes. This utility is particularly useful for creating dropdown lists, validating enum inputs, and working with enum metadata.

**Example usage**

```csharp
using System.ComponentModel;
using TelegramBotFramework.Utilities;

// Example 1: Define an enum with Description attributes
public enum UserRole
{
    [Description("Regular user with basic permissions")]
    User = 0,
    
    [Description("Trusted user with additional capabilities")]
    Trusted = 1,
    
    [Description("Administrator with full system access")]
    Admin = 2,
    
    [Description("Super administrator with unrestricted access")]
    Owner = 3
}

// Example 2: Get all enum values
var allRoles = EnumHelper.GetAllValues<UserRole>();
foreach (var role in allRoles)
{
    Console.WriteLine($"Role: {role}");
}

// Example 3: Safely parse a string to an enum with fallback
string userInput = "admin";
UserRole parsedRole = EnumHelper.TryParse(userInput, UserRole.User);
Console.WriteLine($"Parsed role: {parsedRole}"); // UserRole.Admin

// Example 4: Get description from DescriptionAttribute
UserRole role = UserRole.Admin;
string description = role.GetDescription();
Console.WriteLine($"Admin description: {description}"); // "Administrator with full system access"

// Example 5: Convert enum to dictionary for UI binding
var roleDictionary = EnumHelper.EnumToDictionary<UserRole>();
foreach (var kvp in roleDictionary)
{
    Console.WriteLine($"{kvp.Key} = {kvp.Value}");
}

// Example 6: Create display dictionary with descriptions
var displayDictionary = EnumHelper.EnumToDisplayDictionary<UserRole>();
foreach (var kvp in displayDictionary)
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
}

// Example 7: Check if a string is a valid enum value
bool isValid = EnumHelper.IsValid<UserRole>("trusted");
Console.WriteLine($"Is 'trusted' valid: {isValid}"); // true

// Example 8: Get the name of an enum value
string enumName = EnumHelper.GetName(UserRole.Owner);
Console.WriteLine($"Enum name: {enumName}"); // "Owner"

// Example 9: Get numeric value of an enum
UserRole roleValue = UserRole.Trusted;
object numericValue = roleValue.GetNumericValue();
Console.WriteLine($"Numeric value: {numericValue}"); // 1

// Example 10: Check if enum value has a specific flag
[Flags]
public enum PermissionFlags
{
    None = 0,
    Read = 1,
    Write = 2,
    Execute = 4,
    All = Read | Write | Execute
}

var permissions = PermissionFlags.Read | PermissionFlags.Write;
bool hasWrite = permissions.HasFlag(PermissionFlags.Write);
Console.WriteLine($"Has write permission: {hasWrite}"); // true
```

## MessageFormatter

The `MessageFormatter` class provides utility methods for formatting Telegram messages into different output formats including plain text, markdown, HTML, and conversation threads. It's particularly useful for logging, debugging, and displaying messages in various contexts within your bot application.

**Key features:**
- Format messages as plain text for logging purposes
- Generate Telegram-compatible markdown formatting for message display
- Create HTML-formatted messages for web interfaces
- Format message collections as conversation threads
- Truncate messages for preview displays
- Generate detailed debug information for troubleshooting

**Example usage**

```csharp
using TelegramBotFramework.Formatters;
using TelegramBotFramework.Models;

// Create a sample message
var message = new Message
{
    MessageId = 123,
    UserId = 456,
    ChatId = 789,
    Content = "Hello! This is a test message with _special_ characters.",
    Type = MessageType.Text,
    CreatedAt = DateTime.UtcNow,
    IsEdited = false
};

// Format as plain text for logging
string plainText = MessageFormatter.FormatAsPlainText(message);
Console.WriteLine(plainText);
/* Output:
[2024-07-16 14:30:00] 456:
Hello! This is a test message with _special_ characters.
*/

// Format as Telegram markdown for sending to users
string markdown = MessageFormatter.FormatAsMarkdown(message);
Console.WriteLine(markdown);
/* Output:
**[14:30]** _456_: Hello! This is a test message with \_special\_ characters.
*/

// Format as HTML for web interfaces
string html = MessageFormatter.FormatAsHtml(message);
Console.WriteLine(html);
/* Output:
<div class='message'><span class='timestamp'>[14:30]</span> <strong>456</strong>: <span class='text'>Hello! This is a test message with _special_ characters.</span></div>
*/

// Format multiple messages as a conversation thread
var messages = new List<Message> { message };
string conversation = MessageFormatter.FormatAsConversation(messages);
Console.WriteLine(conversation);

// Truncate message for preview display
string preview = MessageFormatter.TruncateForPreview(message, 50);
Console.WriteLine(preview); // "Hello! This is a test message with _special_ charact…"

// Get detailed debug information
string debugInfo = MessageFormatter.FormatForDebug(message);
Console.WriteLine(debugInfo);
```

## UserSession

The `UserSession` class represents an active user session that tracks conversation state, context data, and interaction history. Sessions store user-specific data across multiple interactions, enabling stateful conversations, menu navigation, and persistent context between messages. The class provides methods for managing session state, context data, command history, and session lifecycle tracking.

**Key features:**
- Session identification with unique `SessionId`
- User and chat metadata (UserId, ChatId) for context tracking
- Session state management with `SessionState` enum (Active, Idle, Suspended, Expired, Closed)
- Context data storage via `ContextData` dictionary for custom session properties
- Command history tracking with automatic pruning
- Session expiration and activity tracking
- Convenience properties like `IsActive` and `IsExpired()`

**Example usage**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Models;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
    BotToken = "your-bot-token",
    BotUsername = "your-bot-username"
});

var serviceProvider = services.BuildServiceProvider();
var orchestrator = serviceProvider.GetRequiredService<IBotOrchestrator>();

// Create or retrieve a user session
var userId = 123456789L;
var chatId = 987654321L;

// Get or create a session for the user
var session = await orchestrator.GetUserSessionAsync(userId);

// Update session state and context
session.State = SessionState.Active;
session.CurrentContext = "admin_panel";
session.CurrentMenuId = "main_menu";

// Store custom context data
session.SetContextData("preferences.theme", "dark");
session.SetContextData("preferences.language", "en");

// Retrieve context data
string? theme = session.GetContextData("preferences.theme");
Console.WriteLine($"User theme preference: {theme}"); // "dark"

// Track user activity
session.UpdateActivity();
Console.WriteLine($"Interaction count: {session.InteractionCount}"); // 1

// Add command to history
session.AddCommandToHistory("/admin");
session.AddCommandToHistory("/settings");

// Check session status
bool isExpired = session.IsExpired();
bool isActive = session.IsActive;
Console.WriteLine($"Session active: {isActive}, Expired: {isExpired}");

// Get session duration
var duration = session.GetDuration();
Console.WriteLine($"Session duration: {duration.TotalMinutes} minutes");

// Clear specific context data
session.RemoveContextData("preferences.language");

// Clear all context data
session.ClearContextData();

// Access session properties
Console.WriteLine($"Session ID: {session.SessionId}");
Console.WriteLine($"User ID: {session.UserId}");
Console.WriteLine($"Chat ID: {session.ChatId}");
Console.WriteLine($"Created: {session.CreatedAt}");
Console.WriteLine($"Last activity: {session.LastActivityAt}");
Console.WriteLine($"Current context: {session.CurrentContext}");
Console.WriteLine($"Current menu: {session.CurrentMenuId}");
```

## IBotOrchestrator

The `IBotOrchestrator` interface serves as the central coordinator for all bot operations, providing a unified API for processing user messages, executing commands, managing menus, and handling sessions. It orchestrates interactions between the various framework services (user management, command processing, session handling, message processing, and menu navigation) through a middleware pipeline, enabling clean separation of concerns and extensible bot behavior.

The orchestrator handles the complete lifecycle of user interactions: from initial message processing through command execution, menu display, and session management, returning comprehensive execution contexts that contain the results of each operation.

**Key features:**
- Message processing with automatic user/session creation
- Command execution with parameter support
- Menu display and navigation
- Session lifecycle management (retrieval and termination)
- Integration with middleware pipeline for extensibility
- Comprehensive execution context tracking

**Example usage**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Models;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
  BotToken = "your-bot-token",
  BotUsername = "your-bot-username"
});

var serviceProvider = services.BuildServiceProvider();
var orchestrator = serviceProvider.GetRequiredService<IBotOrchestrator>();

// Example 1: Process a user message
var userId = 123456789L;
var chatId = 987654321L;

var messageContext = await orchestrator.ProcessUserMessageAsync(
  userId,
  chatId,
  "/start Welcome to the bot!",
  "John",
  "Doe"
);

if (messageContext.IsValid)
{
  Console.WriteLine("Message processed successfully!");
  Console.WriteLine($"Context ID: {messageContext.ContextId}");
}

// Example 2: Execute a user command
var commandContext = await orchestrator.ExecuteUserCommandAsync(
  userId,
  chatId,
  "/help"
);

if (commandContext.IsValid && commandContext.Command != null)
{
  Console.WriteLine($"Command executed: {commandContext.Command.Name}");
  Console.WriteLine($"Response: {commandContext.GetState<string>("response")}");
}

// Example 3: Display a menu
var mainMenu = await orchestrator.DisplayMenuAsync(userId, "main_menu");
Console.WriteLine($"Displaying menu: {mainMenu.Title}");

// Example 4: Handle a menu button click
bool buttonHandled = await orchestrator.HandleMenuButtonAsync(
  userId,
  "main_menu",
  "/settings"
);

if (buttonHandled)
{
  Console.WriteLine("Menu button handled successfully!");
}

// Example 5: Get user session
var session = await orchestrator.GetUserSessionAsync(userId);
Console.WriteLine($"Session state: {session.State}");

// Example 6: End user session
bool sessionEnded = await orchestrator.EndUserSessionAsync(userId);
Console.WriteLine($"Session ended: {sessionEnded}");
```

## BotController

The `BotController` class serves as the main API controller for handling incoming Telegram bot updates and commands. It provides REST endpoints for processing messages, retrieving user information, managing sessions, and accessing bot commands and menus. The controller integrates with the framework's core services (user management, command processing, session handling, and message processing) to provide a complete bot API interface.

The controller handles the complete lifecycle of user interactions: from initial message processing through command execution, session management, and user information retrieval, returning appropriate HTTP responses for each operation.

**Key features:**
- RESTful API endpoints for bot operations
- Integration with core framework services
- Health check endpoint for monitoring
- Error handling and logging
- Support for both command and non-command messages

**Example usage:**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Controllers;
using TelegramBotFramework.Models;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
    BotToken = "your-bot-token",
    BotUsername = "your-bot-username"
});

var serviceProvider = services.BuildServiceProvider();

// Resolve the BotController
var botController = serviceProvider.GetRequiredService<BotController>();

// Example 1: Health check
var healthResult = botController.Health();
Console.WriteLine($"Health status: {healthResult.StatusCode}");

// Example 2: Process a user message
var messageRequest = new BotController.ProcessMessageRequest
{
    UserId = 123456789,
    ChatId = 987654321,
    FirstName = "John",
    LastName = "Doe",
    Content = "/start Welcome to the bot!",
    MessageType = MessageType.Text
};

var messageResult = await botController.ProcessMessage(messageRequest);
if (messageResult is OkObjectResult okResult)
{
    var response = okResult.Value as dynamic;
    Console.WriteLine($"Message processed successfully! Context ID: {response.contextId}");
}

// Example 3: Get user information
var userResult = await botController.GetUser(123456789);
if (userResult is OkObjectResult okUserResult)
{
    var user = okUserResult.Value;
    Console.WriteLine($"User found: {user}");
}

// Example 4: Get active session
var sessionResult = await botController.GetSession(123456789);
if (sessionResult is OkObjectResult okSessionResult)
{
    var session = okSessionResult.Value;
    Console.WriteLine($"Session found: {session}");
}

// Example 5: Get available commands
var commandsResult = await botController.GetCommands();
if (commandsResult is OkObjectResult okCommandsResult)
{
    var commands = okCommandsResult.Value;
    Console.WriteLine($"Available commands: {commands}");
}

// Example 6: Get menu
var menuResult = await botController.GetMenu("main_menu");
if (menuResult is OkObjectResult okMenuResult)
{
    var menu = okMenuResult.Value;
    Console.WriteLine($"Menu found: {menu}");
}
```

## AdminController

The `AdminController` class provides administrative endpoints for managing bot configuration, users, commands, menus, and sessions. It exposes RESTful API methods for administrative operations that require elevated privileges, including user management (promotion/demotion, banning/unbanning), command lifecycle management, menu operations, and session cleanup.

This controller is essential for building admin dashboards, management UIs, and automated administration tools that need to control bot behavior programmatically.



**Key features:**
- Bot configuration retrieval and monitoring
- User administration (promote/demote administrators, ban/unban users)
- Command management (register, retrieve, and delete commands)
- Menu operations (list active menus)
- Session management (close expired sessions)
- Statistics and monitoring endpoints

**Example usage:**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Controllers;
using TelegramBotFramework.Models;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
  BotToken = "your-bot-token",
  BotUsername = "your-bot-username"
});

var serviceProvider = services.BuildServiceProvider();

// Resolve the AdminController
var adminController = serviceProvider.GetRequiredService<AdminController>();

// Example 1: Get bot configuration
var configResult = adminController.GetConfiguration();
if (configResult is OkObjectResult okConfigResult)
{
  var config = okConfigResult.Value;
  Console.WriteLine($"Bot username: {config.botUsername}");
  Console.WriteLine($"Session timeout: {config.sessionTimeoutMinutes} minutes");
}

// Example 2: Get statistics
var statsResult = await adminController.GetStatistics();
if (statsResult is OkObjectResult okStatsResult)
{
  var stats = okStatsResult.Value;
  Console.WriteLine($"Total users: {stats.totalUsers}");
  Console.WriteLine($"Active users: {stats.activeUsers}");
  Console.WriteLine($"Admin count: {stats.adminCount}");
}

// Example 3: Get all administrators
var adminsResult = await adminController.GetAdministrators();
if (adminsResult is OkObjectResult okAdminsResult)
{
  var admins = okAdminsResult.Value as List<BotUser>;
  Console.WriteLine($"Found {admins?.Count} administrators");
}

// Example 4: Promote user to administrator
var promoteResult = await adminController.PromoteToAdmin(123456789);
if (promoteResult is OkObjectResult okPromoteResult)
{
  Console.WriteLine("User promoted to admin successfully");
}

// Example 5: Ban user
var banResult = await adminController.BanUser(987654321);
if (banResult is OkObjectResult okBanResult)
{
  Console.WriteLine("User banned successfully");
}

// Example 6: Register a new command
var newCommand = new Command
{
  Name = "/announce",
  Description = "Send announcement to all users",
  HandlerType = "AnnouncementCommandHandler",
  IsActive = true,
  RequiresAdmin = true
};
var registerResult = await adminController.RegisterCommand(newCommand);
if (registerResult is CreatedResult createdResult)
{
  Console.WriteLine($"Command registered at: {createdResult.Location}");
}

// Example 7: Get a specific command
var getCommandResult = await adminController.GetCommand("/announce");
if (getCommandResult is OkObjectResult okGetCommandResult)
{
  var command = okGetCommandResult.Value as Command;
  Console.WriteLine($"Command found: {command?.Name}");
}

// Example 8: Get all active menus
var menusResult = await adminController.GetMenus();
if (menusResult is OkObjectResult okMenusResult)
{
  var menus = okMenusResult.Value as List<Menu>;
  Console.WriteLine($"Found {menus?.Count} active menus");
}

// Example 9: Close expired sessions
var closeSessionsResult = await adminController.CloseExpiredSessions();
if (closeSessionsResult is OkObjectResult okCloseResult)
{
  var result = okCloseResult.Value;
  Console.WriteLine($"Closed {result.message}");
}
```

## JsonFormatter

The `JsonFormatter` class provides utility methods for converting objects, collections, and Telegram messages into JSON format. It handles proper JSON serialization with camelCase property naming, case-insensitive matching, and null value handling. This formatter is particularly useful for API responses, logging message history, and generating structured JSON reports for interoperability with other systems.

**Key features:**
- Convert single objects to JSON with automatic property detection and recursive serialization
- Format collections of any type to JSON with a root `items` object containing the collection and `count` property
- Format Telegram `Message` objects into standardized JSON structure
- Format error messages as JSON for logging and monitoring systems
- Support for both pretty-printed (default) and compact JSON output via constructor parameter
- Automatic JSON escaping for special characters in content
- Recursive serialization of complex object graphs

**Example usage**

```csharp
using TelegramBotFramework.Formatters;
using TelegramBotFramework.Models;

// Create a JSON formatter with pretty printing (default)
var jsonFormatter = new JsonFormatter();

// Format a single object to JSON
var user = new { Id = 1, Name = "John Doe", Email = "john@example.com", Role = "Admin" };
string userJson = jsonFormatter.Format(user);

// Output: {"id":1,"name":"John Doe","email":"john@example.com","role":"Admin"}

// Format a collection to JSON
var users = new List<object>
{
    new { Id = 1, Name = "John Doe", Email = "john@example.com", Role = "Admin" },
    new { Id = 2, Name = "Jane Smith", Email = "jane@example.com", Role = "User" }
};

string usersJson = jsonFormatter.Format(users);

// Output: {"items":[{"id":1,"name":"John Doe","email":"john@example.com","role":"Admin"},{"id":2,"name":"Jane Smith","email":"jane@example.com","role":"User"}],"count":2}

// Format a Telegram Message to JSON
var message = new Message
{
    MessageId = 123,
    Content = "Hello World!",
    UserId = 456,
    ChatId = 789,
    CreatedAt = DateTime.UtcNow,
    Type = MessageType.Text
};

string messageJson = jsonFormatter.FormatMessage(message);

// Output: {"id":123,"content":"Hello World!","userId":456,"chatId":789,"createdAt":"2024-07-16T14:30:00Z","isEdited":false,"type":"Text"}

// Format multiple messages to JSON
var messages = new List<Message> { message };
string messagesJson = jsonFormatter.FormatMessages(messages);

// Output: {"messages":[{"id":123,"content":"Hello World!","userId":456,"chatId":789,"createdAt":"2024-07-16T14:30:00Z","isEdited":false,"type":"Text"}],"count":1}

// Format an error message as JSON for logging
string errorJson = jsonFormatter.FormatError(
    "API_TIMEOUT",
    "Telegram API request timed out",
    "The request to getUpdates exceeded 30 seconds"
);

// Output: {"error":"API_TIMEOUT","message":"Telegram API request timed out","details":"The request to getUpdates exceeded 30 seconds","timestamp":"2024-07-16T14:30:00Z"}

// Create a formatter with compact JSON output (no pretty printing)
var compactFormatter = new JsonFormatter(pretty: false);
string compactJson = compactFormatter.Format(user);
// Output: {"id":1,"name":"John Doe","email":"john@example.com","role":"Admin"}
```

## XmlFormatter

The `XmlFormatter` class provides utility methods for converting objects, collections, and Telegram messages into XML format. It handles proper XML escaping, hierarchical structures, and supports both pretty-printed and compact XML output. This formatter is particularly useful for exporting bot data, logging message history, and generating structured XML reports for interoperability with other systems.

**Key features:**
- Convert single objects to XML with automatic property detection and recursive serialization
- Format collections of any type to XML with a root `<items>` element containing `<item>` elements
- Format Telegram `Message` objects into standardized XML structure
- Format error messages as XML for logging and monitoring systems
- Support for both pretty-printed (default) and compact XML output via constructor parameter
- Automatic XML escaping for special characters in content
- Recursive serialization of complex object graphs

**Example usage**

```csharp
using TelegramBotFramework.Formatters;
using TelegramBotFramework.Models;

// Create an XML formatter with pretty printing (default)
var xmlFormatter = new XmlFormatter();

// Format a single object to XML
var user = new { Id = 1, Name = "John Doe", Email = "john@example.com", Role = "Admin" };
string userXml = xmlFormatter.Format(user);

/* Output:
<User>
  <Id>1</Id>
  <Name>John Doe</Name>
  <Email>john@example.com</Email>
  <Role>Admin</Role>
</User>
*/

// Format a collection to XML
var users = new List<object>
{
    new { Id = 1, Name = "John Doe", Email = "john@example.com", Role = "Admin" },
    new { Id = 2, Name = "Jane Smith", Email = "jane@example.com", Role = "User" }
};

string usersXml = xmlFormatter.Format(users);

/* Output:
<items>
  <item>
    <Id>1</Id>
    <Name>John Doe</Name>
    <Email>john@example.com</Email>
    <Role>Admin</Role>
  </item>
  <item>
    <Id>2</Id>
    <Name>Jane Smith</Name>
    <Email>jane@example.com</Email>
    <Role>User</Role>
  </item>
</items>
*/

// Format a Telegram Message to XML
var message = new Message
{
    MessageId = 123,
    Content = "Hello World!",
    UserId = 456,
    ChatId = 789,
    CreatedAt = DateTime.UtcNow,
    Type = MessageType.Text
};

string messageXml = xmlFormatter.FormatMessage(message);

/* Output:
<message>
  <id>123</id>
  <content>Hello World!</content>
  <userId>456</userId>
  <chatId>789</chatId>
  <createdAt>2024-07-16T14:30:00.0000000Z</createdAt>
  <type>Text</type>
</message>
*/

// Format multiple messages to XML
var messages = new List<Message> { message };
string messagesXml = xmlFormatter.FormatMessages(messages);

/* Output:
<messages count="1">
  <message>
    <id>123</id>
    <content>Hello World!</content>
    <userId>456</userId>
    <chatId>789</chatId>
    <createdAt>2024-07-16T14:30:00.0000000Z</createdAt>
    <type>Text</type>
  </message>
</messages>
*/

// Format an error message as XML for logging
string errorXml = xmlFormatter.FormatError(
    "API_TIMEOUT",
    "Telegram API request timed out",
    "The request to getUpdates exceeded 30 seconds"
);

/* Output:
<error>
  <code>API_TIMEOUT</code>
  <message>Telegram API request timed out</message>
  <details>The request to getUpdates exceeded 30 seconds</details>
  <timestamp>2024-07-16T14:30:00.0000000Z</timestamp>
</error>
*/

// Create a formatter with compact XML output (no pretty printing)
var compactFormatter = new XmlFormatter(pretty: false);
string compactXml = compactFormatter.Format(user);
// Output: <User><Id>1</Id><Name>John Doe</Name><Email>john@example.com</Email><Role>Admin</Role></User>
```

## CsvFormatter

The `CsvFormatter` class provides utility methods for converting collections of objects and Telegram messages into CSV format. It handles proper escaping of fields, quoted values, and supports both single objects and collections through generic methods. This formatter is particularly useful for exporting bot data, logging message history, and generating reports in a standardized CSV format.

**Key features:**
- Convert collections of any type to CSV with automatic property detection
- Format Telegram `Message` objects into CSV rows with standard columns
- Format error messages as CSV for logging and monitoring systems
- Proper CSV escaping and quoting for fields containing commas, quotes, or newlines
- Support for generic collections via `Format<T>(IEnumerable<T>)` overload

**Example usage**

```csharp
using TelegramBotFramework.Formatters;
using TelegramBotFramework.Models;

// Format a collection of objects to CSV
var users = new List<User> 
{
    new User { Id = 1, Name = "John Doe", Email = "john@example.com", Role = "Admin" },
    new User { Id = 2, Name = "Jane Smith", Email = "jane@example.com", Role = "User" },
    new User { Id = 3, Name = "Bob Johnson", Email = "bob@example.com", Role = "User" }
};

var csvFormatter = new CsvFormatter();
string csvOutput = csvFormatter.Format(users);

// Output:
// Id,Name,Email,Role
// 1,John Doe,john@example.com,Admin
// 2,Jane Smith,jane@example.com,User
// 3,Bob Johnson,bob@example.com,User

// Format a single object
var singleUser = new User { Id = 4, Name = "Alice Brown", Email = "alice@example.com", Role = "Moderator" };
string singleUserCsv = csvFormatter.Format(singleUser);
// Output: Id,Name,Email,Role\n4,Alice Brown,alice@example.com,Moderator

// Format Telegram messages to CSV
var messages = new List<Message>
{
    new Message 
    {
        MessageId = 123,
        Content = "Hello World!",
        UserId = 456,
        ChatId = 789,
        CreatedAt = DateTime.UtcNow,
        Type = MessageType.Text
    },
    new Message 
    {
        MessageId = 124,
        Content = "How are you?",
        UserId = 457,
        ChatId = 789,
        CreatedAt = DateTime.UtcNow.AddMinutes(5),
        Type = MessageType.Text
    }
};

string messagesCsv = csvFormatter.FormatMessages(messages);

// Format error messages as CSV for logging
string errorCsv = csvFormatter.FormatError(
    "API_TIMEOUT",
    "Telegram API request timed out",
    "The request to getUpdates exceeded 30 seconds"
);
```

## InlineKeyboardBuilderTests

The `InlineKeyboardBuilderTests` class provides comprehensive unit tests for the `InlineKeyboardBuilder` class, verifying the behavior of inline keyboard construction, button management, row handling, validation, and conversion methods. It uses xUnit for test execution and FluentAssertions for readable assertions, ensuring that inline keyboards are built correctly according to Telegram Bot API specifications.

**Key test scenarios:**
- Single button construction with `Build_WithSingleCallbackButton_CreatesOneRowOneButton`
- URL button handling with `Build_WithUrlButton_SetsTypeAndUrl`
- Switch inline query buttons with `Build_WithSwitchInlineButton_SetsTypeAndQuery`
- Automatic row wrapping with `Build_AutoWrapsButtonsAtMaxPerRow`
- Manual row breaks with `NewRow_ForcesRowBreakBeforeMaxReached`
- Button label conversion with `ToButtonLabels_ReturnsTwoDimensionalLabelArray`
- Menu model conversion with `ToMenu_ConvertsMarkupToMenuModel`
- Validation scenarios with `Build_WithNoButtons_ThrowsInvalidOperationException`, `AddButton_WithCallbackDataExceeding64Bytes_ThrowsArgumentException`, and `AddButton_WithEmptyText_ThrowsArgumentException`

**Example usage:**

```csharp
using FluentAssertions;
using TelegramBotFramework.Keyboard;
using Xunit;

// Test that a single callback button creates correct markup
var markup = InlineKeyboardBuilder.Create()
    .AddButton("Click me", "click")
    .Build();

markup.RowCount.Should().Be(1);
markup.TotalButtonCount.Should().Be(1);
markup.InlineKeyboard[0][0].Text.Should().Be("Click me");
markup.InlineKeyboard[0][0].CallbackData.Should().Be("click");

// Test URL button creation
var urlMarkup = InlineKeyboardBuilder.Create()
    .AddUrlButton("Visit Website", "https://example.com")
    .Build();

urlMarkup.InlineKeyboard[0][0].Type.Should().Be(InlineButtonType.Url);
urlMarkup.InlineKeyboard[0][0].Url.Should().Be("https://example.com");

// Test auto-wrapping with max buttons per row
var wrappedMarkup = InlineKeyboardBuilder.Create(maxButtonsPerRow: 2)
    .AddButton("Button 1", "btn1")
    .AddButton("Button 2", "btn2")
    .AddButton("Button 3", "btn3")
    .Build();

wrappedMarkup.RowCount.Should().Be(2);
wrappedMarkup.InlineKeyboard[0].Count.Should().Be(2);
wrappedMarkup.InlineKeyboard[1].Count.Should().Be(1);

// Test conversion to menu model
var menu = InlineKeyboardBuilder.Create()
    .AddButton("Help", "help")
    .AddUrlButton("Docs", "https://docs.example.com")
    .ToMenu("main_menu", "Main Menu");

menu.Id.Should().Be("main_menu");
menu.Title.Should().Be("Main Menu");
```

## ExecutionContextTests

The `ExecutionContextTests` class provides unit tests for the `ExecutionContext` class, verifying the behavior of context initialization, state management, error handling, validation, and lifecycle tracking. It uses xUnit for test execution and FluentAssertions for readable assertions, ensuring that execution contexts maintain proper state throughout message processing pipelines and command execution flows.

**Key test scenarios:**
- Context initialization with `Constructor_WithDefaultValues_InitializesCorrectly` and `Constructor_WithUserAndSession_StoresReferences`
- State management with `SetState_AddsStateToStatesDictionary`, `SetState_OverwritesExistingState`, and `GetState_*` methods
- Error handling with `AddError_AddsErrorToErrorsList`, `AddError_WithNullError_DoesNotAdd`, and `AddError_WithEmptyError_DoesNotAdd`
- Validation logic with `Validate_*` methods for various validation scenarios
- Processing control with `StopProcessing_SetsIsStoppedToTrue`
- Lifecycle tracking with `GetDuration_ReturnsTimeSpanSinceCreation`

**Example usage:**

```csharp
using FluentAssertions;
using TelegramBotFramework.Models;
using Xunit;

// Create a new execution context with default values
var context = new ExecutionContext();

// Context is automatically initialized with:
// - ContextId: unique identifier for the execution context
// - CreatedAt: timestamp when context was created
// - IsValid: true when no errors are present
// - Errors: empty list for collecting validation errors
// - States: empty dictionary for storing execution state

context.ContextId.Should().NotBeEmpty();
context.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
context.IsValid.Should().BeTrue();
context.Errors.Should().BeEmpty();
context.States.Should().BeEmpty();

// Set execution state for command processing
context.SetState("current_command", "/start");
context.SetState("user_language", "en");
context.SetState("attempt_count", 0);

// Retrieve execution state
string? currentCommand = context.GetState<string>("current_command");
int attemptCount = context.GetState<int>("attempt_count");

// Add validation errors
context.AddError("UserId is required");
context.AddError("ChatId is required");

// Check if context is valid
bool isValid = context.IsValid;
bool hasErrors = context.Errors.Any();

// Stop processing if validation fails
if (!context.IsValid)
{
    context.StopProcessing();
}

// Track execution duration
var duration = context.GetDuration();
Console.WriteLine($"Execution took: {duration.TotalMilliseconds}ms");
```

## CommandServiceTests

The `CommandServiceTests` class provides unit tests for the `CommandService` class, verifying command retrieval, execution, rate limiting, and permission validation logic. It uses Moq for mocking dependencies and FluentAssertions for readable test assertions, ensuring that command handling behaves correctly under various scenarios including disabled commands, insufficient permissions, and rate limiting constraints.

**Key test scenarios:**
- Command retrieval with `GetCommandAsync` for both existing and non-existent commands
- Command execution validation with `ExecuteCommandAsync` for disabled commands and permission checks
- Rate limiting verification with `IsCommandRateLimitedAsync` for both exceeded and within-limit scenarios
- Command registration validation with `RegisterCommandAsync` for invalid commands

**Example usage:**

```csharp
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TelegramBotFramework.Models;
using TelegramBotFramework.Repositories;
using TelegramBotFramework.Services;
using TelegramBotFramework.Tests;

// Setup test dependencies
var mockRepository = new Mock<ICommandRepository>();
var mockUserService = new Mock<IUserService>();
var mockLogger = new Mock<ILogger<CommandService>>();
var commandService = new CommandService(mockRepository.Object, mockUserService.Object, mockLogger.Object);

// Test 1: GetCommandAsync returns command when it exists
var existingCommand = new Models.Command { Name = "/test" };
mockRepository
    .Setup(r => r.GetByNameAsync("/test", It.IsAny<CancellationToken>()))
    .ReturnsAsync(existingCommand);

var result = await commandService.GetCommandAsync("test");
result.Should().NotBeNull();
result!.Name.Should().Be("/test");

// Test 2: GetCommandAsync returns null when command doesn't exist
Models.Command? nullResult = await commandService.GetCommandAsync("unknown");
nullResult.Should().BeNull();

// Test 3: ExecuteCommandAsync validates disabled commands
var disabledCommand = new Models.Command { Name = "/disabled", IsEnabled = false };
var context = new Models.ExecutionContext { Command = disabledCommand, UserId = 1, ChatId = 1 };
var executionResult = await commandService.ExecuteCommandAsync(context);
executionResult.Errors.Should().Contain(e => e.Contains("is disabled"));

// Test 4: ExecuteCommandAsync validates permissions
var adminCommand = new Models.Command { Name = "/admin", RequiresAdmin = true };
var user = new Models.BotUser { Role = Models.UserRole.User };
var permissionContext = new Models.ExecutionContext { Command = adminCommand, User = user, UserId = 1, ChatId = 1 };
var permissionResult = await commandService.ExecuteCommandAsync(permissionContext);
permissionResult.Errors.Should().Contain(e => e.Contains("Insufficient permissions"));

// Test 5: IsCommandRateLimitedAsync checks rate limits
var rateLimitedCommand = new Models.Command { Name = "/test", RateLimitPerMinute = 1 };
mockRepository
    .Setup(r => r.GetByNameAsync("/test", It.IsAny<CancellationToken>()))
    .ReturnsAsync(rateLimitedCommand);

bool firstRequestAllowed = await commandService.IsCommandRateLimitedAsync(1L, "/test");
bool secondRequestLimited = await commandService.IsCommandRateLimitedAsync(1L, "/test");
secondRequestLimited.Should().BeTrue();
```

## BotUser

The `BotUser` class represents a Telegram user interacting with the bot. It stores user profile information, activity statistics, authentication status, and custom metadata. The class provides methods for user validation, activity tracking, and metadata management, making it ideal for implementing user sessions, role-based access control, and personalized bot experiences.

## BotConfiguration

The `BotConfiguration` class represents the central configuration object for the Telegram Bot Framework. It encapsulates all essential settings including bot credentials, database connections, session management, logging configuration, rate limiting, webhook settings, and custom application-specific parameters. This class serves as the primary configuration mechanism when initializing the framework.

**Example usage**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Models;

// Create and configure the bot configuration
var configuration = new BotConfiguration
{
    BotToken = "123456789:ABCDEF...",
    BotUsername = "my_bot_username",
    OwnerId = 123456789,
    DatabaseConnectionString = "Server=localhost;Database=BotDb;User=sa;Password=your_password;",
    SessionTimeoutMinutes = 30,
    MessageProcessingTimeoutSeconds = 15,
    EnableLogging = true,
    LogLevel = LogLevel.Info,
    MaxConcurrentRequests = 20,
    EnableWebhook = true,
    WebhookUrl = "https://mybot.com/webhook",
    WebhookSecret = "your-secret-token",
    ApiKey = "your-api-key",
    EnableRateLimiting = true,
    RateLimitPerMinute = 60,
    LocalizationLanguage = "en",
    AdminIds = new List<long> { 111111111, 222222222 },
    CustomSettings = new Dictionary<string, string>
    {
        { "custom_feature_enabled", "true" },
        { "feature_timeout", "300" }
    }
};

// Validate the configuration
configuration.Validate();

// Check if a user is admin
bool isAdmin = configuration.IsAdmin(111111111);

// Get a custom setting value
string? customValue = configuration.GetCustomSetting("custom_feature_enabled");

// Register the configuration with the DI container
var services = new ServiceCollection();
services.AddTelegramBotFramework(configuration);

var serviceProvider = services.BuildServiceProvider();
```

**Key features:**
- User profile management with first/last name, username, and phone number
- Activity tracking with timestamps for last activity and message count
- Role-based access control with `UserRole` enum (User, Moderator, Admin, Owner)
- User status management with `UserStatus` enum (Active, Inactive, Banned, Suspended)
- Metadata storage for custom user properties and preferences
- Validation and display name generation

**Example usage**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Models;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
    BotToken = "your-bot-token",
    BotUsername = "your-bot-username"
});

var serviceProvider = services.BuildServiceProvider();

// Create a new bot user
var botUser = new BotUser
{
    TelegramId = 123456789,
    FirstName = "John",
    LastName = "Doe",
    Username = "johndoe",
    PhoneNumber = "+1234567890",
    Status = BotUser.UserStatus.Active,
    Role = BotUser.UserRole.Admin,
    IsPremium = true,
    IsBot = false,
    Metadata = new Dictionary<string, string>
    {
        { "preferred_language", "en" },
        { "timezone", "UTC-5" }
    }
};

// Validate user data
botUser.Validate();

// Get user's display name
string displayName = botUser.GetDisplayName();
Console.WriteLine($"User: {displayName}"); // "John Doe"

// Update user activity (automatically increments message count)
botUser.UpdateActivity();
Console.WriteLine($"Messages sent: {botUser.MessagesCount}"); // 1

// Set custom metadata
botUser.SetMetadata("last_command", "/admin");
botUser.SetMetadata("theme", "dark");

// Retrieve metadata
string? lastCommand = botUser.GetMetadata("last_command");
Console.WriteLine($"Last command: {lastCommand}"); // "/admin"

// Check user permissions
bool isAdmin = botUser.Role == BotUser.UserRole.Admin || botUser.Role == BotUser.UserRole.Administrator;
Console.WriteLine($"Is admin: {isAdmin}"); // true

// Check if user is premium
bool isPremium = botUser.IsPremium;
Console.WriteLine($"Is premium: {isPremium}"); // true

// Access user properties
Console.WriteLine($"User ID: {botUser.TelegramId}");
Console.WriteLine($"Username: @{botUser.Username}");
Console.WriteLine($"Status: {botUser.Status}");
Console.WriteLine($"Created: {botUser.CreatedAt}");
Console.WriteLine($"Last activity: {botUser.LastActivityAt}");
```

## Menu

The `Menu` class represents an interactive menu interface that can be rendered as inline or reply keyboard buttons in Telegram. Menus organize bot commands and navigation options into structured layouts, supporting dynamic button arrangements, variable substitution, and callback-based interactions. The class provides methods for menu validation, button management, and state tracking.

**Key features:**
- Menu identification with unique `Id` and `MenuId` properties
- Support for inline and reply keyboard menu types via `MenuType` enum
- Dynamic button arrangement with configurable `MaxButtonsPerRow`
- Variable substitution for dynamic menu content using `Variables` dictionary
- Menu navigation with `BackMenuId` for hierarchical structures
- Button management methods: `AddButton`, `RemoveButton`, `GetButton`
- Variable management methods: `SetVariable`, `GetVariable`
- Automatic layout arrangement via `GetArrangedButtons`
- Menu validation with `Validate()` method

**Example usage**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Models;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
  BotToken = "your-bot-token",
  BotUsername = "your-bot-username"
});

var serviceProvider = services.BuildServiceProvider();

// Create a main menu with inline buttons
var mainMenu = new Menu
{
  Id = "main_menu",
  Title = "Main Menu",
  Description = "Welcome to the bot! Choose an option below:",
  Type = MenuType.Inline,
  IsActive = true,
  DisplayOrder = 1,
  MaxButtonsPerRow = 2,
  BackMenuId = null, // No parent menu
  Variables = new Dictionary<string, string>
  {
    { "bot_name", "My Awesome Bot" },
    { "user_count", "1250" }
  }
};

// Add buttons to the menu
mainMenu.AddButton(new MenuButton
{
  Label = "📊 Dashboard",
  CallbackData = "/dashboard",
  Action = ButtonAction.Callback,
  DisplayOrder = 1,
  Icon = "📊"
});

mainMenu.AddButton(new MenuButton
{
  Label = "🔧 Settings",
  CallbackData = "/settings",
  Action = ButtonAction.Callback,
  DisplayOrder = 2,
  Icon = "⚙️"
});

mainMenu.AddButton(new MenuButton
{
  Label = "📈 Analytics",
  CallbackData = "/analytics",
  Action = ButtonAction.Callback,
  DisplayOrder = 3,
  Icon = "📈"
});

mainMenu.AddButton(new MenuButton
{
  Label = "🔗 Open Website",
  Url = "https://example.com",
  Action = ButtonAction.OpenUrl,
  DisplayOrder = 4
});

// Validate the menu structure
mainMenu.Validate();

// Get buttons arranged by rows (respecting MaxButtonsPerRow)
var arrangedButtons = mainMenu.GetArrangedButtons();
Console.WriteLine($"Menu has {arrangedButtons.Count} rows of buttons");

// Set a variable for dynamic content
mainMenu.SetVariable("user_count", "1320");

// Get a variable value
string? botName = mainMenu.GetVariable("bot_name");
Console.WriteLine($"Bot name: {botName}");

// Get a specific button
var settingsButton = mainMenu.GetButton("/settings");
if (settingsButton != null)
{
  Console.WriteLine($"Found button: {settingsButton.Label}");
}

// Remove a button by callback data
bool removed = mainMenu.RemoveButton("/old_command");
Console.WriteLine($"Button removed: {removed}");

// Access menu properties
Console.WriteLine($"Menu ID: {mainMenu.Id}");
Console.WriteLine($"Title: {mainMenu.Title}");
Console.WriteLine($"Type: {mainMenu.Type}");
Console.WriteLine($"Is Active: {mainMenu.IsActive}");
Console.WriteLine($"Created: {mainMenu.CreatedAt}");
Console.WriteLine($"Updated: {mainMenu.UpdatedAt}");
Console.WriteLine($"Max Buttons Per Row: {mainMenu.MaxButtonsPerRow}");
```

## InlineQuery

The `InlineQuery` class represents an inline query received from a Telegram user. It encapsulates the query text, user information, pagination state, and processing metadata, enabling bots to handle inline queries efficiently and track their lifecycle from reception to response.

**Example usage**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Models;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
    BotToken = "your-bot-token",
    BotUsername = "your-bot-username"
});

var serviceProvider = services.BuildServiceProvider();

// Create an inline query instance (typically received from Telegram API)
var inlineQuery = new InlineQuery
{
    QueryId = Guid.NewGuid().ToString(),
    UserId = 123456789,
    Query = "search query",
    Offset = "",
    Status = InlineQueryStatus.Pending,
    ReceivedAt = DateTime.UtcNow,
    Metadata = new Dictionary<string, object>
    {
        { "source", "user_input" },
        { "priority", 1 }
    }
};

// Validate the inline query
inlineQuery.Validate();

// Set additional metadata for processing context
inlineQuery.SetMetadata("processing_started", DateTime.UtcNow);

// Track processing state
inlineQuery.Status = InlineQueryStatus.Processing;

// After processing completes
inlineQuery.Status = InlineQueryStatus.Answered;
inlineQuery.AnsweredAt = DateTime.UtcNow;
Console.WriteLine($"Processing took: {inlineQuery.GetProcessingDurationMs()}ms");

// Access metadata
string? source = inlineQuery.GetMetadata("source") as string;
Console.WriteLine($"Query source: {source}"); // "user_input"

// Create a result for this inline query
var result = new InlineQueryResult
{
    Title = "Search Result",
    Description = "Description of the search result",
    Content = "This is the content that will be sent when the user selects this result",
    Type = InlineQueryResultType.Article,
    ThumbnailUrl = "https://example.com/thumbnail.jpg",
    CustomPayload = "search_result_123"
};

// Validate the result
result.Validate();

// Access inline query properties
Console.WriteLine($"Query ID: {inlineQuery.QueryId}");
Console.WriteLine($"User ID: {inlineQuery.UserId}");
Console.WriteLine($"Query: {inlineQuery.Query}");
Console.WriteLine($"Status: {inlineQuery.Status}");
Console.WriteLine($"Received at: {inlineQuery.ReceivedAt}");
Console.WriteLine($"Answered at: {inlineQuery.AnsweredAt}");
```

## IInlineQueryService

The `IInlineQueryService` interface handles Telegram inline queries with transparent result caching and page-based pagination. It processes inline queries by delegating result generation to a factory function on cache misses, then caches the complete result set for subsequent pages of the same query within a configurable time-to-live window. This service enables efficient handling of paginated inline query results while maintaining performance through intelligent caching.

**Example usage**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
    BotToken = "your-bot-token",
    BotUsername = "your-bot-username"
});

var serviceProvider = services.BuildServiceProvider();

// Resolve the inline query service
var inlineQueryService = serviceProvider.GetRequiredService<IInlineQueryService>();

// Create an inline query instance
var inlineQuery = new InlineQuery
{
    QueryId = Guid.NewGuid().ToString(),
    UserId = 123456789,
    Query = "search music",
    Offset = "",
    Status = InlineQueryStatus.Pending,
    ReceivedAt = DateTime.UtcNow
};

// Define a results factory that returns all matching results for the query
Func<InlineQuery, CancellationToken, Task<IList<InlineQueryResult>>> resultsFactory = 
    async (query, ct) =>
    {
        // In a real implementation, this would query your data source
        // For example: search music tracks, documents, or other content
        var results = new List<InlineQueryResult>();
        
        for (int i = 1; i <= 25; i++)
        {
            results.Add(new InlineQueryResult
            {
                Title = $"Result {i}",
                Description = $"Description for result {i}",
                Content = $"Content for result {i}",
                Type = InlineQueryResultType.Article,
                Id = i.ToString()
            });
        }
        
        return results;
    };

// Handle the inline query with pagination (default page size is 10)
var pagedResult = await inlineQueryService.HandleAsync(
    inlineQuery,
    resultsFactory,
    pageSize: 10
);

// Access the paginated results
Console.WriteLine($"Total results: {pagedResult.TotalCount}");
Console.WriteLine($"Page {pagedResult.PageNumber} with {pagedResult.Results.Count} results");
Console.WriteLine($"Next offset: {pagedResult.NextOffset}");

// Get results from cache for subsequent pages without invoking the factory
var cachedResult = await inlineQueryService.GetCachedAsync(
    "search music",
    pageNumber: 2
);

if (cachedResult != null)
{
    Console.WriteLine($"Retrieved page 2 from cache: {cachedResult.Results.Count} results");
}

// Record query telemetry for monitoring
await inlineQueryService.RecordQueryAsync(inlineQuery, pagedResult.TotalCount);

// Invalidate cache when data changes (e.g., new items added to your data source)
await inlineQueryService.InvalidateCacheAsync("search music");
```

## Message

The `Message` class represents a user message received by the bot. It encapsulates message metadata, content, attachments, and processing state, providing methods for tracking message lifecycle, managing attachments, and storing custom metadata for extended functionality.

## InMemoryUserRepository

The `InMemoryUserRepository` class provides an in-memory implementation of the `IUserRepository` interface for storing and managing user data. This repository stores `BotUser` entities in memory with thread-safe operations, making it ideal for development, testing, and lightweight production scenarios where persistence isn't required.

**Key features:**
- Thread-safe operations using `lock` synchronization
- Fast in-memory storage with O(1) average complexity for most operations
- Comprehensive user lookup methods (by ID, username, status, role, etc.)
- Pagination support for large user collections
- Search functionality across user properties
- Async API with cancellation support

**Example usage:**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Models;
using TelegramBotFramework.Repositories;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
    BotToken = "your-bot-token",
    BotUsername = "your-bot-username"
});

var serviceProvider = services.BuildServiceProvider();

// Resolve the in-memory user repository
var userRepository = serviceProvider.GetRequiredService<IUserRepository>()
    as InMemoryUserRepository;

// Create a new user
var newUser = new BotUser
{
    TelegramId = 123456789,
    FirstName = "John",
    LastName = "Doe",
    Username = "johndoe",
    PhoneNumber = "+1234567890",
    Status = UserStatus.Active,
    Role = UserRole.User,
    IsPremium = true,
    IsBot = false,
    Metadata = new Dictionary<string, string>
    {
        { "preferred_language", "en" },
        { "timezone", "UTC-5" }
    }
};

// Add the user to the repository
var createdUser = await userRepository.CreateAsync(newUser);
Console.WriteLine($"User created: {createdUser.TelegramId}");

// Retrieve a user by ID
var retrievedUser = await userRepository.GetByIdAsync(123456789);
if (retrievedUser != null)
{
    Console.WriteLine($"Found user: {retrievedUser.GetDisplayName()}");
}

// Get all users
var allUsers = await userRepository.GetAllAsync();
Console.WriteLine($"Total users: {allUsers.Count}");

// Update a user
retrievedUser!.Status = UserStatus.Inactive;
var updatedUser = await userRepository.UpdateAsync(retrievedUser);
Console.WriteLine($"User updated: {updatedUser.Status}");

// Search for users by name
var searchResults = await userRepository.SearchAsync("John");
Console.WriteLine($"Found {searchResults.Count} users matching 'John'");

// Get users by status
var activeUsers = await userRepository.GetByStatusAsync(UserStatus.Active);
Console.WriteLine($"Active users: {activeUsers.Count}");

// Get users by role
var adminUsers = await userRepository.GetByRoleAsync(UserRole.Admin);
Console.WriteLine($"Admin users: {adminUsers.Count}");

// Get paginated results
var page1 = await userRepository.GetPaginatedAsync(1, 10);
Console.WriteLine($"Page 1 users: {page1.Count}");

// Check if user exists
bool exists = await userRepository.ExistsAsync(123456789);
Console.WriteLine($"User exists: {exists}");

// Get user by Telegram ID
var userByTelegramId = await userRepository.GetByTelegramIdAsync(123456789);
Console.WriteLine($"User by Telegram ID: {userByTelegramId?.Username}");

// Get user by username
var userByUsername = await userRepository.GetByUsernameAsync("johndoe");
Console.WriteLine($"User by username: {userByUsername?.TelegramId}");

// Count users
int userCount = await userRepository.CountAsync();
Console.WriteLine($"Total user count: {userCount}");

// Delete a user
bool deleted = await userRepository.DeleteAsync(123456789);
Console.WriteLine($"User deleted: {deleted}");
```

## InMemoryMessageRepository

The `InMemoryMessageRepository` class provides an in-memory implementation of the `IMessageRepository` interface for storing and managing Telegram message data. This repository stores `Message` entities in memory with thread-safe operations, making it ideal for development, testing, and lightweight production scenarios where persistence isn't required.

**Key features:**
- Thread-safe operations using `lock` synchronization
- Fast in-memory storage with O(1) average complexity for most operations
- Comprehensive message lookup methods (by ID, user, chat, status, command, date range)
- Pagination support for large message collections
- Async API with cancellation support
- Automatic message ID generation

**Example usage:**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Models;
using TelegramBotFramework.Repositories;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
    BotToken = "your-bot-token",
    BotUsername = "your-bot-username"
});

var serviceProvider = services.BuildServiceProvider();

// Resolve the in-memory message repository
var messageRepository = serviceProvider.GetRequiredService<IMessageRepository>()
    as InMemoryMessageRepository;

// Create a new message
var newMessage = new Message
{
    UserId = 123456789,
    ChatId = 987654321,
    Content = "/start Welcome to the bot!",
    Type = MessageType.Text,
    Status = MessageStatus.Processed,
    CommandName = "/start",
    CreatedAt = DateTime.UtcNow,
    Metadata = new Dictionary<string, object>
    {
        { "user_language", "en" },
        { "message_source", "web" }
    }
};

// Add the message to the repository (automatically assigns MessageId)
var createdMessage = await messageRepository.CreateAsync(newMessage);
Console.WriteLine($"Message created with ID: {createdMessage.MessageId}");

// Retrieve a message by ID
var retrievedMessage = await messageRepository.GetByIdAsync(createdMessage.MessageId);
if (retrievedMessage != null)
{
    Console.WriteLine($"Found message: {retrievedMessage.Content}");
}

// Get all messages
var allMessages = await messageRepository.GetAllAsync();
Console.WriteLine($"Total messages: {allMessages.Count}");

// Get messages by user
var userMessages = await messageRepository.GetByUserIdAsync(123456789);
Console.WriteLine($"User has {userMessages.Count} messages");

// Get messages by chat
var chatMessages = await messageRepository.GetByChatIdAsync(987654321);
Console.WriteLine($"Chat has {chatMessages.Count} messages");

// Get messages by status
var processedMessages = await messageRepository.GetByStatusAsync(MessageStatus.Processed);
Console.WriteLine($"Processed messages: {processedMessages.Count}");

// Get messages by command
var startMessages = await messageRepository.GetByCommandAsync("/start");
Console.WriteLine($"Start command messages: {startMessages.Count}");

// Get messages within a date range
var recentMessages = await messageRepository.GetByDateRangeAsync(
    DateTime.UtcNow.AddDays(-1),
    DateTime.UtcNow
);
Console.WriteLine($"Messages from last 24 hours: {recentMessages.Count}");

// Get paginated results (latest messages first)
var page1 = await messageRepository.GetPaginatedAsync(1, 10);
Console.WriteLine($"Page 1 messages: {page1.Count}");

// Check if message exists
bool exists = await messageRepository.ExistsAsync(createdMessage.MessageId);
Console.WriteLine($"Message exists: {exists}");

// Count messages
int messageCount = await messageRepository.CountAsync();
Console.WriteLine($"Total message count: {messageCount}");

// Update a message
retrievedMessage!.Status = MessageStatus.Processed;
var updatedMessage = await messageRepository.UpdateAsync(retrievedMessage);
Console.WriteLine($"Message updated: {updatedMessage.Status}");

// Delete a message
bool deleted = await messageRepository.DeleteAsync(createdMessage.MessageId);
Console.WriteLine($"Message deleted: {deleted}");
```

## MessageService

The `MessageService` class provides centralized message processing, storage, and lifecycle management for Telegram bot applications. It handles incoming message processing, retrieval, status tracking, and archiving operations, enabling reliable message handling and monitoring capabilities.

**Key features:**
- Message processing and persistence via `ProcessIncomingMessageAsync`
- Message retrieval with `GetMessageAsync` and `GetUserMessagesAsync`
- Failed message management with `GetFailedMessagesAsync` and `MarkAsFailedAsync`
- Message status tracking and processing state management
- Unprocessed message counting via `GetUnprocessedMessageCountAsync`
- Automatic message archiving via `ArchiveOldMessagesAsync`

**Example usage**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
    BotToken = "your-bot-token",
    BotUsername = "your-bot-username"
});

var serviceProvider = services.BuildServiceProvider();

// Resolve the message service
var messageService = serviceProvider.GetRequiredService<MessageService>();

// Process an incoming user message
var incomingMessage = new Message
{
    MessageId = 42,
    UserId = 123456789,
    ChatId = 987654321,
    Content = "/start Hello world!",
    Type = MessageType.Text,
    CreatedAt = DateTime.UtcNow
};

var processedMessage = await messageService.ProcessIncomingMessageAsync(incomingMessage);
Console.WriteLine($"Message processed: {processedMessage.MessageId}, Status: {processedMessage.Status}");

// Retrieve a specific message by ID
var retrievedMessage = await messageService.GetMessageAsync(42);
if (retrievedMessage != null)
{
    Console.WriteLine($"Found message: {retrievedMessage.Content}");
}

// Get all messages from a specific user
var userMessages = await messageService.GetUserMessagesAsync(123456789, limit: 20);
Console.WriteLine($"User has {userMessages.Count} messages");

// Get failed messages for error handling
var failedMessages = await messageService.GetFailedMessagesAsync(limit: 50);
Console.WriteLine($"Found {failedMessages.Count} failed messages");

// Mark a message as processed after successful handling
bool markedProcessed = await messageService.MarkAsProcessedAsync(42);
Console.WriteLine($"Message marked as processed: {markedProcessed}");

// Mark a message as failed when processing encounters an error
bool markedFailed = await messageService.MarkAsFailedAsync(43, "Failed to parse command");
Console.WriteLine($"Message marked as failed: {markedFailed}");

// Check for unprocessed messages (useful for monitoring)
int unprocessedCount = await messageService.GetUnprocessedMessageCountAsync();
Console.WriteLine($"Unprocessed messages: {unprocessedCount}");

// Archive old messages to keep storage clean
await messageService.ArchiveOldMessagesAsync(daysOld: 30);
Console.WriteLine("Old messages archived");
```

## HttpErrorHandlingMiddleware

The `HttpErrorHandlingMiddleware` class handles HTTP errors by logging error details and returning a user-friendly error message. It captures error information including the error code, message, timestamp, request path, and trace identifier for debugging purposes.

**Example usage**

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TelegramBotFramework.Middleware;

// Setup middleware in your ASP.NET Core pipeline
app.UseMiddleware<HttpErrorHandlingMiddleware>();

// The middleware will automatically handle HTTP errors and return appropriate responses
```

**Example with custom error handling:**

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TelegramBotFramework.Middleware;

public class CustomErrorHandlingMiddleware : HttpErrorHandlingMiddleware
{
    private readonly ILogger<CustomErrorHandlingMiddleware> _logger;

    public CustomErrorHandlingMiddleware(RequestDelegate next, ILogger<CustomErrorHandlingMiddleware> logger)
        : base(next, logger)
    {
        _logger = logger;
    }

    protected override async Task HandleErrorAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Custom error handling for request {Path}", context.Request.Path);
        
        // You can access error details from the base class
        await base.HandleErrorAsync(context, exception);
    }
}

// Register in your pipeline
app.UseMiddleware<CustomErrorHandlingMiddleware>();
```

**Example usage**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Models;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
    BotToken = "your-bot-token",
    BotUsername = "your-bot-username"
});

var serviceProvider = services.BuildServiceProvider();

// Create a new message instance
var message = new Message
{
    MessageId = 42,
    UserId = 123456789,
    ChatId = 987654321,
    Content = "/start Hello world!",
    Type = MessageType.Text,
    CreatedAt = DateTime.UtcNow,
    IsEdited = false,
    Metadata = new Dictionary<string, object>
    {
        { "user_language", "en" },
        { "message_source", "web" }
    }
};

// Validate the message
message.Validate();

// Add attachments (e.g., photos, documents)
message.AddAttachment("https://example.com/image.jpg");
message.AddAttachment("https://example.com/document.pdf");

// Track processing state
message.Status = MessageStatus.Processing;

// After processing completes
message.MarkAsProcessed();
Console.WriteLine($"Processing took: {message.GetProcessingDurationMs()}ms");

// Set error metadata if processing failed
try
{
    // Process message...
}
catch (Exception ex)
{
    message.MarkAsFailed(ex.Message);
}

// Access metadata
string? language = message.GetMetadata("user_language") as string;
Console.WriteLine($"User language: {language}");

// Store additional metadata
message.SetMetadata("processed_by", "message_handler_v2");
message.SetMetadata("priority", 1);

// Check message properties
Console.WriteLine($"Message ID: {message.MessageId}");
Console.WriteLine($"User ID: {message.UserId}");
Console.WriteLine($"Chat ID: {message.ChatId}");
Console.WriteLine($"Content: {message.Content}");
Console.WriteLine($"Type: {message.Type}");
Console.WriteLine($"Status: {message.Status}");
Console.WriteLine($"Created: {message.CreatedAt}");
Console.WriteLine($"Processed: {message.ProcessedAt}");
Console.WriteLine($"Is Edited: {message.IsEdited}");
Console.WriteLine($"Attachments: {message.AttachmentUrls?.Count ?? 0}");
```

## Command

The `Command` class represents a bot command that can be executed by users. It defines the command's metadata, behavior, and execution constraints including name, description, access control, rate limiting, and parameter definitions. Commands are the primary mechanism for handling user input in Telegram bots built with this framework.

**Key features:**
- Command routing with role-based access control via `RequiresAdmin`
- Rate limiting support with configurable `RateLimitPerMinute`
- Parameter definitions for structured command arguments
- Command patterns with alias support via `GetCommandPatterns()`
- Execution tracking with `ExecutionCount` and `RecordExecution()`
- Validation and metadata via `Validate()`, `CreatedAt`, and `UpdatedAt`
- Command type classification via `Type` (Standard, Menu, Inline, Callback)

**Example usage**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Models;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
    BotToken = "your-bot-token",
    BotUsername = "your-bot-username"
});

var serviceProvider = services.BuildServiceProvider();

// Create a standard command
var startCommand = new Command
{
    Name = "/start",
    Description = "Starts the bot and shows welcome message",
    HandlerType = "BotCommandHandler",
    Type = CommandType.Standard,
    RequiresAdmin = false,
    IsEnabled = true,
    RateLimitPerMinute = 30,
    Parameters = new List<CommandParameter>
    {
        new CommandParameter
        {
            Name = "name",
            Type = "string",
            IsRequired = false,
            Description = "User's name for personalized greeting"
        }
    }
};

// Validate the command definition
startCommand.Validate();

// Check if a user can execute this command
bool canExecute = startCommand.CanExecuteBy(UserRole.User); // true
bool adminCanExecute = startCommand.CanExecuteBy(UserRole.Administrator); // true
bool restrictedCanExecute = startCommand.CanExecuteBy(UserRole.Restricted); // false

// Record command execution
startCommand.RecordExecution();
Console.WriteLine($"Command executed {startCommand.ExecutionCount} times");

// Get all command patterns (including alias if defined)
var patterns = startCommand.GetCommandPatterns();
foreach (var pattern in patterns)
{
    Console.WriteLine($"Command pattern: {pattern}");
}

// Check rate limiting
bool isRateLimited = startCommand.IsRateLimited(25); // false (under limit)
bool isOverLimit = startCommand.IsRateLimited(35); // true (over limit)

// Access metadata
Console.WriteLine($"Command created at: {startCommand.CreatedAt}");
Console.WriteLine($"Last updated: {startCommand.UpdatedAt}");
```

## CommandService

The `CommandService` class provides centralized command management for registering, retrieving, executing, and monitoring bot commands. It serves as the primary service for command lifecycle management, including rate limiting, permission checks, and execution tracking. The service integrates with the command repository and user service to enforce access control and rate limiting policies across all bot commands.

**Key features:**
- Command registration and unregistration via `RegisterCommandAsync` and `UnregisterCommandAsync`
- Command retrieval with `GetCommandAsync` and `GetAvailableCommandsAsync`
- Command execution with validation and error handling via `ExecuteCommandAsync`
- Role-based access control and rate limiting enforcement
- Execution tracking and statistics via `RecordCommandExecutionAsync` and `GetCommandExecutionCountAsync`
- In-memory rate limiting with configurable windows

**Example usage**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
  BotToken = "your-bot-token",
  BotUsername = "your-bot-username"
});

var serviceProvider = services.BuildServiceProvider();

// Resolve the command service
var commandService = serviceProvider.GetRequiredService<CommandService>();

// Register a new command
var newCommand = new Command
{
  Name = "/weather",
  Description = "Get weather information for a location",
  HandlerType = "WeatherCommandHandler",
  Type = CommandType.Standard,
  RequiresAdmin = false,
  IsEnabled = true,
  RateLimitPerMinute = 10,
  Parameters = new List<CommandParameter>
  {
    new CommandParameter
    {
      Name = "location",
      Type = "string",
      IsRequired = true,
      Description = "City or ZIP code for weather lookup"
    }
  }
};

await commandService.RegisterCommandAsync(newCommand);
Console.WriteLine($"Command registered: {newCommand.Name}");

// Check if a user can execute a command
bool canExecute = await commandService.CanUserExecuteCommandAsync(
  userId: 123456789,
  commandName: "/weather"
);
Console.WriteLine($"Can user execute command: {canExecute}");

// Check if a command is rate limited for a user
bool isRateLimited = await commandService.IsCommandRateLimitedAsync(
  userId: 123456789,
  commandName: "/weather"
);
Console.WriteLine($"Command is rate limited: {isRateLimited}");

// Get a specific command
var weatherCommand = await commandService.GetCommandAsync("/weather");
if (weatherCommand != null)
{
  Console.WriteLine($"Found command: {weatherCommand.Name}");
  Console.WriteLine($"Description: {weatherCommand.Description}");
  Console.WriteLine($"Execution count: {weatherCommand.ExecutionCount}");
}

// Get all available commands for a user role
var availableCommands = await commandService.GetAvailableCommandsAsync(UserRole.User);
Console.WriteLine($"Available commands: {availableCommands.Count}");

// Execute a command with proper context
var executionContext = new ExecutionContext
{
  UserId = 123456789,
  ChatId = 987654321,
  Command = weatherCommand,
  Arguments = new Dictionary<string, object> { { "location", "London" } }
};

executionContext = await commandService.ExecuteCommandAsync(executionContext);
if (executionContext.IsValid)
{
  Console.WriteLine("Command executed successfully!");
  Console.WriteLine($"Execution time: {executionContext.GetState<bool>("executed")}");
}
else
{
  Console.WriteLine("Command execution failed:");
  foreach (var error in executionContext.Errors ?? new List<string>())
  {
    Console.WriteLine($"- {error}");
  }
}

// Record command execution and get statistics
await commandService.RecordCommandExecutionAsync("/weather");
int executionCount = await commandService.GetCommandExecutionCountAsync("/weather");
Console.WriteLine($"Total executions: {executionCount}");

// Unregister a command when needed
bool unregistered = await commandService.UnregisterCommandAsync("/weather");
Console.WriteLine($"Command unregistered: {unregistered}");
```

## BackgroundTaskWorker

The `BackgroundTaskWorker` class provides a lightweight background task queue and execution engine for running long-running operations without blocking the main request processing pipeline. It manages concurrent task execution with configurable limits, tracks task lifecycle (queued, started, completed), and provides detailed statistics about the worker's performance.

The worker uses a queue-based approach with a configurable maximum number of concurrent tasks, automatically scaling task execution as slots become available. Each task runs in a separate background thread, allowing the main application to remain responsive while processing potentially time-consuming operations like file processing, API calls, or database operations.

**Key features:**
- Configurable maximum concurrent tasks (default: 4)
- Graceful start/stop with cancellation support
- Task lifecycle tracking with timestamps
- Detailed statistics via `GetStatistics()`
- Thread-safe task queue with semaphore synchronization
- Automatic error handling and logging
- Lightweight implementation with minimal overhead

**Example usage**

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TelegramBotFramework.BackgroundWorkers;

// Setup your services
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());

var serviceProvider = services.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<ILogger<BackgroundTaskWorker>>();

// Create a background task worker with 8 concurrent tasks
var taskWorker = new BackgroundTaskWorker(maxConcurrentTasks: 8, logger: logger);

// Start the worker
taskWorker.Start();

// Queue a background task (e.g., process a large file)
taskWorker.QueueTask(async cancellationToken =>
{
    // Simulate a long-running operation
    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
    
    Console.WriteLine("Background task completed!");
    
    // Perform additional work...
    for (int i = 0; i < 10; i++)
    {
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        Console.WriteLine($"Processing step {i + 1}/10");
    }
}, "File Processing Task");

// Queue another task with different work
taskWorker.QueueTask(async cancellationToken =>
{
    // Call external API
    await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
    Console.WriteLine("API call completed!");
}, "API Integration Task");

// Get current statistics
var stats = taskWorker.GetStatistics();
Console.WriteLine($"Queued: {stats.QueuedTaskCount}, Running: {stats.RunningTaskCount}, Max: {stats.MaxConcurrentTasks}");

// Later, when shutting down the application...
// await taskWorker.StopAsync(TimeSpan.FromSeconds(10));
```

## BotConfigurationTestsExtensions

Utility extensions used in the test suite to build and validate `BotConfiguration` objects fluently. They provide shortcuts for creating a baseline valid configuration and then tweaking individual settings such as owners, admins, webhook, rate‑limiting, session timeout, concurrency limits, logging, and localization.

## BotFrameworkException

Base exception class for all framework-specific errors. `BotFrameworkException` provides an `ErrorCode` property to categorize exceptions and includes constructors for both simple messages and detailed error scenarios with inner exceptions. This exception serves as the foundation for specialized exceptions like `CommandExecutionException`, `InsufficientPermissionException`, and `SessionException`.

**Example usage**

```csharp
using TelegramBotFramework.Exceptions;

// Basic exception with error code
var exception = new BotFrameworkException("Something went wrong", "GENERIC_ERROR");
Console.WriteLine(exception.ErrorCode); // "GENERIC_ERROR"

// Exception with inner exception
try
{
    // Some operation that might fail
}
catch (Exception ex)
{
    throw new BotFrameworkException("Failed to process command", "COMMAND_PROCESSING_FAILED", ex);
}

// Using specialized exception
throw new InsufficientPermissionException(123456789, "admin");
```

## StateManagementExampleExtensions

Provides extension methods for the `StateManagementExample` class that simplify state management operations, form validation, and survey data handling. These extensions offer convenient methods for validating registration forms and survey data, generating formatted summaries, and updating survey metrics asynchronously.

**Example usage**

```csharp
using TelegramBotFramework.Models;
using TelegramBotFramework.Tests; // Namespace where BotConfigurationTestsExtensions lives

// Start from a known‑good configuration and customize it for a specific test case
var config = BotConfigurationTestsExtensions.CreateValidConfiguration()
    .WithOwnerId(123456789)
    .WithAdminIds(new[] { 111111111, 222222222 })
    .WithCustomSettings(settings => {
        // Example of adjusting any custom property on the configuration
        settings.EnableLogging = true;
    })
    .WithWebhookEnabled()
    .WithRateLimitingDisabled()
    .WithSessionTimeout(TimeSpan.FromMinutes(20))
    .WithMaxConcurrentRequests(15)
    .WithLoggingDisabled()
    .WithLocalizationLanguage("en");

// Validate the configuration using the provided assertions
config.ShouldBeValid();
config.ShouldBeAdmin(123456789);
config.ShouldNotBeAdmin(333333333);
config.SessionTimeoutShouldBe(TimeSpan.FromMinutes(20));

// Example of expecting a validation failure
BotConfigurationTestsExtensions.ShouldThrowValidationException(() =>
    BotConfigurationTestsExtensions.CreateValidConfiguration()
        .WithOwnerId(0) // Invalid owner ID triggers validation error
);
```

This section demonstrates how the test helpers can be combined to create expressive, readable unit tests for configuration validation logic.

## IEventHandler

The `IEventHandler<TEvent>` interface defines the contract for event handlers that process specific event types in the Telegram Bot Framework. Event handlers receive events published through the `EventBus` and execute corresponding business logic. The framework provides a base implementation `EventHandlerBase<TEvent>` that includes common logging functionality and error handling.

Handlers are registered with the event bus and automatically invoked when their corresponding event type is published. This pattern enables clean separation of concerns and makes it easy to add new event-driven features to your bot.

**Example usage**

```csharp
using Microsoft.Extensions.Logging;
using TelegramBotFramework.Events;
using TelegramBotFramework.Integration;

// Define a custom event
public class UserRegisteredEvent : IEvent
{
    public Guid CorrelationId { get; } = Guid.NewGuid();
    public long UserId { get; set; }
    public string Username { get; set; }
}

// Implement a handler for the custom event
public class UserRegistrationHandler : EventHandlerBase<UserRegisteredEvent>
{
    private readonly ILogger<UserRegistrationHandler> _logger;

    public UserRegistrationHandler(ILogger<UserRegistrationHandler> logger) : base(logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(UserRegisteredEvent @event)
    {
        _logger.LogInformation("User {Username} (ID: {UserId}) registered successfully",
            @event.Username, @event.UserId);

        // Perform registration logic here
        await Task.CompletedTask;
    }
}

// Usage in your application
var services = new ServiceCollection();
services.AddLogging();
services.AddTelegramBotFramework(new BotConfiguration
{
    BotToken = "your-bot-token",
    BotUsername = "your-bot-username"
});

var serviceProvider = services.BuildServiceProvider();
var eventBus = serviceProvider.GetRequiredService<EventBus>();
var handler = serviceProvider.GetRequiredService<UserRegistrationHandler>();

// Subscribe the handler to events
eventBus.Subscribe<UserRegisteredEvent>(handler);

// Publish an event
var userEvent = new UserRegisteredEvent
{
    UserId = 123456789,
    Username = "newuser"
};

await eventBus.PublishAsync(userEvent);

// Get handler information
var handlerName = handler.GetHandlerName(); // "UserRegistrationHandler"
```

## EventBus

The `EventBus` class provides an in-process publish-subscribe event bus implementation that enables decoupled communication between components in your Telegram bot application. It allows you to define custom event types and register handlers that respond to those events, making it ideal for scenarios like state changes, notifications, or triggering background operations without tight coupling.

The event bus is thread-safe and supports concurrent subscription management and event publishing. You can query the number of subscribers for specific event types and retrieve all registered event types for diagnostic purposes.

**Example usage**

```csharp
using TelegramBotFramework.Events;
using TelegramBotFramework.Integration;

// Define a custom event
public class UserRegisteredEvent : IEvent
{
    public Guid CorrelationId { get; } = Guid.NewGuid();
    public long UserId { get; set; }
    public string Username { get; set; }
}

// Define an event handler
public class UserRegistrationHandler : IEventHandler<UserRegisteredEvent>
{
    private readonly ILogger<UserRegistrationHandler> _logger;
    
    public UserRegistrationHandler(ILogger<UserRegistrationHandler> logger)
    {
        _logger = logger;
    }
    
    public async Task HandleAsync(UserRegisteredEvent @event)
    {
        _logger.LogInformation("User {Username} (ID: {UserId}) registered successfully",
            @event.Username, @event.UserId);
        
        // Perform registration logic here
        await Task.CompletedTask;
    }
}

// Usage in your application
var eventBus = new EventBus();
var handler = new UserRegistrationHandler(logger);

// Subscribe the handler to events
eventBus.Subscribe<UserRegisteredEvent>(handler);

// Publish an event
var userEvent = new UserRegisteredEvent
{
    UserId = 123456789,
    Username = "newuser"
};

await eventBus.PublishAsync(userEvent);

// Check subscriber count
eventBus.GetSubscriberCount<UserRegisteredEvent>(); // Returns 1

// Get all registered event types
var registeredTypes = eventBus.GetRegisteredEventTypes();

// Unsubscribe when no longer needed
eventBus.Unsubscribe<UserRegisteredEvent>(handler);

// Clear all subscriptions
eventBus.Clear();
```

## WebhookOptions

The `WebhookOptions` class provides configuration settings for Telegram webhook mode. It defines the URL, secret token, connection limits, and update filtering options that control how your bot receives updates from Telegram via HTTPS webhooks.

**Key features:**
- HTTPS endpoint configuration for receiving Telegram updates
- Secret token validation for secure webhook endpoints
- Connection throttling with configurable maximum simultaneous connections
- Update type filtering to receive only specific update types
- Pending updates management

**Example usage**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Integration;

// Configure webhook options
services.Configure<WebhookOptions>(options =>
{
    options.Url = "https://yourdomain.com/api/webhook";
    options.SecretToken = "your-secret-token";
    options.MaxConnections = 40;
    options.AllowedUpdates = new[] { "message", "callback_query" };
    options.ListenPath = "/api/webhook/telegram";
    options.DropPendingUpdates = false;
});

// Register WebhookService as hosted service
services.AddHostedService<WebhookService>();
```

## ExternalApiIntegration

The `ExternalApiIntegration` class provides a robust wrapper for making HTTP requests to external APIs with built-in retry logic, timeout handling, and response parsing capabilities. It simplifies integration with third-party services by handling common concerns like retry policies, error logging, and JSON serialization.

**Key features:**
- Automatic retry with exponential backoff for transient failures
- Timeout and error handling with comprehensive logging
- Support for GET and POST requests with custom headers
- Built-in JSON response parsing and deserialization
- Configurable retry count for handling rate limits and service unavailability

**Example usage**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Integration;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
    BotToken = "your-bot-token",
    BotUsername = "your-bot-username"
});

var serviceProvider = services.BuildServiceProvider();
var externalApi = serviceProvider.GetRequiredService<ExternalApiIntegration>();

// Make a GET request to fetch data from an external API
var weatherData = await externalApi.GetAsync<WeatherResponse>(
    "https://api.weatherapi.com/v1/current.json?key=YOUR_API_KEY&q=London"
);

if (weatherData != null)
{
    Console.WriteLine($"Current temperature in London: {weatherData.Current.TempC}°C");
}

// Make a POST request to send data to an external API
var userData = new { name = "John Doe", email = "john@example.com" };
bool postSuccess = await externalApi.PostAsync(
    "https://api.example.com/users",
    userData,
    apiKey: "your-api-key"
);

if (postSuccess)
{
    Console.WriteLine("User data posted successfully");
}

// Make a request with custom headers
var headers = new Dictionary<string, string>
{
    { "X-API-Version", "2.0" },
    { "X-Request-ID", Guid.NewGuid().ToString() }
};

string? apiResponse = await externalApi.GetWithHeadersAsync(
    "https://api.example.com/v2/data",
    headers
);

if (apiResponse != null)
{
    // Parse the response manually if needed
    var parsedResponse = ExternalApiIntegration.ParseResponse<ApiResponse>(apiResponse);
    Console.WriteLine($"API response: {parsedResponse?.Status}");
}
```

## ExecutionContext

The `ExecutionContext` class represents the execution context for a command or operation within the Telegram Bot Framework. It encapsulates all relevant information about the current execution flow, including user and chat identifiers, session data, command context, message content, and state management. The context is passed through the middleware pipeline, allowing each middleware component to read, modify, or extend the execution environment.

**Key features:**
- Context identification with unique `ContextId`
- User and chat metadata (UserId, ChatId, BotUser, UserSession)
- Command and message context tracking
- Parameter and state management via `GetParameter`/`SetParameter` and `GetState`/`SetState`
- Pipeline control with `RespondAndStop` and `StopProcessing`
- Error tracking and validation
- Execution timing via `CreatedAt` and `GetDuration()`

**Example usage**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Integration;
using TelegramBotFramework.Models;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
    BotToken = "your-bot-token",
    BotUsername = "your-bot-username"
});

var serviceProvider = services.BuildServiceProvider();

// Resolve dependencies
var orchestrator = serviceProvider.GetRequiredService<IBotOrchestrator>();

// Process a user message to create an execution context
var userId = 123456789L;
var chatId = 987654321L;
var context = await orchestrator.ProcessUserMessageAsync(userId, chatId, "/start", "TestUser");

// Access context properties
Console.WriteLine($"Context ID: {context.ContextId}");
Console.WriteLine($"User ID: {context.UserId}");
Console.WriteLine($"Chat ID: {context.ChatId}");
Console.WriteLine($"Created at: {context.CreatedAt}");
Console.WriteLine($"Is valid: {context.IsValid}");

// Use parameter storage for custom data
context.SetParameter("request_id", Guid.NewGuid().ToString());
var requestId = context.GetParameter<string>("request_id");
Console.WriteLine($"Request ID: {requestId}");

// Use state management
context.SetState("conversation_step", "welcome");
var currentStep = context.GetState<string>("conversation_step");
Console.WriteLine($"Current step: {currentStep}");

// Check for errors
if (!context.IsValid)
{
    foreach (var error in context.Errors ?? new List<string>()) 
    {
        Console.WriteLine($"Error: {error}");
    }
}

// Get execution duration
var duration = context.GetDuration();
Console.WriteLine($"Execution took: {duration.TotalMilliseconds}ms");
```

## PollingStrategy

The `PollingStrategy` class implements a polling mechanism for fetching Telegram updates as an alternative to webhooks. It continuously requests updates from the Telegram API, tracks the last processed update ID, and provides status information about the polling process.

## FileConversationStateStore

The `FileConversationStateStore` class provides a file-system-backed implementation of `IConversationStateStore` that persists conversation states to JSON files on disk. Each user's active flow state is serialized to a dedicated file named `{userId}.json` in a configurable directory, enabling state persistence across application restarts and enabling state sharing between multiple bot instances that share the same storage directory.

This implementation is ideal for single-host deployments with low-to-medium traffic. For high-concurrency or multi-node scenarios, consider using a database-backed store instead.

**Key features:**
- Automatic directory creation on initialization
- Thread-safe file operations with `SemaphoreSlim` synchronization
- Automatic cleanup of corrupted state files
- Support for loading all active states at once for recovery scenarios
- Configurable JSON serialization with enum support

**Example usage**

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TelegramBotFramework.ConversationFlow;
using TelegramBotFramework.Integration;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
    BotToken = "your-bot-token",
    BotUsername = "your-bot-username"
});
services.AddLogging(builder => builder.AddConsole());

var serviceProvider = services.BuildServiceProvider();

// Configure conversation flow options to use file-based state store
services.Configure<ConversationFlowOptions>(options =>
{
    // Use file-based state store for conversation persistence
    options.StateStoreFactory = (loggerFactory) => 
    {
        var logger = loggerFactory.CreateLogger<FileConversationStateStore>();
        return new FileConversationStateStore(
            directory: "/var/lib/telegram-bot/states",
            logger: logger
        );
    };
    
    // Other conversation flow options...
    options.DefaultFlowTimeout = TimeSpan.FromMinutes(15);
    options.AutoResumeOnSessionRestore = true;
});

// Register conversation flow engine
services.AddConversationFlows();

var provider = services.BuildServiceProvider();

// Resolve the state store
var stateStore = provider.GetRequiredService<IConversationStateStore>()
    as FileConversationStateStore;

// Save a user's conversation state
var userState = new UserFlowState
{
    UserId = 123456789,
    FlowKey = "survey",
    CurrentStep = "question1",
    Status = FlowStateStatus.Active,
    CreatedAt = DateTime.UtcNow,
    LastUpdated = DateTime.UtcNow
};

await stateStore.SaveStateAsync(userState);

// Load a user's conversation state
var loadedState = await stateStore.LoadStateAsync(123456789);
if (loadedState != null)
{
    Console.WriteLine($"Loaded state for user {loadedState.UserId}: {loadedState.CurrentStep}");
}

// Load all active states (e.g., during application startup for recovery)
var activeStates = await stateStore.LoadAllActiveStatesAsync();
Console.WriteLine($"Found {activeStates.Count} active conversation states");

// Delete a user's state when conversation completes
await stateStore.DeleteStateAsync(123456789);
```

**Key features:**
- Continuous polling loop with configurable interval
- Graceful start/stop control
- Status monitoring with `PollingStatus`
- Event-based update handling via `OnUpdateReceived`

**Example usage**

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TelegramBotFramework.Integration;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
    BotToken = "your-bot-token",
    BotUsername = "your-bot-username"
});
services.AddLogging(builder => builder.AddConsole());

var serviceProvider = services.BuildServiceProvider();

// Resolve dependencies
var apiClient = serviceProvider.GetRequiredService<TelegramApiClient>();
var pollingStrategy = serviceProvider.GetRequiredService<PollingStrategy>();

// Subscribe to update events
pollingStrategy.OnUpdateReceived += async update =>
{
    Console.WriteLine($"Received update {update.UpdateId} of type {update.MessageType}");
    // Handle the update here
};

// Start polling with 2-second interval
pollingStrategy.Start(TimeSpan.FromSeconds(2));

// Get current status
var status = pollingStrategy.GetStatus();
Console.WriteLine($"Polling running: {status.IsRunning}");
Console.WriteLine($"Last update ID: {status.LastUpdateId}");
Console.WriteLine($"Last poll time: {status.LastPollTime}");

// Manually process an update (useful for testing)
var testUpdate = new TelegramUpdate { UpdateId = 123 };
await pollingStrategy.ProcessUpdateAsync(testUpdate);

// Stop polling gracefully
await pollingStrategy.StopAsync();
```

## ConversationFlowOptions

The `ConversationFlowOptions` class configures the behavior of the conversation flow engine, including timeouts, limits, eviction policies, and user notifications. It controls how conversation states are managed, restored, and cleaned up, enabling durable multi-step interactions with configurable timeouts and automatic session restoration.

## ConversationFlowEngine

The `ConversationFlowEngine` manages durable conversation flows that guide users through multi-step interactions with configurable timeouts, state persistence, and automatic session restoration. It handles flow registration, execution, state tracking, and cleanup, enabling complex workflows like surveys, registrations, and multi-step commands with minimal boilerplate.

**Key features:**
- Flow registration and discovery via `RegisterFlowAsync`/`GetFlowAsync`
- User flow state management with `StartFlowAsync`, `ProcessInputAsync`, `ResumeFlowAsync`
- Active flow tracking via `GetActiveFlowStateAsync`, `IsUserInFlowAsync`
- Flow state history and cleanup with `GetFlowHistoryAsync`, `CleanupExpiredFlowStatesAsync`
- Graceful flow termination via `AbortFlowAsync`

**Example usage**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.ConversationFlow;
using TelegramBotFramework.Integration;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
BotToken = "your-bot-token",
BotUsername = "your-bot-username"
});

// Configure conversation flow options
services.Configure<ConversationFlowOptions>(options =>
{
// Set default flow timeout to 10 minutes
options.DefaultFlowTimeout = TimeSpan.FromMinutes(10);

// Enable automatic session restoration
options.AutoResumeOnSessionRestore = true;

// Allow up to 5 historical flow states per user
options.MaxHistoryPerUser = 5;

// Customize timeout messages
options.FlowTimeoutMessage = "Your conversation timed out. Please start again.";
options.FlowAbandonedMessage = "Your current conversation was interrupted.";

// Notify users when their flow times out instead of silently discarding
options.TimeoutEvictionPolicy = FlowEvictionPolicy.NotifyUser;

// Set cleanup to run every 30 minutes
options.CleanupIntervalMinutes = 30;

// Customize abort behavior
options.AbortKeyword = "/cancel";
options.AbortAcknowledgementMessage = "Flow cancelled. Use /start to begin again.";
});

// Register conversation flow engine
services.AddConversationFlows();

var serviceProvider = services.BuildServiceProvider();

// Resolve conversation flow engine
var flowEngine = serviceProvider.GetRequiredService<ConversationFlowEngine>();

// Define a simple survey flow
var surveyFlow = new FlowDefinition
{
FlowId = "user_survey",
Name = "User Feedback Survey",
Description = "Collects user feedback through a multi-step survey",
InitialStepId = "welcome",
Steps = new List<FlowStep>
{
new FlowStep
{
StepId = "welcome",
Prompt = "Welcome to our feedback survey! Let's take 2 minutes to improve. Ready to start?",
InputType = FlowInputType.Confirmation,
IsTerminal = false,
DefaultNextStepId = "rating"
},
new FlowStep
{
StepId = "rating",
Prompt = "On a scale of 1-5, how satisfied are you with our service?",
InputType = FlowInputType.Number,
IsTerminal = false,
Validation = new FlowValidation
{
Min = 1,
Max = 5,
ErrorMessage = "Please enter a number between 1 and 5"
},
DefaultNextStepId = "comments"
},
new FlowStep
{
StepId = "comments",
Prompt = "What would you like us to improve? (Optional)",
InputType = FlowInputType.Text,
IsTerminal = true
}
},
Timeout = TimeSpan.FromMinutes(8),
AllowResume = true,
CompletionMenuId = "main_menu"
};

// Register the flow with the engine
await flowEngine.RegisterFlowAsync(surveyFlow);

// Later, when a user sends a message that should start the flow
var userId = 123456789L;
var chatId = 987654321L;

// Check if user is already in a flow
bool isInFlow = await flowEngine.IsUserInFlowAsync(userId);
if (!isInFlow)
{
// Start the survey flow for this user
var startResult = await flowEngine.StartFlowAsync(userId, chatId, "user_survey");
Console.WriteLine($"Flow started: {startResult.Status}");
}

// Process user input through the flow
var input = "4";
var processResult = await flowEngine.ProcessInputAsync(userId, chatId, input);
Console.WriteLine($"Step completed: {processResult.CurrentStepId}, Next: {processResult.NextStepId}");

// Get the user's current active flow state
var activeState = await flowEngine.GetActiveFlowStateAsync(userId);
if (activeState != null)
{
Console.WriteLine($"User {userId} is in flow '{activeState.FlowKey}' at step '{activeState.CurrentStepId}'");
}

// List all registered flows
var allFlows = await flowEngine.GetAllFlowsAsync();
Console.WriteLine($"Registered flows: {allFlows.Count}");

// Get a specific flow definition
var flowDefinition = await flowEngine.GetFlowAsync("user_survey");
if (flowDefinition != null)
{
Console.WriteLine($"Flow '{flowDefinition.Name}' has {flowDefinition.Steps.Count} steps");
}

// Abort the flow if user wants to cancel
await flowEngine.AbortFlowAsync(userId);

// Get flow history for a user
var history = await flowEngine.GetFlowHistoryAsync(userId);
Console.WriteLine($"User {userId} has {history.Count} historical flow states");

// Cleanup expired flow states (typically called periodically)
int expiredCount = await flowEngine.CleanupExpiredFlowStatesAsync();
Console.WriteLine($"Cleaned up {expiredCount} expired flow states");
```

## FlowDefinition

The `FlowDefinition` class represents a complete conversation flow definition in the framework. It defines the flow's identity, structure, behavior, and metadata, enabling the creation of multi-step conversations with input validation, transitions, and configurable timeouts. Each flow consists of a sequence of steps that guide users through a specific interaction pattern, such as surveys, registrations, or multi-step commands.

**Example usage**

```csharp
using TelegramBotFramework.ConversationFlow;

// Define a conversation flow for a user registration survey
var registrationFlow = new FlowDefinition
{
    FlowId = "user_registration",
    Name = "User Registration Survey",
    Description = "Multi-step registration process for new users",
    InitialStepId = "welcome",
    Steps = new List<FlowStep>
    {
        new FlowStep
        {
            StepId = "welcome",
            Prompt = "Welcome to our bot! Let's get you registered. What's your name?",
            InputType = FlowInputType.Text,
            IsTerminal = false,
            DefaultNextStepId = "email",
            QuickReplies = new List<string> { "Skip registration" }
        },
        new FlowStep
        {
            StepId = "email",
            Prompt = "Great! Now please enter your email address:",
            InputType = FlowInputType.Email,
            IsTerminal = false,
            VariableName = "user_email",
            Validation = new FlowValidation
            {
                Pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                ErrorMessage = "Please enter a valid email address"
            },
            DefaultNextStepId = "confirmation"
        },
        new FlowStep
        {
            StepId = "confirmation",
            Prompt = "Thank you! Here's what we have:\n\nName: {{user_name}}\nEmail: {{user_email}}\n\nIs this correct?",
            InputType = FlowInputType.Confirmation,
            IsTerminal = true,
            Transitions = new List<FlowTransition>
            {
                new FlowTransition { From = "yes", To = "complete" },
                new FlowTransition { From = "no", To = "welcome" }
            }
        },
        new FlowStep
        {
            StepId = "complete",
            Prompt = "Registration complete! Thank you for signing up.",
            InputType = FlowInputType.None,
            IsTerminal = true
        }
    },
    Timeout = TimeSpan.FromMinutes(5),
    AllowResume = true,
    CompletionMenuId = "main_menu",
    Metadata = new Dictionary<string, string>
    {
        { "category", "registration" },
        { "version", "1.0" }
    }
};

// Register the flow with the conversation engine
var flowEngine = serviceProvider.GetRequiredService<ConversationFlowEngine>();
flowEngine.DefineFlow(registrationFlow);
```

**Key features:**
- Configurable default flow timeout and cleanup intervals
- Automatic session restoration for interrupted conversations
- Flow eviction policies (silent discard, notify user, or reset to initial step)
- Customizable user messages for timeouts and cancellations
- Event publishing for flow lifecycle tracking

**Example usage**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.ConversationFlow;
using TelegramBotFramework.Integration;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
  BotToken = "your-bot-token",
  BotUsername = "your-bot-username"
});

// Configure conversation flow options
services.Configure<ConversationFlowOptions>(options =>
{
  // Set default flow timeout to 15 minutes
  options.DefaultFlowTimeout = TimeSpan.FromMinutes(15);
  
  // Enable automatic session restoration
  options.AutoResumeOnSessionRestore = true;
  
  // Allow up to 3 historical flow states per user
  options.MaxHistoryPerUser = 3;
  
  // Customize timeout messages
  options.FlowTimeoutMessage = "Your conversation timed out. Please start again.";
  options.FlowAbandonedMessage = "Your current conversation was interrupted.";
  
  // Notify users when their flow times out instead of silently discarding
  options.TimeoutEvictionPolicy = FlowEvictionPolicy.NotifyUser;
  
  // Set cleanup to run every 30 minutes
  options.CleanupIntervalMinutes = 30;
  
  // Customize abort behavior
  options.AbortKeyword = "/cancel";
  options.AbortAcknowledgementMessage = "Flow cancelled. Use /start to begin again.";
  
  // Enable flow events for tracking
  options.EnableFlowEvents = true;
});

// Register conversation flow engine
services.AddConversationFlows();

var serviceProvider = services.BuildServiceProvider();

// Resolve conversation flow engine
var flowEngine = serviceProvider.GetRequiredService<ConversationFlowEngine>();

// Define a conversation flow (typically in your bot commands)
// flowEngine.DefineFlow("survey", flow =>
// {
//   flow.AddStep("welcome", async (ctx, ct) => 
//   {
//     await ctx.SendTextMessageAsync("Welcome to the survey!");
//   })
//   .AddStep("question1", async (ctx, ct) => 
//   {
//     await ctx.SendTextMessageAsync("What's your name?");
//   })
//   // ... additional steps
//   .SetTimeout(TimeSpan.FromMinutes(10));
// });
```

## HttpClientFactory

The `HttpClientFactory` provides a centralized way to create and manage HTTP clients with consistent configuration for connection pooling, timeouts, and headers. It helps avoid the common pitfall of socket exhaustion while maintaining proper resource cleanup through connection lifecycle management.

**Key features:**
- Connection pooling with configurable timeouts and lifetimes
- Automatic decompression support (GZip/Deflate)
- Default headers for User-Agent and Accept
- Caching of created clients for reuse
- Support for custom headers and authentication
- Thread-safe disposal of all cached clients

**Example usage**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Integration;

// Setup your services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
    BotToken = "your-bot-token",
    BotUsername = "your-bot-username"
});

var serviceProvider = services.BuildServiceProvider();
var httpClientFactory = serviceProvider.GetRequiredService<HttpClientFactory>();

// Get a generic HTTP client for a specific base URL
var apiClient = httpClientFactory.GetClient("https://api.example.com", TimeSpan.FromSeconds(60));

// Make a request
var response = await apiClient.GetAsync("/endpoint");
var content = await response.Content.ReadAsStringAsync();

// Get a pre-configured Telegram API client (optimized for Telegram)
var telegramClient = httpClientFactory.GetTelegramClient();
var botInfo = await telegramClient.GetAsync("/bot123456789:ABC-DEF1234ghIkl-zyx57W2v1u123ew11/getMe");

// Get a client with custom headers
var clientWithHeaders = httpClientFactory.GetClientWithHeaders(
    "https://api.example.com",
    new Dictionary<string, string>
    {
        { "X-Custom-Header", "custom-value" },
        { "X-Request-ID", Guid.NewGuid().ToString() }
    }
);

// Get a client with authentication
var authenticatedClient = httpClientFactory.GetClientWithAuth(
    "https://api.example.com",
    "your-auth-token",
    "Bearer"
);

// Dispose all clients when done (typically handled by DI container)
httpClientFactory.Dispose();
```

## WebhookHandler

The `WebhookHandler` class processes incoming webhook updates from Telegram, validates their authenticity, and extracts structured data for further processing. It handles various update types including messages, callback queries, edited messages, and inline queries, providing a clean interface for webhook-based Telegram bot integration.

**Example usage**

```csharp
using Microsoft.Extensions.Logging;
using TelegramBotFramework.Integration;

// Create handler instance
var handler = new WebhookHandler(logger);

// Example webhook payload from Telegram (simplified)
string webhookJson = """
{
  "update_id": 123456789,
  "message": {
    "message_id": 42,
    "chat": { "id": 987654321 },
    "from": { "id": 111111111 },
    "date": 1234567890,
    "text": "/start Hello world"
  }
}
""";

// Process the webhook update
var update = await handler.ProcessUpdateAsync(webhookJson);

if (update != null)
{
    Console.WriteLine($"Update ID: {update.UpdateId}");
    Console.WriteLine($"Type: {update.MessageType}");
    Console.WriteLine($"Timestamp: {update.Timestamp}");
    
    if (update.Message != null)
    {
        Console.WriteLine($"Message ID: {update.Message.MessageId}");
        Console.WriteLine($"Chat ID: {update.Message.ChatId}");
        Console.WriteLine($"User ID: {update.Message.UserId}");
        Console.WriteLine($"Text: {update.Message.Text}");
    }
}

// Validate webhook request authenticity (when using secret token)
bool isValid = handler.ValidateWebhookRequest(
    webhookJson,
    signature: "sha256=...",
    secretKey: "your-webhook-secret"
);
```

## TelegramApiClient

The `TelegramApiClient` class provides a lightweight wrapper around the Telegram Bot API, enabling direct interaction with Telegram's API endpoints. It handles HTTP communication, authentication, error logging, and response parsing, offering a simple interface for sending messages, managing webhooks, and querying bot information.

This client is particularly useful for custom API interactions beyond the framework's built-in capabilities or when you need fine-grained control over Telegram API calls.

**Example usage**

```csharp
using Microsoft.Extensions.Logging;
using TelegramBotFramework.Integration;

// Initialize the client with your bot token
var apiClient = new TelegramApiClient("123456789:ABC-DEF1234ghIkl-zyx57W2v1u123ew11");

// Send a simple text message
bool messageSent = await apiClient.SendMessageAsync(chatId: 987654321, text: "Hello from Telegram API Client!");

// Send a message with inline keyboard buttons
bool messageWithButtonsSent = await apiClient.SendMessageWithButtonsAsync(
    chatId: 987654321,
    text: "Choose an option:",
    buttonLabels: new[]
    {
        new[] { "Option 1", "Option 2" },
        new[] { "Option 3" }
    }
);

// Edit an existing message
bool messageEdited = await apiClient.EditMessageAsync(
    chatId: 987654321,
    messageId: 42,
    newText: "Updated message text"
);

// Delete a message
bool messageDeleted = await apiClient.DeleteMessageAsync(chatId: 987654321, messageId: 42);

// Get information about the bot
string? botInfo = await apiClient.GetMeAsync();
Console.WriteLine(botInfo);

// Answer a callback query from an inline button
bool callbackAnswered = await apiClient.AnswerCallbackQueryAsync(
    callbackQueryId: "abc123-def456",
    notificationText: "Processing your request..."
);

// Configure webhook for receiving updates
bool webhookSet = await apiClient.SetWebhookAsync("https://yourdomain.com/webhook");

// Remove webhook to switch to polling mode
bool webhookRemoved = await apiClient.RemoveWebhookAsync();

// Check if the client is enabled (always true unless explicitly disabled)
bool isEnabled = apiClient.IsEnabled;

// Use the logging capability
apiClient.Log(LogLevel.Information, new EventId(1), "API client initialized", null, (state, ex) => state?.ToString() ?? "No state");
```

## EventPublisher

The `EventPublisher` class provides a convenient API for publishing standard framework events to the event bus. It offers strongly-typed methods for common scenarios like message handling, command execution, and state transitions, while supporting custom event types through a generic `PublishAsync<TEvent>` method. The publisher also enables correlation tracking across related events via the `WithCorrelationId` fluent method.

This class is designed to be used as a service injected into your components, providing a clean abstraction over direct event bus interactions.

**Example usage**

```csharp
using Microsoft.Extensions.DependencyInjection;
using TelegramBotFramework.Events;
using TelegramBotFramework.Integration;

// Setup services
var services = new ServiceCollection();
services.AddTelegramBotFramework(new BotConfiguration
{
    BotToken = "your-bot-token",
    BotUsername = "your-bot-username"
});

var serviceProvider = services.BuildServiceProvider();

// Resolve dependencies
var eventPublisher = serviceProvider.GetRequiredService<EventPublisher>();
var eventBus = serviceProvider.GetRequiredService<IEventBus>();

// Register example handlers
services.AddSingleton<LoggingMessageEventHandler>();
serviceProvider = services.BuildServiceProvider();
eventBus.Subscribe<MessageReceivedEvent>(serviceProvider.GetRequiredService<LoggingMessageEventHandler>());

// Publish standard events
await eventPublisher
    .WithCorrelationId(Guid.NewGuid().ToString())
    .PublishMessageReceivedAsync(chatId: 123456789, userId: 987654321, messageText: "Hello world");

await eventPublisher.PublishCommandExecutedAsync(
    commandName: "start",
    userId: 987654321,
    arguments: null,
    success: true
);

await eventPublisher.PublishBotStateChangedAsync(
    previousState: "idle",
    newState: "active",
    reason: "User initiated conversation"
);

// Publish custom events
var customEvent = new UserRegisteredEvent
{
    UserId = 987654321,
    Username = "newuser"
};

await eventPublisher.PublishAsync(customEvent);
```

## BotBenchmarks

The `BotBenchmarks` class provides performance benchmarks for key framework operations using [BenchmarkDotNet](https://benchmarkdotnet.org/). It measures the execution time and memory allocation of message processing, session retrieval, and session termination operations to help identify performance bottlenecks and optimize the framework's efficiency.

**Example usage**

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TelegramBotFramework.Configuration;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;

namespace MyBotBenchmarks;

[MemoryDiagnoser]
public class MyBotBenchmarks
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IBotOrchestrator _botOrchestrator;
    private readonly long _userId = 12345;
    private readonly long _chatId = 67890;

    public MyBotBenchmarks()
    {
        var services = new ServiceCollection();
        var config = new BotConfiguration
        {
            BotToken = "test-token",
            BotUsername = "test-bot"
        };

        services.AddTelegramBotFramework(config);
        services.AddLogging(builder => builder.AddFilter("TelegramBotFramework", LogLevel.None));

        _serviceProvider = services.BuildServiceProvider();
        _botOrchestrator = _serviceProvider.GetRequiredService<IBotOrchestrator>();
    }

    [IterationSetup]
    public void Setup()
    {
        // Ensure a session exists for benchmarking
        try
        {
            _botOrchestrator.GetUserSessionAsync(_userId).GetAwaiter().GetResult();
        }
        catch
        {
            _botOrchestrator.ProcessUserMessageAsync(_userId, _chatId, "/start", "TestUser").GetAwaiter().GetResult();
        }
    }

    [Benchmark]
    public async Task<ExecutionContext> ProcessMessageBenchmark()
    {
        return await _botOrchestrator.ProcessUserMessageAsync(_userId, _chatId, "/echo", "TestUser");
    }

    [Benchmark]
    public async Task<UserSession> GetUserSessionBenchmark()
    {
        return await _botOrchestrator.GetUserSessionAsync(_userId);
    }

    [Benchmark]
    public async Task<bool> EndUserSessionBenchmark()
    {
        return await _botOrchestrator.EndUserSessionAsync(_userId);
    }

    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<MyBotBenchmarks>();
    }
}
```
