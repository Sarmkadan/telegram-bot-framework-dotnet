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

