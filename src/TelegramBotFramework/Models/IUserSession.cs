#nullable enable
namespace TelegramBotFramework.Models;

/// <summary>
/// Represents a user's active session with state tracking.
/// </summary>
public interface IUserSession
{
    string SessionId { get; set; }
    long UserId { get; set; }
    long ChatId { get; set; }
    SessionState State { get; set; }
    string CurrentContext { get; set; }
    string? CurrentMenuId { get; set; }
    DateTime CreatedAt { get; set; }
    DateTime? LastActivityAt { get; set; }
    DateTime? ExpiresAt { get; set; }
    Dictionary<string, string>? ContextData { get; set; }
    List<string>? CommandHistory { get; set; }
    int InteractionCount { get; set; }
    string? UserInput { get; set; }
    bool IsExpired();
    void UpdateActivity();
    TimeSpan GetDuration();
    void SetContextData(string key, string value);
    string? GetContextData(string key);
    bool RemoveContextData(string key);
    void ClearContextData();
}