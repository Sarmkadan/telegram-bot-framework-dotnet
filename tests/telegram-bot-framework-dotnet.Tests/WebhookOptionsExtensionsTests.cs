using System;
using FluentAssertions;
using TelegramBotFramework.Integration;
using Xunit;

namespace TelegramBotFramework.Tests
{
    public class WebhookOptionsExtensionsTests
    {
        [Fact]
        public void AreUpdatesAllowed_ReturnsTrue_WhenAllowedUpdatesIsNull()
        {
            // Arrange
            var options = new WebhookOptions
            {
                AllowedUpdates = null
            };

            // Act
            var result = options.AreUpdatesAllowed();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void AreUpdatesAllowed_ReturnsFalse_WhenAllowedUpdatesIsEmpty()
        {
            // Arrange
            var options = new WebhookOptions
            {
                AllowedUpdates = Array.Empty<string>()
            };

            // Act
            var result = options.AreUpdatesAllowed();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void AreUpdatesAllowed_ReturnsTrue_WhenAllowedUpdatesHasItems()
        {
            // Arrange
            var options = new WebhookOptions
            {
                AllowedUpdates = new[] { "message", "edited_message" }
            };

            // Act
            var result = options.AreUpdatesAllowed();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void AreUpdatesAllowed_ThrowsArgumentNullException_WhenOptionsIsNull()
        {
            // Arrange
            WebhookOptions? options = null;

            // Act
            Action act = () => options!.AreUpdatesAllowed();

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("options");
        }

        [Fact]
        public void GetMaxConnectionsAsLong_ReturnsCorrectValue()
        {
            // Arrange
            var options = new WebhookOptions
            {
                MaxConnections = 42
            };

            // Act
            var result = options.GetMaxConnectionsAsLong();

            // Assert
            result.Should().Be(42);
        }

        [Fact]
        public void GetMaxConnectionsAsLong_ReturnsZero_WhenMaxConnectionsIsZero()
        {
            // Arrange
            var options = new WebhookOptions
            {
                MaxConnections = 0
            };

            // Act
            var result = options.GetMaxConnectionsAsLong();

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public void GetMaxConnectionsAsLong_ThrowsArgumentNullException_WhenOptionsIsNull()
        {
            // Arrange
            WebhookOptions? options = null;

            // Act
            Action act = () => options!.GetMaxConnectionsAsLong();

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("options");
        }

        [Fact]
        public void HasSecretToken_ReturnsTrue_WhenSecretTokenIsNonEmpty()
        {
            // Arrange
            var options = new WebhookOptions
            {
                SecretToken = "my-secret"
            };

            // Act
            var result = options.HasSecretToken();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void HasSecretToken_ReturnsTrue_WhenSecretTokenIsWhitespace()
        {
            // Arrange
            var options = new WebhookOptions
            {
                SecretToken = "   "
            };

            // Act
            var result = options.HasSecretToken();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void HasSecretToken_ReturnsFalse_WhenSecretTokenIsNull()
        {
            // Arrange
            var options = new WebhookOptions
            {
                SecretToken = null
            };

            // Act
            var result = options.HasSecretToken();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void HasSecretToken_ReturnsFalse_WhenSecretTokenIsEmpty()
        {
            // Arrange
            var options = new WebhookOptions
            {
                SecretToken = string.Empty
            };

            // Act
            var result = options.HasSecretToken();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void HasSecretToken_ThrowsArgumentNullException_WhenOptionsIsNull()
        {
            // Arrange
            WebhookOptions? options = null;

            // Act
            Action act = () => options!.HasSecretToken();

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("options");
        }
    }
}
