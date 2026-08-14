using System;
using System.Text.Json;
using TelegramBotFramework.Models;
using Xunit;

namespace TelegramBotFramework.Tests;

public class BotUserJsonExtensionsTests
{
    [Fact]
    public void ToJson_WithValidObject_ReturnsNonNullString()
    {
        // Arrange
        var user = new BotUser();

        // Act
        var result = user.ToJson();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ToJson_WithNullInput_ThrowsArgumentNullException()
    {
        // Arrange
        BotUser? user = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => user.ToJson());
    }

    [Fact]
    public void ToJson_WithIndentedTrue_ReturnsFormattedJson()
    {
        // Arrange
        var user = new BotUser();

        // Act
        var result = user.ToJson(indented: true);

        // Assert
        Assert.Contains("\n", result);
        Assert.Contains("  ", result);
    }

    [Fact]
    public void FromJson_WithValidJson_ReturnsBotUser()
    {
        // Arrange
        var json = "{}";

        // Act
        var result = BotUserJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void FromJson_WithNullInput_ReturnsNull()
    {
        // Arrange
        string? json = null;

        // Act
        var result = BotUserJsonExtensions.FromJson(json);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FromJson_WithEmptyString_ReturnsNull()
    {
        // Arrange
        var json = "   ";

        // Act
        var result = BotUserJsonExtensions.FromJson(json);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FromJson_WithInvalidJson_ThrowsJsonException()
    {
        // Arrange
        var json = "{ invalid }";

        // Act & Assert
        Assert.Throws<JsonException>(() => BotUserJsonExtensions.FromJson(json));
    }

    [Fact]
    public void TryFromJson_WithValidJson_ReturnsTrueAndObject()
    {
        // Arrange
        var json = "{}";

        // Act
        var result = BotUserJsonExtensions.TryFromJson(json, out var user);

        // Assert
        Assert.True(result);
        Assert.NotNull(user);
    }

    [Fact]
    public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var json = "invalid";

        // Act
        var result = BotUserJsonExtensions.TryFromJson(json, out var user);

        // Assert
        Assert.False(result);
        Assert.Null(user);
    }
}
