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

## Installation

### Via NuGet Package (recommended)

The framework is available as a NuGet package:

```bash
dotnet add package Zaiets.telegram.bot.framework.dotnet
```

### From Source

```bash
git clone https://github.com/sarmkadan/telegram-bot-framework-dotnet.git
cd telegram-bot-framework-dotnet
dotnet restore && dotnet build
```

## Quick Start

Set your bot token in `src/TelegramBotFramework/appsettings.json`:

```json
{
  "botToken": "YOUR_BOT_TOKEN_HERE",
  "botUsername": "your_bot_username"
}
```

```bash
cd src/TelegramBotFramework && dotnet run
```

## Configuration

The framework can be configured via:

- Environment variables (recommended for production)
- Configuration files (appsettings.json)
- Code configuration


### Environment Variables

| Variable | Description |
|----------|-------------|
| `TELEGRAM_BOT_TOKEN` | Bot token from @BotFather (required) |
| `TELEGRAM_BOT_USERNAME` | Bot username (required) |
| `SESSION_TIMEOUT_MINUTES` | Session TTL, default 30 |
| `ENABLE_LOGGING` | Enable console logging, default true |

### Code Configuration

```csharp
var config = ConfigurationLoader.LoadFromEnvironment();
builder.Services.AddTelegramBotFramework(config);

// Optional: webhook mode
builder.Services.AddWebhookMode(opts =>
{
  opts.Url = "https://your-domain.com/api/webhook/telegram";
  opts.SecretToken = "your-secret-token";
});
```

## Usage Examples

For complete, runnable examples showing various features like command handling, caching, state management, and ASP.NET integration, please check the [/examples](examples/) directory.

### Command Handler

```csharp
[Command("start", Description = "Start the bot")]
public class StartCommandHandler : ICommandHandler
{
  public Task<ExecutionContext> HandleAsync(ExecutionContext ctx, CancellationToken ct = default)
  {
    ctx.SetState("response", "Hello!");
    return Task.FromResult(ctx);
  }
}
```

### Inline Keyboard Builder

```csharp
var markup = InlineKeyboardBuilder.Create(maxButtonsPerRow: 2)
  .AddButton("Accept", "action:accept")
  .AddButton("Decline", "action:decline")
  .NewRow()
  .AddUrlButton("Docs", "https://example.com/docs")
  .Build();
```

### Conversation Flow

```csharp
services.AddConversationFlows(opts => opts.InactivityTimeoutMinutes = 30);

// File-backed (survives restarts)
services.AddConversationFlowsWithFileStore("/var/bot/flow-states");
```

### Caching

```csharp
var user = await cache.GetOrCreateAsync($"user:{userId}",
  async () => await userService.GetUserAsync(userId),
  TimeSpan.FromHours(1));
```

## Docker Support

You can run the Telegram Bot Framework using Docker for easy deployment and consistent environments.

### Prerequisites

- Docker Engine 20.10+
- Docker Compose 2.0+
- Environment variables for your bot configuration


### Quick Start with Docker Compose

1. Build and start the services:

```bash
docker-compose up --build
```

2. The bot will be available at `http://localhost:8080`


### Configuration

The bot is configured via environment variables. Create a `.env` file:

```env
TELEGRAM_BOT_TOKEN=your_bot_token_from_botfather
TELEGRAM_BOT_USERNAME=your_bot_username
ASPNETCORE_ENVIRONMENT=Production
LOG_LEVEL=Information
```

### Available Services

The docker-compose setup includes:

- **telegram-bot**: The main bot application
- **redis**: Distributed cache for session management
- **postgres**: Optional PostgreSQL database for persistent storage

### Custom Docker Build

Build the Docker image manually:

```bash
docker build -t telegram-bot-framework .
```

Run the container:

```bash
docker run -p 8080:8080 -e TELEGRAM_BOT_TOKEN=your_token telegram-bot-framework
```

### Health Check

The container includes a health check that verifies the bot is running:

```bash
docker ps --filter "name=telegram-bot" --format "{{.Status}}"
```

## Testing

```bash
dotnet test
```

The framework includes 189 tests across model validation, infrastructure, utility, and feature coverage.

## License

MIT - see [LICENSE](LICENSE)