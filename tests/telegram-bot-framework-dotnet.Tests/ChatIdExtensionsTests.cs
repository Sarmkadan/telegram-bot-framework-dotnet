using System;
using FluentAssertions;
using TelegramBotFramework.Extensions;
using Xunit;

namespace TelegramBotFramework.Tests.Extensions;

public class ChatIdExtensionsTests
{
    [Theory]
    [InlineData(-123456, true)]   // basic group (negative, not starting with -100)
    [InlineData(-100123456, false)] // supergroup/channel (starts with -100)
    [InlineData(-100, false)]     // edge case: exactly -100 (still starts with -100)
    [InlineData(123456, false)]   // private chat (positive)
    [InlineData(0, false)]        // zero is not a valid chat ID, but treat as not group
    public void IsGroup_ReturnsExpectedResult(long chatId, bool expected)
    {
        // Act
        bool result = chatId.IsGroup();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(-123456, false)]  // basic group (negative, not starting with -100)
    [InlineData(-100123456, true)] // supergroup/channel (starts with -100)
    [InlineData(-100, true)]      // edge case: exactly -100 (starts with -100)
    [InlineData(123456, false)]   // private chat (positive)
    [InlineData(0, false)]        // zero is not a valid chat ID, but treat as not channel
    public void IsChannel_ReturnsExpectedResult(long chatId, bool expected)
    {
        // Act
        bool result = chatId.IsChannel();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(-123456, false)]  // basic group (negative)
    [InlineData(-100123456, false)] // supergroup/channel (negative)
    [InlineData(-100, false)]     // edge case: exactly -100 (negative)
    [InlineData(123456, true)]    // private chat (positive)
    [InlineData(0, false)]        // zero is not a valid chat ID, but treat as not private
    public void IsPrivate_ReturnsExpectedResult(long chatId, bool expected)
    {
        // Act
        bool result = chatId.IsPrivate();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(-123456, "-123456")]
    [InlineData(-100123456, "-100123456")]
    [InlineData(0, "0")]
    [InlineData(123456, "123456")]
    public void ToTelegramString_ReturnsStringRepresentation(long chatId, string expected)
    {
        // Act
        string result = chatId.ToTelegramString();

        // Assert
        result.Should().Be(expected);
    }
}