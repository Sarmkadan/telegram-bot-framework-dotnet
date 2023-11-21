#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using TelegramBotFramework.Utilities;
using Xunit;

namespace TelegramBotFramework.Tests;

public sealed class StringExtensionTests
{
    [Theory]
    [InlineData("Hello World", 5, "Hell…")]
    [InlineData("Hi", 10, "Hi")]
    [InlineData("Short", 5, "Short")]
    public void Truncate_VariousInputs_TruncatesCorrectly(string input, int maxLength, string expected)
    {
        input.Truncate(maxLength).Should().Be(expected);
    }

    [Fact]
    public void Truncate_NullInput_ReturnsNull()
    {
        string? value = null;
        value!.Truncate(10).Should().BeNull();
    }

    [Fact]
    public void IsValidEmail_WithValidFormat_ReturnsTrue()
    {
        "user@example.com".IsValidEmail().Should().BeTrue();
    }

    [Fact]
    public void IsValidEmail_WithMissingAtSign_ReturnsFalse()
    {
        "userexample.com".IsValidEmail().Should().BeFalse();
    }

    [Fact]
    public void IsValidEmail_WithEmptyString_ReturnsFalse()
    {
        "".IsValidEmail().Should().BeFalse();
    }

    [Fact]
    public void IsValidEmail_WithMissingDomain_ReturnsFalse()
    {
        "user@".IsValidEmail().Should().BeFalse();
    }

    [Fact]
    public void Repeat_PositiveCount_ProducesRepeatedString()
    {
        "ab".Repeat(3).Should().Be("ababab");
    }

    [Fact]
    public void Repeat_ZeroCount_ReturnsEmpty()
    {
        "abc".Repeat(0).Should().BeEmpty();
    }

    [Fact]
    public void Repeat_NegativeCount_ReturnsEmpty()
    {
        "abc".Repeat(-1).Should().BeEmpty();
    }

    [Fact]
    public void ExtractNumbers_FromMixedString_ReturnsOnlyDigits()
    {
        "abc123def456".ExtractNumbers().Should().Be("123456");
    }

    [Fact]
    public void ExtractNumbers_FromStringWithNoDigits_ReturnsEmpty()
    {
        "abcdef".ExtractNumbers().Should().BeEmpty();
    }

    [Fact]
    public void EnsureStartsWith_WhenPrefixMissing_PrependPrefix()
    {
        "example.com".EnsureStartsWith("https://").Should().Be("https://example.com");
    }

    [Fact]
    public void EnsureStartsWith_WhenAlreadyHasPrefix_ReturnsUnchanged()
    {
        "https://example.com".EnsureStartsWith("https://").Should().Be("https://example.com");
    }

    [Fact]
    public void EnsureEndsWith_WhenSuffixMissing_AppendsSuffix()
    {
        "hello".EnsureEndsWith("!").Should().Be("hello!");
    }

    [Fact]
    public void EnsureEndsWith_WhenAlreadyHasSuffix_ReturnsUnchanged()
    {
        "hello!".EnsureEndsWith("!").Should().Be("hello!");
    }

    [Fact]
    public void Capitalize_WithLowercaseFirstChar_CapitalizesFirstChar()
    {
        "hello world".Capitalize().Should().Be("Hello world");
    }

    [Fact]
    public void Capitalize_WithAlreadyCapitalized_ReturnsUnchanged()
    {
        "Hello".Capitalize().Should().Be("Hello");
    }

    [Fact]
    public void IsAlphanumeric_WithPureAlphanumericString_ReturnsTrue()
    {
        "abc123".IsAlphanumeric().Should().BeTrue();
    }

    [Fact]
    public void IsAlphanumeric_WithSpecialCharacters_ReturnsFalse()
    {
        "abc!123".IsAlphanumeric().Should().BeFalse();
    }

    [Fact]
    public void IsAlphanumeric_WithSpaces_ReturnsFalse()
    {
        "abc 123".IsAlphanumeric().Should().BeFalse();
    }

    [Fact]
    public void Reverse_OfPalindrome_ReturnsSameString()
    {
        "racecar".Reverse().Should().Be("racecar");
    }

    [Fact]
    public void Reverse_OfAsymmetricString_ReturnsReversed()
    {
        "hello".Reverse().Should().Be("olleh");
    }
}

public sealed class ValidationUtilityTests
{
    [Theory]
    [InlineData(1L, true)]
    [InlineData(999999999L, true)]
    [InlineData(0L, false)]
    [InlineData(-1L, false)]
    public void IsValidTelegramUserId_VariousInputs_ReturnsExpectedResult(long id, bool expected)
    {
        ValidationUtility.IsValidTelegramUserId(id).Should().Be(expected);
    }

    [Theory]
    [InlineData(12345L, true)]
    [InlineData(-100500L, true)]
    [InlineData(0L, false)]
    public void IsValidTelegramChatId_VariousInputs_ReturnsExpectedResult(long chatId, bool expected)
    {
        ValidationUtility.IsValidTelegramChatId(chatId).Should().Be(expected);
    }

    [Fact]
    public void IsValidCommandName_WithLeadingSlash_ReturnsTrue()
    {
        ValidationUtility.IsValidCommandName("/start").Should().BeTrue();
    }

    [Fact]
    public void IsValidCommandName_WithUnderscoreAllowed_ReturnsTrue()
    {
        ValidationUtility.IsValidCommandName("/get_status").Should().BeTrue();
    }

    [Fact]
    public void IsValidCommandName_WithoutLeadingSlash_ReturnsFalse()
    {
        ValidationUtility.IsValidCommandName("start").Should().BeFalse();
    }

    [Fact]
    public void IsValidCommandName_WithNullValue_ReturnsFalse()
    {
        ValidationUtility.IsValidCommandName(null).Should().BeFalse();
    }

    [Fact]
    public void IsValidCommandName_WithSpecialCharsAfterSlash_ReturnsFalse()
    {
        ValidationUtility.IsValidCommandName("/hello-world").Should().BeFalse();
    }

    [Fact]
    public void IsStrongPassword_WithAllRequirements_ReturnsTrue()
    {
        ValidationUtility.IsStrongPassword("SecureP@ss1").Should().BeTrue();
    }

    [Fact]
    public void IsStrongPassword_WithTooShortPassword_ReturnsFalse()
    {
        ValidationUtility.IsStrongPassword("Ab1!").Should().BeFalse();
    }

    [Fact]
    public void IsStrongPassword_WithNoSpecialCharacter_ReturnsFalse()
    {
        ValidationUtility.IsStrongPassword("SecurePass1").Should().BeFalse();
    }

    [Fact]
    public void IsStrongPassword_WithNoDigit_ReturnsFalse()
    {
        ValidationUtility.IsStrongPassword("SecureP@ssword").Should().BeFalse();
    }

    [Fact]
    public void IsStrongPassword_WithNoUppercase_ReturnsFalse()
    {
        ValidationUtility.IsStrongPassword("securep@ss1").Should().BeFalse();
    }

    [Fact]
    public void IsValidPhoneNumber_WithFormattedNumber_ReturnsTrue()
    {
        ValidationUtility.IsValidPhoneNumber("+1 (555) 123-4567").Should().BeTrue();
    }

    [Fact]
    public void IsValidPhoneNumber_WithTooFewDigits_ReturnsFalse()
    {
        ValidationUtility.IsValidPhoneNumber("123").Should().BeFalse();
    }

    [Fact]
    public void IsValidGuid_WithValidGuid_ReturnsTrue()
    {
        ValidationUtility.IsValidGuid(Guid.NewGuid().ToString()).Should().BeTrue();
    }

    [Fact]
    public void IsValidGuid_WithInvalidGuid_ReturnsFalse()
    {
        ValidationUtility.IsValidGuid("not-a-guid").Should().BeFalse();
    }

    [Fact]
    public void IsValidLength_WhenStringWithinRange_ReturnsTrue()
    {
        ValidationUtility.IsValidLength("hello", 3, 10).Should().BeTrue();
    }

    [Fact]
    public void IsValidLength_WhenStringExceedsMaximum_ReturnsFalse()
    {
        ValidationUtility.IsValidLength("hello world", 1, 5).Should().BeFalse();
    }

    [Fact]
    public void IsValidLength_WhenStringBelowMinimum_ReturnsFalse()
    {
        ValidationUtility.IsValidLength("hi", 5, 10).Should().BeFalse();
    }

    [Fact]
    public void IsNumeric_WithValidDecimal_ReturnsTrue()
    {
        ValidationUtility.IsNumeric("3.14").Should().BeTrue();
    }

    [Fact]
    public void IsNumeric_WithNegativeNumber_ReturnsTrue()
    {
        ValidationUtility.IsNumeric("-42").Should().BeTrue();
    }

    [Fact]
    public void IsNumeric_WithAlphaCharacters_ReturnsFalse()
    {
        ValidationUtility.IsNumeric("12abc").Should().BeFalse();
    }

    [Fact]
    public void IsValidIPv4_WithValidAddress_ReturnsTrue()
    {
        ValidationUtility.IsValidIPv4("192.168.1.1").Should().BeTrue();
    }

    [Fact]
    public void IsValidIPv4_WithOutOfRangeOctet_ReturnsFalse()
    {
        ValidationUtility.IsValidIPv4("999.168.1.1").Should().BeFalse();
    }
}

public sealed class CollectionExtensionTests
{
    [Fact]
    public void Chunk_DividesListIntoBatchesOfCorrectSize()
    {
        var list = Enumerable.Range(1, 10).ToList();

        var chunks = list.Chunk(3).ToList();

        chunks.Should().HaveCount(4);
        chunks[0].Should().HaveCount(3);
        chunks[3].Should().HaveCount(1);
    }

    [Fact]
    public void Chunk_WithBatchSizeOfOne_EachElementIsOwnBatch()
    {
        var list = new[] { "a", "b", "c" };

        var chunks = list.Chunk(1).ToList();

        chunks.Should().HaveCount(3);
    }

    [Fact]
    public void Chunk_WithBatchSizeZero_ThrowsArgumentException()
    {
        var list = new List<int> { 1, 2, 3 };

        var act = () => list.Chunk(0).ToList();

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IsNullOrEmpty_WithNull_ReturnsTrue()
    {
        IEnumerable<int>? source = null;

        source.IsNullOrEmpty().Should().BeTrue();
    }

    [Fact]
    public void IsNullOrEmpty_WithEmptyList_ReturnsTrue()
    {
        new List<string>().IsNullOrEmpty().Should().BeTrue();
    }

    [Fact]
    public void IsNullOrEmpty_WithItems_ReturnsFalse()
    {
        new[] { 1, 2, 3 }.IsNullOrEmpty().Should().BeFalse();
    }

    [Fact]
    public void HasItems_WithNonEmptyCollection_ReturnsTrue()
    {
        new[] { 1, 2, 3 }.HasItems().Should().BeTrue();
    }

    [Fact]
    public void HasItems_WithNull_ReturnsFalse()
    {
        IEnumerable<int>? source = null;

        source.HasItems().Should().BeFalse();
    }

    [Fact]
    public void DistinctBy_WithDuplicateKeys_ReturnsFirstOccurrenceOfEach()
    {
        var items = new[] { ("a", 1), ("b", 2), ("a", 3) };

        var distinct = items.DistinctBy(x => x.Item1).ToList();

        distinct.Should().HaveCount(2);
        distinct[0].Item2.Should().Be(1);
    }

    [Fact]
    public void DistinctBy_WithAllUniqueKeys_ReturnsAllItems()
    {
        var items = new[] { ("x", 10), ("y", 20), ("z", 30) };

        var distinct = items.DistinctBy(x => x.Item1).ToList();

        distinct.Should().HaveCount(3);
    }

    [Fact]
    public void GetOrDefault_WhenIndexOutOfBounds_ReturnsDefault()
    {
        var list = new List<string> { "a", "b" };

        list.GetOrDefault(5).Should().BeNull();
    }

    [Fact]
    public void GetOrDefault_WhenNegativeIndex_ReturnsDefault()
    {
        var list = new List<int> { 10, 20, 30 };

        list.GetOrDefault(-1, -1).Should().Be(-1);
    }

    [Fact]
    public void GetOrDefault_WhenIndexValid_ReturnsElement()
    {
        var list = new List<string> { "x", "y", "z" };

        list.GetOrDefault(2).Should().Be("z");
    }

    [Fact]
    public void ToDictionarySafe_WithDuplicateKeys_KeepsFirstOccurrence()
    {
        var items = new[] { ("key", 1), ("key", 2), ("other", 3) };

        var dict = items.ToDictionarySafe(x => x.Item1, x => x.Item2);

        dict["key"].Should().Be(1);
        dict.Should().HaveCount(2);
    }
}

public sealed class DateTimeExtensionTests
{
    [Fact]
    public void ToUnixTimestamp_WhenGivenUnixEpoch_ReturnsZero()
    {
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        epoch.ToUnixTimestamp().Should().Be(0);
    }

    [Fact]
    public void ToUnixTimestamp_RoundTripWithFromUnixTimestamp_ReproducesOriginal()
    {
        var original = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        var timestamp = original.ToUnixTimestamp();

        var restored = DateTimeExtensions.FromUnixTimestamp(timestamp);

        restored.Should().Be(original);
    }

    [Fact]
    public void IsBetween_WhenDateIsInsideRange_ReturnsTrue()
    {
        var date = new DateTime(2024, 6, 15);
        var start = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 12, 31);

        date.IsBetween(start, end).Should().BeTrue();
    }

    [Fact]
    public void IsBetween_WhenDateIsExactlyOnBoundary_ReturnsTrue()
    {
        var date = new DateTime(2024, 1, 1);

        date.IsBetween(new DateTime(2024, 1, 1), new DateTime(2024, 12, 31)).Should().BeTrue();
    }

    [Fact]
    public void IsBetween_WhenDateIsOutsideRange_ReturnsFalse()
    {
        var date = new DateTime(2025, 1, 1);
        var start = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 12, 31);

        date.IsBetween(start, end).Should().BeFalse();
    }

    [Fact]
    public void StartOfDay_ReturnsDateWithTimeSetToMidnight()
    {
        var dt = new DateTime(2024, 6, 15, 14, 30, 45);

        var start = dt.StartOfDay();

        start.Should().Be(new DateTime(2024, 6, 15, 0, 0, 0));
    }

    [Fact]
    public void EndOfDay_ReturnsLastTickOfDay()
    {
        var dt = new DateTime(2024, 6, 15, 0, 0, 0);

        var end = dt.EndOfDay();

        end.Hour.Should().Be(23);
        end.Minute.Should().Be(59);
        end.Second.Should().Be(59);
    }

    [Fact]
    public void StartOfMonth_ReturnsFirstDayAtMidnight()
    {
        var dt = new DateTime(2024, 6, 15, 10, 30, 0);

        var start = dt.StartOfMonth();

        start.Day.Should().Be(1);
        start.Hour.Should().Be(0);
    }

    [Fact]
    public void AddBusinessDays_FromFriday_SkipsWeekendToMonday()
    {
        var friday = new DateTime(2024, 6, 14);

        var result = friday.AddBusinessDays(1);

        result.DayOfWeek.Should().Be(DayOfWeek.Monday);
        result.Should().Be(new DateTime(2024, 6, 17));
    }

    [Fact]
    public void AddBusinessDays_AddingFiveDays_SkipsOneWeekend()
    {
        var monday = new DateTime(2024, 6, 10);

        var result = monday.AddBusinessDays(5);

        result.Should().Be(new DateTime(2024, 6, 17));
    }
}