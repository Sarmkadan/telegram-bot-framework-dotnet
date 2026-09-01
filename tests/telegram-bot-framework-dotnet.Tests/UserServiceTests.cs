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
/// Unit tests for the <see cref="UserService"/> class.
/// Tests the user management functionality including creation, retrieval, updating, and deletion.
/// </summary>
public sealed class UserServiceTests : IUserServiceTests
{
    /// <summary>
    /// Mock repository for testing user persistence operations.
    /// </summary>
    private readonly Mock<IUserRepository> _mockUserRepository = new();

    /// <summary>
    /// Mock logger for testing logging behavior.
    /// </summary>
    private readonly Mock<ILogger<UserService>> _mockLogger = new();

    /// <summary>
    /// Instance of UserService under test.
    /// </summary>
    private readonly UserService _userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserServiceTests"/> class.
    /// Sets up the test dependencies including mock repositories and logger.
    /// </summary>
    public UserServiceTests()
    {
        _userService = new UserService(_mockUserRepository.Object, _mockLogger.Object);
    }

    /// <summary>
    /// Tests that GetOrCreateUserAsync returns existing user when user already exists in repository.
    /// </summary>
    /// <returns>Returns the existing user without creating a new one.</returns>
    [Fact]
    public async Task GetOrCreateUserAsync_WithExistingUser_ReturnsExistingUser()
    {

        _mockLogger.Object.LogInformation("GetOrCreateUserAsync_WithExistingUser_ReturnsExistingUser called with {UserId}", UserServiceTestsConstants.ExistingUserId);

        // Arrange
        var existingUser = new BotUser { UserId = UserServiceTestsConstants.ExistingUserId, FirstName = UserServiceTestsConstants.FirstNameJohn, LastName = UserServiceTestsConstants.LastNameDoe, Username = UserServiceTestsConstants.UsernameJohnDoe };

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(UserServiceTestsConstants.ExistingUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _userService.GetOrCreateUserAsync(UserServiceTestsConstants.ExistingUserId, UserServiceTestsConstants.FirstNameJohn, UserServiceTestsConstants.LastNameDoe, UserServiceTestsConstants.UsernameJohnDoe).ConfigureAwait(false);

        // Assert
        result.Should().Be(existingUser);
        _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<BotUser>(), It.IsAny<CancellationToken>()), Times.Never);

        _mockLogger.Object.LogInformation("GetOrCreateUserAsync_WithExistingUser_ReturnsExistingUser completed with {UserId}", UserServiceTestsConstants.ExistingUserId);
    }

    /// <summary>
    /// Tests that GetOrCreateUserAsync creates and returns a new user when user doesn't exist.
    /// </summary>
    /// <returns>Returns a newly created user with default properties.</returns>
    [Fact]
    public async Task GetOrCreateUserAsync_WithNonExistingUser_CreatesAndReturnsNewUser()
    {

        _mockLogger.Object.LogInformation("GetOrCreateUserAsync_WithNonExistingUser_CreatesAndReturnsNewUser called with {UserId}", UserServiceTestsConstants.ExistingUserId);

        // Arrange
        _mockLogger.Object.LogWarning("User {UserId} was not found; exercising user creation fallback", UserServiceTestsConstants.ExistingUserId);

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(UserServiceTestsConstants.ExistingUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BotUser?)null);
        _mockUserRepository
            .Setup(r => r.CreateAsync(It.IsAny<BotUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BotUser user, CancellationToken _) => user);

        // Act
        var result = await _userService.GetOrCreateUserAsync(UserServiceTestsConstants.ExistingUserId, UserServiceTestsConstants.FirstNameJohn, UserServiceTestsConstants.LastNameDoe, UserServiceTestsConstants.UsernameJohnDoe).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.TelegramId.Should().Be(UserServiceTestsConstants.ExistingUserId);
        result.FirstName.Should().Be(UserServiceTestsConstants.FirstNameJohn);
        result.LastName.Should().Be(UserServiceTestsConstants.LastNameDoe);
        result.Username.Should().Be(UserServiceTestsConstants.UsernameJohnDoe);
        result.Status.Should().Be(UserStatus.Active);
        result.MessagesCount.Should().Be(0);
        result.LastActivityAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<BotUser>(), It.IsAny<CancellationToken>()), Times.Once);

        _mockLogger.Object.LogInformation("GetOrCreateUserAsync_WithNonExistingUser_CreatesAndReturnsNewUser completed with {UserId}", UserServiceTestsConstants.ExistingUserId);
    }

    /// <summary>
    /// Tests that GetOrCreateUserAsync handles null last name and username parameters correctly.
    /// </summary>
    /// <returns>Returns a user with null last name and username when null values are provided.</returns>
    [Fact]
    public async Task GetOrCreateUserAsync_WithNullLastName_CreatesUserWithoutLastName()
    {

        _mockLogger.Object.LogInformation("GetOrCreateUserAsync_WithNullLastName_CreatesUserWithoutLastName called with {UserId} and {FirstName}", UserServiceTestsConstants.ExistingUserId, UserServiceTestsConstants.FirstNameJohn);

        // Arrange
        _mockLogger.Object.LogWarning("Creating user {UserId} with missing optional profile details", UserServiceTestsConstants.ExistingUserId);

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(UserServiceTestsConstants.ExistingUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BotUser?)null);
        _mockUserRepository
            .Setup(r => r.CreateAsync(It.IsAny<BotUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BotUser user, CancellationToken _) => user);

        // Act
        var result = await _userService.GetOrCreateUserAsync(UserServiceTestsConstants.ExistingUserId, UserServiceTestsConstants.FirstNameJohn, null, null).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be(UserServiceTestsConstants.FirstNameJohn);
        result.LastName.Should().BeNull();
        result.Username.Should().BeNull();

        _mockLogger.Object.LogInformation("GetOrCreateUserAsync_WithNullLastName_CreatesUserWithoutLastName completed with {UserId}", UserServiceTestsConstants.ExistingUserId);
    }

    /// <summary>
    /// Tests that GetOrCreateUserAsync updates existing user details when user already exists.
    /// </summary>
    /// <returns>Returns the updated user with new details.</returns>
    [Fact]
    public async Task GetOrCreateUserAsync_WithExistingUserWithDifferentDetails_UpdatesUser()
    {

        _mockLogger.Object.LogInformation("GetOrCreateUserAsync_WithExistingUserWithDifferentDetails_UpdatesUser called with {UserId}", UserServiceTestsConstants.ExistingUserId);

        // Arrange
        var existingUser = new BotUser { UserId = UserServiceTestsConstants.ExistingUserId, FirstName = UserServiceTestsConstants.FirstNameOld, LastName = UserServiceTestsConstants.LastNameOld, Username = UserServiceTestsConstants.UsernameOldUser };
        var updatedUser = new BotUser { UserId = UserServiceTestsConstants.ExistingUserId, FirstName = UserServiceTestsConstants.FirstNameJohn, LastName = UserServiceTestsConstants.LastNameDoe, Username = UserServiceTestsConstants.UsernameJohnDoe };

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(UserServiceTestsConstants.ExistingUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);
        _mockUserRepository
            .Setup(r => r.UpdateAsync(It.IsAny<BotUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _userService.GetOrCreateUserAsync(UserServiceTestsConstants.ExistingUserId, UserServiceTestsConstants.FirstNameJohn, UserServiceTestsConstants.LastNameDoe, UserServiceTestsConstants.UsernameJohnDoe).ConfigureAwait(false);

        // Assert
        result.Should().Be(existingUser);
        result.FirstName.Should().Be(UserServiceTestsConstants.FirstNameJohn);
        result.LastName.Should().Be(UserServiceTestsConstants.LastNameDoe);
        result.Username.Should().Be(UserServiceTestsConstants.UsernameJohnDoe);
        _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<BotUser>(), It.IsAny<CancellationToken>()), Times.Once);

        _mockLogger.Object.LogInformation("GetOrCreateUserAsync_WithExistingUserWithDifferentDetails_UpdatesUser completed with {UserId}", UserServiceTestsConstants.ExistingUserId);
    }

    /// <summary>
    /// Tests that GetUserByIdAsync returns the user when user exists in repository.
    /// </summary>
    /// <returns>Returns the existing user.</returns>
    [Fact]
    public async Task GetUserByIdAsync_WithExistingUser_ReturnsUser()
    {

        _mockLogger.Object.LogInformation("GetUserByIdAsync_WithExistingUser_ReturnsUser called with {UserId}", UserServiceTestsConstants.ExistingUserId);

        // Arrange
        var user = new BotUser { UserId = UserServiceTestsConstants.ExistingUserId, FirstName = UserServiceTestsConstants.FirstNameJohn, LastName = UserServiceTestsConstants.LastNameDoe };

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(UserServiceTestsConstants.ExistingUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.GetUserByIdAsync(UserServiceTestsConstants.ExistingUserId).ConfigureAwait(false);

        // Assert
        result.Should().Be(user);

        _mockLogger.Object.LogInformation("GetUserByIdAsync_WithExistingUser_ReturnsUser completed with {UserId}", UserServiceTestsConstants.ExistingUserId);
    }

    /// <summary>
    /// Tests that GetUserByIdAsync returns null when user doesn't exist in repository.
    /// </summary>
    /// <returns>Returns null for non-existing user.</returns>
    [Fact]
    public async Task GetUserByIdAsync_WithNonExistingUser_ReturnsNull()
    {

        _mockLogger.Object.LogInformation("GetUserByIdAsync_WithNonExistingUser_ReturnsNull called with {UserId}", UserServiceTestsConstants.NonExistingUserId);

        // Arrange
        _mockLogger.Object.LogWarning("User lookup fallback returned no user for {UserId}", UserServiceTestsConstants.NonExistingUserId);

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(UserServiceTestsConstants.NonExistingUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BotUser?)null);

        // Act
        var result = await _userService.GetUserByIdAsync(UserServiceTestsConstants.NonExistingUserId).ConfigureAwait(false);

        // Assert
        result.Should().BeNull();

        _mockLogger.Object.LogInformation("GetUserByIdAsync_WithNonExistingUser_ReturnsNull completed with {UserId}", UserServiceTestsConstants.NonExistingUserId);
    }

    /// <summary>
    /// Tests that RecordUserActivityAsync updates user's last activity timestamp and increments message count.
    /// </summary>
    [Fact]
    public async Task RecordUserActivityAsync_UpdatesLastActivityAndIncrementsMessagesCount()
    {

        _mockLogger.Object.LogInformation("RecordUserActivityAsync_UpdatesLastActivityAndIncrementsMessagesCount called with {UserId}", UserServiceTestsConstants.ExistingUserId);

        // Arrange
        var user = new BotUser { UserId = UserServiceTestsConstants.ExistingUserId, FirstName = UserServiceTestsConstants.FirstNameJohn, MessagesCount = 5 };
        var updatedUser = new BotUser { UserId = UserServiceTestsConstants.ExistingUserId, FirstName = UserServiceTestsConstants.FirstNameJohn, MessagesCount = 6 };

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(UserServiceTestsConstants.ExistingUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockUserRepository
            .Setup(r => r.UpdateAsync(It.IsAny<BotUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _userService.RecordUserActivityAsync(UserServiceTestsConstants.ExistingUserId).ConfigureAwait(false);

        // Assert
        _mockUserRepository.Verify(r => r.UpdateAsync(It.Is<BotUser>(u =>
            u.MessagesCount == 6 &&
            u.LastActivityAt.HasValue &&
            u.LastActivityAt.Value.Date == DateTime.UtcNow.Date
        ), It.IsAny<CancellationToken>()), Times.Once);

        _mockLogger.Object.LogInformation("RecordUserActivityAsync_UpdatesLastActivityAndIncrementsMessagesCount completed with {UserId}", UserServiceTestsConstants.ExistingUserId);
    }

    /// <summary>
    /// Tests that RecordUserActivityAsync doesn't throw exception when user doesn't exist.
    /// </summary>
    [Fact]
    public async Task RecordUserActivityAsync_WithNonExistingUser_DoesNotThrow()
    {

        _mockLogger.Object.LogInformation("RecordUserActivityAsync_WithNonExistingUser_DoesNotThrow called with {UserId}", UserServiceTestsConstants.NonExistingUserId);

        // Arrange
        _mockLogger.Object.LogWarning("Skipping activity update fallback for missing user {UserId}", UserServiceTestsConstants.NonExistingUserId);

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(UserServiceTestsConstants.NonExistingUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BotUser?)null);

        // Act & Assert
        await _userService.Invoking(s => s.RecordUserActivityAsync(999))
            .Should().NotThrowAsync();

        _mockLogger.Object.LogInformation("RecordUserActivityAsync_WithNonExistingUser_DoesNotThrow completed with {UserId}", UserServiceTestsConstants.NonExistingUserId);
    }

    /// <summary>
    /// Tests that UpdateUserAsync updates user properties with provided values.
    /// </summary>
    /// <returns>Returns the updated user with new properties.</returns>
    [Fact]
    public async Task UpdateUserAsync_UpdatesUserProperties()
    {

        _mockLogger.Object.LogInformation("UpdateUserAsync_UpdatesUserProperties called with {UserId}", UserServiceTestsConstants.ExistingUserId);

        // Arrange
        var user = new BotUser { UserId = UserServiceTestsConstants.ExistingUserId, FirstName = UserServiceTestsConstants.FirstNameJohn, LastName = UserServiceTestsConstants.LastNameDoe, Username = UserServiceTestsConstants.UsernameJohnDoe };
        var updatedUser = new BotUser { UserId = UserServiceTestsConstants.ExistingUserId, FirstName = UserServiceTestsConstants.FirstNameJohn, LastName = UserServiceTestsConstants.LastNameSmithUpdated, Username = "johnsmith" };

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(UserServiceTestsConstants.ExistingUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockUserRepository
            .Setup(r => r.UpdateAsync(It.IsAny<BotUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _userService.UpdateUserAsync(UserServiceTestsConstants.ExistingUserId, UserServiceTestsConstants.FirstNameJohn, UserServiceTestsConstants.LastNameSmithUpdated, "johnsmith").ConfigureAwait(false);

        // Assert
        result.Should().Be(updatedUser);
        result.LastName.Should().Be(UserServiceTestsConstants.LastNameSmithUpdated);
        result.Username.Should().Be("johnsmith");
        _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<BotUser>(), It.IsAny<CancellationToken>()), Times.Once);

        _mockLogger.Object.LogInformation("UpdateUserAsync_UpdatesUserProperties completed with {UserId}", UserServiceTestsConstants.ExistingUserId);
    }

    /// <summary>
    /// Tests that UpdateUserAsync preserves unchanged values when partial updates are provided.
    /// </summary>
    /// <returns>Returns the user with unchanged values preserved.</returns>
    [Fact]
    public async Task UpdateUserAsync_WithPartialUpdates_PreservesUnchangedValues()
    {

        _mockLogger.Object.LogInformation("UpdateUserAsync_WithPartialUpdates_PreservesUnchangedValues called with {UserId}", UserServiceTestsConstants.ExistingUserId);

        // Arrange
        _mockLogger.Object.LogWarning("Applying partial user update fallback for {UserId}", UserServiceTestsConstants.ExistingUserId);

        var user = new BotUser { UserId = UserServiceTestsConstants.ExistingUserId, FirstName = UserServiceTestsConstants.FirstNameJohn, LastName = UserServiceTestsConstants.LastNameDoe, Username = UserServiceTestsConstants.UsernameJohnDoe };

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(UserServiceTestsConstants.ExistingUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockUserRepository
            .Setup(r => r.UpdateAsync(It.IsAny<BotUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.UpdateUserAsync(UserServiceTestsConstants.ExistingUserId, UserServiceTestsConstants.FirstNameJohn, null, null).ConfigureAwait(false);

        // Assert
        result.Should().Be(user);
        result.LastName.Should().Be(UserServiceTestsConstants.LastNameDoe);
        result.Username.Should().Be(UserServiceTestsConstants.UsernameJohnDoe);

        _mockLogger.Object.LogInformation("UpdateUserAsync_WithPartialUpdates_PreservesUnchangedValues completed with {UserId}", UserServiceTestsConstants.ExistingUserId);
    }

    /// <summary>
    /// Tests that DeleteUserAsync deletes existing user and returns true.
    /// </summary>
    /// <returns>Returns true when user is successfully deleted.</returns>
    [Fact]
    public async Task DeleteUserAsync_WithExistingUser_DeletesAndReturnsTrue()
    {

        _mockLogger.Object.LogInformation("DeleteUserAsync_WithExistingUser_DeletesAndReturnsTrue called with {UserId}", UserServiceTestsConstants.ExistingUserId);

        // Arrange
        _mockUserRepository
            .Setup(r => r.DeleteAsync(UserServiceTestsConstants.ExistingUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.DeleteUserAsync(UserServiceTestsConstants.ExistingUserId).ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();
        _mockUserRepository.Verify(r => r.DeleteAsync(UserServiceTestsConstants.ExistingUserId, It.IsAny<CancellationToken>()), Times.Once);

        _mockLogger.Object.LogInformation("DeleteUserAsync_WithExistingUser_DeletesAndReturnsTrue completed with {UserId}", UserServiceTestsConstants.ExistingUserId);
    }

    /// <summary>
    /// Tests that DeleteUserAsync returns false when user doesn't exist.
    /// </summary>
    /// <returns>Returns false when user doesn't exist.</returns>
    [Fact]
    public async Task DeleteUserAsync_WithNonExistingUser_ReturnsFalse()
    {

        _mockLogger.Object.LogInformation("DeleteUserAsync_WithNonExistingUser_ReturnsFalse called with {UserId}", UserServiceTestsConstants.NonExistingUserId);

        // Arrange
        _mockLogger.Object.LogWarning("Delete fallback returned false for missing user {UserId}", UserServiceTestsConstants.NonExistingUserId);

        _mockUserRepository
            .Setup(r => r.DeleteAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _userService.DeleteUserAsync(999).ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();

        _mockLogger.Object.LogInformation("DeleteUserAsync_WithNonExistingUser_ReturnsFalse completed with {UserId}", UserServiceTestsConstants.NonExistingUserId);
    }

    /// <summary>
    /// Tests that SearchUsersAsync filters users by first name using case-insensitive search.
    /// </summary>
    /// <returns>Returns filtered list of users matching the first name.</returns>
    [Fact]
    public async Task SearchUsersAsync_FiltersByFirstName()
    {

        _mockLogger.Object.LogInformation("SearchUsersAsync_FiltersByFirstName called with {Query}", UserServiceTestsConstants.FirstNameJohn);

        // Arrange
        var users = new List<BotUser>
        {
            new BotUser { UserId = 1, FirstName = UserServiceTestsConstants.FirstNameJohn, LastName = UserServiceTestsConstants.LastNameDoe },
            new BotUser { UserId = UserServiceTestsConstants.UserIdTwo, FirstName = UserServiceTestsConstants.FirstNameJane, LastName = UserServiceTestsConstants.LastNameSmithUpdated },
            new BotUser { UserId = UserServiceTestsConstants.UserIdThree, FirstName = UserServiceTestsConstants.FirstNameJohnny, LastName = UserServiceTestsConstants.LastNameAppleseed }
        };

        _mockUserRepository
            .Setup(r => r.SearchAsync(UserServiceTestsConstants.FirstNameJohn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(users.Where(u => u.FirstName.Contains(UserServiceTestsConstants.FirstNameJohn, StringComparison.OrdinalIgnoreCase)).ToList());

        // Act
        var result = await _userService.SearchUsersAsync(UserServiceTestsConstants.FirstNameJohn).ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(u => u.FirstName.Should().ContainEquivalentOf(UserServiceTestsConstants.FirstNameJohn));

        _mockLogger.Object.LogInformation("SearchUsersAsync_FiltersByFirstName completed with {ResultCount} results", result.Count);
    }

    /// <summary>
    /// Tests that SearchUsersAsync returns all users when empty query is provided.
    /// </summary>
    /// <returns>Returns all users in the repository.</returns>
    [Fact]
    public async Task SearchUsersAsync_WithEmptyQuery_ReturnsAllUsers()
    {

        _mockLogger.Object.LogInformation("SearchUsersAsync_WithEmptyQuery_ReturnsAllUsers called with {Query}", string.Empty);

        // Arrange
        _mockLogger.Object.LogWarning("Empty search query triggered all-users fallback");

        var users = new List<BotUser>
        {
            new BotUser { UserId = UserServiceTestsConstants.UserIdOne, FirstName = UserServiceTestsConstants.FirstNameJohn },
            new BotUser { UserId = UserServiceTestsConstants.UserIdTwo, FirstName = UserServiceTestsConstants.FirstNameJane }
        };

        _mockUserRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await _userService.SearchUsersAsync("").ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(2);

        _mockLogger.Object.LogInformation("SearchUsersAsync_WithEmptyQuery_ReturnsAllUsers completed with {ResultCount} results", result.Count);
    }

    /// <summary>
    /// Tests that GetUsersByStatusAsync returns users filtered by the specified status.
    /// </summary>
    /// <returns>Returns filtered list of users matching the specified status.</returns>
    [Fact]
    public async Task GetUsersByStatusAsync_ReturnsFilteredUsers()
    {

        _mockLogger.Object.LogInformation("GetUsersByStatusAsync_ReturnsFilteredUsers called with {Status}", UserStatus.Active);

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

        _mockLogger.Object.LogInformation("GetUsersByStatusAsync_ReturnsFilteredUsers completed with {ResultCount} results for {Status}", result.Count, UserStatus.Active);
    }
}
