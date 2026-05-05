[![Build](https://github.com/sarmkadan/telegram-bot-framework-dotnet/actions/workflows/build.yml/badge.svg)](https://github.com/sarmkadan/telegram-bot-framework-dotnet/actions/workflows/build.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

# Telegram Bot Framework for .NET

An opinionated, production-ready framework for building Telegram bots with C# and .NET 10. Provides built-in support for commands, menus, state management, middleware pipeline, and enterprise-grade features.

**Table of Contents**
- [Features](#features)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [Installation Guide](#installation-guide)
- [Usage Examples](#usage-examples)
- [API Reference](#api-reference)
- [Configuration Reference](#configuration-reference)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)

---

## Features

- **Command System**: Automatic command routing with parameter validation and permission checks
- **Interactive Menus**: Inline keyboards with nested navigation and callback handling
- **Session Management**: User session tracking with configurable timeout and state persistence
- **State Machine**: Built-in finite state machine for complex user flows and conversations
- **Middleware Pipeline**: Extensible pipeline for logging, authorization, rate limiting, and validation
- **User Management**: Role-based access control (User, Moderator, Admin, Owner) with ban/suspend functionality
- **Rate Limiting**: Multiple strategies (token bucket, sliding window) for per-user or per-command throttling
- **Message Processing**: Full message lifecycle tracking with status management
- **Caching Layer**: Pluggable cache providers (in-memory, distributed)
- **Event System**: Pub-Sub event bus for decoupled component communication
- **Background Workers**: Queue-based task execution and scheduled tasks
- **REST API**: Complete API for bot management, user interaction, and admin operations
- **Error Handling**: Global exception handling with structured error responses
- **Integration**: Built-in Telegram API client, webhook support, and polling strategies
- **Formatters**: Multi-format output (JSON, CSV, XML, Telegram markdown)

---

## Architecture

### System Overview

```
┌─────────────────────────────────────────────────────┐
│         Telegram User Interaction                   │
└────────────────────┬────────────────────────────────┘
                     │
                     ▼
        ┌────────────────────────┐
        │  BotController/        │
        │  WebhookHandler        │
        └────────┬───────────────┘
                 │
        ┌────────▼──────────────────────────────────┐
        │      Middleware Pipeline                  │
        ├────────────────────────────────────────────┤
        │  1. ErrorHandling (Exception catching)    │
        │  2. Logging (Request/Response tracing)    │
        │  3. Authentication (API key validation)   │
        │  4. RateLimiting (Traffic control)        │
        │  5. Validation (Payload verification)     │
        └────────┬──────────────────────────────────┘
                 │
        ┌────────▼──────────────────────────────────┐
        │      BotOrchestrator                      │
        │  (Main service coordinator)               │
        └────────┬──────────────────────────────────┘
                 │
      ┌──────────┼──────────┬──────────┬────────┐
      │          │          │          │        │
      ▼          ▼          ▼          ▼        ▼
┌─────────┐ ┌────────┐ ┌─────────┐ ┌──────┐ ┌──────┐
│Command  │ │Message │ │Session& │ │Event │ │Cache │
│Service  │ │Service │ │Menu     │ │Bus   │ │Layer │
│         │ │        │ │Service  │ │      │ │      │
└─────────┘ └────────┘ └─────────┘ └──────┘ └──────┘
      │          │          │          │        │
      └──────────┼──────────┼──────────┼────────┘
                 │
      ┌──────────▼──────────────┐
      │   Repositories          │
      ├───────────────────────────┤
      │ InMemoryRepository (v1)   │
      │ Database adapters (v2+)   │
      └──────────────────────────┘
      
```

### Directory Structure

```
telegram-bot-framework-dotnet/
├── src/TelegramBotFramework/
│   ├── Models/                    # Domain entities (User, Command, Menu, Message, etc)
│   ├── Services/                  # Business logic (Command, User, Message, Session)
│   ├── Repositories/              # Data access abstraction
│   ├── Controllers/               # REST API endpoints (Bot, Admin)
│   ├── Middleware/                # Request pipeline (Logging, Auth, RateLimit, etc)
│   ├── Configuration/             # DI setup and configuration
│   ├── Caching/                   # Cache abstraction (Local, Distributed)
│   ├── Events/                    # Event bus and handlers
│   ├── Integration/               # Telegram API, Webhooks, Polling
│   ├── Strategies/                # Rate limiting strategies
│   ├── Formatters/                # Output formatters (JSON, CSV, XML)
│   ├── BackgroundWorkers/         # Async task execution
│   ├── Utilities/                 # Extension methods and helpers
│   ├── Exceptions/                # Custom exception types
│   ├── Constants/                 # Shared constants
│   ├── Program.cs                 # Application entry point
│   └── TelegramBotFramework.csproj # Project file
├── examples/                      # Sample applications
├── docs/                          # Detailed documentation
├── tests/                         # Unit and integration tests (future)
├── Dockerfile                     # Container image definition
├── docker-compose.yml             # Multi-container orchestration
├── Makefile                       # Build automation
├── CHANGELOG.md                   # Version history
├── .editorconfig                  # Editor settings
├── .gitignore                     # Git ignore rules
├── README.md                      # This file
├── LICENSE                        # MIT license
└── CONTRIBUTING.md                # Contribution guidelines
```

---

## Getting Started

### Prerequisites

- **.NET 10 SDK** or later ([Download](https://dotnet.microsoft.com/download))
- **Telegram Bot Token** (obtain from [@BotFather](https://t.me/botfather) on Telegram)
- **Optional**: Docker for containerized deployment

### Installation Guide

#### Method 1: Clone from Repository

```bash
# Clone the repository
git clone https://github.com/Sarmkadan/telegram-bot-framework-dotnet.git
cd telegram-bot-framework-dotnet

# Restore NuGet dependencies
dotnet restore

# Build the project
dotnet build

# Run the project
cd src/TelegramBotFramework
dotnet run
```

#### Method 2: Create New Project from Template

```bash
# In the future, use the template:
dotnet new telegram-bot-template --name MyBot
cd MyBot
dotnet run
```

#### Method 3: Docker Deployment

```bash
# Build and run with Docker Compose
docker-compose up -d

# View logs
docker-compose logs -f telegram-bot

# Stop containers
docker-compose down
```

#### Method 4: Publish Release Build

```bash
# Build release version
dotnet publish -c Release -o ./publish

# Run from published artifacts
./publish/TelegramBotFramework
```

### Configuration

#### appsettings.json

Create or update `appsettings.json` in `src/TelegramBotFramework/`:

```json
{
  "BotConfiguration": {
    "BotToken": "YOUR_BOT_TOKEN_HERE",
    "BotUsername": "your_bot_username",
    "WebhookUrl": "https://your-domain.com/api/bot/webhook",
    "UseWebhook": false
  },
  "SessionConfiguration": {
    "SessionTimeoutMinutes": 30,
    "MaxActiveSessions": 1000,
    "SessionCleanupIntervalMinutes": 5
  },
  "MessageConfiguration": {
    "ProcessingTimeoutSeconds": 10,
    "MaxMessageLength": 4096,
    "ArchiveMessagesOlderThanDays": 30
  },
  "RateLimitConfiguration": {
    "EnableRateLimiting": true,
    "DefaultLimitPerMinute": 30,
    "Strategy": "TokenBucket",
    "BurstCapacity": 5
  },
  "CacheConfiguration": {
    "Provider": "LocalCache",
    "DefaultExpirationMinutes": 60
  },
  "LoggingConfiguration": {
    "LogLevel": "Information",
    "EnableConsoleOutput": true,
    "EnableFileOutput": false,
    "LogFilePath": "logs/bot.log"
  }
}
```

#### Environment Variables

Override configuration with environment variables:

```bash
export TELEGRAM_BOT_TOKEN=your_token
export TELEGRAM_BOT_USERNAME=your_username
export SESSION_TIMEOUT_MINUTES=30
export RATE_LIMIT_PER_MINUTE=30
export ENABLE_LOGGING=true
export LOG_LEVEL=Information
```

#### Development vs Production

**Development** (`appsettings.Development.json`):
```json
{
  "LogLevel": "Debug",
  "EnableRateLimiting": false,
  "SessionTimeoutMinutes": 5
}
```

**Production**:
```json
{
  "LogLevel": "Warning",
  "EnableRateLimiting": true,
  "SessionTimeoutMinutes": 60,
  "MaxActiveSessions": 10000
}
```

---

## Usage Examples

### Example 1: Basic Command Handler

```csharp
// Register a simple /start command
var commandService = serviceProvider.GetRequiredService<ICommandService>();

var command = new Command
{
    Name = "/start",
    Description = "Start the bot",
    HandlerType = "StartCommandHandler",
    Type = CommandType.Standard,
    IsEnabled = true,
    RequiresAdmin = false
};

await commandService.RegisterCommandAsync(command);
```

### Example 2: Interactive Menu

```csharp
// Create a menu with buttons
var sessionService = serviceProvider.GetRequiredService<ISessionAndMenuService>();

var menu = new Menu
{
    Id = "main_menu",
    Title = "👋 Welcome to Bot",
    Description = "Choose an option:",
    Type = MenuType.Inline,
    IsActive = true,
    MaxButtonsPerRow = 2
};

menu.AddButton(new MenuButton 
{ 
    Label = "📋 Settings", 
    CallbackData = "settings",
    Action = ButtonAction.NavigateMenu 
});

menu.AddButton(new MenuButton 
{ 
    Label = "❓ Help", 
    CallbackData = "help",
    Action = ButtonAction.NavigateMenu 
});

menu.AddButton(new MenuButton 
{ 
    Label = "🚪 Exit", 
    CallbackData = "exit",
    Action = ButtonAction.CloseMenu 
});

await sessionService.CreateMenuAsync(menu);
```

### Example 3: User Session & State

```csharp
// Manage user sessions with state
var sessionService = serviceProvider.GetRequiredService<ISessionAndMenuService>();

var userId = 123456789L;
var chatId = 123456789L;

// Create session
var session = await sessionService.CreateSessionAsync(userId, chatId);

// Store context data
session.SetContextData("current_step", "input_name");
session.SetContextData("user_form_data", JsonConvert.SerializeObject(new { Name = "John" }));

// Update session
await sessionService.UpdateSessionAsync(session);

// Retrieve later
var existingSession = await sessionService.GetSessionAsync(userId);
var currentStep = existingSession?.GetContextData("current_step");
```

### Example 4: User Management & Roles

```csharp
// Manage users with roles and permissions
var userService = serviceProvider.GetRequiredService<IUserService>();

// Get or create user
var user = await userService.GetOrCreateUserAsync(telegramId: 123456789, "John", "Doe");

// Update user
user.Username = "johndoe";
user.PhoneNumber = "+1234567890";
await userService.UpdateUserAsync(user);

// Manage roles and status
await userService.PromoteToModeratorAsync(user.Id);
await userService.PromoteToAdminAsync(user.Id);
await userService.DemoteFromAdminAsync(user.Id);

// Ban/suspend users
await userService.BanUserAsync(user.Id, "Spam");
await userService.UnbanUserAsync(user.Id);
await userService.SuspendUserAsync(user.Id, TimeSpan.FromHours(24));
```

### Example 5: Message Processing Pipeline

```csharp
// Process incoming messages with full tracking
var messageService = serviceProvider.GetRequiredService<IMessageService>();

var message = new Message
{
    UserId = userId,
    ChatId = chatId,
    Content = "Hello bot!",
    Type = MessageType.Text,
    Metadata = new Dictionary<string, object>
    {
        { "command", "/help" },
        { "user_agent", "TelegramAndroid" }
    }
};

// Process the message
var processed = await messageService.ProcessIncomingMessageAsync(message);

// Check processing result
if (processed.Status == MessageStatus.Processed)
{
    logger.LogInformation("Message processed: {Content}", processed.Content);
}
else if (processed.Status == MessageStatus.Failed)
{
    logger.LogError("Message processing failed: {Error}", processed.Metadata?["error"]);
}
```

### Example 6: Rate Limiting

```csharp
// Configure rate limiting with different strategies
var rateLimitConfig = new RateLimitingConfiguration
{
    Strategy = RateLimitStrategy.TokenBucket,
    DefaultLimitPerMinute = 30,
    BurstCapacity = 5
};

// Rate limiting is enforced automatically by middleware
// Users hitting the limit will receive a 429 (Too Many Requests) response
```

### Example 7: Caching

```csharp
// Use caching for performance optimization
var cacheProvider = serviceProvider.GetRequiredService<ICacheProvider>();

// Get user with auto-cache
var user = await cacheProvider.GetOrCreateAsync(
    $"user:{userId}",
    async () => await userService.GetUserAsync(userId),
    TimeSpan.FromHours(1)
);

// Set value
await cacheProvider.SetAsync("session:123", sessionData, TimeSpan.FromMinutes(30));

// Get value
var data = await cacheProvider.GetAsync("session:123");

// Remove value
await cacheProvider.RemoveAsync("session:123");
```

### Example 8: Custom Middleware

```csharp
// Create custom middleware for additional processing
public class CustomAnalyticsMiddleware : IMiddleware
{
    private readonly ILogger<CustomAnalyticsMiddleware> _logger;
    private readonly IAnalyticsService _analytics;

    public CustomAnalyticsMiddleware(ILogger<CustomAnalyticsMiddleware> logger, 
        IAnalyticsService analytics)
    {
        _logger = logger;
        _analytics = analytics;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            await _analytics.TrackEventAsync(new AnalyticsEvent
            {
                EventType = "api_request",
                Path = context.Request.Path,
                Method = context.Request.Method,
                StatusCode = context.Response.StatusCode,
                Duration = stopwatch.ElapsedMilliseconds
            });
        }
    }
}
```

### Example 9: Event Publishing

```csharp
// Publish custom events for decoupled communication
var eventBus = serviceProvider.GetRequiredService<IEventBus>();
var correlationId = Guid.NewGuid().ToString();

// Subscribe to events
eventBus.Subscribe<MessageReceivedEvent>(async evt =>
{
    logger.LogInformation("Message received: {Content}", evt.MessageContent);
    await HandleMessageAsync(evt);
});

// Publish event
await eventBus.PublishAsync(new MessageReceivedEvent
{
    CorrelationId = correlationId,
    ChatId = chatId,
    UserId = userId,
    MessageContent = "User message"
});
```

### Example 10: Background Tasks

```csharp
// Execute long-running tasks without blocking
var backgroundWorker = serviceProvider.GetRequiredService<IBackgroundTaskWorker>();

// Queue a background task
await backgroundWorker.QueueTaskAsync(async () =>
{
    logger.LogInformation("Processing background task");
    
    // Perform long-running operation
    await Task.Delay(5000);
    
    // Update user stats, send emails, etc.
    await UpdateUserStatisticsAsync();
});

// Schedule recurring tasks
var scheduledManager = serviceProvider.GetRequiredService<IScheduledTaskManager>();
await scheduledManager.ScheduleRecurringAsync(
    "cleanup_sessions",
    async () => await sessionService.CloseExpiredSessionsAsync(),
    TimeSpan.FromMinutes(5)
);
```

---

## API Reference

### Bot Endpoints

#### POST /api/bot/message
Process an incoming message from Telegram.

**Request:**
```json
{
  "userId": 123456789,
  "chatId": 123456789,
  "content": "Hello bot!",
  "type": "text",
  "metadata": {
    "messageId": 42
  }
}
```

**Response:**
```json
{
  "success": true,
  "messageId": "msg-abc123",
  "status": "processed",
  "processedAt": "2026-05-04T10:30:00Z"
}
```

#### GET /api/bot/health
Health check endpoint.

**Response:**
```json
{
  "status": "healthy",
  "uptime": "2h 30m",
  "timestamp": "2026-05-04T10:30:00Z"
}
```

#### GET /api/bot/user/{userId}
Get user information.

**Response:**
```json
{
  "id": "usr-123",
  "telegramId": 123456789,
  "firstName": "John",
  "lastName": "Doe",
  "username": "johndoe",
  "role": "user",
  "status": "active",
  "createdAt": "2026-01-01T00:00:00Z",
  "updatedAt": "2026-05-04T10:30:00Z"
}
```

#### GET /api/bot/commands
List all available commands.

**Response:**
```json
{
  "commands": [
    {
      "name": "/start",
      "description": "Start the bot",
      "type": "standard",
      "requiresAdmin": false,
      "isEnabled": true,
      "rateLimitPerMinute": 30
    }
  ]
}
```

### Admin Endpoints

#### GET /api/admin/config
Get bot configuration (admin only).

#### GET /api/admin/statistics
Get bot statistics and metrics.

**Response:**
```json
{
  "totalUsers": 1250,
  "activeUsers": 340,
  "totalMessages": 45230,
  "uptime": "7d 2h 15m",
  "averageResponseTime": 245
}
```

#### POST /api/admin/promote-admin/{userId}
Promote user to admin.

#### POST /api/admin/ban-user/{userId}
Ban a user.

**Request:**
```json
{
  "reason": "Spam"
}
```

#### GET /api/admin/menus
List all menus.

---

## Configuration Reference

### BotConfiguration
- `BotToken` - Telegram bot token (required)
- `BotUsername` - Bot username (required)
- `WebhookUrl` - Webhook URL for updates
- `UseWebhook` - Enable webhook mode (default: false)

### SessionConfiguration
- `SessionTimeoutMinutes` - Session expiration time (default: 30)
- `MaxActiveSessions` - Maximum concurrent sessions (default: 1000)
- `SessionCleanupIntervalMinutes` - Cleanup frequency (default: 5)

### RateLimitConfiguration
- `EnableRateLimiting` - Enable rate limiting (default: true)
- `DefaultLimitPerMinute` - Default requests per minute (default: 30)
- `Strategy` - Strategy type: TokenBucket, SlidingWindow, FixedWindow
- `BurstCapacity` - Burst allowance (default: 5)

### CacheConfiguration
- `Provider` - Cache provider: LocalCache, DistributedCache
- `DefaultExpirationMinutes` - Default cache TTL (default: 60)

---

## Troubleshooting

### Bot not receiving messages

**Problem**: Webhook messages are not being received.

**Solution**:
1. Verify webhook URL is publicly accessible
2. Ensure HTTPS is configured
3. Check bot token is correct
4. Verify webhook certificate is valid
5. Check logs for incoming requests

```bash
# Check webhook status
curl -X POST https://api.telegram.org/bot<TOKEN>/getWebhookInfo
```

### Rate limiting too strict

**Problem**: Users are getting rate-limited too frequently.

**Solution**:
1. Adjust `DefaultLimitPerMinute` in configuration
2. Increase `BurstCapacity` for burst traffic
3. Switch to `TokenBucket` strategy for more lenient limits
4. Set per-command rate limits to allow important commands

### Memory usage growing

**Problem**: Memory usage increases over time.

**Solution**:
1. Configure `SessionTimeoutMinutes` appropriately
2. Enable automatic session cleanup
3. Reduce `SessionCleanupIntervalMinutes`
4. Switch from local cache to distributed cache (Redis)
5. Archive old messages

### Database connection errors

**Problem**: Cannot connect to database.

**Solution**:
1. Verify connection string in configuration
2. Check database server is running
3. Verify credentials are correct
4. Check firewall rules allow connection
5. Review logs for specific error messages

### High response times

**Problem**: API responses are slow.

**Solution**:
1. Enable caching with appropriate TTL
2. Use connection pooling
3. Reduce session cleanup frequency
4. Enable rate limiting to reduce load
5. Scale horizontally with multiple instances

---

## Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

### Development Setup

```bash
# Clone repository
git clone https://github.com/Sarmkadan/telegram-bot-framework-dotnet.git
cd telegram-bot-framework-dotnet

# Install dependencies
dotnet restore

# Run tests
dotnet test

# Build project
dotnet build

# Format code
dotnet format
```

### Coding Standards

- Use nullable reference types
- Follow C# naming conventions
- Add XML documentation for public members
- Write unit tests for new features
- Keep code simple and maintainable

---

## License

MIT License - See [LICENSE](LICENSE) file for details.

Copyright (c) 2026 Vladyslav Zaiets

---

## Support

For issues, questions, or suggestions:
- 📮 [Open an issue](https://github.com/Sarmkadan/telegram-bot-framework-dotnet/issues)
- 📧 Email: rutova2@gmail.com
- 🌐 Website: https://sarmkadan.com

---

**Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect**

[Portfolio](https://sarmkadan.com) | [GitHub](https://github.com/Sarmkadan) | [Telegram](https://t.me/sarmkadan)
