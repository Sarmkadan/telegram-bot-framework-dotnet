using Xunit;
using System;
using System.Collections.Generic;
using TelegramBotFramework.Models;

namespace TelegramBotFrameworkDotnet.Tests
{
    public class CommandTests
    {
        [Fact]
        public void Validate_HappyPath_ReturnsTrue()
        {
            // Arrange
            var command = new Command { Name = "test", HandlerType = "handler" };

            // Act
            var result = command.Validate();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Validate_NullName_ThrowsException()
        {
            // Arrange
            var command = new Command { HandlerType = "handler" };

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => command.Validate());
        }

        [Fact]
        public void GetCommandPatterns_HappyPath_ReturnsPatterns()
        {
            // Arrange
            var command = new Command { Name = "test", Aliases = new List<string> { "alias1", "alias2" } };

            // Act
            var patterns = command.GetCommandPatterns();

            // Assert
            Assert.Equal(3, patterns.Count());
        }

        [Fact]
        public void CanExecuteBy_HappyPath_ReturnsTrue()
        {
            // Arrange
            var command = new Command { IsEnabled = true };

            // Act
            var result = command.CanExecuteBy(UserRole.Administrator);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanExecuteBy_DisabledCommand_ReturnsFalse()
        {
            // Arrange
            var command = new Command { IsEnabled = false };

            // Act
            var result = command.CanExecuteBy(UserRole.Administrator);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void RecordExecution_HappyPath_IncrementsCount()
        {
            // Arrange
            var command = new Command { ExecutionCount = 0 };

            // Act
            command.RecordExecution();

            // Assert
            Assert.Equal(1, command.ExecutionCount);
        }

        [Fact]
        public void IsRateLimited_HappyPath_ReturnsTrue()
        {
            // Arrange
            var command = new Command { RateLimitPerMinute = 1 };

            // Act
            var result = command.IsRateLimited(2);

            // Assert
            Assert.True(result);
        }
    }
}
