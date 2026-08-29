#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using static TelegramBotFramework.Tests.BotConfigurationAdditionalTestsConstants;
using TelegramBotFramework.Models;
using Xunit;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Contains additional test cases for <see cref="BotConfiguration"/> class functionality.
/// Focuses on edge cases, null handling, and validation scenarios not covered in main test classes.
/// </summary>
public sealed class BotConfigurationAdditionalTests
{
    /// <summary>
    /// Tests that <see cref="BotConfiguration.AddAdmin(long)"/> properly initializes the AdminIds list when it's null.
    /// Ensures the list is created and the admin is added successfully.
    /// </summary>
    [Fact]
    public void BotConfiguration_WithNullAdminIds_ListIsInitialized()
    {
        // Arrange
        var config = new BotConfiguration
        {
            BotToken = TestBotToken,
            BotUsername = TestBotUsername,
            AdminIds = null
        };

        // Act
        config.AddAdmin(TestAdminId);

        // Assert
        config.AdminIds.Should().NotBeNull();
        config.AdminIds.Should().HaveCount(SingleItemCount);
        config.AdminIds.Should().Contain(TestAdminId);
    }

    /// <summary>
    /// Tests that <see cref="BotConfiguration.SetCustomSetting(string, string)"/> properly initializes the CustomSettings dictionary when it's null.
    /// Ensures the dictionary is created and the custom setting is added successfully.
    /// </summary>
    [Fact]
    public void BotConfiguration_WithNullCustomSettings_DictionaryIsInitialized()
    {
        // Arrange
        var config = new BotConfiguration
        {
            BotToken = TestBotToken,
            BotUsername = TestBotUsername,
            CustomSettings = null
        };

        // Act
        config.SetCustomSetting(CustomSettingKey, CustomSettingValue);

        // Assert
        config.CustomSettings.Should().NotBeNull();
        config.CustomSettings.Should().HaveCount(SingleItemCount);
        config.CustomSettings.Should().ContainKey(CustomSettingKey).WhoseValue.Should().Be(CustomSettingValue);
    }

    /// <summary>
    /// Tests that <see cref="BotConfiguration.SetCustomSetting(string, string)"/> properly handles an empty CustomSettings dictionary.
    /// Ensures the dictionary remains initialized and new settings can be added.
    /// </summary>
    [Fact]
    public void BotConfiguration_WithEmptyCustomSettings_DictionaryIsInitialized()
    {
        // Arrange
        var config = new BotConfiguration
        {
            BotToken = TestBotToken,
            BotUsername = TestBotUsername,
            CustomSettings = new Dictionary<string, string>()
        };

        // Act
        config.SetCustomSetting(CustomSettingKey, CustomSettingValue);

        // Assert
        config.CustomSettings.Should().NotBeNull();
        config.CustomSettings.Should().HaveCount(SingleItemCount);
        config.CustomSettings.Should().ContainKey(CustomSettingKey).WhoseValue.Should().Be(CustomSettingValue);
    }

    /// <summary>
    /// Tests that <see cref="BotConfiguration.IsAdmin(long)"/> returns false when AdminIds list is empty.
    /// Ensures the method correctly handles empty admin lists.
    /// </summary>
    [Fact]
    public void IsAdmin_WithEmptyAdminIds_ReturnsFalse()
    {
        // Arrange
        var config = new BotConfiguration
        {
            BotToken = TestBotToken,
            BotUsername = TestBotUsername,
            AdminIds = new List<long>()
        };

        // Act
        var result = config.IsAdmin(TestAdminId);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="BotConfiguration.IsAdmin(long)"/> returns true for the owner ID even when AdminIds list is empty.
    /// Ensures the owner ID is always considered an admin regardless of the admin list state.
    /// </summary>
    [Fact]
    public void IsAdmin_WithEmptyAdminIdsAndOwnerId_ReturnsTrueForOwner()
    {
        // Arrange
        var config = new BotConfiguration
        {
            BotToken = TestBotToken,
            BotUsername = TestBotUsername,
            OwnerId = TestAdminId,
            AdminIds = new List<long>()
        };

        // Act
        var result = config.IsAdmin(TestAdminId);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="BotConfiguration.GetSessionTimeout()"/> returns the default session timeout of 30 minutes when no custom value is set.
    /// Ensures the method provides a sensible default value.
    /// </summary>
    [Fact]
    public void GetSessionTimeout_WithDefaultValue_Returns30Minutes()
    {
        // Arrange
        var config = new BotConfiguration
        {
            BotToken = TestBotToken,
            BotUsername = TestBotUsername
        };

        // Act
        var timeout = config.GetSessionTimeout();

        // Assert
        timeout.Should().Be(DefaultSessionTimeout);
    }

    /// <summary>
    /// Tests that <see cref="BotConfiguration.GetSessionTimeout()"/> returns the correct custom session timeout when SessionTimeoutMinutes is set.
    /// Ensures the method respects the configured session timeout value.
    /// </summary>
    [Fact]
    public void GetSessionTimeout_WithCustomValue_ReturnsCorrectTimeSpan()
    {
        // Arrange
        var config = new BotConfiguration
        {
            BotToken = TestBotToken,
            BotUsername = TestBotUsername,
            SessionTimeoutMinutes = CustomSessionTimeoutMinutes
        };

        // Act
        var timeout = config.GetSessionTimeout();

        // Assert
        timeout.Should().Be(CustomSessionTimeout);
    }

    /// <summary>
    /// Tests that <see cref="BotConfiguration.Validate()"/> throws an exception when BotUsername contains only whitespace.
    /// Ensures validation correctly rejects invalid username values.
    /// </summary>
    [Fact]
    public void Validate_WithWhitespaceBotUsername_ThrowsException()
    {
        // Arrange
        var config = new BotConfiguration
        {
            BotToken = ValidTestBotToken,
            BotUsername = WhitespaceValue
        };

        // Act & Assert
        config.Invoking(c => c.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage(BotUsernameRequiredMessage);
    }

    /// <summary>
    /// Tests that <see cref="BotConfiguration.Validate()"/> throws an exception when BotToken contains only whitespace.
    /// Ensures validation correctly rejects invalid token values.
    /// </summary>
    [Fact]
    public void Validate_WithWhitespaceBotToken_ThrowsException()
    {
        // Arrange
        var config = new BotConfiguration
        {
            BotToken = WhitespaceValue,
            BotUsername = TestBotUsername
        };

        // Act & Assert
        config.Invoking(c => c.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage(BotTokenRequiredMessage);
    }

    /// <summary>
    /// Tests that <see cref="BotConfiguration.Validate()"/> throws an exception when BotToken is a single character.
    /// Ensures validation correctly rejects invalid token values that are too short.
    /// </summary>
    [Fact]
    public void Validate_WithSingleCharacterBotToken_ThrowsException()
    {
        // Arrange
        var config = new BotConfiguration
        {
            BotToken = SingleCharacterBotToken,
            BotUsername = TestBotUsername
        };

        // Act & Assert
        config.Invoking(c => c.Validate())
            .Should().Throw<InvalidOperationException>()
            .WithMessage(BotTokenRequiredMessage);
    }

    /// <summary>
    /// Tests that <see cref="BotConfiguration.Validate()"/> returns true when MaxConcurrentRequests is set to a valid maximum value (100).
    /// Ensures validation accepts valid maximum concurrent request values.
    /// </summary>
    [Fact]
    public void Validate_WithMaxConcurrentRequests_ReturnsTrue()
    {
        // Arrange
        var config = new BotConfiguration
        {
            BotToken = ValidTestBotToken,
            BotUsername = TestBotUsername,
            MaxConcurrentRequests = MaximumConcurrentRequests
        };

        // Act
        var result = config.Validate();

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="BotConfiguration.Validate()"/> returns true when MaxConcurrentRequests is set to the minimum valid value (1).
    /// Ensures validation accepts the minimum valid concurrent request value.
    /// </summary>
    [Fact]
    public void Validate_WithOneMaxConcurrentRequests_ReturnsTrue()
    {
        // Arrange
        var config = new BotConfiguration
        {
            BotToken = ValidTestBotToken,
            BotUsername = TestBotUsername,
            MaxConcurrentRequests = MinimumValidValue
        };

        // Act
        var result = config.Validate();

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="BotConfiguration.Validate()"/> returns true when SessionTimeoutMinutes is set to the minimum valid value (1).
    /// Ensures validation accepts the minimum valid session timeout value.
    /// </summary>
    [Fact]
    public void Validate_WithOneSessionTimeout_ReturnsTrue()
    {
        // Arrange
        var config = new BotConfiguration
        {
            BotToken = ValidTestBotToken,
            BotUsername = TestBotUsername,
            SessionTimeoutMinutes = MinimumValidValue
        };

        // Act
        var result = config.Validate();

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="BotConfiguration.SetCustomSetting(string, string)"/> overwrites an existing custom setting value with the new value.
    /// Ensures the method correctly updates existing settings rather than creating duplicates.
    /// </summary>
    [Fact]
    public void SetCustomSetting_OverwritesExistingValue()
    {
        // Arrange
        var config = new BotConfiguration
        {
            CustomSettings = new Dictionary<string, string> { { CustomSettingKey, OldCustomSettingValue } }
        };

        // Act
        config.SetCustomSetting(CustomSettingKey, NewCustomSettingValue);

        // Assert
        config.CustomSettings.Should().HaveCount(SingleItemCount);
        config.CustomSettings[CustomSettingKey].Should().Be(NewCustomSettingValue);
    }

    /// <summary>
    /// Tests that <see cref="BotConfiguration.RemoveAdmin(long)"/> returns false when attempting to remove an admin from an empty AdminIds list.
    /// Ensures the method correctly handles removal attempts on empty lists.
    /// </summary>
    [Fact]
    public void RemoveAdmin_WithEmptyAdminIds_ReturnsFalse()
    {
        // Arrange
        var config = new BotConfiguration
        {
            BotToken = TestBotToken,
            BotUsername = TestBotUsername,
            AdminIds = new List<long>()
        };

        // Act
        var result = config.RemoveAdmin(TestAdminId);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="BotConfiguration.RemoveAdmin(long)"/> removes only the specified admin from the AdminIds list while preserving other admins.
    /// Ensures the method correctly removes the target admin without affecting other entries.
    /// </summary>
    [Fact]
    public void RemoveAdmin_RemovesOnlySpecifiedAdmin()
    {
        // Arrange
        var config = new BotConfiguration
        {
            BotToken = TestBotToken,
            BotUsername = TestBotUsername,
            AdminIds = new List<long> { FirstAdminId, AdminIdToRemove, ThirdAdminId }
        };

        // Act
        var result = config.RemoveAdmin(AdminIdToRemove);

        // Assert
        result.Should().BeTrue();
        config.AdminIds.Should().HaveCount(RemainingAdminCount);
        config.AdminIds.Should().NotContain(AdminIdToRemove);
        config.AdminIds.Should().Contain(FirstAdminId);
        config.AdminIds.Should().Contain(ThirdAdminId);
    }
}
