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
