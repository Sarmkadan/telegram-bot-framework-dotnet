#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Models;

/// <summary>
/// Represents a Telegram user interacting with the bot.
/// </summary>
public sealed class BotUser
{
    public long TelegramId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Username { get; set; }

    public string? PhoneNumber { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Active;

    public UserRole Role { get; set; } = UserRole.User;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsBot { get; set; }

    public bool IsPremium { get; set; }

    public int CommandsExecuted { get; set; }

    public int MessagesCount { get; set; }

    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// Validates the bot user data.
    /// </summary>
    /// <returns>True if user data is valid, throws otherwise.</returns>
    public bool Validate()
    {
        if (TelegramId <= 0)
            throw new InvalidOperationException("TelegramId must be positive");

        if (string.IsNullOrWhiteSpace(FirstName))
            throw new InvalidOperationException("FirstName cannot be empty");

        return true;
    }

    /// <summary>
    /// Gets the user's full display name.
    /// </summary>
    public string GetDisplayName() =>
        string.IsNullOrWhiteSpace(LastName)
            ? FirstName ?? "Unknown"
            : $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Updates user activity timestamp.
    /// </summary>
    public void UpdateActivity()
    {
        UpdatedAt = DateTime.UtcNow;
        MessagesCount++;
    }

    /// <summary>
    /// Sets user metadata value.
    /// </summary>
    public void SetMetadata(string key, string value)
    {
        Metadata ??= new Dictionary<string, string>();
        Metadata[key] = value;
    }

    /// <summary>
    /// Gets user metadata value.
    /// </summary>
    public string? GetMetadata(string key) =>
        Metadata?.TryGetValue(key, out var value) == true ? value : null;
}

public enum UserStatus
{
    Active = 0,
    Inactive = 1,
    Banned = 2,
    Suspended = 3
}

public enum UserRole
{
    User = 0,
    Moderator = 1,
    Admin = 2,
    Administrator = Admin,
    Owner = 3
}