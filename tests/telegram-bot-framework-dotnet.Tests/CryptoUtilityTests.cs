using Xunit;
using FluentAssertions;
using TelegramBotFramework.Utilities;
using System.Text;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Tests for the CryptoUtility class.
/// </summary>
public class CryptoUtilityTests : ICryptoUtilityTests
{
    /// <summary>
    /// Tests that the HashSHA256 method produces deterministic output for the same input.
    /// </summary>
    /// <param name="input">The input string to hash.</param>
    [Theory]
    [InlineData("hello")]
    [InlineData("world")]
    public void HashSHA256_ShouldBeDeterministic(string input)
    {
        ArgumentException.ThrowIfNullOrEmpty(input);

        var hash1 = CryptoUtility.HashSHA256(input);
        var hash2 = CryptoUtility.HashSHA256(input);

        hash1.Should().NotBeNullOrEmpty();
        hash1.Should().Be(hash2);
    }

    /// <summary>
    /// Tests that the HashSHA256 method returns an empty string for null or empty input.
    /// </summary>
    [Fact]
    public void HashSHA256_EmptyInput_ReturnsEmptyString()
    {
        CryptoUtility.HashSHA256("").Should().Be("");
        CryptoUtility.HashSHA256(null!).Should().Be("");
    }

    /// <summary>
    /// Tests that the HashMD5 method produces deterministic output for the same input.
    /// </summary>
    [Fact]
    public void HashMD5_ShouldBeDeterministic()
    {
        var input = "hello";
        var hash1 = CryptoUtility.HashMD5(input);
        var hash2 = CryptoUtility.HashMD5(input);

        hash1.Should().NotBeNullOrEmpty();
        hash1.Should().Be(hash2);
    }

    /// <summary>
    /// Tests that the HashPassword and VerifyPassword methods work correctly together for a valid password.
    /// </summary>
    [Fact]
    public void HashPassword_VerifyPassword_Roundtrip()
    {
        var password = "superSecretPassword123!";
        var hash = CryptoUtility.HashPassword(password);

        hash.Should().NotBeNullOrEmpty();
        CryptoUtility.VerifyPassword(password, hash).Should().BeTrue();
        CryptoUtility.VerifyPassword("wrongPassword", hash).Should().BeFalse();
    }

    /// <summary>
    /// Tests that the HashPassword method throws an ArgumentException when given an empty password.
    /// </summary>
    [Fact]
    public void HashPassword_EmptyPassword_ThrowsArgumentException()
    {
        Action act = () => CryptoUtility.HashPassword("");
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that the VerifyPassword method returns false when given an invalid hash.
    /// </summary>
    [Fact]
    public void VerifyPassword_InvalidHash_ReturnsFalse()
    {
        CryptoUtility.VerifyPassword("password", "invalidHash").Should().BeFalse();
    }

    /// <summary>
    /// Tests that the GenerateRandomString method returns a string of the specified length.
    /// </summary>
    [Fact]
    public void GenerateRandomString_ReturnsCorrectLength()
    {
        var length = 10;
        var randomString = CryptoUtility.GenerateRandomString(length);
        randomString.Length.Should().Be(length);
    }

    /// <summary>
    /// Tests that the GenerateRandomString method throws an ArgumentException when given an invalid length (zero or negative).
    /// </summary>
    [Fact]
    public void GenerateRandomString_InvalidLength_ThrowsArgumentException()
    {
        Action act = () => CryptoUtility.GenerateRandomString(0);
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that the GenerateRandomToken method returns a hex string of the specified length (2 characters per byte).
    /// </summary>
    [Fact]
    public void GenerateRandomToken_ReturnsCorrectLength()
    {
        var bytes = 16;
        // GenerateRandomToken returns hex, which is 2 characters per byte
        var token = CryptoUtility.GenerateRandomToken(bytes);
        token.Length.Should().Be(bytes * 2);
    }

    /// <summary>
    /// Verifies that computing an HMAC-SHA256 value twice with the same message and key produces the same non-empty result.
    /// </summary>
    /// <param name="message">The message supplied to both HMAC-SHA256 computations.</param>
    /// <param name="key">The key supplied to both HMAC-SHA256 computations.</param>
    [Theory]
    [InlineData("message", "key")]
    public void ComputeHmacSHA256_Deterministic(string message, string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        ArgumentException.ThrowIfNullOrEmpty(key);

        var hmac1 = CryptoUtility.ComputeHmacSHA256(message, key);
        var hmac2 = CryptoUtility.ComputeHmacSHA256(message, key);

        hmac1.Should().NotBeNullOrEmpty();
        hmac1.Should().Be(hmac2);
    }

    /// <summary>
    /// Verifies that Base64-encoding and then decoding a string restores the original value.
    /// </summary>
    [Fact]
    public void Base64_Roundtrip()
    {
        var original = "Hello, World!";
        var encoded = CryptoUtility.EncodeBase64(original);
        var decoded = CryptoUtility.DecodeBase64(encoded);

        decoded.Should().Be(original);
    }

    /// <summary>
    /// Tests that the DecodeBase64 method returns null when given an invalid Base64 string.
    /// </summary>
    [Fact]
    public void DecodeBase64_InvalidInput_ReturnsNull()
    {
        CryptoUtility.DecodeBase64("NotBase64!").Should().BeNull();
    }
}