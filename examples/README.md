# Examples

Complete example applications demonstrating the Telegram Bot Framework capabilities.

Each example is a self-contained, runnable C# program that shows specific features in action.

## Examples Overview

### 1. **BasicBotExample.cs**

**What it teaches**: Fundamental bot operations and command handling.

**Key concepts**:
- Command registration and execution
- User creation and retrieval
- Message processing pipeline
- Basic logging

**Use when**: You're starting with the framework or want to understand the basics.

```csharp
// Register a simple command
var command = new Command
{
    Name = "/start",
    Description = "Start the bot",
    HandlerType = "StartCommandHandler",
    Type = CommandType.Standard
};
await commandService.RegisterCommandAsync(command);
```

**Related files**:
- `Models/Command.cs`
- `Services/CommandService.cs`
- `Services/UserService.cs`

---

### 2. **MenuNavigationExample.cs**

**What it teaches**: Interactive menu creation and navigation flows.

**Key concepts**:
- Menu creation with buttons
- Button actions and callbacks
- Menu navigation and history
- Session menu state management

**Use when**: Building conversational interfaces with menu-driven interactions.

```csharp
// Create a menu with buttons
var menu = new Menu
{
    Id = "main_menu",
    Title = "Main Menu",
    Type = MenuType.Inline,
    MaxButtonsPerRow = 2
};

menu.AddButton(new MenuButton
{
    Label = "Settings",
    CallbackData = "settings",
    Action = ButtonAction.NavigateMenu
});

await sessionService.CreateMenuAsync(menu);
```

**Related files**:
- `Models/Menu.cs`
- `Models/MenuButton.cs`
- `Services/SessionAndMenuService.cs`

---

### 3. **StateManagementExample.cs**

**What it teaches**: Managing complex user flows with form data and multi-step processes.

**Key concepts**:
- Session context data storage
- Multi-step form flows
- JSON serialization of complex data
- State tracking across interactions

**Use when**: Building workflows like user registration, surveys, or multi-step processes.

```csharp
// Store form data in session
var formData = new RegistrationForm { FirstName = "John" };
session.SetContextData("registration_form", JsonSerializer.Serialize(formData));

// Retrieve and deserialize later
var json = session.GetContextData("registration_form");
var form = JsonSerializer.Deserialize<RegistrationForm>(json);
```

**Related files**:
- `Models/UserSession.cs`
- `Services/SessionAndMenuService.cs`

---

### 4. **AdminOperationsExample.cs**

**What it teaches**: User management, role-based access control, and admin operations.

**Key concepts**:
- User creation and updates
- Role promotion and demotion
- User banning and suspension
- Admin user management

**Use when**: Building admin panels or managing user permissions and restrictions.

```csharp
// Promote user to admin
await userService.PromoteToAdminAsync(userId);

// Ban user with reason
await userService.BanUserAsync(userId, "Spamming content");

// Suspend temporarily
await userService.SuspendUserAsync(userId, TimeSpan.FromHours(24));
```

**Related files**:
- `Models/BotUser.cs`
- `Services/UserService.cs`
- `Models/UserRole.cs` and `UserStatus.cs`

---

### 5. **CachingExample.cs**

**What it teaches**: Performance optimization using caching strategies and patterns.

**Key concepts**:
- Cache-aside pattern (get or create)
- TTL and expiration management
- Bulk cache operations
- Cache invalidation patterns

**Use when**: Optimizing performance for frequently accessed data.

```csharp
// Get from cache or fetch from source
var user = await cacheProvider.GetOrCreateAsync(
    "user:123",
    async () => await userService.GetUserAsync(userId),
    TimeSpan.FromHours(1)
);

// Invalidate cache after update
await cacheProvider.RemoveAsync("user:123");
```

**Related files**:
- `Caching/ICacheProvider.cs`
- `Caching/LocalCacheProvider.cs`
- `Caching/DistributedCacheProvider.cs`

---

### 6. **EventDrivenExample.cs**

**What it teaches**: Event-driven architecture with pub-sub pattern for decoupled components.

**Key concepts**:
- Event publishing and subscribing
- Correlation IDs for tracing
- Custom event handlers
- Event-driven workflow orchestration

**Use when**: Building scalable, loosely-coupled systems with multiple processing flows.

```csharp
// Subscribe to message received events
eventBus.Subscribe<MessageReceivedEvent>(async evt =>
{
    _logger.LogInformation("Message from {UserId}: {Content}",
        evt.UserId, evt.MessageContent);
});

// Publish custom events
await eventBus.PublishAsync(new CustomEvent { ... });
```

**Related files**:
- `Events/IEventBus.cs`
- `Events/EventBus.cs`
- `Events/IEventHandler.cs`

---

### 7. **ExternalApiIntegrationExample.cs**

**What it teaches**: Calling third-party APIs, error handling, and response caching.

**Key concepts**:
- HTTP client factory usage
- API error handling
- Retry logic with exponential backoff
- Caching API responses
- Timeout management

**Use when**: Integrating with external services, weather APIs, currency converters, etc.

```csharp
// Call external API with retry logic
var httpClient = httpClientFactory.GetHttpClient();
var response = await httpClient.SendAsync(request, TimeSpan.FromSeconds(10));

// Parse and cache response
if (response.IsSuccessStatusCode)
{
    var content = await response.Content.ReadAsStringAsync();
    await cacheProvider.SetAsync(cacheKey, content, TimeSpan.FromMinutes(5));
}
```

**Related files**:
- `Integration/HttpClientFactory.cs`
- `Integration/ExternalApiIntegration.cs`
- `Integration/TelegramApiClient.cs`

---

## Running Examples

### Option 1: From Example Classes

In your `Program.cs` or main application:

```csharp
var services = new ServiceCollection();
services.AddTelegramBotFramework(botConfig);

var serviceProvider = services.BuildServiceProvider();

// Run specific example
var example = new BasicBotExample(serviceProvider);
await example.RunAsync();
```

### Option 2: Standalone Execution

Each example can be extracted and run independently in a console application.

### Option 3: Integration with Bot Logic

Adapt example patterns into your actual bot command handlers and event subscribers.

---

## Example Patterns & Best Practices

### Pattern 1: Service Injection

```csharp
public class MyExample
{
    private readonly IUserService _userService;
    private readonly IMessageService _messageService;

    public MyExample(IServiceProvider serviceProvider)
    {
        _userService = serviceProvider.GetRequiredService<IUserService>();
        _messageService = serviceProvider.GetRequiredService<IMessageService>();
    }
}
```

### Pattern 2: Error Handling

```csharp
try
{
    var result = await serviceMethod();
}
catch (NotFoundException ex)
{
    _logger.LogWarning("Resource not found: {Message}", ex.Message);
}
catch (UnauthorizedAccessException ex)
{
    _logger.LogWarning("Insufficient permissions: {Message}", ex.Message);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error occurred");
    throw;
}
```

### Pattern 3: Async/Await

```csharp
// Always use async for I/O operations
public async Task ProcessUserAsync(long userId)
{
    var user = await userService.GetUserAsync(userId);
    user.LastActivityAt = DateTime.UtcNow;
    await userService.UpdateUserAsync(user);
}
```

### Pattern 4: Logging

```csharp
_logger.LogInformation("User {UserId} executed command {Command}",
    userId, commandName);
_logger.LogError(ex, "Error processing message for user {UserId}",
    userId);
```

---

## Common Workflows

### Workflow 1: User Registration Flow

Uses: **StateManagementExample** + **MenuNavigationExample**

1. User sends `/start`
2. Bot presents registration menu
3. User inputs name, email, phone (multi-step form)
4. Bot stores data in session
5. On completion, create user record

### Workflow 2: Command with Admin Verification

Uses: **AdminOperationsExample** + **BasicBotExample**

1. User sends command
2. Check user role (User, Admin, Owner)
3. If not authorized, send "Insufficient permissions"
4. If authorized, execute command logic

### Workflow 3: Real-Time Data with Caching

Uses: **CachingExample** + **ExternalApiIntegrationExample**

1. User requests data (weather, crypto price)
2. Check cache first
3. If cache miss, call external API
4. Cache result for TTL
5. Serve user from cache on repeat requests

### Workflow 4: Event-Driven Analytics

Uses: **EventDrivenExample**

1. Subscribe to MessageReceivedEvent
2. Track message metrics
3. Subscribe to CommandExecutedEvent
4. Track command usage
5. Publish custom events for reporting

---

## Learning Path

**Beginner**: Start with these examples in order:
1. BasicBotExample
2. MenuNavigationExample
3. AdminOperationsExample

**Intermediate**: Next level examples:
4. StateManagementExample
5. CachingExample

**Advanced**: Complex patterns:
6. EventDrivenExample
7. ExternalApiIntegrationExample

---

## Troubleshooting Examples

### Example throws "ServiceNotRegisteredException"

**Solution**: Ensure DI is set up in Program.cs:
```csharp
services.AddTelegramBotFramework(botConfig);
```

### Example data is not persisting

**Solution**: Examples use in-memory storage. For persistence, implement a database repository.

### Cache example shows cache misses every time

**Solution**: LocalCacheProvider is in-process only. For distributed caching, use Redis.

---

## Further Learning

- Read [Architecture Guide](../docs/architecture.md) to understand system design
- Check [API Reference](../docs/api-reference.md) for endpoint details
- Review [Deployment Guide](../docs/deployment.md) for production patterns
- See [FAQ](../docs/faq.md) for common questions

---

## Contributing Examples

Want to add more examples? Follow these guidelines:

1. **File naming**: `DescriptiveNameExample.cs`
2. **Header**: Include author attribution
3. **Documentation**: XML comments on methods
4. **Length**: 50-200 lines (focused scope)
5. **Pattern**: Async/await, dependency injection, error handling
6. **Logging**: Meaningful log messages with context

Example template:
```csharp
public class FeatureExample
{
    public async Task RunAsync()
    {
        try
        {
            _logger.LogInformation("Starting FeatureExample");
            
            // Example code
            
            _logger.LogInformation("FeatureExample completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in FeatureExample");
            throw;
        }
    }
}
```

---

## Questions or Issues?

- 📧 Email: rutova2@gmail.com
- 💬 GitHub Issues: https://github.com/Sarmkadan/telegram-bot-framework-dotnet/issues
- 🌐 Website: https://sarmkadan.com

**Enjoy building with the Telegram Bot Framework!** 🚀
