#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using TelegramBotFramework.Configuration;
using TelegramBotFramework.Models;

var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

// Load configuration
BotConfiguration botConfig;
try
{
    // Try to load from environment first
    botConfig = ConfigurationLoader.LoadFromEnvironment();
}
catch
{
    // Fall back to appsettings.json
    var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
    botConfig = ConfigurationLoader.LoadFromJsonFile(configPath);
}

// Register framework services
builder.Services.AddTelegramBotFramework(botConfig);

// Add controllers
builder.Services.AddControllers();

// Add endpoint metadata for development diagnostics
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
}

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Initialize default commands and menus
await InitializeDefaultDataAsync(app.Services);

app.Run();

/// <summary>
/// Initializes default commands and menus in the framework.
/// </summary>
static async Task InitializeDefaultDataAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var commandService = scope.ServiceProvider.GetRequiredService<TelegramBotFramework.Services.ICommandService>();
    var menuService = scope.ServiceProvider.GetRequiredService<TelegramBotFramework.Services.IMenuService>();
    var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Program>>();

    try
    {
        // Register default commands
        var startCommand = new Command
        {
            Name = "/start",
            Description = "Start the bot",
            HandlerType = "StartCommandHandler",
            Type = CommandType.Standard,
            IsEnabled = true,
            RequiresAdmin = false
        };

        await commandService.RegisterCommandAsync(startCommand);

        var helpCommand = new Command
        {
            Name = "/help",
            Description = "Show help information",
            HandlerType = "HelpCommandHandler",
            Type = CommandType.Standard,
            IsEnabled = true,
            RequiresAdmin = false
        };

        await commandService.RegisterCommandAsync(helpCommand);

        var settingsCommand = new Command
        {
            Name = "/settings",
            Description = "Open user settings",
            HandlerType = "SettingsCommandHandler",
            Type = CommandType.Standard,
            IsEnabled = true,
            RequiresAdmin = false
        };

        await commandService.RegisterCommandAsync(settingsCommand);

        // Create main menu
        var mainMenu = new Menu
        {
            Id = "main_menu",
            Title = "Main Menu",
            Description = "Welcome to the bot",
            Type = MenuType.Inline,
            IsActive = true,
            DisplayOrder = 1,
            MaxButtonsPerRow = 2
        };

        var helpButton = new MenuButton
        {
            Label = "❓ Help",
            CallbackData = "help",
            Action = ButtonAction.ExecuteCommand,
            DisplayOrder = 1
        };

        var settingsButton = new MenuButton
        {
            Label = "⚙️ Settings",
            CallbackData = "settings",
            Action = ButtonAction.NavigateMenu,
            DisplayOrder = 2
        };

        mainMenu.AddButton(helpButton);
        mainMenu.AddButton(settingsButton);

        await menuService.CreateMenuAsync(mainMenu);

        logger.LogInformation("Default data initialized successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error initializing default data");
        throw;
    }
}