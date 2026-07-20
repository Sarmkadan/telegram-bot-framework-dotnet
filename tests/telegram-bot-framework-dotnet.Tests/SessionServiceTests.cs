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
public sealed class SessionServiceTests
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
    /// Instance of the service under test with mocked dependencies.
    /// </summary>
    private readonly SessionService _sessionService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionServiceTests"/> class.
    /// Sets up the mocked dependencies and creates the service instance under test.
    /// </summary>
    public SessionServiceTests()
    {
        _sessionService = new SessionService(_mockSessionRepository.Object, _mockLogger.Object);
    }

    /// <summary>
    /// Tests that <see cref="SessionService.GetActiveSessionAsync"/> returns an active session
    /// when one exists for the specified user ID.
    /// </summary>
    [Fact]
    public async Task GetActiveSessionAsync_WithExistingActiveSession_ReturnsSession()
    {
        // Arrange
        var session = new UserSession
        {
            SessionId = "session-123",
            UserId = 123,
            ChatId = 456,
            IsActive = true,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _mockSessionRepository
            .Setup(r => r.GetActiveSessionAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await _sessionService.GetActiveSessionAsync(123).ConfigureAwait(false);

        // Assert
        result.Should().Be(session);
        result!.IsActive.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="SessionService.GetActiveSessionAsync"/> returns null
    /// when no active session exists for the specified user ID.
    /// </summary>
    [Fact]
    public async Task GetActiveSessionAsync_WithNoActiveSession_ReturnsNull()
    {
        // Arrange
        _mockSessionRepository
            .Setup(r => r.GetActiveSessionAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserSession?)null);

        // Act
        var result = await _sessionService.GetActiveSessionAsync(999).ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that <see cref="SessionService.CreateSessionAsync"/> creates a new session
    /// with the specified user ID and chat ID.
    /// </summary>
    [Fact]
    public async Task CreateSessionAsync_CreatesNewSession()
    {
        // Arrange
        _mockSessionRepository
            .Setup(r => r.CreateAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserSession s, CancellationToken _) => s);

        // Act
        var result = await _sessionService.CreateSessionAsync(123, 456).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(123);
        result.ChatId.Should().Be(456);
        result.IsActive.Should().BeTrue();
        result.SessionId.Should().StartWith("session_");
        _mockSessionRepository.Verify(r => r.CreateAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="SessionService.CreateSessionAsync"/> with custom timeout
    /// creates a session with the correct expiration time.
    /// </summary>
    [Fact]
    public async Task CreateSessionAsync_WithCustomTimeout_CreatesSessionWithCorrectExpiration()
    {
        // Arrange
        var newSession = new UserSession
        {
            SessionId = "new-session-123",
            UserId = 123,
            ChatId = 456,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };

        _mockSessionRepository
            .Setup(r => r.CreateAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newSession);

        // Act
        var result = await _sessionService.CreateSessionAsync(123, 456, TimeSpan.FromHours(1)).ConfigureAwait(false);

        // Assert
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(1), TimeSpan.FromSeconds(5));
    }

    /// <summary>
    /// Tests that <see cref="SessionService.RecordSessionActivityAsync"/> updates the last activity timestamp
    /// and increments the interaction count for the specified session.
    /// </summary>
    [Fact]
    public async Task RecordSessionActivityAsync_UpdatesLastActivityAndIncrementsInteractionCount()
    {
        // Arrange
        var session = new UserSession
        {
            SessionId = "session-123",
            UserId = 123,
            ChatId = 456,
            IsActive = true,
            InteractionCount = 5
        };
        var updatedSession = new UserSession
        {
            SessionId = "session-123",
            UserId = 123,
            ChatId = 456,
            IsActive = true,
            InteractionCount = 6,
            LastActivityAt = DateTime.UtcNow
        };

        _mockSessionRepository
            .Setup(r => r.GetByIdAsync("session-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _mockSessionRepository
            .Setup(r => r.UpdateAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedSession);

        // Act
        await _sessionService.RecordSessionActivityAsync("session-123").ConfigureAwait(false);

        // Assert
        _mockSessionRepository.Verify(r => r.UpdateAsync(It.Is<UserSession>(s =>
            s.InteractionCount == 6 &&
            s.LastActivityAt.HasValue
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that <see cref="SessionService.RecordSessionActivityAsync"/> does not throw
    /// when attempting to record activity for a non-existent session.
    /// </summary>
    [Fact]
    public async Task RecordSessionActivityAsync_WithNonExistingSession_DoesNotThrow()
    {
        // Arrange
        _mockSessionRepository
            .Setup(r => r.GetByIdAsync("nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserSession?)null);

        // Act & Assert
        await _sessionService.Invoking(s => s.RecordSessionActivityAsync("nonexistent"))
            .Should().NotThrowAsync();
    }

    /// <summary>
    /// Tests that <see cref="SessionService.CloseSessionAsync"/> closes an active session and returns true
    /// when the session exists and is active.
    /// </summary>
    [Fact]
    public async Task CloseSessionAsync_WithActiveSession_ClosesSessionAndReturnsTrue()
    {
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
    }

    /// <summary>
    /// Tests that <see cref="SessionService.CloseSessionAsync"/> returns false
    /// when attempting to close an already closed session.
    /// </summary>
    [Fact]
    public async Task CloseSessionAsync_WithAlreadyClosedSession_ReturnsFalse()
    {
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
    }

    /// <summary>
    /// Tests that <see cref="SessionService.CloseSessionAsync"/> returns false
    /// when attempting to close a non-existent session.
    /// </summary>
    [Fact]
    public async Task CloseSessionAsync_WithNonExistingSession_ReturnsFalse()
    {
        // Arrange
        _mockSessionRepository
            .Setup(r => r.GetByIdAsync("nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserSession?)null);

        // Act
        var result = await _sessionService.CloseSessionAsync("nonexistent").ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
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
        // Arrange
        var session = new UserSession
        {
            SessionId = "session-123",
            UserId = 123,
            ChatId = 456,
            IsActive = true,
            CurrentMenuId = "old_menu"
        };
        var updatedSession = new UserSession
        {
            SessionId = "session-123",
            UserId = 123,
            ChatId = 456,
            IsActive = true,
            CurrentMenuId = "new_menu"
        };

        _mockSessionRepository
            .Setup(r => r.GetByIdAsync("session-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _mockSessionRepository
            .Setup(r => r.UpdateAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedSession);

        // Act
        await _sessionService.NavigateToMenuAsync("session-123", "new_menu").ConfigureAwait(false);

        // Assert
        _mockSessionRepository.Verify(r => r.UpdateAsync(It.Is<UserSession>(s =>
            s.CurrentMenuId == "new_menu"
        ), It.IsAny<CancellationToken>()), Times.Once);
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
        // Arrange
        var session = new UserSession
        {
            SessionId = "session-123",
            UserId = 123,
            ChatId = 456
        };

        _mockSessionRepository
            .Setup(r => r.GetByIdAsync("session-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await _sessionService.GetSessionByIdAsync("session-123").ConfigureAwait(false);

        // Assert
        result.Should().Be(session);
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
        // Arrange
        _mockSessionRepository
            .Setup(r => r.GetByIdAsync("nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserSession?)null);

        // Act
        var result = await _sessionService.GetSessionByIdAsync("nonexistent").ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that <see cref="SessionService.GetAllActiveSessionsAsync"/> returns all active sessions
    /// from the repository.
    /// </summary>
    /// <returns>A collection of active <see cref="UserSession"/> objects.</returns>
    [Fact]
    public async Task GetAllActiveSessionsAsync_ReturnsActiveSessions()
    {
        // Arrange
        var activeSessions = new List<UserSession>
        {
            new UserSession { SessionId = "session-1", UserId = 1, IsActive = true },
            new UserSession { SessionId = "session-2", UserId = 2, IsActive = true }
        };

        _mockSessionRepository
            .Setup(r => r.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeSessions);

        // Act
        var result = await _sessionService.GetAllActiveSessionsAsync().ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(s => s.IsActive.Should().BeTrue());
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
        // Arrange
        var userSessions = new List<UserSession>
        {
            new UserSession { SessionId = "session-1", UserId = 123, IsActive = true },
            new UserSession { SessionId = "session-2", UserId = 123, IsActive = false }
        };

        _mockSessionRepository
            .Setup(r => r.GetByUserIdAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userSessions);

        // Act
        var result = await _sessionService.GetSessionsByUserIdAsync(123).ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(s => s.UserId.Should().Be(123));
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
        // Arrange
        _mockSessionRepository
            .Setup(r => r.DeleteAsync("session-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sessionService.DeleteSessionAsync("session-123").ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();
        _mockSessionRepository.Verify(r => r.DeleteAsync("session-123", It.IsAny<CancellationToken>()), Times.Once);
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
        // Arrange
        _mockSessionRepository
            .Setup(r => r.DeleteAsync("nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sessionService.DeleteSessionAsync("nonexistent").ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
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
        // Arrange
        var sessions = new List<UserSession>
        {
            new UserSession { SessionId = "session-1", UserId = 1, IsActive = true, LastActivityAt = DateTime.UtcNow.AddDays(-1) },
            new UserSession { SessionId = "session-2", UserId = 2, IsActive = true, LastActivityAt = DateTime.UtcNow.AddDays(-2) },
            new UserSession { SessionId = "session-3", UserId = 3, IsActive = false }
        };

        _mockSessionRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions);
        _mockSessionRepository
            .Setup(r => r.UpdateAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserSession s, CancellationToken _) => s);

        // Act
        var result = await _sessionService.ExpireInactiveSessionsAsync(TimeSpan.FromHours(24)).ConfigureAwait(false);

        // Assert
        result.Should().Be(2);
        _mockSessionRepository.Verify(r => r.UpdateAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    /// <summary>
    /// Tests that <see cref="SessionService.PruneExpiredSessions"/> prunes all sessions that have already expired
    /// based on their ExpiresAt timestamp and returns the count of pruned sessions.
    /// </summary>
    /// <returns>The number of sessions that were pruned.</returns>
    [Fact]
    public async Task PruneExpiredSessions_WithExpiredSessions_PrunesThemAndReturnsCount()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var sessions = new List<UserSession>
        {
            new UserSession { SessionId = "session-1", UserId = 1, IsActive = true, ExpiresAt = now.AddDays(-1) }, // Expired
            new UserSession { SessionId = "session-2", UserId = 2, IsActive = true, ExpiresAt = now.AddDays(1) }, // Not expired
            new UserSession { SessionId = "session-3", UserId = 3, IsActive = true, ExpiresAt = now.AddDays(-2) } // Expired
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
    }

    /// <summary>
    /// Tests that <see cref="SessionService.PruneExpiredSessions"/> returns 0 when there are no expired sessions.
    /// </summary>
    /// <returns>The number of sessions pruned (should be 0).</returns>
    [Fact]
    public async Task PruneExpiredSessions_WithNoExpiredSessions_ReturnsZero()
    {
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
    }
}