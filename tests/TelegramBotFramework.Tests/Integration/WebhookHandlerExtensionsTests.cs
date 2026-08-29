using FluentAssertions;
using NSubstitute;
using TelegramBotFramework.Integration;
using TelegramBotFramework.Models;
using Xunit;

namespace TelegramBotFramework.Tests.Integration;

/// <summary>
/// Contains unit tests for the <see cref="WebhookHandler"/> extension methods.
/// </summary>
public class WebhookHandlerExtensionsTests : IWebhookHandlerExtensionsTests
{
    /// <summary>
    /// Verifies that <see cref="WebhookHandler.GetMessageText"/> returns <c>null</c>
    /// when the supplied <see cref="TelegramUpdate"/> instance itself is <c>null</c>.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="WebhookHandler.GetMessageText"/> throws an <see cref="ArgumentNullException"/>
    /// when the <see cref="TelegramUpdate"/> argument is <c>null</c>.
    /// </summary>
    [Fact]
    public void GetMessageText_UpdateIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var handler = new WebhookHandler();
        TelegramUpdate update = null;

        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => handler.GetMessageText(update));
    }

    /// <summary>
    /// Verifies that <see cref="WebhookHandler.HasCallbackData"/> returns <c>true</c>
    /// when the <see cref="TelegramUpdate.CallbackData"/> matches the expected value.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="WebhookHandler.HasCallbackData"/> returns <c>false</c>
    /// when the <see cref="TelegramUpdate.CallbackData"/> does not match the expected value.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="WebhookHandler.GetChatId"/> returns <c>0</c>
    /// when the <see cref="TelegramUpdate.Message"/> property is <c>null</c>.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="WebhookHandler.GetUserId"/> returns <c>0</c>
    /// when the <see cref="TelegramUpdate.Message"/> property is <c>null</c>.
    /// </summary>
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
