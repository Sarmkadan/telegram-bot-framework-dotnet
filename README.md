# Telegram Bot Framework for .NET

An opinionated, production-ready framework for building Telegram bots with C# and .NET 10. Provides built-in support for commands, menus, state management, and middleware pipeline.

## Features

- **Command System**: Register and execute commands with automatic routing
- **Menu Navigation**: Interactive inline menus with callback handling
- **Session Management**: User sessions with automatic timeout and state tracking
- **State Machine**: Built-in state management for complex user flows
- **Middleware Pipeline**: Extensible middleware for logging, authorization, and rate limiting
- **User Management**: Role-based access control (User, Moderator, Admin, Owner)
- **Rate Limiting**: Per-user, per-command rate limiting
- **Message Processing**: Full message lifecycle tracking
- **In-Memory Storage**: Fast in-memory repositories for Phase 1 (database adapters coming soon)
- **REST API**: Complete REST API for bot management and user interaction

## Architecture

The framework is built on modern .NET principles:

```
TelegramBotFramework/
├── Models/              # Domain entities
├── Services/            # Business logic layer
├── Repositories/        # Data access layer
├── Middleware/          # Request pipeline
├── Controllers/         # REST API endpoints
├── Configuration/       # DI and setup
└── Constants/           # Shared constants
```

## Getting Started

### Prerequisites

- .NET 10 SDK or later
- Telegram Bot Token (from BotFather)

### Installation

```bash
git clone https://github.com/yourusername/telegram-bot-framework-dotnet.git
cd telegram-bot-framework-dotnet
dotnet restore
```

### Configuration

Create or update `appsettings.json`:

```json
{
  "botToken": "YOUR_BOT_TOKEN_HERE",
  "botUsername": "your_bot_username",
  "sessionTimeoutMinutes": 30,
  "messageProcessingTimeoutSeconds": 10,
  "maxConcurrentRequests": 10,
  "enableLogging": true,
  "enableRateLimiting": true,
  "rateLimitPerMinute": 30
}
```

Or use environment variables:

```bash
export TELEGRAM_BOT_TOKEN=your_token
export TELEGRAM_BOT_USERNAME=your_username
```

### Running

```bash
cd src/TelegramBotFramework
dotnet run
```

The API will be available at `https://localhost:5001`

## Core Concepts

### Commands

Commands are the primary way users interact with the bot:

```csharp
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

### Menus

Create interactive menus with buttons:

```csharp
var menu = new Menu
{
    Id = "main_menu",
    Title = "Main Menu",
    Type = MenuType.Inline,
    IsActive = true
};

var button = new MenuButton
{
    Label = "📋 Settings",
    CallbackData = "settings",
    Action = ButtonAction.NavigateMenu
};

menu.AddButton(button);
await menuService.CreateMenuAsync(menu);
```

### Sessions

User sessions track state across interactions:

```csharp
var session = await sessionService.CreateSessionAsync(userId, chatId);
session.SetContextData("current_step", "input_name");
await sessionService.UpdateSessionAsync(session);
```

### Users

Users have roles and statuses for permission management:

```csharp
var user = await userService.GetOrCreateUserAsync(telegramId, "John", "Doe");
await userService.PromoteToAdminAsync(userId);
await userService.BanUserAsync(userId);
```

### Messages

Track all incoming and processed messages:

```csharp
var message = new Message
{
    UserId = userId,
    ChatId = chatId,
    Content = "Hello bot!",
    Type = MessageType.Text
};

var processed = await messageService.ProcessIncomingMessageAsync(message);
```

## API Endpoints

### Bot Endpoints

- `POST /api/bot/message` - Process incoming message
- `GET /api/bot/health` - Health check
- `GET /api/bot/user/{userId}` - Get user info
- `GET /api/bot/session/{userId}` - Get active session
- `GET /api/bot/commands` - List available commands
- `GET /api/bot/menu/{menuId}` - Get menu

### Admin Endpoints

- `GET /api/admin/config` - Bot configuration
- `GET /api/admin/statistics` - Bot statistics
- `GET /api/admin/admins` - List administrators
- `POST /api/admin/promote-admin/{userId}` - Promote user
- `POST /api/admin/demote-admin/{userId}` - Demote admin
- `POST /api/admin/ban-user/{userId}` - Ban user
- `POST /api/admin/unban-user/{userId}` - Unban user
- `POST /api/admin/commands` - Register command
- `GET /api/admin/commands/{commandName}` - Get command
- `DELETE /api/admin/commands/{commandName}` - Delete command
- `GET /api/admin/menus` - List menus
- `POST /api/admin/sessions/close-expired` - Close expired sessions

## Middleware Pipeline

The framework includes built-in middleware:

1. **ErrorHandlingMiddleware** - Catches and logs errors
2. **LoggingMiddleware** - Logs request/response
3. **RateLimitMiddleware** - Enforces rate limits
4. **AuthorizationMiddleware** - Checks permissions

Custom middleware can extend the pipeline by implementing `IBotMiddleware`.

## Data Models

### BotUser

Represents a Telegram user:

```csharp
- TelegramId: long
- FirstName, LastName: string
- Username: string
- Status: UserStatus (Active, Inactive, Banned, Suspended)
- Role: UserRole (User, Moderator, Administrator, Owner)
- CreatedAt, UpdatedAt: DateTime
- Metadata: Dictionary<string, string>
```

### Command

Represents a bot command:

```csharp
- Name: string (e.g., "/start")
- Description: string
- HandlerType: string
- Type: CommandType
- RequiresAdmin: bool
- IsEnabled: bool
- RateLimitPerMinute: int?
- Parameters: List<CommandParameter>
```

### UserSession

Tracks user session state:

```csharp
- SessionId: string
- UserId, ChatId: long
- State: SessionState (Active, Idle, Suspended, Expired, Closed)
- CurrentMenuId: string
- ContextData: Dictionary<string, string>
- CommandHistory: List<string>
- ExpiresAt: DateTime?
```

### Menu

Interactive menu interface:

```csharp
- Id: string
- Title, Description: string
- Type: MenuType (Inline, ReplyKeyboard, Custom)
- Buttons: List<MenuButton>
- IsActive: bool
- MaxButtonsPerRow: int
```

### Message

Tracks message processing:

```csharp
- MessageId: long
- UserId, ChatId: long
- Content: string
- Type: MessageType
- Status: MessageStatus (Received, Processing, Processed, Failed, Archived)
- ProcessedAt: DateTime?
- Metadata: Dictionary<string, object>
```

## Exception Handling

Custom exception types for better error handling:

- `BotFrameworkException` - Base exception
- `CommandExecutionException` - Command execution failed
- `CommandNotFoundException` - Command not found
- `InsufficientPermissionException` - User lacks permission
- `SessionException` - Session operation failed
- `UserException` - User operation failed
- `RateLimitExceededException` - Rate limit exceeded
- `ConfigurationException` - Configuration error

## Logging

The framework uses `Microsoft.Extensions.Logging`:

```csharp
var logger = serviceProvider.GetRequiredService<ILogger<YourClass>>();
logger.LogInformation("User {UserId} executed {Command}", userId, commandName);
```

## Dependency Injection

All services are registered via DI:

```csharp
builder.Services.AddTelegramBotFramework(botConfig);

// Services are now available:
var userService = serviceProvider.GetRequiredService<IUserService>();
var commandService = serviceProvider.GetRequiredService<ICommandService>();
```

## Constants

Core constants are defined in `Constants/BotConstants.cs`:

- Command prefixes and delimiters
- Session and context keys
- Default timeout and rate limit values
- Error/success messages
- Cache key prefixes

## Future Roadmap

Phase 2:
- SQL Server / PostgreSQL adapters
- MongoDB support
- Webhook support
- Advanced state machine
- Command parameter validation
- Localization system
- Event publishing

Phase 3:
- Plugin system
- Custom middleware extensions
- Database migrations
- Performance optimization
- Distributed caching

## License

MIT License - Copyright (c) 2026 Vladyslav Zaiets

See LICENSE file for details.

## Author

**Vladyslav Zaiets**
- Website: https://sarmkadan.com
- Email: rutova2@gmail.com
- CTO & Software Architect

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues.

## Support

For issues, questions, or suggestions, please open an GitHub issue.
