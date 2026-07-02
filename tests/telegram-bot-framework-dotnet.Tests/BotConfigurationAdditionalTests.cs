#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using TelegramBotFramework.Models;
using Xunit;

namespace TelegramBotFramework.Tests;

public sealed class BotConfigurationAdditionalTests
{
    [Fact]
    public void BotConfiguration_WithNullAdminIds_ListIsInitialized()
    {
        // Arrange
        var config = new BotConfiguration
        {
            BotToken = "test-token",
            BotUsername = "TestBot",
            AdminIds = null
        };

        // Act
        config.AddAdmin(12345);

        // Assert
        config.AdminIds.Should().NotBeNull();
        config.AdminIds.Should().HaveCount(1);
        config.AdminIds.Should().Contain(12345);
    }

    [Fact]
    public void BotConfiguration_WithNullCustomSettings_DictionaryIsInitialized()
    {
        // Arrange
        var config = new BotConfiguration
        {
            BotToken = "test-token",
            BotUsername = "TestBot",
            CustomSettings = null
        };

        // Act
        config.SetCustomSetting("key", "value");

        // Assert
        config.CustomSettings.Should().NotBeNull();
        config.CustomSettings.Should().HaveCount(1);
        config.CustomSettings.Should().ContainKey("key").WhoseValue.Should().Be("value");
    }

    [Fact]
    public void BotConfiguration_WithEmptyCustomSettings_DictionaryIsInitialized()
    {
        // Arrange
        var config = new BotConfiguration
        {
            BotToken = "test-token",
            BotUsername = "TestBot",
            CustomSettings = new Dictionary<string, string>()
        };

        // Act
        config.SetCustomSetting("key", "value");

        // Assert
        config.CustomSettings.Should().NotBeNull();
        config.CustomSettings.Should().HaveCount(1);
        config.CustomSettings.Should().ContainKey("key").WhoseValue.Should().Be("value");
    }

    [Fact]
    public void IsAdmin_WithEmptyAdminIds_ReturnsFalse()
    {
        // Arrange
        var config = new BotConfiguration
        {
            BotToken = "test-token",
            BotUsername = "TestBot",
            AdminIds = new List<long>()
        };

        // Act
        var result = config.IsAdmin(12345);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAdmin_WithEmptyAdminIdsAndOwnerId_ReturnsTrueForOwner()
    {
        // Arrange
        var config = new BotConfiguration
        {
            BotToken = "test-token",
            BotUsername = "TestBot",
            OwnerId = 12345,
            AdminIds = new List<long>()
        };

        // Act
        var result = config.IsAdmin(12345);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetSessionTimeout_WithDefaultValue_Returns30Minutes()
    {
        // Arrange
        var config = new BotConfiguration
        {
            BotToken = "test-token",
            BotUsername = "TestBot"
        };

        // Act
        var timeout = config.GetSessionTimeout();

        // Assert
        timeout.Should().Be(TimeSpan.FromMinutes(30));
    }

    [Fact]
    public void GetSessionTimeout_WithCustomValue_ReturnsCorrectTimeSpan()
    {
        // Arrange
        var config = new BotConfiguration
        {
            BotToken = "test-token",
            BotUsername = "TestBot",
            SessionTimeoutMinutes = 60
        };

        // Act
        var timeout = config.GetSessionTimeout();

        // Assert
        timeout.Should().Be(TimeSpan.FromMinutes(60));
    }

    [Fact]
    public void Validate_WithWhitespaceBotUsername_ThrowsException()
    {
        // Arrange
        var config = new BotConfiguration
        {
            BotToken = "test-token-123",
            BotUsername = " "
        };

        // Act & Assert
        config.Invoking(c => c.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("BotUsername is required");
    }

    [Fact]
    public void Validate_WithWhitespaceBotToken_ThrowsException()
    {
        // Arrange
        var config = new BotConfiguration
        {
            BotToken = " ",
            BotUsername = "TestBot"
        };

        // Act & Assert
        config.Invoking(c => c.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("BotToken is required");
    }

    [Fact]
    public void Validate_WithSingleCharacterBotToken_ThrowsException()
    {
        // Arrange
        var config = new BotConfiguration
        {
            BotToken = "x",
            BotUsername = "TestBot"
        };

        // Act & Assert
        config.Invoking(c => c.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("BotToken is required");
    }

    [Fact]
    public void Validate_WithMaxConcurrentRequests_ReturnsTrue()
    {
        // Arrange
        var config = new BotConfiguration
        {
            BotToken = "test-token-123",
            BotUsername = "TestBot",
            MaxConcurrentRequests = 100
        };

        // Act
        var result = config.Validate();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithOneMaxConcurrentRequests_ReturnsTrue()
    {
        // Arrange
        var config = new BotConfiguration
        {
            BotToken = "test-token-123",
            BotUsername = "TestBot",
            MaxConcurrentRequests = 1
        };

        // Act
        var result = config.Validate();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithOneSessionTimeout_ReturnsTrue()
    {
        // Arrange
        var config = new BotConfiguration
        {
            BotToken = "test-token-123",
            BotUsername = "TestBot",
            SessionTimeoutMinutes = 1
        };

        // Act
        var result = config.Validate();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SetCustomSetting_OverwritesExistingValue()
    {
        // Arrange
        var config = new BotConfiguration
        {
            CustomSettings = new Dictionary<string, string> { { "key", "old_value" } }
        };

        // Act
        config.SetCustomSetting("key", "new_value");

        // Assert
        config.CustomSettings.Should().HaveCount(1);
        config.CustomSettings["key"].Should().Be("new_value");
    }

    [Fact]
    public void RemoveAdmin_WithEmptyAdminIds_ReturnsFalse()
    {
        // Arrange
        var config = new BotConfiguration
        {
            BotToken = "test-token",
            BotUsername = "TestBot",
            AdminIds = new List<long>()
        };

        // Act
        var result = config.RemoveAdmin(12345);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void RemoveAdmin_RemovesOnlySpecifiedAdmin()
    {
        // Arrange
        var config = new BotConfiguration
        {
            BotToken = "test-token",
            BotUsername = "TestBot",
            AdminIds = new List<long> { 111, 222, 333 }
        };

        // Act
        var result = config.RemoveAdmin(222);

        // Assert
        result.Should().BeTrue();
        config.AdminIds.Should().HaveCount(2);
        config.AdminIds.Should().NotContain(222);
        config.AdminIds.Should().Contain(111);
        config.AdminIds.Should().Contain(333);
    }
}
