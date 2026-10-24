# Contributing to Telegram Bot Framework

Thanks for your interest in contributing! Please read our [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) before participating.

## Prerequisites

- **.NET SDK 10.0.100 or newer in the 10.0 line.** `global.json` pins `10.0.100` with `rollForward: latestMinor`. All projects target `net10.0`.
- Git
- Optional: `make` (thin wrappers around the commands below)

## Project Layout

| Path | Contents |
|------|----------|
| `src/TelegramBotFramework/` | The library (NuGet `Zaiets.telegram.bot.framework.dotnet`) |
| `tests/telegram-bot-framework-dotnet.Tests/` | xUnit test project (flat, `<Type>Tests.cs` files) |
| `benchmarks/telegram-bot-framework-dotnet.Benchmarks/` | BenchmarkDotNet benchmarks |
| `examples/` | Standalone usage samples (not part of the solution) |
| `docs/` | Per-type reference documentation |

The solution file is `telegram-bot-framework-dotnet.slnx`.

## Building

```bash
dotnet restore
dotnet build --configuration Release    # or: make build
```

`src/TelegramBotFramework` builds with `TreatWarningsAsErrors=true`, so warnings fail the build. Keep it warning-free.

## Testing

```bash
dotnet test --configuration Release --verbosity normal                  # or: make test
dotnet test --filter "FullyQualifiedName~BotOrchestratorTests"         # a single test class
```

- Frameworks: xUnit, FluentAssertions, Moq.
- Name tests `Method_Scenario_Expectation`, use Arrange/Act/Assert comments, and assert with `.Should()`.
- The test project can see `internal` members through `InternalsVisibleTo`.

## Coding Conventions

- Follow `.editorconfig`: C#, 4-space indentation, LF line endings, lines up to 120 columns, Allman braces, braces always.
- Run `make format` (`dotnet format`) before committing and `make lint` to check code style.
- Nullable reference types are enabled. Guard public entry points with `ArgumentNullException.ThrowIfNull` or `ArgumentException.ThrowIfNullOrWhiteSpace`.
- File-scoped namespaces (`TelegramBotFramework.<Folder>`), one type per file.
- Naming:
  - Private fields use `_camelCase` (e.g. `_service`).
  - Interfaces are prefixed with `I` (e.g. `ICommandHandler`).
  - Async methods use the `Async` suffix and take `CancellationToken` as their last parameter.
  - Library code uses `ConfigureAwait(false)` on every `await`.
- Mark classes `sealed` unless they are designed for inheritance.
- Add XML `///` documentation to all public members.

## Branching and Pull Requests

1. Fork the repository and branch from `main`, using a conventional prefix in the branch name, e.g. `feat/retry-policy`.
2. Keep PRs small and focused: one logical change per PR. Split refactors from behaviour changes.
3. Use conventional commit prefixes: `feat:`, `fix:`, `docs:`, `refactor:`, `ci:`, `chore:`.
4. Before opening the PR, make sure `dotnet build` and `dotnet test` pass locally and `make format` leaves no changes. Add or update tests for any behaviour change.
5. Open the PR against `main`. Describe what changed and why, and link any related issue.
6. CI must pass. The `ci.yml` and `build.yml` workflows run restore, a Release build, and the tests on every PR.

## Reporting Bugs and Requesting Features

- Search the existing issues first.
- **Bugs:** include a minimal reproduction, expected vs actual behaviour, the .NET SDK version (`dotnet --version`), the OS, and the framework package version.
- **Security issues:** do not open a public issue. Follow [SECURITY.md](SECURITY.md).
- **Feature requests:** describe the use case and why it belongs in the framework. For larger changes, open an issue to agree on the approach before writing code.

## License

By contributing, you agree that your contributions are licensed under the [MIT License](LICENSE).
