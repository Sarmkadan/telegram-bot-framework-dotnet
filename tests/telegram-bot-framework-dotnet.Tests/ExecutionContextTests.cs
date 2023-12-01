#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using TelegramBotFramework.Models;
using Xunit;

namespace TelegramBotFramework.Tests;

public sealed class ExecutionContextTests
{
    [Fact]
    public void Constructor_WithDefaultValues_InitializesCorrectly()
    {
        // Act
        var context = new TelegramBotFramework.Models.ExecutionContext();

        // Assert
        context.ContextId.Should().NotBeEmpty();
        context.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        context.IsValid.Should().BeTrue();
        context.Errors.Should().BeEmpty();
        context.States.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithUserAndSession_StoresReferences()
    {
        // Arrange
        var user = new BotUser { TelegramId = 123, FirstName = "John" };
        var session = new UserSession { SessionId = "session-123", UserId = 123, ChatId = 456 };
        var message = new Message { MessageId = 1, UserId = 123, ChatId = 456, Content = "test" };

        // Act
        var context = new TelegramBotFramework.Models.ExecutionContext
        {
            UserId = 123,
            ChatId = 456,
            User = user,
            Session = session,
            Message = message
        };

        // Assert
        context.UserId.Should().Be(123);
        context.ChatId.Should().Be(456);
        context.User.Should().Be(user);
        context.Session.Should().Be(session);
        context.Message.Should().Be(message);
    }

    [Fact]
    public void AddError_AddsErrorToErrorsList()
    {
        // Arrange
        var context = new TelegramBotFramework.Models.ExecutionContext();

        // Act
        context.AddError("Test error 1");
        context.AddError("Test error 2");

        // Assert
        context.Errors.Should().HaveCount(2);
        context.Errors.Should().Contain("Test error 1");
        context.Errors.Should().Contain("Test error 2");
    }

    [Fact]
    public void AddError_WithNullError_DoesNotAdd()
    {
        // Arrange
        var context = new TelegramBotFramework.Models.ExecutionContext();

        // Act
        context.AddError(null);
        context.AddError("Valid error");

        // Assert
        context.Errors.Should().HaveCount(1);
        context.Errors.Should().Contain("Valid error");
    }

    [Fact]
    public void AddError_WithEmptyError_DoesNotAdd()
    {
        // Arrange
        var context = new TelegramBotFramework.Models.ExecutionContext();

        // Act
        context.AddError("");
        context.AddError("Valid error");

        // Assert
        context.Errors.Should().HaveCount(1);
        context.Errors.Should().Contain("Valid error");
    }

    [Fact]
    public void SetState_AddsStateToStatesDictionary()
    {
        // Arrange
        var context = new TelegramBotFramework.Models.ExecutionContext();

        // Act
        context.SetState("key1", "value1");
        context.SetState("key2", 123);
        context.SetState("key3", true);

        // Assert
        context.States.Should().HaveCount(3);
        context.States.Should().ContainKey("key1").WhoseValue.Should().Be("value1");
        context.States.Should().ContainKey("key2").WhoseValue.Should().Be(123);
        context.States.Should().ContainKey("key3").WhoseValue.Should().Be(true);
    }

    [Fact]
    public void SetState_OverwritesExistingState()
    {
        // Arrange
        var context = new TelegramBotFramework.Models.ExecutionContext();

        // Act
        context.SetState("key", "old_value");
        context.SetState("key", "new_value");

        // Assert
        context.States.Should().HaveCount(1);
        context.States["key"].Should().Be("new_value");
    }

    [Fact]
    public void SetState_WithNullKey_DoesNotAdd()
    {
        // Arrange
        var context = new TelegramBotFramework.Models.ExecutionContext();

        // Act
        context.SetState(null, "value");
        context.SetState("key", "value");

        // Assert
        context.States.Should().HaveCount(1);
        context.States.Should().ContainKey("key");
    }

    [Fact]
    public void SetState_WithEmptyKey_DoesNotAdd()
    {
        // Arrange
        var context = new TelegramBotFramework.Models.ExecutionContext();

        // Act
        context.SetState("", "value");
        context.SetState("key", "value");

        // Assert
        context.States.Should().HaveCount(1);
        context.States.Should().ContainKey("key");
    }

    [Fact]
    public void GetState_WithExistingKey_ReturnsValue()
    {
        // Arrange
        var context = new TelegramBotFramework.Models.ExecutionContext();
        context.SetState("test_key", "test_value");

        // Act
        var result = context.GetState<string>("test_key");

        // Assert
        result.Should().Be("test_value");
    }

    [Fact]
    public void GetState_WithNonExistingKey_ReturnsDefault()
    {
        // Arrange
        var context = new TelegramBotFramework.Models.ExecutionContext();

        // Act
        var result = context.GetState<string>("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetState_WithWrongType_ReturnsDefault()
    {
        // Arrange
        var context = new TelegramBotFramework.Models.ExecutionContext();
        context.SetState("number_key", 123);

        // Act
        var result = context.GetState<string>("number_key");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Validate_WithValidContext_ReturnsTrue()
    {
        // Arrange
        var context = new TelegramBotFramework.Models.ExecutionContext
        {
            UserId = 123,
            ChatId = 456,
            User = new BotUser { UserId = 123, FirstName = "John" },
            Session = new UserSession { SessionId = "session-123", UserId = 123, ChatId = 456 },
            Message = new Message { MessageId = 1, UserId = 123, ChatId = 456, Content = "test" }
        };

        // Act
        var result = context.Validate();

        // Assert
        result.Should().BeTrue();
        context.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithNullUser_AddsErrorAndReturnsFalse()
    {
        // Arrange
        var context = new TelegramBotFramework.Models.ExecutionContext
        {
            UserId = 123,
            ChatId = 456,
            User = null,
            Session = new UserSession { SessionId = "session-123", UserId = 123, ChatId = 456 },
            Message = new Message { MessageId = 1, UserId = 123, ChatId = 456, Content = "test" }
        };

        // Act
        var result = context.Validate();

        // Assert
        result.Should().BeFalse();
        context.IsValid.Should().BeFalse();
        context.Errors.Should().Contain(e => e.Contains("User"));
    }

    [Fact]
    public void Validate_WithNullSession_AddsErrorAndReturnsFalse()
    {
        // Arrange
        var context = new TelegramBotFramework.Models.ExecutionContext
        {
            UserId = 123,
            ChatId = 456,
            User = new BotUser { UserId = 123, FirstName = "John" },
            Session = null,
            Message = new Message { MessageId = 1, UserId = 123, ChatId = 456, Content = "test" }
        };

        // Act
        var result = context.Validate();

        // Assert
        result.Should().BeFalse();
        context.IsValid.Should().BeFalse();
        context.Errors.Should().Contain(e => e.Contains("Session"));
    }

    [Fact]
    public void Validate_WithNullMessage_AddsErrorAndReturnsFalse()
    {
        // Arrange
        var context = new TelegramBotFramework.Models.ExecutionContext
        {
            UserId = 123,
            ChatId = 456,
            User = new BotUser { UserId = 123, FirstName = "John" },
            Session = new UserSession { SessionId = "session-123", UserId = 123, ChatId = 456 },
            Message = null
        };

        // Act
        var result = context.Validate();

        // Assert
        result.Should().BeFalse();
        context.IsValid.Should().BeFalse();
        context.Errors.Should().Contain(e => e.Contains("Message"));
    }

    [Fact]
    public void Validate_WithZeroUserId_AddsErrorAndReturnsFalse()
    {
        // Arrange
        var context = new TelegramBotFramework.Models.ExecutionContext
        {
            UserId = 0,
            ChatId = 456,
            User = new BotUser { UserId = 0, FirstName = "John" },
            Session = new UserSession { SessionId = "session-123", UserId = 0, ChatId = 456 },
            Message = new Message { MessageId = 1, UserId = 0, ChatId = 456, Content = "test" }
        };

        // Act
        var result = context.Validate();

        // Assert
        result.Should().BeFalse();
        context.IsValid.Should().BeFalse();
        context.Errors.Should().Contain(e => e.Contains("UserId"));
    }

    [Fact]
    public void Validate_WithZeroChatId_AddsErrorAndReturnsFalse()
    {
        // Arrange
        var context = new TelegramBotFramework.Models.ExecutionContext
        {
            UserId = 123,
            ChatId = 0,
            User = new BotUser { UserId = 123, FirstName = "John" },
            Session = new UserSession { SessionId = "session-123", UserId = 123, ChatId = 0 },
            Message = new Message { MessageId = 1, UserId = 123, ChatId = 0, Content = "test" }
        };

        // Act
        var result = context.Validate();

        // Assert
        result.Should().BeFalse();
        context.IsValid.Should().BeFalse();
        context.Errors.Should().Contain(e => e.Contains("ChatId"));
    }

    [Fact]
    public void StopProcessing_SetsIsStoppedToTrue()
    {
        // Arrange
        var context = new TelegramBotFramework.Models.ExecutionContext();

        // Act
        context.StopProcessing();

        // Assert
        context.IsStopped.Should().BeTrue();
    }

    [Fact]
    public void GetDuration_ReturnsTimeSpanSinceCreation()
    {
        // Arrange
        var context = new TelegramBotFramework.Models.ExecutionContext();
        var createdAt = context.CreatedAt;

        // Simulate some time passing
        Thread.Sleep(10);

        // Act
        var duration = context.GetDuration();

        // Assert
        duration.Should().BeCloseTo(DateTime.UtcNow - createdAt, TimeSpan.FromMilliseconds(50));
        duration.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public void IsValid_WhenNoErrors_ReturnsTrue()
    {
        // Arrange
        var context = new TelegramBotFramework.Models.ExecutionContext();

        // Act
        context.AddError(null); // Won't add
        context.AddError(""); // Won't add

        // Assert
        context.IsValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WhenHasErrors_ReturnsFalse()
    {
        // Arrange
        var context = new TelegramBotFramework.Models.ExecutionContext();

        // Act
        context.AddError("Error occurred");

        // Assert
        context.IsValid.Should().BeFalse();
    }
}
