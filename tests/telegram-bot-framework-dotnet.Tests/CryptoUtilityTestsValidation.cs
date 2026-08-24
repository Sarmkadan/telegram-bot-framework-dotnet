using Xunit;
using FluentAssertions;
using TelegramBotFramework.Utilities;
using System.Text;
using System.Globalization;

namespace TelegramBotFramework.Tests
{
    public static class CryptoUtilityTestsValidation
    {
        public static IReadOnlyList<string> Validate(CryptoUtilityTests value)
        {
            var errors = new List<string>();

            // HashSHA256_ShouldBeDeterministic
            if (value.HashSHA256_ShouldBeDeterministic == null)
            {
                errors.Add("HashSHA256_ShouldBeDeterministic is null");
            }

            // HashSHA256_EmptyInput_ReturnsEmptyString
            if (value.HashSHA256_EmptyInput_ReturnsEmptyString == null)
            {
                errors.Add("HashSHA256_EmptyInput_ReturnsEmptyString is null");
            }

            // HashMD5_ShouldBeDeterministic
            if (value.HashMD5_ShouldBeDeterministic == null)
            {
                errors.Add("HashMD5_ShouldBeDeterministic is null");
            }

            // HashPassword_VerifyPassword_Roundtrip
            if (value.HashPassword_VerifyPassword_Roundtrip == null)
            {
                errors.Add("HashPassword_VerifyPassword_Roundtrip is null");
            }

            // HashPassword_EmptyPassword_ThrowsArgumentException
            if (value.HashPassword_EmptyPassword_ThrowsArgumentException == null)
            {
                errors.Add("HashPassword_EmptyPassword_ThrowsArgumentException is null");
            }

            // VerifyPassword_InvalidHash_ReturnsFalse
            if (value.VerifyPassword_InvalidHash_ReturnsFalse == null)
            {
                errors.Add("VerifyPassword_InvalidHash_ReturnsFalse is null");
            }

            // GenerateRandomString_ReturnsCorrectLength
            if (value.GenerateRandomString_ReturnsCorrectLength == null)
            {
                errors.Add("GenerateRandomString_ReturnsCorrectLength is null");
            }

            // GenerateRandomString_InvalidLength_ThrowsArgumentException
            if (value.GenerateRandomString_InvalidLength_ThrowsArgumentException == null)
            {
                errors.Add("GenerateRandomString_InvalidLength_ThrowsArgumentException is null");
            }

            // GenerateRandomToken_ReturnsCorrectLength
            if (value.GenerateRandomToken_ReturnsCorrectLength == null)
            {
                errors.Add("GenerateRandomToken_ReturnsCorrectLength is null");
            }

            // ComputeHmacSHA256_Deterministic
            if (value.ComputeHmacSHA256_Deterministic == null)
            {
                errors.Add("ComputeHmacSHA256_Deterministic is null");
            }

            // Base64_Roundtrip
            if (value.Base64_Roundtrip == null)
            {
                errors.Add("Base64_Roundtrip is null");
            }

            // DecodeBase64_InvalidInput_ReturnsNull
            if (value.DecodeBase64_InvalidInput_ReturnsNull == null)
            {
                errors.Add("DecodeBase64_InvalidInput_ReturnsNull is null");
            }

            return errors;
        }

        public static bool IsValid(CryptoUtilityTests value)
        {
            return Validate(value).Count == 0;
        }

        public static void EnsureValid(CryptoUtilityTests value)
        {
            var errors = Validate(value);
            if (errors.Count > 0)
            {
                throw new ArgumentException(string.Join(" ", errors));
            }
        }
    }
}
