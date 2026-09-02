using FluentAssertions;
using TelegramBotFramework.Exceptions;
using static TelegramBotFramework.Tests.BotFrameworkExceptionTestsConstants;
using Xunit;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Verifies that bot framework exception constructors initialize messages, error codes, inner exceptions, and exception-specific properties correctly.
/// </summary>
public class BotFrameworkExceptionTests : IBotFrameworkExceptionTests
{
    /// <summary>
    /// Verifies that each supported <see cref="BotFrameworkException"/> constructor preserves its message and optional error code and inner exception.
    /// </summary>
    [Fact]
    public void BotFrameworkException_ShouldSetPropertiesCorrectly()
    {
        var message = TestMessage;
        var errorCode = TestErrorCode;
        var inner = new Exception(InnerExceptionMessage);

        var ex1 = new BotFrameworkException(message);
        ex1.Message.Should().Be(message);
        ex1.ErrorCode.Should().BeNull();

        var ex2 = new BotFrameworkException(message, inner);
        ex2.Message.Should().Be(message);
        ex2.InnerException.Should().Be(inner);

        var ex3 = new BotFrameworkException(message, errorCode);
        ex3.Message.Should().Be(message);
        ex3.ErrorCode.Should().Be(errorCode);

        var ex4 = new BotFrameworkException(message, errorCode, inner);
        ex4.Message.Should().Be(message);
        ex4.ErrorCode.Should().Be(errorCode);
        ex4.InnerException.Should().Be(inner);
    }

    /// <summary>
    /// Verifies that command execution and command-not-found exceptions expose the expected command name, error code, message, and inner exception.
    /// </summary>
    [Fact]
    public void CommandExceptions_ShouldSetPropertiesCorrectly()
    {
        var message = ExecutionFailedMessage;
        var command = TestCommandName;
        var inner = new Exception(InnerExceptionMessage);

        var ex1 = new CommandExecutionException(message, command);
        ex1.Message.Should().Be(message);
        ex1.ErrorCode.Should().Be(CommandExecutionErrorCode);
        ex1.CommandName.Should().Be(command);

        var ex2 = new CommandExecutionException(message, command, inner);
        ex2.InnerException.Should().Be(inner);
        ex2.CommandName.Should().Be(command);

        var ex3 = new CommandNotFoundException(command);
        ex3.Message.Should().Contain(command);
        ex3.ErrorCode.Should().Be(CommandNotFoundErrorCode);
        ex3.CommandName.Should().Be(command);
    }

    /// <summary>
    /// Verifies that permission and session exceptions expose their identifying values, error codes, and optional inner exception.
    /// </summary>
    [Fact]
    public void PermissionAndSessionExceptions_ShouldSetPropertiesCorrectly()
    {
        var userId = PermissionTestUserId;
        var permission = AdminPermission;
        var sessionId = TestSessionId;
        var message = SessionFailedMessage;
        var inner = new Exception(InnerExceptionMessage);

        var ex1 = new InsufficientPermissionException(userId, permission);
        ex1.UserId.Should().Be(userId);
        ex1.RequiredPermission.Should().Be(permission);
        ex1.ErrorCode.Should().Be(InsufficientPermissionErrorCode);

        var ex2 = new SessionException(message, sessionId);
        ex2.SessionId.Should().Be(sessionId);
        ex2.ErrorCode.Should().Be(SessionErrorCode);

        var ex3 = new SessionException(message, sessionId, inner);
        ex3.InnerException.Should().Be(inner);
    }

    /// <summary>
    /// Verifies that user and rate-limit exceptions expose the affected user, expected error codes, retry interval, and optional inner exception.
    /// </summary>
    [Fact]
    public void UserAndRateLimitExceptions_ShouldSetPropertiesCorrectly()
    {
        var userId = UserExceptionTestUserId;
        var retryAfter = RetryAfterSeconds;
        var message = UserFailedMessage;
        var inner = new Exception(InnerExceptionMessage);

        var ex1 = new UserException(message, userId);
        ex1.UserId.Should().Be(userId);
        ex1.ErrorCode.Should().Be(UserErrorCode);

        var ex2 = new UserException(message, userId, inner);
        ex2.InnerException.Should().Be(inner);

        var ex3 = new RateLimitExceededException(userId, retryAfter);
        ex3.UserId.Should().Be(userId);
        ex3.RetryAfterSeconds.Should().Be(retryAfter);
        ex3.ErrorCode.Should().Be(RateLimitExceededErrorCode);
    }

    /// <summary>
    /// Verifies that configuration and duplicate-update exceptions expose the expected error codes, update identifier, and optional inner exception.
    /// </summary>
    [Fact]
    public void ConfigurationAndDuplicateUpdateExceptions_ShouldSetPropertiesCorrectly()
    {
        var message = ConfigurationErrorMessage;
        var updateId = TestUpdateId;
        var inner = new Exception(InnerExceptionMessage);

        var ex1 = new ConfigurationException(message);
        ex1.ErrorCode.Should().Be(ConfigurationErrorCode);

        var ex2 = new ConfigurationException(message, inner);
        ex2.InnerException.Should().Be(inner);

        var ex3 = new DuplicateUpdateException(message, updateId);
        ex3.UpdateId.Should().Be(updateId);
        ex3.ErrorCode.Should().Be(DuplicateUpdateErrorCode);

        var ex4 = new DuplicateUpdateException(message, updateId, inner);
        ex4.InnerException.Should().Be(inner);
    }
}
