#nullable enable
using FluentAssertions;
using TelegramBotFramework.Utilities;
using Xunit;

namespace TelegramBotFramework.Tests;

public sealed class StringExtensionEdgeCaseTests
{
    [Fact]
    public void Truncate_NullInput_ReturnsEmptyOrNull()
    {
        string? input = null;
        var act = () => input!.Truncate(10);
        // Should handle null gracefully
        act.Should().NotThrow<NullReferenceException>();
    }

    [Fact]
    public void Truncate_EmptyInput_ReturnsEmpty()
    {
        "".Truncate(10).Should().BeEmpty();
    }

    [Fact]
    public void Truncate_ExactLength_ReturnsUnchanged()
    {
        "12345".Truncate(5).Should().Be("12345");
    }

    [Fact]
    public void Truncate_ZeroLength_DoesNotThrow()
    {
        var act = () => "test".Truncate(0);
        act.Should().NotThrow();
    }

    [Fact]
    public void Truncate_SingleChar_DoesNotThrow()
    {
        var act = () => "test".Truncate(1);
        act.Should().NotThrow();
    }

    [Fact]
    public void Truncate_LongInput_ResultNotLongerThanMax()
    {
        var result = "This is a very long string that needs truncation".Truncate(15);
        result.Length.Should().BeLessThanOrEqualTo(15);
    }
}
