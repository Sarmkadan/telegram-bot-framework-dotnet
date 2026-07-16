# Architecture

This document describes the actual structure of the codebase (`src/TelegramBotFramework`), the reasoning behind the main design decisions, and where to plug in your own code. Everything below maps to real types in the source - if a class is named here, it exists.

## Big Picture

One project, one assembly. The solution is a framework library that also ships a runnable ASP.NET Core host (`Program.cs`) so you can boot a bot with nothing but a token.

```
Telegram ──(webhook or long polling)──► Integration layer
                                            │ TelegramUpdate
                                            ▼
              Controllers (HTTP surface, webhook + admin/testing API)
                                            │
                                            ▼
                       BotOrchestrator (Services/BotOrchestrator.cs)
                                            │ ExecutionContext
                                            ▼
        Middleware pipeline (error handling → logging → authorization → rate limit)
                                            │
                                            ▼
     Domain services (User / Session / Command / Message / Menu services)
                                            │
                                            ▼
              Repositories (IRepository<T,TId>, in-memory implementations)
```

## Module Breakdown

| Folder | What lives there |
|---|---|
| `Models/` | Domain entities: `BotUser`, `Message`, `Command`, `Menu`, `UserSession`, `ExecutionContext`, plus `BotConfiguration` and options types. Entities carry their own `Validate()` methods. |
| `Services/` | `BotOrchestrator` (the coordinator) and the domain services: `UserService`, `CommandService`, `MessageService`, `SessionAndMenuService` (implements both `ISessionService` and `IMenuService`), `InlineQueryService`. |
| `Middleware/` | Two separate middleware families - see below. `IBotMiddleware` is the bot pipeline contract. |
| `Repositories/` | `IRepository<T, TId>` plus typed interfaces (`IUserRepository`, `ISessionRepository`, ...) and their `InMemory*` implementations. |
| `Integration/` | Telegram wire-level code: `TelegramApiClient` (+ `ITelegramApiClient`), `PollingStrategy`, `WebhookService` (an `IHostedService`), `WebhookHandler` (update parsing), `WebhookOptions`, `HttpClientFactory`. |
| `Controllers/` | ASP.NET Core controllers: `WebhookController` (receives Telegram POSTs, checks the secret token), `BotController` and `AdminController` (message/command/user management over HTTP). |
| `ConversationFlow/` | Multi-step dialog engine: `ConversationFlowEngine`, `IConversationStateStore` with in-memory and file-backed stores, `ConversationFlowMiddleware` to hook flows into the pipeline. |
| `Commands/` | `ICommandHandler` contract and the built-in `HelpCommandHandler`. |
| `Events/` | `EventBus` (in-process pub/sub, `IEventBus`), `EventPublisher`, `IEventHandler<T>`. |
| `Strategies/` | Rate limiting: `TokenBucketStrategy`, `SlidingWindowStrategy`, `FixedWindowStrategy`, and `InMemoryRateLimitingStrategy` (the one registered by default). |
| `Caching/` | `ICacheProvider`, `LocalCacheProvider` (in-memory + TTL), abstract `DistributedCacheProvider` base, `NoOpCacheProvider`. |
| `BackgroundWorkers/` | `BackgroundTaskWorker` (queue + worker loop) and `ScheduledTaskManager` (interval-based scheduling). |
| `Keyboard/` | `InlineKeyboardBuilder` - fluent builder producing Telegram inline keyboard markup. |
| `Formatters/` | `MessageFormatter` (Markdown/HTML escaping), `JsonFormatter`, `CsvFormatter`, `XmlFormatter`. |
| `Configuration/` | `DependencyInjectionSetup.AddTelegramBotFramework()`, `WebhookSetup.AddWebhookMode()`, `ConfigurationLoader` (env vars or JSON file). |
| `Exceptions/` | `BotFrameworkException` hierarchy (`CommandExecutionException`, `SessionException`, `RateLimitExceededException`, ...). |

## Key Design Decisions

### One `ExecutionContext` object through the whole pipeline

Every user interaction is folded into a `Models.ExecutionContext` (user, session, message, resolved command, parameters, errors, `IsValid`, `IsStopped`). The orchestrator builds it, the middleware chain transforms it, and it comes back out as the result.

*Rationale:* a single mutable context makes the middleware contract trivial (`ProcessAsync(context, next)`) and keeps cross-cutting state (errors, stop flag) in one place instead of threading a dozen parameters around.

*Trade-off:* it is a grab-bag object - everything can see everything. Fine at this size; if the pipeline grows, splitting read-only input from mutable result would be the first refactor.

### Custom bot middleware pipeline, not ASP.NET Core middleware

There are two middleware families and they are intentionally separate:

- **Bot pipeline** (`IBotMiddleware`): `BotErrorHandlingMiddleware`, `BotLoggingMiddleware`, `AuthorizationMiddleware`, `RateLimitingMiddleware`, `ConversationFlowMiddleware`. Runs inside `BotOrchestrator` over `ExecutionContext`.
- **HTTP middleware** (`HttpErrorHandlingMiddleware`, `HttpLoggingMiddleware`, `AuthenticationMiddleware`): plain ASP.NET Core-style middleware guarding the HTTP surface.

*Rationale:* bot updates can arrive from long polling too, where there is no HTTP request at all. Tying authorization/rate limiting to ASP.NET middleware would make polling mode a second-class citizen.

*Ordering:* `IBotMiddleware.Priority` decides the order - the orchestrator sorts descending, so **higher priority runs earlier**: logging (100) → authorization (30) → rate limiting (20) → error handling (10). A middleware can short-circuit by setting `IsStopped`; the pipeline in `BotOrchestrator.ExecuteMiddlewarePipelineAsync` then skips the rest.

### Repository pattern with in-memory defaults

All persistence goes through `IRepository<T, TId>` sub-interfaces. The only shipped implementations are in-memory (`ConcurrentDictionary`-based), registered as singletons in `DependencyInjectionSetup`.

*Rationale:* the framework stays dependency-free (no EF, no driver packages) and works out of the box. The interfaces are the seam where a real database goes.

*Trade-off:* state dies with the process and does not scale past one instance. `BotConfiguration.DatabaseConnectionString` exists but nothing consumes it yet - it is a placeholder for persistent repositories.

### Webhook and polling as siblings

`WebhookService` is an `IHostedService`: it registers the webhook URL (with optional secret token) on startup and removes it on shutdown; `WebhookController` receives the actual POSTs and validates `X-Telegram-Bot-Api-Secret-Token`. `PollingStrategy` is the alternative: a background loop calling `getUpdates` with offset tracking. Both normalize updates via `WebhookHandler` and raise the same `OnUpdateReceived` event.

*Rationale:* production wants webhooks; local development behind NAT wants polling. Sharing the update-parsing code keeps the two modes behaviorally identical downstream.

### Interfaces for the Telegram client

`TelegramApiClient` implements `ITelegramApiClient`; `PollingStrategy` and `WebhookService` depend on the interface. This exists purely so integration components can be tested with a fake client instead of hitting the real API.

### In-process event bus

`EventBus` is a minimal pub/sub (subscribe by event type, `PublishAsync` fans out). Conversation flows publish `FlowStartedEvent` / `FlowStepCompletedEvent` / `FlowCompletedEvent` / `FlowAbortedEvent` through it.

*Trade-off:* handlers run in-process and unordered; there is no retry or persistence. It is for decoupling within one process, not a message queue replacement.

### Conversation flows with pluggable state stores

`ConversationFlowEngine` executes declarative `FlowDefinition`s (steps, validation, branching). Active state lives behind `IConversationStateStore`: `InMemoryConversationStateStore` for tests/single instance, `FileConversationStateStore` for cheap durability across restarts (JSON files, write-behind with periodic flush).

*Rationale:* multi-step dialogs are the hardest part of bot UX; keeping the state store behind an interface means a Redis/DB store is an implementation away, without touching the engine.

## Data Flow (incoming text message)

1. Update arrives - `WebhookController` (webhook mode) or `PollingStrategy` (polling mode), parsed by `WebhookHandler` into a `TelegramUpdate`.
2. `BotOrchestrator.ProcessUserMessageAsync`:
   - `UserService.GetOrCreateUserAsync` - upsert the user.
   - `SessionService.GetActiveSessionAsync` / `CreateSessionAsync` - attach or open a session; activity timestamps recorded on both.
   - `MessageService.ProcessIncomingMessageAsync` - persist the message.
   - If the text starts with `/`, `CommandService.GetCommandAsync` resolves the command into the context.
3. The `ExecutionContext` runs through the bot middleware chain.
4. On success the message is marked processed; on errors `MessageService.MarkAsFailedAsync` records why.
5. Replies go out through `ITelegramApiClient` (`SendMessageAsync`, `SendMessageWithButtonsAsync` fed by `InlineKeyboardBuilder`).

## Extension Points

- **Custom command:** implement `ICommandHandler`, register it transient - `CommandService` picks up all registered handlers.
- **Custom middleware:** implement `IBotMiddleware`, choose a `Priority`, register as `IBotMiddleware`.
- **Persistence:** implement the typed repository interfaces (`IUserRepository` etc.) and register them before/instead of the in-memory ones.
- **Conversation state:** implement `IConversationStateStore` for Redis/DB-backed flow state.
- **Rate limiting:** implement `IRateLimitingStrategy` or swap in one of the three shipped strategies.
- **Caching:** implement `ICacheProvider`, or subclass `DistributedCacheProvider` for a distributed backend.
- **Events:** subscribe handlers on `IEventBus` for flow and domain events.

## DI Registration

`AddTelegramBotFramework(botConfig)` registers repositories, domain services, the orchestrator, the built-in help command handler, the in-memory rate limiting strategy and the four default bot middleware, plus console logging mapped from `BotConfiguration.LogLevel`. `AddWebhookMode(opts)` adds `TelegramApiClient` (exposed as `ITelegramApiClient`) and `WebhookService` as a hosted service. Configuration comes from environment variables first, `appsettings.json` as fallback (see `Program.cs` / `ConfigurationLoader`).

## Known Limitations

- All shipped repositories are in-memory; `DatabaseConnectionString` is accepted but unused. Restart = data loss.
- `EventBus`, caches and rate limiters are per-process - horizontal scaling needs distributed implementations that do not exist here yet.
- `TelegramApiClient` covers the subset of the Bot API the framework needs (send/edit/delete, callbacks, webhook management, getUpdates), not the full API surface.
- `HandleMenuButtonAsync` treats `OpenUrl` / `SwitchInline` actions as presentation-layer concerns and does nothing with them server-side.
- The bot error-handling middleware runs last in the chain (priority 10), so it converts exceptions thrown by domain code, not by the earlier middleware.
