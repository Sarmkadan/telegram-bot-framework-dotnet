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
            _logger.LogInformation(AdminOperationsExampleConstants.StartingLogMessage);

            try
            {
                // Create multiple users with different scenarios
                await DemonstrateUserRoleManagementAsync().ConfigureAwait(false);
                await DemonstrateBanAndSuspensionAsync().ConfigureAwait(false);
                await DemonstrateUserQueryingAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, AdminOperationsExampleConstants.ErrorLogMessage);
                throw;
            }
        }

        private async Task DemonstrateUserRoleManagementAsync()
        {
            _logger.LogInformation(AdminOperationsExampleConstants.UserRoleManagementHeading);

            // Create a regular user
            var user1 = await _userService.GetOrCreateUserAsync(AdminOperationsExampleConstants.AliceTelegramId, AdminOperationsExampleConstants.AliceFirstName, AdminOperationsExampleConstants.AliceLastName).ConfigureAwait(false);
            _logger.LogInformation(AdminOperationsExampleConstants.UserCreatedLogMessage,
                user1.Id, user1.FirstName, user1.Role);

            // Create another user and promote to moderator
            var user2 = await _userService.GetOrCreateUserAsync(AdminOperationsExampleConstants.BobTelegramId, AdminOperationsExampleConstants.BobFirstName, AdminOperationsExampleConstants.BobLastName).ConfigureAwait(false);
            await _userService.PromoteToModeratorAsync(user2.Id).ConfigureAwait(false);
            var updatedUser2 = await _userService.GetUserByIdAsync(user2.Id).ConfigureAwait(false);
            _logger.LogInformation(AdminOperationsExampleConstants.UserPromotedLogMessage, user2.Id, updatedUser2?.Role);

            // Create another user and promote to admin
            var user3 = await _userService.GetOrCreateUserAsync(AdminOperationsExampleConstants.CharlieTelegramId, AdminOperationsExampleConstants.CharlieFirstName, AdminOperationsExampleConstants.CharlieLastName).ConfigureAwait(false);
            await _userService.PromoteToAdminAsync(user3.Id).ConfigureAwait(false);
            var updatedUser3 = await _userService.GetUserByIdAsync(user3.Id).ConfigureAwait(false);
            _logger.LogInformation(AdminOperationsExampleConstants.UserPromotedLogMessage, user3.Id, updatedUser3?.Role);

            // Create owner user
            var user4 = await _userService.GetOrCreateUserAsync(AdminOperationsExampleConstants.DaveTelegramId, AdminOperationsExampleConstants.DaveFirstName, AdminOperationsExampleConstants.DaveLastName).ConfigureAwait(false);
            await _userService.PromoteToAdminAsync(user4.Id).ConfigureAwait(false);
            var updatedUser4 = await _userService.GetUserByIdAsync(user4.Id).ConfigureAwait(false);
            _logger.LogInformation(AdminOperationsExampleConstants.UserCreatedWithRoleLogMessage, user4.Id, updatedUser4?.Role);

            // Demote admin back to moderator
            await _userService.DemoteFromAdminAsync(updatedUser3.Id).ConfigureAwait(false);
            var demotedUser3 = await _userService.GetUserByIdAsync(user3.Id).ConfigureAwait(false);
            _logger.LogInformation(AdminOperationsExampleConstants.UserDemotedLogMessage, user3.Id, demotedUser3?.Role);
        }

        private async Task DemonstrateBanAndSuspensionAsync()
        {
            _logger.LogInformation(AdminOperationsExampleConstants.BanAndSuspensionHeading);

            // Create user to ban
            var spamUser = await _userService.GetOrCreateUserAsync(AdminOperationsExampleConstants.SpamUserTelegramId, AdminOperationsExampleConstants.SpamUserFirstName, AdminOperationsExampleConstants.SpamUserLastName).ConfigureAwait(false);
            _logger.LogInformation(AdminOperationsExampleConstants.PotentialSpamUserCreatedLogMessage, spamUser.Id);

            // Ban the user
            await _userService.BanUserAsync(spamUser.Id, AdminOperationsExampleConstants.SpamBanReason).ConfigureAwait(false);
            var bannedUser = await _userService.GetUserByIdAsync(spamUser.Id).ConfigureAwait(false);
            _logger.LogInformation(AdminOperationsExampleConstants.UserBannedLogMessage, spamUser.Id, bannedUser?.Status);

            // Unban the user
            await _userService.UnbanUserAsync(spamUser.Id).ConfigureAwait(false);
            var unbannedUser = await _userService.GetUserByIdAsync(spamUser.Id).ConfigureAwait(false);
            _logger.LogInformation(AdminOperationsExampleConstants.UserUnbannedLogMessage, spamUser.Id, unbannedUser?.Status);

            // Suspend user temporarily
            var suspendUser = await _userService.GetOrCreateUserAsync(AdminOperationsExampleConstants.SuspendedUserTelegramId, AdminOperationsExampleConstants.SuspendedUserFirstName, AdminOperationsExampleConstants.SuspendedUserLastName).ConfigureAwait(false);
            await _userService.SuspendUserAsync(suspendUser.Id, TimeSpan.FromHours(AdminOperationsExampleConstants.SuspensionDurationHours)).ConfigureAwait(false);
            var suspendedUser = await _userService.GetUserByIdAsync(suspendUser.Id).ConfigureAwait(false);
            _logger.LogInformation(AdminOperationsExampleConstants.UserSuspendedLogMessage, suspendUser.Id, suspendedUser?.Status);
        }

        private async Task DemonstrateUserQueryingAsync()
        {
            _logger.LogInformation(AdminOperationsExampleConstants.UserQueryingHeading);

            // Create multiple users
            var users = new List<long>
            {
                AdminOperationsExampleConstants.FirstQueryUserTelegramId,
                AdminOperationsExampleConstants.SecondQueryUserTelegramId,
                AdminOperationsExampleConstants.ThirdQueryUserTelegramId
            };
            foreach (var userId in users)
            {
                await _userService.GetOrCreateUserAsync(userId, AdminOperationsExampleConstants.QueryUserFirstName, userId.ToString()).ConfigureAwait(false);
            }

            // Query user by telegram ID
            var user = await _userService.GetUserByTelegramIdAsync(AdminOperationsExampleConstants.FirstQueryUserTelegramId).ConfigureAwait(false);
            if (user  is not null)
            {
                _logger.LogInformation(AdminOperationsExampleConstants.UserFoundByTelegramIdLogMessage,
                    user.FirstName, user.LastName);
            }

            // Update user profile information
            if (user  is not null)
            {
                user.Username = AdminOperationsExampleConstants.ExampleUsername;
                user.PhoneNumber = AdminOperationsExampleConstants.ExamplePhoneNumber;
                user.Metadata[AdminOperationsExampleConstants.LocationMetadataKey] = AdminOperationsExampleConstants.ExampleLocation;
                await _userService.UpdateUserAsync(user).ConfigureAwait(false);
                _logger.LogInformation(AdminOperationsExampleConstants.UserProfileUpdatedLogMessage, user.Id);
            }

            // Get user with full details
            var detailedUser = await _userService.GetUserByIdAsync(user!.Id).ConfigureAwait(false);
            if (detailedUser  is not null)
            {
                _logger.LogInformation(AdminOperationsExampleConstants.UserDetailsLogMessage,
                    detailedUser.Id, detailedUser.TelegramId, detailedUser.Username,
                    detailedUser.Status, detailedUser.Role);
            }
        }
    }
}
