using FluentAssertions;
using TelegramBotFramework.Models;
using Xunit;

namespace TelegramBotFramework.Tests.Models;

public class MessageExtensionsTests
{
    [Fact]
    public void IsCommand_MessageIsCommand_ReturnsTrue()
    {
        // Arrange
        var message = new Message { CommandName = "/test" };

        // Act
        var result = message.IsCommand();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsCommand_MessageIsNotCommand_ReturnsFalse()
    {
        // Arrange
        var message = new Message { CommandName = "test" };

        // Act
        var result = message.IsCommand();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasAttachments_MessageHasAttachments_ReturnsTrue()
    {
        // Arrange
        var message = new Message { AttachmentUrls = new[] { "test" } };

        // Act
        var result = message.HasAttachments();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetTypeString_MessageHasType_ReturnsTypeString()
    {
        // Arrange
        var message = new Message { Type = MessageType.Text };

        // Act
        var result = message.GetTypeString();

        // Assert
        result.Should().Be("text");
    }

    [Fact]
    public void IsReply_MessageIsReply_ReturnsTrue()
    {
        // Arrange
        var message = new Message { ReplyToMessageId = 1 };

        // Act
        var result = message.IsReply();

        // Assert
        result.Should().BeTrue();
    }
}
