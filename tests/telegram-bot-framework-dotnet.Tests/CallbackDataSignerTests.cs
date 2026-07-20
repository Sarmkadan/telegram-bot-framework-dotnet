#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using TelegramBotFramework.Utilities;
using Xunit;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Unit tests for <see cref="CallbackDataSigner"/>.
/// </summary>
public sealed class CallbackDataSignerTests
{
    private const string TestSecret = "test-secret-key-123";
    private const string TestData = "user_action:123";

    [Fact]
    public void Sign_WithValidDataAndSecret_ReturnsSignedData()
    {
        // Act
        var signed = CallbackDataSigner.Sign(TestData, TestSecret);

        // Assert
        signed.Should().NotBeNullOrEmpty();
        signed.Should().Contain(TestData);
        signed.Should().Contain("|");
    }

    [Fact]
    public void Sign_WithNullData_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => CallbackDataSigner.Sign(null!, TestSecret));
    }

    [Fact]
    public void Sign_WithNullSecret_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => CallbackDataSigner.Sign(TestData, null!));
    }

    [Fact]
    public void Sign_WithEmptyData_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => CallbackDataSigner.Sign(string.Empty, TestSecret));
    }

    [Fact]
    public void Sign_WithEmptySecret_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => CallbackDataSigner.Sign(TestData, string.Empty));
    }

    [Fact]
    public void Sign_WithWhitespaceData_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => CallbackDataSigner.Sign("   ", TestSecret));
    }

    [Fact]
    public void Sign_WithWhitespaceSecret_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => CallbackDataSigner.Sign(TestData, "   "));
    }

    [Fact]
    public void TryValidate_WithValidSignedData_ReturnsTrueAndExtractsOriginalData()
    {
        // Arrange
        var signed = CallbackDataSigner.Sign(TestData, TestSecret);

        // Act
        var result = CallbackDataSigner.TryValidate(signed, TestSecret, out var extractedData);

        // Assert
        result.Should().BeTrue();
        extractedData.Should().Be(TestData);
    }

    [Fact]
    public void TryValidate_WithInvalidSecret_ReturnsFalse()
    {
        // Arrange
        var signed = CallbackDataSigner.Sign(TestData, TestSecret);
        var wrongSecret = "wrong-secret";

        // Act
        var result = CallbackDataSigner.TryValidate(signed, wrongSecret, out var extractedData);

        // Assert
        result.Should().BeFalse();
        extractedData.Should().BeEmpty();
    }

    [Fact]
    public void TryValidate_WithTamperedSignedData_ReturnsFalse()
    {
        // Arrange - create valid signed data
        var signed = CallbackDataSigner.Sign(TestData, TestSecret);

        // Tamper with it
        var tampered = signed.Replace("|", ";");

        // Act
        var result = CallbackDataSigner.TryValidate(tampered, TestSecret, out var extractedData);

        // Assert
        result.Should().BeFalse();
        extractedData.Should().BeEmpty();
    }

    [Fact]
    public void TryValidate_WithNullSignedData_ReturnsFalse()
    {
        // Act
        var result = CallbackDataSigner.TryValidate(null!, TestSecret, out var extractedData);

        // Assert
        result.Should().BeFalse();
        extractedData.Should().BeEmpty();
    }

    [Fact]
    public void TryValidate_WithEmptySignedData_ReturnsFalse()
    {
        // Act
        var result = CallbackDataSigner.TryValidate(string.Empty, TestSecret, out var extractedData);

        // Assert
        result.Should().BeFalse();
        extractedData.Should().BeEmpty();
    }

    [Fact]
    public void TryValidate_WithNullSecret_ReturnsFalse()
    {
        // Arrange
        var signed = CallbackDataSigner.Sign(TestData, TestSecret);

        // Act
        var result = CallbackDataSigner.TryValidate(signed, null!, out var extractedData);

        // Assert
        result.Should().BeFalse();
        extractedData.Should().BeEmpty();
    }

    [Fact]
    public void TryValidate_WithEmptySecret_ReturnsFalse()
    {
        // Arrange
        var signed = CallbackDataSigner.Sign(TestData, TestSecret);

        // Act
        var result = CallbackDataSigner.TryValidate(signed, string.Empty, out var extractedData);

        // Assert
        result.Should().BeFalse();
        extractedData.Should().BeEmpty();
    }

    [Fact]
    public void TryValidate_WithMissingSeparator_ReturnsFalse()
    {
        // Arrange - manually create data without separator
        var dataWithoutSeparator = "invalid_signed_data";

        // Act
        var result = CallbackDataSigner.TryValidate(dataWithoutSeparator, TestSecret, out var extractedData);

        // Assert
        result.Should().BeFalse();
        extractedData.Should().BeEmpty();
    }

    [Fact]
    public void TryValidate_WithSeparatorAtEnd_ReturnsFalse()
    {
        // Arrange - manually create data with separator at end
        var dataWithSeparatorAtEnd = "data|";

        // Act
        var result = CallbackDataSigner.TryValidate(dataWithSeparatorAtEnd, TestSecret, out var extractedData);

        // Assert
        result.Should().BeFalse();
        extractedData.Should().BeEmpty();
    }

    [Fact]
    public void Sign_ProducesDifferentOutputForSameInputWithDifferentSecrets()
    {
        // Arrange
        var secret1 = "secret1";
        var secret2 = "secret2";

        // Act
        var signed1 = CallbackDataSigner.Sign(TestData, secret1);
        var signed2 = CallbackDataSigner.Sign(TestData, secret2);

        // Assert
        signed1.Should().NotBe(signed2);
        signed1.Should().Contain("|");
        signed2.Should().Contain("|");
    }

    [Fact]
    public void Sign_ProducesSameOutputForSameInputAndSecret()
    {
        // Arrange
        var secret = "consistent-secret";

        // Act
        var signed1 = CallbackDataSigner.Sign(TestData, secret);
        var signed2 = CallbackDataSigner.Sign(TestData, secret);

        // Assert
        signed1.Should().Be(signed2);
    }

    [Fact]
    public void TryValidate_WithLongData_FitsWithinTelegramLimit()
    {
        // Arrange - use a reasonably long but valid data string
        // Max data length: 64 - separator (1 byte) - signature (16 hex chars = 8 bytes) = 47 bytes
        // But we need to account for UTF-8 encoding overhead, so use 40 chars to be safe
        var longData = new string('x', 40);
        var secret = "test-secret";

        // Act
        var signed = CallbackDataSigner.Sign(longData, secret);
        var isValid = CallbackDataSigner.TryValidate(signed, secret, out var extracted);

        // Assert
        var byteLength = System.Text.Encoding.UTF8.GetByteCount(signed);
        byteLength.Should().BeLessOrEqualTo(64);
        isValid.Should().BeTrue();
        extracted.Should().Be(longData);
    }

    [Fact]
    public void Sign_WithDataThatWouldExceedLimit_ThrowsArgumentException()
    {
        // Arrange - create data that would exceed limit when signed
        // Max data length: 64 - separator - signature (16 chars) = ~47 chars
        var veryLongData = new string('x', 100);
        var secret = "test-secret";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => CallbackDataSigner.Sign(veryLongData, secret));
    }

    [Fact]
    public void TryValidate_HandlesExceptionGracefully_ReturnsFalse()
    {
        // This test ensures that any unexpected exception during validation
        // doesn't propagate and instead returns false

        // Arrange - create malformed data
        var malformed = "incomplete|sig";

        // Act
        var result = CallbackDataSigner.TryValidate(malformed, TestSecret, out var extracted);

        // Assert - should gracefully return false rather than throw
        result.Should().BeFalse();
        extracted.Should().BeEmpty();
    }

    [Fact]
    public void Sign_AndValidate_RoundTripWorksCorrectly()
    {
        // Arrange
        var originalData = "command:delete_user:12345";
        var secret = "my-secret-key";

        // Act
        var signed = CallbackDataSigner.Sign(originalData, secret);
        var isValid = CallbackDataSigner.TryValidate(signed, secret, out var extractedData);

        // Assert
        isValid.Should().BeTrue();
        extractedData.Should().Be(originalData);
    }
}
