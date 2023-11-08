# Changelog

All notable changes to the Telegram Bot Framework for .NET are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2025-09-14

### Added
- Add conversation flow engine with branching dialogs and context
- Docker support with multi-stage builds
- Health check endpoints (/health, /health/ready)
- Integration test suite with xUnit
- Migration guide from v1.x

### Changed
- Upgraded to .NET 10.0
- Modern C# features (records, primary constructors)
- Improved API consistency

### Fixed
- Various edge cases found through testing

## [1.0.0] - 2025-07-14

### Added
- Full middleware pipeline: ErrorHandling, Logging, Authentication, RateLimiting, RequestValidation
- REST API with BotController and AdminController
- Admin operations: promote/demote, ban, suspend, bulk user management
- Message lifecycle tracking (Received → Processing → Processed / Failed)
- Webhook signature validation (HMAC-SHA256)
- Request correlation IDs for distributed tracing
- Docker support with multi-stage builds and health checks
- Docker Compose orchestration with Redis and PostgreSQL service definitions
- CI/CD pipeline via GitHub Actions (build, CodeQL, NuGet publish)
- Makefile for build automation targets
- .editorconfig for consistent code formatting
- Comprehensive README, CONTRIBUTING, and example suite

### Changed
- Promoted from beta — all public APIs considered stable
- Hardened rate-limit precision: sliding window now uses sub-millisecond timestamps
- Improved structured logging with request/response correlation fields
- Finalised NuGet package metadata and README packaging

### Fixed
- Race condition in `SessionAndMenuService` expiration check under concurrent access
- `InMemoryRepository` iterator invalidation when removing expired entries
- Webhook handler not flushing response before closing connection

### Security
- Input validation enforced at all public API boundaries
- Request body size limits applied globally (4 MB default)
- Sensitive configuration values excluded from structured logs

---

## [0.9.0] - 2025-06-23

### Added
- `IRepository<T>` generic repository abstraction with `InMemoryRepository<T>` implementation
- `InMemoryMessageSessionRepository` specialisation for session storage
- `DependencyInjectionSetup` with `AddTelegramBotFramework` extension for one-call registration
- `BotConstants` centralising magic strings and numeric limits
- `appsettings.json` schema with all configuration sections documented

### Changed
- `BotOrchestrator` now resolves all service dependencies through DI rather than manual construction
- Reduced allocations in hot paths by reusing `StringBuilder` instances in `MessageFormatter`

### Fixed
- `LocalCacheProvider` TTL check used wall-clock time incorrectly after system sleep
- Missing null check in `CommandService.FindByNameAsync` when registry is empty

---

## [0.8.0] - 2025-06-02

### Added
- `AdminController` with endpoints: config, statistics, promote-admin, ban-user, menus
- `CsvFormatter` and `XmlFormatter` for multi-format export alongside existing `JsonFormatter`
- `MessageFormatter` for Telegram markdown rendering with entity escaping
- `ExternalApiIntegration` helper with retry and timeout policies
- `ValidationUtility` with common guard methods
- `ReflectionHelper` for attribute-based command discovery

### Changed
- `BotController` responses now include a `processedAt` timestamp field
- `ErrorHandlingMiddleware` returns RFC 7807 Problem Details format

### Fixed
- `JsonFormatter` did not serialise `DateTimeOffset` fields as ISO 8601
- `AdminController` statistics endpoint returned zero uptime on first request

---

## [0.7.0] - 2025-05-12

### Added
- `TelegramApiClient` wrapping the Telegram Bot API with typed request/response models
- `WebhookHandler` for processing incoming Telegram updates over HTTPS
- `PollingStrategy` as an alternative to webhooks for development environments
- `HttpClientFactory` with named clients and connection pool management
- `PollingStrategy` configuration: poll interval, timeout, backoff

### Changed
- `BotOrchestrator` delegates update routing to either `WebhookHandler` or `PollingStrategy`
- Improved error propagation from Telegram API calls to structured log entries

### Fixed
- Long-poll timeout value not applied to `HttpClient` deadline, causing premature cancellation
- Duplicate update processing when network retry delivered the same update ID twice

---

## [0.6.0] - 2025-04-21

### Added
- `IEventBus` / `EventBus` pub-sub implementation for decoupled component communication
- `EventPublisher` convenience wrapper for fire-and-forget publishing
- `IEventHandler<T>` interface for strongly-typed subscribers
- `BackgroundTaskWorker` backed by `System.Threading.Channels` for queue-based task execution
- `ScheduledTaskManager` for recurring tasks with configurable intervals
- `DateTimeExtensions` for common UTC/local time conversions
- `EnumHelper` for display-name attribute lookups

### Changed
- Session expiry now publishes a `SessionExpiredEvent` instead of logging only
- Background worker queue capacity configurable via `appsettings.json`

### Fixed
- Event subscribers not cleaned up on application shutdown, causing listener leak
- Scheduled tasks drifting over time due to `Task.Delay` accumulation — replaced with absolute next-fire calculation

---

## [0.5.0] - 2025-04-02

### Added
- `ICacheProvider` abstraction with `LocalCacheProvider` (ConcurrentDictionary + TTL) and `DistributedCacheProvider` (IDistributedCache wrapper)
- `RateLimitingMiddleware` enforcing per-user and per-command limits
- `RateLimitingStrategy` with TokenBucket, SlidingWindow, and FixedWindow algorithms
- `CryptoUtility` for HMAC-SHA256 and secure random generation
- `StringExtensions`: `ToSnakeCase`, `Truncate`, `IsNullOrWhiteSpace` guards
- `CollectionExtensions`: `Batch`, `IsNullOrEmpty`, `ToHashSet` helpers

### Changed
- `AuthenticationMiddleware` now validates API keys via `CryptoUtility.SecureCompare` to prevent timing attacks
- Rate limit exceeded response returns `Retry-After` header

### Fixed
- `LocalCacheProvider` did not evict expired entries on `GetAsync`, returning stale data
- Token bucket counter not reset correctly when the window rolled over

---

## [0.4.0] - 2025-03-14

### Added
- Middleware pipeline: `BotMiddleware` base, `LoggingMiddleware`, `ErrorHandlingMiddleware`, `AuthenticationMiddleware`, `RequestValidationMiddleware`
- `BotFrameworkException` hierarchy for typed error propagation
- `InlineQueryService` and `InlineQueryExtensions` for inline query routing
- `InlineQuery` model with result builder helpers
- `JsonUtility` thin wrapper over `System.Text.Json` with common options preset

### Changed
- All service methods now accept `CancellationToken` parameters
- `Message` model extended with `Metadata` dictionary for arbitrary key-value context

### Fixed
- Middleware short-circuit on validation failure was not halting subsequent middleware execution
- `LoggingMiddleware` logged request body twice on error paths

---

## [0.3.0] - 2025-02-24

### Added
- `UserSession` model with `ContextData` dictionary for multi-step conversation state
- `SessionAndMenuService` managing session lifecycle and menu state transitions
- `Menu` and `MenuButton` models with `ButtonAction` enum (NavigateMenu, CloseMenu, ExecuteCommand, OpenUrl)
- Automatic session expiry with configurable timeout
- `IUserService` interface and `UserService` implementation
- User roles: User, Moderator, Admin, Owner — with promotion/demotion and ban/suspend flows
- `BotUser` model with full audit fields (`CreatedAt`, `UpdatedAt`, `LastSeenAt`)

### Changed
- `CommandService` now validates command name format (must start with `/`)
- Repository operations use async/await throughout

### Fixed
- Menu button order was non-deterministic due to unsorted button collection
- Session creation for the same user ID from concurrent requests could produce duplicate sessions

---

## [0.2.0] - 2025-02-03

### Added
- `Command` model with `CommandType` enum (Standard, Admin, Inline, System)
- `CommandService` with register, resolve, enable/disable, and list operations
- `MessageService` for incoming message processing with status tracking (Received, Processing, Processed, Failed)
- `BotOrchestrator` as top-level coordinator wiring commands, messages, and sessions
- `BotController` REST endpoint scaffolding (`POST /api/bot/message`, `GET /api/bot/health`, `GET /api/bot/user/{id}`)
- `ExecutionContext` carrying per-request metadata through the service layer
- `Program.cs` with `WebApplication` minimal host bootstrap

### Changed
- `BotConfiguration` model split into focused sub-sections (Session, Message, RateLimit, Cache, Logging)

### Fixed
- Command handler lookup was case-sensitive; normalised to lowercase comparison

---

## [0.1.0] - 2025-01-15

### Added
- Initial project structure: `src/TelegramBotFramework`, `tests/`, `examples/`, `docs/`
- Solution file `telegram-bot-framework-dotnet.sln`
- `BotConfiguration` model and `appsettings.json` skeleton
- `BotUser` and `Message` domain models
- Stub `IRepository<T>` interface
- `.gitignore`, `.gitattributes`, `LICENSE` (MIT), `README.md` skeleton
- GitHub Actions workflow for `dotnet build` on push/PR

---

## Contributors

- [Vladyslav Zaiets](https://github.com/Sarmkadan) — Creator & Maintainer
- Community contributors and issue reporters

---

## Support & Contact

- [Documentation](docs/)
- [Report Issues](https://github.com/Sarmkadan/telegram-bot-framework-dotnet/issues)
- [GitHub Discussions](https://github.com/Sarmkadan/telegram-bot-framework-dotnet/discussions)
- Website: https://sarmkadan.com

---

## License

MIT License — See [LICENSE](LICENSE) file for details.

Copyright (c) 2025 Vladyslav Zaiets
