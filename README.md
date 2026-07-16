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
