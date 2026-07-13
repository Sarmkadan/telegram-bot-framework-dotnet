#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TelegramBotFramework.Models;
using TelegramBotFramework.Repositories;
using TelegramBotFramework.Services;
using Xunit;

namespace TelegramBotFramework.Tests;

public sealed class SessionServiceTests
{
    private readonly Mock<ISessionRepository> _mockSessionRepository = new();
    private readonly Mock<ILogger<SessionService>> _mockLogger = new();
    private readonly SessionService _sessionService;

    public SessionServiceTests()
    {
        _sessionService = new SessionService(_mockSessionRepository.Object, _mockLogger.Object);
    }

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
}
