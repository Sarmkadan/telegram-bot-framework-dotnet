#nullable enable

using FluentAssertions;
using TelegramBotFramework.Models;
using Xunit;
using static TelegramBotFramework.Tests.BotConfigurationTestsConstants;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Contains unit tests for the <see cref="BotConfiguration"/> class.
/// Tests configuration validation, default values, and various helper methods.
/// </summary>
public sealed class BotConfigurationTests : IBotConfigurationTests
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
        config.SessionTimeoutMinutes.Should().Be(DefaultSessionTimeoutMinutes);
        config.MessageProcessingTimeoutSeconds.Should().Be(DefaultMessageProcessingTimeoutSeconds);
        config.EnableLogging.Should().BeTrue();
        config.LogLevel.Should().Be(LogLevel.Info);
        config.MaxConcurrentRequests.Should().Be(DefaultMaxConcurrentRequests);
        config.EnableWebhook.Should().BeFalse();
        config.ApiKey.Should().BeNull();
        config.WebhookUrl.Should().BeNull();
        config.WebhookSecret.Should().BeNull();
        config.CustomSettings.Should().NotBeNull().And.BeEmpty();
        config.AdminIds.Should().NotBeNull().And.BeEmpty();
        config.EnableRateLimiting.Should().BeTrue();
        config.RateLimitPerMinute.Should().Be(DefaultRateLimitPerMinute);
        config.LocalizationLanguage.Should().Be(DefaultLocalizationLanguage);
    }

    /// <summary>
    /// Tests that validation returns true when all required fields are properly set.
    /// </summary>
    [Fact]
    public void Validate_WithValidConfiguration_ReturnsTrue()
    {
        var config = new BotConfiguration
        {
            BotToken = TestBotToken,
            BotUsername = TestBotUsername,
            SessionTimeoutMinutes = ValidSessionTimeoutMinutes,
            MaxConcurrentRequests = ValidMaxConcurrentRequests
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
            BotToken = EmptyValue,
            BotUsername = TestBotUsername
        };

        config.Invoking(c => c.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage(BotTokenRequiredMessage);
    }

    /// <summary>
    /// Tests that validation throws an exception when BotUsername is empty.
    /// </summary>
    [Fact]
    public void Validate_WithEmptyBotUsername_ThrowsException()
    {
        var config = new BotConfiguration
        {
            BotToken = TestBotToken,
            BotUsername = EmptyValue
        };

        config.Invoking(c => c.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage(BotUsernameRequiredMessage);
    }

    /// <summary>
    /// Tests that validation throws an exception when BotToken contains only whitespace.
    /// </summary>
    [Fact]
    public void Validate_WithWhitespaceBotToken_ThrowsException()
    {
        var config = new BotConfiguration
        {
            BotToken = WhitespaceValue,
            BotUsername = TestBotUsername
        };

        config.Invoking(c => c.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage(BotTokenRequiredMessage);
    }

    /// <summary>
    /// Tests that validation throws an exception when SessionTimeoutMinutes is zero.
    /// </summary>
    [Fact]
    public void Validate_WithZeroSessionTimeout_ThrowsException()
    {
        var config = new BotConfiguration
        {
            BotToken = TestBotToken,
            BotUsername = TestBotUsername,
            SessionTimeoutMinutes = ZeroValue
        };

        config.Invoking(c => c.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage(SessionTimeoutRequiredMessage);
    }

    /// <summary>
    /// Tests that validation throws an exception when SessionTimeoutMinutes is negative.
    /// </summary>
    [Fact]
    public void Validate_WithNegativeSessionTimeout_ThrowsException()
    {
        var config = new BotConfiguration
        {
            BotToken = TestBotToken,
            BotUsername = TestBotUsername,
            SessionTimeoutMinutes = NegativeSessionTimeoutMinutes
        };

        config.Invoking(c => c.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage(SessionTimeoutRequiredMessage);
    }

    /// <summary>
    /// Tests that validation throws an exception when MaxConcurrentRequests is zero.
    /// </summary>
    [Fact]
    public void Validate_WithZeroMaxConcurrentRequests_ThrowsException()
    {
        var config = new BotConfiguration
        {
            BotToken = TestBotToken,
            BotUsername = TestBotUsername,
            MaxConcurrentRequests = ZeroValue
        };

        config.Invoking(c => c.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage(MaxConcurrentRequestsRequiredMessage);
    }

    /// <summary>
    /// Tests that validation throws an exception when MaxConcurrentRequests is negative.
    /// </summary>
    [Fact]
    public void Validate_WithNegativeMaxConcurrentRequests_ThrowsException()
    {
        var config = new BotConfiguration
        {
            BotToken = TestBotToken,
            BotUsername = TestBotUsername,
            MaxConcurrentRequests = NegativeMaxConcurrentRequests
        };

        config.Invoking(c => c.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage(MaxConcurrentRequestsRequiredMessage);
    }

    /// <summary>
    /// Tests that IsAdmin returns true when checking the OwnerId.
    /// </summary>
    [Fact]
    public void IsAdmin_WithOwnerId_ReturnsTrue()
    {
        var config = new BotConfiguration
        {
            BotToken = TestBotToken,
            BotUsername = TestBotUsername,
            OwnerId = TestOwnerId
        };

        var isAdmin = config.IsAdmin(TestOwnerId);

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
            BotToken = TestBotToken,
            BotUsername = TestBotUsername,
            AdminIds = new List<long> { TestOwnerId, TestAdminId }
        };

        var isAdmin = config.IsAdmin(TestOwnerId);

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
            BotToken = TestBotToken,
            BotUsername = TestBotUsername,
            OwnerId = TestOwnerId,
            AdminIds = new List<long> { TestAdminId }
        };

        var isAdmin = config.IsAdmin(NonAdminId);

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
            BotToken = TestBotToken,
            BotUsername = TestBotUsername,
            OwnerId = TestOwnerId,
            AdminIds = null
        };

        var isAdmin = config.IsAdmin(TestOwnerId);

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
            BotToken = TestBotToken,
            BotUsername = TestBotUsername,
            AdminIds = new List<long> { TestOwnerId }
        };

        config.AddAdmin(TestAdminId);

        config.AdminIds.Should().HaveCount(ExpectedTwoItemCount).And.Contain(TestAdminId);
    }

    /// <summary>
    /// Tests that AddAdmin does not add a duplicate admin to the AdminIds list.
    /// </summary>
    [Fact]
    public void AddAdmin_WithExistingAdmin_DoesNotAddDuplicate()
    {
        var config = new BotConfiguration
        {
            BotToken = TestBotToken,
            BotUsername = TestBotUsername,
            AdminIds = new List<long> { TestOwnerId }
        };

        config.AddAdmin(TestOwnerId);

        config.AdminIds.Should().HaveCount(ExpectedSingleItemCount);
    }

    /// <summary>
    /// Tests that AddAdmin initializes the AdminIds list when it is null.
    /// </summary>
    [Fact]
    public void AddAdmin_WithNullAdminIds_InitializesList()
    {
        var config = new BotConfiguration
        {
            BotToken = TestBotToken,
            BotUsername = TestBotUsername,
            AdminIds = null
        };

        config.AddAdmin(TestOwnerId);

        config.AdminIds.Should().NotBeNull().And.HaveCount(ExpectedSingleItemCount).And.Contain(TestOwnerId);
    }

    /// <summary>
    /// Tests that RemoveAdmin returns true and removes the admin when the admin exists in the list.
    /// </summary>
    [Fact]
    public void RemoveAdmin_WithExistingAdmin_ReturnsTrueAndRemoves()
    {
        var config = new BotConfiguration
        {
            BotToken = TestBotToken,
            BotUsername = TestBotUsername,
            AdminIds = new List<long> { TestOwnerId, TestAdminId }
        };

        var result = config.RemoveAdmin(TestOwnerId);

        result.Should().BeTrue();
        config.AdminIds.Should().HaveCount(ExpectedSingleItemCount).And.NotContain(TestOwnerId);
    }

    /// <summary>
    /// Tests that RemoveAdmin returns false when the admin does not exist in the list.
    /// </summary>
    [Fact]
    public void RemoveAdmin_WithNonExistingAdmin_ReturnsFalse()
    {
        var config = new BotConfiguration
        {
            BotToken = TestBotToken,
            BotUsername = TestBotUsername,
            AdminIds = new List<long> { TestOwnerId }
        };

        var result = config.RemoveAdmin(NonAdminId);

        result.Should().BeFalse();
        config.AdminIds.Should().HaveCount(ExpectedSingleItemCount);
    }

    /// <summary>
    /// Tests that RemoveAdmin returns false when AdminIds is null.
    /// </summary>
    [Fact]
    public void RemoveAdmin_WithNullAdminIds_ReturnsFalse()
    {
        var config = new BotConfiguration
        {
            BotToken = TestBotToken,
            BotUsername = TestBotUsername,
            AdminIds = null
        };

        var result = config.RemoveAdmin(TestOwnerId);

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
            SessionTimeoutMinutes = TestSessionTimeoutMinutes
        };

        var timeout = config.GetSessionTimeout();

        timeout.Should().Be(TimeSpan.FromMinutes(TestSessionTimeoutMinutes));
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
                { ApiKeySettingKey, ApiKeySettingValue },
                { EndpointSettingKey, EndpointSettingValue }
            }
        };

        var value = config.GetCustomSetting(ApiKeySettingKey);

        value.Should().Be(ApiKeySettingValue);
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
                { ApiKeySettingKey, ApiKeySettingValue }
            }
        };

        var value = config.GetCustomSetting(NonexistentSettingKey);

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

        var value = config.GetCustomSetting(ArbitrarySettingKey);

        value.Should().BeNull();
    }

    /// <summary>
    /// Tests that SetCustomSetting adds a new key-value pair when the key does not exist.
    /// </summary>
    [Fact]
    public void SetCustomSetting_WithNewKey_AddsValue()
    {
        var config = new BotConfiguration();

        config.SetCustomSetting(NewSettingKey, NewSettingValue);

        config.CustomSettings.Should().HaveCount(ExpectedSingleItemCount).And.ContainKey(NewSettingKey)
            .WhoseValue.Should().Be(NewSettingValue);
    }

    /// <summary>
    /// Tests that SetCustomSetting updates the value when the key already exists.
    /// </summary>
    [Fact]
    public void SetCustomSetting_WithExistingKey_UpdatesValue()
    {
        var config = new BotConfiguration
        {
            CustomSettings = new Dictionary<string, string> { { ExistingSettingKey, OldSettingValue } }
        };

        config.SetCustomSetting(ExistingSettingKey, NewSettingValue);

        config.CustomSettings.Should().HaveCount(ExpectedSingleItemCount).And.ContainKey(ExistingSettingKey)
            .WhoseValue.Should().Be(NewSettingValue);
    }

    /// <summary>
    /// Tests that SetCustomSetting initializes the CustomSettings dictionary when it is null.
    /// </summary>
    [Fact]
    public void SetCustomSetting_WithNullCustomSettings_InitializesDictionary()
    {
        var config = new BotConfiguration();

        config.SetCustomSetting(ExistingSettingKey, SettingValue);

        config.CustomSettings.Should().NotBeNull().And.HaveCount(ExpectedSingleItemCount).And.ContainKey(ExistingSettingKey);
    }
}
