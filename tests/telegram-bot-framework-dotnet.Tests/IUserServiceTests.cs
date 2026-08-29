#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Threading;
using System.Threading.Tasks;
using TelegramBotFramework.Models;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Interface defining the contract for UserService unit tests.
/// </summary>
public interface IUserServiceTests
{
    Task GetOrCreateUserAsync_WithExistingUser_ReturnsExistingUser();
    Task GetOrCreateUserAsync_WithNonExistingUser_CreatesAndReturnsNewUser();
    Task GetOrCreateUserAsync_WithNullLastName_CreatesUserWithoutLastName();
    Task GetOrCreateUserAsync_WithExistingUserWithDifferentDetails_UpdatesUser();
    Task GetUserByIdAsync_WithExistingUser_ReturnsUser();
    Task GetUserByIdAsync_WithNonExistingUser_ReturnsNull();
    Task RecordUserActivityAsync_UpdatesLastActivityAndIncrementsMessagesCount();
    Task RecordUserActivityAsync_WithNonExistingUser_DoesNotThrow();
    Task UpdateUserAsync_UpdatesUserProperties();
    Task UpdateUserAsync_WithPartialUpdates_PreservesUnchangedValues();
    Task DeleteUserAsync_WithExistingUser_DeletesAndReturnsTrue();
    Task DeleteUserAsync_WithNonExistingUser_ReturnsFalse();
    Task SearchUsersAsync_FiltersByFirstName();
    Task SearchUsersAsync_WithEmptyQuery_ReturnsAllUsers();
    Task GetUsersByStatusAsync_ReturnsFilteredUsers();
}