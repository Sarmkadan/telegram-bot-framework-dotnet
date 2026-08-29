#nullable enable

using TelegramBotFramework.Models;

namespace TelegramBotFramework.Tests
{
    public interface IBotConfigurationTests
    {
        void BotConfiguration_DefaultValues_AreCorrect();
        void Validate_WithValidConfiguration_ReturnsTrue();
        void Validate_WithEmptyBotToken_ThrowsException();
        void Validate_WithEmptyBotUsername_ThrowsException();
        void Validate_WithWhitespaceBotToken_ThrowsException();
        void Validate_WithZeroSessionTimeout_ThrowsException();
        void Validate_WithNegativeSessionTimeout_ThrowsException();
        void Validate_WithZeroMaxConcurrentRequests_ThrowsException();
        void Validate_WithNegativeMaxConcurrentRequests_ThrowsException();
        void IsAdmin_WithOwnerId_ReturnsTrue();
        void IsAdmin_WithAdminId_ReturnsTrue();
        void IsAdmin_WithNonAdminId_ReturnsFalse();
        void IsAdmin_WithNullAdminIds_ReturnsFalse();
        void AddAdmin_WithNewAdmin_AddsToList();
        void AddAdmin_WithExistingAdmin_DoesNotAddDuplicate();
        void AddAdmin_WithNullAdminIds_InitializesList();
        void RemoveAdmin_WithExistingAdmin_ReturnsTrueAndRemoves();
        void RemoveAdmin_WithNonExistingAdmin_ReturnsFalse();
        void RemoveAdmin_WithNullAdminIds_ReturnsFalse();
        void GetSessionTimeout_ReturnsCorrectTimeSpan();
    }
}