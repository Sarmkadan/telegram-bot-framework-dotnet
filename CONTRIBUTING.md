# Contributing to Telegram Bot Framework

Thank you for your interest in contributing! This document explains how to get started, build and test locally, and submit quality pull requests.

## Code of Conduct

Please read our [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) before participating.

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Git
- A GitHub account

## Building Locally

```bash
# Clone your fork
git clone https://github.com/YOUR_USERNAME/telegram-bot-framework-dotnet.git
cd telegram-bot-framework-dotnet

# Restore dependencies
dotnet restore

# Build in Release mode
dotnet build --configuration Release

# Or use the Makefile shortcut
make build
```

## Running Tests

```bash
# Run all tests
dotnet test --verbosity normal

# Run with TRX output for detailed results
dotnet test --verbosity normal --logger "trx" --results-directory TestResults

# Or use the Makefile shortcut
make test
```

## Branching and Pull Requests

1. Fork the repository and create a feature branch:
   ```bash
   git checkout -b feature/your-feature-name
   ```
2. Keep commits focused and atomic. Use conventional commit prefixes:
   - `feat:` for new features
   - `fix:` for bug fixes
   - `docs:` for documentation changes
   - `ci:` for CI/CD changes
   - `refactor:` for code restructuring without behaviour change
3. Ensure all tests pass before opening a PR.
4. Open a pull request against `main` with a clear description of the change and why it is needed.
5. Link any relevant issues in the PR description.

## Code Style

- Follow the rules defined in [.editorconfig](.editorconfig).
- Use 4 spaces for indentation in C# files.
- Prefix private fields with `_` (e.g. `_service`).
- Prefix interfaces with `I` (e.g. `ICommandHandler`).
- Provide XML documentation comments (`///`) for all public APIs.
- Keep lines under 120 characters.

## Reporting Issues

- Search existing issues before opening a new one.
- Provide a minimal reproduction case, expected vs actual behaviour, and environment details (.NET version, OS).

## License

By contributing you agree that your contributions will be licensed under the MIT License.
