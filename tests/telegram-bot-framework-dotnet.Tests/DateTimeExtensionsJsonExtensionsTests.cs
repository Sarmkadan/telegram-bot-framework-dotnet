using System;
using System.Text.Json;
using TelegramBotFramework.Utilities;
using Xunit;

namespace TelegramBotFramework.Tests;

public class DateTimeExtensionsJsonExtensionsTests
{
    [Fact]
    public void ToJson_ShouldSerializeDateTimeCorrectly()
    {
        // Arrange
        var date = new DateTime(2023, 10, 5, 14, 30, 0, DateTimeKind.Utc);

        // Act
        var json = date.ToJson();

        // Assert
        Assert.NotNull(json);
        Assert.Contains("2023-10-05T14:30:00Z", json);
    }

    [Fact]
    public void ToJson_Indented_ShouldFormatJson()
    {
        // Arrange
        var date = DateTime.Now;

        // Act
        var json = date.ToJson(indented: true);

        // Assert
        Assert.NotNull(json);
        // Verify it is valid JSON by deserializing it back
        var parsed = JsonSerializer.Deserialize<DateTime>(json);
        Assert.Equal(date, parsed);
    }

    [Fact]
    public void FromJson_ValidJson_ShouldReturnDateTime()
    {
        // Arrange
        var json = "\"2023-10-05T14:30:00Z\"";
        var expected = new DateTime(2023, 10, 5, 14, 30, 0, DateTimeKind.Utc);

        // Act
        var result = DateTimeExtensionsJsonExtensions.FromJson(json);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void FromJson_NullOrEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        string? nullJson = null;
        string emptyJson = string.Empty;
        string whitespaceJson = "   ";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => DateTimeExtensionsJsonExtensions.FromJson(nullJson!));
        Assert.Throws<ArgumentException>(() => DateTimeExtensionsJsonExtensions.FromJson(emptyJson));
        Assert.Throws<ArgumentException>(() => DateTimeExtensionsJsonExtensions.FromJson(whitespaceJson));
    }

    [Fact]
    public void FromJson_InvalidJson_ShouldThrowJsonException()
    {
        // Arrange
        var invalidJson = "not-a-date";

        // Act & Assert
        Assert.Throws<JsonException>(() => DateTimeExtensionsJsonExtensions.FromJson(invalidJson));
    }

    [Fact]
    public void TryFromJson_ValidJson_ShouldReturnTrueAndDateTime()
    {
        // Arrange
        var json = "\"2023-10-05T14:30:00Z\"";
        var expected = new DateTime(2023, 10, 5, 14, 30, 0, DateTimeKind.Utc);

        // Act
        var result = DateTimeExtensionsJsonExtensions.TryFromJson(json, out var date);

        // Assert
        Assert.True(result);
        Assert.Equal(expected, date);
    }

    [Fact]
    public void TryFromJson_NullJson_ShouldReturnFalse()
    {
        // Act
        var result = DateTimeExtensionsJsonExtensions.TryFromJson(null!, out var date);

        // Assert
        Assert.False(result);
        Assert.Equal(default, date);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ShouldReturnFalse()
    {
        // Arrange
        var invalidJson = "invalid";

        // Act
        var result = DateTimeExtensionsJsonExtensions.TryFromJson(invalidJson, out var date);

        // Assert
        Assert.False(result);
        Assert.Equal(default, date);
    }
}
