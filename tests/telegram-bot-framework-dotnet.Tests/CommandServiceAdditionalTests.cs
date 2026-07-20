#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TelegramBotFramework.Models;
using TelegramBotFramework.Repositories;
using TelegramBotFramework.Services;
using Xunit;
using ExecutionContext = TelegramBotFramework.Models.ExecutionContext;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Additional test suite for <see cref="CommandService"/> functionality covering advanced scenarios
/// such as role-based command filtering, command execution tracking, and rate limiting.
/// </summary>
public sealed class CommandServiceAdditionalTests
{
    private readonly Mock<ICommandRepository> _mockRepository = new();
    private readonly Mock<IUserService> _mockUserService = new();
    private readonly Mock<ICommandUsageTracker> _mockCommandUsageTracker = new();
    private readonly Mock<ILogger<CommandService>> _mockLogger = new();
    private readonly CommandService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandServiceAdditionalTests"/> class.
    /// Sets up mock dependencies and creates a new <see cref="CommandService"/> instance for testing.
    /// </summary>
    public CommandServiceAdditionalTests()
    {
        _service = new CommandService(_mockRepository.Object, _mockUserService.Object, _mockCommandUsageTracker.Object, _mockLogger.Object);
    }

    /// <summary>
    /// Tests that users with Administrator role can access both admin and regular commands.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetAvailableCommandsAsync_WithAdminRole_ReturnsAdminCommands()
    {
        // Arrange
        var adminCommand = new Command { Name = "/admin", IsEnabled = true, RequiresAdmin = true };
        var regularCommand = new Command { Name = "/help", IsEnabled = true, RequiresAdmin = false };
        var disabledCommand = new Command { Name = "/disabled", IsEnabled = false, RequiresAdmin = false };

        _mockRepository
            .Setup(r => r.GetEnabledAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Command> { adminCommand, regularCommand, disabledCommand });

        // Act
        var result = await _service.GetAvailableCommandsAsync(UserRole.Administrator).ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Name == "/admin");
        result.Should().Contain(c => c.Name == "/help");
    }

    /// <summary>
    /// Tests that regular users can only access commands that don't require admin privileges.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetAvailableCommandsAsync_WithUserRole_ReturnsOnlyNonAdminCommands()
    {
        // Arrange
        var adminCommand = new Command { Name = "/admin", IsEnabled = true, RequiresAdmin = true };
        var regularCommand = new Command { Name = "/help", IsEnabled = true, RequiresAdmin = false };

        _mockRepository
            .Setup(r => r.GetEnabledAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Command> { adminCommand, regularCommand });

        // Act
        var result = await _service.GetAvailableCommandsAsync(UserRole.User).ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(c => c.Name == "/help");
        result.Should().NotContain(c => c.Name == "/admin");
    }

    /// <summary>
    /// Tests that moderators can access commands for moderators and above (excluding admin-only commands).
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetAvailableCommandsAsync_WithModeratorRole_ReturnsCommandsForModeratorAndAbove()
    {
        // Arrange
        var adminCommand = new Command { Name = "/admin", IsEnabled = true, RequiresAdmin = true };
        var moderatorCommand = new Command { Name = "/moderate", IsEnabled = true, RequiresAdmin = false };
        var userCommand = new Command { Name = "/help", IsEnabled = true, RequiresAdmin = false };

        _mockRepository
            .Setup(r => r.GetEnabledAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Command> { adminCommand, moderatorCommand, userCommand });

        // Act
        var result = await _service.GetAvailableCommandsAsync(UserRole.Moderator).ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Name == "/moderate");
        result.Should().Contain(c => c.Name == "/help");
        result.Should().NotContain(c => c.Name == "/admin");
    }

    /// <summary>
    /// Tests that a valid command execution completes successfully and updates the command's execution count.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteCommandAsync_WithValidContext_ExecutesSuccessfully()
    {
        // Arrange
        var command = new Command { Name = "/test", IsEnabled = true, HandlerType = "TestHandler" };
        var user = new BotUser { UserId = 123, FirstName = "John", Role = UserRole.User };
        var context = new ExecutionContext
        {
            UserId = 123,
            ChatId = 456,
            User = user,
            Command = command,
            Message = new Message { MessageId = 1, Content = "/test", Type = MessageType.Text }
        };

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(command);

        // Act
        var result = await _service.ExecuteCommandAsync(context).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Command.Should().NotBeNull();
        result.Command!.ExecutionCount.Should().Be(1);
        _mockRepository.Verify(r => r.UpdateAsync(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that attempting to execute a disabled command results in an error and no execution count update.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteCommandAsync_WithDisabledCommand_AddsErrorToContext()
    {
        // Arrange
        var command = new Command { Name = "/disabled", IsEnabled = false, HandlerType = "TestHandler" };
        var user = new BotUser { UserId = 123, FirstName = "John", Role = UserRole.Administrator };
        var context = new ExecutionContext
        {
            UserId = 123,
            ChatId = 456,
            User = user,
            Command = command,
            Message = new Message { MessageId = 1, Content = "/disabled", Type = MessageType.Text }
        };

        // Act
        var result = await _service.ExecuteCommandAsync(context).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("disabled"));
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Command>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that users without sufficient permissions receive an error when attempting to execute admin-only commands.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteCommandAsync_WithInsufficientPermissions_AddsErrorToContext()
    {
        // Arrange
        var command = new Command { Name = "/admin", IsEnabled = true, RequiresAdmin = true, HandlerType = "TestHandler" };
        var user = new BotUser { UserId = 123, FirstName = "John", Role = UserRole.User };
        var context = new ExecutionContext
        {
            UserId = 123,
            ChatId = 456,
            User = user,
            Command = command,
            Message = new Message { MessageId = 1, Content = "/admin", Type = MessageType.Text }
        };

        // Act
        var result = await _service.ExecuteCommandAsync(context).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Insufficient permissions"));
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Command>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that inactive users (e.g., banned) cannot execute any commands.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CanUserExecuteCommandAsync_WithInactiveUser_ReturnsFalse()
    {
        // Arrange
        var user = new BotUser { UserId = 123, FirstName = "John", Status = UserStatus.Banned };
        var command = new Command { Name = "/test", IsEnabled = true };

        _mockUserService
            .Setup(u => u.GetUserByIdAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockRepository
            .Setup(r => r.GetByNameAsync("/test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(command);

        // Act
        var result = await _service.CanUserExecuteCommandAsync(123, "test").ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that attempting to check permissions for a non-existent command returns false.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CanUserExecuteCommandAsync_WithNonExistentCommand_ReturnsFalse()
    {
        // Arrange
        var user = new BotUser { UserId = 123, FirstName = "John", Status = UserStatus.Active };

        _mockUserService
            .Setup(u => u.GetUserByIdAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockRepository
            .Setup(r => r.GetByNameAsync("/nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Command?)null);

        // Act
        var result = await _service.CanUserExecuteCommandAsync(123, "nonexistent").ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that disabled commands cannot be executed even by authorized users.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CanUserExecuteCommandAsync_WithDisabledCommand_ReturnsFalse()
    {
        // Arrange
        var user = new BotUser { UserId = 123, FirstName = "John", Status = UserStatus.Active };
        var command = new Command { Name = "/disabled", IsEnabled = false };

        _mockUserService
            .Setup(u => u.GetUserByIdAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockRepository
            .Setup(r => r.GetByNameAsync("/disabled", It.IsAny<CancellationToken>()))
            .ReturnsAsync(command);

        // Act
        var result = await _service.CanUserExecuteCommandAsync(123, "disabled").ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that executing a command successfully increments its execution count in the repository.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task RecordCommandExecutionAsync_WithValidCommand_IncrementsExecutionCount()
    {
        // Arrange
        var command = new Command { Name = "/test", HandlerType = "TestHandler", ExecutionCount = 5 };

        _mockRepository
            .Setup(r => r.GetByNameAsync("/test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(command);
        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(command);

        // Act
        await _service.RecordCommandExecutionAsync("test").ConfigureAwait(false);

        // Assert
        command.ExecutionCount.Should().Be(6);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Command>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that attempting to record execution for a non-existent command doesn't throw an exception.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task RecordCommandExecutionAsync_WithNonExistentCommand_DoesNotThrow()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByNameAsync("/nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Command?)null);

        // Act & Assert
        await _service.Invoking(s => s.RecordCommandExecutionAsync("nonexistent"))
            .Should().NotThrowAsync();
    }

    /// <summary>
    /// Tests that retrieving the execution count for an existing command returns the correct value.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetCommandExecutionCountAsync_WithExistingCommand_ReturnsCount()
    {
        // Arrange
        var command = new Command { Name = "/test", ExecutionCount = 42 };

        _mockRepository
            .Setup(r => r.GetByNameAsync("/test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(command);

        // Act
        var result = await _service.GetCommandExecutionCountAsync("test").ConfigureAwait(false);

        // Assert
        result.Should().Be(42);
    }

    /// <summary>
    /// Tests that retrieving the execution count for a non-existent command returns zero.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetCommandExecutionCountAsync_WithNonExistentCommand_ReturnsZero()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByNameAsync("/nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Command?)null);

        // Act
        var result = await _service.GetCommandExecutionCountAsync("nonexistent").ConfigureAwait(false);

        // Assert
        result.Should().Be(0);
    }

    /// <summary>
    /// Tests that commands without rate limiting configured are never considered rate limited.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task IsCommandRateLimitedAsync_WithNoRateLimitConfigured_ReturnsFalse()
    {
        // Arrange
        var command = new Command { Name = "/test", RateLimitPerMinute = null };

        _mockRepository
            .Setup(r => r.GetByNameAsync("/test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(command);

        // Act
        var result = await _service.IsCommandRateLimitedAsync(123, "test").ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that rate limiting is applied per user, allowing different users to execute commands
    /// even when the same command has a rate limit configured.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task IsCommandRateLimitedAsync_WithMultipleUsers_ResetsRateLimitPerUser()
    {
        // Arrange
        var command = new Command { Name = "/test", RateLimitPerMinute = 1 };

        _mockRepository
            .Setup(r => r.GetByNameAsync("/test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(command);

        // First user - should not be rate limited
        var result1 = await _service.IsCommandRateLimitedAsync(100, "test").ConfigureAwait(false);
        result1.Should().BeFalse();

        // Second user - should not be rate limited (different user)
        var result2 = await _service.IsCommandRateLimitedAsync(200, "test").ConfigureAwait(false);
        result2.Should().BeFalse();

        // First user again - should be rate limited now
        var result3 = await _service.IsCommandRateLimitedAsync(100, "test").ConfigureAwait(false);
        result3.Should().BeTrue();
    }
}
