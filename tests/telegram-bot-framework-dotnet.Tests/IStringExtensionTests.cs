namespace TelegramBotFramework.Tests;

/// <summary>
/// Interface for string extension tests.
/// </summary>
public interface IStringExtensionTests
{
    void Truncate_VariousInputs_TruncatesCorrectly(string input, int maxLength, string expected);
    void Truncate_NullInput_ReturnsNull();
    void IsValidEmail_WithValidFormat_ReturnsTrue();
    void IsValidEmail_WithMissingAtSign_ReturnsFalse();
    void IsValidEmail_WithEmptyString_ReturnsFalse();
    void IsValidEmail_WithMissingDomain_ReturnsFalse();
    void Repeat_PositiveCount_ProducesRepeatedString();
    void Repeat_ZeroCount_ReturnsEmpty();
    void Repeat_NegativeCount_ReturnsEmpty();
    void ExtractNumbers_FromMixedString_ReturnsOnlyDigits();
    void ExtractNumbers_FromStringWithNoDigits_ReturnsEmpty();
    void EnsureStartsWith_WhenPrefixMissing_PrependPrefix();
    void EnsureStartsWith_WhenAlreadyHasPrefix_ReturnsUnchanged();
    void EnsureEndsWith_WhenSuffixMissing_AppendsSuffix();
    void EnsureEndsWith_WhenAlreadyHasSuffix_ReturnsUnchanged();
    void Capitalize_WithLowercaseFirstChar_CapitalizesFirstChar();
    void Capitalize_WithAlreadyCapitalized_ReturnsUnchanged();
    void IsAlphanumeric_WithPureAlphanumericString_ReturnsTrue();
    void IsAlphanumeric_WithSpecialCharacters_ReturnsFalse();
    void IsAlphanumeric_WithSpaces_ReturnsFalse();
}