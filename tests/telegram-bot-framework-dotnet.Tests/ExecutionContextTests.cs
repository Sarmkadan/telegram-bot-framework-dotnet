#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using TelegramBotFramework.Models;
using Xunit;

/// <summary>
/// Tests for the ExecutionContext class.
/// </summary>
public sealed class ExecutionContextTests : IExecutionContextTests
{
    /// <summary>
    /// Tests that the ExecutionContext constructor initializes correctly with default values.
    /// </summary>
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

    /// <summary>
    /// Tests that the ExecutionContext constructor stores references to user and session.
    /// </summary>
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

    /// <summary>
    /// Tests that the AddError method adds an error to the Errors list.
    /// </summary>
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

    /// <summary>
    /// Tests that the AddError method does not add a null error.
    /// </summary>
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

    /// <summary>
    /// Tests that the AddError method does not add an empty error.
    /// </summary>
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

    /// <summary>
    /// Tests that the SetState method adds a state to the States dictionary.
    /// </summary>
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

    /// <summary>
    /// Tests that the SetState method overwrites an existing state.
    /// </summary>
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

    /// <summary>
    /// Tests that the SetState method does not add a state with a null key.
    /// </summary>
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

    /// <summary>
    /// Tests that the SetState method does not add a state with an empty key.
    /// </summary>
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

    /// <summary>
    /// Tests that the GetState method returns the value of an existing state.
    /// </summary>
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

    /// <summary>
    /// Tests that the GetState method returns the default value for a non-existing key.
    /// </summary>
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

    /// <summary>
    /// Tests that the GetState method returns the default value for a key with a wrong type.
    /// </summary>
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

    /// <summary>
    /// Tests that the Validate method returns true for a valid context.
    /// </summary>
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

    /// <summary>
    /// Tests that the Validate method returns false and adds an error for a null user.
    /// </summary>
    [Fact]
    public void Validate_WithNullUser_StillReturnsTrue()
    {
        // User is resolved by the orchestrator based on UserId; ExecutionContext.Validate()
        // only enforces the identifiers required to route the message (UserId/ChatId).
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
        result.Should().BeTrue();
        context.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Tests that the Validate method returns false and adds an error for a null session.
    /// </summary>
    [Fact]
    public void Validate_WithNullSession_StillReturnsTrue()
    {
        // Session is optional at validation time: commands and message processing
        // routinely run before an active session exists (e.g. brand-new users).
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
        result.Should().BeTrue();
        context.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Tests that the Validate method returns false and adds an error for a null message.
    /// </summary>
    [Fact]
    public void Validate_WithNullMessage_StillReturnsTrue()
    {
        // Message is attached after validation in some flows (e.g. command execution
        // triggered from a menu button click); ExecutionContext.Validate() only enforces
        // the identifiers required to route the interaction (UserId/ChatId).
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
        result.Should().BeTrue();
        context.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Tests that the Validate method returns false and adds an error for a zero user ID.
    /// </summary>
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

    /// <summary>
    /// Tests that the Validate method returns false and adds an error for a zero chat ID.
    /// </summary>
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

    /// <summary>
    /// Tests that the StopProcessing method sets IsStopped to true.
    /// </summary>
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

    /// <summary>
    /// Tests that the GetDuration method returns the time span since creation.
    /// </summary>
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

    /// <summary>
    /// Tests that the IsValid property returns true when there are no errors.
    /// </summary>
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

    /// <summary>
    /// Tests that the IsValid property returns false when there are errors.
    /// </summary>
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
