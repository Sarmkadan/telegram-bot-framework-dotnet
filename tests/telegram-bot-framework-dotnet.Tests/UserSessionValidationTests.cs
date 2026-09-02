using System;
using System.Collections.Generic;
using System.Linq;
using TelegramBotFramework.Models;
using Xunit;
using FluentAssertions;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Verifies validation, validity checks, and exception enforcement for user sessions.
/// </summary>
public class UserSessionValidationTests : IUserSessionValidationTests
{
    private UserSession CreateValidSession()
    {
        return new UserSession
        {
            SessionId = UserSessionValidationTestsConstants.ValidSessionId,
            UserId = UserSessionValidationTestsConstants.ValidUserId,
            ChatId = UserSessionValidationTestsConstants.ValidChatId,
            CurrentContext = UserSessionValidationTestsConstants.ValidCurrentContext,
            CreatedAt = DateTime.UtcNow.AddMinutes(UserSessionValidationTestsConstants.CreatedAtMinutesAgo),
            LastActivityAt = DateTime.UtcNow.AddMinutes(UserSessionValidationTestsConstants.LastActivityAtMinutesAgo),
            ExpiresAt = DateTime.UtcNow.AddHours(UserSessionValidationTestsConstants.ExpiresAtHoursFromNow),
            InteractionCount = UserSessionValidationTestsConstants.ValidInteractionCount
        };
    }

    /// <summary>
    /// Verifies that validating a fully populated valid session returns no errors.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldReturnEmptyList_WhenSessionIsValid()
    {
        // Arrange
        var session = CreateValidSession();

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that validating a null session throws an <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldThrowArgumentNullException_WhenSessionIsNull()
    {
        // Arrange
        UserSession session = null!;

        // Act
        Action act = () => session.ValidateSession();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that validation reports a null session identifier.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldReturnError_WhenSessionIdIsNull()
    {
        // Arrange
        var session = CreateValidSession();
        session.SessionId = null!;

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be(UserSessionValidationTestsConstants.SessionIdCannotBeNullOrWhitespace);
    }

    /// <summary>
    /// Verifies that validation reports a whitespace-only session identifier.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldReturnError_WhenSessionIdIsWhitespace()
    {
        // Arrange
        var session = CreateValidSession();
        session.SessionId = UserSessionValidationTestsConstants.WhitespaceValue;

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be(UserSessionValidationTestsConstants.SessionIdCannotBeNullOrWhitespace);
    }

    /// <summary>
    /// Verifies that validation reports a session identifier longer than 100 characters.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldReturnError_WhenSessionIdExceeds100Characters()
    {
        // Arrange
        var session = CreateValidSession();
        session.SessionId = new string(
            UserSessionValidationTestsConstants.RepeatedCharacter,
            UserSessionValidationTestsConstants.SessionIdMaxLength + UserSessionValidationTestsConstants.LengthBeyondLimit);

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be(UserSessionValidationTestsConstants.SessionIdExceedsMaxLength);
    }

    /// <summary>
    /// Verifies that validation reports a zero user identifier.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldReturnError_WhenUserIdIsZero()
    {
        // Arrange
        var session = CreateValidSession();
        session.UserId = UserSessionValidationTestsConstants.InvalidZeroId;

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be(UserSessionValidationTestsConstants.UserIdMustBePositive);
    }

    /// <summary>
    /// Verifies that validation reports a negative user identifier.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldReturnError_WhenUserIdIsNegative()
    {
        // Arrange
        var session = CreateValidSession();
        session.UserId = UserSessionValidationTestsConstants.InvalidNegativeValue;

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be(UserSessionValidationTestsConstants.UserIdMustBePositive);
    }

    /// <summary>
    /// Verifies that validation reports a zero chat identifier.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldReturnError_WhenChatIdIsZero()
    {
        // Arrange
        var session = CreateValidSession();
        session.ChatId = UserSessionValidationTestsConstants.InvalidZeroId;

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be(UserSessionValidationTestsConstants.ChatIdMustBePositive);
    }

    /// <summary>
    /// Verifies that validation reports a negative chat identifier.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldReturnError_WhenChatIdIsNegative()
    {
        // Arrange
        var session = CreateValidSession();
        session.ChatId = UserSessionValidationTestsConstants.InvalidNegativeValue;

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be(UserSessionValidationTestsConstants.ChatIdMustBePositive);
    }

    /// <summary>
    /// Verifies that validation reports a null current context.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldReturnError_WhenCurrentContextIsNull()
    {
        // Arrange
        var session = CreateValidSession();
        session.CurrentContext = null!;

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be(UserSessionValidationTestsConstants.CurrentContextCannotBeNullOrWhitespace);
    }

    /// <summary>
    /// Verifies that validation reports a whitespace-only current context.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldReturnError_WhenCurrentContextIsWhitespace()
    {
        // Arrange
        var session = CreateValidSession();
        session.CurrentContext = UserSessionValidationTestsConstants.WhitespaceValue;

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be(UserSessionValidationTestsConstants.CurrentContextCannotBeNullOrWhitespace);
    }

    /// <summary>
    /// Verifies that validation reports a current context longer than 50 characters.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldReturnError_WhenCurrentContextExceeds50Characters()
    {
        // Arrange
        var session = CreateValidSession();
        session.CurrentContext = new string(
            UserSessionValidationTestsConstants.RepeatedCharacter,
            UserSessionValidationTestsConstants.CurrentContextMaxLength + UserSessionValidationTestsConstants.LengthBeyondLimit);

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be(UserSessionValidationTestsConstants.CurrentContextExceedsMaxLength);
    }

    /// <summary>
    /// Verifies that validation reports a current menu identifier longer than 50 characters.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldReturnError_WhenCurrentMenuIdExceeds50Characters()
    {
        // Arrange
        var session = CreateValidSession();
        session.CurrentMenuId = new string(
            UserSessionValidationTestsConstants.RepeatedCharacter,
            UserSessionValidationTestsConstants.CurrentMenuIdMaxLength + UserSessionValidationTestsConstants.LengthBeyondLimit);

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be(UserSessionValidationTestsConstants.CurrentMenuIdExceedsMaxLength);
    }

    /// <summary>
    /// Verifies that validation reports an unset creation timestamp.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldReturnError_WhenCreatedAtIsDefault()
    {
        // Arrange
        var session = CreateValidSession();
        session.CreatedAt = default;

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be(UserSessionValidationTestsConstants.CreatedAtMustBeSet);
    }

    /// <summary>
    /// Verifies that validation reports a creation timestamp in the future.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldReturnError_WhenCreatedAtIsInFuture()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var session = new UserSession
        {
            SessionId = UserSessionValidationTestsConstants.ValidSessionId,
            UserId = UserSessionValidationTestsConstants.ValidUserId,
            ChatId = UserSessionValidationTestsConstants.ValidChatId,
            CurrentContext = UserSessionValidationTestsConstants.ValidCurrentContext,
            CreatedAt = now.AddMinutes(UserSessionValidationTestsConstants.FutureMinutes),
            LastActivityAt = null, // Don't set LastActivityAt to avoid it triggering other errors
            InteractionCount = UserSessionValidationTestsConstants.ValidInteractionCount
        };

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be(UserSessionValidationTestsConstants.CreatedAtCannotBeInFuture);
    }

    /// <summary>
    /// Verifies that validation reports a default last-activity timestamp when it is set.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldReturnError_WhenLastActivityAtIsDefault()
    {
        // Arrange
        var session = new UserSession
        {
            SessionId = UserSessionValidationTestsConstants.ValidSessionId,
            UserId = UserSessionValidationTestsConstants.ValidUserId,
            ChatId = UserSessionValidationTestsConstants.ValidChatId,
            CurrentContext = UserSessionValidationTestsConstants.ValidCurrentContext,
            CreatedAt = DateTime.UtcNow.AddMinutes(UserSessionValidationTestsConstants.CreatedAtMinutesAgo),
            LastActivityAt = UserSessionValidationTestsConstants.DateTimeMinValue, // default(DateTime)
            InteractionCount = UserSessionValidationTestsConstants.ValidInteractionCount
        };

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be(UserSessionValidationTestsConstants.LastActivityAtMustBeValidIfSet);
    }

    /// <summary>
    /// Verifies that validation reports a last-activity timestamp in the future.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldReturnError_WhenLastActivityAtIsInFuture()
    {
        // Arrange
        var session = CreateValidSession();
        session.LastActivityAt = DateTime.UtcNow.AddMinutes(UserSessionValidationTestsConstants.FutureMinutes);

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be(UserSessionValidationTestsConstants.LastActivityAtCannotBeInFuture);
    }

    /// <summary>
    /// Verifies that validation reports last activity occurring before session creation.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldReturnError_WhenLastActivityAtIsBeforeCreatedAt()
    {
        // Arrange
        var session = CreateValidSession();
        session.CreatedAt = DateTime.UtcNow;
        session.LastActivityAt = DateTime.UtcNow.AddMinutes(UserSessionValidationTestsConstants.CreatedAtMinutesAgo);

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be(UserSessionValidationTestsConstants.LastActivityAtCannotBeBeforeCreatedAt);
    }

    /// <summary>
    /// Verifies that validation reports a default expiration timestamp when it is set.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldReturnError_WhenExpiresAtIsDefault()
    {
        // Arrange
        var session = new UserSession
        {
            SessionId = UserSessionValidationTestsConstants.ValidSessionId,
            UserId = UserSessionValidationTestsConstants.ValidUserId,
            ChatId = UserSessionValidationTestsConstants.ValidChatId,
            CurrentContext = UserSessionValidationTestsConstants.ValidCurrentContext,
            CreatedAt = DateTime.UtcNow.AddMinutes(UserSessionValidationTestsConstants.CreatedAtMinutesAgo),
            LastActivityAt = DateTime.UtcNow.AddMinutes(UserSessionValidationTestsConstants.LastActivityAtMinutesAgo),
            ExpiresAt = UserSessionValidationTestsConstants.DateTimeMinValue, // default(DateTime)
            InteractionCount = UserSessionValidationTestsConstants.ValidInteractionCount
        };

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be(UserSessionValidationTestsConstants.ExpiresAtMustBeValidIfSet);
    }

    /// <summary>
    /// Verifies that validation reports an expiration timestamp before session creation.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldReturnError_WhenExpiresAtIsBeforeCreatedAt()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var session = new UserSession
        {
            SessionId = UserSessionValidationTestsConstants.ValidSessionId,
            UserId = UserSessionValidationTestsConstants.ValidUserId,
            ChatId = UserSessionValidationTestsConstants.ValidChatId,
            CurrentContext = UserSessionValidationTestsConstants.ValidCurrentContext,
            CreatedAt = now.AddMinutes(UserSessionValidationTestsConstants.ExpiresAtCreatedOffsetMinutes), // Set to past to avoid "CreatedAt cannot be in future" error
            LastActivityAt = now.AddMinutes(UserSessionValidationTestsConstants.ExpiresAtActivityOffsetMinutes), // Set to valid time after CreatedAt
            ExpiresAt = now.AddMinutes(UserSessionValidationTestsConstants.ExpiresAtInvalidOffsetMinutes), // Set to time before CreatedAt
            InteractionCount = UserSessionValidationTestsConstants.ValidInteractionCount
        };

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be(UserSessionValidationTestsConstants.ExpiresAtCannotBeBeforeCreatedAt);
    }

    /// <summary>
    /// Verifies that validation reports an expiration timestamp more than one year in the future.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldReturnError_WhenExpiresAtIsMoreThanOneYearInFuture()
    {
        // Arrange
        var session = CreateValidSession();
        session.ExpiresAt = DateTime.UtcNow.AddYears(
            UserSessionValidationTestsConstants.ExpiresAtMaxYearsInFuture
            + UserSessionValidationTestsConstants.YearsBeyondExpirationLimit);

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be(UserSessionValidationTestsConstants.ExpiresAtCannotBeMoreThanOneYearInFuture);
    }

    /// <summary>
    /// Verifies that validation reports context data containing more than 1,000 entries.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldReturnError_WhenContextDataExceeds1000Entries()
    {
        // Arrange
        var session = CreateValidSession();
        session.ContextData = Enumerable.Range(
                UserSessionValidationTestsConstants.InvalidZeroId,
                UserSessionValidationTestsConstants.ContextDataMaxEntries + UserSessionValidationTestsConstants.LengthBeyondLimit)
            .ToDictionary(
                i => string.Format(UserSessionValidationTestsConstants.ContextDataKeyFormat, i),
                i => string.Format(UserSessionValidationTestsConstants.ContextDataValueFormat, i));

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be(UserSessionValidationTestsConstants.ContextDataCannotContainMoreThanMaxEntries);
    }

    /// <summary>
    /// Verifies that validation accepts a null context-data collection without errors.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldHandleNullContextDataGracefully()
    {
        // Arrange
        var session = CreateValidSession();
        session.ContextData = null;

        // Act
        var errors = session.ValidateSession();

        // Assert - null ContextData should not cause errors
        errors.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that validation reports a context-data entry with a whitespace-only key.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldReturnError_WhenContextDataHasWhitespaceKey()
    {
        // Arrange
        var session = CreateValidSession();
        session.ContextData = new Dictionary<string, string>
        {
            [UserSessionValidationTestsConstants.WhitespaceValue] = UserSessionValidationTestsConstants.ContextDataValue
        };

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be(UserSessionValidationTestsConstants.ContextDataContainsEntryWithNullOrWhitespaceKey);
    }

    /// <summary>
    /// Verifies that validation reports a context-data key longer than 100 characters.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldReturnError_WhenContextDataKeyExceeds100Characters()
    {
        // Arrange
        var session = CreateValidSession();
        session.ContextData = new Dictionary<string, string>
        {
            [new string(
                UserSessionValidationTestsConstants.RepeatedCharacter,
                UserSessionValidationTestsConstants.ContextDataKeyMaxLength + UserSessionValidationTestsConstants.LengthBeyondLimit)]
                = UserSessionValidationTestsConstants.ContextDataValue
        };

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be(UserSessionValidationTestsConstants.ContextDataKeyCannotExceedMaxLength);
    }

    /// <summary>
    /// Verifies that validation reports a context-data entry with a null value.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldReturnError_WhenContextDataHasNullValue()
    {
        // Arrange
        var session = CreateValidSession();
        session.ContextData = new Dictionary<string, string> { [UserSessionValidationTestsConstants.ContextDataKey] = null! };

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Match<string>(s => s.StartsWith(string.Format(
                UserSessionValidationTestsConstants.ContextDataKeyHasNullOrWhitespaceValueFormat,
                UserSessionValidationTestsConstants.ContextDataKey)));
    }

    /// <summary>
    /// Verifies that validation reports a context-data entry with a whitespace-only value.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldReturnError_WhenContextDataHasWhitespaceValue()
    {
        // Arrange
        var session = CreateValidSession();
        session.ContextData = new Dictionary<string, string>
        {
            [UserSessionValidationTestsConstants.ContextDataKey] = UserSessionValidationTestsConstants.WhitespaceValue
        };

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Match<string>(s => s.StartsWith(string.Format(
                UserSessionValidationTestsConstants.ContextDataKeyHasNullOrWhitespaceValueFormat,
                UserSessionValidationTestsConstants.ContextDataKey)));
    }

    /// <summary>
    /// Verifies that validation reports a context-data value longer than 1,000 characters.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldReturnError_WhenContextDataValueExceeds1000Characters()
    {
        // Arrange
        var session = CreateValidSession();
        session.ContextData = new Dictionary<string, string>
        {
            [UserSessionValidationTestsConstants.ContextDataKey] = new string(
                UserSessionValidationTestsConstants.RepeatedCharacter,
                UserSessionValidationTestsConstants.ContextDataValueMaxLength + UserSessionValidationTestsConstants.LengthBeyondLimit)
        };

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Match<string>(s => s.StartsWith(string.Format(
                UserSessionValidationTestsConstants.ContextDataValueForKeyCannotExceedMaxLengthFormat,
                UserSessionValidationTestsConstants.ContextDataKey)));
    }

    /// <summary>
    /// Verifies that validation reports command history containing more than 50 entries.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldReturnError_WhenCommandHistoryExceeds50Entries()
    {
        // Arrange
        var session = CreateValidSession();
        session.CommandHistory = Enumerable.Range(
                UserSessionValidationTestsConstants.InvalidZeroId,
                UserSessionValidationTestsConstants.CommandHistoryMaxEntries + UserSessionValidationTestsConstants.LengthBeyondLimit)
            .Select(i => string.Format(UserSessionValidationTestsConstants.CommandHistoryEntryFormat, i))
            .ToList();

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be(UserSessionValidationTestsConstants.CommandHistoryCannotContainMoreThanMaxEntries);
    }

    /// <summary>
    /// Verifies that validation reports a null command-history entry.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldReturnError_WhenCommandHistoryHasNullEntry()
    {
        // Arrange
        var session = CreateValidSession();
        session.CommandHistory = new List<string> { null! };

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be(UserSessionValidationTestsConstants.CommandHistoryContainsNullOrWhitespaceEntry);
    }

    /// <summary>
    /// Verifies that validation reports a whitespace-only command-history entry.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldReturnError_WhenCommandHistoryHasWhitespaceEntry()
    {
        // Arrange
        var session = CreateValidSession();
        session.CommandHistory = new List<string> { UserSessionValidationTestsConstants.WhitespaceValue };

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be(UserSessionValidationTestsConstants.CommandHistoryContainsNullOrWhitespaceEntry);
    }

    /// <summary>
    /// Verifies that validation reports a command-history entry longer than 200 characters.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldReturnError_WhenCommandHistoryEntryExceeds200Characters()
    {
        // Arrange
        var session = CreateValidSession();
        session.CommandHistory = new List<string>
        {
            new(
                UserSessionValidationTestsConstants.RepeatedCharacter,
                UserSessionValidationTestsConstants.CommandHistoryEntryMaxLength + UserSessionValidationTestsConstants.LengthBeyondLimit)
        };

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be(UserSessionValidationTestsConstants.CommandHistoryEntryCannotExceedMaxLength);
    }

    /// <summary>
    /// Verifies that validation reports a negative interaction count.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldReturnError_WhenInteractionCountIsNegative()
    {
        // Arrange
        var session = CreateValidSession();
        session.InteractionCount = UserSessionValidationTestsConstants.InvalidNegativeValue;

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be(UserSessionValidationTestsConstants.InteractionCountCannotBeNegative);
    }

    /// <summary>
    /// Verifies that validation reports user input longer than 1,000 characters.
    /// </summary>
    [Fact]
    public void ValidateSession_ShouldReturnError_WhenUserInputExceeds1000Characters()
    {
        // Arrange
        var session = CreateValidSession();
        session.UserInput = new string(
            UserSessionValidationTestsConstants.RepeatedCharacter,
            UserSessionValidationTestsConstants.UserInputMaxLength + UserSessionValidationTestsConstants.LengthBeyondLimit);

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be(UserSessionValidationTestsConstants.UserInputCannotExceedMaxLength);
    }

    /// <summary>
    /// Verifies that the validity check returns true for a valid session.
    /// </summary>
    [Fact]
    public void IsValid_ShouldReturnTrue_WhenSessionIsValid()
    {
        // Arrange
        var session = CreateValidSession();

        // Act
        var isValid = session.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that the validity check returns false when the user identifier is zero.
    /// </summary>
    [Fact]
    public void IsValid_ShouldReturnFalse_WhenSessionIsInvalid()
    {
        // Arrange
        var session = CreateValidSession();
        session.UserId = UserSessionValidationTestsConstants.InvalidZeroId; // Invalid

        // Act
        var isValid = session.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that enforcing validity does not throw for a valid session.
    /// </summary>
    [Fact]
    public void EnsureValid_ShouldNotThrow_WhenSessionIsValid()
    {
        // Arrange
        var session = CreateValidSession();

        // Act
        Action act = () => session.EnsureValid();

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that enforcing validity on a null session throws an <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void EnsureValid_ShouldThrowArgumentNullException_WhenSessionIsNull()
    {
        // Arrange
        UserSession session = null!;

        // Act
        Action act = () => session.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that enforcing validity on a session with a zero user identifier throws an <see cref="ArgumentException"/>.
    /// </summary>
    [Fact]
    public void EnsureValid_ShouldThrowArgumentException_WhenSessionIsInvalid()
    {
        // Arrange
        var session = CreateValidSession();
        session.UserId = UserSessionValidationTestsConstants.InvalidZeroId; // Invalid

        // Act
        Action act = () => session.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage(UserSessionValidationTestsConstants.EnsureValidFailureMessagePattern);
    }
}
