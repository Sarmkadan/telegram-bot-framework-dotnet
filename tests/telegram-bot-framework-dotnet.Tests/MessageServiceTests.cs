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
            UserId = MessageServiceTestsConstants.TestUserId,
            ChatId = MessageServiceTestsConstants.TestChatId,
            Content = MessageServiceTestsConstants.HelloWorldContent,
            Type = MessageType.Text
        };

        var createdMessage = new Message
        {
            MessageId = MessageServiceTestsConstants.TestMessageId,
            UserId = MessageServiceTestsConstants.TestUserId,
            ChatId = MessageServiceTestsConstants.TestChatId,
            Content = MessageServiceTestsConstants.HelloWorldContent,
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
        result.MessageId.Should().Be(MessageServiceTestsConstants.TestMessageId);
        result.Status.Should().Be(MessageStatus.Processing);
        _mockMessageRepository.Verify(x => x.CreateAsync(message, It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(MessageServiceTestsConstants.LogMessageReceived) && t != null),
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
            UserId = MessageServiceTestsConstants.InvalidId, // Invalid UserId
            ChatId = MessageServiceTestsConstants.TestChatId,
            Content = MessageServiceTestsConstants.HelloWorldContent
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _messageService.ProcessIncomingMessageAsync(message));
    }

    [Fact]
    public async Task GetMessageAsync_WithExistingMessageId_ReturnsMessage()
    {
        // Arrange
        const long messageId = MessageServiceTestsConstants.TestMessageId;
        var expectedMessage = new Message { MessageId = messageId, Content = MessageServiceTestsConstants.TestMessageContent };

        _mockMessageRepository
            .Setup(x => x.GetByIdAsync(messageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedMessage);

        // Act
        var result = await _messageService.GetMessageAsync(messageId);

        // Assert
        result.Should().NotBeNull();
        result!.MessageId.Should().Be(messageId);
        result.Content.Should().Be(MessageServiceTestsConstants.TestMessageContent);
    }

    [Fact]
    public async Task GetMessageAsync_WithNonExistingMessageId_ReturnsNull()
    {
        // Arrange
        const long messageId = MessageServiceTestsConstants.NonExistingId;

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
        const long userId = MessageServiceTestsConstants.TestUserId;
        var messages = new List<Message>
        {
            new Message { MessageId = MessageServiceTestsConstants.FirstMessageId, UserId = userId, Content = MessageServiceTestsConstants.FirstMessageContent, CreatedAt = DateTime.UtcNow.AddMinutes(-MessageServiceTestsConstants.OldestMessageMinutesOld) },
            new Message { MessageId = MessageServiceTestsConstants.SecondMessageId, UserId = userId, Content = MessageServiceTestsConstants.SecondMessageContent, CreatedAt = DateTime.UtcNow.AddMinutes(-MessageServiceTestsConstants.RecentMessageMinutesOld) },
            new Message { MessageId = MessageServiceTestsConstants.ThirdMessageId, UserId = userId, Content = MessageServiceTestsConstants.ThirdMessageContent, CreatedAt = DateTime.UtcNow }
        };

        _mockMessageRepository
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(messages);

        // Act
        var result = await _messageService.GetUserMessagesAsync(userId, MessageServiceTestsConstants.RequestedMessageCount);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(MessageServiceTestsConstants.RequestedMessageCount);
        result[MessageServiceTestsConstants.FirstPosition].Content.Should().Be(MessageServiceTestsConstants.ThirdMessageContent); // Most recent first
        result[MessageServiceTestsConstants.SecondPosition].Content.Should().Be(MessageServiceTestsConstants.SecondMessageContent);
    }

    [Fact]
    public async Task GetFailedMessagesAsync_WithFailedMessages_ReturnsFailedMessages()
    {
        // Arrange
        var failedMessages = new List<Message>
        {
            new Message { MessageId = MessageServiceTestsConstants.FirstMessageId, Content = MessageServiceTestsConstants.FailedMessage1Content, Status = MessageStatus.Failed },
            new Message { MessageId = MessageServiceTestsConstants.SecondMessageId, Content = MessageServiceTestsConstants.FailedMessage2Content, Status = MessageStatus.Failed }
        };

        _mockMessageRepository
            .Setup(x => x.GetByStatusAsync(MessageStatus.Failed, It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedMessages);

        // Act
        var result = await _messageService.GetFailedMessagesAsync(MessageServiceTestsConstants.FailedMessageLimit);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(MessageServiceTestsConstants.ExpectedFailedMessageCount);
        result.All(m => m.Status == MessageStatus.Failed).Should().BeTrue();
    }

    [Fact]
    public async Task MarkAsProcessedAsync_WithExistingMessageId_ReturnsTrue()
    {
        // Arrange
        const long messageId = MessageServiceTestsConstants.ExistingMessageId;
        var message = new Message { MessageId = messageId, Content = MessageServiceTestsConstants.TestMessageContent };

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
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(MessageServiceTestsConstants.LogMessageProcessed) && t != null),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task MarkAsProcessedAsync_WithNonExistingMessageId_ReturnsFalse()
    {
        // Arrange
        const long messageId = MessageServiceTestsConstants.NonExistingId;

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
        const long messageId = MessageServiceTestsConstants.ExistingMessageId;
        var message = new Message { MessageId = messageId, Content = MessageServiceTestsConstants.TestMessageContent };
        const string errorMessage = MessageServiceTestsConstants.ErrorMessage;

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
        message.GetMetadata(MessageServiceTestsConstants.ErrorMetadataKey).Should().Be(errorMessage);
        _mockMessageRepository.Verify(x => x.UpdateAsync(message, It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(MessageServiceTestsConstants.LogMessageFailed) && t != null),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task MarkAsFailedAsync_WithNonExistingMessageId_ReturnsFalse()
    {
        // Arrange
        const long messageId = MessageServiceTestsConstants.NonExistingId;
        const string errorMessage = MessageServiceTestsConstants.ErrorMessage;

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
            new Message { MessageId = MessageServiceTestsConstants.FirstMessageId, Status = MessageStatus.Processing },
            new Message { MessageId = MessageServiceTestsConstants.SecondMessageId, Status = MessageStatus.Processing }
        };

        var receivedMessages = new List<Message>
        {
            new Message { MessageId = MessageServiceTestsConstants.ThirdMessageId, Status = MessageStatus.Received },
            new Message { MessageId = MessageServiceTestsConstants.FourthMessageId, Status = MessageStatus.Received },
            new Message { MessageId = MessageServiceTestsConstants.FifthMessageId, Status = MessageStatus.Received }
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
        result.Should().Be(MessageServiceTestsConstants.ExpectedUnprocessedCount); // 2 processing + 3 received
    }

    [Fact]
    public async Task ArchiveOldMessagesAsync_WithOldMessages_ArchivesThem()
    {
        // Arrange
        const int daysOld = MessageServiceTestsConstants.DaysOld;
        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
        var oldMessage = new Message
        {
            MessageId = MessageServiceTestsConstants.FirstMessageId,
            Content = MessageServiceTestsConstants.OldMessageContent,
            CreatedAt = cutoffDate.AddDays(-MessageServiceTestsConstants.OlderThanCutoffDays), // Older than cutoff
            Status = MessageStatus.Processed
        };

        var recentMessage = new Message
        {
            MessageId = MessageServiceTestsConstants.SecondMessageId,
            Content = MessageServiceTestsConstants.RecentMessageContent,
            CreatedAt = DateTime.UtcNow.AddHours(-MessageServiceTestsConstants.RecentMessageHoursOld), // Newer than cutoff
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
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(MessageServiceTestsConstants.LogArchived) && t != null),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendPollAsync_WithValidInput_ReturnsCreatedMessage()
    {
        // Arrange
        const long chatId = MessageServiceTestsConstants.LargeChatId;
        const string question = MessageServiceTestsConstants.FavoriteColorQuestion;
        var options = MessageServiceTestsConstants.ColorOptions;
        const bool allowsMultipleAnswers = false;

        _mockTelegramApiClient
            .Setup(x => x.SendPollAsync(chatId, question, options, allowsMultipleAnswers))
            .ReturnsAsync(MessageServiceTestsConstants.TelegramPollMessageId); // Returns messageId from Telegram API

        var createdMessage = new Message
        {
            MessageId = MessageServiceTestsConstants.CreatedMessageId,
            ChatId = chatId,
            Content = question,
            Type = MessageType.Poll,
            Status = MessageStatus.Processed
        };
        createdMessage.SetMetadata(MessageServiceTestsConstants.PollTypeMetadataKey, MessageServiceTestsConstants.QuizPollType);
        createdMessage.SetMetadata(MessageServiceTestsConstants.PollOptionsMetadataKey, options);
        createdMessage.SetMetadata(MessageServiceTestsConstants.PollAllowsMultipleAnswersMetadataKey, allowsMultipleAnswers);
        createdMessage.SetMetadata(MessageServiceTestsConstants.PollMessageIdMetadataKey, MessageServiceTestsConstants.TelegramPollMessageId);

        _mockMessageRepository
            .Setup(x => x.CreateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdMessage);

        // Act
        var result = await _messageService.SendPollAsync(chatId, question, options, allowsMultipleAnswers);

        // Assert
        result.Should().NotBeNull();
        result!.MessageId.Should().Be(MessageServiceTestsConstants.CreatedMessageId);
        result.Type.Should().Be(MessageType.Poll);
        result.Content.Should().Be(question);
        result.Metadata.Should().ContainKey(MessageServiceTestsConstants.PollTypeMetadataKey);
        result.Metadata.Should().ContainKey(MessageServiceTestsConstants.PollOptionsMetadataKey);
        result.Metadata.Should().ContainKey(MessageServiceTestsConstants.PollAllowsMultipleAnswersMetadataKey);
        result.Metadata.Should().ContainKey(MessageServiceTestsConstants.PollMessageIdMetadataKey);

        _mockTelegramApiClient.Verify(x => x.SendPollAsync(chatId, question, options, allowsMultipleAnswers), Times.Once);
        _mockMessageRepository.Verify(x => x.CreateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(MessageServiceTestsConstants.LogPollSent) && t != null),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendPollAsync_WithInvalidChatId_ThrowsArgumentException()
    {
        // Arrange
        const long chatId = MessageServiceTestsConstants.InvalidId; // Invalid chat ID
        const string question = MessageServiceTestsConstants.TestQuestion;
        var options = MessageServiceTestsConstants.TwoOptions;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _messageService.SendPollAsync(chatId, question, options));
    }

    [Fact]
    public async Task SendPollAsync_WithEmptyQuestion_ThrowsArgumentException()
    {
        // Arrange
        const long chatId = MessageServiceTestsConstants.LargeChatId;
        const string question = MessageServiceTestsConstants.EmptyQuestion; // Empty question
        var options = MessageServiceTestsConstants.TwoOptions;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _messageService.SendPollAsync(chatId, question, options));
    }

    [Fact]
    public async Task SendPollAsync_WithTooManyOptions_ThrowsArgumentException()
    {
        // Arrange
        const long chatId = MessageServiceTestsConstants.LargeChatId;
        const string question = MessageServiceTestsConstants.TestQuestion;
        var options = new string[MessageServiceTestsConstants.TooManyOptionsCount]; // More than 10 options
        for (int i = 0; i < MessageServiceTestsConstants.TooManyOptionsCount; i++)
        {
            options[i] = string.Format(MessageServiceTestsConstants.GeneratedOptionFormat, i + MessageServiceTestsConstants.OneBasedIndexOffset);
        }

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _messageService.SendPollAsync(chatId, question, options));
    }

    [Fact]
    public async Task SendPollAsync_WhenTelegramApiFails_ReturnsNull()
    {
        // Arrange
        const long chatId = MessageServiceTestsConstants.LargeChatId;
        const string question = MessageServiceTestsConstants.TestQuestion;
        var options = MessageServiceTestsConstants.TwoOptions;

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
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(MessageServiceTestsConstants.LogPollSendFailed) && t != null),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendPollAsync_WhenTelegramApiThrowsException_ReturnsNull()
    {
        // Arrange
        const long chatId = MessageServiceTestsConstants.LargeChatId;
        const string question = MessageServiceTestsConstants.TestQuestion;
        var options = MessageServiceTestsConstants.TwoOptions;

        _mockTelegramApiClient
            .Setup(x => x.SendPollAsync(chatId, question, options, false))
            .ThrowsAsync(new Exception(MessageServiceTestsConstants.ApiErrorMessage));

        // Act
        var result = await _messageService.SendPollAsync(chatId, question, options);

        // Assert
        result.Should().BeNull();
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(MessageServiceTestsConstants.LogPollSendError) && t != null),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendMediaGroupAsync_WithValidInput_ReturnsCreatedMessages()
    {
        // Arrange
        const long chatId = MessageServiceTestsConstants.LargeChatId;
        var items = new List<MediaGroupItem>
        {
            new MediaGroupItem(MediaType.Photo, MessageServiceTestsConstants.Photo1Url, MessageServiceTestsConstants.FirstPhotoCaption),
            new MediaGroupItem(MediaType.Photo, MessageServiceTestsConstants.Photo2Url, MessageServiceTestsConstants.SecondPhotoCaption)
        };
        const string caption = MessageServiceTestsConstants.PhotoAlbumCaption;

        var messageIds = new List<int> { MessageServiceTestsConstants.CreatedMessageId, MessageServiceTestsConstants.AnotherMessageId };

        _mockTelegramApiClient
            .Setup(x => x.SendMediaGroupAsync(chatId, items, It.IsAny<CancellationToken>()))
            .ReturnsAsync(messageIds);

        var createdMessage1 = new Message
        {
            MessageId = MessageServiceTestsConstants.CreatedMessageId,
            ChatId = chatId,
            Content = caption,
            Type = MessageType.Photo,
            Status = MessageStatus.Processed
        };
        createdMessage1.SetMetadata(MessageServiceTestsConstants.MediaTypeMetadataKey, MessageServiceTestsConstants.PhotoMediaType);
        createdMessage1.SetMetadata(MessageServiceTestsConstants.FileIdOrUrlMetadataKey, MessageServiceTestsConstants.Photo1Url);
        createdMessage1.SetMetadata(MessageServiceTestsConstants.PollMessageIdMetadataKey, MessageServiceTestsConstants.CreatedMessageId);
        createdMessage1.SetMetadata(MessageServiceTestsConstants.MediaPositionMetadataKey, MessageServiceTestsConstants.FirstPosition);
        createdMessage1.SetMetadata(MessageServiceTestsConstants.MediaCaptionMetadataKey, caption);

        var createdMessage2 = new Message
        {
            MessageId = MessageServiceTestsConstants.AnotherMessageId,
            ChatId = chatId,
            Content = caption,
            Type = MessageType.Photo,
            Status = MessageStatus.Processed
        };
        createdMessage2.SetMetadata(MessageServiceTestsConstants.MediaTypeMetadataKey, MessageServiceTestsConstants.PhotoMediaType);
        createdMessage2.SetMetadata(MessageServiceTestsConstants.FileIdOrUrlMetadataKey, MessageServiceTestsConstants.Photo2Url);
        createdMessage2.SetMetadata(MessageServiceTestsConstants.PollMessageIdMetadataKey, MessageServiceTestsConstants.AnotherMessageId);
        createdMessage2.SetMetadata(MessageServiceTestsConstants.MediaPositionMetadataKey, MessageServiceTestsConstants.SecondPosition);
        createdMessage2.SetMetadata(MessageServiceTestsConstants.MediaCaptionMetadataKey, caption);

        _mockMessageRepository
            .SetupSequence(x => x.CreateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdMessage1)
            .ReturnsAsync(createdMessage2);

        // Act
        var result = await _messageService.SendMediaGroupAsync(chatId, items, caption);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(MessageServiceTestsConstants.ExpectedMediaMessageCount);
        result![MessageServiceTestsConstants.FirstPosition].MessageId.Should().Be(MessageServiceTestsConstants.CreatedMessageId);
        result[MessageServiceTestsConstants.SecondPosition].MessageId.Should().Be(MessageServiceTestsConstants.AnotherMessageId);
        result[MessageServiceTestsConstants.FirstPosition].Type.Should().Be(MessageType.Photo);
        result[MessageServiceTestsConstants.SecondPosition].Type.Should().Be(MessageType.Photo);

        _mockTelegramApiClient.Verify(x => x.SendMediaGroupAsync(chatId, items, It.IsAny<CancellationToken>()), Times.Once);
        _mockMessageRepository.Verify(x => x.CreateAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()), Times.Exactly(MessageServiceTestsConstants.ExpectedMediaMessageCount));
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(MessageServiceTestsConstants.LogMediaGroupSent) && t != null),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendMediaGroupAsync_WithInvalidChatId_ThrowsArgumentException()
    {
        // Arrange
        const long chatId = MessageServiceTestsConstants.InvalidId; // Invalid chat ID
        var items = new List<MediaGroupItem>
        {
            new MediaGroupItem(MediaType.Photo, MessageServiceTestsConstants.Photo1Url)
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _messageService.SendMediaGroupAsync(chatId, items));
    }

    [Fact]
    public async Task SendMediaGroupAsync_WithTooFewItems_ThrowsArgumentException()
    {
        // Arrange
        const long chatId = MessageServiceTestsConstants.LargeChatId;
        var items = new List<MediaGroupItem>(); // Empty list

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _messageService.SendMediaGroupAsync(chatId, items));
    }

    [Fact]
    public async Task SendMediaGroupAsync_WithTooManyItems_ThrowsArgumentException()
    {
        // Arrange
        const long chatId = MessageServiceTestsConstants.LargeChatId;
        var items = new List<MediaGroupItem>();
        for (int i = 0; i < MessageServiceTestsConstants.TooManyMediaItemsCount; i++)
        {
            items.Add(new MediaGroupItem(MediaType.Photo, string.Format(MessageServiceTestsConstants.GeneratedPhotoFileFormat, i)));
        }

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _messageService.SendMediaGroupAsync(chatId, items));
    }

    [Fact]
    public async Task SendMediaGroupAsync_WhenTelegramApiFails_ReturnsNull()
    {
        // Arrange
        const long chatId = MessageServiceTestsConstants.LargeChatId;
        var items = new List<MediaGroupItem>
        {
            new MediaGroupItem(MediaType.Photo, MessageServiceTestsConstants.Photo1Url),
            new MediaGroupItem(MediaType.Photo, MessageServiceTestsConstants.Photo2Url)
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
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(MessageServiceTestsConstants.LogMediaGroupSendFailed) && t != null),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendMediaGroupAsync_WhenTelegramApiThrowsException_ReturnsNull()
    {
        // Arrange
        const long chatId = MessageServiceTestsConstants.LargeChatId;
        var items = new List<MediaGroupItem>
        {
            new MediaGroupItem(MediaType.Photo, MessageServiceTestsConstants.Photo1Url),
            new MediaGroupItem(MediaType.Photo, MessageServiceTestsConstants.Photo2Url)
        };

        _mockTelegramApiClient
            .Setup(x => x.SendMediaGroupAsync(chatId, items, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(MessageServiceTestsConstants.ApiErrorMessage));

        // Act
        var result = await _messageService.SendMediaGroupAsync(chatId, items);

        // Assert
        result.Should().BeNull();
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(MessageServiceTestsConstants.LogMediaGroupSendError) && t != null),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
