using Xunit;
using TelegramBotFramework.Services;

namespace TelegramBotFrameworkDotnet.Tests
{
    public class BroadcastOptionsTests
    {
        [Fact]
        public void DefaultConstructor_SetsDefaultValues()
        {
            // Arrange and Act
            var options = new BroadcastOptions();

            // Assert
            Assert.Equal(25, options.MessagesPerSecond);
            Assert.Equal(5, options.MaxConcurrency);
            Assert.Equal(3, options.MaxRetryAttempts);
            Assert.Equal(TimeSpan.FromSeconds(1), options.RetryDelay);
            Assert.True(options.ContinueOnError);
            Assert.Null(options.MessageFormatter);
            Assert.Null(options.BatchDelay);
        }

        [Fact]
        public void MessagesPerSecond_SetGetterAndSetter()
        {
            // Arrange
            var options = new BroadcastOptions();

            // Act
            options.MessagesPerSecond = 10;

            // Assert
            Assert.Equal(10, options.MessagesPerSecond);
        }

        [Fact]
        public void MaxConcurrency_SetGetterAndSetter()
        {
            // Arrange
            var options = new BroadcastOptions();

            // Act
            options.MaxConcurrency = 10;

            // Assert
            Assert.Equal(10, options.MaxConcurrency);
        }

        [Fact]
        public void MaxRetryAttempts_SetGetterAndSetter()
        {
            // Arrange
            var options = new BroadcastOptions();

            // Act
            options.MaxRetryAttempts = 10;

            // Assert
            Assert.Equal(10, options.MaxRetryAttempts);
        }

        [Fact]
        public void RetryDelay_SetGetterAndSetter()
        {
            // Arrange
            var options = new BroadcastOptions();

            // Act
            options.RetryDelay = TimeSpan.FromSeconds(10);

            // Assert
            Assert.Equal(TimeSpan.FromSeconds(10), options.RetryDelay);
        }

        [Fact]
        public void ContinueOnError_SetGetterAndSetter()
        {
            // Arrange
            var options = new BroadcastOptions();

            // Act
            options.ContinueOnError = false;

            // Assert
            Assert.False(options.ContinueOnError);
        }

        [Fact]
        public void MessageFormatter_SetGetterAndSetter()
        {
            // Arrange
            var options = new BroadcastOptions();

            // Act
            options.MessageFormatter = (message, id) => message;

            // Assert
            Assert.NotNull(options.MessageFormatter);
        }

        [Fact]
        public void BatchDelay_SetGetterAndSetter()
        {
            // Arrange
            var options = new BroadcastOptions();

            // Act
            options.BatchDelay = TimeSpan.FromSeconds(10);

            // Assert
            Assert.Equal(TimeSpan.FromSeconds(10), options.BatchDelay);
        }
    }
}
