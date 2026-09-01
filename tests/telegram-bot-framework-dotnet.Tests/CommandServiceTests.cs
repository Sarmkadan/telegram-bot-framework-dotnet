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

namespace TelegramBotFramework.Tests;

/// <summary>
/// Tests for the CommandService class.
/// </summary>
public sealed class CommandServiceTests : ICommandServiceTests
{
    private readonly Mock<ICommandRepository> _mockRepository = new();
    private readonly Mock<IUserService> _mockUserService = new();
    private readonly Mock<ICommandUsageTracker> _mockCommandUsageTracker = new();
    private readonly Mock<ILogger<CommandService>> _mockLogger = new();
    private readonly CommandService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandServiceTests"/> class.
    /// </summary>
    public CommandServiceTests()
    {
        _service = new CommandService(_mockRepository.Object, _mockUserService.Object, _mockCommandUsageTracker.Object, _mockLogger.Object);
    }

    /// <summary>
    /// Tests that GetCommandAsync returns a command when it exists.
    /// </summary>
    [Fact]
    public async Task GetCommandAsync_WhenExists_ReturnsCommand()
    {
        _mockLogger.Object.LogInformation("GetCommandAsync_WhenExists_ReturnsCommand called with {CommandName}", CommandServiceTestsConstants.TestCommandNameWithoutSlash);
        var command = new Models.Command { Name = CommandServiceTestsConstants.TestCommandNameWithSlash };
        _mockRepository
            .Setup(r => r.GetByNameAsync(CommandServiceTestsConstants.TestCommandNameWithSlash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(command);

        var result = await _service.GetCommandAsync(CommandServiceTestsConstants.TestCommandNameWithoutSlash).ConfigureAwait(false);

        result.Should().NotBeNull();
        result!.Name.Should().Be(CommandServiceTestsConstants.TestCommandNameWithSlash);
        _mockLogger.Object.LogInformation("GetCommandAsync_WhenExists_ReturnsCommand completed with {CommandName}", result.Name);
    }

    /// <summary>
    /// Tests that GetCommandAsync returns null when the command does not exist.
    /// </summary>
    [Fact]
    public async Task GetCommandAsync_WhenDoesNotExist_ReturnsNull()
    {
        _mockLogger.Object.LogInformation("GetCommandAsync_WhenDoesNotExist_ReturnsNull called with {CommandName}", CommandServiceTestsConstants.UnknownCommandNameWithoutSlash);
        _mockRepository
            .Setup(r => r.GetByNameAsync(CommandServiceTestsConstants.UnknownCommandNameWithSlash, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Models.Command?)null);

        var result = await _service.GetCommandAsync(CommandServiceTestsConstants.UnknownCommandNameWithoutSlash).ConfigureAwait(false);

        _mockLogger.Object.LogWarning("Command lookup returned no result for {CommandName}", CommandServiceTestsConstants.UnknownCommandNameWithoutSlash);
        result.Should().BeNull();
        _mockLogger.Object.LogInformation("GetCommandAsync_WhenDoesNotExist_ReturnsNull completed with {CommandName}", CommandServiceTestsConstants.UnknownCommandNameWithoutSlash);
    }

    /// <summary>
    /// Tests that ExecuteCommandAsync adds an error to the context when the command is disabled.
    /// </summary>
    [Fact]
    public async Task ExecuteCommandAsync_WhenCommandIsDisabled_AddsErrorToContext()
    {
        var command = new Models.Command { Name = CommandServiceTestsConstants.DisabledCommandName, IsEnabled = false };
        var context = new Models.ExecutionContext { Command = command, UserId = CommandServiceTestsConstants.DefaultUserId, ChatId = CommandServiceTestsConstants.DefaultChatId };
        _mockLogger.Object.LogInformation("ExecuteCommandAsync_WhenCommandIsDisabled_AddsErrorToContext called with {CommandName} for user {UserId} in chat {ChatId}", command.Name, context.UserId, context.ChatId);

        var result = await _service.ExecuteCommandAsync(context).ConfigureAwait(false);

        _mockLogger.Object.LogWarning("Command {CommandName} is disabled for user {UserId}", command.Name, context.UserId);
        result.Errors.Should().Contain(e => e.Contains("is disabled"));
        _mockLogger.Object.LogInformation("ExecuteCommandAsync_WhenCommandIsDisabled_AddsErrorToContext completed with {ErrorCount} errors", result.Errors.Count);
    }

    /// <summary>
    /// Tests that ExecuteCommandAsync adds an error to the context when the user lacks sufficient permissions.
    /// </summary>
    [Fact]
    public async Task ExecuteCommandAsync_WithInsufficientPermissions_AddsErrorToContext()
    {
        var command = new Models.Command { Name = CommandServiceTestsConstants.AdminCommandName, RequiresAdmin = true };
        var user = new Models.BotUser { Role = Models.UserRole.User };
        var context = new Models.ExecutionContext { Command = command, User = user, UserId = CommandServiceTestsConstants.DefaultUserId, ChatId = CommandServiceTestsConstants.DefaultChatId };
        _mockLogger.Object.LogInformation("ExecuteCommandAsync_WithInsufficientPermissions_AddsErrorToContext called with {CommandName}, user {UserId}, and role {UserRole}", command.Name, context.UserId, user.Role);

        var result = await _service.ExecuteCommandAsync(context).ConfigureAwait(false);

        _mockLogger.Object.LogWarning("User {UserId} with role {UserRole} has insufficient permissions for {CommandName}", context.UserId, user.Role, command.Name);
        result.Errors.Should().Contain(e => e.Contains("Insufficient permissions"));
        _mockLogger.Object.LogInformation("ExecuteCommandAsync_WithInsufficientPermissions_AddsErrorToContext completed with {ErrorCount} errors", result.Errors.Count);
    }

    /// <summary>
    /// Tests that IsCommandRateLimitedAsync returns true when the command is rate limited.
    /// </summary>
    [Fact]
    public async Task IsCommandRateLimitedAsync_WhenExceedsLimit_ReturnsTrue()
    {
        const long userId = CommandServiceTestsConstants.DefaultUserId;
        const string commandName = CommandServiceTestsConstants.TestCommandNameWithSlash;
        _mockLogger.Object.LogInformation("IsCommandRateLimitedAsync_WhenExceedsLimit_ReturnsTrue called with user {UserId} and command {CommandName}", userId, commandName);
        var command = new Models.Command { Name = commandName, RateLimitPerMinute = 1 };
        _mockRepository
            .Setup(r => r.GetByNameAsync(commandName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(command);

        // First request - allowed
        await _service.IsCommandRateLimitedAsync(userId, commandName).ConfigureAwait(false);
        // Second request - rate limited
        var isLimited = await _service.IsCommandRateLimitedAsync(userId, commandName).ConfigureAwait(false);

        _mockLogger.Object.LogWarning("Command {CommandName} is rate limited for user {UserId}", commandName, userId);
        isLimited.Should().BeTrue();
        _mockLogger.Object.LogInformation("IsCommandRateLimitedAsync_WhenExceedsLimit_ReturnsTrue completed with rate limit status {IsLimited}", isLimited);
    }

    /// <summary>
    /// Tests that IsCommandRateLimitedAsync returns false when the command is not rate limited.
    /// </summary>
    [Fact]
    public async Task IsCommandRateLimitedAsync_WhenWithinLimit_ReturnsFalse()
    {
        const long userId = CommandServiceTestsConstants.AlternativeUserId;
        const string commandName = CommandServiceTestsConstants.TestCommandNameWithSlash;
        _mockLogger.Object.LogInformation("IsCommandRateLimitedAsync_WhenWithinLimit_ReturnsFalse called with user {UserId} and command {CommandName}", userId, commandName);
        var command = new Models.Command { Name = commandName, RateLimitPerMinute = 5 };
        _mockRepository
            .Setup(r => r.GetByNameAsync(commandName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(command);

        var isLimited = await _service.IsCommandRateLimitedAsync(userId, commandName).ConfigureAwait(false);

        isLimited.Should().BeFalse();
        _mockLogger.Object.LogInformation("IsCommandRateLimitedAsync_WhenWithinLimit_ReturnsFalse completed with rate limit status {IsLimited}", isLimited);
    }

    /// <summary>
    /// Tests that RegisterCommandAsync throws an exception when the command is invalid.
    /// </summary>
    [Fact]
    public async Task RegisterCommandAsync_WithInvalidCommand_ThrowsException()
    {
        var command = new Models.Command { Name = CommandServiceTestsConstants.InvalidCommandName }; // Missing leading slash, validation will fail
        _mockLogger.Object.LogInformation("RegisterCommandAsync_WithInvalidCommand_ThrowsException called with {CommandName}", command.Name);

        Func<Task> act = async () => await _service.RegisterCommandAsync(command).ConfigureAwait(false);

        await act.Should().ThrowAsync<Exception>();
        _mockLogger.Object.LogWarning("Registration rejected invalid command {CommandName}", command.Name);
        _mockLogger.Object.LogInformation("RegisterCommandAsync_WithInvalidCommand_ThrowsException completed with {CommandName}", command.Name);
    }
}
