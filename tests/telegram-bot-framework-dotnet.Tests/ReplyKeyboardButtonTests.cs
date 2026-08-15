using TelegramBotFramework.Keyboard;
using Xunit;

namespace TelegramBotFramework.Tests;

public sealed class ReplyKeyboardButtonTests
{
    [Fact]
    public void DefaultValues_ShouldBeEmptyOrFalse()
    {
        var button = new ReplyKeyboardButton();

        Assert.Equal(string.Empty, button.Text);
        Assert.False(button.RequestContact);
        Assert.False(button.RequestLocation);
    }

    [Fact]
    public void TextProperty_ShouldAcceptNonEmptyString()
    {
        var button = new ReplyKeyboardButton { Text = "Hello" };

        Assert.Equal("Hello", button.Text);
    }

    [Fact]
    public void TextProperty_ShouldAcceptWhitespace()
    {
        var button = new ReplyKeyboardButton { Text = "   " };

        Assert.Equal("   ", button.Text);
    }

    [Fact]
    public void RequestContactAndRequestLocation_ShouldBeSettable()
    {
        var button = new ReplyKeyboardButton
        {
            RequestContact = true,
            RequestLocation = true
        };

        Assert.True(button.RequestContact);
        Assert.True(button.RequestLocation);
    }

    [Fact]
    public void MixedBooleanSettings_ShouldReflectCorrectly()
    {
        var button = new ReplyKeyboardButton
        {
            RequestContact = true,
            RequestLocation = false
        };

        Assert.True(button.RequestContact);
        Assert.False(button.RequestLocation);
    }

    [Fact]
    public void ObjectInitializer_WithOnlyText_ShouldSetTextAndLeaveBooleansFalse()
    {
        var button = new ReplyKeyboardButton { Text = "OnlyText" };

        Assert.Equal("OnlyText", button.Text);
        Assert.False(button.RequestContact);
        Assert.False(button.RequestLocation);
    }
}
