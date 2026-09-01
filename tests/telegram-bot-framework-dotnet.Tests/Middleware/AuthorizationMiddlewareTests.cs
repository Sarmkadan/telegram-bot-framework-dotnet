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
        _loggerMock.Object.LogInformation("Starting invalid context authorization test for user {UserId} and chat {ChatId}", InvalidUserId, TestChatId);

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

        _loggerMock.Object.LogWarning("Authorization used invalid-context fallback for user {UserId}; next called: {NextCalled}", context.UserId, nextCalled);

        // Assert
        result.Should().BeSameAs(context);
        nextCalled.Should().BeTrue();
        context.Errors.Should().BeEmpty();

        _loggerMock.Object.LogInformation("Completed invalid context authorization test for user {UserId}; context valid: {IsValid}", context.UserId, context.IsValid);
    }

    /// <summary>
    /// Tests that middleware with null user logs warning and passes to next.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenUserNull_LogsWarningAndPassesToNext()
    {
        _loggerMock.Object.LogInformation("Starting missing-user authorization test for user {UserId} and chat {ChatId}", RegularUserId, TestChatId);

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

        _loggerMock.Object.LogWarning("Authorization used missing-user fallback for user {UserId}; next called: {NextCalled}", context.UserId, nextCalled);

        // Assert
        result.Should().BeSameAs(context);
        nextCalled.Should().BeTrue();
        context.Errors.Should().ContainSingle(e => e.Contains("User not found in context for authorization"));
        _loggerMock.Invocations.Should().Contain(x => x.Arguments.Any(a =>
            a.ToString() != null && a.ToString().Contains("User not found")));

        _loggerMock.Object.LogInformation("Completed missing-user authorization test for user {UserId}; error count: {ErrorCount}", context.UserId, context.Errors.Count);
    }

    /// <summary>
    /// Tests that regular user without command passes through authorization.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenUserIsRegularAndNoCommand_PassesThrough()
    {
        _loggerMock.Object.LogInformation("Starting regular-user authorization test for user {UserId} with command {CommandName}", RegularUserId, null);

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

        _loggerMock.Object.LogInformation("Completed regular-user authorization test for user {UserId}; next called: {NextCalled}", context.UserId, nextCalled);
    }

    /// <summary>
    /// Tests that admin user passes authorization without command.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenUserIsAdminAndNoCommand_PassesThrough()
    {
        _loggerMock.Object.LogInformation("Starting admin authorization test for user {UserId} with command {CommandName}", AdminUserId, null);

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

        _loggerMock.Object.LogInformation("Completed admin authorization test for user {UserId}; next called: {NextCalled}", context.UserId, nextCalled);
    }

    /// <summary>
    /// Tests that regular user is blocked when trying to execute admin command.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenRegularUserTriesAdminCommand_BlocksAndAddsError()
    {
        _loggerMock.Object.LogInformation("Starting admin-command authorization test for user {UserId} and command {CommandName}", RegularUserId, AdminCommandName);

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

        _loggerMock.Object.LogWarning("Authorization denied for user {UserId} with role {UserRole} on command {CommandName}", context.UserId, context.User?.Role, context.Command?.Name);

        // Assert
        result.Should().BeSameAs(context);
        nextCalled.Should().BeFalse(); // next should not be called
        context.Errors.Should().ContainSingle(e => e.Contains(UnauthorizedCommandErrorFragment));
        context.IsValid.Should().BeFalse();
        _loggerMock.Invocations.Should().Contain(x => x.Arguments.Any(a =>
            a.ToString() != null && a.ToString().Contains("denied access to command")));

        _loggerMock.Object.LogInformation("Completed denied admin-command authorization test for user {UserId}; context valid: {IsValid}", context.UserId, context.IsValid);
    }

    /// <summary>
    /// Tests that admin user passes admin command authorization.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenAdminUserExecutesAdminCommand_PassesThrough()
    {
        _loggerMock.Object.LogInformation("Starting admin-command authorization test for admin user {UserId} and command {CommandName}", AdminUserId, AdminCommandName);

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

        _loggerMock.Object.LogInformation("Completed admin-command authorization test for admin user {UserId}; next called: {NextCalled}", context.UserId, nextCalled);
    }

    /// <summary>
    /// Tests that moderator user is blocked when trying to execute admin command.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenModeratorTriesAdminCommand_BlocksAndAddsError()
    {
        _loggerMock.Object.LogInformation("Starting admin-command authorization test for moderator {UserId} and command {CommandName}", ModeratorUserId, AdminCommandName);

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

        _loggerMock.Object.LogWarning("Authorization denied for moderator {UserId} with role {UserRole} on command {CommandName}", context.UserId, context.User?.Role, context.Command?.Name);

        // Assert
        result.Should().BeSameAs(context);
        nextCalled.Should().BeFalse(); // next should not be called
        context.Errors.Should().ContainSingle(e => e.Contains(UnauthorizedCommandErrorFragment));
        context.IsValid.Should().BeFalse();

        _loggerMock.Object.LogInformation("Completed denied moderator authorization test for user {UserId}; context valid: {IsValid}", context.UserId, context.IsValid);
    }

    /// <summary>
    /// Tests that user with admin role passes admin command authorization.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenUserHasAdminRoleExecutesAdminCommand_PassesThrough()
    {
        _loggerMock.Object.LogInformation("Starting administrator-role authorization test for user {UserId} and command {CommandName}", AdminUserId, AdminCommandName);

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

        _loggerMock.Object.LogInformation("Completed administrator-role authorization test for user {UserId}; next called: {NextCalled}", context.UserId, nextCalled);
    }

    /// <summary>
    /// Tests that user without command can execute regular commands.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenUserWithoutCommand_ExecutesRegularCommands()
    {
        _loggerMock.Object.LogInformation("Starting no-command authorization test for user {UserId} and chat {ChatId}", RegularUserId, TestChatId);

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

        _loggerMock.Object.LogInformation("Completed no-command authorization test for user {UserId}; next called: {NextCalled}", context.UserId, nextCalled);
    }

    /// <summary>
    /// Tests middleware priority.
    /// </summary>
    [Fact]
    public void Priority_ReturnsCorrectValue()
    {
        _loggerMock.Object.LogInformation("Starting middleware priority test with expected priority {ExpectedPriority}", ExpectedMiddlewarePriority);

        // Arrange
        var middleware = new AuthorizationMiddleware(
            _userServiceMock.Object,
            _commandServiceMock.Object,
            _loggerMock.Object
        );

        // Act & Assert
        middleware.Priority.Should().Be(ExpectedMiddlewarePriority);

        _loggerMock.Object.LogInformation("Completed middleware priority test with actual priority {ActualPriority}", middleware.Priority);
    }

    /// <summary>
    /// Tests that middleware with null command service throws.
    /// </summary>
    [Fact]
    public void Constructor_WhenCommandServiceNull_Throws()
    {
        _loggerMock.Object.LogInformation("Starting null command service constructor test for {MiddlewareType}", nameof(AuthorizationMiddleware));

        // Act
        var act = () => new AuthorizationMiddleware(
            _userServiceMock.Object,
            null!,
            _loggerMock.Object
        );

        // Assert
        act.Should().Throw<ArgumentNullException>();

        _loggerMock.Object.LogInformation("Completed null command service constructor test for {MiddlewareType}", nameof(AuthorizationMiddleware));
    }

    /// <summary>
    /// Tests that middleware with null user service throws.
    /// </summary>
    [Fact]
    public void Constructor_WhenUserServiceNull_Throws()
    {
        _loggerMock.Object.LogInformation("Starting null user service constructor test for {MiddlewareType}", nameof(AuthorizationMiddleware));

        // Act
        var act = () => new AuthorizationMiddleware(
            null!,
            _commandServiceMock.Object,
            _loggerMock.Object
        );

        // Assert
        act.Should().Throw<ArgumentNullException>();

        _loggerMock.Object.LogInformation("Completed null user service constructor test for {MiddlewareType}", nameof(AuthorizationMiddleware));
    }

    /// <summary>
    /// Tests that middleware with null logger throws.
    /// </summary>
    [Fact]
    public void Constructor_WhenLoggerNull_Throws()
    {
        _loggerMock.Object.LogInformation("Starting null logger constructor test for {MiddlewareType}", nameof(AuthorizationMiddleware));

        // Act
        var act = () => new AuthorizationMiddleware(
            _userServiceMock.Object,
            _commandServiceMock.Object,
            null!
        );

        // Assert
        act.Should().Throw<ArgumentNullException>();

        _loggerMock.Object.LogInformation("Completed null logger constructor test for {MiddlewareType}", nameof(AuthorizationMiddleware));
    }
}
