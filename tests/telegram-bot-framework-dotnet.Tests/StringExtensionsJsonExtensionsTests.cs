using System;
using System.Text.Json;
using TelegramBotFramework.Utilities;
using Xunit;

namespace TelegramBotFramework.Tests;

public sealed class StringExtensionsJsonExtensionsTests
{
    [Fact]
    public void ToJson_WithNonNullString_ReturnsJsonString()
    {
        // Arrange
        const string input = "hello world";

        // Act
        string json = input.ToJson();

        // Assert
        Assert.Equal("\"hello world\"", json);
    }

    [Fact]
    public void ToJson_WithIndentation_ReturnsIndentedJson()
    {
        // Arrange
        const string input = "indented";

        // Act
        string json = input.ToJson(indented: true);

        // Assert
        // For a simple string, indentation does not add whitespace, but the option must be respected.
        Assert.Equal("\"indented\"", json);
    }

    [Fact]
    public void ToJson_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        string? input = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => input!.ToJson());
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsString()
    {
        // Arrange
        const string json = "\"sample\"";

        // Act
        string? result = StringExtensionsJsonExtensions.FromJson(json);

        // Assert
        Assert.Equal("sample", result);
    }

    [Fact]
    public void FromJson_EmptyString_ReturnsNull()
    {
        // Arrange
        const string json = "";

        // Act
        string? result = StringExtensionsJsonExtensions.FromJson(json);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FromJson_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        string? json = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => StringExtensionsJsonExtensions.FromJson(json!));
    }

    [Fact]
    public void FromJson_InvalidJson_ThrowsJsonException()
    {
        // Arrange
        const string json = "not a json string";

        // Act & Assert
        Assert.Throws<JsonException>(() => StringExtensionsJsonExtensions.FromJson(json));
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndValue()
    {
        // Arrange
        const string json = "\"valid\"";

        // Act
        bool success = StringExtensionsJsonExtensions.TryFromJson(json, out string? value);

        // Assert
        Assert.True(success);
        Assert.Equal("valid", value);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        const string json = "invalid json";

        // Act
        bool success = StringExtensionsJsonExtensions.TryFromJson(json, out string? value);

        // Assert
        Assert.False(success);
        Assert.Null(value);
    }

    [Fact]
    public void TryFromJson_EmptyString_ReturnsTrueAndNull()
    {
        // Arrange
        const string json = "";

        // Act
        bool success = StringExtensionsJsonExtensions.TryFromJson(json, out string? value);

        // Assert
        Assert.True(success);
        Assert.Null(value);
    }

    [Fact]
    public void TryFromJson_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        string? json = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => StringExtensionsJsonExtensions.TryFromJson(json!, out _));
    }
}
