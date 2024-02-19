#nullable enable

using FluentAssertions;
using TelegramBotFramework.Models;
using Xunit;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Contains unit tests for the <see cref="BotConfiguration"/> class.
/// Tests configuration validation, default values, and various helper methods.
/// </summary>
public sealed class BotConfigurationTests
{
    /// <summary>
    /// Tests that a new <see cref="BotConfiguration"/> instance has correct default values for all properties.
    /// </summary>
    [Fact]
    public void BotConfiguration_DefaultValues_AreCorrect()
    {
        var config = new BotConfiguration();

        config.BotToken.Should().BeEmpty();
        config.BotUsername.Should().BeEmpty();
        config.OwnerId.Should().BeNull();
        config.DatabaseConnectionString.Should().BeEmpty();
        config.SessionTimeoutMinutes.Should().Be(30);
        config.MessageProcessingTimeoutSeconds.Should().Be(10);
        config.EnableLogging.Should().BeTrue();
        config.LogLevel.Should().Be(LogLevel.Info);
        config.MaxConcurrentRequests.Should().Be(10);
        config.EnableWebhook.Should().BeFalse();
        config.ApiKey.Should().BeNull();
        config.WebhookUrl.Should().BeNull();
        config.WebhookSecret.Should().BeNull();
        config.CustomSettings.Should().NotBeNull().And.BeEmpty();
        config.AdminIds.Should().NotBeNull().And.BeEmpty();
        config.EnableRateLimiting.Should().BeTrue();
        config.RateLimitPerMinute.Should().Be(30);
        config.LocalizationLanguage.Should().Be("en");
    }

    /// <summary>
    /// Tests that validation returns true when all required fields are properly set.
    /// </summary>
    [Fact]
    public void Validate_WithValidConfiguration_ReturnsTrue()
    {
        var config = new BotConfiguration
        {
            BotToken = "test-token-123",
            BotUsername = "TestBot",
            SessionTimeoutMinutes = 5,
            MaxConcurrentRequests = 20
        };

        var result = config.Validate();

        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that validation throws an exception when BotToken is empty.
    /// </summary>
    [Fact]
    public void Validate_WithEmptyBotToken_ThrowsException()
    {
        var config = new BotConfiguration
        {
            BotToken = "",
            BotUsername = "TestBot"
        };

        config.Invoking(c => c.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("BotToken is required");
    }

    /// <summary>
    /// Tests that validation throws an exception when BotUsername is empty.
    /// </summary>
    [Fact]
    public void Validate_WithEmptyBotUsername_ThrowsException()
    {
        var config = new BotConfiguration
        {
            BotToken = "test-token-123",
            BotUsername = ""
        };

        config.Invoking(c => c.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("BotUsername is required");
    }

    /// <summary>
    /// Tests that validation throws an exception when BotToken contains only whitespace.
    /// </summary>
    [Fact]
    public void Validate_WithWhitespaceBotToken_ThrowsException()
    {
        var config = new BotConfiguration
        {
            BotToken = "   ",
            BotUsername = "TestBot"
        };

        config.Invoking(c => c.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("BotToken is required");
    }

    /// <summary>
    /// Tests that validation throws an exception when SessionTimeoutMinutes is zero.
    /// </summary>
    [Fact]
    public void Validate_WithZeroSessionTimeout_ThrowsException()
    {
        var config = new BotConfiguration
        {
            BotToken = "test-token-123",
            BotUsername = "TestBot",
            SessionTimeoutMinutes = 0
        };

        config.Invoking(c => c.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("SessionTimeoutMinutes must be at least 1");
    }

    /// <summary>
    /// Tests that validation throws an exception when SessionTimeoutMinutes is negative.
    /// </summary>
    [Fact]
    public void Validate_WithNegativeSessionTimeout_ThrowsException()
    {
        var config = new BotConfiguration
        {
            BotToken = "test-token-123",
            BotUsername = "TestBot",
            SessionTimeoutMinutes = -5
        };

        config.Invoking(c => c.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("SessionTimeoutMinutes must be at least 1");
    }

    /// <summary>
    /// Tests that validation throws an exception when MaxConcurrentRequests is zero.
    /// </summary>
    [Fact]
    public void Validate_WithZeroMaxConcurrentRequests_ThrowsException()
    {
        var config = new BotConfiguration
        {
            BotToken = "test-token-123",
            BotUsername = "TestBot",
            MaxConcurrentRequests = 0
        };

        config.Invoking(c => c.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("MaxConcurrentRequests must be at least 1");
    }

    /// <summary>
    /// Tests that validation throws an exception when MaxConcurrentRequests is negative.
    /// </summary>
    [Fact]
    public void Validate_WithNegativeMaxConcurrentRequests_ThrowsException()
    {
        var config = new BotConfiguration
        {
            BotToken = "test-token-123",
            BotUsername = "TestBot",
            MaxConcurrentRequests = -1
        };

        config.Invoking(c => c.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("MaxConcurrentRequests must be at least 1");
    }

    /// <summary>
    /// Tests that IsAdmin returns true when checking the OwnerId.
    /// </summary>
    [Fact]
    public void IsAdmin_WithOwnerId_ReturnsTrue()
    {
        var config = new BotConfiguration
        {
            BotToken = "test-token-123",
            BotUsername = "TestBot",
            OwnerId = 12345
        };

        var isAdmin = config.IsAdmin(12345);

        isAdmin.Should().BeTrue();
    }

    /// <summary>
    /// Tests that IsAdmin returns true when checking an ID in the AdminIds list.
    /// </summary>
    [Fact]
    public void IsAdmin_WithAdminId_ReturnsTrue()
    {
        var config = new BotConfiguration
        {
            BotToken = "test-token-123",
            BotUsername = "TestBot",
            AdminIds = new List<long> { 12345, 67890 }
        };

        var isAdmin = config.IsAdmin(12345);

        isAdmin.Should().BeTrue();
    }

    /// <summary>
    /// Tests that IsAdmin returns false when checking an ID not in OwnerId or AdminIds.
    /// </summary>
    [Fact]
    public void IsAdmin_WithNonAdminId_ReturnsFalse()
    {
        var config = new BotConfiguration
        {
            BotToken = "test-token-123",
            BotUsername = "TestBot",
            OwnerId = 12345,
            AdminIds = new List<long> { 67890 }
        };

        var isAdmin = config.IsAdmin(99999);

        isAdmin.Should().BeFalse();
    }

    /// <summary>
    /// Tests that IsAdmin returns true when OwnerId is set and AdminIds is null.
    /// </summary>
    [Fact]
    public void IsAdmin_WithNullAdminIds_ReturnsFalse()
    {
        var config = new BotConfiguration
        {
            BotToken = "test-token-123",
            BotUsername = "TestBot",
            OwnerId = 12345,
            AdminIds = null
        };

        var isAdmin = config.IsAdmin(12345);

        isAdmin.Should().BeTrue();
    }

    /// <summary>
    /// Tests that AddAdmin adds a new admin to the AdminIds list.
    /// </summary>
    [Fact]
    public void AddAdmin_WithNewAdmin_AddsToList()
    {
        var config = new BotConfiguration
        {
            BotToken = "test-token-123",
            BotUsername = "TestBot",
            AdminIds = new List<long> { 12345 }
        };

        config.AddAdmin(67890);

        config.AdminIds.Should().HaveCount(2).And.Contain(67890);
    }

    /// <summary>
    /// Tests that AddAdmin does not add a duplicate admin to the AdminIds list.
    /// </summary>
    [Fact]
    public void AddAdmin_WithExistingAdmin_DoesNotAddDuplicate()
    {
        var config = new BotConfiguration
        {
            BotToken = "test-token-123",
            BotUsername = "TestBot",
            AdminIds = new List<long> { 12345 }
        };

        config.AddAdmin(12345);

        config.AdminIds.Should().HaveCount(1);
    }

    /// <summary>
    /// Tests that AddAdmin initializes the AdminIds list when it is null.
    /// </summary>
    [Fact]
    public void AddAdmin_WithNullAdminIds_InitializesList()
    {
        var config = new BotConfiguration
        {
            BotToken = "test-token-123",
            BotUsername = "TestBot",
            AdminIds = null
        };

        config.AddAdmin(12345);

        config.AdminIds.Should().NotBeNull().And.HaveCount(1).And.Contain(12345);
    }

    /// <summary>
    /// Tests that RemoveAdmin returns true and removes the admin when the admin exists in the list.
    /// </summary>
    [Fact]
    public void RemoveAdmin_WithExistingAdmin_ReturnsTrueAndRemoves()
    {
        var config = new BotConfiguration
        {
            BotToken = "test-token-123",
            BotUsername = "TestBot",
            AdminIds = new List<long> { 12345, 67890 }
        };

        var result = config.RemoveAdmin(12345);

        result.Should().BeTrue();
        config.AdminIds.Should().HaveCount(1).And.NotContain(12345);
    }

    /// <summary>
    /// Tests that RemoveAdmin returns false when the admin does not exist in the list.
    /// </summary>
    [Fact]
    public void RemoveAdmin_WithNonExistingAdmin_ReturnsFalse()
    {
        var config = new BotConfiguration
        {
            BotToken = "test-token-123",
            BotUsername = "TestBot",
            AdminIds = new List<long> { 12345 }
        };

        var result = config.RemoveAdmin(99999);

        result.Should().BeFalse();
        config.AdminIds.Should().HaveCount(1);
    }

    /// <summary>
    /// Tests that RemoveAdmin returns false when AdminIds is null.
    /// </summary>
    [Fact]
    public void RemoveAdmin_WithNullAdminIds_ReturnsFalse()
    {
        var config = new BotConfiguration
        {
            BotToken = "test-token-123",
            BotUsername = "TestBot",
            AdminIds = null
        };

        var result = config.RemoveAdmin(12345);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that GetSessionTimeout returns the correct TimeSpan based on SessionTimeoutMinutes.
    /// </summary>
    [Fact]
    public void GetSessionTimeout_ReturnsCorrectTimeSpan()
    {
        var config = new BotConfiguration
        {
            SessionTimeoutMinutes = 45
        };

        var timeout = config.GetSessionTimeout();

        timeout.Should().Be(TimeSpan.FromMinutes(45));
    }

    /// <summary>
    /// Tests that GetCustomSetting returns the correct value for an existing key.
    /// </summary>
    [Fact]
    public void GetCustomSetting_WithExistingKey_ReturnsValue()
    {
        var config = new BotConfiguration
        {
            CustomSettings = new Dictionary<string, string>
            {
                { "api_key", "secret123" },
                { "endpoint", "https://api.example.com" }
            }
        };

        var value = config.GetCustomSetting("api_key");

        value.Should().Be("secret123");
    }

    /// <summary>
    /// Tests that GetCustomSetting returns null for a non-existing key.
    /// </summary>
    [Fact]
    public void GetCustomSetting_WithNonExistingKey_ReturnsNull()
    {
        var config = new BotConfiguration
        {
            CustomSettings = new Dictionary<string, string>
            {
                { "api_key", "secret123" }
            }
        };

        var value = config.GetCustomSetting("nonexistent");

        value.Should().BeNull();
    }

    /// <summary>
    /// Tests that GetCustomSetting returns null when CustomSettings is null.
    /// </summary>
    [Fact]
    public void GetCustomSetting_WithNullCustomSettings_ReturnsNull()
    {
        var config = new BotConfiguration
        {
            CustomSettings = null
        };

        var value = config.GetCustomSetting("any_key");

        value.Should().BeNull();
    }

    /// <summary>
    /// Tests that SetCustomSetting adds a new key-value pair when the key does not exist.
    /// </summary>
    [Fact]
    public void SetCustomSetting_WithNewKey_AddsValue()
    {
        var config = new BotConfiguration();

        config.SetCustomSetting("new_key", "new_value");

        config.CustomSettings.Should().HaveCount(1).And.ContainKey("new_key").WhoseValue.Should().Be("new_value");
    }

    /// <summary>
    /// Tests that SetCustomSetting updates the value when the key already exists.
    /// </summary>
    [Fact]
    public void SetCustomSetting_WithExistingKey_UpdatesValue()
    {
        var config = new BotConfiguration
        {
            CustomSettings = new Dictionary<string, string> { { "key", "old_value" } }
        };

        config.SetCustomSetting("key", "new_value");

        config.CustomSettings.Should().HaveCount(1).And.ContainKey("key").WhoseValue.Should().Be("new_value");
    }

    /// <summary>
    /// Tests that SetCustomSetting initializes the CustomSettings dictionary when it is null.
    /// </summary>
    [Fact]
    public void SetCustomSetting_WithNullCustomSettings_InitializesDictionary()
    {
        var config = new BotConfiguration();

        config.SetCustomSetting("key", "value");

        config.CustomSettings.Should().NotBeNull().And.HaveCount(1).And.ContainKey("key");
    }
}
