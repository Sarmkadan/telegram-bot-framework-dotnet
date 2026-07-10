#nullable enable

namespace TelegramBotFramework.Models;

/// <summary>
/// Provides extension methods for <see cref="UserSession"/> to enhance session management capabilities.
/// </summary>
public static class UserSessionExtensions
{
    /// <summary>
    /// Determines if the session is currently idle based on the state and last activity time.
    /// </summary>
    /// <param name="session">The user session to check.</param>
    /// <param name="idleTimeout">The timeout duration after which the session is considered idle. Default is 5 minutes.</param>
    /// <returns>True if the session is idle; otherwise, false.</returns>
    public static bool IsIdle(this UserSession session, TimeSpan? idleTimeout = null)
    {
        var timeout = idleTimeout ?? TimeSpan.FromMinutes(5);

        return session.State == SessionState.Active &&
               session.LastActivityAt.HasValue &&
               DateTime.UtcNow - session.LastActivityAt.Value > timeout;
    }

    /// <summary>
    /// Determines if the session is about to expire soon based on the expiration time.
    /// </summary>
    /// <param name="session">The user session to check.</param>
    /// <param name="warningThreshold">The time threshold before expiration to consider it "about to expire". Default is 1 hour.</param>
    /// <returns>True if the session will expire within the warning threshold; otherwise, false.</returns>
    public static bool IsAboutToExpire(this UserSession session, TimeSpan? warningThreshold = null)
    {
        var threshold = warningThreshold ?? TimeSpan.FromHours(1);

        return session.ExpiresAt.HasValue &&
               session.ExpiresAt.Value - DateTime.UtcNow <= threshold;
    }

    /// <summary>
    /// Gets the remaining time until session expiration.
    /// </summary>
    /// <param name="session">The user session to check.</param>
    /// <returns>The remaining time until expiration, or null if the session never expires.</returns>
    public static TimeSpan? GetTimeUntilExpiration(this UserSession session)
    {
        if (!session.ExpiresAt.HasValue)
            return null;

        var remaining = session.ExpiresAt.Value - DateTime.UtcNow;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    /// <summary>
    /// Safely gets a typed value from context data.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the value to.</typeparam>
    /// <param name="session">The user session.</param>
    /// <param name="key">The context data key.</param>
    /// <param name="defaultValue">The default value to return if the key doesn't exist or parsing fails.</param>
    /// <returns>The parsed value or the default value.</returns>
    public static T? GetContextData<T>(this UserSession session, string key, T? defaultValue = default)
    {
        var value = session.GetContextData(key);

        if (value == null)
            return defaultValue;

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(value);
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Safely sets a typed value in context data.
    /// </summary>
    /// <typeparam name="T">The type of the value to serialize.</typeparam>
    /// <param name="session">The user session.</param>
    /// <param name="key">The context data key.</param>
    /// <param name="value">The value to store.</param>
    public static void SetContextData<T>(this UserSession session, string key, T? value)
    {
        if (value == null)
        {
            session.RemoveContextData(key);
            return;
        }

        session.SetContextData(key, System.Text.Json.JsonSerializer.Serialize(value));
    }

    /// <summary>
    /// Checks if the session has any context data stored.
    /// </summary>
    /// <param name="session">The user session to check.</param>
    /// <returns>True if context data exists; otherwise, false.</returns>
    public static bool HasContextData(this UserSession session)
    {
        return session.ContextData != null && session.ContextData.Count > 0;
    }

    /// <summary>
    /// Gets the number of active sessions that share the same user ID.
    /// </summary>
    /// <param name="session">The user session to check.</param>
    /// <param name="allSessions">Collection of all active sessions to search through.</param>
    /// <returns>The count of active sessions for this user.</returns>
    public static int GetActiveSessionCountForUser(this UserSession session, IEnumerable<UserSession> allSessions)
    {
        return allSessions.Count(s => s.UserId == session.UserId && s.IsActive);
    }

    /// <summary>
    /// Determines if this session is the most recently active session for the user.
    /// </summary>
    /// <param name="session">The user session to check.</param>
    /// <param name="allSessions">Collection of all active sessions to search through.</param>
    /// <returns>True if this is the most recent session for the user; otherwise, false.</returns>
    public static bool IsMostRecentSession(this UserSession session, IEnumerable<UserSession> allSessions)
    {
        var userSessions = allSessions.Where(s => s.UserId == session.UserId && s.IsActive).ToList();

        if (userSessions.Count == 0)
            return false;

        var mostRecent = userSessions.MaxBy(s => s.LastActivityAt ?? s.CreatedAt);
        return mostRecent?.SessionId == session.SessionId;
    }
}