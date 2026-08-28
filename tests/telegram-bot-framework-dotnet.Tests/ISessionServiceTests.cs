#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Threading;
using System.Threading.Tasks;
using TelegramBotFramework.Models;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Interface defining the contract for SessionService test methods.
/// </summary>
public interface ISessionServiceTests
{
    Task GetActiveSessionAsync_WithExistingActiveSession_ReturnsSession();
    Task GetActiveSessionAsync_WithNoActiveSession_ReturnsNull();
    Task CreateSessionAsync_CreatesNewSession();
    Task CreateSessionAsync_WithCustomTimeout_CreatesSessionWithCorrectExpiration();
    Task RecordSessionActivityAsync_UpdatesLastActivityAndIncrementsInteractionCount();
    Task RecordSessionActivityAsync_WithNonExistingSession_DoesNotThrow();
    Task CloseSessionAsync_WithActiveSession_ClosesSessionAndReturnsTrue();
    Task CloseSessionAsync_WithAlreadyClosedSession_ReturnsFalse();
    Task CloseSessionAsync_WithNonExistingSession_ReturnsFalse();
    Task NavigateToMenuAsync_UpdatesCurrentMenuId();
    Task GetSessionByIdAsync_WithExistingSession_ReturnsSession();
    Task GetSessionByIdAsync_WithNonExistingSession_ReturnsNull();
    Task GetAllActiveSessionsAsync_ReturnsActiveSessions();
    Task GetSessionsByUserIdAsync_ReturnsUserSessions();
    Task DeleteSessionAsync_WithExistingSession_DeletesAndReturnsTrue();
    Task DeleteSessionAsync_WithNonExistingSession_ReturnsFalse();
    Task ExpireInactiveSessionsAsync_WithInactiveSessions_ClosesThem();
    Task PruneExpiredSessions_WithExpiredSessions_PrunesThemAndReturnsCount();
    Task PruneExpiredSessions_WithNoExpiredSessions_ReturnsZero();
}