using System;
using System.Collections.Generic;
using Xunit;
using TelegramBotFramework.Models;

namespace tests.telegram_bot_framework_dotnet.Tests
{
    public class TelegramBotFrameworkDotnetOptionsValidationExtensionsTests
    {
        [Fact]
        public void ValidateOptions_HappyPath_ReturnsEmptyList()
        {
            // Arrange
            var options = new TelegramBotFrameworkDotnetOptions
            {
                BotToken = "token",
                BotUsername = "username",
                DatabaseConnectionString = "connectionString",
                SessionTimeoutMinutes = 10,
                MessageProcessingTimeoutSeconds = 30,
                MaxConcurrentRequests = 50,
                RateLimitPerMinute = 100
            };

            // Act
            var errors = options.ValidateOptions();

            // Assert
            Assert.Empty(errors);
        }

        [Fact]
        public void ValidateOptions_NullOptions_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ((TelegramBotFrameworkDotnetOptions)null).ValidateOptions());
        }

        [Fact]
        public void IsValidOptions_HappyPath_ReturnsTrue()
        {
            // Arrange
            var options = new TelegramBotFrameworkDotnetOptions
            {
                BotToken = "token",
                BotUsername = "username",
                DatabaseConnectionString = "connectionString",
                SessionTimeoutMinutes = 10,
                MessageProcessingTimeoutSeconds = 30,
                MaxConcurrentRequests = 50,
                RateLimitPerMinute = 100
            };

            // Act
            var isValid = options.IsValidOptions();

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void IsValidOptions_InvalidOptions_ReturnsFalse()
        {
            // Arrange
            var options = new TelegramBotFrameworkDotnetOptions
            {
                BotToken = string.Empty,
                BotUsername = "username",
                DatabaseConnectionString = "connectionString",
                SessionTimeoutMinutes = 10,
                MessageProcessingTimeoutSeconds = 30,
                MaxConcurrentRequests = 50,
                RateLimitPerMinute = 100
            };

            // Act
            var isValid = options.IsValidOptions();

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void EnsureOptionsValid_HappyPath_DoesNotThrow()
        {
            // Arrange
            var options = new TelegramBotFrameworkDotnetOptions
            {
                BotToken = "token",
                BotUsername = "username",
                DatabaseConnectionString = "connectionString",
                SessionTimeoutMinutes = 10,
                MessageProcessingTimeoutSeconds = 30,
                MaxConcurrentRequests = 50,
                RateLimitPerMinute = 100
            };

            // Act and Assert
            options.EnsureOptionsValid();
        }

        [Fact]
        public void EnsureOptionsValid_InvalidOptions_ThrowsArgumentException()
        {
            // Arrange
            var options = new TelegramBotFrameworkDotnetOptions
            {
                BotToken = string.Empty,
                BotUsername = "username",
                DatabaseConnectionString = "connectionString",
                SessionTimeoutMinutes = 10,
                MessageProcessingTimeoutSeconds = 30,
                MaxConcurrentRequests = 50,
                RateLimitPerMinute = 100
            };

            // Act and Assert
            Assert.Throws<ArgumentException>(() => options.EnsureOptionsValid());
        }
    }
}
