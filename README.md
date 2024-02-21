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

## WebhookServiceExtensions

Provides extension methods for configuring and managing webhook services in Telegram bot applications. Includes methods for registering webhook services, checking registration status, and accessing webhook‑related options and statistics.

**Example usage**

```csharp
// Example: Configure webhook service (see WebhookOptions for details)
services.AddWebhookService(options => {
    options.Url = "https://mybot.com/webhook";
});
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
