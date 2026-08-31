using System;
using FluentAssertions;
using TelegramBotFramework.Exceptions;
using Xunit;

namespace TelegramBotFramework.Tests;

public class BotFrameworkExceptionJsonExtensionsTests : IBotFrameworkExceptionJsonExtensionsTests
{
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

    public void ThrowsArgumentNullException_WhenValueIsNull()
    {
        // Arrange
        BotFrameworkException? exception = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception!.ToJson());
    }

    public void SerializesErrorCodeProperty()
    {
        // Arrange
        var exception = new BotFrameworkException(BotFrameworkExceptionJsonExtensionsTestsConstants.SessionFailedMessage, BotFrameworkExceptionJsonExtensionsTestsConstants.SessionErrorCode);

        // Act
        var json = exception.ToJson();

        // Assert
        json.Should().Contain(BotFrameworkExceptionJsonExtensionsTestsConstants.SessionErrorJsonProperty);
    }

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

    public void ReturnsNull_WhenJsonIsNull()
    {
        // Arrange & Act
        var result = BotFrameworkExceptionJsonExtensions.FromJson(null);

        // Assert
        result.Should().BeNull();
    }

    public void ReturnsNull_WhenJsonIsEmpty()
    {
        // Arrange & Act
        var result = BotFrameworkExceptionJsonExtensions.FromJson(BotFrameworkExceptionJsonExtensionsTestsConstants.EmptyJson);

        // Assert
        result.Should().BeNull();
    }

    public void ReturnsNull_WhenJsonIsWhitespace()
    {
        // Arrange & Act
        var result = BotFrameworkExceptionJsonExtensions.FromJson(BotFrameworkExceptionJsonExtensionsTestsConstants.WhitespaceJson);

        // Assert
        result.Should().BeNull();
    }

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

    public void ReturnsNull_WhenJsonIsMalformed()
    {
        // Arrange
        var malformedJson = BotFrameworkExceptionJsonExtensionsTestsConstants.MalformedJson;

        // Act
        var result = BotFrameworkExceptionJsonExtensions.FromJson(malformedJson);

        // Assert
        result.Should().BeNull();
    }

    public void ReturnsNull_WhenJsonHasInvalidStructure()
    {
        // Arrange
        var invalidJson = BotFrameworkExceptionJsonExtensionsTestsConstants.InvalidStructureJson;

        // Act
        var result = BotFrameworkExceptionJsonExtensions.FromJson(invalidJson);

        // Assert
        result.Should().BeNull();
    }

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

    public void ThrowsArgumentNullException_WhenJsonIsNull()
    {
        // Arrange
        string? json = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => BotFrameworkExceptionJsonExtensions.TryFromJson(json!, out _));
    }

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
