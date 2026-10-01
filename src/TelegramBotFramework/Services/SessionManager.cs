#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;

namespace TelegramBotFramework.Services;

/// <summary>
/// Defines the contract for managing user sessions with lifecycle operations.
/// </summary>
public interface ISessionManager : IDisposable
{
    /// <summary>
    /// Gets or creates a session for the given user and chat.
    /// </summary>
    Task<Models.UserSession> AcquireSessionAsync(long userId, long chatId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases a session, marking it as inactive after the current request completes.
    /// </summary>
    Task ReleaseSessionAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Expires all sessions that have been inactive longer than the configured threshold.
    /// </summary>
    /// <returns>The number of expired sessions.</returns>
    Task<int> ExpireInactiveSessionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of currently active sessions.
    /// </summary>
    int ActiveSessionCount { get; }
}

/// <summary>
/// Manages user session lifecycles including creation, reuse, expiration and cleanup.
/// Wraps <see cref="ISessionService"/> with an in-memory activity tracker for fast
/// duplicate-session prevention and inactivity detection.
/// </summary>
public sealed class SessionManager : ISessionManager
{
    private readonly ISessionService _sessionService;
    private readonly ILogger<SessionManager> _logger;
    private readonly TimeSpan _inactivityThreshold;
    private readonly ConcurrentDictionary<long, string> _activeUserSessions = new();
    private readonly ConcurrentDictionary<string, DateTime> _lastActivity = new();
    private bool _disposed;

    /// <inheritdoc />
    public int ActiveSessionCount => _activeUserSessions.Count;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionManager"/> class.
    /// </summary>
    /// <param name="sessionService">The underlying session service.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="inactivityThreshold">
    /// How long a session may be idle before it is eligible for expiration.
    /// Defaults to 30 minutes when <see langword="null"/>.
    /// </param>
    public SessionManager(
        ISessionService sessionService,
        ILogger<SessionManager> logger,
        TimeSpan? inactivityThreshold = null)
    {
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _inactivityThreshold = inactivityThreshold ?? TimeSpan.FromMinutes(30);
    }

    /// <inheritdoc />
    public async Task<Models.UserSession> AcquireSessionAsync(long userId, long chatId, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_activeUserSessions.TryGetValue(userId, out var existingSessionId))
        {
            var existing = await _sessionService.GetSessionAsync(existingSessionId, cancellationToken).ConfigureAwait(false);
            if (existing is not null)
            {
                _lastActivity[existingSessionId] = DateTime.UtcNow;
                await _sessionService.RecordSessionActivityAsync(existingSessionId, cancellationToken).ConfigureAwait(false);
                _logger.LogDebug("Reusing session {SessionId} for user {UserId}", existingSessionId, userId);
                return existing;
            }

            _activeUserSessions.TryRemove(userId, out _);
            _lastActivity.TryRemove(existingSessionId, out _);
        }

        var session = await _sessionService.GetActiveSessionAsync(userId, cancellationToken).ConfigureAwait(false)
                      ?? await _sessionService.CreateSessionAsync(userId, chatId, cancellationToken).ConfigureAwait(false);

        _activeUserSessions[userId] = session.SessionId;
        _lastActivity[session.SessionId] = DateTime.UtcNow;

        _logger.LogInformation("Acquired session {SessionId} for user {UserId}", session.SessionId, userId);
        return session;
    }

    /// <inheritdoc />
    public async Task ReleaseSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);

        var session = await _sessionService.GetSessionAsync(sessionId, cancellationToken).ConfigureAwait(false);
        if (session is null)
        {
            _logger.LogWarning("Attempted to release unknown session {SessionId}", sessionId);
            return;
        }

        _activeUserSessions.TryRemove(session.UserId, out _);
        _lastActivity.TryRemove(sessionId, out _);

        await _sessionService.CloseSessionAsync(sessionId, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Released session {SessionId} for user {UserId}", sessionId, session.UserId);
    }

    /// <inheritdoc />
    public async Task<int> ExpireInactiveSessionsAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var cutoff = DateTime.UtcNow - _inactivityThreshold;
        var expiredIds = _lastActivity
            .Where(kvp => kvp.Value < cutoff)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var sessionId in expiredIds)
        {
            _lastActivity.TryRemove(sessionId, out _);
        }

        var keysToRemove = _activeUserSessions
            .Where(kvp => expiredIds.Contains(kvp.Value))
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var userId in keysToRemove)
        {
            _activeUserSessions.TryRemove(userId, out _);
        }

        var expired = await _sessionService.ExpireInactiveSessionsAsync(_inactivityThreshold, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Expired {Count} inactive sessions (threshold: {Threshold})", expired, _inactivityThreshold);
        return expired;
    }

    /// <summary>
    /// Releases managed resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _activeUserSessions.Clear();
        _lastActivity.Clear();
    }
}
