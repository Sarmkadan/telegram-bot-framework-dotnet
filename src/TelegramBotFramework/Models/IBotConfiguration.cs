#nullable enable

namespace TelegramBotFramework.Models;

/// <summary>
/// Defines the contract for bot configuration settings.
/// </summary>
public interface IBotConfiguration
{
    string BotToken { get; set; }
    string BotUsername { get; set; }
    long? OwnerId { get; set; }
    string DatabaseConnectionString { get; set; }
    int SessionTimeoutMinutes { get; set; }
    int MessageProcessingTimeoutSeconds { get; set; }
    bool EnableLogging { get; set; }
    LogLevel LogLevel { get; set; }
    int MaxConcurrentRequests { get; set; }
    bool EnableWebhook { get; set; }
    string? ApiKey { get; set; }
    string? WebhookUrl { get; set; }
    string? WebhookSecret { get; set; }
    Dictionary<string, string> CustomSettings { get; set; }
    List<long> AdminIds { get; set; }
    bool EnableRateLimiting { get; set; }
    int RateLimitPerMinute { get; set; }
    string? LocalizationLanguage { get; set; }
    bool Validate();
    string? GetCustomSetting(string key);
}