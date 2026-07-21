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
/// Contains unit tests for ValidationUtility methods.
/// Tests cover valid, invalid, null, and boundary inputs for each validator.
/// </summary>
public sealed class ValidationUtilityTests
{
    #region Telegram User ID Validation

    /// <summary>
    /// Tests IsValidTelegramUserId with various positive and negative inputs.
    /// </summary>
    [Theory]
    [InlineData(1L, true)]
    [InlineData(100L, true)]
    [InlineData(999999999L, true)]
    [InlineData(1000000000L, true)]
    [InlineData(0L, false)]
    [InlineData(-1L, false)]
    [InlineData(-100L, false)]
    [InlineData(-999999999L, false)]
    public void IsValidTelegramUserId_VariousInputs_ReturnsExpectedResult(long userId, bool expected)
    {
        ValidationUtility.IsValidTelegramUserId(userId).Should().Be(expected);
    }

    #endregion

    #region Telegram Chat ID Validation

    /// <summary>
    /// Tests IsValidTelegramChatId with positive, negative, and zero values.
    /// </summary>
    [Theory]
    [InlineData(12345L, true)]
    [InlineData(1L, true)]
    [InlineData(-1L, true)]
    [InlineData(-100500L, true)]
    [InlineData(-999999999L, true)]
    [InlineData(0L, false)]
    public void IsValidTelegramChatId_VariousInputs_ReturnsExpectedResult(long chatId, bool expected)
    {
        ValidationUtility.IsValidTelegramChatId(chatId).Should().Be(expected);
    }

    #endregion

    #region Telegram Token Validation

    /// <summary>
    /// Tests IsValidTelegramToken with valid token formats.
    /// </summary>
    [Theory]
    [InlineData("1234567890:abcdefghijklmnopqrstuvwxyzA", true)]
    [InlineData("1:abcdefghijklmnopqrstuvwxyzA", true)]
    [InlineData("0:abcdefghijklmnopqrstuvwxyzA", true)]
    [InlineData("123:ABCDEFGHIJKLMNOPQRSTUVWXYZ-", true)]
    public void IsValidTelegramToken_ValidFormats_ReturnsTrue(string token, bool expected)
    {
        ValidationUtility.IsValidTelegramToken(token).Should().Be(expected);
    }

    /// <summary>
    /// Tests IsValidTelegramToken with invalid token formats.
    /// </summary>
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("1234567890:abc", false)]
    [InlineData("1234567890:abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQR", false)]
    [InlineData("1234567890abc:token", false)]
    [InlineData("token", false)]
    [InlineData("1234567890:", false)]
    [InlineData(":abcdefghijklmnopqrstuvwxyzABCDEFGHIJ", false)]
    [InlineData("1234567890:abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ", false)]
    public void IsValidTelegramToken_InvalidFormats_ReturnsFalse(string? token, bool expected)
    {
        ValidationUtility.IsValidTelegramToken(token).Should().Be(expected);
    }

    #endregion

    #region URL Validation

    /// <summary>
    /// Tests IsValidUrl with valid URLs.
    /// </summary>
    [Theory]
    [InlineData("https://example.com", true)]
    [InlineData("http://example.com", true)]
    [InlineData("https://sub.example.com/path?query=value#fragment", true)]
    [InlineData("http://localhost:5000", true)]
    [InlineData("https://192.168.1.1:8080", true)]
    [InlineData("ftp://files.example.com", true)]
    public void IsValidUrl_ValidUrls_ReturnsTrue(string url, bool expected)
    {
        ValidationUtility.IsValidUrl(url).Should().Be(expected);
    }

    /// <summary>
    /// Tests IsValidUrl with null, empty, whitespace, and invalid URLs.
    /// </summary>
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("not-a-url", false)]
    [InlineData("example.com", false)]
    [InlineData("http://", false)]
    [InlineData("https://", false)]
    public void IsValidUrl_InvalidInputs_ReturnsFalse(string? url, bool expected)
    {
        ValidationUtility.IsValidUrl(url).Should().Be(expected);
    }

    #endregion

    #region IPv4 Validation

    /// <summary>
    /// Tests IsValidIPv4 with valid IPv4 addresses.
    /// </summary>
    [Theory]
    [InlineData("192.168.1.1", true)]
    [InlineData("10.0.0.1", true)]
    [InlineData("172.16.0.1", true)]
    [InlineData("255.255.255.255", true)]
    [InlineData("0.0.0.0", true)]
    [InlineData("192.168.1.255", true)]
    public void IsValidIPv4_ValidAddresses_ReturnsTrue(string ipAddress, bool expected)
    {
        ValidationUtility.IsValidIPv4(ipAddress).Should().Be(expected);
    }

    /// <summary>
    /// Tests IsValidIPv4 with invalid IPv4 addresses.
    /// </summary>
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("256.1.1.1", false)]
    [InlineData("192.168.1", false)]
    [InlineData("192.168.1.1.1", false)]
    [InlineData("192.168.1.256", false)]
    [InlineData("192.168.-1.1", false)]
    [InlineData("abc.def.ghi.jkl", false)]
    [InlineData("192.168.1.1:8080", false)]
    public void IsValidIPv4_InvalidInputs_ReturnsFalse(string? ipAddress, bool expected)
    {
        ValidationUtility.IsValidIPv4(ipAddress).Should().Be(expected);
    }

    #endregion

    #region Phone Number Validation

    /// <summary>
    /// Tests IsValidPhoneNumber with valid phone numbers in various formats.
    /// </summary>
    [Theory]
    [InlineData("+1 (555) 123-4567", true)]
    [InlineData("1234567890", true)]
    [InlineData("+44 20 7946 0958", true)]
    [InlineData("555-123-4567", true)]
    [InlineData("5551234567", true)]
    [InlineData("+380 99 123 45 67", true)]
    public void IsValidPhoneNumber_ValidFormats_ReturnsTrue(string phoneNumber, bool expected)
    {
        ValidationUtility.IsValidPhoneNumber(phoneNumber).Should().Be(expected);
    }

    /// <summary>
    /// Tests IsValidPhoneNumber with invalid phone numbers.
    /// </summary>
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("123", false)]
    [InlineData("123456789", false)]
    [InlineData("abc", false)]
    [InlineData("123-456-789", false)]
    public void IsValidPhoneNumber_InvalidInputs_ReturnsFalse(string? phoneNumber, bool expected)
    {
        ValidationUtility.IsValidPhoneNumber(phoneNumber).Should().Be(expected);
    }

    /// <summary>
    /// Tests IsValidPhoneNumber with boundary length values.
    /// </summary>
    [Theory]
    [InlineData("1234567890", true)]  // Exactly 10 digits
    [InlineData("12345678901", true)] // 11 digits
    [InlineData("123456789012345", true)] // 15 digits (max)
    [InlineData("123456789", false)] // 9 digits (below min)
    [InlineData("1234567890123456", false)] // 16 digits (above max)
    public void IsValidPhoneNumber_BoundaryLengths_ReturnsExpectedResult(string phoneNumber, bool expected)
    {
        ValidationUtility.IsValidPhoneNumber(phoneNumber).Should().Be(expected);
    }

    #endregion

    #region Command Name Validation

    /// <summary>
    /// Tests IsValidCommandName with valid command names.
    /// </summary>
    [Theory]
    [InlineData("/start", true)]
    [InlineData("/help", true)]
    [InlineData("/get_status", true)]
    [InlineData("/command123", true)]
    [InlineData("/UPPERCASE", true)]
    [InlineData("/lowercase", true)]
    [InlineData("/with_underscore", true)]
    [InlineData("/with123numbers", true)]
    public void IsValidCommandName_ValidFormats_ReturnsTrue(string commandName, bool expected)
    {
        ValidationUtility.IsValidCommandName(commandName).Should().Be(expected);
    }

    /// <summary>
    /// Tests IsValidCommandName with invalid command names.
    /// </summary>
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("start", false)]
    [InlineData("start/", false)]
    [InlineData("/hello-world", false)]
    [InlineData("/hello world", false)]
    [InlineData("/hello@world", false)]
    [InlineData("/hello.world", false)]
    public void IsValidCommandName_InvalidInputs_ReturnsFalse(string? commandName, bool expected)
    {
        ValidationUtility.IsValidCommandName(commandName).Should().Be(expected);
    }

    #endregion

    #region Filename Validation

    /// <summary>
    /// Tests IsValidFilename with valid filenames.
    /// </summary>
    [Theory]
    [InlineData("file.txt", true)]
    [InlineData("document.pdf", true)]
    [InlineData("valid_filename", true)]
    [InlineData("file-with-dashes.txt", true)]
    [InlineData("file_with_underscores.txt", true)]
    [InlineData("file123.txt", true)]
    [InlineData("a", true)]
    public void IsValidFilename_ValidNames_ReturnsTrue(string filename, bool expected)
    {
        ValidationUtility.IsValidFilename(filename).Should().Be(expected);
    }

    /// <summary>
    /// Tests IsValidFilename with invalid filenames containing forbidden characters.
    /// </summary>
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("file/name.txt", false)]
    [InlineData("file:name.txt", false)]
    [InlineData("file*name.txt", false)]
    [InlineData("file?name.txt", false)]
    [InlineData("file\"name.txt", false)]
    [InlineData("file<name.txt", false)]
    [InlineData("file>name.txt", false)]
    [InlineData("file|name.txt", false)]
    public void IsValidFilename_InvalidInputs_ReturnsFalse(string? filename, bool expected)
    {
        ValidationUtility.IsValidFilename(filename).Should().Be(expected);
    }

    #endregion

    #region Password Strength Validation

    /// <summary>
    /// Tests IsStrongPassword with valid strong passwords.
    /// </summary>
    [Theory]
    [InlineData("SecureP@ss1", true)]
    [InlineData("P@ssw0rd!", true)]
    [InlineData("MyP@ss123", true)]
    [InlineData("C0mpl3x!Pass", true)]
    [InlineData("Abcdefg1!", true)]
    public void IsStrongPassword_ValidStrongPasswords_ReturnsTrue(string password, bool expected)
    {
        ValidationUtility.IsStrongPassword(password).Should().Be(expected);
    }

    /// <summary>
    /// Tests IsStrongPassword with invalid passwords (too short or missing requirements).
    /// </summary>
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("NoSpecial1", false)] // Missing special character
    [InlineData("abcdefg1!", false)] // Missing uppercase
    [InlineData("ABCDEFG1!", false)] // Missing lowercase
    [InlineData("Abcdefgh!", false)] // Missing digit
    [InlineData("NoSpecial1", false)] // Missing special character
    [InlineData("short1!", false)] // Too short (7 chars)
    [InlineData("Ab1!", false)] // Too short (4 chars)
    public void IsStrongPassword_InvalidInputs_ReturnsFalse(string? password, bool expected)
    {
        ValidationUtility.IsStrongPassword(password).Should().Be(expected);
    }

    /// <summary>
    /// Tests IsStrongPassword with boundary length values.
    /// </summary>
    [Theory]
    [InlineData("Ab1!xyz", false)] // 7 chars (below min)
    [InlineData("Ab1!xyzz", true)] // 8 chars (exactly min)
    [InlineData("Ab1!xyzzzzzzzzzz", true)] // 16 chars
    public void IsStrongPassword_BoundaryLengths_ReturnsExpectedResult(string password, bool expected)
    {
        ValidationUtility.IsStrongPassword(password).Should().Be(expected);
    }

    #endregion

    #region Length Validation

    /// <summary>
    /// Tests IsValidLength with valid string lengths.
    /// </summary>
    [Theory]
    [InlineData("hello", 3, 10, true)]
    [InlineData("hi", 1, 5, true)]
    [InlineData("a", 1, 1, true)]
    [InlineData("test", 0, 10, true)]
    [InlineData("", 0, 0, true)]
    public void IsValidLength_ValidLengths_ReturnsTrue(string value, int minLength, int maxLength, bool expected)
    {
        ValidationUtility.IsValidLength(value, minLength, maxLength).Should().Be(expected);
    }

    /// <summary>
    /// Tests IsValidLength with invalid string lengths.
    /// </summary>
    [Theory]
    [InlineData(null, 0, 10, true)] // null with minLength=0 should return true
    [InlineData(null, 1, 10, false)] // null with minLength>0 should return false
    [InlineData("hello", 10, 5, false)] // min > max (invalid range)
    [InlineData("hello world", 1, 5, false)] // Exceeds maximum
    [InlineData("hi", 5, 10, false)] // Below minimum
    public void IsValidLength_InvalidInputs_ReturnsExpectedResult(string? value, int minLength, int maxLength, bool expected)
    {
        ValidationUtility.IsValidLength(value, minLength, maxLength).Should().Be(expected);
    }

    #endregion

    #region Numeric Validation

    /// <summary>
    /// Tests IsNumeric with valid numeric strings.
    /// </summary>
    [Theory]
    [InlineData("3.14", true)]
    [InlineData("-42", true)]
    [InlineData("0", true)]
    [InlineData("123", true)]
    [InlineData("+123", true)]
    [InlineData("3.14159", true)]
    [InlineData("-3.14", true)]
    public void IsNumeric_ValidNumericStrings_ReturnsTrue(string value, bool expected)
    {
        ValidationUtility.IsNumeric(value).Should().Be(expected);
    }

    /// <summary>
    /// Tests IsNumeric with invalid non-numeric strings.
    /// </summary>
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("12abc", false)]
    [InlineData("abc123", false)]
    [InlineData("12 34", false)]
    [InlineData("1.2.3", false)]
    public void IsNumeric_InvalidInputs_ReturnsFalse(string? value, bool expected)
    {
        ValidationUtility.IsNumeric(value).Should().Be(expected);
    }

    #endregion

    #region GUID Validation

    /// <summary>
    /// Tests IsValidGuid with valid GUID strings.
    /// </summary>
    [Theory]
    [InlineData("550e8400-e29b-41d4-a716-446655440000", true)]
    [InlineData("00000000-0000-0000-0000-000000000000", true)]
    [InlineData("123e4567-e89b-12d3-a456-426614174000", true)]
    public void IsValidGuid_ValidGuidStrings_ReturnsTrue(string value, bool expected)
    {
        ValidationUtility.IsValidGuid(value).Should().Be(expected);
    }

    /// <summary>
    /// Tests IsValidGuid with invalid GUID strings.
    /// </summary>
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("not-a-guid", false)]
    [InlineData("550e8400-e29b-41d4-a716-44665544000", false)] // Too short
    [InlineData("550e8400-e29b-41d4-a716-4466554400000", false)] // Too long
    [InlineData("550e8400-e29b-41d4-a716-44665544000G", false)] // Invalid character (uppercase G)
    public void IsValidGuid_InvalidInputs_ReturnsFalse(string? value, bool expected)
    {
        ValidationUtility.IsValidGuid(value).Should().Be(expected);
    }

    #endregion
}