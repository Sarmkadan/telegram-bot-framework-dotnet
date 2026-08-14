using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using TelegramBotFramework.Services;
using Xunit;

namespace TelegramBotFrameworkDotnet.Tests
{
    public class ScheduledMessageServiceExtensionsTests
    {
        [Fact]
        public async Task ScheduleMessageAsync_HappyPath_ReturnsMessageId()
        {
            // Arrange
            var serviceMock = new Mock<IScheduledMessageService>();
            var chatId = 123;
            var text = "Hello, world!";
            var sendAt = DateTime.Now.AddMinutes(1);
            var messageId = "message-id";

            serviceMock.Setup(s => s.ScheduleMessageAsync(chatId, text, It.IsAny<DateTimeOffset>())).ReturnsAsync(messageId);

            // Act
            var result = await ScheduledMessageServiceExtensions.ScheduleMessageAsync(serviceMock.Object, chatId, text, sendAt);

            // Assert
            Assert.Equal(messageId, result);
        }

        [Fact]
        public async Task ScheduleMessageAsync_NullService_ThrowsArgumentNullException()
        {
            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => ScheduledMessageServiceExtensions.ScheduleMessageAsync(null, 123, "text", DateTime.Now));
        }

        [Fact]
        public async Task ScheduleMessageAsync_NullText_ThrowsArgumentNullException()
        {
            // Arrange
            var serviceMock = new Mock<IScheduledMessageService>();

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => ScheduledMessageServiceExtensions.ScheduleMessageAsync(serviceMock.Object, 123, null, DateTime.Now));
        }

        [Fact]
        public async Task GetScheduledMessagesForChat_HappyPath_ReturnsMessages()
        {
            // Arrange
            var serviceMock = new Mock<IScheduledMessageService>();
            var chatId = 123;
            var messages = new List<ScheduledMessage> { new ScheduledMessage() };

            serviceMock.Setup(s => s.GetScheduledMessagesForChat(chatId)).Returns(messages);

            // Act
            var result = ScheduledMessageServiceExtensions.GetScheduledMessagesForChat(serviceMock.Object, chatId);

            // Assert
            Assert.Equal(messages, result);
        }

        [Fact]
        public void GetScheduledMessagesForChat_NullService_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ScheduledMessageServiceExtensions.GetScheduledMessagesForChat(null, 123));
        }

        [Fact]
        public async Task GetScheduledMessage_HappyPath_ReturnsMessage()
        {
            // Arrange
            var serviceMock = new Mock<IScheduledMessageService>();
            var messageId = "message-id";
            var message = new ScheduledMessage();

            serviceMock.Setup(s => s.GetScheduledMessage(messageId)).Returns(message);

            // Act
            var result = ScheduledMessageServiceExtensions.GetScheduledMessage(serviceMock.Object, messageId);

            // Assert
            Assert.Equal(message, result);
        }

        [Fact]
        public void GetScheduledMessage_NullService_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ScheduledMessageServiceExtensions.GetScheduledMessage(null, "message-id"));
        }
    }
}
