#nullable enable

namespace TelegramBotFramework.Tests;

internal static class InlineKeyboardBuilderEdgeCaseTestsConstants
{
    public const string EmptyKeyboardMessagePattern = "*empty keyboard*";
    public const string WhitespaceValue = "   ";
    public const string DuplicateCallbackData = "same_data";
    public const string DuplicateButtonText = "OK";
    public const string DefaultCallbackData = "data";
    public const string TestButtonText = "Test";
    public const string SearchButtonText = "Search";
    public const string FirstButtonText = "Btn1";
    public const string SecondButtonText = "Btn2";
    public const string ThirdButtonText = "Btn3";
    public const string GenericButtonText = "Btn";
    public const string UrlButtonText = "Text";
    public const string FirstCallbackData = "data1";
    public const string SecondCallbackData = "data2";
    public const string ThirdCallbackData = "data3";
    public const string EmptyValue = "";
    public const string LabelA = "A";
    public const string LabelB = "B";
    public const string LabelC = "C";
    public const string LabelD = "D";
    public const string LabelE = "E";
    public const string LabelF = "F";
    public const string CallbackDataA = "a";
    public const string CallbackDataB = "b";
    public const string CallbackDataC = "c";
    public const string CallbackDataD = "d";
    public const string CallbackDataE = "e";
    public const string CallbackDataF = "f";
    public const string UnicodeCallbackData = "café";
    public const string CallbackDataByteLimitMessagePattern = "*64*byte*";
    public const string FirstUrl = "https://example.com/1";
    public const string SecondUrl = "https://example.com/2";
    public const string ExampleUrl = "https://example.com";
    public const char LongTextCharacter = 'A';
    public const char CallbackDataCharacter = 'x';
    public const char UnicodeCharacter = 'é';
    public const int MinimumButtonsPerRow = 1;
    public const int LargeButtonsPerRow = 100;
    public const int TwoButtonsPerRow = 2;
    public const int ThreeButtonsPerRow = 3;
    public const int LongButtonTextLength = 1000;
    public const int CallbackDataByteLimit = 64;
    public const int CallbackDataLengthOverLimit = 65;
    public const int InvalidZeroButtonsPerRow = 0;
    public const int InvalidNegativeButtonsPerRow = -1;
}
