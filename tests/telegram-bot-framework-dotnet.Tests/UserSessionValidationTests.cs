using System;
using System.Collections.Generic;
using System.Linq;
using TelegramBotFramework.Models;
using Xunit;
using FluentAssertions;

namespace TelegramBotFramework.Tests;

public class UserSessionValidationTests
{
    private UserSession CreateValidSession()
    {
        return new UserSession
        {
            SessionId = "valid-session-id",
            UserId = 12345,
            ChatId = 67890,
            CurrentContext = "menu",
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            LastActivityAt = DateTime.UtcNow.AddMinutes(-5),
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            InteractionCount = 5
        };
    }

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
            .Which.Should().Be("SessionId cannot be null or whitespace.");
    }

    [Fact]
    public void ValidateSession_ShouldReturnError_WhenSessionIdIsWhitespace()
    {
        // Arrange
        var session = CreateValidSession();
        session.SessionId = "   ";

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("SessionId cannot be null or whitespace.");
    }

    [Fact]
    public void ValidateSession_ShouldReturnError_WhenSessionIdExceeds100Characters()
    {
        // Arrange
        var session = CreateValidSession();
        session.SessionId = new string('a', 101);

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("SessionId cannot exceed 100 characters.");
    }

    [Fact]
    public void ValidateSession_ShouldReturnError_WhenUserIdIsZero()
    {
        // Arrange
        var session = CreateValidSession();
        session.UserId = 0;

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("UserId must be a positive integer greater than zero.");
    }

    [Fact]
    public void ValidateSession_ShouldReturnError_WhenUserIdIsNegative()
    {
        // Arrange
        var session = CreateValidSession();
        session.UserId = -1;

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("UserId must be a positive integer greater than zero.");
    }

    [Fact]
    public void ValidateSession_ShouldReturnError_WhenChatIdIsZero()
    {
        // Arrange
        var session = CreateValidSession();
        session.ChatId = 0;

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("ChatId must be a positive integer greater than zero.");
    }

    [Fact]
    public void ValidateSession_ShouldReturnError_WhenChatIdIsNegative()
    {
        // Arrange
        var session = CreateValidSession();
        session.ChatId = -1;

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("ChatId must be a positive integer greater than zero.");
    }

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
            .Which.Should().Be("CurrentContext cannot be null or whitespace.");
    }

    [Fact]
    public void ValidateSession_ShouldReturnError_WhenCurrentContextIsWhitespace()
    {
        // Arrange
        var session = CreateValidSession();
        session.CurrentContext = "   ";

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("CurrentContext cannot be null or whitespace.");
    }

    [Fact]
    public void ValidateSession_ShouldReturnError_WhenCurrentContextExceeds50Characters()
    {
        // Arrange
        var session = CreateValidSession();
        session.CurrentContext = new string('a', 51);

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("CurrentContext cannot exceed 50 characters.");
    }

    [Fact]
    public void ValidateSession_ShouldReturnError_WhenCurrentMenuIdExceeds50Characters()
    {
        // Arrange
        var session = CreateValidSession();
        session.CurrentMenuId = new string('a', 51);

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("CurrentMenuId cannot exceed 50 characters.");
    }

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
            .Which.Should().Be("CreatedAt must be set to a valid DateTime.");
    }

    [Fact]
    public void ValidateSession_ShouldReturnError_WhenCreatedAtIsInFuture()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var session = new UserSession
        {
            SessionId = "valid-session-id",
            UserId = 12345,
            ChatId = 67890,
            CurrentContext = "menu",
            CreatedAt = now.AddMinutes(10),
            LastActivityAt = null, // Don't set LastActivityAt to avoid it triggering other errors
            InteractionCount = 5
        };

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("CreatedAt cannot be in the future.");
    }

    [Fact]
    public void ValidateSession_ShouldReturnError_WhenLastActivityAtIsDefault()
    {
        // Arrange
        var session = new UserSession
        {
            SessionId = "valid-session-id",
            UserId = 12345,
            ChatId = 67890,
            CurrentContext = "menu",
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            LastActivityAt = DateTime.MinValue, // default(DateTime)
            InteractionCount = 5
        };

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("LastActivityAt must be a valid DateTime if set.");
    }

    [Fact]
    public void ValidateSession_ShouldReturnError_WhenLastActivityAtIsInFuture()
    {
        // Arrange
        var session = CreateValidSession();
        session.LastActivityAt = DateTime.UtcNow.AddMinutes(10);

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("LastActivityAt cannot be in the future.");
    }

    [Fact]
    public void ValidateSession_ShouldReturnError_WhenLastActivityAtIsBeforeCreatedAt()
    {
        // Arrange
        var session = CreateValidSession();
        session.CreatedAt = DateTime.UtcNow;
        session.LastActivityAt = DateTime.UtcNow.AddMinutes(-10);

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("LastActivityAt cannot be before CreatedAt.");
    }

    [Fact]
    public void ValidateSession_ShouldReturnError_WhenExpiresAtIsDefault()
    {
        // Arrange
        var session = new UserSession
        {
            SessionId = "valid-session-id",
            UserId = 12345,
            ChatId = 67890,
            CurrentContext = "menu",
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            LastActivityAt = DateTime.UtcNow.AddMinutes(-5),
            ExpiresAt = DateTime.MinValue, // default(DateTime)
            InteractionCount = 5
        };

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("ExpiresAt must be a valid DateTime if set.");
    }

    [Fact]
    public void ValidateSession_ShouldReturnError_WhenExpiresAtIsBeforeCreatedAt()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var session = new UserSession
        {
            SessionId = "valid-session-id",
            UserId = 12345,
            ChatId = 67890,
            CurrentContext = "menu",
            CreatedAt = now.AddMinutes(-30), // Set to past to avoid "CreatedAt cannot be in future" error
            LastActivityAt = now.AddMinutes(-20), // Set to valid time after CreatedAt
            ExpiresAt = now.AddMinutes(-35), // Set to time before CreatedAt
            InteractionCount = 5
        };

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("ExpiresAt cannot be before CreatedAt.");
    }

    [Fact]
    public void ValidateSession_ShouldReturnError_WhenExpiresAtIsMoreThanOneYearInFuture()
    {
        // Arrange
        var session = CreateValidSession();
        session.ExpiresAt = DateTime.UtcNow.AddYears(2);

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("ExpiresAt cannot be more than 1 year in the future.");
    }

    [Fact]
    public void ValidateSession_ShouldReturnError_WhenContextDataExceeds1000Entries()
    {
        // Arrange
        var session = CreateValidSession();
        session.ContextData = Enumerable.Range(0, 1001)
            .ToDictionary(i => $"key{i}", i => $"value{i}");

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("ContextData dictionary cannot contain more than 1000 entries.");
    }

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

    [Fact]
    public void ValidateSession_ShouldReturnError_WhenContextDataHasWhitespaceKey()
    {
        // Arrange
        var session = CreateValidSession();
        session.ContextData = new Dictionary<string, string> { ["   "] = "value" };

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("ContextData contains an entry with null or whitespace key.");
    }

    [Fact]
    public void ValidateSession_ShouldReturnError_WhenContextDataKeyExceeds100Characters()
    {
        // Arrange
        var session = CreateValidSession();
        session.ContextData = new Dictionary<string, string> { [new string('a', 101)] = "value" };

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("ContextData key cannot exceed 100 characters.");
    }

    [Fact]
    public void ValidateSession_ShouldReturnError_WhenContextDataHasNullValue()
    {
        // Arrange
        var session = CreateValidSession();
        session.ContextData = new Dictionary<string, string> { ["key"] = null! };

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Match<string>(s => s.StartsWith("ContextData key 'key' has null or whitespace value."));
    }

    [Fact]
    public void ValidateSession_ShouldReturnError_WhenContextDataHasWhitespaceValue()
    {
        // Arrange
        var session = CreateValidSession();
        session.ContextData = new Dictionary<string, string> { ["key"] = "   " };

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Match<string>(s => s.StartsWith("ContextData key 'key' has null or whitespace value."));
    }

    [Fact]
    public void ValidateSession_ShouldReturnError_WhenContextDataValueExceeds1000Characters()
    {
        // Arrange
        var session = CreateValidSession();
        session.ContextData = new Dictionary<string, string> { ["key"] = new string('a', 1001) };

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Match<string>(s => s.StartsWith("ContextData value for key 'key' cannot exceed 1000 characters."));
    }

    [Fact]
    public void ValidateSession_ShouldReturnError_WhenCommandHistoryExceeds50Entries()
    {
        // Arrange
        var session = CreateValidSession();
        session.CommandHistory = Enumerable.Range(0, 51).Select(i => $"command{i}").ToList();

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("CommandHistory cannot contain more than 50 entries.");
    }

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
            .Which.Should().Be("CommandHistory contains null or whitespace entry.");
    }

    [Fact]
    public void ValidateSession_ShouldReturnError_WhenCommandHistoryHasWhitespaceEntry()
    {
        // Arrange
        var session = CreateValidSession();
        session.CommandHistory = new List<string> { "   " };

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("CommandHistory contains null or whitespace entry.");
    }

    [Fact]
    public void ValidateSession_ShouldReturnError_WhenCommandHistoryEntryExceeds200Characters()
    {
        // Arrange
        var session = CreateValidSession();
        session.CommandHistory = new List<string> { new string('a', 201) };

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("CommandHistory entry cannot exceed 200 characters.");
    }

    [Fact]
    public void ValidateSession_ShouldReturnError_WhenInteractionCountIsNegative()
    {
        // Arrange
        var session = CreateValidSession();
        session.InteractionCount = -1;

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("InteractionCount cannot be negative.");
    }

    [Fact]
    public void ValidateSession_ShouldReturnError_WhenUserInputExceeds1000Characters()
    {
        // Arrange
        var session = CreateValidSession();
        session.UserInput = new string('a', 1001);

        // Act
        var errors = session.ValidateSession();

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be("UserInput cannot exceed 1000 characters.");
    }

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

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenSessionIsInvalid()
    {
        // Arrange
        var session = CreateValidSession();
        session.UserId = 0; // Invalid

        // Act
        var isValid = session.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

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

    [Fact]
    public void EnsureValid_ShouldThrowArgumentException_WhenSessionIsInvalid()
    {
        // Arrange
        var session = CreateValidSession();
        session.UserId = 0; // Invalid

        // Act
        Action act = () => session.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*UserSession validation failed*");
    }
}