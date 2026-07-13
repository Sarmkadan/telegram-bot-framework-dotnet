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

public sealed class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository = new();
    private readonly Mock<ILogger<UserService>> _mockLogger = new();
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userService = new UserService(_mockUserRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetOrCreateUserAsync_WithExistingUser_ReturnsExistingUser()
    {
        // Arrange
        var existingUser = new BotUser { UserId = 123, FirstName = "John", LastName = "Doe", Username = "johndoe" };

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _userService.GetOrCreateUserAsync(123, "John", "Doe", "johndoe").ConfigureAwait(false);

        // Assert
        result.Should().Be(existingUser);
        _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<BotUser>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetOrCreateUserAsync_WithNonExistingUser_CreatesAndReturnsNewUser()
    {
        // Arrange
        _mockUserRepository
            .Setup(r => r.GetByIdAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BotUser?)null);
        _mockUserRepository
            .Setup(r => r.CreateAsync(It.IsAny<BotUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BotUser user, CancellationToken _) => user);

        // Act
        var result = await _userService.GetOrCreateUserAsync(123, "John", "Doe", "johndoe").ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.TelegramId.Should().Be(123);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Username.Should().Be("johndoe");
        result.Status.Should().Be(UserStatus.Active);
        result.MessagesCount.Should().Be(0);
        result.LastActivityAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<BotUser>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOrCreateUserAsync_WithNullLastName_CreatesUserWithoutLastName()
    {
        // Arrange
        _mockUserRepository
            .Setup(r => r.GetByIdAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BotUser?)null);
        _mockUserRepository
            .Setup(r => r.CreateAsync(It.IsAny<BotUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BotUser user, CancellationToken _) => user);

        // Act
        var result = await _userService.GetOrCreateUserAsync(123, "John", null, null).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("John");
        result.LastName.Should().BeNull();
        result.Username.Should().BeNull();
    }

    [Fact]
    public async Task GetOrCreateUserAsync_WithExistingUserWithDifferentDetails_UpdatesUser()
    {
        // Arrange
        var existingUser = new BotUser { UserId = 123, FirstName = "OldName", LastName = "OldLast", Username = "olduser" };
        var updatedUser = new BotUser { UserId = 123, FirstName = "John", LastName = "Doe", Username = "johndoe" };

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);
        _mockUserRepository
            .Setup(r => r.UpdateAsync(It.IsAny<BotUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _userService.GetOrCreateUserAsync(123, "John", "Doe", "johndoe").ConfigureAwait(false);

        // Assert
        result.Should().Be(existingUser);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Username.Should().Be("johndoe");
        _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<BotUser>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithExistingUser_ReturnsUser()
    {
        // Arrange
        var user = new BotUser { UserId = 123, FirstName = "John", LastName = "Doe" };

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.GetUserByIdAsync(123).ConfigureAwait(false);

        // Assert
        result.Should().Be(user);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithNonExistingUser_ReturnsNull()
    {
        // Arrange
        _mockUserRepository
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BotUser?)null);

        // Act
        var result = await _userService.GetUserByIdAsync(999).ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RecordUserActivityAsync_UpdatesLastActivityAndIncrementsMessagesCount()
    {
        // Arrange
        var user = new BotUser { UserId = 123, FirstName = "John", MessagesCount = 5 };
        var updatedUser = new BotUser { UserId = 123, FirstName = "John", MessagesCount = 6 };

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockUserRepository
            .Setup(r => r.UpdateAsync(It.IsAny<BotUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _userService.RecordUserActivityAsync(123).ConfigureAwait(false);

        // Assert
        _mockUserRepository.Verify(r => r.UpdateAsync(It.Is<BotUser>(u =>
            u.MessagesCount == 6 &&
            u.LastActivityAt.HasValue &&
            u.LastActivityAt.Value.Date == DateTime.UtcNow.Date
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordUserActivityAsync_WithNonExistingUser_DoesNotThrow()
    {
        // Arrange
        _mockUserRepository
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BotUser?)null);

        // Act & Assert
        await _userService.Invoking(s => s.RecordUserActivityAsync(999))
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task UpdateUserAsync_UpdatesUserProperties()
    {
        // Arrange
        var user = new BotUser { UserId = 123, FirstName = "John", LastName = "Doe", Username = "johndoe" };
        var updatedUser = new BotUser { UserId = 123, FirstName = "John", LastName = "Smith", Username = "johnsmith" };

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockUserRepository
            .Setup(r => r.UpdateAsync(It.IsAny<BotUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _userService.UpdateUserAsync(123, "John", "Smith", "johnsmith").ConfigureAwait(false);

        // Assert
        result.Should().Be(updatedUser);
        result.LastName.Should().Be("Smith");
        result.Username.Should().Be("johnsmith");
        _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<BotUser>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_WithPartialUpdates_PreservesUnchangedValues()
    {
        // Arrange
        var user = new BotUser { UserId = 123, FirstName = "John", LastName = "Doe", Username = "johndoe" };

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockUserRepository
            .Setup(r => r.UpdateAsync(It.IsAny<BotUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.UpdateUserAsync(123, "John", null, null).ConfigureAwait(false);

        // Assert
        result.Should().Be(user);
        result.LastName.Should().Be("Doe");
        result.Username.Should().Be("johndoe");
    }

    [Fact]
    public async Task DeleteUserAsync_WithExistingUser_DeletesAndReturnsTrue()
    {
        // Arrange
        _mockUserRepository
            .Setup(r => r.DeleteAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.DeleteUserAsync(123).ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();
        _mockUserRepository.Verify(r => r.DeleteAsync(123, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_WithNonExistingUser_ReturnsFalse()
    {
        // Arrange
        _mockUserRepository
            .Setup(r => r.DeleteAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _userService.DeleteUserAsync(999).ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SearchUsersAsync_FiltersByFirstName()
    {
        // Arrange
        var users = new List<BotUser>
        {
            new BotUser { UserId = 1, FirstName = "John", LastName = "Doe" },
            new BotUser { UserId = 2, FirstName = "Jane", LastName = "Smith" },
            new BotUser { UserId = 3, FirstName = "Johnny", LastName = "Appleseed" }
        };

        _mockUserRepository
            .Setup(r => r.SearchAsync("John", It.IsAny<CancellationToken>()))
            .ReturnsAsync(users.Where(u => u.FirstName.Contains("John", StringComparison.OrdinalIgnoreCase)).ToList());

        // Act
        var result = await _userService.SearchUsersAsync("John").ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(u => u.FirstName.Should().ContainEquivalentOf("John"));
    }

    [Fact]
    public async Task SearchUsersAsync_WithEmptyQuery_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<BotUser>
        {
            new BotUser { UserId = 1, FirstName = "John" },
            new BotUser { UserId = 2, FirstName = "Jane" }
        };

        _mockUserRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await _userService.SearchUsersAsync("").ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUsersByStatusAsync_ReturnsFilteredUsers()
    {
        // Arrange
        var activeUsers = new List<BotUser>
        {
            new BotUser { UserId = 1, Status = UserStatus.Active },
            new BotUser { UserId = 2, Status = UserStatus.Active }
        };
        var bannedUser = new BotUser { UserId = 3, Status = UserStatus.Banned };

        _mockUserRepository
            .Setup(r => r.GetByStatusAsync(UserStatus.Active, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeUsers);

        // Act
        var result = await _userService.GetUsersByStatusAsync(UserStatus.Active).ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(u => u.Status.Should().Be(UserStatus.Active));
    }
}
