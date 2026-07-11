using FluentAssertions;
using NSubstitute;
using TelegramBotFramework.Integration;
using TelegramBotFramework.Models;
using Xunit;

namespace TelegramBotFramework.Tests.Integration;

public class WebhookHandlerExtensionsTests
{
    [Fact]
    public void GetMessageText_MessageIsNull_ReturnsNull()
    {
        // Arrange
        var handler = new WebhookHandler();
        var update = (TelegramUpdate)null;

        // Act
        var result = handler.GetMessageText(update);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetMessageText_UpdateIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var handler = new WebhookHandler();
        TelegramUpdate update = null;

        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => handler.GetMessageText(update));
    }

    [Fact]
    public void HasCallbackData_CallbackDataMatches_ReturnsTrue()
    {
        // Arrange
        var handler = new WebhookHandler();
        var update = new TelegramUpdate { CallbackData = "test" };

        // Act
        var result = handler.HasCallbackData(update, "test");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasCallbackData_CallbackDataDoesNotMatch_ReturnsFalse()
    {
        // Arrange
        var handler = new WebhookHandler();
        var update = new TelegramUpdate { CallbackData = "test" };

        // Act
        var result = handler.HasCallbackData(update, "different");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetChatId_MessageIsNull_Returns0()
    {
        // Arrange
        var handler = new WebhookHandler();
        var update = new TelegramUpdate { Message = null };

        // Act
        var result = handler.GetChatId(update);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetUserId_MessageIsNull_Returns0()
    {
        // Arrange
        var handler = new WebhookHandler();
        var update = new TelegramUpdate { Message = null };

        // Act
        var result = handler.GetUserId(update);

        // Assert
        result.Should().Be(0);
    }
}
