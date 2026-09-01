#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TelegramBotFramework.Models;
using TelegramBotFramework.Repositories;
using TelegramBotFramework.Services;
using Xunit;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Unit tests for <see cref="SessionService"/> which provides session management functionality
/// for tracking user interactions and maintaining conversation state in a Telegram bot framework.
/// </summary>
public sealed class SessionServiceTests : ISessionServiceTests
{
    /// <summary>
    /// Mock repository for testing session persistence operations.
    /// </summary>
    private readonly Mock<ISessionRepository> _mockSessionRepository = new();

    /// <summary>
    /// Mock logger for verifying logging behavior during session operations.
    /// </summary>
    private readonly Mock<ILogger<SessionService>> _mockLogger = new();

    /// <summary>
    /// Mock logger for test execution tracing.
    /// </summary>
    private readonly Mock<ILogger<SessionServiceTests>> _testLogger = new();

    /// <summary>
    /// Instance of the service under test with mocked dependencies.
    /// </summary>
    private readonly SessionService _sessionService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionServiceTests"/> class.
    /// Sets up the mocked dependencies and creates the service instance under test.
    /// </summary>
    public SessionServiceTests()
    {
        _testLogger.Object.LogInformation("SessionServiceTests instance created");
        _sessionService = new SessionService(_mockSessionRepository.Object, _mockLogger.Object);
    }

    /// <summary>
    /// Tests that <see cref="SessionService.GetActiveSessionAsync"/> returns an active session
    /// when one exists for the specified user ID.
    /// </summary>
    [Fact]
    public async Task GetActiveSessionAsync_WithExistingActiveSession_ReturnsSession()
    {
        _testLogger.Object.LogInformation("GetActiveSessionAsync_WithExistingActiveSession_ReturnsSession started");
        _mockLogger.Object.LogInformation("GetActiveSessionAsync_WithExistingActiveSession_ReturnsSession called with {UserId}", SessionServiceTestsConstants.TestUserId);

        // Arrange
        var session = new UserSession
        {
            SessionId = SessionServiceTestsConstants.TestSessionId,
            UserId = SessionServiceTestsConstants.TestUserId,
            ChatId = SessionServiceTestsConstants.TestChatId,
            IsActive = true,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _mockSessionRepository
            .Setup(r => r.GetActiveSessionAsync(SessionServiceTestsConstants.TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await _sessionService.GetActiveSessionAsync(123).ConfigureAwait(false);

        // Assert
        result.Should().Be(session);
        result!.IsActive.Should().BeTrue();
        _mockLogger.Object.LogInformation("GetActiveSessionAsync_WithExistingActiveSession_ReturnsSession completed with {SessionId}", result.SessionId);
        _testLogger.Object.LogInformation("GetActiveSessionAsync_WithExistingActiveSession_ReturnsSession completed");
    }

    /// <summary>
    /// Tests that <see cref="SessionService.GetActiveSessionAsync"/> returns null
    /// when no active session exists for the specified user ID.
    /// </summary>
    [Fact]
    public async Task GetActiveSessionAsync_WithNoActiveSession_ReturnsNull()
    {
        _testLogger.Object.LogInformation("GetActiveSessionAsync_WithNoActiveSession_ReturnsNull started");
        _mockLogger.Object.LogInformation("GetActiveSessionAsync_WithNoActiveSession_ReturnsNull called");

        // Arrange
        _mockSessionRepository
            .Setup(r => r.GetActiveSessionAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserSession?)null);

        // Act
        var result = await _sessionService.GetActiveSessionAsync(It.IsAny<long>()).ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
        _mockLogger.Object.LogWarning("GetActiveSessionAsync_WithNoActiveSession_ReturnsNull completed without an active session");
        _mockLogger.Object.LogInformation("GetActiveSessionAsync_WithNoActiveSession_ReturnsNull completed");
        _testLogger.Object.LogInformation("GetActiveSessionAsync_WithNoActiveSession_ReturnsNull completed");
    }

    /// <summary>
    /// Tests that <see cref="SessionService.CreateSessionAsync"/> creates a new session
    /// with the specified user ID and chat ID.
    /// </summary>
    [Fact]
    public async Task CreateSessionAsync_CreatesNewSession()
    {
        _testLogger.Object.LogInformation("CreateSessionAsync_CreatesNewSession started");
        _mockLogger.Object.LogInformation("CreateSessionAsync_CreatesNewSession called with {UserId} and {ChatId}", SessionServiceTestsConstants.TestUserId, SessionServiceTestsConstants.TestChatId);

        // Arrange
        _mockSessionRepository
            .Setup(r => r.CreateAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserSession s, CancellationToken _) => s);

        // Act
        var result = await _sessionService.CreateSessionAsync(SessionServiceTestsConstants.TestUserId, SessionServiceTestsConstants.TestChatId).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(SessionServiceTestsConstants.TestUserId);
        result.ChatId.Should().Be(SessionServiceTestsConstants.TestChatId);
        result.IsActive.Should().BeTrue();
        result.SessionId.Should().StartWith(SessionServiceTestsConstants.SessionIdPrefix);
        _mockSessionRepository.Verify(r => r.CreateAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Object.LogInformation("CreateSessionAsync_CreatesNewSession completed with {SessionId}", result.SessionId);
        _testLogger.Object.LogInformation("CreateSessionAsync_CreatesNewSession completed");
    }

    /// <summary>
    /// Tests that <see cref="SessionService.CreateSessionAsync"/> with custom timeout
    /// creates a session with the correct expiration time.
    /// </summary>
    [Fact]
    public async Task CreateSessionAsync_WithCustomTimeout_CreatesSessionWithCorrectExpiration()
    {
        _testLogger.Object.LogInformation("CreateSessionAsync_WithCustomTimeout_CreatesSessionWithCorrectExpiration started");
        _mockLogger.Object.LogInformation("CreateSessionAsync_WithCustomTimeout_CreatesSessionWithCorrectExpiration called with {UserId}, {ChatId}, and {Timeout}", SessionServiceTestsConstants.TestUserId, SessionServiceTestsConstants.TestChatId, SessionServiceTestsConstants.OneHourTimeout);

        // Arrange
        var newSession = new UserSession
        {
            SessionId = "new-session-123",
            UserId = SessionServiceTestsConstants.TestUserId,
            ChatId = SessionServiceTestsConstants.TestChatId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };

        _mockSessionRepository
            .Setup(r => r.CreateAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newSession);

        // Act
        var result = await _sessionService.CreateSessionAsync(SessionServiceTestsConstants.TestUserId, SessionServiceTestsConstants.TestChatId, SessionServiceTestsConstants.OneHourTimeout).ConfigureAwait(false);

        // Assert
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(1), TimeSpan.FromSeconds(5));
        _mockLogger.Object.LogInformation("CreateSessionAsync_WithCustomTimeout_CreatesSessionWithCorrectExpiration completed with {ExpiresAt}", result.ExpiresAt);
        _testLogger.Object.LogInformation("CreateSessionAsync_WithCustomTimeout_CreatesSessionWithCorrectExpiration completed");
    }

    /// <summary>
    /// Tests that <see cref="SessionService.RecordSessionActivityAsync"/> updates the last activity timestamp
    /// and increments the interaction count for the specified session.
    /// </summary>
    [Fact]
    public async Task RecordSessionActivityAsync_UpdatesLastActivityAndIncrementsInteractionCount()
    {
        _testLogger.Object.LogInformation("RecordSessionActivityAsync_UpdatesLastActivityAndIncrementsInteractionCount started");
        _mockLogger.Object.LogInformation("RecordSessionActivityAsync_UpdatesLastActivityAndIncrementsInteractionCount called with {SessionId}", SessionServiceTestsConstants.TestSessionId);

        // Arrange
        var session = new UserSession
        {
            SessionId = SessionServiceTestsConstants.TestSessionId,
            UserId = SessionServiceTestsConstants.TestUserId,
            ChatId = SessionServiceTestsConstants.TestChatId,
            IsActive = true,
            InteractionCount = SessionServiceTestsConstants.InitialInteractionCount
        };
        var updatedSession = new UserSession
        {
            SessionId = SessionServiceTestsConstants.TestSessionId,
            UserId = SessionServiceTestsConstants.TestUserId,
            ChatId = SessionServiceTestsConstants.TestChatId,
            IsActive = true,
            InteractionCount = SessionServiceTestsConstants.UpdatedInteractionCount,
            LastActivityAt = DateTime.UtcNow
        };

        _mockSessionRepository
            .Setup(r => r.GetByIdAsync(SessionServiceTestsConstants.TestSessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _mockSessionRepository
            .Setup(r => r.UpdateAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedSession);

        // Act
        await _sessionService.RecordSessionActivityAsync("session-123").ConfigureAwait(false);

        // Assert
        _mockSessionRepository.Verify(r => r.UpdateAsync(It.Is<UserSession>(s =>
            s.InteractionCount == SessionServiceTestsConstants.UpdatedInteractionCount &&
            s.LastActivityAt.HasValue
        ), It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Object.LogInformation("RecordSessionActivityAsync_UpdatesLastActivityAndIncrementsInteractionCount completed with {SessionId}", SessionServiceTestsConstants.TestSessionId);
        _testLogger.Object.LogInformation("RecordSessionActivityAsync_UpdatesLastActivityAndIncrementsInteractionCount completed");
    }

    /// <summary>
    /// Tests that <see cref="SessionService.RecordSessionActivityAsync"/> does not throw
    /// when attempting to record activity for a non-existent session.
    /// </summary>
    [Fact]
    public async Task RecordSessionActivityAsync_WithNonExistingSession_DoesNotThrow()
    {
        _testLogger.Object.LogInformation("RecordSessionActivityAsync_WithNonExistingSession_DoesNotThrow started");
        _mockLogger.Object.LogInformation("RecordSessionActivityAsync_WithNonExistingSession_DoesNotThrow called with {SessionId}", SessionServiceTestsConstants.NonExistentSessionId);

        // Arrange
        _mockSessionRepository
            .Setup(r => r.GetByIdAsync(SessionServiceTestsConstants.NonExistentSessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserSession?)null);

        // Act & Assert
        await _sessionService.Invoking(s => s.RecordSessionActivityAsync("nonexistent"))
            .Should().NotThrowAsync();
        _mockLogger.Object.LogWarning("RecordSessionActivityAsync_WithNonExistingSession_DoesNotThrow completed without a session for {SessionId}", SessionServiceTestsConstants.NonExistentSessionId);
        _mockLogger.Object.LogInformation("RecordSessionActivityAsync_WithNonExistingSession_DoesNotThrow completed with {SessionId}", SessionServiceTestsConstants.NonExistentSessionId);
        _testLogger.Object.LogInformation("RecordSessionActivityAsync_WithNonExistingSession_DoesNotThrow completed");
    }

    /// <summary>
    /// Tests that <see cref="SessionService.CloseSessionAsync"/> closes an active session and returns true
    /// when the session exists and is active.
    /// </summary>
    [Fact]
    public async Task CloseSessionAsync_WithActiveSession_ClosesSessionAndReturnsTrue()
    {
        _testLogger.Object.LogInformation("CloseSessionAsync_WithActiveSession_ClosesSessionAndReturnsTrue started");
        _mockLogger.Object.LogInformation("CloseSessionAsync_WithActiveSession_ClosesSessionAndReturnsTrue called with {SessionId}", SessionServiceTestsConstants.TestSessionId);

        // Arrange
        var session = new UserSession
        {
            SessionId = "session-123",
            UserId = 123,
            ChatId = 456,
            IsActive = true
        };

        _mockSessionRepository
            .Setup(r => r.GetByIdAsync("session-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _mockSessionRepository
            .Setup(r => r.UpdateAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await _sessionService.CloseSessionAsync("session-123").ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();
        session.IsActive.Should().BeFalse();
        _mockSessionRepository.Verify(r => r.UpdateAsync(It.Is<UserSession>(s => !s.IsActive), It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Object.LogInformation("CloseSessionAsync_WithActiveSession_ClosesSessionAndReturnsTrue completed with {SessionId}", SessionServiceTestsConstants.TestSessionId);
        _testLogger.Object.LogInformation("CloseSessionAsync_WithActiveSession_ClosesSessionAndReturnsTrue completed");
    }

    /// <summary>
    /// Tests that <see cref="SessionService.CloseSessionAsync"/> returns false
    /// when attempting to close an already closed session.
    /// </summary>
    [Fact]
    public async Task CloseSessionAsync_WithAlreadyClosedSession_ReturnsFalse()
    {
        _testLogger.Object.LogInformation("CloseSessionAsync_WithAlreadyClosedSession_ReturnsFalse started");
        _mockLogger.Object.LogInformation("CloseSessionAsync_WithAlreadyClosedSession_ReturnsFalse called with {SessionId}", SessionServiceTestsConstants.TestSessionId);

        // Arrange
        var session = new UserSession
        {
            SessionId = "session-123",
            UserId = 123,
            ChatId = 456,
            IsActive = false
        };

        _mockSessionRepository
            .Setup(r => r.GetByIdAsync("session-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await _sessionService.CloseSessionAsync("session-123").ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
        _mockLogger.Object.LogWarning("CloseSessionAsync_WithAlreadyClosedSession_ReturnsFalse skipped already closed {SessionId}", SessionServiceTestsConstants.TestSessionId);
        _mockLogger.Object.LogInformation("CloseSessionAsync_WithAlreadyClosedSession_ReturnsFalse completed with {SessionId}", SessionServiceTestsConstants.TestSessionId);
        _testLogger.Object.LogInformation("CloseSessionAsync_WithAlreadyClosedSession_ReturnsFalse completed");
    }

    /// <summary>
    /// Tests that <see cref="SessionService.CloseSessionAsync"/> returns false
    /// when attempting to close a non-existent session.
    /// </summary>
    [Fact]
    public async Task CloseSessionAsync_WithNonExistingSession_ReturnsFalse()
    {
        _mockLogger.Object.LogInformation("CloseSessionAsync_WithNonExistingSession_ReturnsFalse called with {SessionId}", SessionServiceTestsConstants.NonExistentSessionId);

        // Arrange
        _mockSessionRepository
            .Setup(r => r.GetByIdAsync(SessionServiceTestsConstants.NonExistentSessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserSession?)null);

        // Act
        var result = await _sessionService.CloseSessionAsync("nonexistent").ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
        _mockLogger.Object.LogWarning("CloseSessionAsync_WithNonExistingSession_ReturnsFalse completed without a session for {SessionId}", SessionServiceTestsConstants.NonExistentSessionId);
        _mockLogger.Object.LogInformation("CloseSessionAsync_WithNonExistingSession_ReturnsFalse completed with {SessionId}", SessionServiceTestsConstants.NonExistentSessionId);
    }

    /// <summary>
    /// Tests that <see cref="SessionService.NavigateToMenuAsync"/> updates the CurrentMenuId
    /// for the specified session to the new menu identifier.
    /// </summary>
    /// <param name="sessionId">The session identifier to update.</param>
    /// <param name="menuId">The new menu identifier to set as current.</param>
    [Fact]
    public async Task NavigateToMenuAsync_UpdatesCurrentMenuId()
    {
        _mockLogger.Object.LogInformation("NavigateToMenuAsync_UpdatesCurrentMenuId called with {SessionId} and {MenuId}", SessionServiceTestsConstants.TestSessionId, SessionServiceTestsConstants.NewMenuId);

        // Arrange
        var session = new UserSession
        {
            SessionId = SessionServiceTestsConstants.TestSessionId,
            UserId = SessionServiceTestsConstants.TestUserId,
            ChatId = SessionServiceTestsConstants.TestChatId,
            IsActive = true,
            CurrentMenuId = SessionServiceTestsConstants.OldMenuId
        };
        var updatedSession = new UserSession
        {
            SessionId = SessionServiceTestsConstants.TestSessionId,
            UserId = SessionServiceTestsConstants.TestUserId,
            ChatId = SessionServiceTestsConstants.TestChatId,
            IsActive = true,
            CurrentMenuId = SessionServiceTestsConstants.NewMenuId
        };

        _mockSessionRepository
            .Setup(r => r.GetByIdAsync("session-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _mockSessionRepository
            .Setup(r => r.UpdateAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedSession);

        // Act
        await _sessionService.NavigateToMenuAsync(SessionServiceTestsConstants.TestSessionId, SessionServiceTestsConstants.NewMenuId).ConfigureAwait(false);

        // Assert
        _mockSessionRepository.Verify(r => r.UpdateAsync(It.Is<UserSession>(s =>
            s.CurrentMenuId == SessionServiceTestsConstants.NewMenuId
        ), It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Object.LogInformation("NavigateToMenuAsync_UpdatesCurrentMenuId completed with {SessionId} and {MenuId}", SessionServiceTestsConstants.TestSessionId, SessionServiceTestsConstants.NewMenuId);
    }

    /// <summary>
    /// Tests that <see cref="SessionService.GetSessionByIdAsync"/> returns the session
    /// when a session with the specified identifier exists.
    /// </summary>
    /// <param name="sessionId">The session identifier to retrieve.</param>
    /// <returns>The <see cref="UserSession"/> if found, otherwise null.</returns>
    [Fact]
    public async Task GetSessionByIdAsync_WithExistingSession_ReturnsSession()
    {
        _mockLogger.Object.LogInformation("GetSessionByIdAsync_WithExistingSession_ReturnsSession called with {SessionId}", SessionServiceTestsConstants.TestSessionId);

        // Arrange
        var session = new UserSession
        {
            SessionId = SessionServiceTestsConstants.TestSessionId,
            UserId = SessionServiceTestsConstants.TestUserId,
            ChatId = SessionServiceTestsConstants.TestChatId
        };

        _mockSessionRepository
            .Setup(r => r.GetByIdAsync("session-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await _sessionService.GetSessionByIdAsync("session-123").ConfigureAwait(false);

        // Assert
        result.Should().Be(session);
        _mockLogger.Object.LogInformation("GetSessionByIdAsync_WithExistingSession_ReturnsSession completed with {SessionId}", result!.SessionId);
    }

    /// <summary>
    /// Tests that <see cref="SessionService.GetSessionByIdAsync"/> returns null
    /// when no session exists with the specified identifier.
    /// </summary>
    /// <param name="sessionId">The session identifier to search for.</param>
    /// <returns>Null if the session does not exist.</returns>
    [Fact]
    public async Task GetSessionByIdAsync_WithNonExistingSession_ReturnsNull()
    {
        _mockLogger.Object.LogInformation("GetSessionByIdAsync_WithNonExistingSession_ReturnsNull called with {SessionId}", SessionServiceTestsConstants.NonExistentSessionId);

        // Arrange
        _mockSessionRepository
            .Setup(r => r.GetByIdAsync(SessionServiceTestsConstants.NonExistentSessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserSession?)null);

        // Act
        var result = await _sessionService.GetSessionByIdAsync("nonexistent").ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
        _mockLogger.Object.LogWarning("GetSessionByIdAsync_WithNonExistingSession_ReturnsNull completed without a session for {SessionId}", SessionServiceTestsConstants.NonExistentSessionId);
        _mockLogger.Object.LogInformation("GetSessionByIdAsync_WithNonExistingSession_ReturnsNull completed with {SessionId}", SessionServiceTestsConstants.NonExistentSessionId);
    }

    /// <summary>
    /// Tests that <see cref="SessionService.GetAllActiveSessionsAsync"/> returns all active sessions
    /// from the repository.
    /// </summary>
    /// <returns>A collection of active <see cref="UserSession"/> objects.</returns>
    [Fact]
    public async Task GetAllActiveSessionsAsync_ReturnsActiveSessions()
    {
        _mockLogger.Object.LogInformation("GetAllActiveSessionsAsync_ReturnsActiveSessions called");

        // Arrange
        var activeSessions = new List<UserSession>
        {
            new UserSession { SessionId = SessionServiceTestsConstants.SessionId1, UserId = 1, IsActive = true },
            new UserSession { SessionId = SessionServiceTestsConstants.SessionId2, UserId = 2, IsActive = true }
        };

        _mockSessionRepository
            .Setup(r => r.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeSessions);

        // Act
        var result = await _sessionService.GetAllActiveSessionsAsync().ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(s => s.IsActive.Should().BeTrue());
        _mockLogger.Object.LogInformation("GetAllActiveSessionsAsync_ReturnsActiveSessions completed with {SessionCount} sessions", result.Count());
    }

    /// <summary>
    /// Tests that <see cref="SessionService.GetSessionsByUserIdAsync"/> returns all sessions
    /// for the specified user ID.
    /// </summary>
    /// <param name="userId">The user identifier to filter sessions by.</param>
    /// <returns>A collection of <see cref="UserSession"/> objects for the specified user.</returns>
    [Fact]
    public async Task GetSessionsByUserIdAsync_ReturnsUserSessions()
    {
        _mockLogger.Object.LogInformation("GetSessionsByUserIdAsync_ReturnsUserSessions called with {UserId}", SessionServiceTestsConstants.TestUserId);

        // Arrange
        var userSessions = new List<UserSession>
        {
            new UserSession { SessionId = SessionServiceTestsConstants.SessionId1, UserId = SessionServiceTestsConstants.TestUserId, IsActive = true },
            new UserSession { SessionId = SessionServiceTestsConstants.SessionId2, UserId = SessionServiceTestsConstants.TestUserId, IsActive = false }
        };

        _mockSessionRepository
            .Setup(r => r.GetByUserIdAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userSessions);

        // Act
        var result = await _sessionService.GetSessionsByUserIdAsync(123).ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(s => s.UserId.Should().Be(123));
        _mockLogger.Object.LogInformation("GetSessionsByUserIdAsync_ReturnsUserSessions completed with {UserId} and {SessionCount} sessions", SessionServiceTestsConstants.TestUserId, result.Count());
    }

    /// <summary>
    /// Tests that <see cref="SessionService.DeleteSessionAsync"/> deletes the session and returns true
    /// when the session exists and is successfully deleted.
    /// </summary>
    /// <param name="sessionId">The session identifier to delete.</param>
    /// <returns>True if the session was deleted, otherwise false.</returns>
    [Fact]
    public async Task DeleteSessionAsync_WithExistingSession_DeletesAndReturnsTrue()
    {
        _mockLogger.Object.LogInformation("DeleteSessionAsync_WithExistingSession_DeletesAndReturnsTrue called with {SessionId}", SessionServiceTestsConstants.TestSessionId);

        // Arrange
        _mockSessionRepository
            .Setup(r => r.DeleteAsync("session-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sessionService.DeleteSessionAsync("session-123").ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();
        _mockSessionRepository.Verify(r => r.DeleteAsync("session-123", It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Object.LogInformation("DeleteSessionAsync_WithExistingSession_DeletesAndReturnsTrue completed with {SessionId}", SessionServiceTestsConstants.TestSessionId);
    }

    /// <summary>
    /// Tests that <see cref="SessionService.DeleteSessionAsync"/> returns false
    /// when attempting to delete a non-existent session.
    /// </summary>
    /// <param name="sessionId">The session identifier that does not exist.</param>
    /// <returns>False if the session does not exist.</returns>
    [Fact]
    public async Task DeleteSessionAsync_WithNonExistingSession_ReturnsFalse()
    {
        _mockLogger.Object.LogInformation("DeleteSessionAsync_WithNonExistingSession_ReturnsFalse called with {SessionId}", SessionServiceTestsConstants.NonExistentSessionId);

        // Arrange
        _mockSessionRepository
            .Setup(r => r.DeleteAsync("nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sessionService.DeleteSessionAsync("nonexistent").ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
        _mockLogger.Object.LogWarning("DeleteSessionAsync_WithNonExistingSession_ReturnsFalse completed without deleting {SessionId}", SessionServiceTestsConstants.NonExistentSessionId);
        _mockLogger.Object.LogInformation("DeleteSessionAsync_WithNonExistingSession_ReturnsFalse completed with {SessionId}", SessionServiceTestsConstants.NonExistentSessionId);
    }

    /// <summary>
    /// Tests that <see cref="SessionService.ExpireInactiveSessionsAsync"/> closes and expires sessions
    /// that have had no activity beyond the specified timeout period.
    /// </summary>
    /// <param name="timeout">The maximum inactivity period before a session should be expired.</param>
    /// <returns>The number of sessions that were expired and closed.</returns>
    [Fact]
    public async Task ExpireInactiveSessionsAsync_WithInactiveSessions_ClosesThem()
    {
        _mockLogger.Object.LogInformation("ExpireInactiveSessionsAsync_WithInactiveSessions_ClosesThem called with {Timeout}", SessionServiceTestsConstants.TwentyFourHoursTimeout);

        // Arrange
        var sessions = new List<UserSession>
        {
            new UserSession { SessionId = SessionServiceTestsConstants.SessionId1, UserId = 1, IsActive = true, LastActivityAt = DateTime.UtcNow.AddDays(-1) },
            new UserSession { SessionId = SessionServiceTestsConstants.SessionId2, UserId = 2, IsActive = true, LastActivityAt = DateTime.UtcNow.AddDays(-2) },
            new UserSession { SessionId = SessionServiceTestsConstants.SessionId3, UserId = 3, IsActive = false }
        };

        _mockSessionRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions);
        _mockSessionRepository
            .Setup(r => r.UpdateAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserSession s, CancellationToken _) => s);

        // Act
        var result = await _sessionService.ExpireInactiveSessionsAsync(SessionServiceTestsConstants.TwentyFourHoursTimeout).ConfigureAwait(false);

        // Assert
        result.Should().Be(2);
        _mockSessionRepository.Verify(r => r.UpdateAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _mockLogger.Object.LogInformation("ExpireInactiveSessionsAsync_WithInactiveSessions_ClosesThem completed with {ExpiredSessionCount} sessions", result);
    }

    /// <summary>
    /// Tests that <see cref="SessionService.PruneExpiredSessions"/> prunes all sessions that have already expired
    /// based on their ExpiresAt timestamp and returns the count of pruned sessions.
    /// </summary>
    /// <returns>The number of sessions that were pruned.</returns>
    [Fact]
    public async Task PruneExpiredSessions_WithExpiredSessions_PrunesThemAndReturnsCount()
    {
        _mockLogger.Object.LogInformation("PruneExpiredSessions_WithExpiredSessions_PrunesThemAndReturnsCount called");

        // Arrange
        var now = DateTime.UtcNow;
        var sessions = new List<UserSession>
        {
            new UserSession { SessionId = SessionServiceTestsConstants.SessionId1, UserId = 1, IsActive = true, ExpiresAt = now.AddDays(-1) }, // Expired
            new UserSession { SessionId = SessionServiceTestsConstants.SessionId2, UserId = 2, IsActive = true, ExpiresAt = now.AddDays(1) }, // Not expired
            new UserSession { SessionId = SessionServiceTestsConstants.SessionId3, UserId = 3, IsActive = true, ExpiresAt = now.AddDays(-2) } // Expired
        };

        _mockSessionRepository
            .Setup(r => r.GetExpiredAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions.Where(s => s.IsExpired()).ToList());
        _mockSessionRepository
            .Setup(r => r.UpdateAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserSession s, CancellationToken _) => s);

        // Act
        var result = await _sessionService.PruneExpiredSessions().ConfigureAwait(false);

        // Assert
        result.Should().Be(2);
        _mockSessionRepository.Verify(r => r.UpdateAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _mockSessionRepository.Verify(r => r.GetExpiredAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Object.LogInformation("PruneExpiredSessions_WithExpiredSessions_PrunesThemAndReturnsCount completed with {PrunedSessionCount} sessions", result);
    }

    /// <summary>
    /// Tests that <see cref="SessionService.PruneExpiredSessions"/> returns 0 when there are no expired sessions.
    /// </summary>
    /// <returns>The number of sessions pruned (should be 0).</returns>
    [Fact]
    public async Task PruneExpiredSessions_WithNoExpiredSessions_ReturnsZero()
    {
        _mockLogger.Object.LogInformation("PruneExpiredSessions_WithNoExpiredSessions_ReturnsZero called");

        // Arrange
        var sessions = new List<UserSession>
        {
            new UserSession { SessionId = "session-1", UserId = 1, IsActive = true, ExpiresAt = DateTime.UtcNow.AddDays(1) },
            new UserSession { SessionId = "session-2", UserId = 2, IsActive = true, ExpiresAt = DateTime.UtcNow.AddDays(2) }
        };

        _mockSessionRepository
            .Setup(r => r.GetExpiredAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserSession>());

        // Act
        var result = await _sessionService.PruneExpiredSessions().ConfigureAwait(false);

        // Assert
        result.Should().Be(0);
        _mockSessionRepository.Verify(r => r.UpdateAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Object.LogWarning("PruneExpiredSessions_WithNoExpiredSessions_ReturnsZero completed without expired sessions");
        _mockLogger.Object.LogInformation("PruneExpiredSessions_WithNoExpiredSessions_ReturnsZero completed with {PrunedSessionCount} sessions", result);
    }
}
