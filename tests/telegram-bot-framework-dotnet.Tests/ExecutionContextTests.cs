#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using TelegramBotFramework.Models;
using TelegramBotFramework.Tests;
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
        context.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, ExecutionContextTestsConstants.CreationTimeTolerance);
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
        var user = new BotUser { TelegramId = ExecutionContextTestsConstants.DefaultUserId, FirstName = ExecutionContextTestsConstants.DefaultFirstName };
        var session = new UserSession { SessionId = ExecutionContextTestsConstants.DefaultSessionId, UserId = ExecutionContextTestsConstants.DefaultUserId, ChatId = ExecutionContextTestsConstants.DefaultChatId };
        var message = new Message { MessageId = ExecutionContextTestsConstants.DefaultMessageId, UserId = ExecutionContextTestsConstants.DefaultUserId, ChatId = ExecutionContextTestsConstants.DefaultChatId, Content = ExecutionContextTestsConstants.DefaultTestMessage };

        // Act
        var context = new TelegramBotFramework.Models.ExecutionContext
        {
            UserId = ExecutionContextTestsConstants.DefaultUserId,
            ChatId = ExecutionContextTestsConstants.DefaultChatId,
            User = user,
            Session = session,
            Message = message
        };

        // Assert
        context.UserId.Should().Be(ExecutionContextTestsConstants.DefaultUserId);
        context.ChatId.Should().Be(ExecutionContextTestsConstants.DefaultChatId);
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
        context.AddError(ExecutionContextTestsConstants.TestErrorMessage1);
        context.AddError(ExecutionContextTestsConstants.TestErrorMessage2);

        // Assert
        context.Errors.Should().HaveCount(ExecutionContextTestsConstants.TwoItemCount);
        context.Errors.Should().Contain(ExecutionContextTestsConstants.TestErrorMessage1);
        context.Errors.Should().Contain(ExecutionContextTestsConstants.TestErrorMessage2);
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
        context.AddError(ExecutionContextTestsConstants.ValidErrorMessage);

        // Assert
        context.Errors.Should().HaveCount(ExecutionContextTestsConstants.SingleItemCount);
        context.Errors.Should().Contain(ExecutionContextTestsConstants.ValidErrorMessage);
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
        context.AddError(ExecutionContextTestsConstants.EmptyString);
        context.AddError(ExecutionContextTestsConstants.ValidErrorMessage);

        // Assert
        context.Errors.Should().HaveCount(ExecutionContextTestsConstants.SingleItemCount);
        context.Errors.Should().Contain(ExecutionContextTestsConstants.ValidErrorMessage);
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
        context.SetState(ExecutionContextTestsConstants.StateKey1, ExecutionContextTestsConstants.StateValue1);
        context.SetState(ExecutionContextTestsConstants.StateKey2, ExecutionContextTestsConstants.NumericStateValue);
        context.SetState(ExecutionContextTestsConstants.StateKey3, ExecutionContextTestsConstants.TrueValue);

        // Assert
        context.States.Should().HaveCount(ExecutionContextTestsConstants.ThreeItemCount);
        context.States.Should().ContainKey(ExecutionContextTestsConstants.StateKey1).WhoseValue.Should().Be(ExecutionContextTestsConstants.StateValue1);
        context.States.Should().ContainKey(ExecutionContextTestsConstants.StateKey2).WhoseValue.Should().Be(ExecutionContextTestsConstants.NumericStateValue);
        context.States.Should().ContainKey(ExecutionContextTestsConstants.StateKey3).WhoseValue.Should().Be(true);
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
        context.SetState(ExecutionContextTestsConstants.GenericStateKey, ExecutionContextTestsConstants.OldStateValue);
        context.SetState(ExecutionContextTestsConstants.GenericStateKey, ExecutionContextTestsConstants.NewStateValue);

        // Assert
        context.States.Should().HaveCount(ExecutionContextTestsConstants.SingleItemCount);
        context.States[ExecutionContextTestsConstants.GenericStateKey].Should().Be(ExecutionContextTestsConstants.NewStateValue);
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
        context.SetState(null, ExecutionContextTestsConstants.GenericStateValue);
        context.SetState(ExecutionContextTestsConstants.GenericStateKey, ExecutionContextTestsConstants.GenericStateValue);

        // Assert
        context.States.Should().HaveCount(ExecutionContextTestsConstants.SingleItemCount);
        context.States.Should().ContainKey(ExecutionContextTestsConstants.GenericStateKey);
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
        context.SetState(ExecutionContextTestsConstants.EmptyString, ExecutionContextTestsConstants.GenericStateValue);
        context.SetState(ExecutionContextTestsConstants.GenericStateKey, ExecutionContextTestsConstants.GenericStateValue);

        // Assert
        context.States.Should().HaveCount(ExecutionContextTestsConstants.SingleItemCount);
        context.States.Should().ContainKey(ExecutionContextTestsConstants.GenericStateKey);
    }

    /// <summary>
    /// Tests that the GetState method returns the value of an existing state.
    /// </summary>
    [Fact]
    public void GetState_WithExistingKey_ReturnsValue()
    {
        // Arrange
        var context = new TelegramBotFramework.Models.ExecutionContext();
        context.SetState(ExecutionContextTestsConstants.TestStateKey, ExecutionContextTestsConstants.TestStateValue);

        // Act
        var result = context.GetState<string>(ExecutionContextTestsConstants.TestStateKey);

        // Assert
        result.Should().Be(ExecutionContextTestsConstants.TestStateValue);
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
        var result = context.GetState<string>(ExecutionContextTestsConstants.NonExistentStateKey);

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
        context.SetState(ExecutionContextTestsConstants.NumberStateKey, ExecutionContextTestsConstants.NumericStateValue);

        // Act
        var result = context.GetState<string>(ExecutionContextTestsConstants.NumberStateKey);

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
            UserId = ExecutionContextTestsConstants.DefaultUserId,
            ChatId = ExecutionContextTestsConstants.DefaultChatId,
            User = new BotUser { UserId = ExecutionContextTestsConstants.DefaultUserId, FirstName = ExecutionContextTestsConstants.DefaultFirstName },
            Session = new UserSession { SessionId = ExecutionContextTestsConstants.DefaultSessionId, UserId = ExecutionContextTestsConstants.DefaultUserId, ChatId = ExecutionContextTestsConstants.DefaultChatId },
            Message = new Message { MessageId = ExecutionContextTestsConstants.DefaultMessageId, UserId = ExecutionContextTestsConstants.DefaultUserId, ChatId = ExecutionContextTestsConstants.DefaultChatId, Content = ExecutionContextTestsConstants.DefaultTestMessage }
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
            UserId = ExecutionContextTestsConstants.DefaultUserId,
            ChatId = ExecutionContextTestsConstants.DefaultChatId,
            User = null,
            Session = new UserSession { SessionId = ExecutionContextTestsConstants.DefaultSessionId, UserId = ExecutionContextTestsConstants.DefaultUserId, ChatId = ExecutionContextTestsConstants.DefaultChatId },
            Message = new Message { MessageId = ExecutionContextTestsConstants.DefaultMessageId, UserId = ExecutionContextTestsConstants.DefaultUserId, ChatId = ExecutionContextTestsConstants.DefaultChatId, Content = ExecutionContextTestsConstants.DefaultTestMessage }
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
            UserId = ExecutionContextTestsConstants.DefaultUserId,
            ChatId = ExecutionContextTestsConstants.DefaultChatId,
            User = new BotUser { UserId = ExecutionContextTestsConstants.DefaultUserId, FirstName = ExecutionContextTestsConstants.DefaultFirstName },
            Session = null,
            Message = new Message { MessageId = ExecutionContextTestsConstants.DefaultMessageId, UserId = ExecutionContextTestsConstants.DefaultUserId, ChatId = ExecutionContextTestsConstants.DefaultChatId, Content = ExecutionContextTestsConstants.DefaultTestMessage }
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
            UserId = ExecutionContextTestsConstants.DefaultUserId,
            ChatId = ExecutionContextTestsConstants.DefaultChatId,
            User = new BotUser { UserId = ExecutionContextTestsConstants.DefaultUserId, FirstName = ExecutionContextTestsConstants.DefaultFirstName },
            Session = new UserSession { SessionId = ExecutionContextTestsConstants.DefaultSessionId, UserId = ExecutionContextTestsConstants.DefaultUserId, ChatId = ExecutionContextTestsConstants.DefaultChatId },
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
            UserId = ExecutionContextTestsConstants.ZeroId,
            ChatId = ExecutionContextTestsConstants.DefaultChatId,
            User = new BotUser { UserId = ExecutionContextTestsConstants.ZeroId, FirstName = ExecutionContextTestsConstants.DefaultFirstName },
            Session = new UserSession { SessionId = ExecutionContextTestsConstants.DefaultSessionId, UserId = ExecutionContextTestsConstants.ZeroId, ChatId = ExecutionContextTestsConstants.DefaultChatId },
            Message = new Message { MessageId = ExecutionContextTestsConstants.DefaultMessageId, UserId = ExecutionContextTestsConstants.ZeroId, ChatId = ExecutionContextTestsConstants.DefaultChatId, Content = ExecutionContextTestsConstants.DefaultTestMessage }
        };

        // Act
        var result = context.Validate();

        // Assert
        result.Should().BeFalse();
        context.IsValid.Should().BeFalse();
        context.Errors.Should().Contain(e => e.Contains(ExecutionContextTestsConstants.UserIdErrorFragment));
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
            UserId = ExecutionContextTestsConstants.DefaultUserId,
            ChatId = ExecutionContextTestsConstants.ZeroId,
            User = new BotUser { UserId = ExecutionContextTestsConstants.DefaultUserId, FirstName = ExecutionContextTestsConstants.DefaultFirstName },
            Session = new UserSession { SessionId = ExecutionContextTestsConstants.DefaultSessionId, UserId = ExecutionContextTestsConstants.DefaultUserId, ChatId = ExecutionContextTestsConstants.ZeroId },
            Message = new Message { MessageId = ExecutionContextTestsConstants.DefaultMessageId, UserId = ExecutionContextTestsConstants.DefaultUserId, ChatId = ExecutionContextTestsConstants.ZeroId, Content = ExecutionContextTestsConstants.DefaultTestMessage }
        };

        // Act
        var result = context.Validate();

        // Assert
        result.Should().BeFalse();
        context.IsValid.Should().BeFalse();
        context.Errors.Should().Contain(e => e.Contains(ExecutionContextTestsConstants.ChatIdErrorFragment));
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
        Thread.Sleep(ExecutionContextTestsConstants.ShortSleepDuration);

        // Act
        var duration = context.GetDuration();

        // Assert
        duration.Should().BeCloseTo(DateTime.UtcNow - createdAt, TimeSpan.FromMilliseconds(ExecutionContextTestsConstants.TimeToleranceMilliseconds));
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
        context.AddError(ExecutionContextTestsConstants.EmptyString); // Won't add

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
        context.AddError(ExecutionContextTestsConstants.ValidationErrorMessage);

        // Assert
        context.IsValid.Should().BeFalse();
    }
}
