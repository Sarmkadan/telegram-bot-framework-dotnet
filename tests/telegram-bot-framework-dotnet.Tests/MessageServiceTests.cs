#nullable enable
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TelegramBotFramework.Integration;
using TelegramBotFramework.Models;
using TelegramBotFramework.Repositories;
using TelegramBotFramework.Services;
using Xunit;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Tests for <see cref="MessageService"/> class.
/// </summary>
public sealed class MessageServiceTests : IMessageServiceTests
{
    private readonly Mock<IMessageRepository> _mockMessageRepository = new();
    private readonly Mock<ITelegramApiClient> _mockTelegramApiClient = new();
    private readonly Mock<ILogger<MessageService>> _mockLogger = new();
    private readonly MessageService _messageService;

    public MessageServiceTests()
    {
        _messageService = new MessageService(
            _mockMessageRepository.Object,
            _mockTelegramApiClient.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ProcessIncomingMessageAsync_WithValidMessage_ReturnsCreatedMessage()
    {
        // Arrange
        var message = new Message
        {
            UserId = 12345,
            ChatId = 67890,
            Content = "Hello world",
            Type = MessageType.Text
        };

        var createdMessage = new Message
        {
            MessageId = 1,
            UserId = 12345,
            ChatId = 67890,
            Content = "Hello world",
            Type = MessageType.Text,
            Status = MessageStatus.Processing
        };

        _mockMessageRepository
            .Setup(x => x.CreateAsync(message, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdMessage);

        // Act
        var result = await _messageService.ProcessIncomingMessageAsync(message);

        // Assert
        result.Should().NotBeNull();
        result.MessageId.Should().Be(1);
        result.Status.Should().Be(MessageStatus.Processing);
        _mockMessageRepository.Verify(x => x.CreateAsync(message, It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Message received from user") && t != null),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessIncomingMessageAsync_WithInvalidMessage_ThrowsInvalidOperationException()
    {
        // Arrange
        var message = new Message
        {
            UserId = 0, // Invalid UserId
            ChatId = 67890,
            Content = "Hello world"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _messageService.ProcessIncomingMessageAsync(message));
    }

    [Fact]
    public async Task GetMessageAsync_WithExistingMessageId_ReturnsMessage()
    {
        // Arrange
        const long messageId = 123;
        var expectedMessage = new Message { MessageId = messageId, Content = "Test message" };

        _mockMessageRepository
            .Setup(x => x.GetByIdAsync(messageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedMessage);

        // Act
        var result = await _messageService.GetMessageAsync(messageId);

        // Assert
        result.Should().NotBeNull();
        result!.MessageId.Should().Be(messageId);
        result.Content.Should().Be("Test message");
    }

    [Fact]
    public async Task GetMessageAsync_WithNonExistingMessageId_ReturnsNull()
    {
        // Arrange
        const long messageId = 999;

        _mockMessageRepository
            .Setup(x => x.GetByIdAsync(messageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Message?)null);

        // Act
        var result = await _messageService.GetMessageAsync(messageId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserMessagesAsync_WithValidUserId_ReturnsMessages()
    {
        // Arrange
        const long userId = 12345;
        var messages = new List<Message>
        {
            new Message { MessageId = 1, UserId = userId, Content = "First message", CreatedAt = DateTime.UtcNow.AddMinutes(-10) },
            new Message { MessageId = 2, UserId = userId, Content = "Second message", CreatedAt = DateTime.UtcNow.AddMinutes(-5) },
            new Message { MessageId = 3, UserId = userId, Content = "Third message", CreatedAt = DateTime.UtcNow }
        };

        _mockMessageRepository
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(messages);

        // Act
        var result = await _messageService.GetUserMessagesAsync(userId, 2);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Content.Should().Be("Third message"); // Most recent first
        result[1].Content.Should().Be("Second message");
    }

    [Fact]
    public async Task GetFailedMessagesAsync_WithFailedMessages_ReturnsFailedMessages()
    {
        // Arrange
        var failedMessages = new List<Message>
        {
            new Message { MessageId = 1, Content = "Failed message 1", Status = MessageStatus.Failed },
            new Message { MessageId = 2, Content = "Failed message 2", Status = MessageStatus.Failed }
        };

        _mockMessageRepository
            .Setup(x => x.GetByStatusAsync(MessageStatus.Failed, It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedMessages);

        // Act
        var result = await _messageService.GetFailedMessagesAsync(5);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.All(m => m.Status == MessageStatus.Failed).Should().BeTrue();
    }

    [Fact]
    public async Task MarkAsProcessedAsync_WithExistingMessageId_ReturnsTrue()
    {
        // Arrange
        const long messageId = 123;
        var message = new Message { MessageId = messageId, Content = "Test message" };

        _mockMessageRepository
            .Setup(x => x.GetByIdAsync(messageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(message);

        _mockMessageRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Message m, CancellationToken _) => m);

        // Act
        var result = await _messageService.MarkAsProcessedAsync(messageId);

        // Assert
        result.Should().BeTrue();
        message.Status.Should().Be(MessageStatus.Processed);
        message.ProcessedAt.Should().NotBeNull();
        _mockMessageRepository.Verify(x => x.UpdateAsync(message, It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Message marked as processed") && t != null),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task MarkAsProcessedAsync_WithNonExistingMessageId_ReturnsFalse()
    {
        // Arrange
        const long messageId = 999;

        _mockMessageRepository
            .Setup(x => x.GetByIdAsync(messageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Message?)null);

        // Act
        var result = await _messageService.MarkAsProcessedAsync(messageId).ConfigureAwait(false);

        // Assert
        result.Should().BeFalse();
        _mockMessageRepository.Verify(x => x.UpdateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MarkAsFailedAsync_WithExistingMessageId_ReturnsTrue()
    {
        // Arrange
        const long messageId = 123;
        var message = new Message { MessageId = messageId, Content = "Test message" };
        const string errorMessage = "Something went wrong";

        _mockMessageRepository
            .Setup(x => x.GetByIdAsync(messageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(message);

        _mockMessageRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Message m, CancellationToken _) => m);

        // Act
        var result = await _messageService.MarkAsFailedAsync(messageId, errorMessage);

        // Assert
        result.Should().BeTrue();
        message.Status.Should().Be(MessageStatus.Failed);
        message.GetMetadata("error").Should().Be(errorMessage);
        _mockMessageRepository.Verify(x => x.UpdateAsync(message, It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Message marked as failed") && t != null),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task MarkAsFailedAsync_WithNonExistingMessageId_ReturnsFalse()
    {
        // Arrange
        const long messageId = 999;
        const string errorMessage = "Something went wrong";

        _mockMessageRepository
            .Setup(x => x.GetByIdAsync(messageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Message?)null);

        // Act
        var result = await _messageService.MarkAsFailedAsync(messageId, errorMessage);

        // Assert
        result.Should().BeFalse();
        _mockMessageRepository.Verify(x => x.UpdateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetUnprocessedMessageCountAsync_WithProcessingAndReceivedMessages_ReturnsCount()
    {
        // Arrange
        var processingMessages = new List<Message>
        {
            new Message { MessageId = 1, Status = MessageStatus.Processing },
            new Message { MessageId = 2, Status = MessageStatus.Processing }
        };

        var receivedMessages = new List<Message>
        {
            new Message { MessageId = 3, Status = MessageStatus.Received },
            new Message { MessageId = 4, Status = MessageStatus.Received },
            new Message { MessageId = 5, Status = MessageStatus.Received }
        };

        _mockMessageRepository
            .Setup(x => x.GetByStatusAsync(MessageStatus.Processing, It.IsAny<CancellationToken>()))
            .ReturnsAsync(processingMessages);

        _mockMessageRepository
            .Setup(x => x.GetByStatusAsync(MessageStatus.Received, It.IsAny<CancellationToken>()))
            .ReturnsAsync(receivedMessages);

        // Act
        var result = await _messageService.GetUnprocessedMessageCountAsync();

        // Assert
        result.Should().Be(5); // 2 processing + 3 received
    }

    [Fact]
    public async Task ArchiveOldMessagesAsync_WithOldMessages_ArchivesThem()
    {
        // Arrange
        const int daysOld = 30;
        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
        var oldMessage = new Message
        {
            MessageId = 1,
            Content = "Old message",
            CreatedAt = cutoffDate.AddDays(-1), // Older than cutoff
            Status = MessageStatus.Processed
        };

        var recentMessage = new Message
        {
            MessageId = 2,
            Content = "Recent message",
            CreatedAt = DateTime.UtcNow.AddHours(-1), // Newer than cutoff
            Status = MessageStatus.Processed
        };

        var allMessages = new List<Message> { oldMessage, recentMessage };

        _mockMessageRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(allMessages);

        _mockMessageRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Message m, CancellationToken _) => m);

        // Act
        await _messageService.ArchiveOldMessagesAsync(daysOld);

        // Assert
        oldMessage.Status.Should().Be(MessageStatus.Archived);
        recentMessage.Status.Should().Be(MessageStatus.Processed); // Should remain unchanged
        _mockMessageRepository.Verify(x => x.UpdateAsync(oldMessage, It.IsAny<CancellationToken>()), Times.Once);
        _mockMessageRepository.Verify(x => x.UpdateAsync(recentMessage, It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Archived") && t != null),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendPollAsync_WithValidInput_ReturnsCreatedMessage()
    {
        // Arrange
        const long chatId = 123456789;
        const string question = "What is your favorite color?";
        var options = new[] { "Red", "Green", "Blue" };
        const bool allowsMultipleAnswers = false;

        _mockTelegramApiClient
            .Setup(x => x.SendPollAsync(chatId, question, options, allowsMultipleAnswers))
            .ReturnsAsync(42); // Returns messageId from Telegram API

        var createdMessage = new Message
        {
            MessageId = 100,
            ChatId = chatId,
            Content = question,
            Type = MessageType.Poll,
            Status = MessageStatus.Processed
        };
        createdMessage.SetMetadata("poll_type", "quiz");
        createdMessage.SetMetadata("options", options);
        createdMessage.SetMetadata("allows_multiple_answers", allowsMultipleAnswers);
        createdMessage.SetMetadata("message_id", 42);

        _mockMessageRepository
            .Setup(x => x.CreateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdMessage);

        // Act
        var result = await _messageService.SendPollAsync(chatId, question, options, allowsMultipleAnswers);

        // Assert
        result.Should().NotBeNull();
        result!.MessageId.Should().Be(100);
        result.Type.Should().Be(MessageType.Poll);
        result.Content.Should().Be(question);
        result.Metadata.Should().ContainKey("poll_type");
        result.Metadata.Should().ContainKey("options");
        result.Metadata.Should().ContainKey("allows_multiple_answers");
        result.Metadata.Should().ContainKey("message_id");

        _mockTelegramApiClient.Verify(x => x.SendPollAsync(chatId, question, options, allowsMultipleAnswers), Times.Once);
        _mockMessageRepository.Verify(x => x.CreateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Poll sent to chat") && t != null),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendPollAsync_WithInvalidChatId_ThrowsArgumentException()
    {
        // Arrange
        const long chatId = 0; // Invalid chat ID
        const string question = "Test question";
        var options = new[] { "Option 1", "Option 2" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _messageService.SendPollAsync(chatId, question, options));
    }

    [Fact]
    public async Task SendPollAsync_WithEmptyQuestion_ThrowsArgumentException()
    {
        // Arrange
        const long chatId = 123456789;
        const string question = ""; // Empty question
        var options = new[] { "Option 1", "Option 2" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _messageService.SendPollAsync(chatId, question, options));
    }

    [Fact]
    public async Task SendPollAsync_WithTooManyOptions_ThrowsArgumentException()
    {
        // Arrange
        const long chatId = 123456789;
        const string question = "Test question";
        var options = new string[11]; // More than 10 options
        for (int i = 0; i < 11; i++)
        {
            options[i] = $"Option {i + 1}";
        }

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _messageService.SendPollAsync(chatId, question, options));
    }

    [Fact]
    public async Task SendPollAsync_WhenTelegramApiFails_ReturnsNull()
    {
        // Arrange
        const long chatId = 123456789;
        const string question = "Test question";
        var options = new[] { "Option 1", "Option 2" };

        _mockTelegramApiClient
            .Setup(x => x.SendPollAsync(chatId, question, options, false))
            .ReturnsAsync((int?)null); // Simulates API failure

        // Act
        var result = await _messageService.SendPollAsync(chatId, question, options);

        // Assert
        result.Should().BeNull();
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to send poll to chat") && t != null),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendPollAsync_WhenTelegramApiThrowsException_ReturnsNull()
    {
        // Arrange
        const long chatId = 123456789;
        const string question = "Test question";
        var options = new[] { "Option 1", "Option 2" };

        _mockTelegramApiClient
            .Setup(x => x.SendPollAsync(chatId, question, options, false))
            .ThrowsAsync(new Exception("API error"));

        // Act
        var result = await _messageService.SendPollAsync(chatId, question, options);

        // Assert
        result.Should().BeNull();
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error sending poll to chat") && t != null),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendMediaGroupAsync_WithValidInput_ReturnsCreatedMessages()
    {
        // Arrange
        const long chatId = 123456789;
        var items = new List<MediaGroupItem>
        {
            new MediaGroupItem(MediaType.Photo, "photo1.jpg", "First photo"),
            new MediaGroupItem(MediaType.Photo, "photo2.jpg", "Second photo")
        };
        const string caption = "Photo album";

        var messageIds = new List<int> { 100, 101 };

        _mockTelegramApiClient
            .Setup(x => x.SendMediaGroupAsync(chatId, items, It.IsAny<CancellationToken>()))
            .ReturnsAsync(messageIds);

        var createdMessage1 = new Message
        {
            MessageId = 100,
            ChatId = chatId,
            Content = caption,
            Type = MessageType.Photo,
            Status = MessageStatus.Processed
        };
        createdMessage1.SetMetadata("media_type", "photo");
        createdMessage1.SetMetadata("file_id_or_url", "photo1.jpg");
        createdMessage1.SetMetadata("message_id", 100);
        createdMessage1.SetMetadata("position", 0);
        createdMessage1.SetMetadata("caption", caption);

        var createdMessage2 = new Message
        {
            MessageId = 101,
            ChatId = chatId,
            Content = caption,
            Type = MessageType.Photo,
            Status = MessageStatus.Processed
        };
        createdMessage2.SetMetadata("media_type", "photo");
        createdMessage2.SetMetadata("file_id_or_url", "photo2.jpg");
        createdMessage2.SetMetadata("message_id", 101);
        createdMessage2.SetMetadata("position", 1);
        createdMessage2.SetMetadata("caption", caption);

        _mockMessageRepository
            .SetupSequence(x => x.CreateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdMessage1)
            .ReturnsAsync(createdMessage2);

        // Act
        var result = await _messageService.SendMediaGroupAsync(chatId, items, caption);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result![0].MessageId.Should().Be(100);
        result[1].MessageId.Should().Be(101);
        result[0].Type.Should().Be(MessageType.Photo);
        result[1].Type.Should().Be(MessageType.Photo);

        _mockTelegramApiClient.Verify(x => x.SendMediaGroupAsync(chatId, items, It.IsAny<CancellationToken>()), Times.Once);
        _mockMessageRepository.Verify(x => x.CreateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Media group sent to chat") && t != null),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendMediaGroupAsync_WithInvalidChatId_ThrowsArgumentException()
    {
        // Arrange
        const long chatId = 0; // Invalid chat ID
        var items = new List<MediaGroupItem>
        {
            new MediaGroupItem(MediaType.Photo, "photo1.jpg")
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _messageService.SendMediaGroupAsync(chatId, items));
    }

    [Fact]
    public async Task SendMediaGroupAsync_WithTooFewItems_ThrowsArgumentException()
    {
        // Arrange
        const long chatId = 123456789;
        var items = new List<MediaGroupItem>(); // Empty list

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
            items.Add(new MediaGroupItem(MediaType.Photo, $"photo{i}.jpg"));
        }

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _messageService.SendMediaGroupAsync(chatId, items));
    }

    [Fact]
    public async Task SendMediaGroupAsync_WhenTelegramApiFails_ReturnsNull()
    {
        // Arrange
        const long chatId = 123456789;
        var items = new List<MediaGroupItem>
        {
            new MediaGroupItem(MediaType.Photo, "photo1.jpg"),
            new MediaGroupItem(MediaType.Photo, "photo2.jpg")
        };

        _mockTelegramApiClient
            .Setup(x => x.SendMediaGroupAsync(chatId, items, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IList<int>)null!);

        // Act
        var result = await _messageService.SendMediaGroupAsync(chatId, items);

        // Assert
        result.Should().BeNull();
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to send media group to chat") && t != null),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendMediaGroupAsync_WhenTelegramApiThrowsException_ReturnsNull()
    {
        // Arrange
        const long chatId = 123456789;
        var items = new List<MediaGroupItem>
        {
            new MediaGroupItem(MediaType.Photo, "photo1.jpg"),
            new MediaGroupItem(MediaType.Photo, "photo2.jpg")
        };

        _mockTelegramApiClient
            .Setup(x => x.SendMediaGroupAsync(chatId, items, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("API error"));

        // Act
        var result = await _messageService.SendMediaGroupAsync(chatId, items);

        // Assert
        result.Should().BeNull();
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error sending media group to chat") && t != null),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}