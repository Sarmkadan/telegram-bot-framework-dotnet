#nullable enable
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TelegramBotFramework.Integration;
using TelegramBotFramework.Models;
using TelegramBotFramework.Repositories;
using TelegramBotFramework.Services;
using Xunit;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Tests for <see cref="MessageService"/> class, specifically for media group functionality.
/// </summary>
public sealed class MessageServiceMediaGroupTests
{
    private readonly Mock<IMessageRepository> _mockMessageRepository = new();
    private readonly Mock<ITelegramApiClient> _mockTelegramApiClient = new();
    private readonly Mock<ILogger<MessageService>> _mockLogger = new();
    private readonly MessageService _messageService;

    public MessageServiceMediaGroupTests()
    {
        _messageService = new MessageService(
            _mockMessageRepository.Object,
            _mockTelegramApiClient.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task SendMediaGroupAsync_WithValidInput_ReturnsMessages()
    {
        // Arrange
        const long chatId = 123456789;
        var items = new List<MediaGroupItem>
        {
            new(MediaType.Photo, "photo123", "First photo"),
            new(MediaType.Photo, "photo456", "Second photo")
        };

        var messageIds = new List<int> { 100, 101 };
        _mockTelegramApiClient
            .Setup(x => x.SendMediaGroupAsync(chatId, items, It.IsAny<CancellationToken>()))
            .ReturnsAsync(messageIds);

        var createdMessage1 = new Message { MessageId = 100, ChatId = chatId, Content = "Album caption", Type = MessageType.Photo, Metadata = new Dictionary<string, object>() };
        createdMessage1.Metadata["media_type"] = "photo";
        createdMessage1.Metadata["file_id_or_url"] = "photo123";
        createdMessage1.Metadata["message_id"] = 100;
        createdMessage1.Metadata["position"] = 0;
        createdMessage1.Metadata["caption"] = "Album caption";

        var createdMessage2 = new Message { MessageId = 101, ChatId = chatId, Content = "Album caption", Type = MessageType.Photo, Metadata = new Dictionary<string, object>() };
        createdMessage2.Metadata["media_type"] = "photo";
        createdMessage2.Metadata["file_id_or_url"] = "photo456";
        createdMessage2.Metadata["message_id"] = 101;
        createdMessage2.Metadata["position"] = 1;
        createdMessage2.Metadata["caption"] = "Album caption";

        _mockMessageRepository
            .SetupSequence(x => x.CreateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdMessage1)
            .ReturnsAsync(createdMessage2);

        // Act
        var result = await _messageService.SendMediaGroupAsync(chatId, items, "Album caption");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result![0].Type.Should().Be(MessageType.Photo);
        result[0].Metadata.Should().NotBeNull();
        result[0].Metadata.Should().ContainKey("media_type");
        result[0].Metadata.Should().ContainKey("file_id_or_url");
        result[0].Metadata.Should().ContainKey("caption");

        _mockTelegramApiClient.Verify(x => x.SendMediaGroupAsync(chatId, items, It.IsAny<CancellationToken>()), Times.Once);
        _mockMessageRepository.Verify(x => x.CreateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task SendMediaGroupAsync_WithTooFewItems_ThrowsArgumentException()
    {
        // Arrange
        const long chatId = 123456789;
        var items = new List<MediaGroupItem> { new(MediaType.Photo, "photo123") };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _messageService.SendMediaGroupAsync(chatId, items));
    }

    [Fact]
    public async Task SendMediaGroupAsync_WithTooManyItems_ThrowsArgumentException()
    {
        // Arrange
        const long chatId = 123456789;
        var items = new List<MediaGroupItem>();
        for (int i = 0; i < 11; i++)
        {
            items.Add(new MediaGroupItem(MediaType.Photo, $"photo{i}"));
        }

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _messageService.SendMediaGroupAsync(chatId, items));
    }

    [Fact]
    public async Task SendMediaGroupAsync_WithNullItem_ThrowsArgumentException()
    {
        // Arrange
        const long chatId = 123456789;
        var items = new List<MediaGroupItem> { new(MediaType.Photo, "photo123"), null! };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _messageService.SendMediaGroupAsync(chatId, items));
    }

    [Fact]
    public async Task SendMediaGroupAsync_WithEmptyFileIdOrUrl_ThrowsArgumentException()
    {
        // Arrange
        const long chatId = 123456789;
        var items = new List<MediaGroupItem> { new(MediaType.Photo, ""), new(MediaType.Photo, "photo123") };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _messageService.SendMediaGroupAsync(chatId, items));
    }

    [Fact]
    public async Task SendMediaGroupAsync_WithMixedMediaTypes_ReturnsMessages()
    {
        // Arrange
        const long chatId = 123456789;
        var items = new List<MediaGroupItem>
        {
            new(MediaType.Photo, "photo123"),
            new(MediaType.Video, "video456"),
            new(MediaType.Document, "doc789")
        };

        var messageIds = new List<int> { 200, 201, 202 };
        _mockTelegramApiClient
            .Setup(x => x.SendMediaGroupAsync(chatId, items, It.IsAny<CancellationToken>()))
            .ReturnsAsync(messageIds);

        var createdMessage1 = new Message { MessageId = 200, ChatId = chatId, Content = "", Type = MessageType.Photo };
        var createdMessage2 = new Message { MessageId = 201, ChatId = chatId, Content = "", Type = MessageType.Video };
        var createdMessage3 = new Message { MessageId = 202, ChatId = chatId, Content = "", Type = MessageType.Document };

        _mockMessageRepository
            .SetupSequence(x => x.CreateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdMessage1)
            .ReturnsAsync(createdMessage2)
            .ReturnsAsync(createdMessage3);

        // Act
        var result = await _messageService.SendMediaGroupAsync(chatId, items);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result![0].Type.Should().Be(MessageType.Photo);
        result[1].Type.Should().Be(MessageType.Video);
        result[2].Type.Should().Be(MessageType.Document);
    }

    [Fact]
    public async Task SendMediaGroupAsync_WhenApiFails_ReturnsNull()
    {
        // Arrange
        const long chatId = 123456789;
        var items = new List<MediaGroupItem>
        {
            new(MediaType.Photo, "photo123"),
            new(MediaType.Photo, "photo456")
        };

        _mockTelegramApiClient
            .Setup(x => x.SendMediaGroupAsync(chatId, items, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int>());

        // Act
        var result = await _messageService.SendMediaGroupAsync(chatId, items);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SendMediaGroupAsync_WithNegativeChatId_ThrowsArgumentException()
    {
        // Arrange
        const long chatId = -1;
        var items = new List<MediaGroupItem> { new(MediaType.Photo, "photo123"), new(MediaType.Photo, "photo456") };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _messageService.SendMediaGroupAsync(chatId, items));
    }
}
