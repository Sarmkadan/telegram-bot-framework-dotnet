# CLAUDE.md

## Project overview

.NET 10 framework (NuGet `Zaiets.telegram.bot.framework.dotnet`) for building Telegram bots: command handlers, middleware pipeline, conversation flows, DI, webhook/polling integration. Wraps `Telegram.Bot` 22.x.

## Build

```bash
dotnet restore
dotnet build --configuration Release          # or: make build
```

- SDK pinned in `global.json`: 10.0.100 (`rollForward: latestMinor`).
- Solution: `telegram-bot-framework-dotnet.sln` (3 projects: src, tests, benchmarks).
- `src/TelegramBotFramework` has `TreatWarningsAsErrors=true` (XML-doc warnings CS1573/1574/1587/1591 suppressed). Test project does not.
- Run the sample host: `cd src/TelegramBotFramework && dotnet run` (`make run`). Config from env vars, falls back to `appsettings.json`.
- Docker: `Dockerfile`, `docker-compose.yml`, `make docker-build|docker-up`.

## Test

```bash
dotnet test --configuration Release --verbosity normal                    # or: make test
dotnet test --filter "FullyQualifiedName~BotOrchestratorTests"          # single class
dotnet test --logger "trx" --results-directory TestResults                # CI format
```

- xUnit 2.9 + FluentAssertions 7 + Moq 4.20; `tests/telegram-bot-framework-dotnet.Tests/` (flat, ~240 files, namespace `TelegramBotFramework.Tests`).
- Convention: `<Type>Tests.cs`, `sealed` class, `[Fact]`/`[Theory]`, Arrange/Act/Assert comments, `Method_Scenario_Expectation` names, `.Should()` assertions.
- Test project sees `internal` members via `InternalsVisibleTo`.
- Benchmarks: `benchmarks/telegram-bot-framework-dotnet.Benchmarks` (BenchmarkDotNet, `dotnet run -c Release`).

## Lint / Format

```bash
dotnet format                                                  # make format
dotnet build --no-restore /p:EnforceCodeStyleInBuild=true      # make lint
```

Rules in `.editorconfig`: 4 spaces, LF, 120 cols, Allman braces, `_camelCase` private fields, `I`-prefixed interfaces, braces always.

## Architecture

- Entry point: `src/TelegramBotFramework/Program.cs` (minimal ASP.NET host, `WebApplication.CreateBuilder`, `MapControllers`).
- DI root: `Configuration/DependencyInjectionSetup.cs` -> `AddTelegramBotFramework(botConfig)`; `WebhookSetup.AddWebhookMode(...)`. Feature areas add their own `Add*` extension methods (`AddConversationFlows`, `AddBroadcastService`, `AddInlineQueryHandling`, `AddWebhookService`, `AddQuizFlow`).
- `src/TelegramBotFramework/` folders by concern:
  - `Services/` - core services (`BotOrchestrator` dispatches updates; Command/User/Session/Menu/Message/Broadcast/ScheduledMessage/Localization). Interface + impl per service.
  - `Middleware/` - `IBotMiddleware` pipeline (ErrorHandling, Logging, Authentication, Authorization, RateLimiting).
  - `ConversationFlow/` - state machine engine, `QuizFlow/`, state stores (in-memory, file).
  - `Integration/` - `ITelegramApiClient`, webhook service, polling/offset store, external API integration, retry.
  - `Repositories/` - `IRepository<T>` + in-memory implementations (default persistence).
  - `Controllers/` - `BotController` (webhook endpoint), `AdminController`, `HealthController`.
  - `Commands/`, `Attributes/` (`[Command]`, `[Cooldown]`), `Keyboard/` (builders), `Formatters/`, `Caching/`, `BackgroundWorkers/`, `Events/` (EventBus), `Strategies/` (rate limiting), `Utilities/` (`CallbackDataSigner` HMAC, crypto), `Exceptions/`, `Models/`, `Constants/`.
- `examples/` - standalone usage samples (not in the solution). `docs/` - per-type markdown reference. Root `*_IMPLEMENTATION*.md` files are feature notes.

## Conventions

- File-scoped namespaces `TelegramBotFramework.<Folder>`, `#nullable enable`, `ImplicitUsings`; namespaces often fully qualified inline instead of `using`.
- One type per file; companion files split by role: `Foo.cs`, `FooConstants.cs` (internal static constants, no magic strings), `FooExtensions.cs`, `FooValidation.cs`, `FooBuilder.cs`, `FooJsonExtensions.cs`. Nearly every concrete class has an `IFoo` interface.
- XML `///` docs on all public members (`GenerateDocumentationFile=true`).
- Async: `Async` suffix, `CancellationToken` last param, `ConfigureAwait(false)` on all awaits in library code.
- Guards: `ArgumentNullException.ThrowIfNull(x)` / `ArgumentException.ThrowIfNullOrWhiteSpace` at method entry.
- Errors: `BotFrameworkException` base with sealed subclasses (`CommandExecutionException`, `CommandNotFoundException`, `InsufficientPermissionException`, `SessionException`); surfaced through `ErrorHandlingMiddleware`. Program.cs swallows config-load failure to fall back to JSON.
- DI lifetimes: repositories/services singleton, command handlers and middleware transient. Register via `IServiceCollection` extension methods returning `services`.
- Classes `sealed` where not designed for inheritance.
- Commits: conventional prefixes (`feat:`, `fix:`, `docs:`, `refactor:`, `ci:`, `chore:`). No `Co-Authored-By` trailers. Branch: `main`; CI (`.github/workflows/ci.yml`, `build.yml`) runs restore/build Release/test on push and PR.
