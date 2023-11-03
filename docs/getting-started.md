# Getting Started with Telegram Bot Framework

A step-by-step guide to building your first Telegram bot with the framework.

## Prerequisites

- **.NET 10 SDK** - [Download here](https://dotnet.microsoft.com/download)
- **Telegram Account** - For testing
- **BotFather** - Telegram bot for creating/managing bots: [@BotFather](https://t.me/botfather)
- **Code Editor** - VS Code, Visual Studio, or JetBrains Rider

## Step 1: Create Your Bot Token

1. Open Telegram and search for **@BotFather**
2. Send `/start` command
3. Send `/newbot` to create a new bot
4. Follow the prompts:
   - Give it a name (e.g., "MyAwesomeBot")
   - Give it a username (must end with "bot", e.g., "myawesomebot")
5. Save the bot token (looks like: `123456789:ABCDefGHiJKlmnoPQRstuvWXYZ`)

## Step 2: Clone the Repository

```bash
git clone https://github.com/Sarmkadan/telegram-bot-framework-dotnet.git
cd telegram-bot-framework-dotnet
```

## Step 3: Configure Your Bot

Edit `src/TelegramBotFramework/appsettings.json`:

```json
{
  "BotConfiguration": {
    "BotToken": "YOUR_BOT_TOKEN_HERE",
    "BotUsername": "your_bot_username",
    "UseWebhook": false
  }
}
```

Or set environment variable:
```bash
export TELEGRAM_BOT_TOKEN=your_token_here
```

## Step 4: Run the Bot

```bash
cd src/TelegramBotFramework
dotnet restore
dotnet run
```

You should see:
```
info: TelegramBotFramework.Program[0]
      Bot is running on https://localhost:5001
```

## Step 5: Test Your Bot

1. Open Telegram
2. Find your bot (search by username)
3. Send `/start` or any message
4. Bot should respond with the default handler

## Next Steps

- **Read Examples**: Check `examples/` directory for complete implementations
- **API Reference**: See [api-reference.md](api-reference.md)
- **Architecture**: Understand the design in [architecture.md](architecture.md)
- **Deployment**: Learn deployment options in [deployment.md](deployment.md)

## Common Issues

### Bot doesn't respond

**Problem**: Messages sent to bot are not being processed.

**Solution**:
1. Verify bot token is correct
2. Check logs for errors
3. Ensure bot is running (`dotnet run`)
4. Try restarting the bot

### Bot Token errors

**Problem**: Getting "Invalid bot token" error.

**Solution**:
1. Copy token directly from BotFather (no extra spaces)
2. Verify token in appsettings.json
3. Check token hasn't been revoked (use `/token` in BotFather)

### Port already in use

**Problem**: Port 5001 is already in use.

**Solution**:
1. Change port in `launchSettings.json`
2. Or kill the process using the port:
   ```bash
   # Linux/Mac
   lsof -i :5001
   kill -9 <PID>
   
   # Windows
   netstat -ano | findstr :5001
   taskkill /PID <PID> /F
   ```

## Quick Commands

### Build
```bash
dotnet build
```

### Run
```bash
dotnet run
```

### Run Tests
```bash
dotnet test
```

### Publish Release
```bash
dotnet publish -c Release
```

## Project Structure

```
src/TelegramBotFramework/
├── Models/           # Data models (User, Message, Command, etc)
├── Services/         # Business logic
├── Controllers/      # API endpoints
├── Middleware/       # Request pipeline
├── Configuration/    # DI setup
└── Program.cs        # Entry point
```

## Creating Your First Command

In any service:

```csharp
var commandService = serviceProvider.GetRequiredService<ICommandService>();

var command = new Command
{
    Name = "/hello",
    Description = "Say hello",
    HandlerType = "HelloCommandHandler",
    Type = CommandType.Standard,
    IsEnabled = true,
    RequiresAdmin = false
};

await commandService.RegisterCommandAsync(command);
```

## Creating Your First Menu

```csharp
var sessionService = serviceProvider.GetRequiredService<ISessionAndMenuService>();

var menu = new Menu
{
    Id = "hello_menu",
    Title = "Hello Menu",
    Type = MenuType.Inline,
    IsActive = true
};

menu.AddButton(new MenuButton
{
    Label = "👋 Say Hi",
    CallbackData = "hello:say_hi",
    Action = ButtonAction.NavigateMenu
});

await sessionService.CreateMenuAsync(menu);
```

## Next: Full Examples

Visit the `examples/` directory for more complete implementations:
- `BasicBotExample.cs` - Simple command handling
- `MenuNavigationExample.cs` - Interactive menus
- `StateManagementExample.cs` - Complex flows
- `AdminOperationsExample.cs` - User management
- `CachingExample.cs` - Performance optimization

## Getting Help

- 📖 Read the [README](../README.md)
- 📚 Check [Architecture Guide](architecture.md)
- 🔗 [API Reference](api-reference.md)
- 💬 [GitHub Issues](https://github.com/Sarmkadan/telegram-bot-framework-dotnet/issues)
- 📧 Email: rutova2@gmail.com

## Resources

- [Telegram Bot API Documentation](https://core.telegram.org/bots/api)
- [.NET 10 Documentation](https://docs.microsoft.com/dotnet/)
- [C# Documentation](https://docs.microsoft.com/dotnet/csharp/)
