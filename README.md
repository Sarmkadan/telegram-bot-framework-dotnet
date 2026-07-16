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

## BotUser

The `BotUser` class represents a Telegram user interacting with the bot. It stores user profile information, activity statistics, authentication status, and custom metadata. The class provides methods for user validation, activity tracking, and metadata management, making it ideal for implementing user sessions, role-based access control, and personalized bot experiences.

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
