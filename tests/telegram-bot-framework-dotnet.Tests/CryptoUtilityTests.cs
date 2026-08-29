using Xunit;
using FluentAssertions;
using TelegramBotFramework.Utilities;
using System.Text;

namespace TelegramBotFramework.Tests;

public class CryptoUtilityTests : ICryptoUtilityTests
{
    [Theory]
    [InlineData("hello")]
    [InlineData("world")]
    public void HashSHA256_ShouldBeDeterministic(string input)
    {
        var hash1 = CryptoUtility.HashSHA256(input);
        var hash2 = CryptoUtility.HashSHA256(input);
        
        hash1.Should().NotBeNullOrEmpty();
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void HashSHA256_EmptyInput_ReturnsEmptyString()
    {
        CryptoUtility.HashSHA256("").Should().Be("");
        CryptoUtility.HashSHA256(null!).Should().Be("");
    }

    [Fact]
    public void HashMD5_ShouldBeDeterministic()
    {
        var input = "hello";
        var hash1 = CryptoUtility.HashMD5(input);
        var hash2 = CryptoUtility.HashMD5(input);
        
        hash1.Should().NotBeNullOrEmpty();
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void HashPassword_VerifyPassword_Roundtrip()
    {
        var password = "superSecretPassword123!";
        var hash = CryptoUtility.HashPassword(password);
        
        hash.Should().NotBeNullOrEmpty();
        CryptoUtility.VerifyPassword(password, hash).Should().BeTrue();
        CryptoUtility.VerifyPassword("wrongPassword", hash).Should().BeFalse();
    }

    [Fact]
    public void HashPassword_EmptyPassword_ThrowsArgumentException()
    {
        Action act = () => CryptoUtility.HashPassword("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void VerifyPassword_InvalidHash_ReturnsFalse()
    {
        CryptoUtility.VerifyPassword("password", "invalidHash").Should().BeFalse();
    }

    [Fact]
    public void GenerateRandomString_ReturnsCorrectLength()
    {
        var length = 10;
        var randomString = CryptoUtility.GenerateRandomString(length);
        randomString.Length.Should().Be(length);
    }

    [Fact]
    public void GenerateRandomString_InvalidLength_ThrowsArgumentException()
    {
        Action act = () => CryptoUtility.GenerateRandomString(0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GenerateRandomToken_ReturnsCorrectLength()
    {
        var bytes = 16;
        // GenerateRandomToken returns hex, which is 2 characters per byte
        var token = CryptoUtility.GenerateRandomToken(bytes);
        token.Length.Should().Be(bytes * 2);
    }

    [Theory]
    [InlineData("message", "key")]
    public void ComputeHmacSHA256_Deterministic(string message, string key)
    {
        var hmac1 = CryptoUtility.ComputeHmacSHA256(message, key);
        var hmac2 = CryptoUtility.ComputeHmacSHA256(message, key);
        
        hmac1.Should().NotBeNullOrEmpty();
        hmac1.Should().Be(hmac2);
    }

    [Fact]
    public void Base64_Roundtrip()
    {
        var original = "Hello, World!";
        var encoded = CryptoUtility.EncodeBase64(original);
        var decoded = CryptoUtility.DecodeBase64(encoded);
        
        decoded.Should().Be(original);
    }

    [Fact]
    public void DecodeBase64_InvalidInput_ReturnsNull()
    {
        CryptoUtility.DecodeBase64("NotBase64!").Should().BeNull();
    }
}
