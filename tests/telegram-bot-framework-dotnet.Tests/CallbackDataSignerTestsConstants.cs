#nullable enable

namespace TelegramBotFramework.Tests;

internal static class CallbackDataSignerTestsConstants
{
    public const string TestSecret = "test-secret-key-123";
    public const string TestData = "user_action:123";
    public const string SignedDataSeparator = "|";
    public const string TamperedDataSeparator = ";";
    public const string WhitespaceValue = "   ";
    public const string WrongSecret = "wrong-secret";
    public const string DataWithoutSeparator = "invalid_signed_data";
    public const string DataWithSeparatorAtEnd = "data|";
    public const string FirstDistinctSecret = "secret1";
    public const string SecondDistinctSecret = "secret2";
    public const string ConsistentSecret = "consistent-secret";
    public const string LengthLimitTestSecret = "test-secret";
    public const string MalformedSignedData = "incomplete|sig";
    public const string RoundTripData = "command:delete_user:12345";
    public const string RoundTripSecret = "my-secret-key";
    public const char RepeatedDataCharacter = 'x';
    public const int LongDataLength = 40;
    public const int ExcessiveDataLength = 100;
    public const int TelegramCallbackDataByteLimit = 64;
}
