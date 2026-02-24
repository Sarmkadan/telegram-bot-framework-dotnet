// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

#nullable enable

namespace TelegramBotFramework.Models;

/// <summary>
/// Represents the bot configuration settings.
/// </summary>
public sealed class BotConfiguration
{
    public string BotToken { get; set; } = string.Empty;

    public string BotUsername { get; set; } = string.Empty;

    public long? OwnerId { get; set; }

    public string DatabaseConnectionString { get; set; } = string.Empty;

    public int SessionTimeoutMinutes { get; set; } = 30;

    public int MessageProcessingTimeoutSeconds { get; set; } = 10;

    public bool EnableLogging { get; set; } = true;

    public LogLevel LogLevel { get; set; } = LogLevel.Info;

    public int MaxConcurrentRequests { get; set; } = 10;

    public bool EnableWebhook { get; set; }

    public string? WebhookUrl { get; set; }

    public string? WebhookSecret { get; set; }

    public Dictionary<string, string> CustomSettings { get; set; } = new();

    public List<long> AdminIds { get; set; } = new();

    public bool EnableRateLimiting { get; set; } = true;

    public int RateLimitPerMinute { get; set; } = 30;

    public string? LocalizationLanguage { get; set; } = "en";

    /// <summary>
    /// Validates the bot configuration.
    /// </summary>
    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(BotToken))
            throw new InvalidOperationException("BotToken is required");

        if (string.IsNullOrWhiteSpace(BotUsername))
            throw new InvalidOperationException("BotUsername is required");

        if (SessionTimeoutMinutes < 1)
            throw new InvalidOperationException("SessionTimeoutMinutes must be at least 1");

        if (MaxConcurrentRequests < 1)
            throw new InvalidOperationException("MaxConcurrentRequests must be at least 1");

        return true;
    }

    /// <summary>
    /// Gets a custom setting value.
    /// </summary>
    public string? GetCustomSetting(string key) =>
        CustomSettings?.TryGetValue(key, out var value) == true ? value : null;

    /// <summary>
    /// Sets a custom setting value.
    /// </summary>
    public void SetCustomSetting(string key, string value)
    {
        CustomSettings ??= new Dictionary<string, string>();
        CustomSettings[key] = value;
    }

    /// <summary>
    /// Checks if user is admin.
    /// </summary>
    public bool IsAdmin(long userId) =>
        AdminIds?.Contains(userId) == true || OwnerId == userId;

    /// <summary>
    /// Adds admin ID.
    /// </summary>
    public void AddAdmin(long userId)
    {
        AdminIds ??= new List<long>();
        if (!AdminIds.Contains(userId))
            AdminIds.Add(userId);
    }

    /// <summary>
    /// Removes admin ID.
    /// </summary>
    public bool RemoveAdmin(long userId) =>
        AdminIds?.Remove(userId) ?? false;

    /// <summary>
    /// Gets session timeout as TimeSpan.
    /// </summary>
    public TimeSpan GetSessionTimeout() =>
        TimeSpan.FromMinutes(SessionTimeoutMinutes);
}

public enum LogLevel
{
    Debug = 0,
    Info = 1,
    Warning = 2,
    Error = 3,
    Critical = 4
}