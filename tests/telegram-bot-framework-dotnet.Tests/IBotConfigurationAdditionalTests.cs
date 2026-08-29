#nullable enable

using FluentAssertions;
using static TelegramBotFramework.Tests.BotConfigurationAdditionalTestsConstants;
using TelegramBotFramework.Models;
using Xunit;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Contains additional test cases for <see cref="BotConfiguration"/> class functionality.
/// Focuses on edge cases, null handling, and validation scenarios not covered in main test classes.
/// </summary>
public interface IBotConfigurationAdditionalTests
{
    void BotConfiguration_WithNullAdminIds_ListIsInitialized();
    void BotConfiguration_WithNullCustomSettings_DictionaryIsInitialized();
    void BotConfiguration_WithEmptyCustomSettings_DictionaryIsInitialized();
    void IsAdmin_WithEmptyAdminIds_ReturnsFalse();
    void IsAdmin_WithEmptyAdminIdsAndOwnerId_ReturnsTrueForOwner();
    void GetSessionTimeout_WithDefaultValue_Returns30Minutes();
    void GetSessionTimeout_WithCustomValue_ReturnsCorrectTimeSpan();
    void Validate_WithWhitespaceBotUsername_ThrowsException();
    void Validate_WithWhitespaceBotToken_ThrowsException();
    void Validate_WithSingleCharacterBotToken_ThrowsException();
    void Validate_WithMaxConcurrentRequests_ReturnsTrue();
    void Validate_WithOneMaxConcurrentRequests_ReturnsTrue();
    void Validate_WithOneSessionTimeout_ReturnsTrue();
    void SetCustomSetting_OverwritesExistingValue();
    void RemoveAdmin_WithEmptyAdminIds_ReturnsFalse();
    void RemoveAdmin_RemovesOnlySpecifiedAdmin();
}