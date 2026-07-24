using FluentAssertions;
using Moq;
using TelegramBotFramework.Models;
using TelegramBotFramework.Repositories;
using TelegramBotFramework.Services;
using Xunit;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Extension methods for <see cref="MessageServiceTests"/> to provide additional testing utilities.
/// </summary>
public static class MessageServiceTestsExtensions
{
    /// <summary>
    /// Creates a mock message with the specified properties for testing purposes.
    /// </summary>
    /// <param name="messageId">The message identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="chatId">The chat identifier.</param>
    /// <param name="content">The message content.</param>
    /// <param name="type">The message type. Defaults to <see cref="MessageType.Text"/>.</param>
    /// <param name="status">The message status. Defaults to <see cref="MessageStatus.Received"/>.</param>
    /// <returns>A configured <see cref="Message"/> instance.</returns>
    public static Message CreateMockMessage(
        this MessageServiceTests _,
        long messageId = 1,
        long userId = 12345,
        long chatId = 67890,
        string content = "Test message",
        MessageType type = MessageType.Text,
        MessageStatus status = MessageStatus.Received)
    {
        ArgumentNullException.ThrowIfNull(content);

        return new Message
        {
            MessageId = messageId,
            UserId = userId,
            ChatId = chatId,
            Content = content,
            Type = type,
            Status = status,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Sets up the message repository to return a specific message when queried by ID.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="mockRepository">The mock message repository.</param>
    /// <param name="messageId">The message identifier to mock.</param>
    /// <param name="message">The message to return.</param>
    public static void SetupGetMessageById(
        this MessageServiceTests tests,
        Mock<IMessageRepository> mockRepository,
        long messageId,
        Message? message)
    {
        ArgumentNullException.ThrowIfNull(mockRepository);

        mockRepository
            .Setup(x => x.GetByIdAsync(messageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(message);
    }

    /// <summary>
    /// Sets up the message repository to return a collection of messages when queried by user ID.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="mockRepository">The mock message repository.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="messages">The messages to return.</param>
    /// <param name="limit">Optional limit for pagination.</param>
    public static void SetupGetMessagesByUserId(
        this MessageServiceTests tests,
        Mock<IMessageRepository> mockRepository,
        long userId,
        IList<Message> messages,
        int? limit = null)
    {
        ArgumentNullException.ThrowIfNull(mockRepository);
        ArgumentNullException.ThrowIfNull(messages);

        if (limit.HasValue && limit.Value > 0)
        {
            var limitedMessages = messages.Take(limit.Value).ToList();
            mockRepository
                .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(limitedMessages);
        }
        else
        {
            mockRepository
                .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(messages);
        }
    }

    /// <summary>
    /// Verifies that the message repository was called to update a specific message.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <param name="mockRepository">The mock message repository.</param>
    /// <param name="messageId">The expected message identifier.</param>
    /// <param name="times">The number of expected calls. Defaults to <see cref="Times.Once()"/>.</param>
    public static void VerifyMessageUpdated(
        this MessageServiceTests tests,
        Mock<IMessageRepository> mockRepository,
        long messageId,
        Times? times = null)
    {
        ArgumentNullException.ThrowIfNull(mockRepository);

        var expectedTimes = times ?? Times.Once();

        mockRepository.Verify(
            x => x.UpdateAsync(It.Is<Message>(m => m.MessageId == messageId), It.IsAny<CancellationToken>()),
            expectedTimes);
    }
}