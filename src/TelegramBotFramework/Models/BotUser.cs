#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Models;

/// <summary>
/// Represents a Telegram user interacting with the bot.
/// </summary>
public sealed class BotUser : IBotUser, IEquatable<BotUser>
{
    public long TelegramId { get; set; }

    /// <summary>
    /// Alias for <see cref="TelegramId"/> used by consumers that model the user
    /// identifier as "UserId" rather than the Telegram-specific term.
    /// </summary>
    public long UserId
    {
        get => TelegramId;
        set => TelegramId = value;
    }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Username { get; set; }

    public string? PhoneNumber { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Active;

    public UserRole Role { get; set; } = UserRole.User;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp of the user's last recorded activity. <c>null</c> until the
    /// first activity is recorded.
    /// </summary>
    public DateTime? LastActivityAt { get; set; }

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
        LastActivityAt = DateTime.UtcNow;
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

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>true if the current object is equal to the <paramref name="other">parameter</paramref>; otherwise, false.</returns>
    public bool Equals(BotUser? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return TelegramId == other.TelegramId
            && string.Equals(FirstName, other.FirstName, StringComparison.Ordinal)
            && string.Equals(LastName, other.LastName, StringComparison.Ordinal)
            && string.Equals(Username, other.Username, StringComparison.Ordinal)
            && string.Equals(PhoneNumber, other.PhoneNumber, StringComparison.Ordinal)
            && Status == other.Status
            && Role == other.Role
            && CreatedAt.Equals(other.CreatedAt);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((BotUser)obj);
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(TelegramId, FirstName, LastName, Username, PhoneNumber, Status, Role, CreatedAt);
    }

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left-hand side.</param>
    /// <param name="right">The right-hand side.</param>
    /// <returns>true if the values are equal; otherwise, false.</returns>
    public static bool operator ==(BotUser? left, BotUser? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="left">The left-hand side.</param>
    /// <param name="right">The right-hand side.</param>
    /// <returns>true if the values are not equal; otherwise, false.</returns>
    public static bool operator !=(BotUser? left, BotUser? right)
    {
        return !Equals(left, right);
    }
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