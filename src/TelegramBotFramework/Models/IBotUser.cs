#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Models;

/// <summary>
/// Represents a Telegram user interacting with the bot.
/// </summary>
public interface IBotUser
{
    long TelegramId { get; set; }

    string? FirstName { get; set; }

    string? LastName { get; set; }

    string? Username { get; set; }

    string? PhoneNumber { get; set; }

    UserStatus Status { get; set; }

    UserRole Role { get; set; }

    DateTime CreatedAt { get; set; }

    DateTime UpdatedAt { get; set; }

    DateTime? LastActivityAt { get; set; }

    bool IsBot { get; set; }

    bool IsPremium { get; set; }

    int CommandsExecuted { get; set; }

    int MessagesCount { get; set; }

    Dictionary<string, string>? Metadata { get; set; }

    bool Validate();

    string GetDisplayName();

    void UpdateActivity();

    void SetMetadata(string key, string value);

    string? GetMetadata(string key);
}