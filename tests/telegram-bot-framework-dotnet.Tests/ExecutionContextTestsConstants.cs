namespace TelegramBotFramework.Tests;

/// <summary>
/// Constants for ExecutionContextTests.
/// </summary>
internal static class ExecutionContextTestsConstants
{
    /// <summary>
    /// The default test message content.
    /// </summary>
    public const string DefaultTestMessage = "test";

    /// <summary>
    /// The default session ID used in tests.
    /// </summary>
    public const string DefaultSessionId = "session-123";

    /// <summary>
    /// The valid error message used in tests.
    /// </summary>
    public const string ValidErrorMessage = "Valid error";

    /// <summary>
    /// The first test error message.
    /// </summary>
    public const string TestErrorMessage1 = "Test error 1";

    /// <summary>
    /// The second test error message.
    /// </summary>
    public const string TestErrorMessage2 = "Test error 2";

    /// <summary>
    /// The first state key used in tests.
    /// </summary>
    public const string StateKey1 = "key1";

    /// <summary>
    /// The second state key used in tests.
    /// </summary>
    public const string StateKey2 = "key2";

    /// <summary>
    /// The third state key used in tests.
    /// </summary>
    public const string StateKey3 = "key3";

    /// <summary>
    /// The first state value used in tests.
    /// </summary>
    public const string StateValue1 = "value1";

    /// <summary>
    /// The numeric state value used in tests.
    /// </summary>
    public const int NumericStateValue = 123;

    /// <summary>
    /// The old state value used in overwrite tests.
    /// </summary>
    public const string OldStateValue = "old_value";

    /// <summary>
    /// The new state value used in overwrite tests.
    /// </summary>
    public const string NewStateValue = "new_value";

    /// <summary>
    /// The generic state value used in tests.
    /// </summary>
    public const string GenericStateValue = "value";

    /// <summary>
    /// The test state key used in GetState tests.
    /// </summary>
    public const string TestStateKey = "test_key";

    /// <summary>
    /// The test state value used in GetState tests.
    /// </summary>
    public const string TestStateValue = "test_value";

    /// <summary>
    /// The non-existent state key used in GetState tests.
    /// </summary>
    public const string NonExistentStateKey = "nonexistent";

    /// <summary>
    /// The number state key used in GetState type tests.
    /// </summary>
    public const string NumberStateKey = "number_key";

    /// <summary>
    /// The state key used in overwrite and key-validation tests.
    /// </summary>
    public const string GenericStateKey = "key";

    /// <summary>
    /// The default test user's first name.
    /// </summary>
    public const string DefaultFirstName = "John";

    /// <summary>
    /// The user ID fragment expected in validation errors.
    /// </summary>
    public const string UserIdErrorFragment = "UserId";

    /// <summary>
    /// The chat ID fragment expected in validation errors.
    /// </summary>
    public const string ChatIdErrorFragment = "ChatId";

    /// <summary>
    /// The validation error message used in tests.
    /// </summary>
    public const string ValidationErrorMessage = "Error occurred";

    /// <summary>
    /// The default user ID used in tests.
    /// </summary>
    public const long DefaultUserId = 123;

    /// <summary>
    /// The default chat ID used in tests.
    /// </summary>
    public const long DefaultChatId = 456;

    /// <summary>
    /// The default message ID used in tests.
    /// </summary>
    public const long DefaultMessageId = 1;

    /// <summary>
    /// The zero ID used in validation tests.
    /// </summary>
    public const long ZeroId = 0;

    /// <summary>
    /// The short sleep duration used in timing tests (milliseconds).
    /// </summary>
    public const int ShortSleepDuration = 10;

    /// <summary>
    /// The time tolerance used in timing tests (milliseconds).
    /// </summary>
    public const int TimeToleranceMilliseconds = 50;

    /// <summary>
    /// The expected count when a collection contains one item.
    /// </summary>
    public const int SingleItemCount = 1;

    /// <summary>
    /// The expected count when a collection contains two items.
    /// </summary>
    public const int TwoItemCount = 2;

    /// <summary>
    /// The expected count when a collection contains three items.
    /// </summary>
    public const int ThreeItemCount = 3;

    /// <summary>
    /// The empty string constant.
    /// </>
    public const string EmptyString = "";

    /// <summary>
    /// The boolean true constant.
    /// </summary>
    public const bool TrueValue = true;

    /// <summary>
    /// The tolerance for timestamps created during a test.
    /// </summary>
    public static readonly TimeSpan CreationTimeTolerance = TimeSpan.FromSeconds(1);
}
