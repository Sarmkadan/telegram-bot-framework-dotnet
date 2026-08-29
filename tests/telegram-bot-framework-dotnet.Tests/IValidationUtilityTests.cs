#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using FluentAssertions;
using TelegramBotFramework.Utilities;
using Xunit;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Interface for ValidationUtilityTests.
/// </summary>
public interface IValidationUtilityTests
{
    void IsValidTelegramUserId_VariousInputs_ReturnsExpectedResult(long userId, bool expected);
    void IsValidTelegramChatId_VariousInputs_ReturnsExpectedResult(long chatId, bool expected);
    void IsValidTelegramToken_ValidFormats_ReturnsTrue(string token, bool expected);
    void IsValidTelegramToken_InvalidFormats_ReturnsFalse(string? token, bool expected);
    void IsValidUrl_ValidUrls_ReturnsTrue(string url, bool expected);
    void IsValidUrl_InvalidInputs_ReturnsFalse(string? url, bool expected);
    void IsValidIPv4_ValidAddresses_ReturnsTrue(string ipAddress, bool expected);
    void IsValidIPv4_InvalidInputs_ReturnsFalse(string? ipAddress, bool expected);
    void IsValidPhoneNumber_ValidFormats_ReturnsTrue(string phoneNumber, bool expected);
    void IsValidPhoneNumber_InvalidInputs_ReturnsFalse(string? phoneNumber, bool expected);
    void IsValidPhoneNumber_BoundaryLengths_ReturnsExpectedResult(string phoneNumber, bool expected);
    void IsValidCommandName_ValidFormats_ReturnsTrue(string commandName, bool expected);
    void IsValidCommandName_InvalidInputs_ReturnsFalse(string? commandName, bool expected);
    void IsValidFilename_ValidNames_ReturnsTrue(string filename, bool expected);
    void IsValidFilename_InvalidInputs_ReturnsFalse(string? filename, bool expected);
    void IsStrongPassword_ValidStrongPasswords_ReturnsTrue(string password, bool expected);
    void IsStrongPassword_InvalidInputs_ReturnsFalse(string? password, bool expected);
    void IsStrongPassword_BoundaryLengths_ReturnsExpectedResult(string password, bool expected);
    void IsValidLength_ValidLengths_ReturnsTrue(string value, int minLength, int maxLength, bool expected);
    void IsValidLength_InvalidInputs_ReturnsExpectedResult(string? value, int minLength, int maxLength, bool expected);
}