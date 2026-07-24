#nullable enable

using System;
using FluentAssertions;
using Moq;
using TelegramBotFramework.Integration;
using Xunit;

namespace TelegramBotFramework.Tests.Integration;

/// <summary>
/// Tests for <see cref="PollingStrategyJsonExtensions"/> serialization and deserialization methods.
/// </summary>
public sealed class PollingStrategyJsonExtensionsTests
{
    [Fact]
    public void ToJson_WithValidPollingStrategy_ReturnsJsonString()
    {
        // Arrange
        var mockApiClient = new Mock<ITelegramApiClient>();
        var mockOffsetStore = new Mock<IUpdateOffsetStore>();
        var strategy = new PollingStrategy(mockApiClient.Object, mockOffsetStore.Object);

        // Act
        var json = strategy.ToJson();

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        json.Should().BeOfType<string>();
    }

    [Fact]
    public void ToJson_WithIndentedTrue_ReturnsFormattedJson()
    {
        // Arrange
        var mockApiClient = new Mock<ITelegramApiClient>();
        var mockOffsetStore = new Mock<IUpdateOffsetStore>();
        var strategy = new PollingStrategy(mockApiClient.Object, mockOffsetStore.Object);

        // Act
        var json = strategy.ToJson(indented: true);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        json.Should().BeOfType<string>();
    }

    [Fact]
    public void ToJson_WithIndentedFalse_ReturnsCompactJson()
    {
        // Arrange
        var mockApiClient = new Mock<ITelegramApiClient>();
        var mockOffsetStore = new Mock<IUpdateOffsetStore>();
        var strategy = new PollingStrategy(mockApiClient.Object, mockOffsetStore.Object);

        // Act
        var json = strategy.ToJson(indented: false);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        json.Should().BeOfType<string>();
    }

    [Fact]
    public void ToJson_WithNullStrategy_ThrowsArgumentNullException()
    {
        // Arrange
        PollingStrategy? strategy = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => strategy!.ToJson());
    }

    [Fact]
    public void FromJson_WithNullJson_ReturnsNull()
    {
        // Arrange
        string? json = null;

        // Act
        var result = PollingStrategyJsonExtensions.FromJson(json);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FromJson_WithEmptyString_ReturnsNull()
    {
        // Arrange
        var json = string.Empty;

        // Act
        var result = PollingStrategyJsonExtensions.FromJson(json);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FromJson_WithWhitespaceString_ReturnsNull()
    {
        // Arrange
        var json = "   \n\t  ";

        // Act
        var result = PollingStrategyJsonExtensions.FromJson(json);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_WithNullJson_ReturnsFalseAndSetsNullValue()
    {
        // Arrange
        string? json = null;
        PollingStrategy? result = null;

        // Act
        var success = PollingStrategyJsonExtensions.TryFromJson(json, out result);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_WithEmptyString_ReturnsFalseAndSetsNullValue()
    {
        // Arrange
        var json = string.Empty;
        PollingStrategy? result = null;

        // Act
        var success = PollingStrategyJsonExtensions.TryFromJson(json, out result);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_WithWhitespaceString_ReturnsFalseAndSetsNullValue()
    {
        // Arrange
        var json = "   \n\t  ";
        PollingStrategy? result = null;

        // Act
        var success = PollingStrategyJsonExtensions.TryFromJson(json, out result);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }
}