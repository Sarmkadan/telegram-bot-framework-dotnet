using FluentAssertions;
using TelegramBotFramework.Exceptions;
using Xunit;

namespace TelegramBotFramework.Tests;

public class BotFrameworkExceptionTests : IBotFrameworkExceptionTests
{
    [Fact]
    public void BotFrameworkException_ShouldSetPropertiesCorrectly()
    {
        var message = "test message";
        var errorCode = "TEST_ERROR";
        var inner = new Exception("inner");

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

    [Fact]
    public void CommandExceptions_ShouldSetPropertiesCorrectly()
    {
        var message = "execution failed";
        var command = "test_command";
        var inner = new Exception("inner");

        var ex1 = new CommandExecutionException(message, command);
        ex1.Message.Should().Be(message);
        ex1.ErrorCode.Should().Be("COMMAND_EXECUTION_ERROR");
        ex1.CommandName.Should().Be(command);

        var ex2 = new CommandExecutionException(message, command, inner);
        ex2.InnerException.Should().Be(inner);
        ex2.CommandName.Should().Be(command);

        var ex3 = new CommandNotFoundException(command);
        ex3.Message.Should().Contain(command);
        ex3.ErrorCode.Should().Be("COMMAND_NOT_FOUND");
        ex3.CommandName.Should().Be(command);
    }

    [Fact]
    public void PermissionAndSessionExceptions_ShouldSetPropertiesCorrectly()
    {
        var userId = 123L;
        var permission = "admin";
        var sessionId = "session_abc";
        var message = "session failed";
        var inner = new Exception("inner");

        var ex1 = new InsufficientPermissionException(userId, permission);
        ex1.UserId.Should().Be(userId);
        ex1.RequiredPermission.Should().Be(permission);
        ex1.ErrorCode.Should().Be("INSUFFICIENT_PERMISSION");

        var ex2 = new SessionException(message, sessionId);
        ex2.SessionId.Should().Be(sessionId);
        ex2.ErrorCode.Should().Be("SESSION_ERROR");

        var ex3 = new SessionException(message, sessionId, inner);
        ex3.InnerException.Should().Be(inner);
    }

    [Fact]
    public void UserAndRateLimitExceptions_ShouldSetPropertiesCorrectly()
    {
        var userId = 456L;
        var retryAfter = 30;
        var message = "user failed";
        var inner = new Exception("inner");

        var ex1 = new UserException(message, userId);
        ex1.UserId.Should().Be(userId);
        ex1.ErrorCode.Should().Be("USER_ERROR");

        var ex2 = new UserException(message, userId, inner);
        ex2.InnerException.Should().Be(inner);

        var ex3 = new RateLimitExceededException(userId, retryAfter);
        ex3.UserId.Should().Be(userId);
        ex3.RetryAfterSeconds.Should().Be(retryAfter);
        ex3.ErrorCode.Should().Be("RATE_LIMIT_EXCEEDED");
    }

    [Fact]
    public void ConfigurationAndDuplicateUpdateExceptions_ShouldSetPropertiesCorrectly()
    {
        var message = "config error";
        var updateId = 789L;
        var inner = new Exception("inner");

        var ex1 = new ConfigurationException(message);
        ex1.ErrorCode.Should().Be("CONFIGURATION_ERROR");

        var ex2 = new ConfigurationException(message, inner);
        ex2.InnerException.Should().Be(inner);

        var ex3 = new DuplicateUpdateException(message, updateId);
        ex3.UpdateId.Should().Be(updateId);
        ex3.ErrorCode.Should().Be("DUPLICATE_UPDATE");

        var ex4 = new DuplicateUpdateException(message, updateId, inner);
        ex4.InnerException.Should().Be(inner);
    }
}
