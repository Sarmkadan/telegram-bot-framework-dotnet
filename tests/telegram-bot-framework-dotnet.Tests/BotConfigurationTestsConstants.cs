#nullable enable

namespace TelegramBotFramework.Tests;

/// <summary>
/// Constants used by <see cref="BotConfigurationTests"/>.
/// </summary>
internal static class BotConfigurationTestsConstants
{
    public const string TestBotToken = "test-token-123";
    public const string TestBotUsername = "TestBot";
    public const string DefaultLocalizationLanguage = "en";
    public const string EmptyValue = "";
    public const string WhitespaceValue = "   ";

    public const string BotTokenRequiredMessage = "BotToken is required";
    public const string BotUsernameRequiredMessage = "BotUsername is required";
    public const string SessionTimeoutRequiredMessage = "SessionTimeoutMinutes must be at least 1";
    public const string MaxConcurrentRequestsRequiredMessage = "MaxConcurrentRequests must be at least 1";

    public const int DefaultSessionTimeoutMinutes = 30;
    public const int DefaultMessageProcessingTimeoutSeconds = 10;
    public const int DefaultMaxConcurrentRequests = 10;
    public const int DefaultRateLimitPerMinute = 30;
    public const int ValidSessionTimeoutMinutes = 5;
    public const int ValidMaxConcurrentRequests = 20;
    public const int ZeroValue = 0;
    public const int NegativeSessionTimeoutMinutes = -5;
    public const int NegativeMaxConcurrentRequests = -1;
    public const int ExpectedSingleItemCount = 1;
    public const int ExpectedTwoItemCount = 2;
    public const int TestSessionTimeoutMinutes = 45;

    public const long TestOwnerId = 12345;
    public const long TestAdminId = 67890;
    public const long NonAdminId = 99999;

    public const string ApiKeySettingKey = "api_key";
    public const string ApiKeySettingValue = "secret123";
    public const string EndpointSettingKey = "endpoint";
    public const string EndpointSettingValue = "https://api.example.com";
    public const string NonexistentSettingKey = "nonexistent";
    public const string ArbitrarySettingKey = "any_key";
    public const string NewSettingKey = "new_key";
    public const string NewSettingValue = "new_value";
    public const string ExistingSettingKey = "key";
    public const string OldSettingValue = "old_value";
    public const string SettingValue = "value";
}
