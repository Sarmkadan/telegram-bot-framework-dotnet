#nullable enable

namespace TelegramBotFramework.Examples
{
    internal static class AdminOperationsExampleConstants
    {
        public const long AliceTelegramId = 111111111;
        public const long BobTelegramId = 222222222;
        public const long CharlieTelegramId = 333333333;
        public const long DaveTelegramId = 444444444;
        public const long SpamUserTelegramId = 555555555;
        public const long SuspendedUserTelegramId = 666666666;
        public const long FirstQueryUserTelegramId = 777777777;
        public const long SecondQueryUserTelegramId = 888888888;
        public const long ThirdQueryUserTelegramId = 999999999;
        public const double SuspensionDurationHours = 24;

        public const string AliceFirstName = "Alice";
        public const string AliceLastName = "Smith";
        public const string BobFirstName = "Bob";
        public const string BobLastName = "Johnson";
        public const string CharlieFirstName = "Charlie";
        public const string CharlieLastName = "Brown";
        public const string DaveFirstName = "Dave";
        public const string DaveLastName = "Wilson";
        public const string SpamUserFirstName = "Spam";
        public const string SpamUserLastName = "Bot";
        public const string SuspendedUserFirstName = "Temp";
        public const string SuspendedUserLastName = "Ban";
        public const string QueryUserFirstName = "User";
        public const string SpamBanReason = "Spamming content";
        public const string ExampleUsername = "user_username";
        public const string ExamplePhoneNumber = "+1234567890";
        public const string LocationMetadataKey = "location";
        public const string ExampleLocation = "New York";

        public const string StartingLogMessage = "Starting AdminOperationsExample";
        public const string ErrorLogMessage = "Error in AdminOperationsExample";
        public const string UserRoleManagementHeading = "--- User Role Management ---";
        public const string UserCreatedLogMessage = "Created user: {UserId} ({FirstName}) with role {Role}";
        public const string UserPromotedLogMessage = "Promoted {UserId} to {Role}";
        public const string UserCreatedWithRoleLogMessage = "Created {UserId} with {Role}";
        public const string UserDemotedLogMessage = "Demoted {UserId} to {Role}";
        public const string BanAndSuspensionHeading = "--- Ban and Suspension Management ---";
        public const string PotentialSpamUserCreatedLogMessage = "Created potential spam user: {UserId}";
        public const string UserBannedLogMessage = "Banned user {UserId}, Status: {Status}";
        public const string UserUnbannedLogMessage = "Unbanned user {UserId}, Status: {Status}";
        public const string UserSuspendedLogMessage = "Suspended user {UserId}, Status: {Status}";
        public const string UserQueryingHeading = "--- User Querying ---";
        public const string UserFoundByTelegramIdLogMessage = "Found user by Telegram ID: {FirstName} {LastName}";
        public const string UserProfileUpdatedLogMessage = "Updated user profile: {UserId}";
        public const string UserDetailsLogMessage = "User Details: ID={Id}, Telegram={TId}, Username={Username}, Status={Status}, Role={Role}";
    }
}
