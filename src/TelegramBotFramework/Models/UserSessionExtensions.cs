#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

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
    /// <exception cref="ArgumentNullException"><paramref name="session"/> is <see langword="null"/>.</exception>
    public static bool IsIdle(this UserSession session, TimeSpan? idleTimeout = null)
    {
        ArgumentNullException.ThrowIfNull(session);

        var timeout = idleTimeout ?? TimeSpan.FromMinutes(5);

        return session.State != SessionState.Active ||
               !session.LastActivityAt.HasValue ||
               DateTime.UtcNow - session.LastActivityAt.Value >= timeout;
    }

    /// <summary>
    /// Determines if the session is about to expire soon based on the expiration time.
    /// </summary>
    /// <param name="session">The user session to check.</param>
    /// <param name="warningThreshold">The time threshold before expiration to consider it "about to expire". Default is 1 hour.</param>
    /// <returns>True if the session will expire within the warning threshold; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="session"/> is <see langword="null"/>.</exception>
    public static bool IsAboutToExpire(this UserSession session, TimeSpan? warningThreshold = null)
    {
        ArgumentNullException.ThrowIfNull(session);

        var threshold = warningThreshold ?? TimeSpan.FromHours(1);

        return session.ExpiresAt.HasValue &&
               session.ExpiresAt.Value - DateTime.UtcNow <= threshold;
    }

    /// <summary>
    /// Gets the remaining time until session expiration.
    /// </summary>
    /// <param name="session">The user session to check.</param>
    /// <returns>The remaining time until expiration, or null if the session never expires.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="session"/> is <see langword="null"/>.</exception>
    public static TimeSpan? GetTimeUntilExpiration(this UserSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        return session.ExpiresAt.HasValue
            ? session.ExpiresAt.Value - DateTime.UtcNow
            : null;
    }

    /// <summary>
    /// Safely gets a typed value from context data.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the value to.</typeparam>
    /// <param name="session">The user session.</param>
    /// <param name="key">The context data key.</param>
    /// <param name="defaultValue">The default value to return if the key doesn't exist or parsing fails.</param>
    /// <returns>The parsed value or the default value.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="session"/> is <see langword="null"/>
    /// or <paramref name="key"/> is <see langword="null"/>.
    /// </exception>
    public static T? GetContextData<T>(this UserSession session, string key, T? defaultValue = default)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrEmpty(key);

        var value = session.GetContextData(key);

        return value == null
            ? defaultValue
            : TryDeserialize<T>(value) ?? defaultValue;
    }

    /// <summary>
    /// Safely sets a typed value in context data.
    /// </summary>
    /// <typeparam name="T">The type of the value to serialize.</typeparam>
    /// <param name="session">The user session.</param>
    /// <param name="key">The context data key.</param>
    /// <param name="value">The value to store.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="session"/> is <see langword="null"/>
    /// or <paramref name="key"/> is <see langword="null"/>.
    /// </exception>
    public static void SetContextData<T>(this UserSession session, string key, T? value)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrEmpty(key);

        if (value is null)
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
    /// <exception cref="ArgumentNullException"><paramref name="session"/> is <see langword="null"/>.</exception>
    public static bool HasContextData(this UserSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        return session.ContextData is { Count: > 0 };
    }

    /// <summary>
    /// Gets the number of active sessions that share the same user ID.
    /// </summary>
    /// <param name="session">The user session to check.</param>
    /// <param name="allSessions">Collection of all active sessions to search through.</param>
    /// <returns>The count of active sessions for this user.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="session"/> is <see langword="null"/>
    /// or <paramref name="allSessions"/> is <see langword="null"/>.
    /// </exception>
    public static int GetActiveSessionCountForUser(this UserSession session, IEnumerable<UserSession> allSessions)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(allSessions);

        return allSessions.Count(s => s.UserId == session.UserId && s.IsActive);
    }

    /// <summary>
    /// Determines if this session is the most recently active session for the user.
    /// </summary>
    /// <param name="session">The user session to check.</param>
    /// <param name="allSessions">Collection of all active sessions to search through.</param>
    /// <returns>True if this is the most recent session for the user; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="session"/> is <see langword="null"/>
    /// or <paramref name="allSessions"/> is <see langword="null"/>.
    /// </exception>
    public static bool IsMostRecentSession(this UserSession session, IEnumerable<UserSession> allSessions)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(allSessions);

        var userSessions = allSessions.Where(s => s.UserId == session.UserId && s.IsActive).ToList();

        return userSessions.Count > 0 &&
               userSessions.MaxBy(s => s.LastActivityAt ?? s.CreatedAt)?.SessionId == session.SessionId;
    }

    private static T? TryDeserialize<T>(string value)
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(value);
        }
        catch
        {
            return default;
        }
    }
}