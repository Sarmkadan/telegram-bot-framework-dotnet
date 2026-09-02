// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

#nullable enable

namespace TelegramBotFramework.Models;

/// <summary>
/// Represents the bot configuration settings.
/// </summary>
/// <remarks>
/// Contains all configuration parameters for the Telegram bot including authentication,
/// session management, logging, rate limiting, and webhook settings.
/// </remarks>
public sealed class BotConfiguration : IEquatable<BotConfiguration>, IBotConfiguration
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

    public string? ApiKey { get; set; }

    public string? WebhookUrl { get; set; }

    public string? WebhookSecret { get; set; }

    public Dictionary<string, string> CustomSettings { get; set; } = new();

    public List<long> AdminIds { get; set; } = new();

    public bool EnableRateLimiting { get; set; } = true;

    public int RateLimitPerMinute { get; set; } = 30;

    public string? LocalizationLanguage { get; set; } = "en";

    public override string ToString() =>
        $"BotConfiguration {{ BotToken = {BotToken}, BotUsername = {BotUsername}, OwnerId = {OwnerId}, DatabaseConnectionString = {DatabaseConnectionString}, SessionTimeoutMinutes = {SessionTimeoutMinutes}, MessageProcessingTimeoutSeconds = {MessageProcessingTimeoutSeconds} }}";

    public bool Equals(BotConfiguration? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return BotToken == other.BotToken &&
               BotUsername == other.BotUsername &&
               OwnerId == other.OwnerId &&
               DatabaseConnectionString == other.DatabaseConnectionString &&
               SessionTimeoutMinutes == other.SessionTimeoutMinutes &&
               MessageProcessingTimeoutSeconds == other.MessageProcessingTimeoutSeconds &&
               EnableLogging == other.EnableLogging &&
               LogLevel == other.LogLevel;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is BotConfiguration other && Equals(other));
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(BotToken, BotUsername, OwnerId, DatabaseConnectionString, SessionTimeoutMinutes, MessageProcessingTimeoutSeconds, EnableLogging, LogLevel);
    }

    public static bool operator ==(BotConfiguration? left, BotConfiguration? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(BotConfiguration? left, BotConfiguration? right)
    {
        return !Equals(left, right);
    }

    /// <summary>
    /// Validates the bot configuration.
    /// </summary>
    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(BotToken) || BotToken.Trim().Length < 2)
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
    public string? GetCustomSetting(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        return CustomSettings?.TryGetValue(key, out var value) == true ? value : null;
    }

    /// <summary>
    /// Sets a custom setting value.
    /// </summary>
    public void SetCustomSetting(string key, string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentException.ThrowIfNullOrEmpty(value);
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