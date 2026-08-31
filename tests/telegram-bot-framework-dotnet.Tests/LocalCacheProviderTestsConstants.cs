#nullable enable

namespace TelegramBotFramework.Tests;

/// <summary>
/// Constants used in LocalCacheProvider tests to avoid magic strings and numbers.
/// </summary>
internal static class LocalCacheProviderTestsConstants
{
    // String constants for cache keys
    public const string GreetingKey = "greeting";
    public const string GreetingValue = "hello";
    public const string MissingKey = "missing-key";
    public const string ExpiringKey = "expiring";
    public const string ExpiringValue = "value";
    public const string PersistentKey = "persistent";
    public const string PersistentValue = "alive";
    public const string ToRemoveKey = "toRemove";
    public const string PresentKey = "present";
    public const string NotThereKey = "not-there";
    public const string GoneSoonKey = "gone-soon";
    public const string XValue = "x";
    public const string NewKey = "new-key";
    public const string CreatedValue = "created";
    public const string ExistingKey = "existing";
    public const string CachedValue = "cached-value";
    public const string ShouldNotBeUsedValue = "should-not-be-used";
    public const string KeyA = "a";
    public const string KeyB = "b";
    public const string KeyC = "c";
    public const string TrackedKey = "tracked";
    public const string NonExistentKey = "non-existent";
    public const string WhitespaceKey = "   ";
    public const string ValidKey = "valid_key";
    public const string TestValue = "test_value";
    public const string ExpiringKeyName = "expiring_key";
    public const string ExpiringValueName = "expiring_value";
    public const string ExpiryTestKey = "expiry_test_key";
    public const string ExpiryTestValue = "expiry_test_value";
    public const string OverwriteKey = "overwrite_key";
    public const string InitialValue = "initial_value";
    public const string UpdatedValue = "updated_value";
    public const string NonExistentKeyName = "nonexistent_key";
    public const string NullValueKey = "null_value_key";
    public const string ComplexObjectKey = "complex_object_key";
    public const string ComplexObjectName = "Test Object";
    public const string Key1 = "key1";
    public const string Value1 = "value1";
    public const string Key2 = "key2";
    public const string Value2 = "value2";
    public const string GetOrCreateExpiringKey = "get_or_create_expiring_key";
    public const string RemoveKey = "remove_key";
    public const string RemoveValue = "remove_value";

    // Numeric constants for timeouts and delays
    public const short ShortExpirationMs = 1;
    public const short MediumExpirationMs = 50;
    public const short LongExpirationMs = 100;
    public const int OneHour = 1;
    public const int FirstTestIntegerValue = 1;
    public const int SecondTestIntegerValue = 2;
    public const int ThirdTestIntegerValue = 3;
    public const int TestIntegerValue = 42;
    public const int ComplexObjectId = 123;
    public const short ShortDelayMs = 50;
    public const short LongDelayMs = 100;
}
