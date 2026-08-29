namespace TelegramBotFramework.Tests;

public interface ICryptoUtilityTests
{
    void HashSHA256_ShouldBeDeterministic(string input);
    void HashSHA256_EmptyInput_ReturnsEmptyString();
    void HashMD5_ShouldBeDeterministic();
    void HashPassword_VerifyPassword_Roundtrip();
    void HashPassword_EmptyPassword_ThrowsArgumentException();
    void VerifyPassword_InvalidHash_ReturnsFalse();
    void GenerateRandomString_ReturnsCorrectLength();
    void GenerateRandomString_InvalidLength_ThrowsArgumentException();
    void GenerateRandomToken_ReturnsCorrectLength();
    void ComputeHmacSHA256_Deterministic(string message, string key);
    void Base64_Roundtrip();
    void DecodeBase64_InvalidInput_ReturnsNull();
}