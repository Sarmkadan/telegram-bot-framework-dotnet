# Architecture Overview

## System Design

The Telegram Bot Framework is built with a layered, modular architecture that promotes separation of concerns and extensibility.

### Layers

```
┌─────────────────────────────────────┐
│      API Layer (Controllers)         │
│  BotController, AdminController     │
└──────────────────┬──────────────────┘
                   │
┌──────────────────▼──────────────────┐
│    Middleware Pipeline               │
│  Logging, Auth, RateLimit, etc.     │
└──────────────────┬──────────────────┘
                   │
┌──────────────────▼──────────────────┐
│    Orchestration Layer (Services)    │
│  BotOrchestrator, MessageService    │
└──────────────────┬──────────────────┘
                   │
┌──────────────────▼──────────────────┐
│    Domain Logic Layer (Services)     │
│  CommandService, UserService, etc.  │
└──────────────────┬──────────────────┘
                   │
┌──────────────────▼──────────────────┐
│    Data Access Layer (Repositories) │
│  IRepository, InMemoryRepository    │
└─────────────────────────────────────┘
```

## Core Components

### Models (Domain Entities)

**BotUser**
- Represents a Telegram user
- Properties: TelegramId, FirstName, LastName, Username, Role, Status
- Roles: User, Moderator, Admin, Owner
- Status: Active, Inactive, Banned, Suspended

**Message**
- Represents a user message
- Properties: UserId, ChatId, Content, Type, Status
- Types: Text, Photo, Video, Audio, File, Command
- Status: Received, Processing, Processed, Failed

**Command**
- Represents a bot command (e.g., /start)
- Properties: Name, Description, Type, RequiresAdmin
- Parameters support: CommandParameter list
- Rate limiting: Per-minute limits

**Menu**
- Interactive keyboard interface
- Types: Inline, ReplyKeyboard, Custom
- Contains: MenuButton list
- Navigation support

**UserSession**
- Tracks user session state
- Properties: SessionId, UserId, ChatId, State
- Context: ContextData dictionary
- Menu navigation: CurrentMenuId tracking

### Services

**BotOrchestrator**
- Main coordinator service
- Routes incoming messages
- Manages service interaction
- Handles message lifecycle

**CommandService**
- Registers and executes commands
- Parameter validation
- Permission checking
- Rate limit enforcement

**UserService**
- User CRUD operations
- Role management (promote/demote)
- Ban/suspend functionality
- Profile updates

**MessageService**
- Message processing pipeline
- Status tracking
- Archive management
- Metadata handling

**SessionAndMenuService**
- Session creation/management
- Menu management
- Navigation state
- Context data storage

### Middleware Pipeline

```
Request
  │
  ▼
ErrorHandlingMiddleware (Exception catching)
  │
  ▼
LoggingMiddleware (Request/response tracking)
  │
  ▼
AuthenticationMiddleware (API key validation)
  │
  ▼
RateLimitingMiddleware (Traffic control)
  │
  ▼
RequestValidationMiddleware (Payload verification)
  │
  ▼
Endpoint Handler
  │
  ▼
Response
```

Each middleware is optional and can be:
- Configured independently
- Replaced with custom implementation
- Reordered based on requirements

### Caching Architecture

```
┌─────────────┐
│ ICacheProvider (Interface)
└──────┬──────┘
       │
       ├─────────────────┬──────────────────┐
       │                 │                  │
       ▼                 ▼                  ▼
 LocalCacheProvider  DistributedCacheProvider  Custom
  (In-Memory)       (Redis, Memcached)      Implementations
```

**LocalCacheProvider**
- In-process memory cache
- Built-in TTL expiration
- Thread-safe operations
- Suitable for single-instance deployments

**DistributedCacheProvider**
- Multi-instance cache
- Redis-compatible interface
- Shared state across instances
- Better for scaled deployments

### Event System

```
Event Publisher → EventBus → Event Handlers
                     ▲
                     │
                (Subscribe/Publish)
```

**Built-in Events**
- `MessageReceivedEvent` - User sends message
- `CommandExecutedEvent` - Command completes
- `BotStateChangedEvent` - State transition

Custom events can be published:
```csharp
eventBus.PublishAsync(new CustomEvent { ... });
eventBus.Subscribe<CustomEvent>(handler);
```

### Rate Limiting Strategies

**Token Bucket**
- Allows burst traffic up to capacity
- Smooth rate distribution
- Best for user-facing APIs
- Default strategy

**Sliding Window**
- Precise rolling window limiting
- No bursts allowed
- Strict rate control
- Best for resource protection

**Fixed Window**
- Counter resets at fixed intervals
- Simple implementation
- Can allow burst at boundaries
- Legacy option

## Data Flow

### Incoming Message Processing

```
Telegram User
     │
     ▼
WebhookHandler / Polling
     │
     ▼
BotController.ProcessMessage
     │
     ▼
Authentication Middleware (Verify token)
     │
     ▼
RateLimit Middleware (Check limit)
     │
     ▼
MessageService.ProcessIncomingMessage
     │
     ▼
EventBus.PublishMessageReceived
     │
     ├─→ Event Subscribers (Handlers)
     │
     ▼
CommandService.ExecuteCommand (if command)
     │
     ▼
MessageRepository.Store
     │
     ▼
Response to User
```

### Command Execution Flow

```
User sends "/start"
     │
     ▼
BotController
     │
     ▼
CommandService.ExecuteCommand
     │
     ├─ Check if command exists
     ├─ Verify user permissions
     ├─ Check rate limit
     ├─ Validate parameters
     │
     ▼
CommandHandler (Custom logic)
     │
     ├─ Execute command
     ├─ Update session/user state
     ├─ Publish CommandExecutedEvent
     │
     ▼
Send Response to User
```

## Database Design (Phase 2+)

**Users Table**
- TelegramId (PK/indexed)
- FirstName, LastName
- Username, PhoneNumber
- Role, Status
- Metadata (JSON)
- CreatedAt, UpdatedAt (timestamps)

**Messages Table**
- MessageId (PK)
- UserId (FK)
- ChatId (indexed)
- Content, Type
- Status (indexed)
- Metadata (JSON)
- CreatedAt, UpdatedAt

**Sessions Table**
- SessionId (PK)
- UserId (FK)
- ChatId (indexed)
- State, CurrentMenuId
- ContextData (JSON)
- ExpiresAt (indexed)
- CreatedAt, UpdatedAt

**Commands Table**
- CommandId (PK)
- Name (unique)
- Description, Type
- RequiresAdmin
- Parameters (JSON)
- RateLimitPerMinute
- IsEnabled

**Menus Table**
- MenuId (PK)
- Title, Description
- Type, MaxButtonsPerRow
- Buttons (JSON)
- IsActive
- CreatedAt, UpdatedAt

## Configuration Management

**appsettings.json** - Default configuration
**appsettings.Development.json** - Development overrides
**Environment Variables** - Runtime overrides
**Custom Providers** - IConfigurationProvider implementations

## Dependency Injection

The framework uses built-in .NET DI:

```csharp
builder.Services.AddTelegramBotFramework(config);

// Automatically registers:
- ICommandService → CommandService
- IUserService → UserService
- IMessageService → MessageService
- ISessionAndMenuService → SessionAndMenuService
- ICacheProvider → Configured provider
- IEventBus → EventBus (Singleton)
- TelegramApiClient → Direct registration
```

## Extensibility Points

### Custom Services
```csharp
builder.Services.AddScoped<ICustomAnalyticsService, CustomAnalyticsService>();
```

### Custom Middleware
```csharp
app.UseMiddleware<CustomMiddleware>();
```

### Custom Event Handlers
```csharp
eventBus.Subscribe<MessageReceivedEvent>(async evt => {
    await HandleCustomLogicAsync(evt);
});
```

### Custom Repositories
```csharp
builder.Services.AddScoped<IRepository, SqlServerRepository>();
```

## Error Handling

**Exception Hierarchy**
```
Exception
  └─ BotFrameworkException
      ├─ CommandExecutionException
      ├─ CommandNotFoundException
      ├─ InsufficientPermissionException
      ├─ SessionException
      ├─ UserException
      ├─ RateLimitExceededException
      └─ ConfigurationException
```

All exceptions are caught by ErrorHandlingMiddleware and returned as structured responses.

## Performance Considerations

**Caching**
- Cache frequently accessed users (1h TTL)
- Cache commands list (30m TTL)
- Cache menu definitions (1h TTL)

**Connection Pooling**
- HttpClientFactory manages pooled clients
- Reduces connection overhead

**Async/Await**
- All I/O operations are async
- No thread blocking
- Supports thousands of concurrent users

**Rate Limiting**
- Prevents abuse
- Protects resources
- Configurable per command/user

## Scaling Considerations

**Single Instance**
- LocalCacheProvider
- Polling updates
- Suitable for <1k users

**Multiple Instances**
- DistributedCacheProvider (Redis)
- Webhook updates
- Load balancer frontend
- Suitable for 1k-100k users

**Enterprise Scale**
- Database persistence (SQL Server/PostgreSQL)
- Message queue (RabbitMQ, Service Bus)
- Cache layer (Redis)
- CDN for assets
- Kubernetes orchestration
- Suitable for 100k+ users

## Security Architecture

**Authentication**
- Bearer token validation
- X-API-Key header support
- Webhook signature verification (HMAC-SHA256)

**Authorization**
- Role-based access control (RBAC)
- Per-command permission checks
- User status verification

**Input Validation**
- Content-type checking
- JSON schema validation
- Size limits
- Sanitization in formatters

**Data Protection**
- Password hashing (PBKDF2-SHA256)
- Encrypted sensitive fields
- Secure token generation

## Monitoring & Observability

**Logging**
- Structured logging via ILogger
- Correlation IDs for request tracing
- Multiple log levels (Debug, Info, Warning, Error)

**Metrics**
- Request count/latency
- Cache hit/miss rates
- Command execution times
- User session stats

**Health Checks**
- `/api/bot/health` endpoint
- Database connectivity
- Cache availability
- Message queue status

## References

- [Service Locator Pattern](https://en.wikipedia.org/wiki/Service_locator_pattern)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
- [Clean Code Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
