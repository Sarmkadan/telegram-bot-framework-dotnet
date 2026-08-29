#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using FluentAssertions;
using TelegramBotFramework.Models;
using Xunit;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Interface for unit tests of the <see cref="UserSession"/> validation logic.
/// </summary>
public interface IUserSessionValidationTests
{
    void ValidateSession_ShouldReturnEmptyList_WhenSessionIsValid();
    void ValidateSession_ShouldThrowArgumentNullException_WhenSessionIsNull();
    void ValidateSession_ShouldReturnError_WhenSessionIdIsNull();
    void ValidateSession_ShouldReturnError_WhenSessionIdIsWhitespace();
    void ValidateSession_ShouldReturnError_WhenSessionIdExceeds100Characters();
    void ValidateSession_ShouldReturnError_WhenUserIdIsZero();
    void ValidateSession_ShouldReturnError_WhenUserIdIsNegative();
    void ValidateSession_ShouldReturnError_WhenChatIdIsZero();
    void ValidateSession_ShouldReturnError_WhenChatIdIsNegative();
    void ValidateSession_ShouldReturnError_WhenCurrentContextIsNull();
    void ValidateSession_ShouldReturnError_WhenCurrentContextIsWhitespace();
    void ValidateSession_ShouldReturnError_WhenCurrentContextExceeds50Characters();
    void ValidateSession_ShouldReturnError_WhenCurrentMenuIdExceeds50Characters();
    void ValidateSession_ShouldReturnError_WhenCreatedAtIsDefault();
    void ValidateSession_ShouldReturnError_WhenCreatedAtIsInFuture();
    void ValidateSession_ShouldReturnError_WhenLastActivityAtIsDefault();
    void ValidateSession_ShouldReturnError_WhenLastActivityAtIsInFuture();
    void ValidateSession_ShouldReturnError_WhenLastActivityAtIsBeforeCreatedAt();
    void ValidateSession_ShouldReturnError_WhenExpiresAtIsDefault();
    void ValidateSession_ShouldReturnError_WhenExpiresAtIsBeforeCreatedAt();
}