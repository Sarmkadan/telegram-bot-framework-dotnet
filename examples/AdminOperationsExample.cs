#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;

namespace TelegramBotFramework.Examples
{
    /// <summary>
    /// Admin operations example demonstrating user role management, banning, promoting users,
    /// and managing bot configuration from code.
    /// </summary>
public sealed class AdminOperationsExample
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AdminOperationsExample> _logger;
        private readonly IUserService _userService;

        public AdminOperationsExample(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<AdminOperationsExample>>();
            _userService = serviceProvider.GetRequiredService<IUserService>();
        }

        public async Task RunAsync()
        {
            _logger.LogInformation("Starting AdminOperationsExample");

            try
            {
                // Create multiple users with different scenarios
                await DemonstrateUserRoleManagementAsync().ConfigureAwait(false);
                await DemonstrateBanAndSuspensionAsync().ConfigureAwait(false);
                await DemonstrateUserQueryingAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AdminOperationsExample");
                throw;
            }
        }

        private async Task DemonstrateUserRoleManagementAsync()
        {
            _logger.LogInformation("--- User Role Management ---");

            // Create a regular user
            var user1 = await _userService.GetOrCreateUserAsync(111111111, "Alice", "Smith").ConfigureAwait(false);
            _logger.LogInformation("Created user: {UserId} ({FirstName}) with role {Role}",
                user1.Id, user1.FirstName, user1.Role);

            // Create another user and promote to moderator
            var user2 = await _userService.GetOrCreateUserAsync(222222222, "Bob", "Johnson").ConfigureAwait(false);
            await _userService.PromoteToModeratorAsync(user2.Id).ConfigureAwait(false);
            var updatedUser2 = await _userService.GetUserByIdAsync(user2.Id).ConfigureAwait(false);
            _logger.LogInformation("Promoted {UserId} to {Role}", user2.Id, updatedUser2?.Role);

            // Create another user and promote to admin
            var user3 = await _userService.GetOrCreateUserAsync(333333333, "Charlie", "Brown").ConfigureAwait(false);
            await _userService.PromoteToAdminAsync(user3.Id).ConfigureAwait(false);
            var updatedUser3 = await _userService.GetUserByIdAsync(user3.Id).ConfigureAwait(false);
            _logger.LogInformation("Promoted {UserId} to {Role}", user3.Id, updatedUser3?.Role);

            // Create owner user
            var user4 = await _userService.GetOrCreateUserAsync(444444444, "Dave", "Wilson").ConfigureAwait(false);
            await _userService.PromoteToAdminAsync(user4.Id).ConfigureAwait(false);
            var updatedUser4 = await _userService.GetUserByIdAsync(user4.Id).ConfigureAwait(false);
            _logger.LogInformation("Created {UserId} with {Role}", user4.Id, updatedUser4?.Role);

            // Demote admin back to moderator
            await _userService.DemoteFromAdminAsync(updatedUser3.Id).ConfigureAwait(false);
            var demotedUser3 = await _userService.GetUserByIdAsync(user3.Id).ConfigureAwait(false);
            _logger.LogInformation("Demoted {UserId} to {Role}", user3.Id, demotedUser3?.Role);
        }

        private async Task DemonstrateBanAndSuspensionAsync()
        {
            _logger.LogInformation("--- Ban and Suspension Management ---");

            // Create user to ban
            var spamUser = await _userService.GetOrCreateUserAsync(555555555, "Spam", "Bot").ConfigureAwait(false);
            _logger.LogInformation("Created potential spam user: {UserId}", spamUser.Id);

            // Ban the user
            await _userService.BanUserAsync(spamUser.Id, "Spamming content").ConfigureAwait(false);
            var bannedUser = await _userService.GetUserByIdAsync(spamUser.Id).ConfigureAwait(false);
            _logger.LogInformation("Banned user {UserId}, Status: {Status}", spamUser.Id, bannedUser?.Status);

            // Unban the user
            await _userService.UnbanUserAsync(spamUser.Id).ConfigureAwait(false);
            var unbannedUser = await _userService.GetUserByIdAsync(spamUser.Id).ConfigureAwait(false);
            _logger.LogInformation("Unbanned user {UserId}, Status: {Status}", spamUser.Id, unbannedUser?.Status);

            // Suspend user temporarily
            var suspendUser = await _userService.GetOrCreateUserAsync(666666666, "Temp", "Ban").ConfigureAwait(false);
            await _userService.SuspendUserAsync(suspendUser.Id, TimeSpan.FromHours(24)).ConfigureAwait(false);
            var suspendedUser = await _userService.GetUserByIdAsync(suspendUser.Id).ConfigureAwait(false);
            _logger.LogInformation("Suspended user {UserId}, Status: {Status}", suspendUser.Id, suspendedUser?.Status);
        }

        private async Task DemonstrateUserQueryingAsync()
        {
            _logger.LogInformation("--- User Querying ---");

            // Create multiple users
            var users = new List<long> { 777777777, 888888888, 999999999 };
            foreach (var userId in users)
            {
                await _userService.GetOrCreateUserAsync(userId, "User", userId.ToString()).ConfigureAwait(false);
            }

            // Query user by telegram ID
            var user = await _userService.GetUserByTelegramIdAsync(777777777).ConfigureAwait(false);
            if (user  is not null)
            {
                _logger.LogInformation("Found user by Telegram ID: {FirstName} {LastName}",
                    user.FirstName, user.LastName);
            }

            // Update user profile information
            if (user  is not null)
            {
                user.Username = "user_username";
                user.PhoneNumber = "+1234567890";
                user.Metadata["location"] = "New York";
                await _userService.UpdateUserAsync(user).ConfigureAwait(false);
                _logger.LogInformation("Updated user profile: {UserId}", user.Id);
            }

            // Get user with full details
            var detailedUser = await _userService.GetUserByIdAsync(user!.Id).ConfigureAwait(false);
            if (detailedUser  is not null)
            {
                _logger.LogInformation("User Details: ID={Id}, Telegram={TId}, Username={Username}, Status={Status}, Role={Role}",
                    detailedUser.Id, detailedUser.TelegramId, detailedUser.Username,
                    detailedUser.Status, detailedUser.Role);
            }
        }
    }
}