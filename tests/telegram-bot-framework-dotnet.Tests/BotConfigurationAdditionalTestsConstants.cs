#nullable enable

namespace TelegramBotFramework.Tests;

/// <summary>
/// Constants used by <see cref="BotConfigurationAdditionalTests"/>.
/// </summary>
internal static class BotConfigurationAdditionalTestsConstants
{
    public const string TestBotToken = "test-token";
    public const string ValidTestBotToken = "test-token-123";
    public const string TestBotUsername = "TestBot";
    public const string WhitespaceValue = " ";
    public const string SingleCharacterBotToken = "x";
    public const string CustomSettingKey = "key";
    public const string CustomSettingValue = "value";
    public const string OldCustomSettingValue = "old_value";
    public const string NewCustomSettingValue = "new_value";
    public const string BotUsernameRequiredMessage = "BotUsername is required";
    public const string BotTokenRequiredMessage = "BotToken is required";

    public const long TestAdminId = 12345;
    public const long FirstAdminId = 111;
    public const long AdminIdToRemove = 222;
    public const long ThirdAdminId = 333;

    public const int SingleItemCount = 1;
    public const int RemainingAdminCount = 2;
    public const int DefaultSessionTimeoutMinutes = 30;
    public const int CustomSessionTimeoutMinutes = 60;
    public const int MaximumConcurrentRequests = 100;
    public const int MinimumValidValue = 1;

    public static readonly TimeSpan DefaultSessionTimeout =
        TimeSpan.FromMinutes(DefaultSessionTimeoutMinutes);

    public static readonly TimeSpan CustomSessionTimeout =
        TimeSpan.FromMinutes(CustomSessionTimeoutMinutes);
}
