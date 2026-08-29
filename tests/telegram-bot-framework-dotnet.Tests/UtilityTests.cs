#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using TelegramBotFramework.Utilities;
using Xunit;

namespace TelegramBotFramework.Tests;

/// <summary>
    /// Contains unit tests for string extension methods.
    /// </summary>
    public sealed class StringExtensionTests : IStringExtensionTests
    {
    /// <summary>
    /// Tests that the Truncate method correctly truncates strings to the specified maximum length.
    /// </summary>
    /// <param name="input">The input string to truncate.</param>
    /// <param name="maxLength">The maximum length to truncate to.</param>
    /// <param name="expected">The expected truncated result.</param>
    [Theory]
    [InlineData("Hello World", 5, "Hell…")]
    [InlineData("Hi", 10, "Hi")]
    [InlineData("Short", 5, "Short")]
    public void Truncate_VariousInputs_TruncatesCorrectly(string input, int maxLength, string expected)
    {
        input.Truncate(maxLength).Should().Be(expected);
    }

    /// <summary>
    /// Tests that the Truncate method returns null when given a null input string.
    /// </summary>
    [Fact]
    public void Truncate_NullInput_ReturnsNull()
    {
        string? value = null;
        value!.Truncate(10).Should().BeNull();
    }

    /// <summary>
    /// Tests that the IsValidEmail method returns true for a string with a valid email format.
    /// </summary>
    [Fact]
    public void IsValidEmail_WithValidFormat_ReturnsTrue()
    {
        "user@example.com".IsValidEmail().Should().BeTrue();
    }

    /// <summary>
    /// Tests that the IsValidEmail method returns false when the email is missing the @ sign.
    /// </summary>
    [Fact]
    public void IsValidEmail_WithMissingAtSign_ReturnsFalse()
    {
        "userexample.com".IsValidEmail().Should().BeFalse();
    }

    /// <summary>
    /// Tests that the IsValidEmail method returns false when given an empty string.
    /// </summary>
    [Fact]
    public void IsValidEmail_WithEmptyString_ReturnsFalse()
    {
        "".IsValidEmail().Should().BeFalse();
    }

    /// <summary>
    /// Tests that the IsValidEmail method returns false when the email is missing the domain part.
    /// </summary>
    [Fact]
    public void IsValidEmail_WithMissingDomain_ReturnsFalse()
    {
        "user@".IsValidEmail().Should().BeFalse();
    }

    /// <summary>
    /// Tests that the Repeat method produces the correct repeated string when given a positive count.
    /// </summary>
    [Fact]
    public void Repeat_PositiveCount_ProducesRepeatedString()
    {
        "ab".Repeat(3).Should().Be("ababab");
    }

    /// <summary>
    /// Tests that the Repeat method returns an empty string when given a zero count.
    /// </summary>
    [Fact]
    public void Repeat_ZeroCount_ReturnsEmpty()
    {
        "abc".Repeat(0).Should().BeEmpty();
    }

    /// <summary>
    /// Tests that the Repeat method returns an empty string when given a negative count.
    /// </summary>
    [Fact]
    public void Repeat_NegativeCount_ReturnsEmpty()
    {
        "abc".Repeat(-1).Should().BeEmpty();
    }

    /// <summary>
    /// Tests that the ExtractNumbers method returns only the digits from a mixed string.
    /// </summary>
    [Fact]
    public void ExtractNumbers_FromMixedString_ReturnsOnlyDigits()
    {
        "abc123def456".ExtractNumbers().Should().Be("123456");
    }

    /// <summary>
    /// Tests that the ExtractNumbers method returns an empty string when the input contains no digits.
    /// </summary>
    [Fact]
    public void ExtractNumbers_FromStringWithNoDigits_ReturnsEmpty()
    {
        "abcdef".ExtractNumbers().Should().BeEmpty();
    }

    /// <summary>
    /// Tests that the EnsureStartsWith method prepends the specified prefix when it's missing from the string.
    /// </summary>
    [Fact]
    public void EnsureStartsWith_WhenPrefixMissing_PrependPrefix()
    {
        "example.com".EnsureStartsWith("https://").Should().Be("https://example.com");
    }

    /// <summary>
    /// Tests that the EnsureStartsWith method returns the string unchanged when it already has the specified prefix.
    /// </summary>
    [Fact]
    public void EnsureStartsWith_WhenAlreadyHasPrefix_ReturnsUnchanged()
    {
        "https://example.com".EnsureStartsWith("https://").Should().Be("https://example.com");
    }

    /// <summary>
    /// Tests that the EnsureEndsWith method appends the specified suffix when it's missing from the string.
    /// </summary>
    [Fact]
    public void EnsureEndsWith_WhenSuffixMissing_AppendsSuffix()
    {
        "hello".EnsureEndsWith("!").Should().Be("hello!");
    }

    /// <summary>
    /// Tests that the EnsureEndsWith method returns the string unchanged when it already has the specified suffix.
    /// </summary>
    [Fact]
    public void EnsureEndsWith_WhenAlreadyHasSuffix_ReturnsUnchanged()
    {
        "hello!".EnsureEndsWith("!").Should().Be("hello!");
    }

    /// <summary>
    /// Tests that the Capitalize method capitalizes the first character of a string.
    /// </summary>
    [Fact]
    public void Capitalize_WithLowercaseFirstChar_CapitalizesFirstChar()
    {
        "hello world".Capitalize().Should().Be("Hello world");
    }

    /// <summary>
    /// Tests that the Capitalize method returns the string unchanged when the first character is already capitalized.
    /// </summary>
    [Fact]
    public void Capitalize_WithAlreadyCapitalized_ReturnsUnchanged()
    {
        "Hello".Capitalize().Should().Be("Hello");
    }

    /// <summary>
    /// Tests that the IsAlphanumeric method returns true for a string containing only alphanumeric characters.
    /// </summary>
    [Fact]
    public void IsAlphanumeric_WithPureAlphanumericString_ReturnsTrue()
    {
        "abc123".IsAlphanumeric().Should().BeTrue();
    }

    /// <summary>
    /// Tests that the IsAlphanumeric method returns false when the string contains special characters.
    /// </summary>
    [Fact]
    public void IsAlphanumeric_WithSpecialCharacters_ReturnsFalse()
    {
        "abc!123".IsAlphanumeric().Should().BeFalse();
    }

    /// <summary>
    /// Tests that the IsAlphanumeric method returns false when the string contains spaces.
    /// </summary>
    [Fact]
    public void IsAlphanumeric_WithSpaces_ReturnsFalse()
    {
        "abc 123".IsAlphanumeric().Should().BeFalse();
    }

    /// <summary>
    /// Tests that the Reverse method returns the same string when given a palindrome.
    /// </summary>
    [Fact]
    public void Reverse_OfPalindrome_ReturnsSameString()
    {
        "racecar".Reverse().Should().Be("racecar");
    }

    /// <summary>
    /// Tests that the Reverse method returns the reversed string when given an asymmetric string.
    /// </summary>
    [Fact]
    public void Reverse_OfAsymmetricString_ReturnsReversed()
    {
        "hello".Reverse().Should().Be("olleh");
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