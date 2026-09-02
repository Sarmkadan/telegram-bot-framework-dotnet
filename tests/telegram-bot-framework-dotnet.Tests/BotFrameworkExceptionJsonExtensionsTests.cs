using System;
using FluentAssertions;
using TelegramBotFramework.Exceptions;
using Xunit;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Verifies JSON serialization and deserialization behavior for bot framework exceptions.
/// </summary>
public class BotFrameworkExceptionJsonExtensionsTests : IBotFrameworkExceptionJsonExtensionsTests
{
    /// <summary>
    /// Verifies that serializing a valid bot framework exception produces JSON containing its message and error code.
    /// </summary>
    public void ReturnsValidJsonString_WhenCalledWithValidException()
    {
        // Arrange
        var exception = new BotFrameworkException(BotFrameworkExceptionJsonExtensionsTestsConstants.TestErrorMessage, BotFrameworkExceptionJsonExtensionsTestsConstants.TestErrorCode);

        // Act
        var json = exception.ToJson();

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain(BotFrameworkExceptionJsonExtensionsTestsConstants.TestErrorMessage);
        json.Should().Contain(BotFrameworkExceptionJsonExtensionsTestsConstants.TestErrorCode);
    }

    /// <summary>
    /// Verifies that requesting indented serialization produces JSON containing line breaks, the message, and the error code.
    /// </summary>
    public void ReturnsIndentedJson_WhenIndentedParameterIsTrue()
    {
        // Arrange
        var exception = new BotFrameworkException(BotFrameworkExceptionJsonExtensionsTestsConstants.TestErrorMessage, BotFrameworkExceptionJsonExtensionsTestsConstants.TestErrorCode);

        // Act
        var json = exception.ToJson(indented: true);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain(BotFrameworkExceptionJsonExtensionsTestsConstants.TestErrorMessage);
        json.Should().Contain(BotFrameworkExceptionJsonExtensionsTestsConstants.TestErrorCode);
        json.Should().Contain(BotFrameworkExceptionJsonExtensionsTestsConstants.Newline); // Should have newlines for indentation
    }

    /// <summary>
    /// Verifies that disabling indentation produces compact JSON without line breaks while preserving the message and error code.
    /// </summary>
    public void ReturnsCompactJson_WhenIndentedParameterIsFalse()
    {
        // Arrange
        var exception = new BotFrameworkException(BotFrameworkExceptionJsonExtensionsTestsConstants.TestErrorMessage, BotFrameworkExceptionJsonExtensionsTestsConstants.TestErrorCode);

        // Act
        var json = exception.ToJson(indented: false);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain(BotFrameworkExceptionJsonExtensionsTestsConstants.TestErrorMessage);
        json.Should().Contain(BotFrameworkExceptionJsonExtensionsTestsConstants.TestErrorCode);
        json.Should().NotContain(BotFrameworkExceptionJsonExtensionsTestsConstants.Newline); // Should not have newlines
    }

    /// <summary>
    /// Verifies that serializing a null exception throws an <see cref="ArgumentNullException"/>.
    /// </summary>
    public void ThrowsArgumentNullException_WhenValueIsNull()
    {
        // Arrange
        BotFrameworkException? exception = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception!.ToJson());
    }

    /// <summary>
    /// Verifies that serialization includes the exception's error code property.
    /// </summary>
    public void SerializesErrorCodeProperty()
    {
        // Arrange
        var exception = new BotFrameworkException(BotFrameworkExceptionJsonExtensionsTestsConstants.SessionFailedMessage, BotFrameworkExceptionJsonExtensionsTestsConstants.SessionErrorCode);

        // Act
        var json = exception.ToJson();

        // Assert
        json.Should().Contain(BotFrameworkExceptionJsonExtensionsTestsConstants.SessionErrorJsonProperty);
    }

    /// <summary>
    /// Verifies that serializing a command execution exception includes its message, error code, and command.
    /// </summary>
    public void SerializesCommandExecutionException()
    {
        // Arrange
        var exception = new CommandExecutionException(BotFrameworkExceptionJsonExtensionsTestsConstants.CommandFailedMessage, BotFrameworkExceptionJsonExtensionsTestsConstants.TestCommand);

        // Act
        var json = exception.ToJson();

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain(BotFrameworkExceptionJsonExtensionsTestsConstants.CommandFailedMessage);
        json.Should().Contain(BotFrameworkExceptionJsonExtensionsTestsConstants.CommandExecutionErrorJsonProperty);
        json.Should().Contain(BotFrameworkExceptionJsonExtensionsTestsConstants.TestCommand);
    }

    /// <summary>
    /// Verifies that deserializing a null JSON value returns null.
    /// </summary>
    public void ReturnsNull_WhenJsonIsNull()
    {
        // Arrange & Act
        var result = BotFrameworkExceptionJsonExtensions.FromJson(null);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that deserializing an empty JSON string returns null.
    /// </summary>
    public void ReturnsNull_WhenJsonIsEmpty()
    {
        // Arrange & Act
        var result = BotFrameworkExceptionJsonExtensions.FromJson(BotFrameworkExceptionJsonExtensionsTestsConstants.EmptyJson);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that deserializing whitespace-only JSON returns null.
    /// </summary>
    public void ReturnsNull_WhenJsonIsWhitespace()
    {
        // Arrange & Act
        var result = BotFrameworkExceptionJsonExtensions.FromJson(BotFrameworkExceptionJsonExtensionsTestsConstants.WhitespaceJson);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that valid serialized exception JSON is deserialized with the original message and error code.
    /// </summary>
    public void ReturnsDeserializedException_WhenJsonIsValid()
    {
        // Arrange
        var originalException = new BotFrameworkException(BotFrameworkExceptionJsonExtensionsTestsConstants.DeserializedTestErrorMessage, BotFrameworkExceptionJsonExtensionsTestsConstants.TestErrorCode001);
        var json = originalException.ToJson();

        // Act
        var result = BotFrameworkExceptionJsonExtensions.FromJson(json);

        // Assert
        result.Should().NotBeNull();
        result!.Message.Should().Be(BotFrameworkExceptionJsonExtensionsTestsConstants.DeserializedTestErrorMessage);
        result.ErrorCode.Should().Be(BotFrameworkExceptionJsonExtensionsTestsConstants.TestErrorCode001);
    }

    /// <summary>
    /// Verifies that JSON with camel-case properties is deserialized into an exception with the expected message and error code.
    /// </summary>
    public void ReturnsDeserializedException_WhenJsonHasCamelCaseProperties()
    {
        // Arrange
        var json = BotFrameworkExceptionJsonExtensionsTestsConstants.CamelCaseJson;

        // Act
        var result = BotFrameworkExceptionJsonExtensions.FromJson(json);

        // Assert
        result.Should().NotBeNull();
        result!.Message.Should().Be(BotFrameworkExceptionJsonExtensionsTestsConstants.CamelCaseTestMessage);
        result.ErrorCode.Should().Be(BotFrameworkExceptionJsonExtensionsTestsConstants.CamelCaseTestErrorCode);
    }

    /// <summary>
    /// Verifies that deserializing malformed JSON returns null.
    /// </summary>
    public void ReturnsNull_WhenJsonIsMalformed()
    {
        // Arrange
        var malformedJson = BotFrameworkExceptionJsonExtensionsTestsConstants.MalformedJson;

        // Act
        var result = BotFrameworkExceptionJsonExtensions.FromJson(malformedJson);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that deserializing JSON with an invalid structure returns null.
    /// </summary>
    public void ReturnsNull_WhenJsonHasInvalidStructure()
    {
        // Arrange
        var invalidJson = BotFrameworkExceptionJsonExtensionsTestsConstants.InvalidStructureJson;

        // Act
        var result = BotFrameworkExceptionJsonExtensions.FromJson(invalidJson);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that attempting to deserialize null JSON returns false and a null exception.
    /// </summary>
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

    /// <summary>
    /// Verifies that attempting to deserialize null JSON through the try-pattern API throws an <see cref="ArgumentNullException"/>.
    /// </summary>
    public void ThrowsArgumentNullException_WhenJsonIsNull()
    {
        // Arrange
        string? json = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => BotFrameworkExceptionJsonExtensions.TryFromJson(json!, out _));
    }

    /// <summary>
    /// Verifies that attempting to deserialize an empty JSON string returns false and a null exception.
    /// </summary>
    public void ReturnsFalse_WhenJsonIsEmpty()
    {
        // Arrange
        var json = BotFrameworkExceptionJsonExtensionsTestsConstants.EmptyJson;

        // Act
        var result = BotFrameworkExceptionJsonExtensions.TryFromJson(json, out var exception);

        // Assert
        result.Should().BeFalse();
        exception.Should().BeNull();
    }

    /// <summary>
    /// Verifies that attempting to deserialize whitespace-only JSON returns false and a null exception.
    /// </summary>
    public void ReturnsFalse_WhenJsonIsWhitespace()
    {
        // Arrange
        var json = BotFrameworkExceptionJsonExtensionsTestsConstants.WhitespaceJson;

        // Act
        var result = BotFrameworkExceptionJsonExtensions.TryFromJson(json, out var exception);

        // Assert
        result.Should().BeFalse();
        exception.Should().BeNull();
    }

    /// <summary>
    /// Verifies that attempting to deserialize valid JSON returns true and an exception with the original message and error code.
    /// </summary>
    public void ReturnsTrueAndDeserializedException_WhenJsonIsValid()
    {
        // Arrange
        var originalException = new BotFrameworkException(BotFrameworkExceptionJsonExtensionsTestsConstants.ValidExceptionMessage, BotFrameworkExceptionJsonExtensionsTestsConstants.ValidErrorCode);
        var json = originalException.ToJson();

        // Act
        var result = BotFrameworkExceptionJsonExtensions.TryFromJson(json, out var exception);

        // Assert
        result.Should().BeTrue();
        exception.Should().NotBeNull();
        exception!.Message.Should().Be(BotFrameworkExceptionJsonExtensionsTestsConstants.ValidExceptionMessage);
        exception.ErrorCode.Should().Be(BotFrameworkExceptionJsonExtensionsTestsConstants.ValidErrorCode);
    }

    /// <summary>
    /// Verifies that attempting to deserialize malformed JSON returns false and a null exception.
    /// </summary>
    public void ReturnsFalseAndNull_WhenJsonIsMalformed()
    {
        // Arrange
        var malformedJson = BotFrameworkExceptionJsonExtensionsTestsConstants.MalformedJson;

        // Act
        var result = BotFrameworkExceptionJsonExtensions.TryFromJson(malformedJson, out var exception);

        // Assert
        result.Should().BeFalse();
        exception.Should().BeNull();
    }
}
