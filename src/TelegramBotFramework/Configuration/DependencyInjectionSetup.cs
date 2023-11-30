#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Configuration;

/// <summary>
/// Dependency injection setup and service registration.
/// </summary>
public static class DependencyInjectionSetup
{
    /// <summary>
    /// Registers all bot framework services in the DI container.
    /// </summary>
    public static Microsoft.Extensions.DependencyInjection.IServiceCollection
        AddTelegramBotFramework(
            this Microsoft.Extensions.DependencyInjection.IServiceCollection services,
            Models.BotConfiguration botConfig)
    {
        if (services  is null)
            throw new ArgumentNullException(nameof(services));

        if (botConfig  is null)
            throw new ArgumentNullException(nameof(botConfig));

        botConfig.Validate();

        // Register configuration as singleton
        services.AddSingleton(botConfig);

        // Register repositories as singletons (in-memory for Phase 1)
        services.AddSingleton<Repositories.IUserRepository, Repositories.InMemoryUserRepository>();
        services.AddSingleton<Repositories.ICommandRepository, Repositories.InMemoryCommandRepository>();
        services.AddSingleton<Repositories.IMessageRepository, Repositories.InMemoryMessageRepository>();
        services.AddSingleton<Repositories.ISessionRepository, Repositories.InMemorySessionRepository>();
        services.AddSingleton<Repositories.IMenuRepository, Repositories.InMemoryMenuRepository>();

        // Register services as singletons
        services.AddSingleton<Services.IUserService, Services.UserService>();
        services.AddSingleton<Services.ICommandService, Services.CommandService>();
        services.AddSingleton<Services.ISessionService, Services.SessionService>();
        services.AddSingleton<Services.IMenuService, Services.MenuService>();
        services.AddSingleton<Services.IMessageService, Services.MessageService>();
        services.AddSingleton<Services.IBotOrchestrator, Services.BotOrchestrator>();

        // Register built-in command handlers
        services.AddTransient<Commands.ICommandHandler, Commands.HelpCommandHandler>();

        // Register rate limiting strategy
        services.AddSingleton<Strategies.IRateLimitingStrategy, Strategies.InMemoryRateLimitingStrategy>();

        // Register middleware components
        services.AddTransient<Middleware.IBotMiddleware, Middleware.BotErrorHandlingMiddleware>();
        services.AddTransient<Middleware.IBotMiddleware, Middleware.BotLoggingMiddleware>();
        services.AddTransient<Middleware.IBotMiddleware, Middleware.AuthorizationMiddleware>();
        services.AddTransient<Middleware.IBotMiddleware, Middleware.RateLimitingMiddleware>();

        // Register logging
        services.AddLogging(config =>
        {
            config.ClearProviders();
            config.AddConsole();

            var logLevel = MapLogLevel(botConfig.LogLevel);
            config.SetMinimumLevel(logLevel);
        });

        return services;
    }

    /// <summary>
    /// Maps BotConfiguration LogLevel to Microsoft.Extensions.Logging.LogLevel.
    /// </summary>
    private static Microsoft.Extensions.Logging.LogLevel MapLogLevel(Models.LogLevel configLevel)
    {
        return configLevel switch
        {
            Models.LogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
            Models.LogLevel.Info => Microsoft.Extensions.Logging.LogLevel.Information,
            Models.LogLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
            Models.LogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
            Models.LogLevel.Critical => Microsoft.Extensions.Logging.LogLevel.Critical,
            _ => Microsoft.Extensions.Logging.LogLevel.Information
        };
    }
}

/// <summary>
/// Extension methods for registering webhook mode in the dependency-injection container.
/// </summary>
public static class WebhookSetup
{
    /// <summary>
    /// Registers webhook mode services and the <see cref="Integration.WebhookService"/> hosted service.
    /// Call this after <c>AddTelegramBotFramework</c> in your startup code.
    /// </summary>
    /// <param name="services">The <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/> to configure.</param>
    /// <param name="configure">Delegate that populates <see cref="Integration.WebhookOptions"/>.</param>
    /// <returns>The same service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// services
    ///     .AddTelegramBotFramework(config)
    ///     .AddWebhookMode(opts =>
    ///     {
    ///         opts.Url        = "https://mybot.example.com/api/webhook/telegram";
    ///         opts.SecretToken = "my-secret";
    ///     });
    /// </code>
    /// </example>
    public static Microsoft.Extensions.DependencyInjection.IServiceCollection AddWebhookMode(
        this Microsoft.Extensions.DependencyInjection.IServiceCollection services,
        Action<Integration.WebhookOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new Integration.WebhookOptions();
        configure(options);
        options.Validate();

        services.AddSingleton(options);
        services.AddSingleton<Integration.TelegramApiClient>(sp =>
        {
            var config = sp.GetRequiredService<Models.BotConfiguration>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Integration.TelegramApiClient>>();
            return new Integration.TelegramApiClient(config.BotToken, null, logger);
        });
        services.AddSingleton<Integration.WebhookService>();
        services.AddSingleton<Integration.IWebhookService>(sp =>
            sp.GetRequiredService<Integration.WebhookService>());
        services.AddHostedService(sp =>
            sp.GetRequiredService<Integration.WebhookService>());

        return services;
    }
}

/// <summary>
/// Default configuration loader from appsettings.json.
/// </summary>
public sealed class ConfigurationLoader
{
    /// <summary>
    /// Loads bot configuration from JSON file.
    /// </summary>
    public static Models.BotConfiguration LoadFromJsonFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Configuration file not found: {filePath}");

        var json = File.ReadAllText(filePath);
        var doc = System.Text.Json.JsonDocument.Parse(json);
        var root = doc.RootElement;

        var config = new Models.BotConfiguration
        {
            BotToken = root.GetProperty("botToken").GetString() ?? string.Empty,
            BotUsername = root.GetProperty("botUsername").GetString() ?? string.Empty,
            DatabaseConnectionString = root.TryGetProperty("databaseConnectionString", out var dbProp)
                ? dbProp.GetString() ?? string.Empty
                : string.Empty,
            SessionTimeoutMinutes = root.TryGetProperty("sessionTimeoutMinutes", out var timeoutProp)
                ? timeoutProp.GetInt32()
                : Constants.BotConstants.DefaultSessionTimeoutMinutes,
            MessageProcessingTimeoutSeconds = root.TryGetProperty("messageProcessingTimeoutSeconds", out var msgTimeoutProp)
                ? msgTimeoutProp.GetInt32()
                : Constants.BotConstants.DefaultMessageTimeoutSeconds,
            MaxConcurrentRequests = root.TryGetProperty("maxConcurrentRequests", out var concurrentProp)
                ? concurrentProp.GetInt32()
                : Constants.BotConstants.DefaultMaxConcurrentRequests,
            EnableLogging = root.TryGetProperty("enableLogging", out var loggingProp)
                ? loggingProp.GetBoolean()
                : true,
            EnableRateLimiting = root.TryGetProperty("enableRateLimiting", out var rateLimitProp)
                ? rateLimitProp.GetBoolean()
                : true,
        };

        config.Validate();
        return config;
    }

    /// <summary>
    /// Loads bot configuration from environment variables.
    /// </summary>
    public static Models.BotConfiguration LoadFromEnvironment()
    {
        var botToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN")
            ?? throw new InvalidOperationException("TELEGRAM_BOT_TOKEN environment variable not set");

        var botUsername = Environment.GetEnvironmentVariable("TELEGRAM_BOT_USERNAME")
            ?? throw new InvalidOperationException("TELEGRAM_BOT_USERNAME environment variable not set");

        var config = new Models.BotConfiguration
        {
            BotToken = botToken,
            BotUsername = botUsername,
            DatabaseConnectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING") ?? string.Empty,
            SessionTimeoutMinutes = int.TryParse(
                Environment.GetEnvironmentVariable("SESSION_TIMEOUT_MINUTES"), out var timeout)
                ? timeout
                : Constants.BotConstants.DefaultSessionTimeoutMinutes,
            EnableLogging = bool.TryParse(
                Environment.GetEnvironmentVariable("ENABLE_LOGGING"), out var logging)
                ? logging
                : true,
        };

        config.Validate();
        return config;
    }
}