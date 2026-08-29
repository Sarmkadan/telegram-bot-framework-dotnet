#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Tests for AuthorizationMiddleware class
// =============================================================================

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;
using Xunit;
using ExecutionContext = TelegramBotFramework.Models.ExecutionContext;

namespace TelegramBotFramework.Middleware.Tests;

using static AuthorizationMiddlewareTestsConstants;

/// <summary>
/// Tests for the AuthorizationMiddleware class.
/// </summary>
public sealed class AuthorizationMiddlewareTests : IAuthorizationMiddlewareTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<ICommandService> _commandServiceMock;
    private readonly Mock<ILogger<AuthorizationMiddleware>> _loggerMock;

    public AuthorizationMiddlewareTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _commandServiceMock = new Mock<ICommandService>();
        _loggerMock = new Mock<ILogger<AuthorizationMiddleware>>();
    }

    /// <summary>
    /// Tests that middleware with invalid context passes to next.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenContextInvalid_PassesToNext()
    {
        // Arrange
        var middleware = new AuthorizationMiddleware(
            _userServiceMock.Object,
            _commandServiceMock.Object,
            _loggerMock.Object
        );

        var context = new ExecutionContext
        {
            UserId = InvalidUserId,
            ChatId = TestChatId,
            User = new BotUser { TelegramId = RegularUserId, FirstName = "Test" },
            IsValid = false
        };

        var nextCalled = false;
        Task<ExecutionContext> Next(ExecutionContext ctx)
        {
            nextCalled = true;
            return Task.FromResult(ctx);
        }

        // Act
        var result = await middleware.ProcessAsync(context, Next, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(context);
        nextCalled.Should().BeTrue();
        context.Errors.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that middleware with null user logs warning and passes to next.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenUserNull_LogsWarningAndPassesToNext()
    {
        // Arrange
        var middleware = new AuthorizationMiddleware(
            _userServiceMock.Object,
            _commandServiceMock.Object,
            _loggerMock.Object
        );

        var context = new ExecutionContext
        {
            UserId = RegularUserId,
            ChatId = TestChatId,
            User = null,
            IsValid = true
        };

        var nextCalled = false;
        Task<ExecutionContext> Next(ExecutionContext ctx)
        {
            nextCalled = true;
            return Task.FromResult(ctx);
        }

        // Act
        var result = await middleware.ProcessAsync(context, Next, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(context);
        nextCalled.Should().BeTrue();
        context.Errors.Should().ContainSingle(e => e.Contains("User not found in context for authorization"));
        _loggerMock.Invocations.Should().Contain(x => x.Arguments.Any(a =>
            a.ToString() != null && a.ToString().Contains("User not found")));
    }

    /// <summary>
    /// Tests that regular user without command passes through authorization.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenUserIsRegularAndNoCommand_PassesThrough()
    {
        // Arrange
        var middleware = new AuthorizationMiddleware(
            _userServiceMock.Object,
            _commandServiceMock.Object,
            _loggerMock.Object
        );

        var context = new ExecutionContext
        {
            UserId = RegularUserId,
            ChatId = TestChatId,
            User = new BotUser { TelegramId = RegularUserId, FirstName = RegularUserFirstName, Role = UserRole.User },
            IsValid = true
        };

        var nextCalled = false;
        Task<ExecutionContext> Next(ExecutionContext ctx)
        {
            nextCalled = true;
            return Task.FromResult(ctx);
        }

        // Act
        var result = await middleware.ProcessAsync(context, Next, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(context);
        nextCalled.Should().BeTrue();
        context.Errors.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that admin user passes authorization without command.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenUserIsAdminAndNoCommand_PassesThrough()
    {
        // Arrange
        var middleware = new AuthorizationMiddleware(
            _userServiceMock.Object,
            _commandServiceMock.Object,
            _loggerMock.Object
        );

        var context = new ExecutionContext
        {
            UserId = AdminUserId,
            ChatId = TestChatId,
            User = new BotUser { TelegramId = AdminUserId, FirstName = AdminUserFirstName, Role = UserRole.Admin },
            IsValid = true
        };

        var nextCalled = false;
        Task<ExecutionContext> Next(ExecutionContext ctx)
        {
            nextCalled = true;
            return Task.FromResult(ctx);
        }

        // Act
        var result = await middleware.ProcessAsync(context, Next, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(context);
        nextCalled.Should().BeTrue();
        context.Errors.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that regular user is blocked when trying to execute admin command.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenRegularUserTriesAdminCommand_BlocksAndAddsError()
    {
        // Arrange
        var command = new Command
        {
            Name = AdminCommandName,
            Description = AdminCommandDescription,
            HandlerType = AdminCommandHandlerType,
            RequiresAdmin = true,
            IsEnabled = true
        };

        _commandServiceMock.Setup(x => x.GetCommandAsync(AdminCommandName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(command);

        var middleware = new AuthorizationMiddleware(
            _userServiceMock.Object,
            _commandServiceMock.Object,
            _loggerMock.Object
        );

        var context = new ExecutionContext
        {
            UserId = RegularUserId,
            ChatId = TestChatId,
            User = new BotUser { TelegramId = RegularUserId, FirstName = RegularUserFirstName, Role = UserRole.User },
            Command = new Command { Name = AdminCommandName },
            IsValid = true
        };

        var nextCalled = false;
        Task<ExecutionContext> Next(ExecutionContext ctx)
        {
            nextCalled = true;
            return Task.FromResult(ctx);
        }

        // Act
        var result = await middleware.ProcessAsync(context, Next, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(context);
        nextCalled.Should().BeFalse(); // next should not be called
        context.Errors.Should().ContainSingle(e => e.Contains(UnauthorizedCommandErrorFragment));
        context.IsValid.Should().BeFalse();
        _loggerMock.Invocations.Should().Contain(x => x.Arguments.Any(a =>
            a.ToString() != null && a.ToString().Contains("denied access to command")));
    }

    /// <summary>
    /// Tests that admin user passes admin command authorization.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenAdminUserExecutesAdminCommand_PassesThrough()
    {
        // Arrange
        var command = new Command
        {
            Name = AdminCommandName,
            Description = AdminCommandDescription,
            HandlerType = AdminCommandHandlerType,
            RequiresAdmin = true,
            IsEnabled = true
        };

        _commandServiceMock.Setup(x => x.GetCommandAsync(AdminCommandName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(command);

        var middleware = new AuthorizationMiddleware(
            _userServiceMock.Object,
            _commandServiceMock.Object,
            _loggerMock.Object
        );

        var context = new ExecutionContext
        {
            UserId = AdminUserId,
            ChatId = TestChatId,
            User = new BotUser { TelegramId = AdminUserId, FirstName = AdminUserFirstName, Role = UserRole.Admin },
            Command = new Command { Name = AdminCommandName },
            IsValid = true
        };

        var nextCalled = false;
        Task<ExecutionContext> Next(ExecutionContext ctx)
        {
            nextCalled = true;
            return Task.FromResult(ctx);
        }

        // Act
        var result = await middleware.ProcessAsync(context, Next, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(context);
        nextCalled.Should().BeTrue();
        context.Errors.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that moderator user is blocked when trying to execute admin command.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenModeratorTriesAdminCommand_BlocksAndAddsError()
    {
        // Arrange
        var command = new Command
        {
            Name = AdminCommandName,
            Description = AdminCommandDescription,
            HandlerType = AdminCommandHandlerType,
            RequiresAdmin = true,
            IsEnabled = true
        };

        _commandServiceMock.Setup(x => x.GetCommandAsync(AdminCommandName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(command);

        var middleware = new AuthorizationMiddleware(
            _userServiceMock.Object,
            _commandServiceMock.Object,
            _loggerMock.Object
        );

        var context = new ExecutionContext
        {
            UserId = ModeratorUserId,
            ChatId = TestChatId,
            User = new BotUser { TelegramId = ModeratorUserId, FirstName = "Moderator", Role = UserRole.Moderator },
            Command = new Command { Name = AdminCommandName },
            IsValid = true
        };

        var nextCalled = false;
        Task<ExecutionContext> Next(ExecutionContext ctx)
        {
            nextCalled = true;
            return Task.FromResult(ctx);
        }

        // Act
        var result = await middleware.ProcessAsync(context, Next, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(context);
        nextCalled.Should().BeFalse(); // next should not be called
        context.Errors.Should().ContainSingle(e => e.Contains(UnauthorizedCommandErrorFragment));
        context.IsValid.Should().BeFalse();
    }

    /// <summary>
    /// Tests that user with admin role passes admin command authorization.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenUserHasAdminRoleExecutesAdminCommand_PassesThrough()
    {
        // Arrange
        var command = new Command
        {
            Name = AdminCommandName,
            Description = AdminCommandDescription,
            HandlerType = AdminCommandHandlerType,
            RequiresAdmin = true,
            IsEnabled = true
        };

        _commandServiceMock.Setup(x => x.GetCommandAsync(AdminCommandName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(command);

        var middleware = new AuthorizationMiddleware(
            _userServiceMock.Object,
            _commandServiceMock.Object,
            _loggerMock.Object
        );

        var context = new ExecutionContext
        {
            UserId = AdminUserId,
            ChatId = TestChatId,
            User = new BotUser { TelegramId = AdminUserId, FirstName = AdminUserFirstName, Role = UserRole.Administrator },
            Command = new Command { Name = AdminCommandName },
            IsValid = true
        };

        var nextCalled = false;
        Task<ExecutionContext> Next(ExecutionContext ctx)
        {
            nextCalled = true;
            return Task.FromResult(ctx);
        }

        // Act
        var result = await middleware.ProcessAsync(context, Next, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(context);
        nextCalled.Should().BeTrue();
        context.Errors.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that user without command can execute regular commands.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenUserWithoutCommand_ExecutesRegularCommands()
    {
        // Arrange
        var middleware = new AuthorizationMiddleware(
            _userServiceMock.Object,
            _commandServiceMock.Object,
            _loggerMock.Object
        );

        var context = new ExecutionContext
        {
            UserId = RegularUserId,
            ChatId = TestChatId,
            User = new BotUser { TelegramId = RegularUserId, FirstName = "User", Role = UserRole.User },
            IsValid = true
        };

        var nextCalled = false;
        Task<ExecutionContext> Next(ExecutionContext ctx)
        {
            nextCalled = true;
            return Task.FromResult(ctx);
        }

        // Act
        var result = await middleware.ProcessAsync(context, Next, CancellationToken.None);

        // Assert
        result.Should().BeSameAs(context);
        nextCalled.Should().BeTrue();
        context.Errors.Should().BeEmpty();
    }

    /// <summary>
    /// Tests middleware priority.
    /// </summary>
    [Fact]
    public void Priority_ReturnsCorrectValue()
    {
        // Arrange
        var middleware = new AuthorizationMiddleware(
            _userServiceMock.Object,
            _commandServiceMock.Object,
            _loggerMock.Object
        );

        // Act & Assert
        middleware.Priority.Should().Be(ExpectedMiddlewarePriority);
    }

    /// <summary>
    /// Tests that middleware with null command service throws.
    /// </summary>
    [Fact]
    public void Constructor_WhenCommandServiceNull_Throws()
    {
        // Act
        var act = () => new AuthorizationMiddleware(
            _userServiceMock.Object,
            null!,
            _loggerMock.Object
        );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that middleware with null user service throws.
    /// </summary>
    [Fact]
    public void Constructor_WhenUserServiceNull_Throws()
    {
        // Act
        var act = () => new AuthorizationMiddleware(
            null!,
            _commandServiceMock.Object,
            _loggerMock.Object
        );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that middleware with null logger throws.
    /// </summary>
    [Fact]
    public void Constructor_WhenLoggerNull_Throws()
    {
        // Act
        var act = () => new AuthorizationMiddleware(
            _userServiceMock.Object,
            _commandServiceMock.Object,
            null!
        );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
