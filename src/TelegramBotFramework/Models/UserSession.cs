#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace TelegramBotFramework.Models;

/// <summary>
/// Represents a user's active session with state tracking.
/// </summary>
public sealed class UserSession : IUserSession, IEquatable<UserSession>
{
    public string SessionId { get; set; } = string.Empty;

    public long UserId { get; set; }

    public long ChatId { get; set; }

    public SessionState State { get; set; } = SessionState.Active;

    /// <summary>
    /// Convenience view of <see cref="State"/> as a boolean flag. Setting this
    /// property transitions the session to <see cref="SessionState.Active"/> or
    /// <see cref="SessionState.Closed"/>.
    /// </summary>
    public bool IsActive
    {
        get => State == SessionState.Active;
        set => State = value ? SessionState.Active : SessionState.Closed;
    }

    public string CurrentContext { get; set; } = "menu";

    public string? CurrentMenuId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastActivityAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiresAt { get; set; }

    public Dictionary<string, string>? ContextData { get; set; }

    public List<string>? CommandHistory { get; set; }

    public int InteractionCount { get; set; }

    public string? UserInput { get; set; }

    /// <summary>
    /// Checks if the session has expired.
    /// </summary>
    public bool IsExpired() =>
        ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;

    /// <summary>
    /// Updates last activity timestamp.
    /// </summary>
    public void UpdateActivity()
    {
        LastActivityAt = DateTime.UtcNow;
        InteractionCount++;
    }

    /// <summary>
    /// Gets the session duration.
    /// </summary>
    public TimeSpan GetDuration() =>
        DateTime.UtcNow - CreatedAt;

    /// <summary>
    /// Sets context data.
    /// </summary>
    public void SetContextData(string key, string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentException.ThrowIfNullOrEmpty(value);
        ContextData ??= new Dictionary<string, string>();
        ContextData[key] = value;
    }

    /// <summary>
    /// Gets context data.
    /// </summary>
    public string? GetContextData(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        return ContextData?.TryGetValue(key, out var value) == true ? value : null;
    }

    /// <summary>
    /// Removes context data.
    /// </summary>
    public bool RemoveContextData(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        return ContextData?.Remove(key) ?? false;
    }

    /// <summary>
    /// Clears all context data.
    /// </summary>
    public void ClearContextData()
    {
        ContextData?.Clear();
    }

    /// <summary>
    /// Adds command to history.
    /// </summary>
    public void AddCommandToHistory(string command)
    {
        ArgumentException.ThrowIfNullOrEmpty(command);
        CommandHistory ??= new List<string>();
        CommandHistory.Add($"{DateTime.UtcNow:O}:{command}");

        // Keep only last 50 commands
        if (CommandHistory.Count > 50)
            CommandHistory.RemoveAt(0);
    }

    /// <summary>
    /// Gets command history.
    /// </summary>
    public IEnumerable<string> GetCommandHistory() =>
        CommandHistory ?? Enumerable.Empty<string>();

    /// <summary>
    /// Validates session data.
    /// </summary>
    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(SessionId))
            throw new InvalidOperationException("SessionId is required");

        if (UserId <= 0)
            throw new InvalidOperationException("UserId must be positive");

        if (ChatId <= 0)
            throw new InvalidOperationException("ChatId must be positive");

        return true;
    }

    public bool Equals(UserSession? other)
    {
        if (other is null)
            return false;

        return SessionId == other.SessionId &&
               UserId == other.UserId &&
               ChatId == other.ChatId &&
               State == other.State &&
               CurrentContext == other.CurrentContext &&
               CurrentMenuId == other.CurrentMenuId &&
               CreatedAt == other.CreatedAt &&
               LastActivityAt == other.LastActivityAt;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as UserSession);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(SessionId, UserId, ChatId, State, CurrentContext, CurrentMenuId, CreatedAt, LastActivityAt);
    }

    public static bool operator ==(UserSession? left, UserSession? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(UserSession? left, UserSession? right)
    {
        return !Equals(left, right);
    }
}

public enum SessionState
{
    Active = 0,
    Idle = 1,
    Suspended = 2,
    Expired = 3,
    Closed = 4
}