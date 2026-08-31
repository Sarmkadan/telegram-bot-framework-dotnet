#nullable enable

namespace TelegramBotFramework.Tests.Integration;

internal static class PollingStrategyTestsConstants
{
    public const int InitialUpdateOffset = 0;
    public const int FirstUpdateId = 100;
    public const int SecondUpdateId = 101;
    public const int ThirdUpdateId = 102;
    public const int ProcessedUpdateId = 123;
    public const int EventUpdateId = 456;
    public const int FailingUpdateId = 789;
    public const int StatusUpdateId = 999;
    public const long MessageIdOffset = 1000;
    public const long DefaultChatId = 123;
    public const long DefaultFromId = 456;
    public const long DefaultDate = 1234567890;
    public const string DefaultTestText = "Test";
    public const string JsonUpdate = "{\"update_id\": 123, \"message\": {\"message_id\": 456, \"chat\": {\"id\": 789}, \"from\": {\"id\": 101112}, \"date\": 1234567890, \"text\": \"Hello\"}}";

    public const int ShortPollIntervalMs = 50;
    public const int StandardPollIntervalMs = 100;
    public const int LongPollIntervalMs = 500;

    public const int ShortDelayMs = 100;
    public const int AdditionalPollDelayMs = 150;
    public const int MediumDelayMs = 200;
    public const int LongDelayMs = 300;
    public const int ExtraLongDelayMs = 350;
    public const int VeryLongDelayMs = 800;

    public const string AlreadyRunningLogSubstring = "already running";
    public const string PollingStoppedLogSubstring = "Polling stopped";
    public const string ErrorProcessingUpdateLogSubstring = "Error processing update";
    public const string CustomIntervalLogSubstring = "500ms";
    public const string DefaultIntervalLogSubstring = "1000ms";
    public const string TestExceptionMessage = "Test exception";
    public const string ApiFailureMessage = "API failure";
    public const string UpdateJsonFormat = "{{ \"update_id\": {0}, \"message\": {{ \"message_id\": {1}, \"chat\": {{ \"id\": {2} }}, \"from\": {{ \"id\": {3} }}, \"date\": {4}, \"text\": \"{5}\" }} }}";
}
