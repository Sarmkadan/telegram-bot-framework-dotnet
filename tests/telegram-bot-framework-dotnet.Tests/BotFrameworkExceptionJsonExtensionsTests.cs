using System;
using FluentAssertions;
using TelegramBotFramework.Exceptions;
using Xunit;

namespace TelegramBotFramework.Tests;

public class BotFrameworkExceptionJsonExtensionsTests
{
    public class ToJson : BotFrameworkExceptionJsonExtensionsTests
    {
        [Fact]
        public void ReturnsValidJsonString_WhenCalledWithValidException()
        {
            // Arrange
            var exception = new BotFrameworkException("Test error message", "TEST_ERROR");

            // Act
            var json = exception.ToJson();

            // Assert
            json.Should().NotBeNullOrEmpty();
            json.Should().Contain("Test error message");
            json.Should().Contain("TEST_ERROR");
        }

        [Fact]
        public void ReturnsIndentedJson_WhenIndentedParameterIsTrue()
        {
            // Arrange
            var exception = new BotFrameworkException("Test error message", "TEST_ERROR");

            // Act
            var json = exception.ToJson(indented: true);

            // Assert
            json.Should().NotBeNullOrEmpty();
            json.Should().Contain("Test error message");
            json.Should().Contain("TEST_ERROR");
            json.Should().Contain("\n"); // Should have newlines for indentation
        }

        [Fact]
        public void ReturnsCompactJson_WhenIndentedParameterIsFalse()
        {
            // Arrange
            var exception = new BotFrameworkException("Test error message", "TEST_ERROR");

            // Act
            var json = exception.ToJson(indented: false);

            // Assert
            json.Should().NotBeNullOrEmpty();
            json.Should().Contain("Test error message");
            json.Should().Contain("TEST_ERROR");
            json.Should().NotContain("\n"); // Should not have newlines
        }

        [Fact]
        public void ThrowsArgumentNullException_WhenValueIsNull()
        {
            // Arrange
            BotFrameworkException? exception = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => exception!.ToJson());
        }

        [Fact]
        public void SerializesErrorCodeProperty()
        {
            // Arrange
            var exception = new BotFrameworkException("Session failed", "SESSION_ERROR");

            // Act
            var json = exception.ToJson();

            // Assert
            json.Should().Contain("sessionError");
        }

        [Fact]
        public void SerializesCommandExecutionException()
        {
            // Arrange
            var exception = new CommandExecutionException("Command failed", "test-command");

            // Act
            var json = exception.ToJson();

            // Assert
            json.Should().NotBeNullOrEmpty();
            json.Should().Contain("Command failed");
            json.Should().Contain("commandExecutionError");
            json.Should().Contain("test-command");
        }
    }

    public class FromJson : BotFrameworkExceptionJsonExtensionsTests
    {
        [Fact]
        public void ReturnsNull_WhenJsonIsNull()
        {
            // Arrange & Act
            var result = BotFrameworkExceptionJsonExtensions.FromJson(null);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ReturnsNull_WhenJsonIsEmpty()
        {
            // Arrange & Act
            var result = BotFrameworkExceptionJsonExtensions.FromJson("");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ReturnsNull_WhenJsonIsWhitespace()
        {
            // Arrange & Act
            var result = BotFrameworkExceptionJsonExtensions.FromJson("   ");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ReturnsDeserializedException_WhenJsonIsValid()
        {
            // Arrange
            var originalException = new BotFrameworkException("Test error", "TEST_ERROR_001");
            var json = originalException.ToJson();

            // Act
            var result = BotFrameworkExceptionJsonExtensions.FromJson(json);

            // Assert
            result.Should().NotBeNull();
            result!.Message.Should().Be("Test error");
            result.ErrorCode.Should().Be("TEST_ERROR_001");
        }

        [Fact]
        public void ReturnsDeserializedException_WhenJsonHasCamelCaseProperties()
        {
            // Arrange
            var json = "{\"message\":\"Camel case test\",\"errorCode\":\"CAMEL_TEST\"}";

            // Act
            var result = BotFrameworkExceptionJsonExtensions.FromJson(json);

            // Assert
            result.Should().NotBeNull();
            result!.Message.Should().Be("Camel case test");
            result.ErrorCode.Should().Be("CAMEL_TEST");
        }

        [Fact]
        public void ReturnsNull_WhenJsonIsMalformed()
        {
            // Arrange
            var malformedJson = "{ invalid json";

            // Act
            var result = BotFrameworkExceptionJsonExtensions.FromJson(malformedJson);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ReturnsNull_WhenJsonHasInvalidStructure()
        {
            // Arrange
            var invalidJson = "{\"invalid\":\"structure\"}";

            // Act
            var result = BotFrameworkExceptionJsonExtensions.FromJson(invalidJson);

            // Assert
            result.Should().BeNull();
        }
    }

    public class TryFromJson : BotFrameworkExceptionJsonExtensionsTests
    {
        [Fact]
        public void ReturnsFalse_WhenJsonIsNull()
        {
            // Arrange
            string? json = null;

            // Act
            var result = BotFrameworkExceptionJsonExtensions.TryFromJson(json!, out var exception);

            // Assert
            result.Should().BeFalse();
            exception.Should().BeNull();
        }

        [Fact]
        public void ThrowsArgumentNullException_WhenJsonIsNull()
        {
            // Arrange
            string? json = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => BotFrameworkExceptionJsonExtensions.TryFromJson(json!, out _));
        }

        [Fact]
        public void ReturnsFalse_WhenJsonIsEmpty()
        {
            // Arrange
            var json = "";

            // Act
            var result = BotFrameworkExceptionJsonExtensions.TryFromJson(json, out var exception);

            // Assert
            result.Should().BeFalse();
            exception.Should().BeNull();
        }

        [Fact]
        public void ReturnsFalse_WhenJsonIsWhitespace()
        {
            // Arrange
            var json = "   ";

            // Act
            var result = BotFrameworkExceptionJsonExtensions.TryFromJson(json, out var exception);

            // Assert
            result.Should().BeFalse();
            exception.Should().BeNull();
        }

        [Fact]
        public void ReturnsTrueAndDeserializedException_WhenJsonIsValid()
        {
            // Arrange
            var originalException = new BotFrameworkException("Valid exception", "VALID_001");
            var json = originalException.ToJson();

            // Act
            var result = BotFrameworkExceptionJsonExtensions.TryFromJson(json, out var exception);

            // Assert
            result.Should().BeTrue();
            exception.Should().NotBeNull();
            exception!.Message.Should().Be("Valid exception");
            exception.ErrorCode.Should().Be("VALID_001");
        }

        [Fact]
        public void ReturnsFalseAndNull_WhenJsonIsMalformed()
        {
            // Arrange
            var malformedJson = "{ invalid json";

            // Act
            var result = BotFrameworkExceptionJsonExtensions.TryFromJson(malformedJson, out var exception);

            // Assert
            result.Should().BeFalse();
            exception.Should().BeNull();
        }
    }
}
