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
public sealed class CommandServiceTests
{
    private readonly Mock<ICommandRepository> _mockRepository = new();
    private readonly Mock<IUserService> _mockUserService = new();
    private readonly Mock<ILogger<CommandService>> _mockLogger = new();
    private readonly CommandService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandServiceTests"/> class.
    /// </summary>
    public CommandServiceTests()
    {
        _service = new CommandService(_mockRepository.Object, _mockUserService.Object, _mockLogger.Object);
    }

    /// <summary>
    /// Tests that GetCommandAsync returns a command when it exists.
    /// </summary>
    [Fact]
    public async Task GetCommandAsync_WhenExists_ReturnsCommand()
    {
        var command = new Models.Command { Name = "/test" };
        _mockRepository
            .Setup(r => r.GetByNameAsync("/test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(command);

        var result = await _service.GetCommandAsync("test").ConfigureAwait(false);

        result.Should().NotBeNull();
        result!.Name.Should().Be("/test");
    }

    /// <summary>
    /// Tests that GetCommandAsync returns null when the command does not exist.
    /// </summary>
    [Fact]
    public async Task GetCommandAsync_WhenDoesNotExist_ReturnsNull()
    {
        _mockRepository
            .Setup(r => r.GetByNameAsync("/unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Models.Command?)null);

        var result = await _service.GetCommandAsync("unknown").ConfigureAwait(false);

        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that ExecuteCommandAsync adds an error to the context when the command is disabled.
    /// </summary>
    [Fact]
    public async Task ExecuteCommandAsync_WhenCommandIsDisabled_AddsErrorToContext()
    {
        var command = new Models.Command { Name = "/disabled", IsEnabled = false };
        var context = new Models.ExecutionContext { Command = command, UserId = 1, ChatId = 1 };

        var result = await _service.ExecuteCommandAsync(context).ConfigureAwait(false);

        result.Errors.Should().Contain(e => e.Contains("is disabled"));
    }

    /// <summary>
    /// Tests that ExecuteCommandAsync adds an error to the context when the user lacks sufficient permissions.
    /// </summary>
    [Fact]
    public async Task ExecuteCommandAsync_WithInsufficientPermissions_AddsErrorToContext()
    {
        var command = new Models.Command { Name = "/admin", RequiresAdmin = true };
        var user = new Models.BotUser { Role = Models.UserRole.User };
        var context = new Models.ExecutionContext { Command = command, User = user, UserId = 1, ChatId = 1 };

        var result = await _service.ExecuteCommandAsync(context).ConfigureAwait(false);

        result.Errors.Should().Contain(e => e.Contains("Insufficient permissions"));
    }

    /// <summary>
    /// Tests that IsCommandRateLimitedAsync returns true when the command is rate limited.
    /// </summary>
    [Fact]
    public async Task IsCommandRateLimitedAsync_WhenExceedsLimit_ReturnsTrue()
    {
        const long userId = 1L;
        const string commandName = "/test";
        var command = new Models.Command { Name = commandName, RateLimitPerMinute = 1 };
        _mockRepository
            .Setup(r => r.GetByNameAsync(commandName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(command);

        // First request - allowed
        await _service.IsCommandRateLimitedAsync(userId, commandName).ConfigureAwait(false);
        // Second request - rate limited
        var isLimited = await _service.IsCommandRateLimitedAsync(userId, commandName).ConfigureAwait(false);

        isLimited.Should().BeTrue();
    }

    /// <summary>
    /// Tests that IsCommandRateLimitedAsync returns false when the command is not rate limited.
    /// </summary>
    [Fact]
    public async Task IsCommandRateLimitedAsync_WhenWithinLimit_ReturnsFalse()
    {
        const long userId = 2L;
        const string commandName = "/test";
        var command = new Models.Command { Name = commandName, RateLimitPerMinute = 5 };
        _mockRepository
            .Setup(r => r.GetByNameAsync(commandName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(command);

        var isLimited = await _service.IsCommandRateLimitedAsync(userId, commandName).ConfigureAwait(false);

        isLimited.Should().BeFalse();
    }

    /// <summary>
    /// Tests that RegisterCommandAsync throws an exception when the command is invalid.
    /// </summary>
    [Fact]
    public async Task RegisterCommandAsync_WithInvalidCommand_ThrowsException()
    {
        var command = new Models.Command { Name = "invalid" }; // Missing leading slash, validation will fail

        Func<Task> act = async () => await _service.RegisterCommandAsync(command).ConfigureAwait(false);

        await act.Should().ThrowAsync<Exception>();
    }
}
