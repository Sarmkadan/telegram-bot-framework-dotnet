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
public sealed class CommandServiceAdditionalTests : ICommandServiceAdditionalTests
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
        _mockLogger.Object.LogInformation("Initializing {ClassName}", nameof(CommandServiceAdditionalTests));
        _service = new CommandService(_mockRepository.Object, _mockUserService.Object, _mockCommandUsageTracker.Object, _mockLogger.Object);
    }

    /// <summary>
    /// Tests that users with Administrator role can access both admin and regular commands.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetAvailableCommandsAsync_WithAdminRole_ReturnsAdminCommands()
    {
        _mockLogger.Object.LogInformation("Starting test {TestMethod}", nameof(GetAvailableCommandsAsync_WithAdminRole_ReturnsAdminCommands));
        // Arrange
        var adminCommand = new Command { Name = CommandServiceAdditionalTestsConstants.AdminCommandName, IsEnabled = true, RequiresAdmin = true };
        var regularCommand = new Command { Name = CommandServiceAdditionalTestsConstants.HelpCommandName, IsEnabled = true, RequiresAdmin = false };
        var disabledCommand = new Command { Name = CommandServiceAdditionalTestsConstants.DisabledCommandName, IsEnabled = false, RequiresAdmin = false };

        _mockRepository
            .Setup(r => r.GetEnabledAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Command> { adminCommand, regularCommand, disabledCommand });

        // Act
        var result = await _service.GetAvailableCommandsAsync(UserRole.Administrator).ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(CommandServiceAdditionalTestsConstants.AdminAvailableCommandCount);
        result.Should().Contain(c => c.Name == CommandServiceAdditionalTestsConstants.AdminCommandName);
        result.Should().Contain(c => c.Name == CommandServiceAdditionalTestsConstants.HelpCommandName);
        _mockLogger.Object.LogInformation("Finished test {TestMethod}", nameof(GetAvailableCommandsAsync_WithAdminRole_ReturnsAdminCommands));
    }

    /// <summary>
    /// Tests that regular users can only access commands that don't require admin privileges.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetAvailableCommandsAsync_WithUserRole_ReturnsOnlyNonAdminCommands()
    {
        _mockLogger.Object.LogInformation("Starting test {TestMethod}", nameof(GetAvailableCommandsAsync_WithUserRole_ReturnsOnlyNonAdminCommands));
        // Arrange
        var adminCommand = new Command { Name = CommandServiceAdditionalTestsConstants.AdminCommandName, IsEnabled = true, RequiresAdmin = true };
        var regularCommand = new Command { Name = CommandServiceAdditionalTestsConstants.HelpCommandName, IsEnabled = true, RequiresAdmin = false };

        _mockRepository
            .Setup(r => r.GetEnabledAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Command> { adminCommand, regularCommand });

        // Act
        var result = await _service.GetAvailableCommandsAsync(UserRole.User).ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(CommandServiceAdditionalTestsConstants.UserAvailableCommandCount);
        result.Should().Contain(c => c.Name == CommandServiceAdditionalTestsConstants.HelpCommandName);
        result.Should().NotContain(c => c.Name == CommandServiceAdditionalTestsConstants.AdminCommandName);
        _mockLogger.Object.LogInformation("Finished test {TestMethod}", nameof(GetAvailableCommandsAsync_WithUserRole_ReturnsOnlyNonAdminCommands));
    }

    /// <summary>
    /// Tests that moderators can access commands for moderators and above (excluding admin-only commands).
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetAvailableCommandsAsync_WithModeratorRole_ReturnsCommandsForModeratorAndAbove()
    {
        _mockLogger.Object.LogInformation("Starting test {TestMethod}", nameof(GetAvailableCommandsAsync_WithModeratorRole_ReturnsCommandsForModeratorAndAbove));
        // Arrange
        var adminCommand = new Command { Name = CommandServiceAdditionalTestsConstants.AdminCommandName, IsEnabled = true, RequiresAdmin = true };
        var moderatorCommand = new Command { Name = CommandServiceAdditionalTestsConstants.ModeratorCommandName, IsEnabled = true, RequiresAdmin = false };
        var userCommand = new Command { Name = CommandServiceAdditionalTestsConstants.HelpCommandName, IsEnabled = true, RequiresAdmin = false };

        _mockRepository
            .Setup(r => r.GetEnabledAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Command> { adminCommand, moderatorCommand, userCommand });

        // Act
        var result = await _service.GetAvailableCommandsAsync(UserRole.Moderator).ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(CommandServiceAdditionalTestsConstants.ModeratorAvailableCommandCount);
        result.Should().Contain(c => c.Name == CommandServiceAdditionalTestsConstants.ModeratorCommandName);
        result.Should().Contain(c => c.Name == CommandServiceAdditionalTestsConstants.HelpCommandName);
        result.Should().NotContain(c => c.Name == CommandServiceAdditionalTestsConstants.AdminCommandName);
        _mockLogger.Object.LogInformation("Finished test {TestMethod}", nameof(GetAvailableCommandsAsync_WithModeratorRole_ReturnsCommandsForModeratorAndAbove));
    }

    /// <summary>
    /// Tests that a valid command execution completes successfully and updates the command's execution count.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteCommandAsync_WithValidContext_ExecutesSuccessfully()
    {
        _mockLogger.Object.LogInformation("Starting test {TestMethod}", nameof(ExecuteCommandAsync_WithValidContext_ExecutesSuccessfully));
        // Arrange
        var command = new Command { Name = CommandServiceAdditionalTestsConstants.TestCommandName, IsEnabled = true, HandlerType = CommandServiceAdditionalTestsConstants.TestHandlerType };
        var user = new BotUser { UserId = CommandServiceAdditionalTestsConstants.TestUserId, FirstName = CommandServiceAdditionalTestsConstants.TestUserFirstName, Role = UserRole.User };
        var context = new ExecutionContext
        {
            UserId = CommandServiceAdditionalTestsConstants.TestUserId,
            ChatId = CommandServiceAdditionalTestsConstants.TestChatId,
            User = user,
            Command = command,
            Message = new Message { MessageId = CommandServiceAdditionalTestsConstants.TestMessageId, Content = CommandServiceAdditionalTestsConstants.TestCommandName, Type = MessageType.Text }
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
        result.Command!.ExecutionCount.Should().Be(CommandServiceAdditionalTestsConstants.FirstExecutionCount);
        _mockRepository.Verify(r => r.UpdateAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Object.LogInformation("Finished test {TestMethod}", nameof(ExecuteCommandAsync_WithValidContext_ExecutesSuccessfully));
    }

    /// <summary>
    /// Tests that attempting to execute a disabled command results in an error and no execution count update.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteCommandAsync_WithDisabledCommand_AddsErrorToContext()
    {
        _mockLogger.Object.LogInformation("Starting test {TestMethod}", nameof(ExecuteCommandAsync_WithDisabledCommand_AddsErrorToContext));
        // Arrange
        var command = new Command { Name = CommandServiceAdditionalTestsConstants.DisabledCommandName, IsEnabled = false, HandlerType = CommandServiceAdditionalTestsConstants.TestHandlerType };
        var user = new BotUser { UserId = CommandServiceAdditionalTestsConstants.TestUserId, FirstName = CommandServiceAdditionalTestsConstants.TestUserFirstName, Role = UserRole.Administrator };
        var context = new ExecutionContext
        {
            UserId = CommandServiceAdditionalTestsConstants.TestUserId,
            ChatId = CommandServiceAdditionalTestsConstants.TestChatId,
            User = user,
            Command = command,
            Message = new Message { MessageId = CommandServiceAdditionalTestsConstants.TestMessageId, Content = CommandServiceAdditionalTestsConstants.DisabledCommandName, Type = MessageType.Text }
        };

        // Act
        _mockLogger.Object.LogWarning("Executing disabled command {CommandName} for user {UserId}", command.Name, user.UserId);
        var result = await _service.ExecuteCommandAsync(context).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains(CommandServiceAdditionalTestsConstants.DisabledErrorText));
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Command>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Object.LogInformation("Finished test {TestMethod}", nameof(ExecuteCommandAsync_WithDisabledCommand_AddsErrorToContext));
    }

    /// <summary>
    /// Tests that users without sufficient permissions receive an error when attempting to execute admin-only commands.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ExecuteCommandAsync_WithInsufficientPermissions_AddsErrorToContext()
    {
        _mockLogger.Object.LogInformation("Starting test {TestMethod}", nameof(ExecuteCommandAsync_WithInsufficientPermissions_AddsErrorToContext));
        // Arrange
        var command = new Command { Name = CommandServiceAdditionalTestsConstants.AdminCommandName, IsEnabled = true, RequiresAdmin = true, HandlerType = CommandServiceAdditionalTestsConstants.TestHandlerType };
        var user = new BotUser { UserId = CommandServiceAdditionalTestsConstants.TestUserId, FirstName = CommandServiceAdditionalTestsConstants.TestUserFirstName, Role = UserRole.User };
        var context = new ExecutionContext
        {
            UserId = CommandServiceAdditionalTestsConstants.TestUserId,
            ChatId = CommandServiceAdditionalTestsConstants.TestChatId,
            User = user,
            Command = command,
            Message = new Message { MessageId = CommandServiceAdditionalTestsConstants.TestMessageId, Content = CommandServiceAdditionalTestsConstants.AdminCommandName, Type = MessageType.Text }
        };

        // Act
        _mockLogger.Object.LogWarning("Executing admin command {CommandName} with insufficient user role {UserRole}", command.Name, user.Role);
        var result = await _service.ExecuteCommandAsync(context).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains(CommandServiceAdditionalTestsConstants.InsufficientPermissionsErrorText));
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Command>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Object.LogInformation("Finished test {TestMethod}", nameof(ExecuteCommandAsync_WithInsufficientPermissions_AddsErrorToContext));
    }

    /// <summary>
    /// Tests that inactive users (e.g., banned) cannot execute any commands.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CanUserExecuteCommandAsync_WithInactiveUser_ReturnsFalse()
    {
        _mockLogger.Object.LogInformation("Starting test {TestMethod}", nameof(CanUserExecuteCommandAsync_WithInactiveUser_ReturnsFalse));
        // Arrange
        var user = new BotUser { UserId = CommandServiceAdditionalTestsConstants.TestUserId, FirstName = CommandServiceAdditionalTestsConstants.TestUserFirstName, Status = UserStatus.Banned };
        var command = new Command { Name = CommandServiceAdditionalTestsConstants.TestCommandName, IsEnabled = true };

        _mockUserService
            .Setup(u => u.GetUserByIdAsync(CommandServiceAdditionalTestsConstants.TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockRepository
            .Setup(r => r.GetByNameAsync(CommandServiceAdditionalTestsConstants.TestCommandName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(command);

        // Act
        _mockLogger.Object.LogWarning("Checking command {CommandName} for inactive user {UserId} with status {UserStatus}", command.Name, user.UserId, user.Status);
        var result = await _service.CanUserExecuteCommandAsync(CommandServiceAdditionalTestsConstants.TestUserId, CommandServiceAdditionalTestsConstants.TestCommandInput).ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
        _mockLogger.Object.LogInformation("Finished test {TestMethod}", nameof(CanUserExecuteCommandAsync_WithInactiveUser_ReturnsFalse));
    }

    /// <summary>
    /// Tests that attempting to check permissions for a non-existent command returns false.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CanUserExecuteCommandAsync_WithNonExistentCommand_ReturnsFalse()
    {
        _mockLogger.Object.LogInformation("Starting test {TestMethod}", nameof(CanUserExecuteCommandAsync_WithNonExistentCommand_ReturnsFalse));
        // Arrange
        var user = new BotUser { UserId = CommandServiceAdditionalTestsConstants.TestUserId, FirstName = CommandServiceAdditionalTestsConstants.TestUserFirstName, Status = UserStatus.Active };

        _mockUserService
            .Setup(u => u.GetUserByIdAsync(CommandServiceAdditionalTestsConstants.TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockRepository
            .Setup(r => r.GetByNameAsync(CommandServiceAdditionalTestsConstants.NonExistentCommandName, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Command?)null);

        // Act
        _mockLogger.Object.LogWarning("Checking unavailable command {CommandName} for user {UserId}", CommandServiceAdditionalTestsConstants.NonExistentCommandName, user.UserId);
        var result = await _service.CanUserExecuteCommandAsync(CommandServiceAdditionalTestsConstants.TestUserId, CommandServiceAdditionalTestsConstants.NonExistentCommandInput).ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
        _mockLogger.Object.LogInformation("Finished test {TestMethod}", nameof(CanUserExecuteCommandAsync_WithNonExistentCommand_ReturnsFalse));
    }

    /// <summary>
    /// Tests that disabled commands cannot be executed even by authorized users.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CanUserExecuteCommandAsync_WithDisabledCommand_ReturnsFalse()
    {
        _mockLogger.Object.LogInformation("Starting test {TestMethod}", nameof(CanUserExecuteCommandAsync_WithDisabledCommand_ReturnsFalse));
        // Arrange
        var user = new BotUser { UserId = CommandServiceAdditionalTestsConstants.TestUserId, FirstName = CommandServiceAdditionalTestsConstants.TestUserFirstName, Status = UserStatus.Active };
        var command = new Command { Name = CommandServiceAdditionalTestsConstants.DisabledCommandName, IsEnabled = false };

        _mockUserService
            .Setup(u => u.GetUserByIdAsync(CommandServiceAdditionalTestsConstants.TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockRepository
            .Setup(r => r.GetByNameAsync(CommandServiceAdditionalTestsConstants.DisabledCommandName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(command);

        // Act
        _mockLogger.Object.LogWarning("Checking disabled command {CommandName} for user {UserId}", command.Name, user.UserId);
        var result = await _service.CanUserExecuteCommandAsync(CommandServiceAdditionalTestsConstants.TestUserId, CommandServiceAdditionalTestsConstants.DisabledCommandInput).ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
        _mockLogger.Object.LogInformation("Finished test {TestMethod}", nameof(CanUserExecuteCommandAsync_WithDisabledCommand_ReturnsFalse));
    }

    /// <summary>
    /// Tests that executing a command successfully increments its execution count in the repository.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task RecordCommandExecutionAsync_WithValidCommand_IncrementsExecutionCount()
    {
        _mockLogger.Object.LogInformation("Starting test {TestMethod}", nameof(RecordCommandExecutionAsync_WithValidCommand_IncrementsExecutionCount));
        // Arrange
        var command = new Command { Name = CommandServiceAdditionalTestsConstants.TestCommandName, HandlerType = CommandServiceAdditionalTestsConstants.TestHandlerType, ExecutionCount = CommandServiceAdditionalTestsConstants.InitialExecutionCount };

        _mockRepository
            .Setup(r => r.GetByNameAsync(CommandServiceAdditionalTestsConstants.TestCommandName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(command);
        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(command);

        // Act
        await _service.RecordCommandExecutionAsync(CommandServiceAdditionalTestsConstants.TestCommandInput).ConfigureAwait(false);

        // Assert
        command.ExecutionCount.Should().Be(CommandServiceAdditionalTestsConstants.ExpectedExecutionCountAfterIncrement);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Command>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Object.LogInformation("Finished test {TestMethod}", nameof(RecordCommandExecutionAsync_WithValidCommand_IncrementsExecutionCount));
    }

    /// <summary>
    /// Tests that attempting to record execution for a non-existent command doesn't throw an exception.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task RecordCommandExecutionAsync_WithNonExistentCommand_DoesNotThrow()
    {
        _mockLogger.Object.LogInformation("Starting test {TestMethod}", nameof(RecordCommandExecutionAsync_WithNonExistentCommand_DoesNotThrow));
        // Arrange
        _mockRepository
            .Setup(r => r.GetByNameAsync(CommandServiceAdditionalTestsConstants.NonExistentCommandName, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Command?)null);

        // Act & Assert
        _mockLogger.Object.LogWarning("Recording execution for unavailable command {CommandName}", CommandServiceAdditionalTestsConstants.NonExistentCommandName);
        await _service.Invoking(s => s.RecordCommandExecutionAsync(CommandServiceAdditionalTestsConstants.NonExistentCommandInput))
            .Should().NotThrowAsync();
        _mockLogger.Object.LogInformation("Finished test {TestMethod}", nameof(RecordCommandExecutionAsync_WithNonExistentCommand_DoesNotThrow));
    }

    /// <summary>
    /// Tests that retrieving the execution count for an existing command returns the correct value.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetCommandExecutionCountAsync_WithExistingCommand_ReturnsCount()
    {
        _mockLogger.Object.LogInformation("Starting test {TestMethod}", nameof(GetCommandExecutionCountAsync_WithExistingCommand_ReturnsCount));
        // Arrange
        var command = new Command { Name = CommandServiceAdditionalTestsConstants.TestCommandName, ExecutionCount = CommandServiceAdditionalTestsConstants.ExecutionCountForGetTest };

        _mockRepository
            .Setup(r => r.GetByNameAsync(CommandServiceAdditionalTestsConstants.TestCommandName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(command);

        // Act
        var result = await _service.GetCommandExecutionCountAsync(CommandServiceAdditionalTestsConstants.TestCommandInput).ConfigureAwait(false);

        // Assert
        result.Should().Be(CommandServiceAdditionalTestsConstants.ExecutionCountForGetTest);
        _mockLogger.Object.LogInformation("Finished test {TestMethod}", nameof(GetCommandExecutionCountAsync_WithExistingCommand_ReturnsCount));
    }

    /// <summary>
    /// Tests that retrieving the execution count for a non-existent command returns zero.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetCommandExecutionCountAsync_WithNonExistentCommand_ReturnsZero()
    {
        _mockLogger.Object.LogInformation("Starting test {TestMethod}", nameof(GetCommandExecutionCountAsync_WithNonExistentCommand_ReturnsZero));
        // Arrange
        _mockRepository
            .Setup(r => r.GetByNameAsync(CommandServiceAdditionalTestsConstants.NonExistentCommandName, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Command?)null);

        // Act
        _mockLogger.Object.LogWarning("Retrieving execution count for unavailable command {CommandName}", CommandServiceAdditionalTestsConstants.NonExistentCommandName);
        var result = await _service.GetCommandExecutionCountAsync(CommandServiceAdditionalTestsConstants.NonExistentCommandInput).ConfigureAwait(false);

        // Assert
        result.Should().Be(CommandServiceAdditionalTestsConstants.MissingCommandExecutionCount);
        _mockLogger.Object.LogInformation("Finished test {TestMethod}", nameof(GetCommandExecutionCountAsync_WithNonExistentCommand_ReturnsZero));
    }

    /// <summary>
    /// Tests that commands without rate limiting configured are never considered rate limited.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task IsCommandRateLimitedAsync_WithNoRateLimitConfigured_ReturnsFalse()
    {
        _mockLogger.Object.LogInformation("Starting test {TestMethod}", nameof(IsCommandRateLimitedAsync_WithNoRateLimitConfigured_ReturnsFalse));
        // Arrange
        var command = new Command { Name = CommandServiceAdditionalTestsConstants.TestCommandName, RateLimitPerMinute = null };

        _mockRepository
            .Setup(r => r.GetByNameAsync(CommandServiceAdditionalTestsConstants.TestCommandName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(command);

        // Act
        _mockLogger.Object.LogWarning("Checking rate limit for command {CommandName} without a configured limit for user {UserId}", command.Name, CommandServiceAdditionalTestsConstants.TestUserId);
        var result = await _service.IsCommandRateLimitedAsync(CommandServiceAdditionalTestsConstants.TestUserId, CommandServiceAdditionalTestsConstants.TestCommandInput).ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
        _mockLogger.Object.LogInformation("Finished test {TestMethod}", nameof(IsCommandRateLimitedAsync_WithNoRateLimitConfigured_ReturnsFalse));
    }

    /// <summary>
    /// Tests that rate limiting is applied per user, allowing different users to execute commands
    /// even when the same command has a rate limit configured.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task IsCommandRateLimitedAsync_WithMultipleUsers_ResetsRateLimitPerUser()
    {
        _mockLogger.Object.LogInformation("Starting test {TestMethod}", nameof(IsCommandRateLimitedAsync_WithMultipleUsers_ResetsRateLimitPerUser));
        // Arrange
        var command = new Command { Name = CommandServiceAdditionalTestsConstants.TestCommandName, RateLimitPerMinute = CommandServiceAdditionalTestsConstants.RateLimitPerMinute };

        _mockRepository
            .Setup(r => r.GetByNameAsync(CommandServiceAdditionalTestsConstants.TestCommandName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(command);

        // First user - should not be rate limited
        var result1 = await _service.IsCommandRateLimitedAsync(CommandServiceAdditionalTestsConstants.FirstRateLimitUserId, CommandServiceAdditionalTestsConstants.TestCommandInput).ConfigureAwait(false);
        result1.Should().BeFalse();

        // Second user - should not be rate limited (different user)
        var result2 = await _service.IsCommandRateLimitedAsync(CommandServiceAdditionalTestsConstants.SecondRateLimitUserId, CommandServiceAdditionalTestsConstants.TestCommandInput).ConfigureAwait(false);
        result2.Should().BeFalse();

        // First user again - should be rate limited now
        var result3 = await _service.IsCommandRateLimitedAsync(CommandServiceAdditionalTestsConstants.FirstRateLimitUserId, CommandServiceAdditionalTestsConstants.TestCommandInput).ConfigureAwait(false);
        result3.Should().BeTrue();
        _mockLogger.Object.LogInformation("Finished test {TestMethod}", nameof(IsCommandRateLimitedAsync_WithMultipleUsers_ResetsRateLimitPerUser));
    }
}
