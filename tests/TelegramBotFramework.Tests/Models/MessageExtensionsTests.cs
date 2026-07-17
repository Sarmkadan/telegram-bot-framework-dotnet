using FluentAssertions;
using TelegramBotFramework.Models;
using Xunit;

namespace TelegramBotFramework.Tests.Models;

/// <summary>
/// Provides unit tests for message extension methods that test various message operations and behaviors.
/// </summary>
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

    /// <summary>
    /// Tests that <see cref="Message.IsCommand()"/> returns true when the message has a command name starting with '/'.
    /// </summary>
    /// <remarks>
    /// This test verifies that messages with command names (e.g., "/start", "/help") are correctly identified as commands.
    /// </remarks>

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

    /// <summary>
    /// Tests that <see cref="Message.IsCommand()"/> returns false when the message has a command name without a leading '/'.
    /// </summary>
    /// <remarks>
    /// This test verifies that messages without the '/' prefix in their command name are not identified as commands.
    /// </remarks>

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

    /// <summary>
    /// Tests that <see cref="Message.HasAttachments()"/> returns true when the message has one or more attachment URLs.
    /// </summary>
    /// <remarks>
    /// This test verifies that messages containing attachment URLs (photos, documents, etc.) are correctly identified as having attachments.
    /// </remarks>

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

    /// <summary>
    /// Tests that <see cref="Message.GetTypeString()"/> returns the correct string representation of the message type.
    /// </summary>
    /// <remarks>
    /// This test verifies that the extension method correctly converts the <see cref="MessageType"/> enum value to its corresponding string representation.
    /// </remarks>

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

    /// <summary>
    /// Tests that <see cref="Message.IsReply()"/> returns true when the message has a ReplyToMessageId set.
    /// </summary>
    /// <remarks>
    /// This test verifies that messages that are replies to other messages (indicated by having a ReplyToMessageId value) are correctly identified as replies.
    /// </remarks>
}
