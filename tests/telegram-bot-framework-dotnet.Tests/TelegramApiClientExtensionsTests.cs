using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using TelegramBotFramework.Integration;

namespace tests.telegram_bot_framework_dotnet.Tests
{
    public class TelegramApiClientExtensionsTests
    {
        [Fact]
        public async Task SendMessageWithButtonsAsync_HappyPath_ReturnsTrue()
        {
            // Arrange
            var clientMock = new Mock<TelegramApiClient>();
            clientMock.Setup(c => c.SendMessageWithButtonsAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string[][]>()))
                .ReturnsAsync(true);

            // Act
            var result = await TelegramApiClientExtensions.SendMessageWithButtonsAsync(clientMock.Object, 123, "Hello", new[] { new[] { "Button1", "Button2" } });

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task SendMessageWithButtonsAsync_NullClient_ThrowsArgumentNullException()
        {
            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => TelegramApiClientExtensions.SendMessageWithButtonsAsync(null, 123, "Hello", new[] { new[] { "Button1", "Button2" } }));
        }

        [Fact]
        public async Task EditMessageTextAsync_HappyPath_ReturnsTrue()
        {
            // Arrange
            var clientMock = new Mock<TelegramApiClient>();
            clientMock.Setup(c => c.EditMessageAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await TelegramApiClientExtensions.EditMessageTextAsync(clientMock.Object, 123, 1, "New text");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task EditMessageTextAsync_NullClient_ThrowsArgumentNullException()
        {
            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => TelegramApiClientExtensions.EditMessageTextAsync(null, 123, 1, "New text"));
        }

        [Fact]
        public async Task AnswerCallbackQueryWithTextAsync_HappyPath_ReturnsTrue()
        {
            // Arrange
            var clientMock = new Mock<TelegramApiClient>();
            clientMock.Setup(c => c.AnswerCallbackQueryAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await TelegramApiClientExtensions.AnswerCallbackQueryWithTextAsync(clientMock.Object, "callbackQueryId", "Text");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetBotInformationAsync_HappyPath_ReturnsBotInfo()
        {
            // Arrange
            var clientMock = new Mock<TelegramApiClient>();
            clientMock.Setup(c => c.GetMeAsync())
                .ReturnsAsync("Bot info");

            // Act
            var result = await TelegramApiClientExtensions.GetBotInformationAsync(clientMock.Object);

            // Assert
            Assert.Equal("Bot info", result);
        }

        [Fact]
        public async Task GetBotInformationAsync_NullClient_ThrowsArgumentNullException()
        {
            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => TelegramApiClientExtensions.GetBotInformationAsync(null));
        }
    }
}
