#nullable enable
namespace TelegramBotFramework.Configuration;

/// <summary>
/// Constants for DependencyInjectionSetup.
/// </summary>
internal static class DependencyInjectionSetupConstants
{
    // JSON property names
    public const string BotTokenJsonProperty = "botToken";
    public const string BotUsernameJsonProperty = "botUsername";
    public const string DatabaseConnectionStringJsonProperty = "databaseConnectionString";
    public const string SessionTimeoutMinutesJsonProperty = "sessionTimeoutMinutes";
    public const string MessageProcessingTimeoutSecondsJsonProperty = "messageProcessingTimeoutSeconds";
    public const string MaxConcurrentRequestsJsonProperty = "maxConcurrentRequests";
    public const string EnableLoggingJsonProperty = "enableLogging";
    public const string EnableRateLimitingJsonProperty = "enableRateLimiting";

    // Environment variable names
    public const string BotTokenEnvVariable = "TELEGRAM_BOT_TOKEN";
    public const string BotUsernameEnvVariable = "TELEGRAM_BOT_USERNAME";
    public const string DatabaseConnectionStringEnvVariable = "DATABASE_CONNECTION_STRING";
    public const string SessionTimeoutMinutesEnvVariable = "SESSION_TIMEOUT_MINUTES";
    public const string EnableLoggingEnvVariable = "ENABLE_LOGGING";
    public const string EnableRateLimitingEnvVariable = "ENABLE_RATE_LIMITING";

    // Format strings
    public const string ConfigurationFileNotFoundMessage = "Configuration file not found: ";
}