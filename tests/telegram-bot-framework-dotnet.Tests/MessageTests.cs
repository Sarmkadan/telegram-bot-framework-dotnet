#nullable enable
using System;
using System.Collections.Generic;
using TelegramBotFramework.Models;
using Xunit;

namespace TelegramBotFramework.Tests;

public class MessageTests
{
    [Fact]
    public void Validate_WithValidData_ReturnsTrue()
    {
        // Arrange
        var msg = new Message
        {
            UserId = 123,
            ChatId = 456,
            Content = "Hello world"
        };

        // Act
        var result = msg.Validate();

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Validate_WithInvalidUserId_ThrowsInvalidOperationException(long invalidUserId)
    {
        // Arrange
        var msg = new Message
        {
            UserId = invalidUserId,
            ChatId = 456,
            Content = "test"
        };

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => msg.Validate());
        Assert.Equal("UserId must be positive", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Validate_WithInvalidChatId_ThrowsInvalidOperationException(long invalidChatId)
    {
        // Arrange
        var msg = new Message
        {
            UserId = 123,
            ChatId = invalidChatId,
            Content = "test"
        };

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => msg.Validate());
        Assert.Equal("ChatId must be positive", ex.Message);
    }

    [Fact]
    public void Validate_WithEmptyContent_ThrowsInvalidOperationException()
    {
        // Arrange
        var msg = new Message
        {
            UserId = 123,
            ChatId = 456,
            Content = "   " // whitespace only
        };

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => msg.Validate());
        Assert.Equal("Message content cannot be empty", ex.Message);
    }

    [Fact]
    public void MarkAsProcessed_SetsProcessedAtAndStatus()
    {
        // Arrange
        var msg = new Message();

        // Act
        msg.MarkAsProcessed();

        // Assert
        Assert.NotNull(msg.ProcessedAt);
        Assert.Equal(MessageStatus.Processed, msg.Status);
        // ProcessedAt should be after CreatedAt (allowing a small tolerance)
        Assert.True(msg.ProcessedAt!.Value >= msg.CreatedAt);
    }

    [Fact]
    public void MarkAsFailed_SetsStatusAndErrorMetadata()
    {
        // Arrange
        var msg = new Message();

        // Act
        const string error = "something went wrong";
        msg.MarkAsFailed(error);

        // Assert
        Assert.Equal(MessageStatus.Failed, msg.Status);
        Assert.NotNull(msg.Metadata);
        Assert.True(msg.Metadata!.ContainsKey("error"));
        Assert.Equal(error, msg.Metadata["error"]);
    }

    [Fact]
    public void AddAttachment_InitializesListAndAddsUrl()
    {
        // Arrange
        var msg = new Message();
        const string url = "https://example.com/image.png";

        // Act
        msg.AddAttachment(url);

        // Assert
        Assert.NotNull(msg.AttachmentUrls);
        Assert.Single(msg.AttachmentUrls);
        Assert.Equal(url, msg.AttachmentUrls![0]);
    }

    [Fact]
    public void SetMetadata_InitializesDictionaryAndStoresValue()
    {
        // Arrange
        var msg = new Message();

        // Act
        msg.SetMetadata("key", 42);

        // Assert
        Assert.NotNull(msg.Metadata);
        Assert.True(msg.Metadata!.ContainsKey("key"));
        Assert.Equal(42, msg.Metadata["key"]);
    }

    [Fact]
    public void GetProcessingDurationMs_BeforeProcessing_ReturnsMinusOne()
    {
        // Arrange
        var msg = new Message();

        // Act
        var duration = msg.GetProcessingDurationMs();

        // Assert
        Assert.Equal(-1, duration);
    }

    [Fact]
    public void GetProcessingDurationMs_AfterProcessing_ReturnsPositiveValue()
    {
        // Arrange
        var msg = new Message();
        msg.MarkAsProcessed();

        // Act
        var duration = msg.GetProcessingDurationMs();

        // Assert
        Assert.True(duration >= 0);
    }
}
